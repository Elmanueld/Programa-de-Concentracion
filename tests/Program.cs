using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using CapaEntidades;
using CapaNegocio;

internal static partial class Program
{
    private static int comprobaciones;
    private static void Comprobar(bool condicion, string nombre)
    {
        if (!condicion) throw new Exception("FALLÓ: " + nombre);
        comprobaciones++;
        Console.WriteLine("CORRECTO: " + nombre);
    }
    private static void Rechazar(Action accion, string nombre)
    {
        bool rechazo = false;
        try
        {
            accion();
        }
        catch (ArgumentException)
        {
            rechazo = true;
        }
        catch (SqlException ex)
        {
            if (ex.Number >= 50001 && ex.Number <= 50006 || ex.Number == 2627 || ex.Number == 2601)
                rechazo = true;
            else
                throw;
        }
        Comprobar(rechazo, nombre);
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            const string clave = "PruebaSegura-2026";
            string hash = SeguridadContrasena.CrearHash(clave);
            Comprobar(hash != clave && hash.Length <= 255, "Formato y longitud del hash");
            Comprobar(hash != SeguridadContrasena.CrearHash(clave), "Sal aleatoria por contraseña");
            Comprobar(SeguridadContrasena.Verificar(clave, hash), "Contraseña correcta");
            Comprobar(!SeguridadContrasena.Verificar("Incorrecta", hash), "Contraseña incorrecta");
            Comprobar(!SeguridadContrasena.Verificar(clave, "PBKDF2-SHA256$600000$no$hash"), "Hash malformado");
            Rechazar(() => SeguridadContrasena.CrearHash("corta"), "Contraseña corta");
            var usuarios = new N_Usuario();
            Rechazar(() => usuarios.RegistrarUsuario("", "x@y.com", clave), "Nombre obligatorio");
            Rechazar(() => usuarios.RegistrarUsuario("Ana", "Ana <x@y.com>", clave), "Correo sin nombre de presentación");
            var tareas = new N_Tarea();
            Rechazar(() => tareas.InsertarTarea(new E_Tarea { IdUsuario = 1, Nombre = "T", FechaInicio = DateTime.Today, FechaFin = DateTime.Today }), "Fechas inválidas");
            Rechazar(() => tareas.EliminarTarea(0, 1), "Usuario inválido");
            PruebasAcceso.Ejecutar(Comprobar, Rechazar);
            if (args.Contains("--interfaz")) PruebasInterfaz.Ejecutar(Comprobar);
            if (args.Contains("--demostracion")) ProbarDemostracionOriginal(clave);
            else if (args.Contains("--integracion")) Integracion(clave);
            else Console.WriteLine("PENDIENTE: integración SQL; ejecutar con --integracion en la instancia exclusiva de pruebas.");
            Console.WriteLine("Total correcto: " + comprobaciones);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static object Sql(string texto, string correo, int id = 0)
    {
        using (var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["conexion"].ConnectionString))
        using (var cmd = new SqlCommand(texto, cn))
        {
            cmd.Parameters.AddWithValue("@Correo", correo);
            cmd.Parameters.AddWithValue("@Id", id);
            cn.Open();
            return cmd.ExecuteScalar();
        }
    }

