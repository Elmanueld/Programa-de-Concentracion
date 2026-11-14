using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CapaEntidades;

namespace CapaNegocio
{
    /// <summary>
    /// Recuerda localmente la última cuenta que inició sesión correctamente.
    /// El contenido completo se protege con DPAPI y solo se puede descifrar
    /// bajo la misma cuenta de Windows.
    /// </summary>
    internal static class CredencialOfflineLocal
    {
        private const int VersionFormato = 1;
        private static readonly object Sincronizador = new object();

        private sealed class Registro
        {
            public int IdUsuario;
            public string Nombre;
            public string Correo;
            public string HashContrasena;
        }

        private static string RutaArchivo
        {
            get
            {
                string carpeta = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Programa-de-Concentracion", "SesionLocal");
                Directory.CreateDirectory(carpeta);
                return Path.Combine(carpeta, "credencial.bin");
            }
        }

        private static string RutaAutoInicioDeshabilitado(int idUsuario)
        {
            string carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programa-de-Concentracion", "SesionLocal");
            return Path.Combine(carpeta, "sin-auto-inicio-" + idUsuario + ".flag");
        }

        public static void Guardar(E_Usuario usuario, string hashContrasena)
        {
            if (usuario == null) throw new ArgumentNullException("usuario");
            if (usuario.IdUsuario <= 0 || String.IsNullOrWhiteSpace(usuario.Correo) ||
                String.IsNullOrWhiteSpace(hashContrasena))
                throw new ArgumentException("No se puede guardar una credencial local incompleta.");

            lock (Sincronizador)
            {
                string marcaDeshabilitada = RutaAutoInicioDeshabilitado(usuario.IdUsuario);
                if (File.Exists(marcaDeshabilitada)) File.Delete(marcaDeshabilitada);
                Registro registro = new Registro
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre ?? String.Empty,
                    Correo = usuario.Correo.Trim(),
                    HashContrasena = hashContrasena
                };

                byte[] claro = Serializar(registro);
                byte[] protegido = ProtectedData.Protect(claro, null, DataProtectionScope.CurrentUser);
                string ruta = RutaArchivo;
                string temporal = ruta + ".tmp";
                try
                {
                    File.WriteAllBytes(temporal, protegido);
                    if (File.Exists(ruta)) File.Replace(temporal, ruta, null);
                    else File.Move(temporal, ruta);
                }
                finally
                {
                    if (File.Exists(temporal)) File.Delete(temporal);
                    Array.Clear(claro, 0, claro.Length);
                    Array.Clear(protegido, 0, protegido.Length);
                }
            }
        }

        public static E_Usuario IniciarSesion(string correo, string contrasena)
        {
            if (String.IsNullOrWhiteSpace(correo) || contrasena == null) return null;
            lock (Sincronizador)
            {
                Registro registro = Cargar();
                if (registro == null || !String.Equals(registro.Correo, correo.Trim(),
                    StringComparison.OrdinalIgnoreCase) ||
                    !SeguridadContrasena.Verificar(contrasena, registro.HashContrasena))
                    return null;

                return new E_Usuario
                {
                    IdUsuario = registro.IdUsuario,
                    Nombre = registro.Nombre,
                    Correo = registro.Correo,
                    SesionSinConexion = true
                };
            }
        }

        public static E_Usuario ObtenerSesionRecordada(out string hashContrasena)
        {
            hashContrasena = null;
            lock (Sincronizador)
            {
                Registro registro = Cargar();
                if (registro == null) return null;
                hashContrasena = registro.HashContrasena;
                return new E_Usuario
                {
                    IdUsuario = registro.IdUsuario,
                    Nombre = registro.Nombre,
                    Correo = registro.Correo
                };
            }
        }

        public static bool InicioAutomaticoDeshabilitado(int idUsuario)
        {
            if (idUsuario <= 0) return true;
            lock (Sincronizador)
            {
                try { return File.Exists(RutaAutoInicioDeshabilitado(idUsuario)); }
                catch (IOException) { return true; }
                catch (UnauthorizedAccessException) { return true; }
            }
        }

