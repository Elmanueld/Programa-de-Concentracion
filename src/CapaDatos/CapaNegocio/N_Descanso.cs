using System;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class N_Descanso
    {
        private readonly D_Descanso Datos = new D_Descanso();

        public void InsertarDescanso(int idUsuario, E_Descanso descanso)
        {
            ValidarUsuario(idUsuario);
            ValidarDescanso(descanso, false);
            Datos.InsertarDescanso(idUsuario, descanso);
        }

        public E_Descanso BuscarDescanso(int idUsuario, int idTarea)
        {
            ValidarUsuario(idUsuario);
            ValidarId(idTarea, "tarea");
            return Datos.BuscarDescanso(idUsuario, idTarea);
        }

        public void ModificarDescanso(int idUsuario, E_Descanso descanso)
        {
            ValidarUsuario(idUsuario);
            ValidarDescanso(descanso, true);
            Datos.ModificarDescanso(idUsuario, descanso);
        }

        public void EliminarDescanso(int idUsuario, int idDescanso)
        {
            ValidarUsuario(idUsuario);
            ValidarId(idDescanso, "descanso");
            Datos.EliminarDescanso(idUsuario, idDescanso);
        }

        private static void ValidarDescanso(E_Descanso descanso, bool requiereId)
        {
            if (descanso == null) throw new ArgumentNullException("descanso");
            if (requiereId) ValidarId(descanso.IdDescanso, "descanso");
            else ValidarId(descanso.IdTarea, "tarea");

            bool ambosNulos = !descanso.IntervaloDescanso.HasValue && !descanso.DuracionDescanso.HasValue;
            bool ambosDefinidos = descanso.IntervaloDescanso.HasValue && descanso.DuracionDescanso.HasValue;
            if (!ambosNulos && !ambosDefinidos)
                throw new ArgumentException("El intervalo y la duración deben estar definidos juntos o ambos quedar vacíos.");

            if (ambosDefinidos &&
                (descanso.IntervaloDescanso.Value <= 0 || descanso.DuracionDescanso.Value <= 0))
                throw new ArgumentException("El intervalo y la duración deben ser mayores que cero.");
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
