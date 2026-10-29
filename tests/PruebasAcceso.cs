using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CapaEntidades;
using CapaNegocio;

internal static class PruebasAcceso
{
    public static void Ejecutar(Action<bool, string> comprobar, Action<Action, string> rechazar)
    {
        var tabla = new DataTable();
        tabla.Columns.Add("IdUsuario", typeof(int));
        tabla.Columns.Add("Correo");
        tabla.Columns.Add("ContrasenaHash");
        tabla.Rows.Add(7, "cuenta@example.com", "hash-de-prueba");
        Type datos = Type.GetType("CapaDatos.D_Usuario, CapaDatos", true);
        MethodInfo leer = datos.GetMethod("LeerSesion", BindingFlags.Static | BindingFlags.NonPublic);
        using (var lector = tabla.CreateDataReader())
        {
            lector.Read();
            var usuario = (E_Usuario)leer.Invoke(null, new object[] { lector });
            comprobar(usuario.IdUsuario == 7 && usuario.Nombre == "" && usuario.ContrasenaHash == "hash-de-prueba",
                "Login admite el resultado SQL anterior sin Nombre");
        }
        tabla.Columns.Add("Nombre");
        tabla.Rows[0]["Nombre"] = "Ana";
        using (var lector = tabla.CreateDataReader())
        {
            lector.Read();
            var usuario = (E_Usuario)leer.Invoke(null, new object[] { lector });
            comprobar(usuario.Nombre == "Ana", "Login conserva Nombre si SQL lo devuelve");
        }
        var vistos = new HashSet<string>();
        bool formato = true;
        for (int i = 0; i < 100; i++)
        {
            string codigo = (string)InvocarNegocio(typeof(N_VerificacionCorreo), "CrearCodigo");
            formato = formato && Regex.IsMatch(codigo, "^[0-9]{6}$");
            vistos.Add(codigo);
        }
        comprobar(formato && vistos.Count > 1, "Códigos aleatorios de seis dígitos");
        string hash = Hash("sal-a", "314159");
        comprobar(hash.Length == 64 && hash == Hash("sal-a", "314159") && hash != Hash("sal-b", "314159"),
            "El código se resume con una sal y no se guarda directamente");
        var servicio = new N_VerificacionCorreo();
        rechazar(() => servicio.ConfirmarCodigo("cuenta@example.com", "123"), "Código corto rechazado antes de SQL");
        rechazar(() => servicio.ConfirmarCodigo("cuenta@example.com", "abcdef"), "Código no numérico rechazado antes de SQL");
        using (var config = new ConfiguracionSmtpPruebas(2525))
        {
            Environment.SetEnvironmentVariable("PC_SMTPHOST", "smtp.example.com");
            bool rechazoTls = false;
            try { InvocarCorreo("ValidarConfiguracion"); }
            catch (InvalidOperationException) { rechazoTls = true; }
            comprobar(rechazoTls, "SMTP remoto sin TLS rechazado sin enviar mensajes");
            Environment.SetEnvironmentVariable("PC_SMTPHOST", "");
            bool rechazoConfiguracion = false;
            try { InvocarCorreo("ValidarConfiguracion"); }
            catch (InvalidOperationException) { rechazoConfiguracion = true; }
            comprobar(rechazoConfiguracion, "Falta de SMTP informa error");
        }
        string recibido = RecibirCorreo(() => InvocarCorreo("Enviar", "buzon@example.com", "314159"));
        comprobar(recibido.Contains("314159") && recibido.Contains("10 minutos"), "SMTP local recibió el código y su vigencia");
        var puertoCerrado = new TcpListener(IPAddress.Loopback, 0);
        puertoCerrado.Start();
        int puerto = ((IPEndPoint)puertoCerrado.LocalEndpoint).Port;
        puertoCerrado.Stop();
        using (var config = new ConfiguracionSmtpPruebas(puerto))
        {
            bool fallo = false;
            try { InvocarCorreo("Enviar", "buzon@example.com", "314159"); }
            catch (SmtpException) { fallo = true; }
            comprobar(fallo, "Fallo de conexión SMTP no se oculta");
        }
    }

    public static void ConConfiguracionSmtp(Action accion)
    {
        using (var config = new ConfiguracionSmtpPruebas(2525)) accion();
    }

