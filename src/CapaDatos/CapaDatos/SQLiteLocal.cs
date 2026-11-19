using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text;

namespace CapaDatos
{
    /// <summary>
    /// Small adapter over the bundled System.Data.SQLite provider. The local
    /// store does not depend on a separately installed SQL server.
    /// </summary>
    internal static class SQLiteLocal
    {
        private const int SqliteRow = 100;
        private const int SqliteDone = 101;
        private static readonly object Inicializador = new object();
        private static bool inicializado;
        private static string ruta;
        [ThreadStatic]
        private static Conexion conexionTransaccional;

        private const string Esquema = @"
PRAGMA foreign_keys=ON;
PRAGMA journal_mode=WAL;
CREATE TABLE IF NOT EXISTS UsuariosLocales(
  IdUsuario INTEGER PRIMARY KEY, Correo TEXT NOT NULL COLLATE NOCASE,
  Nombre TEXT NOT NULL, SyncUpdatedUtc TEXT NOT NULL, SyncDeleted INTEGER NOT NULL DEFAULT 0);
CREATE TABLE IF NOT EXISTS RegistrosPendientes(
  IdUsuarioLocal INTEGER PRIMARY KEY, Correo TEXT NOT NULL COLLATE NOCASE UNIQUE,
  Nombre TEXT NOT NULL, HashProtegido TEXT NOT NULL, IdUsuarioRemoto INTEGER NULL);
CREATE UNIQUE INDEX IF NOT EXISTS IX_RegistrosPendientes_IdUsuarioRemoto
  ON RegistrosPendientes(IdUsuarioRemoto) WHERE IdUsuarioRemoto IS NOT NULL;
CREATE TABLE IF NOT EXISTS EstadoSincronizacion(
  IdUsuario INTEGER PRIMARY KEY, UltimaCopiaCompletaUtc TEXT, UltimaSincronizacionUtc TEXT,
  FOREIGN KEY(IdUsuario) REFERENCES UsuariosLocales(IdUsuario) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS MetadatosLocal(Clave TEXT PRIMARY KEY, Valor TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS Configuraciones(
  IdConfiguracion INTEGER PRIMARY KEY, IdUsuario INTEGER NOT NULL UNIQUE, AutoSesion INTEGER NOT NULL DEFAULT 0,
  IdServidor INTEGER, SyncId TEXT NOT NULL, SyncUpdatedUtc TEXT NOT NULL, SyncDeleted INTEGER NOT NULL DEFAULT 0,
  FOREIGN KEY(IdUsuario) REFERENCES UsuariosLocales(IdUsuario) ON DELETE CASCADE, UNIQUE(IdUsuario,SyncId));
CREATE TABLE IF NOT EXISTS Tareas(
  IdTarea INTEGER PRIMARY KEY, IdServidor INTEGER, IdUsuario INTEGER NOT NULL, Nombre TEXT NOT NULL,
  FechaInicio TEXT NOT NULL, FechaFin TEXT NOT NULL, Estado TEXT NOT NULL DEFAULT 'P' CHECK(Estado IN('P','A','C','X')),
  EsPlantilla INTEGER NOT NULL DEFAULT 0, NoBloquearApps INTEGER NOT NULL DEFAULT 0,
  NoBloquearWebs INTEGER NOT NULL DEFAULT 0, SyncId TEXT NOT NULL, SyncUpdatedUtc TEXT NOT NULL,
  SyncDeleted INTEGER NOT NULL DEFAULT 0, FOREIGN KEY(IdUsuario) REFERENCES UsuariosLocales(IdUsuario) ON DELETE CASCADE,
  UNIQUE(IdUsuario,SyncId), UNIQUE(IdTarea,IdUsuario), CHECK(FechaInicio<FechaFin));
CREATE TABLE IF NOT EXISTS Descansos(
  IdDescanso INTEGER PRIMARY KEY, IdServidor INTEGER, IdTarea INTEGER NOT NULL, IdUsuario INTEGER NOT NULL,
  IntervaloDescanso INTEGER, DuracionDescanso INTEGER, SyncId TEXT NOT NULL,
  SyncUpdatedUtc TEXT NOT NULL, SyncDeleted INTEGER NOT NULL DEFAULT 0,
  FOREIGN KEY(IdTarea,IdUsuario) REFERENCES Tareas(IdTarea,IdUsuario) ON DELETE CASCADE,
  UNIQUE(IdUsuario,SyncId), UNIQUE(IdTarea,IdUsuario),
  CHECK((IntervaloDescanso IS NULL AND DuracionDescanso IS NULL) OR (IntervaloDescanso>0 AND DuracionDescanso>0)));
CREATE TABLE IF NOT EXISTS Aplicaciones(
  IdAplicacion INTEGER PRIMARY KEY, IdServidor INTEGER, IdUsuario INTEGER NOT NULL, Nombre TEXT NOT NULL,
  Identificador TEXT NOT NULL, EsCritica INTEGER NOT NULL DEFAULT 0, SyncId TEXT NOT NULL,
  SyncUpdatedUtc TEXT NOT NULL, SyncDeleted INTEGER NOT NULL DEFAULT 0,
  FOREIGN KEY(IdUsuario) REFERENCES UsuariosLocales(IdUsuario) ON DELETE CASCADE,
  UNIQUE(IdUsuario,SyncId), UNIQUE(IdAplicacion,IdUsuario), UNIQUE(IdUsuario,Identificador));
CREATE TABLE IF NOT EXISTS Webs(
  IdWeb INTEGER PRIMARY KEY, IdServidor INTEGER, IdUsuario INTEGER NOT NULL, Nombre TEXT NOT NULL, Link TEXT NOT NULL,
  SyncId TEXT NOT NULL, SyncUpdatedUtc TEXT NOT NULL, SyncDeleted INTEGER NOT NULL DEFAULT 0,
  FOREIGN KEY(IdUsuario) REFERENCES UsuariosLocales(IdUsuario) ON DELETE CASCADE,
  UNIQUE(IdUsuario,SyncId), UNIQUE(IdWeb,IdUsuario), UNIQUE(IdUsuario,Link));
CREATE TABLE IF NOT EXISTS Tarea_Aplicacion(
  IdTarea INTEGER NOT NULL, IdUsuario INTEGER NOT NULL, IdAplicacion INTEGER NOT NULL,
  SyncUpdatedUtc TEXT NOT NULL, SyncDeleted INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY(IdTarea,IdAplicacion,IdUsuario),
  FOREIGN KEY(IdTarea,IdUsuario) REFERENCES Tareas(IdTarea,IdUsuario) ON DELETE CASCADE,
  FOREIGN KEY(IdAplicacion,IdUsuario) REFERENCES Aplicaciones(IdAplicacion,IdUsuario) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS Tarea_Web(
  IdTarea INTEGER NOT NULL, IdUsuario INTEGER NOT NULL, IdWeb INTEGER NOT NULL,
  SyncUpdatedUtc TEXT NOT NULL, SyncDeleted INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY(IdTarea,IdWeb,IdUsuario),
  FOREIGN KEY(IdTarea,IdUsuario) REFERENCES Tareas(IdTarea,IdUsuario) ON DELETE CASCADE,
  FOREIGN KEY(IdWeb,IdUsuario) REFERENCES Webs(IdWeb,IdUsuario) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS SyncOutbox(
  IdCambio INTEGER PRIMARY KEY AUTOINCREMENT, IdUsuario INTEGER NOT NULL,
  TipoEntidad TEXT NOT NULL, SyncId TEXT NOT NULL, Operacion TEXT NOT NULL CHECK(Operacion IN('I','U','D')),
  Payload TEXT NOT NULL, CreadoUtc TEXT NOT NULL, Intentos INTEGER NOT NULL DEFAULT 0, UltimoError TEXT,
  FOREIGN KEY(IdUsuario) REFERENCES UsuariosLocales(IdUsuario) ON DELETE CASCADE);
CREATE INDEX IF NOT EXISTS IX_SyncOutbox_Pendiente ON SyncOutbox(IdUsuario,IdCambio);
CREATE INDEX IF NOT EXISTS IX_Tareas_Usuario_Fecha ON Tareas(IdUsuario,FechaInicio,FechaFin);
CREATE INDEX IF NOT EXISTS IX_Aplicaciones_Usuario_Nombre ON Aplicaciones(IdUsuario,Nombre);
CREATE INDEX IF NOT EXISTS IX_Webs_Usuario_Nombre ON Webs(IdUsuario,Nombre);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Tareas_IdServidor ON Tareas(IdUsuario,IdServidor) WHERE IdServidor IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS IX_Aplicaciones_IdServidor ON Aplicaciones(IdUsuario,IdServidor) WHERE IdServidor IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS IX_Webs_IdServidor ON Webs(IdUsuario,IdServidor) WHERE IdServidor IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS IX_Descansos_IdServidor ON Descansos(IdUsuario,IdServidor) WHERE IdServidor IS NOT NULL;
";

        public static string RutaBase
        {
            get { AsegurarInicializada(); return ruta; }
        }

        public static void AsegurarInicializada()
        {
            lock (Inicializador)
            {
                if (inicializado) return;
                string rutaPruebas = Environment.GetEnvironmentVariable("PC_SQLITE_LOCAL_PATH");
                if (!String.IsNullOrWhiteSpace(rutaPruebas))
                {
                    ruta = Path.GetFullPath(rutaPruebas);
                    Directory.CreateDirectory(Path.GetDirectoryName(ruta));
                }
                else
                {
                    string carpeta = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Programa-de-Concentracion", "DatosOffline");
                    Directory.CreateDirectory(carpeta);
                    ruta = Path.Combine(carpeta, "ProgramaConcentracion.sqlite");
                }
                using (var conexion = new Conexion(ruta))
                {
                    conexion.EjecutarScript(Esquema);
                }
                inicializado = true;
                try { MigrarEstadoTareaDetenida(); }
                catch
                {
                    inicializado = false;
                    throw;
                }
            }
        }

        private static void MigrarEstadoTareaDetenida()
        {
            string esquema = Convert.ToString(Escalar(
                "SELECT sql FROM sqlite_master WHERE type='table' AND name='Tareas'"));
            if (String.IsNullOrEmpty(esquema) || esquema.IndexOf("'X'", StringComparison.OrdinalIgnoreCase) >= 0)
                return;

            EjecutarScript(@"
PRAGMA foreign_keys=OFF;
BEGIN IMMEDIATE;
CREATE TABLE Tareas_Actualizada(
  IdTarea INTEGER PRIMARY KEY, IdServidor INTEGER, IdUsuario INTEGER NOT NULL, Nombre TEXT NOT NULL,
  FechaInicio TEXT NOT NULL, FechaFin TEXT NOT NULL, Estado TEXT NOT NULL DEFAULT 'P' CHECK(Estado IN('P','A','C','X')),
  EsPlantilla INTEGER NOT NULL DEFAULT 0, NoBloquearApps INTEGER NOT NULL DEFAULT 0,
  NoBloquearWebs INTEGER NOT NULL DEFAULT 0, SyncId TEXT NOT NULL, SyncUpdatedUtc TEXT NOT NULL,
  SyncDeleted INTEGER NOT NULL DEFAULT 0, FOREIGN KEY(IdUsuario) REFERENCES UsuariosLocales(IdUsuario) ON DELETE CASCADE,
  UNIQUE(IdUsuario,SyncId), UNIQUE(IdTarea,IdUsuario), CHECK(FechaInicio<FechaFin));
INSERT INTO Tareas_Actualizada(IdTarea,IdServidor,IdUsuario,Nombre,FechaInicio,FechaFin,Estado,EsPlantilla,
  NoBloquearApps,NoBloquearWebs,SyncId,SyncUpdatedUtc,SyncDeleted)
SELECT IdTarea,IdServidor,IdUsuario,Nombre,FechaInicio,FechaFin,Estado,EsPlantilla,
  NoBloquearApps,NoBloquearWebs,SyncId,SyncUpdatedUtc,SyncDeleted FROM Tareas;
DROP TABLE Tareas;
ALTER TABLE Tareas_Actualizada RENAME TO Tareas;
CREATE INDEX IF NOT EXISTS IX_Tareas_Usuario_Fecha ON Tareas(IdUsuario,FechaInicio,FechaFin);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Tareas_IdServidor ON Tareas(IdUsuario,IdServidor) WHERE IdServidor IS NOT NULL;
COMMIT;
PRAGMA foreign_keys=ON;");
        }

        private static void EjecutarScript(string sql)
        {
            using (var conexion = new Conexion(ruta))
            {
                try { conexion.EjecutarScript(sql); }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("No se pudo migrar el esquema offline. " + ex.Message, ex);
                }
            }
        }

        public static int Ejecutar(string sql, params object[] parametros)
        {
            AsegurarInicializada();
            if (conexionTransaccional != null)
                return EjecutarInterno(conexionTransaccional, sql, parametros);
            return EjecutarInterno(sql, parametros);
        }

        public static void EnTransaccion(Action accion)
        {
            if (accion == null) throw new ArgumentNullException("accion");
            EnTransaccion(() => { accion(); return true; });
        }

        public static T EnTransaccion<T>(Func<T> accion)
        {
            if (accion == null) throw new ArgumentNullException("accion");
            AsegurarInicializada();
            if (conexionTransaccional != null) return accion();

            using (var conexion = new Conexion(ruta))
            {
                EjecutarInterno(conexion, "BEGIN IMMEDIATE;", null);
                conexionTransaccional = conexion;
                try
                {
                    T resultado = accion();
                    EjecutarInterno(conexion, "COMMIT;", null);
                    return resultado;
                }
                catch (Exception errorOriginal)
                {
                    try { EjecutarInterno(conexion, "ROLLBACK;", null); }
                    catch (Exception errorReversion)
                    {
                        throw new AggregateException(
                            "Falló la operación local y SQLite tampoco pudo confirmar su reversión.",
                            errorOriginal, errorReversion);
                    }
                    throw;
                }
                finally { conexionTransaccional = null; }
            }
        }

        public static object Escalar(string sql, params object[] parametros)
        {
            AsegurarInicializada();
            if (conexionTransaccional != null)
            {
                using (var sentencia = conexionTransaccional.Preparar(sql, parametros))
                {
                    int resultado = sentencia.Avanzar();
                    if (resultado == SqliteDone) return null;
                    if (resultado != SqliteRow) throw conexionTransaccional.Error("No se pudo leer el resultado de SQLite.");
                    return sentencia.Leer(0);
                }
            }
            using (var conexion = new Conexion(ruta))
            using (var sentencia = conexion.Preparar(sql, parametros))
            {
                int resultado = sentencia.Avanzar();
                if (resultado == SqliteDone) return null;
                if (resultado != SqliteRow) throw conexion.Error("No se pudo leer el resultado de SQLite.");
                return sentencia.Leer(0);
            }
        }

        public static List<T> Consultar<T>(string sql, Func<Fila, T> mapear, params object[] parametros)
        {
            if (mapear == null) throw new ArgumentNullException("mapear");
            AsegurarInicializada();
            var resultados = new List<T>();
            if (conexionTransaccional != null)
            {
                using (var sentencia = conexionTransaccional.Preparar(sql, parametros))
                {
                    int resultado;
                    while ((resultado = sentencia.Avanzar()) == SqliteRow)
                        resultados.Add(mapear(new Fila(sentencia)));
                    if (resultado != SqliteDone) throw conexionTransaccional.Error("No se pudo leer SQLite.");
                }
                return resultados;
            }
            using (var conexion = new Conexion(ruta))
            using (var sentencia = conexion.Preparar(sql, parametros))
            {
                int resultado;
                while ((resultado = sentencia.Avanzar()) == SqliteRow)
                    resultados.Add(mapear(new Fila(sentencia)));
                if (resultado != SqliteDone) throw conexion.Error("No se pudo leer SQLite.");
            }
            return resultados;
        }

        private static int EjecutarInterno(string sql, object[] parametros)
        {
            using (var conexion = new Conexion(ruta))
                return EjecutarInterno(conexion, sql, parametros);
        }

        private static int EjecutarInterno(Conexion conexion, string sql, object[] parametros)
        {
            using (var sentencia = conexion.Preparar(sql, parametros))
            {
                int resultado;
                do { resultado = sentencia.Avanzar(); }
                while (resultado == SqliteRow);
                if (resultado != SqliteDone) throw conexion.Error("No se pudo guardar en SQLite.");
                return conexion.Cambios;
            }
        }

        internal sealed class Fila
        {
            private readonly Sentencia sentencia;
            internal Fila(Sentencia sentencia) { this.sentencia = sentencia; }
            public object this[int indice] { get { return sentencia.Leer(indice); } }
            public object this[string nombre] { get { return sentencia.Leer(nombre); } }
            public int Entero(string nombre) { return Convert.ToInt32(this[nombre], CultureInfo.InvariantCulture); }
            public bool Booleano(string nombre) { return Convert.ToInt64(this[nombre], CultureInfo.InvariantCulture) != 0; }
            public string Texto(string nombre) { object v = this[nombre]; return v == null ? null : Convert.ToString(v, CultureInfo.InvariantCulture); }
            public DateTime Fecha(string nombre) { return DateTime.Parse(Texto(nombre), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind); }
        }

        internal sealed class Conexion : IDisposable
        {
            private readonly SQLiteConnection db;
            public int Cambios { get { return db.Changes; } }

            public Conexion(string archivo)
            {
                var builder = new SQLiteConnectionStringBuilder
                {
                    DataSource = archivo,
                    Version = 3,
                    Pooling = false
                };
                db = new SQLiteConnection(builder.ConnectionString);
                db.DefaultTimeout = 5;
                db.Open();
                using (var stmt = Preparar("PRAGMA foreign_keys=ON;", null)) stmt.Avanzar();
            }

            public Sentencia Preparar(string sql, object[] parametros)
            {
                var comando = db.CreateCommand();
                comando.CommandText = NumerarParametros(sql, parametros == null ? 0 : parametros.Length);
                if (parametros != null)
                {
                    for (int i = 0; i < parametros.Length; i++)
                    {
                        var parametro = comando.CreateParameter();
                        parametro.ParameterName = "?" + (i + 1).ToString(CultureInfo.InvariantCulture);
                        parametro.Value = NormalizarParametro(parametros[i]);
                        comando.Parameters.Add(parametro);
                    }
                }
                return new Sentencia(this, comando);
            }

            public Exception Error(string mensaje)
            {
                return new InvalidOperationException(mensaje);
            }

            public void EjecutarScript(string sql)
            {
                if (String.IsNullOrWhiteSpace(sql)) return;
                string[] sentencias = sql.Split(';');
                foreach (string texto in sentencias)
                {
                    if (String.IsNullOrWhiteSpace(texto)) continue;
                    using (SQLiteCommand comando = db.CreateCommand())
                    {
                        comando.CommandText = texto;
                        using (SQLiteDataReader lector = comando.ExecuteReader())
                            while (lector.Read()) { }
                    }
                }
            }

            public void Dispose()
            {
                db.Dispose();
            }

            private static object NormalizarParametro(object valor)
            {
                if (valor == null || valor == DBNull.Value) return DBNull.Value;
                if (valor is bool) return (bool)valor ? 1 : 0;
                if (valor is DateTime)
                {
                    DateTime fecha = (DateTime)valor;
                    if (fecha.Kind == DateTimeKind.Local) fecha = fecha.ToUniversalTime();
                    return fecha.ToString("o", CultureInfo.InvariantCulture);
                }
                if (valor is Guid) return ((Guid)valor).ToString("D");
                if (valor is string || valor is byte[] || valor is double || valor is float ||
                    valor is decimal || valor is byte || valor is short || valor is int ||
                    valor is long || valor is uint || valor is ushort || valor is ulong)
                    return valor;
                return Convert.ToString(valor, CultureInfo.InvariantCulture);
            }

            private static string NumerarParametros(string sql, int cantidad)
            {
                if (cantidad == 0 || String.IsNullOrEmpty(sql)) return sql;
                var resultado = new StringBuilder(sql.Length + cantidad * 2);
                char comilla = '\0';
                int parametro = 0;
                for (int i = 0; i < sql.Length; i++)
                {
                    char actual = sql[i];
                    if (comilla != '\0')
                    {
                        resultado.Append(actual);
                        if (actual == comilla)
                        {
                            if (i + 1 < sql.Length && sql[i + 1] == comilla)
                                resultado.Append(sql[++i]);
                            else
                                comilla = '\0';
                        }
                        continue;
                    }

                    if (actual == '\'' || actual == '"' || actual == '`')
                    {
                        comilla = actual;
                        resultado.Append(actual);
                    }
                    else if (actual == '[')
                    {
                        comilla = ']';
                        resultado.Append(actual);
                    }
                    else if (actual == '?' && parametro < cantidad)
                    {
                        parametro++;
                        resultado.Append('?').Append(parametro.ToString(CultureInfo.InvariantCulture));
                    }
                    else resultado.Append(actual);
                }
                if (parametro != cantidad)
                    throw new ArgumentException("La cantidad de parámetros no coincide con la consulta SQLite.");
                return resultado.ToString();
            }
        }

        internal sealed class Sentencia : IDisposable
        {
            private readonly Conexion conexion;
            private readonly SQLiteCommand comando;
            private SQLiteDataReader lector;

            internal Sentencia(Conexion conexion, SQLiteCommand comando)
            {
                this.conexion = conexion;
                this.comando = comando;
            }

            public int Avanzar()
            {
                if (lector == null) lector = comando.ExecuteReader();
                return lector.Read() ? SqliteRow : SqliteDone;
            }

            public object Leer(int indice)
            {
                if (lector == null || lector.IsDBNull(indice)) return null;
                return lector.GetValue(indice);
            }

            public object Leer(string nombre)
            {
                try { return Leer(lector.GetOrdinal(nombre)); }
                catch (IndexOutOfRangeException)
                {
                    throw conexion.Error("No existe la columna SQLite '" + nombre + "'.");
                }
            }

            public void Dispose()
            {
                if (lector != null) lector.Dispose();
                comando.Dispose();
            }
        }
    }
}
