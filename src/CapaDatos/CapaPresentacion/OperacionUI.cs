using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{
    internal static class OperacionUI
    {
        public static bool Ejecutar(Form ventana, Action accion)
        {
            ventana.UseWaitCursor = true;
            ventana.Enabled = false;
            try
            {
                accion();
                return true;
            }
            catch (Exception ex)
            {
                MostrarError(ventana, ex);
                return false;
            }
            finally
            {
                ventana.UseWaitCursor = false;
                ventana.Enabled = true;
            }
        }

        public static async Task<bool> EjecutarAsync(Form ventana, Func<Task> accion)
        {
            ventana.UseWaitCursor = true;
            ventana.Enabled = false;
            try
            {
                await accion();
                return true;
            }
            catch (Exception ex)
            {
                MostrarError(ventana, ex);
                return false;
            }
            finally
            {
                if (!ventana.IsDisposed)
                {
                    ventana.UseWaitCursor = false;
                    ventana.Enabled = true;
                }
            }
        }

        private static void MostrarError(Form ventana, Exception ex)
        {
            Trace.TraceError(ex.ToString());
            string mensaje;
            var sql = ex as SqlException;
            if (ex is ArgumentException || ex is InvalidOperationException)
                mensaje = ex.Message;
            else if (ex is SmtpException)
                mensaje = "No se pudo enviar el correo. La cuenta sigue pendiente. Revise SMTP y espere un minuto antes de reenviar.";
            else if (sql != null && (sql.Number == 2601 || sql.Number == 2627))
                mensaje = "Ya existe un registro con esos datos. Si está registrándose, use otro correo.";
            else if (sql != null && sql.Number >= 50001 && sql.Number <= 50006)
                mensaje = sql.Message;
            else if (sql != null)
                mensaje = "No se pudo completar la operación en SQL Server. Revise el servicio, la instancia, la conexión y la instalación de los scripts.";
            else
                mensaje = "No se pudo completar la operación. Revise la configuración y los detalles en la salida de depuración.";
            MessageBox.Show(ventana, mensaje, "Programa de Concentración", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
