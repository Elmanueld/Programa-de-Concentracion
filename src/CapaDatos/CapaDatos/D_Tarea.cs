using System;
using System.Collections.Generic;
using CapaEntidades;

namespace CapaDatos
{
    public class D_Tarea
    {
        public int InsertarTarea(E_Tarea tarea)
        {
            object id = D_Comun.Escalar("sp_InsertarTarea", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", tarea.IdUsuario);
                D_Comun.Texto(comando, "@Nombre", 70, tarea.Nombre);
                D_Comun.Fecha(comando, "@FechaInicio", tarea.FechaInicio);
                D_Comun.Fecha(comando, "@FechaFin", tarea.FechaFin);
                D_Comun.Booleano(comando, "@EsPlantilla", tarea.EsPlantilla);
            });
            return Convert.ToInt32(id);
        }

        public List<E_Tarea> BuscarTarea(int idUsuario, string buscar)
        {
            return D_Comun.Consultar("sp_BuscarTarea", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Texto(comando, "@Buscar", 70, buscar);
            }, lector => new E_Tarea
            {
                IdTarea = D_Comun.LeerEntero(lector, "IdTarea"),
                IdUsuario = idUsuario,
                Nombre = D_Comun.LeerTexto(lector, "Nombre"),
                FechaInicio = D_Comun.LeerFecha(lector, "FechaInicio"),
                FechaFin = D_Comun.LeerFecha(lector, "FechaFin"),
                Estado = D_Comun.LeerCaracter(lector, "Estado"),
                EsPlantilla = false
            });
        }

        public void ModificarTarea(E_Tarea tarea)
        {
            D_Comun.Ejecutar("sp_ModificarTarea", comando =>
            {
                D_Comun.Entero(comando, "@IdTarea", tarea.IdTarea);
                D_Comun.Entero(comando, "@IdUsuario", tarea.IdUsuario);
                D_Comun.Texto(comando, "@Nombre", 70, tarea.Nombre);
                D_Comun.Fecha(comando, "@FechaInicio", tarea.FechaInicio);
                D_Comun.Fecha(comando, "@FechaFin", tarea.FechaFin);
            });
        }

        public void EliminarTarea(int idUsuario, int idTarea)
        {
            D_Comun.Ejecutar("sp_EliminarTarea", comando => {
                D_Comun.Entero(comando, "@IdTarea", idTarea);
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
            });
        }

        public void CambiarEstadoTarea(int idUsuario, int idTarea, char estado)
        {
            D_Comun.Ejecutar("sp_CambiarEstadoTarea", comando =>
            {
                D_Comun.Entero(comando, "@IdTarea", idTarea);
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Caracter(comando, "@Estado", estado);
            });
        }

        public int? BuscarPlantilla(int idUsuario)
        {
            object valor = D_Comun.Escalar("sp_BuscarPlantilla", comando =>
                D_Comun.Entero(comando, "@IdUsuario", idUsuario));
            return D_Comun.EnteroNullable(valor);
        }
    }
}
