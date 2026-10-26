using System;
using System.Data.SqlClient;
using System.Diagnostics;
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
            catch (ArgumentException ex)
            {
                Mostrar(ventana, ex.Message);
            }
            catch (SqlException ex)
            {
                Trace.TraceError(ex.ToString());
                string mensaje;
                if (ex.Number == 2601 || ex.Number == 2627)
                    mensaje = "Ya existe un registro con esos datos. Si está registrándose, use otro correo.";
                else if (ex.Number >= 50001 && ex.Number <= 50004)
                    mensaje = ex.Message;
                else
                    mensaje = "No se pudo completar la operación en SQL Server. Revise el servicio, la instancia, la conexión y la instalación de los scripts.";
                Mostrar(ventana, mensaje);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                Mostrar(ventana, "No se pudo completar la operación. Revise la configuración y los detalles en la salida de depuración.");
            }
            finally
            {
                ventana.UseWaitCursor = false;
                ventana.Enabled = true;
            }
            return false;
        }

        private static void Mostrar(Form ventana, string mensaje)
        {
            MessageBox.Show(ventana, mensaje, "Programa de Concentración", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
