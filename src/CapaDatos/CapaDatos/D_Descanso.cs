using System.Collections.Generic;
using CapaEntidades;

namespace CapaDatos
{
    public class D_Descanso
    {
        public void InsertarDescanso(E_Descanso descanso)
        {
            D_Comun.Ejecutar("sp_InsertarDescanso", comando =>
            {
                D_Comun.Entero(comando, "@IdTarea", descanso.IdTarea);
                D_Comun.EnteroNulo(comando, "@IntervaloDescanso", descanso.IntervaloDescanso);
                D_Comun.EnteroNulo(comando, "@DuracionDescanso", descanso.DuracionDescanso);
            });
        }

        public E_Descanso BuscarDescanso(int idTarea)
        {
            List<E_Descanso> resultados = D_Comun.Consultar("sp_BuscarDescanso", comando =>
                D_Comun.Entero(comando, "@IdTarea", idTarea), lector => new E_Descanso
                {
                    IdDescanso = D_Comun.LeerEntero(lector, "IdDescanso"),
                    IdTarea = D_Comun.LeerEntero(lector, "IdTarea"),
                    IntervaloDescanso = D_Comun.EnteroNullable(lector["IntervaloDescanso"]),
                    DuracionDescanso = D_Comun.EnteroNullable(lector["DuracionDescanso"])
                });

            return resultados.Count == 0 ? null : resultados[0];
        }

        public void ModificarDescanso(E_Descanso descanso)
        {
            D_Comun.Ejecutar("sp_ModificarDescanso", comando =>
            {
                D_Comun.Entero(comando, "@IdDescanso", descanso.IdDescanso);
                D_Comun.EnteroNulo(comando, "@IntervaloDescanso", descanso.IntervaloDescanso);
                D_Comun.EnteroNulo(comando, "@DuracionDescanso", descanso.DuracionDescanso);
            });
        }

        public void EliminarDescanso(int idDescanso)
        {
            D_Comun.Ejecutar("sp_EliminarDescanso", comando =>
                D_Comun.Entero(comando, "@IdDescanso", idDescanso));
        }
    }
}