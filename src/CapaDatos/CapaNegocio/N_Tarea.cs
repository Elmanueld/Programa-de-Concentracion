using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class N_Tarea
    {
        private readonly D_Tarea Datos = new D_Tarea();

        public int InsertarTarea(E_Tarea tarea)
        {
            ValidarContenido(tarea);
            if (tarea.IdUsuario <= 0)
                throw new ArgumentException("Debe seleccionar un usuario.");
            return Datos.InsertarTarea(tarea);
        }

        public List<E_Tarea> BuscarTarea(int idUsuario, string buscar)
        {
            ValidarId(idUsuario, "usuario");
            return Datos.BuscarTarea(idUsuario, buscar ?? String.Empty);
        }

        public void ModificarTarea(E_Tarea tarea)
        {
            if (tarea == null) throw new ArgumentNullException("tarea");
            ValidarId(tarea.IdTarea, "tarea");
            ValidarId(tarea.IdUsuario, "usuario");
            ValidarContenido(tarea);
            Datos.ModificarTarea(tarea);
        }

        public void EliminarTarea(int idUsuario, int idTarea)
        {
            ValidarId(idUsuario, "usuario");
            ValidarId(idTarea, "tarea");
            Datos.EliminarTarea(idUsuario, idTarea);
        }

        public void CambiarEstadoTarea(int idUsuario, int idTarea, char estado)
        {
            ValidarId(idUsuario, "usuario");
            ValidarId(idTarea, "tarea");
            if (estado != 'A' && estado != 'C')
                throw new ArgumentException("El nuevo estado debe ser A (activa) o C (completa).");
            Datos.CambiarEstadoTarea(idUsuario, idTarea, estado);
        }

        public int? BuscarPlantilla(int idUsuario)
        {
            ValidarId(idUsuario, "usuario");
            return Datos.BuscarPlantilla(idUsuario);
        }

        private static void ValidarContenido(E_Tarea tarea)
        {
            if (tarea == null) throw new ArgumentNullException("tarea");
            if (String.IsNullOrWhiteSpace(tarea.Nombre))
                throw new ArgumentException("El nombre de la tarea es obligatorio.");
            if (tarea.Nombre.Length > 70)
                throw new ArgumentException("El nombre de la tarea no puede exceder 70 caracteres.");
            if (tarea.FechaInicio >= tarea.FechaFin)
                throw new ArgumentException("La fecha de inicio debe ser anterior a la fecha de fin.");
            if (tarea.FechaInicio < new DateTime(1753, 1, 1) || tarea.FechaFin > new DateTime(9999, 12, 31, 23, 59, 59))
                throw new ArgumentException("Las fechas exceden el rango admitido por SQL Server.");
        }

        private static void ValidarId(int id, string tipo)
        {
            if (id <= 0) throw new ArgumentException("El ID de " + tipo + " no es válido.");
        }
    }
}
