using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using CapaEntidades;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace CapaDatos
{
    public class D_ExploradorAplicaciones
    {
        private const string AppsFolder =
            "shell:::{4234d49b-0245-4df3-b780-3893943456e1}";

        private static Dictionary<string, string> _paquetesAppx;
        private static readonly Lazy<HashSet<string>> _programasClasicos =
            new Lazy<HashSet<string>>(CargarNombresProgramasClasicos);
        private static readonly Lazy<HashSet<string>> _rutasInstalacionClasicas =
    new Lazy<HashSet<string>>(CargarRutasInstalacionClasicas);

        public List<E_Aplicacion> BuscarAppsInstaladas()
        {
            var aplicaciones = new List<E_Aplicacion>();

            var identificadores = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );

            var nombres = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );

            object shellObject = null;
            object carpetaObject = null;
            object elementosObject = null;

            try
            {
                Type tipoShell = Type.GetTypeFromProgID("Shell.Application");

                if (tipoShell == null)
                {
                    throw new InvalidOperationException(
                        "No se pudo acceder al Shell de Windows."
                    );
                }

                shellObject = Activator.CreateInstance(tipoShell);
                dynamic shell = shellObject;

                carpetaObject = shell.NameSpace(AppsFolder);

                if (carpetaObject == null)
                {
                    throw new InvalidOperationException(
                        "Windows no pudo abrir la carpeta de aplicaciones."
                    );
                }

                dynamic carpeta = carpetaObject;
                elementosObject = carpeta.Items();
                dynamic elementos = elementosObject;

                int cantidad = Convert.ToInt32(elementos.Count);

                for (int i = 0; i < cantidad; i++)
                {
                    object elementoObject = null;
                    object enlaceObject = null;

                    try
                    {
                        elementoObject = elementos.Item(i);
                        dynamic elemento = elementoObject;

                        string nombre = Convert.ToString(elemento.Name);
                        string identificador =
                            Convert.ToString(elemento.Path);

                        if (String.IsNullOrWhiteSpace(nombre) ||
                            String.IsNullOrWhiteSpace(identificador) ||
                            identificador.Length > 255)
                        {
                            continue;
                        }

                        string rutaIcono = null;
                        string rutaDestinoEnlace = null;
                        int indiceIcono = 0;
                        bool esAccesoDirecto = false;

                        try
                        {
                            esAccesoDirecto =
                                Convert.ToBoolean(elemento.IsLink);

                            if (esAccesoDirecto)
                            {
                                enlaceObject = elemento.GetLink();

                                if (enlaceObject != null)
                                {
                                    dynamic enlace = enlaceObject;

                                    string rutaDeclarada = String.Empty;

                                    try
                                    {
                                        indiceIcono = Convert.ToInt32(
                                            enlace.GetIconLocation(
                                                out rutaDeclarada
                                            )
                                        );
                                    }
                                    catch
                                    {
                                        rutaDeclarada = String.Empty;
                                    }

                                    rutaDeclarada =
                                        ExpandirRuta(rutaDeclarada);

                                    if (File.Exists(rutaDeclarada))
                                        rutaIcono = rutaDeclarada;

                                    try
                                    {
                                        rutaDestinoEnlace =
                                            Convert.ToString(enlace.Path);
                                    }
                                    catch
                                    {
                                        rutaDestinoEnlace = null;
                                    }

                                    rutaDestinoEnlace =
                                        ExpandirRuta(rutaDestinoEnlace);

                                    if (String.IsNullOrWhiteSpace(rutaIcono) &&
                                        File.Exists(rutaDestinoEnlace))
                                    {
                                        rutaIcono = rutaDestinoEnlace;
                                        indiceIcono = 0;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Se mantiene la app y se prueban otras fuentes.
                        }

                        bool esAumid = identificador.IndexOf('!') > 0;

                        bool paqueteInstalado =
                            esAumid && PaqueteEstaInstalado(identificador);

                        if (esAumid && !paqueteInstalado)
                            continue;

                        bool destinoEsProtocolo =
                            Uri.TryCreate(
                                rutaDestinoEnlace,
                                UriKind.Absolute,
                                out Uri destinoUri
                            ) &&
                            !destinoUri.IsFile;

                        string steamAppId = ObtenerSteamAppId(
                                rutaDestinoEnlace,
                                identificador);

                        bool steamInstalado =
                            !String.IsNullOrWhiteSpace(steamAppId) &&
                            SteamAppInstalada(steamAppId);

                        bool programaClasicoInstalado =
                            !destinoEsProtocolo &&
                            ProgramaClasicoInstalado(
                                nombre,
                                rutaDestinoEnlace,
                                identificador);

                        if (!paqueteInstalado &&
                            !programaClasicoInstalado &&
                            !steamInstalado)
                        {
                            continue;
                        }
                        // Para paquetes, usa primero el logo del manifiesto.
                        if (esAumid)
                        {
                            string logoPaquete =
                                ObtenerIconoPaquete(identificador);

                            if (!String.IsNullOrWhiteSpace(logoPaquete))
                            {
                                rutaIcono = logoPaquete;
                                indiceIcono = 0;
                            }
                        }

                        string claveNombre = NormalizarNombre(nombre);

                        if (!identificadores.Add(identificador) ||
                            !nombres.Add(claveNombre))
                        {
                            continue;
                        }

                        aplicaciones.Add(new E_Aplicacion
                        {
                            Nombre = nombre.Trim(),
                            Identificador = identificador,
                            EsCritica = false,
                            RutaIcono = rutaIcono,
                            IndiceIcono = indiceIcono
                        });
                    }
                    finally
                    {
                        LiberarCom(enlaceObject);
                        LiberarCom(elementoObject);
                    }
                }
            }
            finally
            {
                LiberarCom(elementosObject);
                LiberarCom(carpetaObject);
                LiberarCom(shellObject);
            }

            AgregarAccesosDirectosMenuInicio(
                aplicaciones,
                identificadores,
                nombres);

            return aplicaciones
                .OrderBy(app => app.Nombre)
                .ToList();
        }

        private static void AgregarAccesosDirectosMenuInicio(
    List<E_Aplicacion> aplicaciones,
    HashSet<string> identificadores,
    HashSet<string> nombres)
        {
            string[] carpetasMenuInicio =
            {
        Environment.GetFolderPath(
            Environment.SpecialFolder.StartMenu),

        Environment.GetFolderPath(
            Environment.SpecialFolder.CommonStartMenu)
    };

            Type tipoShell =
                Type.GetTypeFromProgID("WScript.Shell");

            if (tipoShell == null)
                return;

            object shellObject = null;

            try
            {
                shellObject = Activator.CreateInstance(tipoShell);
                dynamic shell = shellObject;

                foreach (string carpeta in carpetasMenuInicio
                    .Where(Directory.Exists)
                    .Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    IEnumerable<string> accesos;

                    try
                    {
                        accesos = Directory.EnumerateFiles(
                            carpeta,
                            "*.lnk",
                            SearchOption.AllDirectories);
                    }
                    catch
                    {
                        continue;
                    }

                    foreach (string rutaAcceso in accesos)
                    {
                        object accesoObject = null;

                        try
                        {
                            dynamic acceso =
                                shell.CreateShortcut(rutaAcceso);

                            accesoObject = acceso;

                            string destino = ExpandirRuta(
                                Convert.ToString(acceso.TargetPath));

                            if (String.IsNullOrWhiteSpace(destino) ||
                                !String.Equals(
                                    Path.GetExtension(destino),
                                    ".exe",
                                    StringComparison.OrdinalIgnoreCase) ||
                                !File.Exists(destino) ||
                                destino.Length > 255)
                            {
                                continue;
                            }

                            string nombre =
                                Path.GetFileNameWithoutExtension(rutaAcceso);

                            string claveNombre = NormalizarNombre(nombre);

                            if (String.IsNullOrWhiteSpace(claveNombre) ||
                                identificadores.Contains(destino) ||
                                nombres.Contains(claveNombre))
                            {
                                continue;
                            }

                            // Confirma que el ejecutable pertenezca a un producto
                            // instalado y registrado, usando su ruta.
                            if (!ProgramaClasicoInstalado(nombre, destino))
                                continue;

                            identificadores.Add(destino);
                            nombres.Add(claveNombre);

                            aplicaciones.Add(new E_Aplicacion
                            {
                                Nombre = nombre.Trim(),
                                Identificador = Path.GetFullPath(destino),
                                EsCritica = false,
                                RutaIcono = Path.GetFullPath(destino),
                                IndiceIcono = 0
                            });
                        }
                        catch
                        {
                            // Ignora accesos directos dañados o que no sean ejecutables.
                        }
                        finally
                        {
                            LiberarCom(accesoObject);
                        }
                    }
                }
            }
            catch
            {
                // Si WScript.Shell no está disponible, se conserva la lista de AppsFolder.
            }
            finally
            {
                LiberarCom(shellObject);
            }
        }

        private static bool PaqueteEstaInstalado(string identificador)
        {
            int separador = identificador.IndexOf('!');

            if (separador <= 0)
                return false;

            string familia = identificador.Substring(0, separador);

            const string prefijo = "shell:AppsFolder\\";

            if (familia.StartsWith(
                prefijo,
                StringComparison.OrdinalIgnoreCase))
            {
                familia = familia.Substring(prefijo.Length);
            }

            var paquetes = ObtenerPaquetesAppx();

            // El AUMID solo se acepta si la familia figura en el inventario AppX.
            return paquetes.ContainsKey(familia);
        }

        private static bool EsRutaLocal(string valor)
        {
            if (String.IsNullOrWhiteSpace(valor))
                return false;

            valor = ExpandirRuta(valor);

            if (String.IsNullOrWhiteSpace(valor))
                return false;

            if (valor.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                valor.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                valor.StartsWith("steam://", StringComparison.OrdinalIgnoreCase) ||
                valor.StartsWith("shell:", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return Path.IsPathRooted(valor) ||
                   (valor.Length >= 2 &&
                    Char.IsLetter(valor[0]) &&
                    valor[1] == ':');
        }

        public List<E_Aplicacion> OrdenarPorRelevancia(
            IEnumerable<E_Aplicacion> aplicaciones,
            string termino)
        {
            var lista = aplicaciones == null
                ? new List<E_Aplicacion>()
                : aplicaciones.ToList();

            if (String.IsNullOrWhiteSpace(termino))
            {
                return lista
                    .OrderBy(app => app.Nombre)
                    .ToList();
            }

            string terminoNormalizado =
                NormalizarNombre(termino);

            var resultadosIndice =
                new List<KeyValuePair<string, int>>();

            const string connectionString =
                "Provider=Search.CollatorDSO;" +
                "Extended Properties='Application=Windows';";

            const string sql = @"
                SELECT System.ItemName, System.Search.Rank
                FROM SystemIndex
                WHERE System.Kind = 'program'
                  AND FREETEXT(System.ItemName, ?)
                ORDER BY System.Search.Rank DESC";

            try
            {
                using (var conexion =
                    new OleDbConnection(connectionString))
                using (var comando =
                    new OleDbCommand(sql, conexion))
                {
                    comando.Parameters.Add(
                        "termino",
                        OleDbType.VarWChar,
                        256
                    ).Value = termino.Trim();

                    conexion.Open();

                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            string nombre = lector.IsDBNull(0)
                                ? String.Empty
                                : lector.GetString(0);

                            int rango = lector.IsDBNull(1)
                                ? 0
                                : Convert.ToInt32(lector.GetValue(1));

                            if (!String.IsNullOrWhiteSpace(nombre))
                            {
                                resultadosIndice.Add(
                                    new KeyValuePair<string, int>(
                                        NormalizarNombre(nombre),
                                        rango
                                    )
                                );
                            }
                        }
                    }
                }
            }
            catch
            {
                // Si Windows Search no está disponible,
                // se usa el filtrado local de nombres.
            }

            var rangosPorAplicacion =
                new Dictionary<E_Aplicacion, int>();

            foreach (E_Aplicacion app in lista)
            {
                string nombreApp =
                    NormalizarNombre(app.Nombre);

                var coincidencia = resultadosIndice
                    .Where(resultado =>
                        resultado.Key == nombreApp ||
                        resultado.Key.Contains(nombreApp) ||
                        nombreApp.Contains(resultado.Key))
                    .OrderByDescending(resultado => resultado.Value)
                    .FirstOrDefault();

                if (!String.IsNullOrEmpty(coincidencia.Key))
                {
                    rangosPorAplicacion[app] =
                        coincidencia.Value;
                }
            }

            return lista
                .Where(app =>
                    rangosPorAplicacion.ContainsKey(app) ||
                    NormalizarNombre(app.Nombre)
                        .Contains(terminoNormalizado))
                .OrderByDescending(app =>
                    rangosPorAplicacion.ContainsKey(app) ? 1 : 0)
                .ThenByDescending(app =>
                    rangosPorAplicacion.ContainsKey(app)
                        ? rangosPorAplicacion[app]
                        : 0)
                .ThenBy(app => app.Nombre)
                .ToList();
        }
        private static bool ProgramaClasicoInstalado(
            string nombre,
            params string[] rutasCandidatas)
        {
            string rutaEjecutable =
                ObtenerRutaEjecutable(rutasCandidatas);

            // Una entrada clásica debe resolver a un ejecutable local existente.
            if (String.IsNullOrWhiteSpace(rutaEjecutable))
                return false;

            // Primero valida si el ejecutable está dentro de una ubicación
            // registrada para un producto instalado.
            foreach (string rutaRegistrada in
                _rutasInstalacionClasicas.Value)
            {
                if (String.Equals(
                    rutaEjecutable,
                    rutaRegistrada,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                string prefijoCarpeta = rutaRegistrada.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar)
                    + Path.DirectorySeparatorChar;

                if (rutaEjecutable.StartsWith(
                    prefijoCarpeta,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            // Respaldo para instaladores que registran el programa,
            // pero no publican una ubicación de instalación útil.
            string buscado = NormalizarNombre(nombre);

            if (String.IsNullOrWhiteSpace(buscado))
                return false;

            return _programasClasicos.Value.Any(registrado =>
                CoincideNombreInstalado(buscado, registrado));
        }

        // Lee las claves de desinstalación una sola vez, no una vez por app.
        private static HashSet<string> CargarNombresProgramasClasicos()
        {
            var nombres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            RegistryHive[] hives = { RegistryHive.LocalMachine, RegistryHive.CurrentUser };
            RegistryView[] vistas = { RegistryView.Registry64, RegistryView.Registry32 };
            string[] rutas =
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (RegistryHive hive in hives)
                foreach (RegistryView vista in vistas)
                    foreach (string ruta in rutas)
                    {
                        try
                        {
                            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, vista))
                            using (RegistryKey uninstall = baseKey.OpenSubKey(ruta))
                            {
                                if (uninstall == null) continue;
                                foreach (string subclave in uninstall.GetSubKeyNames())
                                    using (RegistryKey programa = uninstall.OpenSubKey(subclave))
                                    {
                                        if (programa == null) continue;
                                        string displayName = Convert.ToString(programa.GetValue("DisplayName"));
                                        string normalizado = NormalizarNombre(displayName);
                                        if (String.IsNullOrWhiteSpace(normalizado)) continue;

                                        string ubicacion = ExpandirRuta(Convert.ToString(programa.GetValue("InstallLocation")));
                                        string icono = ObtenerRutaDisplayIcon(Convert.ToString(programa.GetValue("DisplayIcon")));
                                        if ((!String.IsNullOrWhiteSpace(ubicacion) && Directory.Exists(ubicacion)) ||
                                            (!String.IsNullOrWhiteSpace(icono) && File.Exists(icono)))
                                            nombres.Add(normalizado);
                                    }
                            }
                        }
                        catch
                        {
                            // Continúa con la siguiente vista del registro.
                        }
                    }
            return nombres;
        }
        private static HashSet<string> CargarRutasInstalacionClasicas()
        {
            var rutasRegistradas = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            RegistryHive[] hives =
            {
        RegistryHive.LocalMachine,
        RegistryHive.CurrentUser
    };

            RegistryView[] vistas =
            {
        RegistryView.Registry64,
        RegistryView.Registry32
    };

            string[] rutasRegistro =
            {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    };

            foreach (RegistryHive hive in hives)
            {
                foreach (RegistryView vista in vistas)
                {
                    foreach (string rutaRegistro in rutasRegistro)
                    {
                        try
                        {
                            using (RegistryKey baseKey =
                                RegistryKey.OpenBaseKey(hive, vista))
                            using (RegistryKey uninstall =
                                baseKey.OpenSubKey(rutaRegistro))
                            {
                                if (uninstall == null)
                                    continue;

                                foreach (string subclave in
                                    uninstall.GetSubKeyNames())
                                {
                                    using (RegistryKey programa =
                                        uninstall.OpenSubKey(subclave))
                                    {
                                        if (programa == null)
                                            continue;

                                        string nombre = Convert.ToString(
                                            programa.GetValue("DisplayName"));

                                        if (String.IsNullOrWhiteSpace(nombre))
                                            continue;

                                        string ubicacion = ExpandirRuta(
                                            Convert.ToString(
                                                programa.GetValue("InstallLocation")));

                                        if (!String.IsNullOrWhiteSpace(ubicacion) &&
                                            Directory.Exists(ubicacion))
                                        {
                                            try
                                            {
                                                rutasRegistradas.Add(
                                                    Path.GetFullPath(ubicacion)
                                                        .TrimEnd(
                                                            Path.DirectorySeparatorChar,
                                                            Path.AltDirectorySeparatorChar));
                                            }
                                            catch
                                            {
                                                // Ignora la ruta mal formada.
                                            }
                                        }

                                        string icono = ObtenerRutaDisplayIcon(
                                            Convert.ToString(
                                                programa.GetValue("DisplayIcon")));

                                        if (!String.IsNullOrWhiteSpace(icono) &&
                                            File.Exists(icono) &&
                                            String.Equals(
                                                Path.GetExtension(icono),
                                                ".exe",
                                                StringComparison.OrdinalIgnoreCase))
                                        {
                                            try
                                            {
                                                rutasRegistradas.Add(
                                                    Path.GetFullPath(icono));
                                            }
                                            catch
                                            {
                                                // Ignora la ruta mal formada.
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Sigue con las demás ubicaciones del registro.
                        }
                    }
                }
            }

            return rutasRegistradas;
        }
        private static bool CoincideNombreInstalado(
            string nombreBuscado,
            string nombreRegistrado)
        {
            if (String.IsNullOrWhiteSpace(nombreBuscado) ||
                String.IsNullOrWhiteSpace(nombreRegistrado))
            {
                return false;
            }

            if (String.Equals(
                nombreBuscado,
                nombreRegistrado,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return (nombreBuscado.Length >= 6 &&
                    nombreRegistrado.Contains(nombreBuscado)) ||
                   (nombreRegistrado.Length >= 6 &&
                    nombreBuscado.Contains(nombreRegistrado));
        }

        private static string ObtenerRutaDisplayIcon(string displayIcon)
        {
            if (String.IsNullOrWhiteSpace(displayIcon))
                return null;

            displayIcon = displayIcon.Trim();

            // DisplayIcon suele tener formato: "C:\App\app.exe",0
            if (displayIcon.StartsWith("\""))
            {
                int cierreComillas = displayIcon.IndexOf('"', 1);

                if (cierreComillas > 1)
                    displayIcon = displayIcon.Substring(1, cierreComillas - 1);
            }
            else
            {
                int coma = displayIcon.IndexOf(',');

                if (coma >= 0)
                    displayIcon = displayIcon.Substring(0, coma);
            }

            return ExpandirRuta(displayIcon);
        }

        private static string ObtenerSteamAppId(
            string destino,
            string identificador)
        {
            const string prefijo = "steam://rungameid/";

            string[] valores = { destino, identificador };

            foreach (string valor in valores)
            {
                if (String.IsNullOrWhiteSpace(valor) ||
                    !valor.StartsWith(
                        prefijo,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string id = valor.Substring(prefijo.Length)
                    .Trim()
                    .TrimEnd('/');

                if (id.Length > 0 && id.All(Char.IsDigit))
                    return id;
            }

            return null;
        }

        private static bool SteamAppInstalada(string appId)
        {
            var bibliotecas = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );

            AgregarRutaSteam(
                bibliotecas,
                RegistryHive.CurrentUser,
                RegistryView.Default,
                @"Software\Valve\Steam",
                "SteamPath"
            );

            AgregarRutaSteam(
                bibliotecas,
                RegistryHive.LocalMachine,
                RegistryView.Registry64,
                @"SOFTWARE\Valve\Steam",
                "InstallPath"
            );

            AgregarRutaSteam(
                bibliotecas,
                RegistryHive.LocalMachine,
                RegistryView.Registry32,
                @"SOFTWARE\Valve\Steam",
                "InstallPath"
            );

            foreach (string biblioteca in bibliotecas.ToArray())
            {
                string archivoBibliotecas = Path.Combine(
                    biblioteca,
                    "steamapps",
                    "libraryfolders.vdf"
                );

                if (!File.Exists(archivoBibliotecas))
                    continue;

                try
                {
                    string contenido =
                        File.ReadAllText(archivoBibliotecas);

                    MatchCollection coincidencias = Regex.Matches(
                        contenido,
                        "\"path\"\\s+\"([^\"]+)\""
                    );

                    foreach (Match coincidencia in coincidencias)
                    {
                        string ruta = coincidencia.Groups[1].Value
                            .Replace("\\\\", "\\");

                        if (!String.IsNullOrWhiteSpace(ruta))
                            bibliotecas.Add(ruta);
                    }
                }
                catch
                {
                    // Se conserva la biblioteca principal si falla la lectura.
                }
            }

            foreach (string biblioteca in bibliotecas)
            {
                string manifiesto = Path.Combine(
                    biblioteca,
                    "steamapps",
                    "appmanifest_" + appId + ".acf"
                );

                if (File.Exists(manifiesto))
                    return true;
            }

            return false;
        }

        private static void AgregarRutaSteam(
            HashSet<string> rutas,
            RegistryHive hive,
            RegistryView vista,
            string subclave,
            string nombreValor)
        {
            try
            {
                using (RegistryKey baseKey =
                    RegistryKey.OpenBaseKey(hive, vista))
                using (RegistryKey key =
                    baseKey.OpenSubKey(subclave))
                {
                    if (key == null)
                        return;

                    string ruta = ExpandirRuta(
                        Convert.ToString(key.GetValue(nombreValor))
                    );

                    if (!String.IsNullOrWhiteSpace(ruta) &&
                        Directory.Exists(ruta))
                    {
                        rutas.Add(ruta);
                    }
                }
            }
            catch
            {
                // Esa ubicación del registro no está disponible.
            }
        }
        private static string ObtenerIconoPaquete(
            string identificador)
        {
            if (String.IsNullOrWhiteSpace(identificador))
                return null;

            int separador = identificador.IndexOf('!');

            if (separador <= 0 ||
                separador >= identificador.Length - 1)
            {
                return null;
            }

            string familiaPaquete =
                identificador.Substring(0, separador).Trim();

            string idAplicacion =
                identificador.Substring(separador + 1).Trim();

            string carpetaPaquete;

            if (!ObtenerPaquetesAppx().TryGetValue(
                familiaPaquete,
                out carpetaPaquete))
            {
                return null;
            }

            string manifiesto = Path.Combine(
                carpetaPaquete,
                "AppxManifest.xml"
            );

            if (!File.Exists(manifiesto))
                return null;

            try
            {
                var documento = new XmlDocument();
                documento.Load(manifiesto);

                XmlNodeList aplicaciones = documento.SelectNodes(
                    "//*[local-name()='Application']"
                );

                foreach (XmlNode aplicacion in aplicaciones)
                {
                    XmlAttribute atributoId =
                        aplicacion.Attributes["Id"];

                    if (atributoId == null ||
                        !String.Equals(
                            atributoId.Value,
                            idAplicacion,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    XmlNode visualElements =
                        aplicacion.SelectSingleNode(
                            "./*[local-name()='VisualElements']"
                        );

                    if (visualElements == null ||
                        visualElements.Attributes == null)
                    {
                        continue;
                    }

                    string[] nombresLogo =
                    {
                        "Square44x44Logo",
                        "Square150x150Logo",
                        "Logo"
                    };

                    foreach (string nombreLogo in nombresLogo)
                    {
                        XmlAttribute atributoLogo =
                            visualElements.Attributes[nombreLogo];

                        if (atributoLogo == null ||
                            String.IsNullOrWhiteSpace(
                                atributoLogo.Value))
                        {
                            continue;
                        }

                        string rutaLogo = ResolverRutaLogo(
                            carpetaPaquete,
                            atributoLogo.Value
                        );

                        if (!String.IsNullOrWhiteSpace(rutaLogo))
                            return rutaLogo;
                    }
                }
            }
            catch
            {
                // Si no se puede leer el manifiesto,
                // se prueba la extracción normal del Shell.
            }

            return null;
        }

        private static Dictionary<string, string>
            ObtenerPaquetesAppx()
        {
            if (_paquetesAppx != null)
                return _paquetesAppx;

            var paquetes = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase
            );

            try
            {
                string carpetaWindows =
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.Windows
                    );

                string powershell = Path.Combine(
                    carpetaWindows,
                    @"System32\WindowsPowerShell\v1.0\powershell.exe"
                );

                if (!File.Exists(powershell))
                {
                    powershell = Path.Combine(
                        carpetaWindows,
                        @"Sysnative\WindowsPowerShell\v1.0\powershell.exe"
                    );
                }

                if (!File.Exists(powershell))
                {
                    _paquetesAppx = paquetes;
                    return _paquetesAppx;
                }

                var inicio = new ProcessStartInfo
                {
                    FileName = powershell,
                    Arguments =
                        "-NoLogo -NoProfile -NonInteractive " +
                        "-Command \"Import-Module Appx; " +
                        "Get-AppxPackage | ForEach-Object { " +
                        "Write-Output ('{0}|{1}' -f " +
                        "$_.PackageFamilyName, $_.InstallLocation) }\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process proceso = Process.Start(inicio))
                {
                    if (proceso != null)
                    {
                        string salida =
                            proceso.StandardOutput.ReadToEnd();

                        proceso.WaitForExit();

                        foreach (string linea in salida.Split(
                            new[] { '\r', '\n' },
                            StringSplitOptions.RemoveEmptyEntries))
                        {
                            string[] partes = linea.Split(
                                new[] { '|' },
                                2
                            );

                            if (partes.Length != 2)
                                continue;

                            string familia = partes[0]
                                .Trim()
                                .TrimStart('\uFEFF');

                            string ruta = partes[1].Trim();

                            if (!String.IsNullOrWhiteSpace(familia) &&
                                !String.IsNullOrWhiteSpace(ruta) &&
                                !paquetes.ContainsKey(familia))
                            {
                                paquetes.Add(familia, ruta);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Los iconos de otras fuentes siguen disponibles.
            }

            _paquetesAppx = paquetes;
            return _paquetesAppx;
        }

        private static string ResolverRutaLogo(
            string carpetaPaquete,
            string rutaRelativa)
        {
            try
            {
                rutaRelativa = rutaRelativa
                    .Replace('/', Path.DirectorySeparatorChar)
                    .Replace('\\', Path.DirectorySeparatorChar);

                string rutaCompleta = Path.Combine(
                    carpetaPaquete,
                    rutaRelativa
                );

                if (File.Exists(rutaCompleta))
                    return rutaCompleta;

                string carpetaLogo =
                    Path.GetDirectoryName(rutaCompleta);

                string nombreBase =
                    Path.GetFileNameWithoutExtension(rutaCompleta);

                if (String.IsNullOrWhiteSpace(carpetaLogo) ||
                    !Directory.Exists(carpetaLogo))
                {
                    return null;
                }

                return Directory
                    .GetFiles(carpetaLogo, nombreBase + "*.png")
                    .OrderByDescending(
                        ruta => new FileInfo(ruta).Length
                    )
                    .FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static string ObtenerRutaEjecutable(
    params string[] candidatos)
        {
            if (candidatos == null)
                return null;

            foreach (string candidato in candidatos)
            {
                string ruta = ExpandirRuta(candidato);

                if (String.IsNullOrWhiteSpace(ruta))
                    continue;

                if (!String.Equals(
                    Path.GetExtension(ruta),
                    ".exe",
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    if (File.Exists(ruta))
                        return Path.GetFullPath(ruta);
                }
                catch
                {
                    // Prueba el siguiente candidato.
                }
            }

            return null;
        }
        private static string ExpandirRuta(string ruta)
        {
            if (String.IsNullOrWhiteSpace(ruta))
                return null;

            return Environment.ExpandEnvironmentVariables(
                ruta.Trim().Trim('"')
            );
        }

        private static string NormalizarNombre(string nombre)
        {
            if (String.IsNullOrWhiteSpace(nombre))
                return String.Empty;

            return new string(
                nombre
                    .Where(Char.IsLetterOrDigit)
                    .Select(Char.ToUpperInvariant)
                    .ToArray()
            );
        }

        private static void LiberarCom(object objeto)
        {
            if (objeto != null && Marshal.IsComObject(objeto))
                Marshal.FinalReleaseComObject(objeto);
        }
    }
}