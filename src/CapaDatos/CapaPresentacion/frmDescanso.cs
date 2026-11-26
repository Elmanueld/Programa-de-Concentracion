using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaNegocio;

namespace CapaPresentacion
{
    public partial class frmDescanso : FormularioSuave
    {
        private readonly System.Windows.Forms.Timer temporizador;

        private DateTime finDescanso;
        private int descansosRestantes;
        private string nombreTarea;
        private bool descansoFinalizado;

        public frmDescanso()
        {
            InitializeComponent();
            if (EnModoDiseno) return;
            temporizador = new System.Windows.Forms.Timer();
            IconosGoogle.Aplicar(pictureBox1, "favorite");
            IconosGoogle.Aplicar(btnCerrar, "close");
            temporizador.Interval = 1000;
            temporizador.Tick += Temporizador_Tick;

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
            descansoFinalizado = false;
            lblTexto.Text = "Es hora del descanso";

            ActualizarContexto();
            temporizador.Start();
        }

        private void ActualizarContexto()
        {
            TimeSpan restante = finDescanso - RelojNegocio.Ahora;

            if (restante <= TimeSpan.Zero)
            {
                temporizador.Stop();
                descansoFinalizado = true;
                lblTexto.Text = "Descanso terminado";
                lblContexto.Text = "El descanso de \"" + (nombreTarea ?? "Tarea") +
                    "\" terminó. Ya puedes continuar con tu tarea.";
                return;
            }

            if (descansoFinalizado) return;

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

        private void Temporizador_Tick(object sender, EventArgs e)
        {
            try { ActualizarContexto(); }
            catch (Exception ex)
            {
                temporizador.Stop();
                OperacionUI.ReportarError(this, ex, "No se pudo actualizar la cuenta del descanso");
            }
        }
    }
}
