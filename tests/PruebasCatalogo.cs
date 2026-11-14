using System;
using System.Linq;
using CapaEntidades;
using CapaNegocio;

internal static partial class Program
{
    private static void ProbarAislamientoCatalogos(int propietario, int otroUsuario,
        Action<bool, string> comprobar, Action<Action, string> rechazar)
    {
        string identificador = "privacidad-" + Guid.NewGuid().ToString("N");
        string enlace = "https://example.com/privacidad/" + Guid.NewGuid().ToString("N");
        var aplicaciones = new N_Aplicacion();
        var webs = new N_Web();

        var appPropia = new E_Aplicacion
        {
            Nombre = "Catálogo privado compartido",
            Identificador = identificador
        };
        var appAjena = new E_Aplicacion
        {
            Nombre = appPropia.Nombre,
            Identificador = identificador
        };
        int idAppPropia = aplicaciones.InsertarAplicacion(propietario, appPropia);
        int idAppAjena = aplicaciones.InsertarAplicacion(otroUsuario, appAjena);
        comprobar(idAppPropia != idAppAjena,
            "Dos usuarios pueden guardar el mismo valor de aplicación en catálogos privados");
        comprobar(aplicaciones.BuscarAplicacion(propietario, identificador).Single().IdAplicacion == idAppPropia &&
            aplicaciones.BuscarAplicacion(otroUsuario, identificador).Single().IdAplicacion == idAppAjena,
            "La búsqueda de aplicaciones solo devuelve el registro del propietario");

        rechazar(() => aplicaciones.ModificarAplicacion(otroUsuario, new E_Aplicacion
        {
            IdAplicacion = idAppPropia,
            IdUsuario = otroUsuario,
            Nombre = "Modificación ajena",
            Identificador = identificador
        }), "SQL impide modificar una aplicación ajena");
        rechazar(() => aplicaciones.EliminarAplicacion(otroUsuario, idAppPropia),
            "SQL impide eliminar una aplicación ajena");

        var webPropia = new E_Web { Nombre = "Enlace privado compartido", Link = enlace };
        var webAjena = new E_Web { Nombre = webPropia.Nombre, Link = enlace };
        int idWebPropia = webs.InsertarWeb(propietario, webPropia);
        int idWebAjena = webs.InsertarWeb(otroUsuario, webAjena);
        comprobar(idWebPropia != idWebAjena,
            "Dos usuarios pueden guardar el mismo valor de web en catálogos privados");
        comprobar(webs.BuscarWeb(propietario, enlace).Single().IdWeb == idWebPropia &&
            webs.BuscarWeb(otroUsuario, enlace).Single().IdWeb == idWebAjena,
            "La búsqueda de webs solo devuelve el registro del propietario");

        rechazar(() => webs.ModificarWeb(otroUsuario, new E_Web
        {
            IdWeb = idWebPropia,
            IdUsuario = otroUsuario,
            Nombre = "Modificación ajena",
            Link = enlace
        }), "SQL impide modificar una web ajena");
        rechazar(() => webs.EliminarWeb(otroUsuario, idWebPropia),
            "SQL impide eliminar una web ajena");

        aplicaciones.EliminarAplicacion(propietario, idAppPropia);
        aplicaciones.EliminarAplicacion(otroUsuario, idAppAjena);
        webs.EliminarWeb(propietario, idWebPropia);
        webs.EliminarWeb(otroUsuario, idWebAjena);
    }
}
