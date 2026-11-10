using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace CapaNegocio
{
    /// <summary>Proxy HTTP local que decide el acceso por dominio y contexto de solicitud.</summary>
    internal sealed class N_ProxyWebLocal : IDisposable
    {
        private const string RaizCertificados = "CN=Programa Concentracion HTTPS Local";
        private const string RutaInternetSettings = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
        private const string RutaCopiaProxy = @"Software\Concentracion\ProxyAnterior";
        private const string ProxyLoopback = "127.0.0.1";
        private static readonly string[] ValoresProxy = { "ProxyEnable", "ProxyServer", "ProxyOverride", "AutoConfigURL", "AutoDetect" };

        private readonly object _sync = new object();
        private readonly Dictionary<string, X509Certificate2> _certificadosSitio = new Dictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, RSACryptoServiceProvider> _clavesSitio = new Dictionary<string, RSACryptoServiceProvider>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _dominiosPermitidos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _dependenciasConocidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _navegacionesExternasPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private TcpListener _listener;
        private CancellationTokenSource _cancelacion;
        private X509Certificate2 _ca;
        private Task _aceptar;
        private bool _disposed;

        public int Puerto { get; private set; }

        public N_ProxyWebLocal()
        {
            RecuperarEstadoAnterior();
            AppDomain.CurrentDomain.ProcessExit += ProcessExit;
        }

        public void Iniciar(IEnumerable<string> dominios)
        {
            lock (_sync)
            {
                if (_disposed) throw new ObjectDisposedException("N_ProxyWebLocal");
                ActualizarDominios(dominios);
                if (_listener != null) return;

                if (_ca == null) _ca = ObtenerOCrearCA();
                _cancelacion = new CancellationTokenSource();
                _listener = new TcpListener(IPAddress.Loopback, 0);
                _listener.Start(128);
                Puerto = ((IPEndPoint)_listener.LocalEndpoint).Port;
                try
                {
                    GuardarConfiguracionProxyAnterior();
                    ConfigurarProxy(Puerto);
                    CancellationToken token = _cancelacion.Token;
                    _aceptar = Task.Run(() => AceptarClientes(token));
                }
                catch
                {
                    _listener.Stop();
                    _listener = null;
                    Puerto = 0;
                    RestaurarConfiguracionProxy();
                    throw;
                }
            }
        }

        public void ActualizarDominios(IEnumerable<string> dominios)
        {
            lock (_sync)
            {
                var nuevosDominios = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string dominio in dominios ?? Enumerable.Empty<string>())
                {
                    string normalizado = NormalizarHost(dominio);
                    if (!String.IsNullOrWhiteSpace(normalizado))
                        nuevosDominios.Add(normalizado);
                }
                if (!_dominiosPermitidos.SetEquals(nuevosDominios))
                {
                    _dependenciasConocidas.Clear();
                    _navegacionesExternasPermitidas.Clear();
                }
                _dominiosPermitidos.Clear();
                _dominiosPermitidos.UnionWith(nuevosDominios);
            }
        }

        private void AceptarClientes(CancellationToken token)
        {
            TcpListener listener = _listener;
            while (listener != null && !token.IsCancellationRequested)
            {
                try
                {
                    TcpClient cliente = listener.AcceptTcpClient();
                    Task.Run(() => AtenderCliente(cliente, token));
                }
                catch (SocketException)
                {
                    if (!token.IsCancellationRequested) Trace.TraceWarning("El proxy local no pudo aceptar una conexión.");
                    break;
                }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex) { Trace.TraceWarning("Error aceptando conexión del proxy: " + ex.Message); break; }
            }
        }

        private void AtenderCliente(TcpClient cliente, CancellationToken token)
        {
            bool esTunelTls = false;
            using (cliente)
            {
                try
                {
                    NetworkStream red = cliente.GetStream();
                    string cabecera = LeerCabecera(red);
                    if (String.IsNullOrEmpty(cabecera)) return;
                    Peticion peticion = AnalizarPeticion(cabecera);
                    if (String.Equals(peticion.Metodo, "CONNECT", StringComparison.OrdinalIgnoreCase))
                    {
                        esTunelTls = true;
                        Uri destino = new Uri("https://" + peticion.Destino + "/");
                        // Las webs autorizadas conservan TLS extremo a extremo. Así el sitio
                        // recibe la huella TLS real del navegador y no la de HttpWebRequest.
                        if (EsDominioPermitido(destino.IdnHost))
                        {
                            TunelTransparente(cliente, destino);
                            return;
                        }
                        EscribirTexto(red, "HTTP/1.1 200 Connection Established\r\nProxy-Agent: Concentracion\r\n\r\n");
                        using (var tls = new SslStream(red, true))
                        {
                            try
                            {
                                tls.AuthenticateAsServer(ObtenerCertificadoSitio(destino.IdnHost), false, SslProtocols.Tls12, false);
                            }
                            catch (AuthenticationException)
                            {
                                // Algunos clientes cancelan el handshake (por ejemplo, certificado rechazado o sesión cerrada).
                                // No es una petición HTTP bloqueada y no se debe intentar responder en texto plano.
                                return;
                            }
                            while (!token.IsCancellationRequested)
                            {
                                string cabeceraTls = LeerCabecera(tls);
                                if (String.IsNullOrEmpty(cabeceraTls)) break;
                                Peticion peticionTls = AnalizarPeticion(cabeceraTls);
                                string host = ObtenerHost(peticionTls, destino.IdnHost);
                                Uri uri = CrearUri(host, peticionTls.Destino, true);
                                byte[] cuerpo = LeerCuerpo(tls, peticionTls.Cabeceras);
                                if (!PuedeSolicitar(host, peticionTls.Cabeceras))
                                {
                                    EscribirBloqueo(tls, uri.IdnHost);
                                    break;
                                }
                                Reenviar(peticionTls, uri, cuerpo, tls);
                                break; // conexión cerrada tras cada respuesta; el navegador vuelve por el proxy.
                            }
                        }
                    }
                    else
                    {
                        Uri uri;
                        if (!Uri.TryCreate(peticion.Destino, UriKind.Absolute, out uri))
                            uri = CrearUri(ObtenerHost(peticion, null), peticion.Destino, false);
                        byte[] cuerpo = LeerCuerpo(red, peticion.Cabeceras);
                        if (!PuedeSolicitar(uri.IdnHost, peticion.Cabeceras))
                            EscribirBloqueo(red, uri.IdnHost);
                        else
                            Reenviar(peticion, uri, cuerpo, red);
                    }
                }
                catch (IOException ex) when (EsCierreNormalCliente(ex)) { }
                catch (SocketException ex) when (EsCierreNormalCliente(ex)) { }
                catch (ObjectDisposedException) when (token.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Solicitud del proxy local fallida: " + ex.Message);
                    if (!esTunelTls)
                        try { EscribirBloqueo(cliente.GetStream(), null); } catch { }
                }
            }
        }

        private static bool EsCierreNormalCliente(Exception ex)
        {
            for (Exception actual = ex; actual != null; actual = actual.InnerException)
            {
                SocketException socket = actual as SocketException;
                if (socket != null && (socket.SocketErrorCode == SocketError.ConnectionAborted ||
                    socket.SocketErrorCode == SocketError.ConnectionReset ||
                    socket.SocketErrorCode == SocketError.Shutdown ||
                    socket.SocketErrorCode == SocketError.OperationAborted ||
                    socket.SocketErrorCode == SocketError.Interrupted))
                    return true;
            }
            return ex is EndOfStreamException;
        }

        private bool PuedeSolicitar(string host, IDictionary<string, string> cabeceras)
        {
            if (EsDominioPermitido(host)) return true;

            string destino;
            string modo;
            cabeceras.TryGetValue("Sec-Fetch-Dest", out destino);
            cabeceras.TryGetValue("Sec-Fetch-Mode", out modo);
            bool esNavegacion =
                String.Equals(destino, "document", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(modo, "navigate", StringComparison.OrdinalIgnoreCase);

            string referer;
            Uri uriReferer = null;
            bool tieneRefererPermitido = cabeceras.TryGetValue("Referer", out referer) &&
                Uri.TryCreate(referer, UriKind.Absolute, out uriReferer) &&
                EsDominioPermitido(uriReferer.IdnHost);

            // Permite abrir el primer enlace externo pulsado desde una web de la tarea.
            // No habilita una navegación nueva si el usuario escribió otra dirección.
            if (esNavegacion)
            {
                if (tieneRefererPermitido)
                {
                    RegistrarNavegacionExterna(host);
                    RegistrarDependencia(host);
                    return true;
                }

                if (uriReferer != null &&
                    EsDestinoMismoDominioExterno(host, uriReferer.IdnHost))
                {
                    RegistrarDependencia(host);
                    return true;
                }

                return false;
            }

            if (cabeceras.TryGetValue("Referer", out referer) &&
                Uri.TryCreate(referer, UriKind.Absolute, out uriReferer) &&
                EsOrigenPermitido(uriReferer.IdnHost))
            {
                RegistrarDependencia(host);
                return true;
            }

            string origin;
            Uri uriOrigin;
            if (cabeceras.TryGetValue("Origin", out origin) &&
                Uri.TryCreate(origin, UriKind.Absolute, out uriOrigin) &&
                EsOrigenPermitido(uriOrigin.IdnHost))
            {
                RegistrarDependencia(host);
                return true;
            }

            return EsDependenciaConocida(host);
        }

        private bool EsDominioPermitido(string host)
        {
            string normalizado = NormalizarHost(host);
            lock (_sync)
                return _dominiosPermitidos.Any(dominio =>
                    String.Equals(normalizado, dominio, StringComparison.OrdinalIgnoreCase) ||
                    normalizado.EndsWith("." + dominio, StringComparison.OrdinalIgnoreCase));
        }

        private bool EsOrigenPermitido(string host)
        {
            if (EsDominioPermitido(host)) return true;
            string normalizado = NormalizarHost(host);
            lock (_sync)
                return _dependenciasConocidas.Any(dominio =>
                    String.Equals(normalizado, dominio, StringComparison.OrdinalIgnoreCase) ||
                    normalizado.EndsWith("." + dominio, StringComparison.OrdinalIgnoreCase));
        }

        private bool EsDependenciaConocida(string host)
        {
            string normalizado = NormalizarHost(host);
            lock (_sync)
                return _dependenciasConocidas.Any(dominio =>
                    String.Equals(normalizado, dominio, StringComparison.OrdinalIgnoreCase) ||
                    normalizado.EndsWith("." + dominio, StringComparison.OrdinalIgnoreCase));
        }

        private bool EsDestinoMismoDominioExterno(string destino, string origen)
        {
            string destinoNormalizado = NormalizarHost(destino);
            string origenNormalizado = NormalizarHost(origen);
            lock (_sync)
            {
                return _navegacionesExternasPermitidas.Any(dominio =>
                    EsIgualOSubdominio(origenNormalizado, dominio) &&
                    EsIgualOSubdominio(destinoNormalizado, dominio));
            }
        }

        private static bool EsIgualOSubdominio(string host, string dominio)
        {
            return String.Equals(host, dominio, StringComparison.OrdinalIgnoreCase) ||
                host.EndsWith("." + dominio, StringComparison.OrdinalIgnoreCase);
        }

        private void RegistrarNavegacionExterna(string host)
        {
            string normalizado = NormalizarHost(host);
            if (String.IsNullOrWhiteSpace(normalizado) || EsDominioPermitido(normalizado)) return;
            lock (_sync) _navegacionesExternasPermitidas.Add(normalizado);
        }

        private void RegistrarDependencia(string host)
        {
            string normalizado = NormalizarHost(host);
            if (String.IsNullOrWhiteSpace(normalizado) || EsDominioPermitido(normalizado)) return;
            lock (_sync) _dependenciasConocidas.Add(normalizado);
        }

        private void Reenviar(Peticion peticion, Uri uri, byte[] cuerpo, Stream salida)
        {
            string upgrade = Valor(peticion.Cabeceras, "Upgrade");
            if (String.Equals(upgrade, "websocket", StringComparison.OrdinalIgnoreCase))
            {
                ReenviarWebSocket(peticion, uri, cuerpo, salida);
                return;
            }

            HttpWebRequest solicitud = (HttpWebRequest)WebRequest.Create(uri);
            solicitud.Proxy = null;
            solicitud.Method = peticion.Metodo;
            solicitud.AllowAutoRedirect = false;
            solicitud.KeepAlive = false;
            solicitud.AutomaticDecompression = DecompressionMethods.None;
            solicitud.Timeout = 60000;
            solicitud.ReadWriteTimeout = 60000;
            solicitud.UserAgent = Valor(peticion.Cabeceras, "User-Agent");
            solicitud.Accept = Valor(peticion.Cabeceras, "Accept");
            solicitud.Referer = Valor(peticion.Cabeceras, "Referer");
            solicitud.ContentType = Valor(peticion.Cabeceras, "Content-Type");

            foreach (KeyValuePair<string, string> encabezado in peticion.Cabeceras)
            {
                string nombre = encabezado.Key;
                if (new[] { "Host", "Connection", "Proxy-Connection", "Content-Length", "Transfer-Encoding", "User-Agent", "Accept", "Referer", "Content-Type" }
                    .Contains(nombre, StringComparer.OrdinalIgnoreCase)) continue;
                try { solicitud.Headers[nombre] = encabezado.Value; } catch { }
            }

            bool metodoConCuerpo = new[] { "POST", "PUT", "PATCH" }.Contains(peticion.Metodo, StringComparer.OrdinalIgnoreCase);
            if (cuerpo.Length > 0 || metodoConCuerpo)
            {
                solicitud.ContentLength = cuerpo.Length;
                using (Stream cuerpoOrigen = solicitud.GetRequestStream())
                    if (cuerpo.Length > 0) cuerpoOrigen.Write(cuerpo, 0, cuerpo.Length);
            }

            HttpWebResponse respuesta;
            try { respuesta = (HttpWebResponse)solicitud.GetResponse(); }
            catch (WebException ex)
            {
                respuesta = ex.Response as HttpWebResponse;
                if (respuesta == null) throw;
            }

            using (respuesta)
            {
                bool sinCuerpo = String.Equals(peticion.Metodo, "HEAD", StringComparison.OrdinalIgnoreCase) ||
                    (int)respuesta.StatusCode == 204 || (int)respuesta.StatusCode == 304;
                var cabeceraSalida = new StringBuilder();
                cabeceraSalida.Append("HTTP/1.1 ").Append((int)respuesta.StatusCode).Append(' ').Append(respuesta.StatusDescription).Append("\r\n");
                foreach (string nombre in respuesta.Headers.AllKeys)
                {
                    if (new[] { "Connection", "Keep-Alive", "Proxy-Connection", "Transfer-Encoding", "Content-Length", "Upgrade", "Trailer" }
                        .Contains(nombre, StringComparer.OrdinalIgnoreCase)) continue;
                    foreach (string valor in respuesta.Headers.GetValues(nombre) ?? new string[0])
                        cabeceraSalida.Append(nombre).Append(": ").Append(valor).Append("\r\n");
                }
                if (!sinCuerpo) cabeceraSalida.Append("Transfer-Encoding: chunked\r\n");
                cabeceraSalida.Append("Connection: close\r\n\r\n");
                EscribirTexto(salida, cabeceraSalida.ToString());

                if (!sinCuerpo && respuesta.GetResponseStream() != null)
                {
                    byte[] buffer = new byte[32768];
                    int leidos;
                    using (Stream cuerpoRespuesta = respuesta.GetResponseStream())
                    {
                        while ((leidos = cuerpoRespuesta.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            EscribirTexto(salida, leidos.ToString("X", CultureInfo.InvariantCulture) + "\r\n");
                            salida.Write(buffer, 0, leidos);
                            EscribirTexto(salida, "\r\n");
                        }
                    }
                    EscribirTexto(salida, "0\r\n\r\n");
                }
                salida.Flush();
            }
        }

        private void ReenviarWebSocket(Peticion peticion, Uri uri, byte[] cuerpo, Stream cliente)
        {
            using (var upstreamClient = new TcpClient())
            {
                upstreamClient.Connect(uri.DnsSafeHost, uri.Port);
                Stream upstream = upstreamClient.GetStream();
                SslStream upstreamTls = null;
                if (uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                {
                    upstreamTls = new SslStream(upstream, false);
                    upstreamTls.AuthenticateAsClient(uri.DnsSafeHost);
                    upstream = upstreamTls;
                }

                string target = String.IsNullOrEmpty(uri.PathAndQuery) ? "/" : uri.PathAndQuery;
                var requestText = new StringBuilder()
                    .Append(peticion.Metodo).Append(' ').Append(target).Append(" HTTP/1.1\r\n")
                    .Append("Host: ").Append(uri.IsDefaultPort ? uri.Host : uri.Authority).Append("\r\n");
                foreach (KeyValuePair<string, string> header in peticion.Cabeceras)
                {
                    if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                        header.Key.Equals("Proxy-Connection", StringComparison.OrdinalIgnoreCase) ||
                        header.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase)) continue;
                    requestText.Append(header.Key).Append(": ").Append(header.Value).Append("\r\n");
                }
                requestText.Append("Connection: Upgrade\r\n\r\n");
                EscribirTexto(upstream, requestText.ToString());
                if (cuerpo.Length > 0) upstream.Write(cuerpo, 0, cuerpo.Length);
                upstream.Flush();

                string response = LeerCabecera(upstream);
                if (String.IsNullOrEmpty(response)) return;
                EscribirTexto(cliente, response);
                cliente.Flush();
                if (!response.StartsWith("HTTP/1.1 101", StringComparison.OrdinalIgnoreCase) &&
                    !response.StartsWith("HTTP/1.0 101", StringComparison.OrdinalIgnoreCase))
                {
                    byte[] buffer = new byte[8192];
                    int read;
                    while ((read = upstream.Read(buffer, 0, buffer.Length)) > 0)
                        cliente.Write(buffer, 0, read);
                    cliente.Flush();
                    return;
                }

                Task toServer = Task.Run(() => CopiarHastaCerrar(cliente, upstream));
                Task toBrowser = Task.Run(() => CopiarHastaCerrar(upstream, cliente));
                Task.WhenAny(toServer, toBrowser).GetAwaiter().GetResult();
            }
        }

        private static void CopiarHastaCerrar(Stream source, Stream destination)
        {
            try
            {
                byte[] buffer = new byte[16384];
                int read;
                while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    destination.Write(buffer, 0, read);
                    destination.Flush();
                }
            }
            catch { }
        }

        private static void TunelTransparente(TcpClient cliente, Uri destino)
        {
            using (var upstreamClient = new TcpClient())
            {
                upstreamClient.Connect(destino.DnsSafeHost, destino.Port);
                Stream navegador = cliente.GetStream();
                Stream servidor = upstreamClient.GetStream();
                EscribirTexto(navegador, "HTTP/1.1 200 Connection Established\r\nProxy-Agent: Concentracion\r\n\r\n");
                navegador.Flush();

                Task haciaServidor = Task.Run(() => CopiarHastaCerrar(navegador, servidor));
                Task haciaNavegador = Task.Run(() => CopiarHastaCerrar(servidor, navegador));
                Task.WhenAny(haciaServidor, haciaNavegador).GetAwaiter().GetResult();
            }
        }

        private X509Certificate2 ObtenerCertificadoSitio(string host)
        {
            // El certificado TLS debe coincidir exactamente con el host solicitado.
            // La normalización de reglas quita "www.", pero aquí eso provoca rechazo por nombre distinto.
            host = NormalizarHostCertificado(host);
            lock (_sync)
            {
                X509Certificate2 certificado;
                if (_certificadosSitio.TryGetValue(host, out certificado)) return certificado;
                var parametros = new CspParameters(24)
                {
                    KeyContainerName = "ConcentracionWeb_" + Guid.NewGuid().ToString("N"),
                    KeyNumber = (int)KeyNumber.Signature,
                    Flags = CspProviderFlags.UseMachineKeyStore
                };
                var clave = new RSACryptoServiceProvider(2048, parametros);
                clave.PersistKeyInCsp = true;
                try
                {
                    var solicitud = new CertificateRequest("CN=" + host, clave, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    solicitud.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
                    solicitud.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));
                    var usos = new OidCollection();
                    usos.Add(new Oid("1.3.6.1.5.5.7.3.1"));
                    solicitud.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(usos, true));
                    var nombres = new SubjectAlternativeNameBuilder();
                    IPAddress ip;
                    if (IPAddress.TryParse(host, out ip)) nombres.AddIpAddress(ip); else nombres.AddDnsName(host);
                    solicitud.CertificateExtensions.Add(nombres.Build());
                    byte[] serial = new byte[16];
                    using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(serial);
                    using (X509Certificate2 certificadoPublico = solicitud.Create(_ca, DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddDays(2), serial))
                        certificado = certificadoPublico.CopyWithPrivateKey(clave);
                    _certificadosSitio.Add(host, certificado);
                    _clavesSitio.Add(host, clave);
                    return certificado;
                }
                catch
                {
                    clave.PersistKeyInCsp = false;
                    clave.Dispose();
                    throw;
                }
            }
        }

        private X509Certificate2 ObtenerOCrearCA()
        {
            using (var personal = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            using (var raices = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
            {
                personal.Open(OpenFlags.ReadWrite);
                raices.Open(OpenFlags.ReadWrite);
                X509Certificate2 existente = personal.Certificates
                    .Cast<X509Certificate2>()
                    .FirstOrDefault(cert => cert.Subject == RaizCertificados && cert.HasPrivateKey);
                if (existente != null)
                {
                    if (!raices.Certificates.Cast<X509Certificate2>().Any(cert => cert.Thumbprint == existente.Thumbprint))
                    {
                        AgregarRaizSinDialogo(StoreLocation.LocalMachine, existente);
                    }
                    return new X509Certificate2(existente);
                }

                using (RSA clave = RSA.Create(2048))
                {
                    var solicitud = new CertificateRequest(RaizCertificados, clave, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    solicitud.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
                    solicitud.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
                    solicitud.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(solicitud.PublicKey, false));
                    X509Certificate2 creado = solicitud.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddYears(5));
                    try
                    {
                        personal.Add(creado);
                        AgregarRaizSinDialogo(StoreLocation.LocalMachine, creado);
                        return new X509Certificate2(creado);
                    }
                    catch
                    {
                        foreach (X509Certificate2 instalado in personal.Certificates.Cast<X509Certificate2>()
                            .Where(cert => cert.Thumbprint == creado.Thumbprint).ToList())
                            EliminarCertificadoSinDialogo(instalado);
                        foreach (X509Certificate2 instalado in raices.Certificates.Cast<X509Certificate2>()
                            .Where(cert => cert.Thumbprint == creado.Thumbprint).ToList())
                            EliminarCertificadoSinDialogo(instalado);
                        throw;
                    }
                    finally { creado.Dispose(); }
                }
            }
        }

        private static void AgregarRaizSinDialogo(StoreLocation ubicacion, X509Certificate2 certificado)
        {
            const uint almacenEquipo = 0x00020000;
            const uint almacenUsuario = 0x00010000;
            IntPtr almacen = CertOpenStore(new IntPtr(10), 0, IntPtr.Zero,
                ubicacion == StoreLocation.LocalMachine ? almacenEquipo : almacenUsuario, "ROOT");
            if (almacen == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error(),
                    "No se pudo abrir el almacén de certificados de confianza.");
            try
            {
                if (!CertAddCertificateContextToStore(almacen, certificado.Handle, 3, IntPtr.Zero))
                    throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error(),
                        "No se pudo agregar el certificado de confianza.");
            }
            finally { CertCloseStore(almacen, 0); }
        }

        private void RetirarCA()
        {
            foreach (StoreLocation ubicacion in new[] { StoreLocation.LocalMachine, StoreLocation.CurrentUser })
            {
                foreach (StoreName almacen in new[] { StoreName.My, StoreName.Root })
                {
                    try
                    {
                        using (var store = new X509Store(almacen, ubicacion))
                        {
                            store.Open(OpenFlags.ReadWrite);
                            foreach (X509Certificate2 certificado in store.Certificates.Cast<X509Certificate2>()
                                .Where(cert => cert.Subject == RaizCertificados).ToList())
                                EliminarCertificadoSinDialogo(certificado);
                        }
                    }
                    catch (Exception ex) { Trace.TraceWarning("No se pudo retirar la CA local: " + ex.Message); }
                }
            }
            if (_ca != null) { _ca.Dispose(); _ca = null; }
        }

        private static void RecuperarEstadoAnterior()
        {
            RestaurarConfiguracionProxy();
            foreach (StoreLocation ubicacion in new[] { StoreLocation.LocalMachine, StoreLocation.CurrentUser })
            {
                foreach (StoreName almacen in new[] { StoreName.My, StoreName.Root })
                {
                    try
                    {
                        using (var store = new X509Store(almacen, ubicacion))
                        {
                            store.Open(OpenFlags.ReadWrite);
                            foreach (X509Certificate2 certificado in store.Certificates.Cast<X509Certificate2>()
                                .Where(cert => cert.Subject == RaizCertificados).ToList())
                                EliminarCertificadoSinDialogo(certificado);
                        }
                    }
                    catch (Exception ex) { Trace.TraceWarning("No se pudo limpiar el certificado proxy anterior: " + ex.Message); }
                }
            }
        }

        private static void EliminarCertificadoSinDialogo(X509Certificate2 certificado)
        {
            IntPtr contexto = CertDuplicateCertificateContext(certificado.Handle);
            if (contexto == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error(),
                    "No se pudo preparar la eliminación del certificado.");
            if (!CertDeleteCertificateFromStore(contexto))
                throw new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error(),
                    "No se pudo retirar el certificado del almacén de Windows.");
        }

        private static void GuardarConfiguracionProxyAnterior()
        {
            using (RegistryKey internet = Registry.CurrentUser.OpenSubKey(RutaInternetSettings, false))
            using (RegistryKey copia = Registry.CurrentUser.CreateSubKey(RutaCopiaProxy))
            {
                if (internet == null || copia == null) throw new InvalidOperationException("No se pudo guardar la configuración de proxy actual.");
                foreach (string nombre in ValoresProxy)
                {
                    object valor = internet.GetValue(nombre, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                    copia.SetValue(nombre + ".Existe", valor == null ? 0 : 1, RegistryValueKind.DWord);
                    if (valor != null)
                    {
                        RegistryValueKind kind = internet.GetValueKind(nombre);
                        copia.SetValue(nombre + ".Tipo", (int)kind, RegistryValueKind.DWord);
                        copia.SetValue(nombre + ".Valor", Convert.ToString(valor, CultureInfo.InvariantCulture), RegistryValueKind.String);
                    }
                }
            }
        }

        private static void ConfigurarProxy(int puerto)
        {
            using (RegistryKey internet = Registry.CurrentUser.CreateSubKey(RutaInternetSettings))
            {
                if (internet == null) throw new InvalidOperationException("No se pudo configurar el proxy del sistema.");
                internet.SetValue("ProxyEnable", 1, RegistryValueKind.DWord);
                internet.SetValue("ProxyServer", ProxyLoopback + ":" + puerto.ToString(CultureInfo.InvariantCulture), RegistryValueKind.String);
                internet.SetValue("ProxyOverride", String.Empty, RegistryValueKind.String);
                internet.DeleteValue("AutoConfigURL", false);
                internet.SetValue("AutoDetect", 0, RegistryValueKind.DWord);
            }
            NotificarProxy();
        }

        private static void RestaurarConfiguracionProxy()
        {
            try
            {
                using (RegistryKey copia = Registry.CurrentUser.OpenSubKey(RutaCopiaProxy, false))
                using (RegistryKey internet = Registry.CurrentUser.CreateSubKey(RutaInternetSettings))
                {
                    if (copia == null || internet == null) return;
                    foreach (string nombre in ValoresProxy)
                    {
                        if (Convert.ToInt32(copia.GetValue(nombre + ".Existe", 0), CultureInfo.InvariantCulture) == 0)
                        {
                            internet.DeleteValue(nombre, false);
                            continue;
                        }
                        var kind = (RegistryValueKind)Convert.ToInt32(copia.GetValue(nombre + ".Tipo", (int)RegistryValueKind.String), CultureInfo.InvariantCulture);
                        string valor = Convert.ToString(copia.GetValue(nombre + ".Valor", ""), CultureInfo.InvariantCulture);
                        object restored = kind == RegistryValueKind.DWord ? (object)Convert.ToInt32(valor, CultureInfo.InvariantCulture) : valor;
                        internet.SetValue(nombre, restored, kind);
                    }
                }
                using (RegistryKey copia = Registry.CurrentUser.OpenSubKey(RutaCopiaProxy, true))
                    if (copia != null) Registry.CurrentUser.DeleteSubKeyTree(RutaCopiaProxy, false);
                NotificarProxy();
            }
            catch (Exception ex) { Trace.TraceWarning("No se pudo restaurar el proxy anterior: " + ex.Message); }
        }

        private static void NotificarProxy()
        {
            InternetSetOption(IntPtr.Zero, 39, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, 37, IntPtr.Zero, 0);
        }

        private static string LeerCabecera(Stream stream)
        {
            var bytes = new List<byte>();
            int estado = 0;
            while (bytes.Count < 65536)
            {
                int valor = stream.ReadByte();
                if (valor < 0) return bytes.Count == 0 ? null : Encoding.ASCII.GetString(bytes.ToArray());
                bytes.Add((byte)valor);
                estado = estado == 0 && valor == 13 ? 1 : estado == 1 && valor == 10 ? 2 : estado == 2 && valor == 13 ? 3 : estado == 3 && valor == 10 ? 4 : valor == 13 ? 1 : 0;
                if (estado == 4) return Encoding.ASCII.GetString(bytes.ToArray());
            }
            throw new InvalidDataException("La cabecera HTTP excede el límite permitido.");
        }

        private static Peticion AnalizarPeticion(string raw)
        {
            string[] lineas = raw.Split(new[] { "\r\n" }, StringSplitOptions.None);
            string[] primera = lineas[0].Split(new[] { ' ' }, 3);
            if (primera.Length < 3) throw new InvalidDataException("Línea HTTP inválida.");
            var request = new Peticion { Metodo = primera[0], Destino = primera[1], Version = primera[2] };
            for (int i = 1; i < lineas.Length; i++)
            {
                int dosPuntos = lineas[i].IndexOf(':');
                if (dosPuntos <= 0) continue;
                request.Cabeceras[lineas[i].Substring(0, dosPuntos).Trim()] = lineas[i].Substring(dosPuntos + 1).Trim();
            }
            return request;
        }

        private static byte[] LeerCuerpo(Stream stream, IDictionary<string, string> headers)
        {
            string lengthText;
            int length;
            if (headers.TryGetValue("Content-Length", out lengthText) && Int32.TryParse(lengthText, out length))
            {
                if (length < 0 || length > 64 * 1024 * 1024) throw new InvalidDataException("El cuerpo HTTP excede el límite permitido.");
                byte[] data = new byte[length];
                LeerExacto(stream, data, length);
                return data;
            }
            string transfer;
            if (headers.TryGetValue("Transfer-Encoding", out transfer) && transfer.IndexOf("chunked", StringComparison.OrdinalIgnoreCase) >= 0)
                return LeerChunked(stream);
            return new byte[0];
        }

        private static byte[] LeerChunked(Stream stream)
        {
            using (var contenido = new MemoryStream())
            {
                while (true)
                {
                    string linea = LeerLinea(stream);
                    int semi = linea.IndexOf(';');
                    int cantidad = Int32.Parse((semi < 0 ? linea : linea.Substring(0, semi)).Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    if (cantidad == 0)
                    {
                        while (!String.IsNullOrEmpty(LeerLinea(stream))) { }
                        break;
                    }
                    if (contenido.Length + cantidad > 64 * 1024 * 1024) throw new InvalidDataException("El cuerpo HTTP excede el límite permitido.");
                    byte[] bloque = new byte[cantidad];
                    LeerExacto(stream, bloque, cantidad);
                    contenido.Write(bloque, 0, cantidad);
                    LeerLinea(stream);
                }
                return contenido.ToArray();
            }
        }

        private static string LeerLinea(Stream stream)
        {
            var bytes = new List<byte>();
            while (bytes.Count < 8192)
            {
                int value = stream.ReadByte();
                if (value < 0) throw new EndOfStreamException();
                if (value == 10) break;
                if (value != 13) bytes.Add((byte)value);
            }
            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        private static void LeerExacto(Stream stream, byte[] buffer, int length)
        {
            int total = 0;
            while (total < length)
            {
                int read = stream.Read(buffer, total, length - total);
                if (read <= 0) throw new EndOfStreamException();
                total += read;
            }
        }

        private static string ObtenerHost(Peticion peticion, string fallback)
        {
            string host;
            if (peticion.Cabeceras.TryGetValue("Host", out host) && !String.IsNullOrWhiteSpace(host))
                return host.Split(':')[0].Trim('[', ']');
            if (!String.IsNullOrEmpty(fallback)) return fallback;
            return new Uri(peticion.Destino).IdnHost;
        }

        private static Uri CrearUri(string host, string destino, bool https)
        {
            Uri uri;
            if (Uri.TryCreate(destino, UriKind.Absolute, out uri)) return uri;
            string path = String.IsNullOrWhiteSpace(destino) ? "/" : destino;
            if (!path.StartsWith("/", StringComparison.Ordinal)) path = "/" + path;
            return new Uri((https ? "https://" : "http://") + host + path);
        }

        private static string Valor(IDictionary<string, string> headers, string name)
        {
            string result;
            return headers.TryGetValue(name, out result) ? result : null;
        }

        private void EscribirBloqueo(Stream salida, string hostBloqueado)
        {
            string rutaHtml = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Aviso.html");
            string html;
            try
            {
                html = File.ReadAllText(rutaHtml, Encoding.UTF8);
            }
            catch
            {
                html = "<!doctype html><html lang=\"es\"><meta charset=\"utf-8\"><title>Web bloqueada</title><h1>Acción no permitida</h1><p>Esta página no está en la lista de webs permitidas para esta tarea.</p></html>";
            }

            string[] dominios;
            lock (_sync) dominios = _dominiosPermitidos.OrderBy(d => d, StringComparer.OrdinalIgnoreCase).ToArray();
            string elementos = String.Join("", dominios.Select(dominio =>
                "<li><a href=\"https://" + WebUtility.HtmlEncode(dominio) + "\">" +
                WebUtility.HtmlEncode(dominio) + "</a></li>"));
            if (String.IsNullOrEmpty(elementos))
                elementos = "<li>No hay páginas permitidas configuradas para esta tarea.</li>";

            html = html.Replace("<li data-placeholder=\"paginas\">La lista de páginas permitidas aparecerá aquí.</li>", elementos);
            if (!String.IsNullOrWhiteSpace(hostBloqueado))
                html = html.Replace("id=\"pagina-bloqueada\">Esta página</", "id=\"pagina-bloqueada\">" + WebUtility.HtmlEncode(hostBloqueado) + "</");

            string rutaImagen = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "src", "block2.png");
            try
            {
                if (File.Exists(rutaImagen))
                    html = html.Replace("src=\"src/block2.png\"", "src=\"data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(rutaImagen)) + "\"");
            }
            catch { }

            byte[] body = Encoding.UTF8.GetBytes(html);
            EscribirTexto(salida, "HTTP/1.1 403 Forbidden\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: " + body.Length + "\r\nConnection: close\r\n\r\n");
            salida.Write(body, 0, body.Length);
            salida.Flush();
        }

        private static void EscribirTexto(Stream stream, string value)
        {
            byte[] data = Encoding.ASCII.GetBytes(value);
            stream.Write(data, 0, data.Length);
        }

        private static string NormalizarHost(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return String.Empty;
            Uri uri;
            string host = Uri.TryCreate(value, UriKind.Absolute, out uri) ? uri.IdnHost : value;
            host = host.Trim().TrimEnd('.').ToLowerInvariant();
            if (host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)) host = host.Substring(4);
            return host;
        }

        private static string NormalizarHostCertificado(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return String.Empty;
            string host = value.Trim().Trim('[', ']').TrimEnd('.');
            IPAddress ip;
            if (IPAddress.TryParse(host, out ip)) return ip.ToString();
            try { return new IdnMapping().GetAscii(host).ToLowerInvariant(); }
            catch (ArgumentException) { return host.ToLowerInvariant(); }
        }

        private void ActualizarDominiosInterno(IEnumerable<string> dominios) { ActualizarDominios(dominios); }

        public void Detener()
        {
            Detener(false);
        }

        private void Detener(bool retirarCertificado)
        {
            lock (_sync)
            {
                bool activo = _listener != null || _cancelacion != null;
                if (!activo && !retirarCertificado) return;
                if (_cancelacion != null)
                {
                    _cancelacion.Cancel();
                    _cancelacion.Dispose();
                    _cancelacion = null;
                }
                if (_listener != null) { _listener.Stop(); _listener = null; }
                _aceptar = null;
                Puerto = 0;
                RestaurarConfiguracionProxy();
                foreach (X509Certificate2 cert in _certificadosSitio.Values) cert.Dispose();
                _certificadosSitio.Clear();
                foreach (RSACryptoServiceProvider clave in _clavesSitio.Values)
                {
                    try { clave.PersistKeyInCsp = false; clave.Dispose(); } catch { }
                }
                _clavesSitio.Clear();
                _dependenciasConocidas.Clear();
                _navegacionesExternasPermitidas.Clear();
                if (retirarCertificado) RetirarCA();
            }
        }

        public void Dispose()
        {
            lock (_sync) _disposed = true;
            Detener(true);
            AppDomain.CurrentDomain.ProcessExit -= ProcessExit;
        }

        private void ProcessExit(object sender, EventArgs e)
        {
            try { Detener(true); } catch { }
        }

        private sealed class Peticion
        {
            public string Metodo;
            public string Destino;
            public string Version;
            public Dictionary<string, string> Cabeceras = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        [System.Runtime.InteropServices.DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr handle, int option, IntPtr buffer, int bufferLength);

        [System.Runtime.InteropServices.DllImport("crypt32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool CertAddCertificateContextToStore(IntPtr store, IntPtr certificate, uint disposition, IntPtr addedCertificate);

        [System.Runtime.InteropServices.DllImport("crypt32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr CertOpenStore(IntPtr storeProvider, uint encodingType, IntPtr cryptographicProvider, uint flags, string storeName);

        [System.Runtime.InteropServices.DllImport("crypt32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool CertCloseStore(IntPtr store, uint flags);

        [System.Runtime.InteropServices.DllImport("crypt32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr CertDuplicateCertificateContext(IntPtr certificate);

        [System.Runtime.InteropServices.DllImport("crypt32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool CertDeleteCertificateFromStore(IntPtr certificate);
    }
}
