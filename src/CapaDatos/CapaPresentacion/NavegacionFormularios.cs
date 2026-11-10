using System;
using System.Windows.Forms;

namespace CapaPresentacion
{
    /// <summary>
    /// Muestra un formulario modal sin dejar visible el formulario anterior y
    /// lo restaura al cerrar el formulario actual.
    /// </summary>
    internal static class NavegacionFormularios
    {
        public static DialogResult MostrarModal(
            Form anterior,
            Form actual)
        {
            if (actual == null)
                throw new ArgumentNullException("actual");

            bool restaurarAnterior = anterior != null &&
                !anterior.IsDisposed && anterior.Visible;

            if (restaurarAnterior)
                anterior.Hide();

            try
            {
                return actual.ShowDialog();
            }
            finally
            {
                if (restaurarAnterior &&
                    anterior != null &&
                    !anterior.IsDisposed)
                {
                    anterior.Show();
                    anterior.Activate();
                }
            }
        }
    }
}
