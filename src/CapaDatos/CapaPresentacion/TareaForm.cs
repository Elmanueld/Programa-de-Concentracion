using System;
using System.Drawing;
using System.Windows.Forms;
using CapaEntidades;
using CapaNegocio;

namespace CapaPresentacion
{
    public sealed class TareaForm : Form
    {
        private readonly TextBox nombre = new TextBox { Dock = DockStyle.Fill, MaxLength = 70 };
        private readonly DateTimePicker inicio = Fecha();
        private readonly DateTimePicker fin = Fecha();

        public TareaForm(int idUsuario, E_Tarea tarea)
        {
            Text = tarea == null ? "Crear tarea" : "Editar tarea";
            ClientSize = new Size(510, 270);
            Font = new Font("Segoe UI", 10);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2, RowCount = 4 };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.Controls.Add(new Label { Text = "Nombre", AutoSize = true }, 0, 0);
            panel.Controls.Add(nombre, 1, 0);
            panel.Controls.Add(new Label { Text = "Inicio", AutoSize = true }, 0, 1);
            panel.Controls.Add(inicio, 1, 1);
            panel.Controls.Add(new Label { Text = "Fin", AutoSize = true }, 0, 2);
            panel.Controls.Add(fin, 1, 2);
            inicio.Value = tarea == null ? DateTime.Now.AddMinutes(5) : tarea.FechaInicio;
            fin.Value = tarea == null ? DateTime.Now.AddMinutes(35) : tarea.FechaFin;
            nombre.Text = tarea == null ? "" : tarea.Nombre;
            var botones = new FlowLayoutPanel { AutoSize = true };
            var guardar = new Button { Text = "Guardar", AutoSize = true };
            var cancelar = new Button { Text = "Cancelar", AutoSize = true, DialogResult = DialogResult.Cancel };
            guardar.Click += (s, e) =>
            {
                if (OperacionUI.Ejecutar(this, () =>
                {
                    var valor = new E_Tarea { IdUsuario = idUsuario, IdTarea = tarea == null ? 0 : tarea.IdTarea,
                        Nombre = nombre.Text.Trim(), FechaInicio = inicio.Value, FechaFin = fin.Value, EsPlantilla = false };
                    var servicio = new N_Tarea();
                    if (tarea == null)
                        servicio.InsertarTarea(valor);
                    else
                        servicio.ModificarTarea(valor);
                }))
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            };
            botones.Controls.Add(guardar);
            botones.Controls.Add(cancelar);
            panel.Controls.Add(botones, 1, 3);
            Controls.Add(panel);
            AcceptButton = guardar;
            CancelButton = cancelar;
        }

        private static DateTimePicker Fecha()
        {
            return new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy HH:mm", MinDate = new DateTime(1753, 1, 1) };
        }
    }
}
