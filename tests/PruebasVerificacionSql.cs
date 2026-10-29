using System;
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
            Sql(@"ALTER PROCEDURE dbo.sp_IniciarSesion @Correo VARCHAR(254) AS
                SELECT IdUsuario, Correo, ContrasenaHash FROM Usuarios
                WHERE Correo=@Correo AND Eliminado=0 AND CorreoVerificado=1", correo);
            var usuario = new N_Usuario().IniciarSesion(correo, clave);
            Comprobar(usuario.Nombre == "Prueba" && usuario.ContrasenaHash == null,
                "Login con procedimiento anterior recupera Nombre y no expone el hash");
        }
        finally
        {
            Sql(Regex.Replace(original, @"(?i)^\s*CREATE(?:\s+OR\s+ALTER)?\s+PROCEDURE", "ALTER PROCEDURE"), correo);
        }
    }
}
