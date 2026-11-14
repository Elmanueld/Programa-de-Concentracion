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
        private PreferenciasApariencia apariencia = PreferenciasApariencia.PorDefecto;

        public DateTime Fecha { get; set; }
        public ucDia()
        {
            InitializeComponent();
            if (FormularioSuave.EnModoDiseno) return;
            TemaAplicacion.Preparar(this);
            listTareas.DrawMode = DrawMode.OwnerDrawFixed;
            listTareas.SelectionMode = SelectionMode.None;
            listTareas.BorderStyle = BorderStyle.None;
            listTareas.DrawItem += ListTareas_DrawItem;
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
            listTareas.BeginUpdate();
            try
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
            finally { listTareas.EndUpdate(); }
        }

        public void ConfigurarDia(DateTime fecha, bool mesActual)
        {
            ConfigurarDia(fecha, mesActual, null);
        }

        public void ConfigurarDia(DateTime fecha, bool mesActual,
            PreferenciasApariencia aparienciaCalendario)
        {
            Fecha = fecha;
            this.mesActual = mesActual;
            if (aparienciaCalendario != null) apariencia = aparienciaCalendario;
            Seleccionado = false;
            lbldia.Text = fecha.Day.ToString();
            ActualizarColor();
        }

        private void ActualizarColor()
        {
            bool esHoy = Fecha.Date == DateTime.Today;
            Color colorDia;
            if (Seleccionado)
                colorDia = Color.FromArgb(apariencia.DiaSeleccionado);
            else if (esHoy)
                colorDia = Color.FromArgb(apariencia.DiaActual);
            else if (!mesActual)
                colorDia = Color.FromArgb(apariencia.DiaFueraDeMes);
            else
                colorDia = Color.FromArgb(apariencia.DiaNormal);

            // La tarjeta y la lista usan siempre el color real del panel que
            // tienen detrás; en modo oscuro se evita volver a los tonos claros.
            Color colorPanel = TemaAplicacion.SuperficieParaModo(Color.FromArgb(apariencia.PanelDia));
            Color colorBorde = TemaAplicacion.BordeParaModo(Color.FromArgb(apariencia.BordeSuave));
            Color colorDiaActual = TemaAplicacion.AcentoParaModo(colorDia);
            Color colorHoy = TemaAplicacion.AcentoParaModo(Color.FromArgb(apariencia.DiaActual));

            pnlDia.Back = colorDiaActual;
            // El día de hoy conserva un borde propio incluso cuando está
            // seleccionado, para que ambos estados sigan siendo reconocibles.
            pnlDia.BorderColor = esHoy ? colorHoy : Color.Transparent;
            pnlDia.BorderWidth = esHoy ? 2F : 0F;
            lbldia.ForeColor = PaletaAplicacion.ColorTextoContraste(colorDiaActual);
            pnlListTareas.Back = colorPanel;
            pnlListTareas.BorderColor = colorBorde;
            listTareas.BackColor = colorPanel;
            listTareas.ForeColor = PaletaAplicacion.ColorTextoContraste(colorPanel);
            listTareas.Invalidate();
        }

        private void ListTareas_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= listTareas.Items.Count) return;
            Color fondo = pnlListTareas.Back.GetValueOrDefault(listTareas.BackColor);
            using (var pincel = new SolidBrush(fondo)) e.Graphics.FillRectangle(pincel, e.Bounds);
            TextRenderer.DrawText(e.Graphics, listTareas.GetItemText(listTareas.Items[e.Index]),
                listTareas.Font, Rectangle.Inflate(e.Bounds, -2, 0),
                PaletaAplicacion.ColorTextoContraste(fondo), TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);
        }

        private void AjustarDistribucion()
        {

        }

        public void ActualizarTema()
        {
            ActualizarColor();
        }

        public void ActualizarApariencia(PreferenciasApariencia nuevaApariencia)
        {
            if (nuevaApariencia != null) apariencia = nuevaApariencia;
            ActualizarColor();
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
