using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class N_TareaAplicacion
    {
        private readonly D_TareaAplicacion Datos = new D_TareaAplicacion();

        public void Insertar(int idTarea, int idAplicacion)
        {
            ValidarId(idTarea, "tarea");
            ValidarId(idAplicacion, "aplicación");
            Datos.Insertar(idTarea, idAplicacion);
        }

        public List<E_TareaAplicacion> Buscar(int idUsuario, int idTarea)
        {
            ValidarId(idUsuario, "usuario");
            ValidarId(idTarea, "tarea");
            return Datos.Buscar(idUsuario, idTarea);
        }

        public void Eliminar(int idTarea, int idAplicacion)
        {
            ValidarId(idTarea, "tarea");
            ValidarId(idAplicacion, "aplicación");
            Datos.Eliminar(idTarea, idAplicacion);
        }

        private static void ValidarId(int id, string tipo)
        {
            if (id <= 0) throw new ArgumentException("El ID de " + tipo + " no es válido.");
        }
    }
}