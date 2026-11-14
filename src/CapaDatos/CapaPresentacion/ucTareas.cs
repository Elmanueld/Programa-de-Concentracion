using System;
using System.Windows.Forms;
using CapaEntidades;

namespace CapaPresentacion
{
    public partial class ucTareas : UserControl
    {
        public E_Tarea Tarea { get; private set; }

        public event EventHandler EliminarSolicitado;
        public event EventHandler EditarSolicitado;

        public ucTareas()
        {
            InitializeComponent();
            if (FormularioSuave.EnModoDiseno) return;
            btnBorrar.Click += btnBorrar_Click;
            btnEditar.Click += btnEditar_Click;
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
            btnEditar.Enabled = tarea.Estado == 'P';
            btnBorrar.Enabled = tarea.Estado == 'P';
            ActualizarApariencia();
        }

        internal void ActualizarApariencia()
        {
            if (Tarea != null && Tarea.Estado == 'X')
            {
                pnlTargetaTarea.Back = System.Drawing.Color.FromArgb(190, 45, 55);
                lblTarea.ForeColor = System.Drawing.Color.White;
                lblHoraInicio.ForeColor = System.Drawing.Color.White;
                lblHoraFin.ForeColor = System.Drawing.Color.White;
                lblTarea.Text = Tarea.Nombre + "  ·  DETENIDA";
            }
            else if (Tarea != null && Tarea.Estado == 'C')
            {
                pnlTargetaTarea.Back = System.Drawing.Color.FromArgb(46, 160, 85);
                lblTarea.ForeColor = System.Drawing.Color.White;
                lblHoraInicio.ForeColor = System.Drawing.Color.White;
                lblHoraFin.ForeColor = System.Drawing.Color.White;
                lblTarea.Text = Tarea.Nombre + "  ·  COMPLETADA";
            }
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            EliminarSolicitado?.Invoke(this, EventArgs.Empty);
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            EditarSolicitado?.Invoke(this, EventArgs.Empty);
        }
    }
}
