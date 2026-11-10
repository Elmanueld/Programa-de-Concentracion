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
    public partial class frmDescanso : Form
    {
        private readonly System.Windows.Forms.Timer temporizador =
            new System.Windows.Forms.Timer();

        private DateTime finDescanso;
        private int descansosRestantes;
        private string nombreTarea;

        public frmDescanso()
        {
            InitializeComponent();
            Tipografia.Aplicar(this);

            temporizador.Interval = 1000;
            temporizador.Tick += (s, e) => ActualizarContexto();

            btnCerrar.Click += (s, e) => Close();
            FormClosed += (s, e) => temporizador.Stop();

            ArrastreFormulario.Habilitar(this, pnlTop);
        }

        public void IniciarCuentaRegresiva(
            string tarea,
            DateTime fechaFinDescanso,
            int cantidadDescansosRestantes)
        {
            nombreTarea = tarea;
            finDescanso = fechaFinDescanso;
            descansosRestantes = cantidadDescansosRestantes;

            ActualizarContexto();
            temporizador.Start();
        }

        private void ActualizarContexto()
        {
            TimeSpan restante = finDescanso - DateTime.Now;

            if (restante <= TimeSpan.Zero)
            {
                temporizador.Stop();
                Close();
                return;
            }

            string tiempo = String.Format(
                "{0:00}:{1:00}",
                (int)restante.TotalMinutes,
                restante.Seconds);

            string textoDescansos;

            if (descansosRestantes == 1)
            {
                textoDescansos =
                    "Este es el último descanso de esta tarea.";
            }
            else
            {
                textoDescansos =
                    "Te quedan " + descansosRestantes +
                    " descansos, contando este.";
            }

            lblContexto.Text =
                "Puedes tomarte un descanso. Tiempo restante: " +
                tiempo +
                ".\r\n" +
                textoDescansos +
                "\r\nTarea: " +
                nombreTarea;
        }
    }
}