    private static void Integracion(string clave)
    {
        // Usamos solo la instancia de pruebas para no tocar los datos del proyecto.
        var conexion = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["conexion"].ConnectionString);
        if (!String.Equals(conexion.DataSource, @"(localdb)\Entregable3Pruebas", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Use exclusivamente la instancia Entregable3Pruebas.");
        string correo = "prueba-" + Guid.NewGuid().ToString("N") + "@example.com";
        string otro = "otro-" + Guid.NewGuid().ToString("N") + "@example.com";
        var usuarios = new N_Usuario();
        var tareas = new N_Tarea();
        try
        {
            usuarios.RegistrarUsuario("Prueba", correo, clave);
            string almacenado = Convert.ToString(Sql("SELECT ContrasenaHash FROM Usuarios WHERE Correo=@Correo", correo));
            Comprobar(SeguridadContrasena.Verificar(clave, almacenado), "Registro persiste hash verificable en SQL");
            Rechazar(() => usuarios.RegistrarUsuario("Duplicado", correo, clave), "Correo duplicado");
            Rechazar(() => usuarios.IniciarSesion(correo, clave), "Cuenta pendiente no accede");
            string modoAnterior = Environment.GetEnvironmentVariable("PC_MODO_DEMOSTRACION");
            try
            {
                Environment.SetEnvironmentVariable("PC_MODO_DEMOSTRACION", "true");
                var demo = new N_Usuario().IniciarSesion(correo, clave);
                Comprobar(demo.IdUsuario > 0 && demo.ContrasenaHash == null, "Demostración permite login pendiente con contraseña correcta");
                Rechazar(() => new N_Usuario().IniciarSesion(correo, "Incorrecta"), "Demostración rechaza contraseña incorrecta");
                Comprobar(Convert.ToInt32(Sql("SELECT CorreoVerificado FROM Usuarios WHERE Correo=@Correo", correo)) == 0,
                    "Demostración no marca el correo como verificado");
            }
            finally { Environment.SetEnvironmentVariable("PC_MODO_DEMOSTRACION", modoAnterior); }
            Rechazar(() => new N_Usuario().IniciarSesion(correo, clave), "Modo normal sigue rechazando correo pendiente");
            ProbarVerificacion(correo);
            var usuario = usuarios.IniciarSesion(correo, clave);
            Comprobar(usuario.IdUsuario > 0 && usuario.ContrasenaHash == null, "Login correcto sin hash en sesión");
            Rechazar(() => usuarios.IniciarSesion(correo, "Incorrecta"), "Login incorrecto rechazado");
            ProbarLoginAnterior(correo, clave);
            ProbarProcedimientosOriginales(usuario.IdUsuario);
            usuarios.RegistrarUsuario("Otro", otro, clave);
            int idOtro = Convert.ToInt32(Sql("SELECT IdUsuario FROM Usuarios WHERE Correo=@Correo", otro));
            int id = tareas.InsertarTarea(new E_Tarea { IdUsuario = usuario.IdUsuario, Nombre = "Lectura", FechaInicio = DateTime.Today.AddHours(12), FechaFin = DateTime.Today.AddHours(13) });
            var tarea = tareas.BuscarTarea(usuario.IdUsuario, "").Single(x => x.IdTarea == id);
            Comprobar(tarea.Estado == 'P', "Crear y listar tarea persistida");
            Comprobar(!tareas.BuscarTarea(idOtro, "").Any(x => x.IdTarea == id), "Otro usuario no lista tarea ajena");
            Rechazar(() => tareas.EliminarTarea(idOtro, id), "SQL rechaza borrar tarea ajena");
            tarea.IdUsuario = idOtro;
            Rechazar(() => tareas.ModificarTarea(tarea), "SQL rechaza editar tarea ajena");
            tarea.IdUsuario = usuario.IdUsuario;
            tarea.Nombre = "Lectura editada";
            tareas.ModificarTarea(tarea);
            Comprobar(new N_Tarea().BuscarTarea(usuario.IdUsuario, "").Single(x => x.IdTarea == id).Nombre == tarea.Nombre, "Editar y recuperar en otra consulta");
            tareas.CambiarEstadoTarea(usuario.IdUsuario, id, 'A');
            Rechazar(() => tareas.ModificarTarea(tarea), "Tarea activa no editable");
            Rechazar(() => tareas.EliminarTarea(usuario.IdUsuario, id), "Tarea activa no eliminable");
            tareas.CambiarEstadoTarea(usuario.IdUsuario, id, 'C');
            tareas.ModificarTarea(tarea);
            Comprobar(tareas.BuscarTarea(usuario.IdUsuario, "").Single(x => x.IdTarea == id).Estado == 'C', "RF-006 permite editar completadas");
            tareas.EliminarTarea(usuario.IdUsuario, id);
            Comprobar(!new N_Tarea().BuscarTarea(usuario.IdUsuario, "").Any(x => x.IdTarea == id), "Eliminación persistente");
            Rechazar(() => tareas.EliminarTarea(usuario.IdUsuario, id), "Borrado inexistente informa error");
            usuarios.EliminarUsuario(usuario.IdUsuario);
            Rechazar(() => usuarios.IniciarSesion(correo, clave), "Cuenta eliminada no accede");
        }
        finally
        {
            foreach (string email in new[] { correo, otro })
                Sql("DELETE FROM Tareas WHERE IdUsuario IN (SELECT IdUsuario FROM Usuarios WHERE Correo=@Correo); DELETE FROM Usuarios WHERE Correo=@Correo;", email);
        }
    }

    private static void ProbarDemostracionOriginal(string clave)
    {
        var destino = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["conexion"].ConnectionString);
        if (!String.Equals(destino.DataSource, @"(localdb)\Entregable3Pruebas", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Use exclusivamente la instancia Entregable3Pruebas.");
        string correo = "demo-" + Guid.NewGuid().ToString("N") + "@example.com";
        string otro = "otro-" + Guid.NewGuid().ToString("N") + "@example.com";
        string anterior = Environment.GetEnvironmentVariable("PC_MODO_DEMOSTRACION");
        try
        {
            Environment.SetEnvironmentVariable("PC_MODO_DEMOSTRACION", "true");
            var usuarios = new N_Usuario();
            usuarios.RegistrarUsuario("Demostración", correo, clave);
            var usuario = usuarios.IniciarSesion(correo, clave);
            Comprobar(usuario.IdUsuario > 0 && usuario.ContrasenaHash == null, "Original: registro y login sin SMTP");
            Rechazar(() => usuarios.IniciarSesion(correo, "Incorrecta"), "Original: contraseña incorrecta rechazada");
            Comprobar(Convert.ToInt32(Sql("SELECT CorreoVerificado FROM Usuarios WHERE Correo=@Correo", correo)) == 0, "Original: correo permanece sin verificar");
            usuarios.RegistrarUsuario("Segundo", otro, clave);
            var segundo = usuarios.IniciarSesion(otro, clave);
            var tareas = new N_Tarea();
            int id = tareas.InsertarTarea(new E_Tarea { IdUsuario=usuario.IdUsuario, Nombre="Demostración", FechaInicio=DateTime.Today, FechaFin=DateTime.Today.AddHours(1) });
            var tarea = tareas.BuscarTarea(usuario.IdUsuario, "").Single(t => t.IdTarea == id);
            Comprobar(tarea.Estado == 'P', "Original: crear y consultar tarea");
            Comprobar(!tareas.BuscarTarea(segundo.IdUsuario, "").Any(t => t.IdTarea == id), "Original: listado aislado por usuario");
            Rechazar(() => tareas.EliminarTarea(segundo.IdUsuario, id), "Original: no elimina tarea ajena");
            tarea.IdUsuario=segundo.IdUsuario;
            Rechazar(() => tareas.ModificarTarea(tarea), "Original: no edita tarea ajena");
            tarea.IdUsuario=usuario.IdUsuario;
            tarea.Nombre="Editada";
            tareas.ModificarTarea(tarea);
            Comprobar(new N_Tarea().BuscarTarea(usuario.IdUsuario, "").Single(t => t.IdTarea == id).Nombre == "Editada", "Original: edición persistente");
            tareas.EliminarTarea(usuario.IdUsuario, id);
            Comprobar(!tareas.BuscarTarea(usuario.IdUsuario, "").Any(t => t.IdTarea == id), "Original: eliminación persistente");
            Comprobar(new N_Usuario().IniciarSesion(correo, clave).IdUsuario == usuario.IdUsuario, "Original: segundo inicio de sesión");
            usuarios.EliminarUsuario(usuario.IdUsuario);
            Rechazar(() => new N_Usuario().IniciarSesion(correo, clave), "Original: cuenta eliminada no accede en demostración");
            Environment.SetEnvironmentVariable("PC_MODO_DEMOSTRACION", "false");
            Rechazar(() => new N_Usuario().IniciarSesion(otro, clave), "Original: modo normal conserva verificación obligatoria");
        }
        finally
        {
            Environment.SetEnvironmentVariable("PC_MODO_DEMOSTRACION", anterior);
            foreach (string email in new[] { correo, otro })
                Sql("DELETE FROM Tareas WHERE IdUsuario IN (SELECT IdUsuario FROM Usuarios WHERE Correo=@Correo); DELETE FROM Usuarios WHERE Correo=@Correo;", email);
        }
    }
}
