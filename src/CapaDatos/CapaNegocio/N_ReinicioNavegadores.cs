using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CapaNegocio
{
    /// <summary>Reinicia los navegadores visibles una vez para que vuelvan a leer las autoridades locales de Windows.</summary>
    internal static class N_ReinicioNavegadores
    {
        private static readonly HashSet<string> Navegadores = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "chrome.exe", "msedge.exe", "firefox.exe", "brave.exe", "opera.exe", "opera_gx.exe",
            "vivaldi.exe", "waterfox.exe", "librewolf.exe", "floorp.exe", "arc.exe"
        };

        private sealed class Proceso
        {
            public int Id;
            public int PadreId;
            public string Nombre;
            public string Ruta;
            public string LineaComandos;
            public string[] Argumentos;
        }

        private sealed class Sesion
        {
            public string Ruta;
            public string[] Argumentos;
            public string ClavePerfil;
            public int IdRaiz;
        }

        public static bool ReiniciarAbiertos()
        {
            try
            {
                List<Proceso> procesos = ObtenerProcesos();
                int sesionActual = Process.GetCurrentProcess().SessionId;
                List<Sesion> sesiones = procesos
                    .Where(p => Navegadores.Contains(p.Nombre) && EsProcesoPrincipal(p) && TieneVentanaVisible(p.Id, sesionActual))
                    .Select(p => new Sesion
                    {
                        Ruta = p.Ruta,
                        Argumentos = p.Argumentos,
                        ClavePerfil = ObtenerClavePerfil(p.Ruta, p.Argumentos),
                        IdRaiz = p.Id
                    })
                    .GroupBy(s => s.ClavePerfil, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();

                if (sesiones.Count == 0) return true;

                var porPadre = procesos.GroupBy(p => p.PadreId).ToDictionary(g => g.Key, g => g.Select(p => p.Id).ToList());
                var idsACerrar = new HashSet<int>();
                foreach (Sesion sesion in sesiones)
                    AgregarDescendientes(sesion.IdRaiz, porPadre, idsACerrar);

                foreach (int id in idsACerrar)
                {
                    try
                    {
                        using (Process proceso = Process.GetProcessById(id))
                            proceso.Kill(); // Evita diálogos de páginas que intentan impedir el cierre.
                    }
                    catch (ArgumentException) { }
                    catch (InvalidOperationException) { }
                    catch (Exception ex) { Trace.TraceWarning("No se pudo cerrar un proceso del navegador: " + ex.Message); }
                }

                EsperarCierre(idsACerrar, 8000);

                bool correcto = true;
                foreach (Sesion sesion in sesiones)
                {
                    try
                    {
                        string argumentos = PrepararArgumentos(sesion.Ruta, sesion.Argumentos);
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = sesion.Ruta,
                            Arguments = argumentos,
                            UseShellExecute = true,
                            WindowStyle = ProcessWindowStyle.Normal
                        });
                    }
                    catch (Exception ex)
                    {
                        correcto = false;
                        Trace.TraceError("No se pudo reabrir el navegador " + sesion.Ruta + ": " + ex);
                    }
                }
                return correcto;
            }
            catch (Exception ex)
            {
                Trace.TraceError("No se pudieron reiniciar los navegadores abiertos: " + ex);
                return false;
            }
        }

        private static List<Proceso> ObtenerProcesos()
        {
            string filtro = String.Join(" OR ", Navegadores.Select(nombre => "Name='" + nombre + "'"));
            var resultado = new List<Proceso>();
            using (var buscador = new ManagementObjectSearcher(
                "SELECT ProcessId, ParentProcessId, Name, ExecutablePath, CommandLine FROM Win32_Process WHERE " + filtro))
            using (ManagementObjectCollection encontrados = buscador.Get())
            {
                foreach (ManagementObject item in encontrados)
                using (item)
                {
                    try
                    {
                        string nombre = Convert.ToString(item["Name"]);
                        string ruta = Convert.ToString(item["ExecutablePath"]);
                        string linea = Convert.ToString(item["CommandLine"]);
                        int id = Convert.ToInt32(item["ProcessId"]);
                        if (String.IsNullOrWhiteSpace(ruta) || String.IsNullOrWhiteSpace(linea)) continue;
                        resultado.Add(new Proceso
                        {
                            Id = id,
                            PadreId = Convert.ToInt32(item["ParentProcessId"]),
                            Nombre = nombre,
                            Ruta = ruta,
                            LineaComandos = linea,
                            Argumentos = SepararArgumentos(linea)
                        });
                    }
                    catch (Exception ex) { Trace.TraceWarning("No se pudo leer un proceso de navegador: " + ex.Message); }
                }
            }
            return resultado;
        }

        private static bool EsProcesoPrincipal(Proceso proceso)
        {
            string tipo = proceso.Argumentos.FirstOrDefault(a => a.StartsWith("--type=", StringComparison.OrdinalIgnoreCase));
            if (tipo != null && !tipo.Equals("--type=browser", StringComparison.OrdinalIgnoreCase)) return false;
            if (proceso.Nombre.Equals("firefox.exe", StringComparison.OrdinalIgnoreCase) &&
                proceso.Argumentos.Any(a => a.Equals("-contentproc", StringComparison.OrdinalIgnoreCase))) return false;
            return true;
        }

        private static bool TieneVentanaVisible(int id, int sesionActual)
        {
            try
            {
                using (Process proceso = Process.GetProcessById(id))
                    return proceso.SessionId == sesionActual && proceso.MainWindowHandle != IntPtr.Zero;
            }
            catch { return false; }
        }

        private static void AgregarDescendientes(int id, IDictionary<int, List<int>> porPadre, ISet<int> resultado)
        {
            if (!resultado.Add(id)) return;
            List<int> hijos;
            if (!porPadre.TryGetValue(id, out hijos)) return;
            foreach (int hijo in hijos) AgregarDescendientes(hijo, porPadre, resultado);
        }

        private static void EsperarCierre(IEnumerable<int> ids, int limiteMs)
        {
            DateTime limite = DateTime.UtcNow.AddMilliseconds(limiteMs);
            while (DateTime.UtcNow < limite)
            {
                bool quedan = ids.Any(id =>
                {
                    try { using (Process proceso = Process.GetProcessById(id)) return !proceso.HasExited; }
                    catch { return false; }
                });
                if (!quedan) return;
                Thread.Sleep(100);
            }
        }

        private static string ObtenerClavePerfil(string ruta, IEnumerable<string> argumentos)
        {
            var perfil = new List<string> { ruta.ToLowerInvariant() };
            string[] args = argumentos.ToArray();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith("--user-data-dir", StringComparison.OrdinalIgnoreCase) ||
                    arg.StartsWith("--profile-directory", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-P", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-profile", StringComparison.OrdinalIgnoreCase) ||
                    arg.StartsWith("--profile=", StringComparison.OrdinalIgnoreCase))
                {
                    perfil.Add(arg.ToLowerInvariant());
                    if (!arg.Contains("=") && i + 1 < args.Length) perfil.Add(args[++i].ToLowerInvariant());
                }
            }
            return String.Join("|", perfil);
        }

        private static string PrepararArgumentos(string ruta, IEnumerable<string> argumentos)
        {
            var args = argumentos.Where(a => !EsUrlNavegacion(a)).ToList();
            string nombre = Path.GetFileName(ruta);
            bool esChromium = !nombre.Equals("firefox.exe", StringComparison.OrdinalIgnoreCase);
            if (esChromium && !args.Any(a => a.Equals("--restore-last-session", StringComparison.OrdinalIgnoreCase)))
                args.Add("--restore-last-session");
            return String.Join(" ", args.Select(CitarArgumento));
        }

        private static bool EsUrlNavegacion(string argumento)
        {
            Uri uri;
            return Uri.TryCreate(argumento, UriKind.Absolute, out uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        private static string[] SepararArgumentos(string lineaComandos)
        {
            int cantidad;
            IntPtr argv = CommandLineToArgvW(lineaComandos, out cantidad);
            if (argv == IntPtr.Zero) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            try
            {
                var resultado = new List<string>();
                for (int i = 1; i < cantidad; i++)
                    resultado.Add(Marshal.PtrToStringUni(Marshal.ReadIntPtr(argv, i * IntPtr.Size)));
                return resultado.ToArray();
            }
            finally { LocalFree(argv); }
        }

        private static string CitarArgumento(string argumento)
        {
            if (argumento.Length > 0 && argumento.All(c => !Char.IsWhiteSpace(c) && c != '"')) return argumento;
            var salida = new StringBuilder("\"");
            int barras = 0;
            foreach (char caracter in argumento)
            {
                if (caracter == '\\') { barras++; continue; }
                if (caracter == '"')
                {
                    salida.Append('\\', barras * 2 + 1).Append('"');
                    barras = 0;
                    continue;
                }
                salida.Append('\\', barras).Append(caracter);
                barras = 0;
            }
            salida.Append('\\', barras * 2).Append('"');
            return salida.ToString();
        }

        [DllImport("shell32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string commandLine, out int argumentCount);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr LocalFree(IntPtr memory);
    }
}