    public static string Hash(string sal, string codigo)
    {
        return (string)InvocarNegocio(typeof(N_VerificacionCorreo), "CalcularHash", sal, codigo);
    }

    public static string RecibirCodigo(Action enviar)
    {
        string cuerpo = RecibirCorreo(enviar);
        var codigo = Regex.Match(cuerpo, "\\b[0-9]{6}\\b");
        if (!codigo.Success) throw new Exception("El correo no contiene el código esperado.");
        return codigo.Value;
    }

    private static object InvocarNegocio(Type tipo, string metodo, params object[] argumentos)
    {
        try
        {
            return tipo.GetMethod(metodo, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .Invoke(null, argumentos);
        }
        catch (TargetInvocationException ex) { throw ex.InnerException; }
    }

    private static object InvocarCorreo(string metodo, params object[] argumentos)
    {
        return InvocarNegocio(typeof(N_VerificacionCorreo).Assembly.GetType("CapaNegocio.CorreoVerificacion"), metodo, argumentos);
    }

    private static string RecibirCorreo(Action enviar)
    {
        var servidor = new TcpListener(IPAddress.Loopback, 0);
        servidor.Start();
        int puerto = ((IPEndPoint)servidor.LocalEndpoint).Port;
        try
        {
            var recepcion = Task.Run(() =>
            {
                using (var cliente = servidor.AcceptTcpClient())
                using (var flujo = cliente.GetStream())
                using (var lector = new StreamReader(flujo, Encoding.ASCII))
                using (var escritor = new StreamWriter(flujo, Encoding.ASCII) { AutoFlush = true, NewLine = "\r\n" })
                {
                    cliente.ReceiveTimeout = 10000;
                    cliente.SendTimeout = 10000;
                    escritor.WriteLine("220 localhost pruebas");
                    string linea;
                    var mensaje = new StringBuilder();
                    while ((linea = lector.ReadLine()) != null)
                    {
                        if (linea.StartsWith("DATA", StringComparison.OrdinalIgnoreCase))
                        {
                            escritor.WriteLine("354 Envie el mensaje");
                            while ((linea = lector.ReadLine()) != null && linea != ".") mensaje.AppendLine(linea);
                            escritor.WriteLine("250 Recibido");
                        }
                        else if (linea.StartsWith("QUIT", StringComparison.OrdinalIgnoreCase))
                        {
                            escritor.WriteLine("221 Fin");
                            break;
                        }
                        else escritor.WriteLine("250 OK");
                    }
                    string texto = mensaje.ToString().Replace("\r\n", "\n");
                    int inicio = texto.IndexOf("\n\n", StringComparison.Ordinal);
                    string cuerpo = inicio >= 0 ? texto.Substring(inicio + 2) : texto;
                    if (texto.IndexOf("Content-Transfer-Encoding: base64", StringComparison.OrdinalIgnoreCase) >= 0)
                        cuerpo = Encoding.UTF8.GetString(Convert.FromBase64String(cuerpo));
                    return cuerpo;
                }
            });
            using (var config = new ConfiguracionSmtpPruebas(puerto)) enviar();
            if (!recepcion.Wait(15000)) throw new Exception("El servidor SMTP local no terminó.");
            return recepcion.Result;
        }
        finally { servidor.Stop(); }
    }

    private sealed class ConfiguracionSmtpPruebas : IDisposable
    {
        private readonly string[] claves = { "PC_SMTPHOST", "PC_SMTPPUERTO", "PC_SMTPSSL", "PC_SMTPREMITENTE", "PC_SMTPUSUARIO", "PC_SMTPCLAVE" };
        private readonly string[] anteriores = new string[6];

        public ConfiguracionSmtpPruebas(int puerto)
        {
            string[] valores = { "127.0.0.1", puerto.ToString(), "false", "pruebas@example.com", "", "" };
            for (int i = 0; i < claves.Length; i++)
            {
                anteriores[i] = Environment.GetEnvironmentVariable(claves[i]);
                Environment.SetEnvironmentVariable(claves[i], valores[i]);
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < claves.Length; i++) Environment.SetEnvironmentVariable(claves[i], anteriores[i]);
        }
    }
}
