using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class N_TareaAplicacion
    {
        private readonly D_TareaAplicacion Datos = new D_TareaAplicacion();

        public void Insertar(int idUsuario, int idTarea, int idAplicacion)
        {
            ValidarUsuario(idUsuario);
            ValidarId(idTarea, "tarea");
            ValidarId(idAplicacion, "aplicación");
            Datos.Insertar(idUsuario, idTarea, idAplicacion);
        }

        public List<E_TareaAplicacion> Buscar(int idUsuario, int idTarea)
        {
            ValidarUsuario(idUsuario);
            ValidarId(idTarea, "tarea");
            return Datos.Buscar(idUsuario, idTarea);
        }

        public void Eliminar(int idUsuario, int idTarea, int idAplicacion)
        {
            ValidarUsuario(idUsuario);
            ValidarId(idTarea, "tarea");
            ValidarId(idAplicacion, "aplicación");
            Datos.Eliminar(idUsuario, idTarea, idAplicacion);
        }

        private static void ValidarId(int id, string tipo)
        {
            if (id == 0) throw new ArgumentException("El ID de " + tipo + " no es válido.");
        }

        private static void ValidarUsuario(int idUsuario)
        {
            if (idUsuario <= 0) throw new ArgumentException("El ID de usuario no es válido.");
        }
    }
}
