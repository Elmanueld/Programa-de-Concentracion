using System;
using System.Collections.Generic;
using CapaEntidades;

namespace CapaDatos
{
    public class D_Descanso
    {
        public void InsertarDescanso(int idUsuario, E_Descanso descanso)
        {
            if (String.IsNullOrEmpty(descanso.SyncId)) descanso.SyncId = Guid.NewGuid().ToString("D");
            D_SincronizacionOffline.ConFallbackEscritura(idUsuario, () => D_Comun.Ejecutar("sp_InsertarDescanso", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                comando.Parameters.Add("@SyncId", System.Data.SqlDbType.UniqueIdentifier).Value = Guid.Parse(descanso.SyncId);
                D_Comun.Entero(comando, "@IdTarea", descanso.IdTarea);
                D_Comun.EnteroNulo(comando, "@IntervaloDescanso", descanso.IntervaloDescanso);
                D_Comun.EnteroNulo(comando, "@DuracionDescanso", descanso.DuracionDescanso);
            }), () => D_SincronizacionOffline.InsertarDescanso(idUsuario, descanso));
        }

        public E_Descanso BuscarDescanso(int idUsuario, int idTarea)
        {
            return D_SincronizacionOffline.ConFallback(idUsuario, () =>
            {
                List<E_Descanso> resultados = D_Comun.Consultar("sp_BuscarDescanso", comando =>
                {
                    D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                    D_Comun.Entero(comando, "@IdTarea", idTarea);
                }, lector => new E_Descanso
                    {
                        IdDescanso = D_Comun.LeerEntero(lector, "IdDescanso"),
                        IdTarea = D_Comun.LeerEntero(lector, "IdTarea"),
                        IntervaloDescanso = D_Comun.EnteroNullable(lector["IntervaloDescanso"]),
                        DuracionDescanso = D_Comun.EnteroNullable(lector["DuracionDescanso"])
                    });

                return resultados.Count == 0 ? null : resultados[0];
            }, () => D_SincronizacionOffline.BuscarDescanso(idUsuario, idTarea));
        }

        public void ModificarDescanso(int idUsuario, E_Descanso descanso)
        {
            D_SincronizacionOffline.ConFallbackEscritura(idUsuario, () => D_Comun.Ejecutar("sp_ModificarDescanso", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Entero(comando, "@IdDescanso", descanso.IdDescanso);
                D_Comun.EnteroNulo(comando, "@IntervaloDescanso", descanso.IntervaloDescanso);
                D_Comun.EnteroNulo(comando, "@DuracionDescanso", descanso.DuracionDescanso);
            }), () => D_SincronizacionOffline.ModificarDescanso(idUsuario, descanso));
        }

        public void EliminarDescanso(int idUsuario, int idDescanso)
        {
            D_SincronizacionOffline.ConFallbackEscritura(idUsuario, () => D_Comun.Ejecutar("sp_EliminarDescanso", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Entero(comando, "@IdDescanso", idDescanso);
            }), () => D_SincronizacionOffline.EliminarDescanso(idUsuario, idDescanso));
        }
    }
}