        public static void DeshabilitarInicioAutomatico(int idUsuario)
        {
            if (idUsuario <= 0) throw new ArgumentException("El ID del usuario no es válido.");
            lock (Sincronizador)
            {
                string ruta = RutaAutoInicioDeshabilitado(idUsuario);
                Directory.CreateDirectory(Path.GetDirectoryName(ruta));
                File.WriteAllText(ruta, DateTime.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        public static void ActualizarCuenta(E_Usuario usuario)
        {
            if (usuario == null || usuario.IdUsuario <= 0) return;
            lock (Sincronizador)
            {
                Registro registro = Cargar();
                if (registro == null || registro.IdUsuario != usuario.IdUsuario) return;
                registro.Nombre = usuario.Nombre ?? String.Empty;
                registro.Correo = (usuario.Correo ?? String.Empty).Trim();
                GuardarRegistro(registro);
            }
        }

        public static void ActualizarContrasena(int idUsuario, string hashContrasena)
        {
            if (idUsuario <= 0 || String.IsNullOrWhiteSpace(hashContrasena)) return;
            lock (Sincronizador)
            {
                Registro registro = Cargar();
                if (registro == null || registro.IdUsuario != idUsuario) return;
                registro.HashContrasena = hashContrasena;
                GuardarRegistro(registro);
            }
        }

        public static void Eliminar(int idUsuario)
        {
            if (idUsuario <= 0) return;
            lock (Sincronizador)
            {
                Registro registro = Cargar();
                if (registro != null && registro.IdUsuario == idUsuario && File.Exists(RutaArchivo))
                    File.Delete(RutaArchivo);
            }
        }

        private static void GuardarRegistro(Registro registro)
        {
            byte[] claro = Serializar(registro);
            byte[] protegido = ProtectedData.Protect(claro, null, DataProtectionScope.CurrentUser);
            string ruta = RutaArchivo;
            string temporal = ruta + ".tmp";
            try
            {
                File.WriteAllBytes(temporal, protegido);
                if (File.Exists(ruta)) File.Replace(temporal, ruta, null);
                else File.Move(temporal, ruta);
            }
            finally
            {
                if (File.Exists(temporal)) File.Delete(temporal);
                Array.Clear(claro, 0, claro.Length);
                Array.Clear(protegido, 0, protegido.Length);
            }
        }

        private static Registro Cargar()
        {
            string ruta = RutaArchivo;
            if (!File.Exists(ruta)) return null;
            byte[] protegido = File.ReadAllBytes(ruta);
            byte[] claro = null;
            try
            {
                claro = ProtectedData.Unprotect(protegido, null, DataProtectionScope.CurrentUser);
                return Deserializar(claro);
            }
            catch (CryptographicException)
            {
                // Un archivo creado por otra cuenta de Windows no se acepta.
                return null;
            }
            catch (InvalidDataException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            finally
            {
                Array.Clear(protegido, 0, protegido.Length);
                if (claro != null) Array.Clear(claro, 0, claro.Length);
            }
        }

        private static byte[] Serializar(Registro registro)
        {
            using (var memoria = new MemoryStream())
            using (var escritor = new BinaryWriter(memoria, Encoding.UTF8, true))
            {
                escritor.Write(VersionFormato);
                escritor.Write(registro.IdUsuario);
                escritor.Write(registro.Nombre ?? String.Empty);
                escritor.Write(registro.Correo ?? String.Empty);
                escritor.Write(registro.HashContrasena ?? String.Empty);
                escritor.Flush();
                return memoria.ToArray();
            }
        }

        private static Registro Deserializar(byte[] datos)
        {
            using (var memoria = new MemoryStream(datos, false))
            using (var lector = new BinaryReader(memoria, Encoding.UTF8, true))
            {
                if (lector.ReadInt32() != VersionFormato) throw new InvalidDataException();
                var registro = new Registro
                {
                    IdUsuario = lector.ReadInt32(),
                    Nombre = lector.ReadString(),
                    Correo = lector.ReadString(),
                    HashContrasena = lector.ReadString()
                };
                if (memoria.Position != memoria.Length || registro.IdUsuario <= 0 ||
                    String.IsNullOrWhiteSpace(registro.Correo) ||
                    String.IsNullOrWhiteSpace(registro.HashContrasena))
                    throw new InvalidDataException();
                return registro;
            }
        }
    }
}
