using System.Collections.Generic;
using CapaEntidades;

namespace CapaDatos
{
    public class D_TareaAplicacion
    {
        public void Insertar(int idUsuario, int idTarea, int idAplicacion)
        {
            D_SincronizacionOffline.ConFallbackEscritura(idUsuario, () => D_Comun.Ejecutar("sp_InsertarTareaAplicacion", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Entero(comando, "@IdTarea", idTarea);
                D_Comun.Entero(comando, "@IdAplicacion", idAplicacion);
            }), () => D_SincronizacionOffline.InsertarTareaAplicacion(idUsuario, idTarea, idAplicacion));
        }

        public List<E_TareaAplicacion> Buscar(int idUsuario, int idTarea)
        {
            return D_SincronizacionOffline.ConFallback(idUsuario, () => D_Comun.Consultar("sp_BuscarTareaAplicacion", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Entero(comando, "@IdTarea", idTarea);
            }, lector => new E_TareaAplicacion
            {
                IdTarea = D_Comun.LeerEntero(lector, "IdTarea"),
                IdAplicacion = D_Comun.LeerEntero(lector, "IdAplicacion"),
                NombreAplicacion = D_Comun.LeerTexto(lector, "Aplicacion"),
                NombreTarea = D_Comun.LeerTexto(lector, "Tarea")
            }), () => D_SincronizacionOffline.BuscarTareaAplicacion(idUsuario, idTarea));
        }

        public void Eliminar(int idUsuario, int idTarea, int idAplicacion)
        {
            D_SincronizacionOffline.ConFallbackEscritura(idUsuario, () => D_Comun.Ejecutar("sp_EliminarTareaAplicacion", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Entero(comando, "@IdTarea", idTarea);
                D_Comun.Entero(comando, "@IdAplicacion", idAplicacion);
            }), () => D_SincronizacionOffline.EliminarTareaAplicacion(idUsuario, idTarea, idAplicacion));
        }
    }
}
