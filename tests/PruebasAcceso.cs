using System;
using System.Data;
using System.Reflection;
using CapaDatos;
using CapaEntidades;
using CapaNegocio;

internal static class PruebasAcceso
{
    public static void Ejecutar(Action<bool, string> comprobar)
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
                "compatibilidad con respuesta de sesión SQL anterior sin Nombre");
        }

        var verificacion = new N_VerificacionCorreo();
        Exception errorCorreo = verificacion.SolicitarCodigo("no-es-correo");
        comprobar(errorCorreo is ArgumentException,
            "correo inválido rechazado antes de contactar la API");
        Exception errorCodigo = verificacion.ConfirmarCodigo("cuenta@example.com", "abc123");
        comprobar(errorCodigo is ArgumentException,
            "código con caracteres no numéricos rechazado antes de contactar la API");
    }
}