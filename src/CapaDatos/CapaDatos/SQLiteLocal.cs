using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CapaDatos
{
    /// <summary>
    /// Small SQLite adapter over the Windows SQLite API (winsqlite3.dll), so
    /// the offline store does not depend on a separately installed SQL server.
    /// </summary>
    internal static class SQLiteLocal
    {
        private const int SqliteOk = 0;
        private const int SqliteRow = 100;
        private const int SqliteDone = 101;
        private const int TipoNulo = 5;
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
                    IntPtr error;
                    int resultado = sqlite3_exec(conexion.Handle, Esquema, IntPtr.Zero, IntPtr.Zero, out error);
                    if (resultado != SqliteOk)
                    {
                        string detalle = error == IntPtr.Zero ? "" : Marshal.PtrToStringAnsi(error);
                        if (error != IntPtr.Zero) sqlite3_free(error);
                        throw new InvalidOperationException("No se pudo crear el esquema offline. " + detalle);
                    }
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
                IntPtr error;
                int resultado = sqlite3_exec(conexion.Handle, sql, IntPtr.Zero, IntPtr.Zero, out error);
                if (resultado == SqliteOk) return;
                string detalle = error == IntPtr.Zero ? String.Empty : Marshal.PtrToStringAnsi(error);
                if (error != IntPtr.Zero) sqlite3_free(error);
                throw new InvalidOperationException("No se pudo migrar el esquema offline. " + detalle);
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
                catch
                {
                    try { EjecutarInterno(conexion, "ROLLBACK;", null); }
                    catch { }
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
            private IntPtr db;
            public int Cambios { get { return sqlite3_changes(db); } }
            public IntPtr Handle { get { return db; } }

            public Conexion(string archivo)
            {
                int rc = sqlite3_open16(archivo, out db);
                if (rc != SqliteOk) throw Error("No se pudo abrir la base offline.");
                sqlite3_busy_timeout(db, 5000);
                using (var stmt = Preparar("PRAGMA foreign_keys=ON;", null)) stmt.Avanzar();
            }

            public Sentencia Preparar(string sql, object[] parametros)
            {
                IntPtr sentencia;
                int rc = sqlite3_prepare16_v2(db, sql, -1, out sentencia, IntPtr.Zero);
                if (rc != SqliteOk) throw Error("La consulta SQLite no es válida.");
                var resultado = new Sentencia(this, sentencia);
                try
                {
                    if (parametros != null)
                        for (int i = 0; i < parametros.Length; i++) resultado.Vincular(i + 1, parametros[i]);
                    return resultado;
                }
                catch { resultado.Dispose(); throw; }
            }

            public Exception Error(string mensaje)
            {
                string detalle = db == IntPtr.Zero ? "" : Marshal.PtrToStringAnsi(sqlite3_errmsg(db));
                return new InvalidOperationException(mensaje + (String.IsNullOrEmpty(detalle) ? "" : " " + detalle));
            }

            public void Dispose()
            {
                if (db != IntPtr.Zero) { sqlite3_close_v2(db); db = IntPtr.Zero; }
            }
        }

        internal sealed class Sentencia : IDisposable
        {
            private readonly Conexion conexion;
            private IntPtr statement;

            internal Sentencia(Conexion conexion, IntPtr statement)
            {
                this.conexion = conexion;
                this.statement = statement;
            }

            public int Avanzar() { return sqlite3_step(statement); }

            public object Leer(int indice)
            {
                if (sqlite3_column_type(statement, indice) == TipoNulo) return null;
                int tipo = sqlite3_column_type(statement, indice);
                if (tipo == 1) return sqlite3_column_int64(statement, indice);
                if (tipo == 2) return sqlite3_column_double(statement, indice);
                IntPtr valor = sqlite3_column_text(statement, indice);
                int longitud = sqlite3_column_bytes(statement, indice);
                if (valor == IntPtr.Zero || longitud <= 0) return String.Empty;
                byte[] bytes = new byte[longitud];
                Marshal.Copy(valor, bytes, 0, longitud);
                return Encoding.UTF8.GetString(bytes);
            }

            public object Leer(string nombre)
            {
                for (int i = 0; i < sqlite3_column_count(statement); i++)
                    if (String.Equals(Marshal.PtrToStringAnsi(sqlite3_column_name(statement, i)), nombre, StringComparison.OrdinalIgnoreCase))
                        return Leer(i);
                throw conexion.Error("No existe la columna SQLite '" + nombre + "'.");
            }

            internal void Vincular(int indice, object valor)
            {
                int rc;
                if (valor == null || valor == DBNull.Value) rc = sqlite3_bind_null(statement, indice);
                else if (valor is bool) rc = sqlite3_bind_int64(statement, indice, (bool)valor ? 1 : 0);
                else if (valor is byte || valor is short || valor is int || valor is long)
                    rc = sqlite3_bind_int64(statement, indice, Convert.ToInt64(valor, CultureInfo.InvariantCulture));
                else if (valor is float || valor is double || valor is decimal)
                    rc = sqlite3_bind_double(statement, indice, Convert.ToDouble(valor, CultureInfo.InvariantCulture));
                else if (valor is DateTime)
                {
                    DateTime fecha = (DateTime)valor;
                    if (fecha.Kind == DateTimeKind.Local) fecha = fecha.ToUniversalTime();
                    rc = sqlite3_bind_text16(statement, indice, fecha.ToString("o", CultureInfo.InvariantCulture), -1, new IntPtr(-1));
                }
                else if (valor is Guid)
                    rc = sqlite3_bind_text16(statement, indice, ((Guid)valor).ToString("D"), -1, new IntPtr(-1));
                else
                    rc = sqlite3_bind_text16(statement, indice, Convert.ToString(valor, CultureInfo.InvariantCulture), -1, new IntPtr(-1));
                if (rc != SqliteOk) throw conexion.Error("No se pudo asignar un parámetro SQLite.");
            }

            public void Dispose()
            {
                if (statement != IntPtr.Zero) { sqlite3_finalize(statement); statement = IntPtr.Zero; }
            }
        }

        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_open16")]
        private static extern int sqlite3_open16([MarshalAs(UnmanagedType.LPWStr)] string filename, out IntPtr db);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_exec")]
        private static extern int sqlite3_exec(IntPtr db, [MarshalAs(UnmanagedType.LPStr)] string sql, IntPtr callback, IntPtr argument, out IntPtr errorMessage);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_free")]
        private static extern void sqlite3_free(IntPtr buffer);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_close_v2")]
        private static extern int sqlite3_close_v2(IntPtr db);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_prepare16_v2")]
        private static extern int sqlite3_prepare16_v2(IntPtr db, [MarshalAs(UnmanagedType.LPWStr)] string sql, int bytes, out IntPtr statement, IntPtr tail);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_step")]
        private static extern int sqlite3_step(IntPtr statement);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_finalize")]
        private static extern int sqlite3_finalize(IntPtr statement);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_null")]
        private static extern int sqlite3_bind_null(IntPtr statement, int index);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_int64")]
        private static extern int sqlite3_bind_int64(IntPtr statement, int index, long value);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_double")]
        private static extern int sqlite3_bind_double(IntPtr statement, int index, double value);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_text16")]
        private static extern int sqlite3_bind_text16(IntPtr statement, int index, [MarshalAs(UnmanagedType.LPWStr)] string value, int bytes, IntPtr destructor);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_type")]
        private static extern int sqlite3_column_type(IntPtr statement, int index);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_int64")]
        private static extern long sqlite3_column_int64(IntPtr statement, int index);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_double")]
        private static extern double sqlite3_column_double(IntPtr statement, int index);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_text16")]
        private static extern IntPtr sqlite3_column_text16(IntPtr statement, int index);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_text")]
        private static extern IntPtr sqlite3_column_text(IntPtr statement, int index);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_bytes")]
        private static extern int sqlite3_column_bytes(IntPtr statement, int index);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_name")]
        private static extern IntPtr sqlite3_column_name(IntPtr statement, int index);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_count")]
        private static extern int sqlite3_column_count(IntPtr statement);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_changes")]
        private static extern int sqlite3_changes(IntPtr db);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_busy_timeout")]
        private static extern int sqlite3_busy_timeout(IntPtr db, int milliseconds);
        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_errmsg")]
        private static extern IntPtr sqlite3_errmsg(IntPtr db);
    }
}
