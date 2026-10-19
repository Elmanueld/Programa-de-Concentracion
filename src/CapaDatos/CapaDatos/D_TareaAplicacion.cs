using System.Collections.Generic;
using CapaEntidades;

namespace CapaDatos
{
    public class D_TareaAplicacion
    {
        public void Insertar(int idTarea, int idAplicacion)
        {
            D_Comun.Ejecutar("sp_InsertarTareaAplicacion", comando =>
            {
                D_Comun.Entero(comando, "@IdTarea", idTarea);
                D_Comun.Entero(comando, "@IdAplicacion", idAplicacion);
            });
        }

        public List<E_TareaAplicacion> Buscar(int idUsuario, int idTarea)
        {
            return D_Comun.Consultar("sp_BuscarTareaAplicacion", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Entero(comando, "@IdTarea", idTarea);
            }, lector => new E_TareaAplicacion
            {
                IdTarea = D_Comun.LeerEntero(lector, "IdTarea"),
                IdAplicacion = D_Comun.LeerEntero(lector, "IdAplicacion"),
                NombreAplicacion = D_Comun.LeerTexto(lector, "Aplicacion"),
                NombreTarea = D_Comun.LeerTexto(lector, "Tarea")
            });
        }

        public void Eliminar(int idTarea, int idAplicacion)
        {
            D_Comun.Ejecutar("sp_EliminarTareaAplicacion", comando =>
            {
                D_Comun.Entero(comando, "@IdTarea", idTarea);
                D_Comun.Entero(comando, "@IdAplicacion", idAplicacion);
            });
        }
    }
}