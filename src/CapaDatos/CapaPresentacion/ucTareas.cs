using System;
using System.Windows.Forms;
using CapaEntidades;

namespace CapaPresentacion
{
    public partial class ucTareas : UserControl
    {
        public E_Tarea Tarea { get; private set; }

        public event EventHandler EliminarSolicitado;

        public ucTareas()
        {
            InitializeComponent();
            Tipografia.Aplicar(this);
            btnBorrar.Click += btnBorrar_Click;
        }

        public void CargarTarea(E_Tarea tarea)
        {
            if (tarea == null)
                throw new ArgumentNullException("tarea");

            Tarea = tarea;

            lblTarea.Text = tarea.Nombre;
            lblHoraInicio.Text =
                "Hora de inicio " + tarea.FechaInicio.ToString("hh:mm tt");
            lblHoraFin.Text =
                "Hora de fin " + tarea.FechaFin.ToString("hh:mm tt");
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            EliminarSolicitado?.Invoke(this, EventArgs.Empty);
        }
    }
}
