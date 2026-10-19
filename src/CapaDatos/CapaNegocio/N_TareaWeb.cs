using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class N_TareaWeb
    {
        private readonly D_TareaWeb Datos = new D_TareaWeb();

        public void Insertar(int idTarea, int idWeb)
        {
            ValidarId(idTarea, "tarea");
            ValidarId(idWeb, "web");
            Datos.Insertar(idTarea, idWeb);
        }

        public List<E_TareaWeb> Buscar(int idUsuario, int idTarea)
        {
            ValidarId(idUsuario, "usuario");
            ValidarId(idTarea, "tarea");
            return Datos.Buscar(idUsuario, idTarea);
        }

        public void Eliminar(int idTarea, int idWeb)
        {
            ValidarId(idTarea, "tarea");
            ValidarId(idWeb, "web");
            Datos.Eliminar(idTarea, idWeb);
        }

        private static void ValidarId(int id, string tipo)
        {
            if (id <= 0) throw new ArgumentException("El ID de " + tipo + " no es válido.");
        }
    }
}