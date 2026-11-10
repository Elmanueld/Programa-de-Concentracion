using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{
    public partial class frmAviso : Form
    {
        public frmAviso()
        {
            InitializeComponent();
            Tipografia.Aplicar(this);
            ArrastreFormulario.Habilitar(this, pnlTop);
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        public void ConfigurarContexto(
        string nombreAplicacion,
        string nombreTarea)
        {
            lblContexto.Text =
                "Intentaste abrir " + nombreAplicacion +
                ", pero no está permitida durante \"" +
                nombreTarea +
                "\". Puedes usarla durante el próximo descanso " +
                "o al terminar la tarea.";
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
