using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaEntidades;

namespace CapaPresentacion
{
    public partial class ucDia : UserControl
    {
        private bool mesActual = true;

        public DateTime Fecha { get; set; }
        public ucDia()
        {
            InitializeComponent();
            Tipografia.Aplicar(this);
            EnlazarSeleccion(this);
        }

        public bool Seleccionado { get; private set; }
        public event EventHandler SeleccionSolicitada;

        private void EnlazarSeleccion(Control control)
        {
            control.Click += (s, e) =>
                SeleccionSolicitada?.Invoke(this, EventArgs.Empty);

            foreach (Control hijo in control.Controls)
                EnlazarSeleccion(hijo);
        }

        public int NumeroDia
        {
            get { return int.Parse(lbldia.Text); }
            set { lbldia.Text = value.ToString(); }
        }

        public void CargarTareas(IEnumerable<E_Tarea> tareas)
        {
            listTareas.Items.Clear();

            foreach (E_Tarea tarea in tareas
                .OrderBy(t => t.FechaInicio.TimeOfDay)
                .ThenBy(t => t.Nombre))
            {
                listTareas.Items.Add(
                    tarea.FechaInicio.ToString("hh:mm tt") + " " + tarea.Nombre);
            }
        }

        public void ConfigurarDia(DateTime fecha, bool mesActual)
        {
            Fecha = fecha;
            this.mesActual = mesActual;
            lbldia.Text = fecha.Day.ToString();
            ActualizarColor();
        }

        private void ActualizarColor()
        {
            lbldia.BackColor = Color.Transparent;
            lbldia.ForeColor = Color.White;
            pnlDia.Back = Seleccionado
                ? Color.FromArgb(30, 165, 145)
                : Fecha.Date == DateTime.Today
                    ? Color.FromArgb(22, 119, 255)
                    : mesActual
                        ? Color.FromArgb(75, 119, 209)
                        : Color.FromArgb(82, 91, 110);
        }

        public void Seleccionar(bool seleccionado)
        {
            Seleccionado = seleccionado;
            ActualizarColor();
        }

        private void Lbldia_Click(object sender, EventArgs e)
        {

        }
    }
}
