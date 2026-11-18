using System;
using System.Linq;
using CapaEntidades;
using CapaNegocio;

internal static class Program
{
    private static int comprobaciones;

    private static void Comprobar(bool condicion, string nombre)
    {
        if (!condicion) throw new InvalidOperationException("Prueba fallida: " + nombre);
        comprobaciones++;
        Console.WriteLine("OK " + nombre);
    }

    private static void Rechazar(Action accion, string nombre)
    {
        bool rechazo = false;
        try { accion(); }
        catch (ArgumentException) { rechazo = true; }
        Comprobar(rechazo, nombre);
    }

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Contains("--integracion") || args.Contains("--correo-real"))
        {
            Console.Error.WriteLine("Esas pruebas antiguas usaban LocalDB/SMTP directo y fueron retiradas. Esta suite no se conecta a bases ni envía correos reales.");
            return 2;
        }

        try
        {
            const string clave = "PruebaSegura-2026";
            string hash = SeguridadContrasena.CrearHash(clave);
            Comprobar(hash != clave && hash.Length <= 255, "hash PBKDF2 no contiene la contraseña original");
            Comprobar(hash != SeguridadContrasena.CrearHash(clave), "cada hash utiliza una sal aleatoria");
            Comprobar(SeguridadContrasena.Verificar(clave, hash), "contraseña correcta aceptada");
            Comprobar(!SeguridadContrasena.Verificar("Incorrecta", hash), "contraseña incorrecta rechazada");
            Comprobar(!SeguridadContrasena.Verificar(clave, "PBKDF2-SHA256$600000$no$hash"), "hash malformado rechazado");
            Rechazar(() => SeguridadContrasena.CrearHash("corta"), "contraseña demasiado corta rechazada");

            var usuarios = new N_Usuario();
            Rechazar(() => usuarios.RegistrarUsuario("", "x@y.com", clave), "nombre obligatorio");
            Rechazar(() => usuarios.RegistrarUsuario("Ana", "Ana <x@y.com>", clave), "correo inválido rechazado");

            var tareas = new N_Tarea();
            Rechazar(() => tareas.InsertarTarea(new E_Tarea
            {
                IdUsuario = 1, Nombre = "T", FechaInicio = DateTime.Today, FechaFin = DateTime.Today
            }), "intervalo de tarea inválido rechazado");
            Rechazar(() => tareas.EliminarTarea(0, 1), "usuario inválido rechazado");

            PruebasAcceso.Ejecutar(Comprobar);
            if (args.Contains("--interfaz")) PruebasInterfaz.Ejecutar(Comprobar);

            Console.WriteLine("Las pruebas de SQL remoto y SMTP real requieren un entorno aislado y no se ejecutan desde esta suite.");
            Console.WriteLine("Comprobaciones correctas: " + comprobaciones);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }
}