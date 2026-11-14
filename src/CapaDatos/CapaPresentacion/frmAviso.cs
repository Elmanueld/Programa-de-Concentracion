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
    public partial class frmAviso : FormularioSuave
    {
        public frmAviso()
        {
            InitializeComponent();
            if (EnModoDiseno) return;
            IconosGoogle.Aplicar(pictureBox1, "block");
            IconosGoogle.Aplicar(btnCerrar, "close");
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

        public void ConfigurarRecordatorioTarea(string nombreTarea, DateTime inicio, int minutos)
        {
            Text = "Tarea próxima";
            lblTexto.Text = "Tu tarea empieza pronto";
            lblContexto.Text = "\"" + (nombreTarea ?? "Tarea") + "\" comienza a las " +
                inicio.ToString("HH:mm") + ". Faltan aproximadamente " + minutos + " minutos.";
            IconosGoogle.Aplicar(pictureBox1, "notifications_active");
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
