using System;
using System.Configuration;
using System.IO;
using CapaDatos;
using CapaEntidades;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using CapaNegocio;

internal static partial class Program
{
    private static void ProbarVerificacion(string correo)
    {
        var servicio = new N_VerificacionCorreo();
        string codigo = PruebasAcceso.RecibirCodigo(() => servicio.SolicitarCodigo(correo));
        string almacenado = Convert.ToString(Sql("SELECT CodigoHash FROM CodigosVerificacion WHERE Correo=@Correo", correo));
        Comprobar(almacenado.Length == 64 && almacenado != codigo, "SQL guarda el resumen del código, sin texto directo");
        Rechazar(() => PruebasAcceso.ConConfiguracionSmtp(() => servicio.SolicitarCodigo(correo)), "Reenvío inmediato bloqueado");
        Sql("UPDATE CodigosVerificacion SET ExpiraUtc=DATEADD(MINUTE,-1,SYSUTCDATETIME()) WHERE Correo=@Correo", correo);
        Rechazar(() => servicio.ConfirmarCodigo(correo, codigo), "Código vencido rechazado");
        Sql("UPDATE CodigosVerificacion SET UltimoEnvioUtc=DATEADD(MINUTE,-2,SYSUTCDATETIME()) WHERE Correo=@Correo", correo);
        codigo = PruebasAcceso.RecibirCodigo(() => servicio.SolicitarCodigo(correo));
        string incorrecto = ((Int32.Parse(codigo) + 1) % 1000000).ToString("D6");
        for (int i = 0; i < 5; i++)
            Rechazar(() => servicio.ConfirmarCodigo(correo, incorrecto), "Intento de código incorrecto " + (i + 1));
        Rechazar(() => servicio.ConfirmarCodigo(correo, codigo), "Cinco intentos bloquean incluso el código correcto");
        Comprobar(Convert.ToInt32(Sql("SELECT CorreoVerificado FROM Usuarios WHERE Correo=@Correo", correo)) == 0,
            "Una cuenta no se verifica con códigos inválidos");
        Sql("UPDATE CodigosVerificacion SET UltimoEnvioUtc=DATEADD(MINUTE,-2,SYSUTCDATETIME()) WHERE Correo=@Correo", correo);
        codigo = PruebasAcceso.RecibirCodigo(() => servicio.SolicitarCodigo(correo));
        servicio.ConfirmarCodigo(correo, codigo);
        Comprobar(Convert.ToInt32(Sql("SELECT CorreoVerificado FROM Usuarios WHERE Correo=@Correo", correo)) == 1,
            "Código recibido por SMTP verifica la cuenta en SQL");
        Comprobar(Convert.ToInt32(Sql("SELECT COUNT(*) FROM CodigosVerificacion WHERE Correo=@Correo", correo)) == 0,
            "El código usado se elimina");
        Rechazar(() => servicio.ConfirmarCodigo(correo, codigo), "Código de un solo uso");
    }

    private static void ProbarLoginAnterior(string correo, string clave)
    {
        string original = Convert.ToString(Sql("SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.sp_IniciarSesion'))", correo));
        try
        {
            CambiarProcedimiento(@"ALTER PROCEDURE dbo.sp_IniciarSesion @Correo VARCHAR(254) AS
                SELECT IdUsuario, Correo, ContrasenaHash FROM Usuarios
                WHERE Correo=@Correo AND Eliminado=0 AND CorreoVerificado=1");
            var usuario = new N_Usuario().IniciarSesion(correo, clave);
            Comprobar(usuario.Nombre == "Prueba" && usuario.ContrasenaHash == null,
                "Login con procedimiento anterior recupera Nombre y no expone el hash");
        }
        finally
        {
            CambiarProcedimiento(new Regex(@"^\s*CREATE(?:\s+OR\s+ALTER)?\s+PROCEDURE\b",
                RegexOptions.IgnoreCase | RegexOptions.Multiline).Replace(original, "ALTER PROCEDURE", 1));
        }
    }

    private static void CambiarProcedimiento(string texto)
    {
        using (var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["conexion"].ConnectionString))
        using (var cmd = new SqlCommand(texto, cn))
        {
            cn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    private static void ProbarActualizacionTareas()
    {
        string[] nombres = { "sp_IniciarSesion", "sp_ModificarTarea", "sp_EliminarTarea", "sp_CambiarEstadoTarea" };
        string[] originales = new string[nombres.Length];
        for (int i = 0; i < nombres.Length; i++)
            originales[i] = Convert.ToString(Sql("SELECT OBJECT_DEFINITION(OBJECT_ID('dbo." + nombres[i] + "'))", ""));
        try
        {
            CambiarProcedimiento(@"ALTER PROCEDURE sp_ModificarTarea
	@IdTarea INT,
	@Nombre VARCHAR(70),
	@FechaInicio DATETIME,
	@FechaFin DATETIME

AS
BEGIN
	UPDATE Tareas
	SET Nombre = @Nombre,
		FechaInicio = @FechaInicio,
		FechaFin = @FechaFin
	WHERE IdTarea = @IdTarea
	AND Estado = 'P';
END;");
            CambiarProcedimiento(@"ALTER PROCEDURE sp_EliminarTarea
	@IdTarea INT

AS
BEGIN
	DELETE FROM Tareas
	WHERE IdTarea = @IdTarea
	AND Estado <> 'A'
END;");
            var datos = new D_Tarea();
            ComprobarProcedimientoAnterior(() => datos.ModificarTarea(new E_Tarea {
                IdTarea = 0, IdUsuario = 0, Nombre = "Prueba", FechaInicio = DateTime.Today, FechaFin = DateTime.Today.AddHours(1)
            }), "Edición reproduce SQL 8144 con el procedimiento antiguo");
            ComprobarProcedimientoAnterior(() => datos.EliminarTarea(0, 0),
                "Eliminación reproduce SQL 8144 con el procedimiento antiguo");
            string script = File.ReadAllText(Path.Combine("database", "06_ActualizarEntregable3.sql"));
            foreach (string lote in Regex.Split(script, @"(?im)^\s*GO\s*$"))
                if (!String.IsNullOrWhiteSpace(lote)) CambiarProcedimiento(lote);
            foreach (string nombre in new[] { "sp_ModificarTarea", "sp_EliminarTarea", "sp_CambiarEstadoTarea" })
                Comprobar(Convert.ToInt32(Sql("SELECT COUNT(*) FROM sys.parameters WHERE object_id=OBJECT_ID('dbo." + nombre + "') AND name='@IdUsuario'", "")) == 1,
                    "Actualización 06 agrega el usuario a " + nombre);
        }
        catch
        {
            for (int i = 0; i < nombres.Length; i++)
                CambiarProcedimiento(new Regex(@"^\s*CREATE(?:\s+OR\s+ALTER)?\s+PROCEDURE\b",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline).Replace(originales[i], "ALTER PROCEDURE", 1));
            throw;
        }
    }

    private static void ComprobarProcedimientoAnterior(Action accion, string nombre)
    {
        bool rechazado = false;
        try { accion(); }
        catch (SqlException ex)
        {
            if (ex.Number != 8144) throw;
            rechazado = true;
        }
        Comprobar(rechazado, nombre);
    }

}
