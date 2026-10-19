using System.Collections.Generic;
using CapaEntidades;

namespace CapaDatos
{
    public class D_TareaWeb
    {
        public void Insertar(int idTarea, int idWeb)
        {
            D_Comun.Ejecutar("sp_InsertarTareaWeb", comando =>
            {
                D_Comun.Entero(comando, "@IdTarea", idTarea);
                D_Comun.Entero(comando, "@IdWeb", idWeb);
            });
        }

        public List<E_TareaWeb> Buscar(int idUsuario, int idTarea)
        {
            return D_Comun.Consultar("sp_BuscarTareaWeb", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Entero(comando, "@IdTarea", idTarea);
            }, lector => new E_TareaWeb
            {
                IdTarea = D_Comun.LeerEntero(lector, "IdTarea"),
                IdWeb = D_Comun.LeerEntero(lector, "IdWeb"),
                NombreWeb = D_Comun.LeerTexto(lector, "Web"),
                NombreTarea = D_Comun.LeerTexto(lector, "Tarea")
            });
        }

        public void Eliminar(int idTarea, int idWeb)
        {
            D_Comun.Ejecutar("sp_EliminarTareaWeb", comando =>
            {
                D_Comun.Entero(comando, "@IdTarea", idTarea);
                D_Comun.Entero(comando, "@IdWeb", idWeb);
            });
        }
    }
}