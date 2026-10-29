using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
            EjecutarCambio("sp_ModificarTarea", tarea.IdUsuario, tarea.IdTarea, comando =>
            {
                D_Comun.Entero(comando, "@IdTarea", tarea.IdTarea);
                D_Comun.Texto(comando, "@Nombre", 70, tarea.Nombre);
                D_Comun.Fecha(comando, "@FechaInicio", tarea.FechaInicio);
                D_Comun.Fecha(comando, "@FechaFin", tarea.FechaFin);
            });
        }

        public void EliminarTarea(int idUsuario, int idTarea)
        {
            EjecutarCambio("sp_EliminarTarea", idUsuario, idTarea, comando => {
                D_Comun.Entero(comando, "@IdTarea", idTarea);
            });
        }

        public void CambiarEstadoTarea(int idUsuario, int idTarea, char estado)
        {
            EjecutarCambio("sp_CambiarEstadoTarea", idUsuario, idTarea, comando =>
            {
                D_Comun.Entero(comando, "@IdTarea", idTarea);
                D_Comun.Caracter(comando, "@Estado", estado);
            });
        }

        public int? BuscarPlantilla(int idUsuario)
        {
            object valor = D_Comun.Escalar("sp_BuscarPlantilla", comando =>
                D_Comun.Entero(comando, "@IdUsuario", idUsuario));
            return D_Comun.EnteroNullable(valor);
        }
        private void EjecutarCambio(string procedimiento, int idUsuario, int idTarea, Action<SqlCommand> parametros)
        {
            using (var cn = ConexionDB.Instancia.CrearConexion())
            {
                cn.Open();
                using (var transaccion = cn.BeginTransaction())
                {
                    // Conservamos el propietario bloqueado hasta terminar la operación.
                    using (var consulta = new SqlCommand(@"SELECT Estado FROM Tareas WITH (UPDLOCK, HOLDLOCK)
                        WHERE IdTarea=@IdTarea AND IdUsuario=@IdUsuario AND EsPlantilla=0
                        AND EXISTS (SELECT 1 FROM Usuarios WHERE IdUsuario=@IdUsuario AND Eliminado=0)", cn, transaccion))
                    {
                        D_Comun.Entero(consulta, "@IdTarea", idTarea);
                        D_Comun.Entero(consulta, "@IdUsuario", idUsuario);
                        object estado = consulta.ExecuteScalar();
                        if (estado == null || estado == DBNull.Value)
                            throw new ArgumentException("La tarea no existe o no pertenece al usuario.");
                        if (procedimiento != "sp_CambiarEstadoTarea" && Convert.ToString(estado) == "A")
                            throw new ArgumentException("Una tarea activa no se puede editar ni eliminar.");
                        bool recibeUsuario;
                        using (var revision = new SqlCommand(@"SELECT COUNT(*) FROM sys.parameters
                            WHERE object_id=OBJECT_ID(@Procedimiento) AND name='@IdUsuario'", cn, transaccion))
                        {
                            D_Comun.Texto(revision, "@Procedimiento", 128, "dbo." + procedimiento);
                            recibeUsuario = Convert.ToInt32(revision.ExecuteScalar()) > 0;
                        }
                        if (!recibeUsuario && procedimiento == "sp_ModificarTarea" && Convert.ToString(estado) != "P")
                            throw new ArgumentException("El procedimiento original solo permite editar tareas programadas.");
                        using (var cmd = new SqlCommand(procedimiento, cn, transaccion))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            parametros(cmd);
                            if (procedimiento == "sp_CambiarEstadoTarea")
                            {
                                string siguiente = Convert.ToString(cmd.Parameters["@Estado"].Value);
                                string actual = Convert.ToString(estado);
                                if (!((actual == "P" && siguiente == "A") || (actual == "A" && siguiente == "C")))
                                    throw new ArgumentException("La transición de estado no es válida.");
                            }
                            if (recibeUsuario) D_Comun.Entero(cmd, "@IdUsuario", idUsuario);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    transaccion.Commit();
                }
            }
        }

    }
}
