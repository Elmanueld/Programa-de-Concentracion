using System.Windows.Forms;

namespace CapaPresentacion
{
    public sealed partial class PrincipalForm
    {
        private TableLayoutPanel tlpCalendario;
        private Label lblPequenaLunes;
        private Label lblPequenaMartes;
        private Label lblPequenaMiercoles;
        private Label lblPequenaJueves;
        private Label lblPequenaViernes;
        private Label lblPequenaSabado;
        private Label lblPequenaDomingo;
        private Label lblMesAnio;
        private BufferedFlowLayoutPanel flTareas;
        private Label lblPequenaDiaSeleccionado;
                private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrincipalForm));
            this.tlpCalendario = new System.Windows.Forms.TableLayoutPanel();
            this.lblPequenaLunes = new System.Windows.Forms.Label();
            this.lblPequenaMartes = new System.Windows.Forms.Label();
            this.lblPequenaMiercoles = new System.Windows.Forms.Label();
            this.lblPequenaJueves = new System.Windows.Forms.Label();
            this.lblPequenaViernes = new System.Windows.Forms.Label();
            this.lblPequenaSabado = new System.Windows.Forms.Label();
            this.lblPequenaDomingo = new System.Windows.Forms.Label();
            this.lblMesAnio = new System.Windows.Forms.Label();
            this.flTareas = new BufferedFlowLayoutPanel();
            this.lblPequenaDiaSeleccionado = new System.Windows.Forms.Label();
            this.pnlPequenoMartes = new AntdUI.Panel();
            this.pnlPequenoMiercoles = new AntdUI.Panel();
            this.pnlPequenoLunes = new AntdUI.Panel();
            this.pnlPequenoJueves = new AntdUI.Panel();
            this.pnlPequenoViernes = new AntdUI.Panel();
            this.pnlPequenoSabado = new AntdUI.Panel();
            this.pnlPequenoDomingo = new AntdUI.Panel();
            this.btnSiguiente = new AntdUI.Button();
            this.btnAnterior = new AntdUI.Button();
            this.btnAgregarTarea = new AntdUI.Button();
            this.btnDetener = new AntdUI.Button();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnTopCerrar = new AntdUI.Button();
            this.lblBienvenida = new System.Windows.Forms.Label();
            this.pbFotoDePerfil = new AntdUI.Image3D();
            this.btnTopMinimizar = new AntdUI.Button();
            this.btnTopConfiguracion = new AntdUI.Button();
            this.btnTopCerrarSesion = new AntdUI.Button();
            this.pnlPequenoDiaSeleccionado = new AntdUI.Panel();
            this.pnlLayoutTareas = new AntdUI.Panel();
            this.pnlLayoutCalendario = new AntdUI.Panel();
            this.pnlPequenoMartes.SuspendLayout();
            this.pnlPequenoMiercoles.SuspendLayout();
            this.pnlPequenoLunes.SuspendLayout();
            this.pnlPequenoJueves.SuspendLayout();
            this.pnlPequenoViernes.SuspendLayout();
            this.pnlPequenoSabado.SuspendLayout();
            this.pnlPequenoDomingo.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.pnlPequenoDiaSeleccionado.SuspendLayout();
            this.pnlLayoutTareas.SuspendLayout();
            this.pnlLayoutCalendario.SuspendLayout();
            this.SuspendLayout();
            //
            // tlpCalendario
            //
            this.tlpCalendario.BackColor = System.Drawing.Color.Transparent;
            this.tlpCalendario.ColumnCount = 7;
            this.tlpCalendario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tlpCalendario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tlpCalendario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tlpCalendario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tlpCalendario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tlpCalendario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tlpCalendario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tlpCalendario.ForeColor = System.Drawing.Color.Transparent;
            this.tlpCalendario.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tlpCalendario.Location = new System.Drawing.Point(12, 12);
            this.tlpCalendario.Name = "tlpCalendario";
            this.tlpCalendario.RowCount = 6;
            this.tlpCalendario.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpCalendario.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpCalendario.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpCalendario.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpCalendario.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpCalendario.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tlpCalendario.Size = new System.Drawing.Size(1560, 828);
            this.tlpCalendario.TabIndex = 2;
            //
            // lblPequenaLunes
            //
            this.lblPequenaLunes.BackColor = System.Drawing.Color.Transparent;
            this.lblPequenaLunes.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPequenaLunes.ForeColor = System.Drawing.Color.White;
            this.lblPequenaLunes.Location = new System.Drawing.Point(0, 0);
            this.lblPequenaLunes.Name = "lblPequenaLunes";
            this.lblPequenaLunes.Size = new System.Drawing.Size(204, 36);
            this.lblPequenaLunes.TabIndex = 3;
            this.lblPequenaLunes.Text = "Lunes";
            this.lblPequenaLunes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblPequenaMartes
            //
            this.lblPequenaMartes.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPequenaMartes.ForeColor = System.Drawing.Color.White;
            this.lblPequenaMartes.Location = new System.Drawing.Point(0, 0);
            this.lblPequenaMartes.Name = "lblPequenaMartes";
            this.lblPequenaMartes.Size = new System.Drawing.Size(204, 36);
            this.lblPequenaMartes.TabIndex = 4;
            this.lblPequenaMartes.Text = "Martes";
            this.lblPequenaMartes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblPequenaMiercoles
            //
            this.lblPequenaMiercoles.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPequenaMiercoles.ForeColor = System.Drawing.Color.White;
            this.lblPequenaMiercoles.Location = new System.Drawing.Point(0, 0);
            this.lblPequenaMiercoles.Name = "lblPequenaMiercoles";
            this.lblPequenaMiercoles.Size = new System.Drawing.Size(204, 36);
            this.lblPequenaMiercoles.TabIndex = 5;
            this.lblPequenaMiercoles.Text = "Miércoles";
            this.lblPequenaMiercoles.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblPequenaJueves
            //
            this.lblPequenaJueves.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPequenaJueves.ForeColor = System.Drawing.Color.White;
            this.lblPequenaJueves.Location = new System.Drawing.Point(0, 0);
            this.lblPequenaJueves.Name = "lblPequenaJueves";
            this.lblPequenaJueves.Size = new System.Drawing.Size(204, 36);
            this.lblPequenaJueves.TabIndex = 6;
            this.lblPequenaJueves.Text = "Jueves";
            this.lblPequenaJueves.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblPequenaViernes
            //
            this.lblPequenaViernes.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPequenaViernes.ForeColor = System.Drawing.Color.White;
            this.lblPequenaViernes.Location = new System.Drawing.Point(0, 0);
            this.lblPequenaViernes.Name = "lblPequenaViernes";
            this.lblPequenaViernes.Size = new System.Drawing.Size(204, 36);
            this.lblPequenaViernes.TabIndex = 7;
            this.lblPequenaViernes.Text = "Viernes";
            this.lblPequenaViernes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblPequenaSabado
            //
            this.lblPequenaSabado.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPequenaSabado.ForeColor = System.Drawing.Color.White;
            this.lblPequenaSabado.Location = new System.Drawing.Point(0, 0);
            this.lblPequenaSabado.Name = "lblPequenaSabado";
            this.lblPequenaSabado.Size = new System.Drawing.Size(204, 36);
            this.lblPequenaSabado.TabIndex = 8;
            this.lblPequenaSabado.Text = "Sábado";
            this.lblPequenaSabado.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblPequenaDomingo
            //
            this.lblPequenaDomingo.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPequenaDomingo.ForeColor = System.Drawing.Color.White;
            this.lblPequenaDomingo.Location = new System.Drawing.Point(0, 0);
            this.lblPequenaDomingo.Name = "lblPequenaDomingo";
            this.lblPequenaDomingo.Size = new System.Drawing.Size(204, 36);
            this.lblPequenaDomingo.TabIndex = 9;
            this.lblPequenaDomingo.Text = "Domingo";
            this.lblPequenaDomingo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblMesAnio
            //
            this.lblMesAnio.BackColor = System.Drawing.Color.Transparent;
            this.lblMesAnio.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMesAnio.Location = new System.Drawing.Point(72, 96);
            this.lblMesAnio.Name = "lblMesAnio";
            this.lblMesAnio.Size = new System.Drawing.Size(1464, 36);
            this.lblMesAnio.TabIndex = 12;
            this.lblMesAnio.Text = "MesAnio";
            this.lblMesAnio.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // flTareas
            //
            this.flTareas.AutoScroll = true;
            this.flTareas.BackColor = System.Drawing.Color.Transparent;
            this.flTareas.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flTareas.Location = new System.Drawing.Point(12, 12);
            this.flTareas.Name = "flTareas";
            this.flTareas.Size = new System.Drawing.Size(300, 780);
            this.flTareas.TabIndex = 13;
            this.flTareas.WrapContents = false;
            //
            // lblPequenaDiaSeleccionado
            //
            this.lblPequenaDiaSeleccionado.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPequenaDiaSeleccionado.ForeColor = System.Drawing.Color.White;
            this.lblPequenaDiaSeleccionado.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPequenaDiaSeleccionado.Location = new System.Drawing.Point(12, 0);
            this.lblPequenaDiaSeleccionado.Name = "lblPequenaDiaSeleccionado";
            this.lblPequenaDiaSeleccionado.Size = new System.Drawing.Size(300, 36);
            this.lblPequenaDiaSeleccionado.TabIndex = 15;
            this.lblPequenaDiaSeleccionado.Text = "Tareas del día seleccionado";
            this.lblPequenaDiaSeleccionado.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPequenaDiaSeleccionado.Click += new System.EventHandler(this.Label8_Click);
            //
            // pnlPequenoMartes
            //
            this.pnlPequenoMartes.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.pnlPequenoMartes.BackColor = System.Drawing.Color.Transparent;
            this.pnlPequenoMartes.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(15)))), ((int)(((byte)(28)))));
            this.pnlPequenoMartes.BorderWidth = 1F;
            this.pnlPequenoMartes.Controls.Add(this.lblPequenaMartes);
            this.pnlPequenoMartes.Location = new System.Drawing.Point(252, 132);
            this.pnlPequenoMartes.Name = "pnlPequenoMartes";
            this.pnlPequenoMartes.Size = new System.Drawing.Size(204, 36);
            this.pnlPequenoMartes.TabIndex = 57;
            this.pnlPequenoMartes.Text = "panel3";
            //
            // pnlPequenoMiercoles
            //
            this.pnlPequenoMiercoles.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.pnlPequenoMiercoles.BackColor = System.Drawing.Color.Transparent;
            this.pnlPequenoMiercoles.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(15)))), ((int)(((byte)(28)))));
            this.pnlPequenoMiercoles.BorderWidth = 1F;
            this.pnlPequenoMiercoles.Controls.Add(this.lblPequenaMiercoles);
            this.pnlPequenoMiercoles.Location = new System.Drawing.Point(468, 132);
            this.pnlPequenoMiercoles.Name = "pnlPequenoMiercoles";
            this.pnlPequenoMiercoles.Size = new System.Drawing.Size(204, 36);
            this.pnlPequenoMiercoles.TabIndex = 58;
            this.pnlPequenoMiercoles.Text = "panel1";
            //
            // pnlPequenoLunes
            //
            this.pnlPequenoLunes.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.pnlPequenoLunes.BackColor = System.Drawing.Color.Transparent;
            this.pnlPequenoLunes.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(15)))), ((int)(((byte)(28)))));
            this.pnlPequenoLunes.BorderWidth = 1F;
            this.pnlPequenoLunes.Controls.Add(this.lblPequenaLunes);
            this.pnlPequenoLunes.Location = new System.Drawing.Point(24, 132);
            this.pnlPequenoLunes.Name = "pnlPequenoLunes";
            this.pnlPequenoLunes.Size = new System.Drawing.Size(204, 36);
            this.pnlPequenoLunes.TabIndex = 58;
            this.pnlPequenoLunes.Text = "panel2";
            //
            // pnlPequenoJueves
            //
            this.pnlPequenoJueves.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.pnlPequenoJueves.BackColor = System.Drawing.Color.Transparent;
            this.pnlPequenoJueves.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(15)))), ((int)(((byte)(28)))));
            this.pnlPequenoJueves.BorderWidth = 1F;
            this.pnlPequenoJueves.Controls.Add(this.lblPequenaJueves);
            this.pnlPequenoJueves.Location = new System.Drawing.Point(696, 132);
            this.pnlPequenoJueves.Name = "pnlPequenoJueves";
            this.pnlPequenoJueves.Size = new System.Drawing.Size(204, 36);
            this.pnlPequenoJueves.TabIndex = 59;
            this.pnlPequenoJueves.Text = "panel4";
            //
            // pnlPequenoViernes
            //
            this.pnlPequenoViernes.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.pnlPequenoViernes.BackColor = System.Drawing.Color.Transparent;
            this.pnlPequenoViernes.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(15)))), ((int)(((byte)(28)))));
            this.pnlPequenoViernes.BorderWidth = 1F;
            this.pnlPequenoViernes.Controls.Add(this.lblPequenaViernes);
            this.pnlPequenoViernes.Location = new System.Drawing.Point(924, 132);
            this.pnlPequenoViernes.Name = "pnlPequenoViernes";
            this.pnlPequenoViernes.Size = new System.Drawing.Size(204, 36);
            this.pnlPequenoViernes.TabIndex = 60;
            this.pnlPequenoViernes.Text = "panel5";
            //
            // pnlPequenoSabado
            //
            this.pnlPequenoSabado.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.pnlPequenoSabado.BackColor = System.Drawing.Color.Transparent;
            this.pnlPequenoSabado.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(15)))), ((int)(((byte)(28)))));
            this.pnlPequenoSabado.BorderWidth = 1F;
            this.pnlPequenoSabado.Controls.Add(this.lblPequenaSabado);
            this.pnlPequenoSabado.Location = new System.Drawing.Point(1152, 132);
            this.pnlPequenoSabado.Name = "pnlPequenoSabado";
            this.pnlPequenoSabado.Size = new System.Drawing.Size(204, 36);
            this.pnlPequenoSabado.TabIndex = 61;
            this.pnlPequenoSabado.Text = "panel6";
            //
            // pnlPequenoDomingo
            //
            this.pnlPequenoDomingo.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.pnlPequenoDomingo.BackColor = System.Drawing.Color.Transparent;
            this.pnlPequenoDomingo.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(15)))), ((int)(((byte)(28)))));
            this.pnlPequenoDomingo.BorderWidth = 1F;
            this.pnlPequenoDomingo.Controls.Add(this.lblPequenaDomingo);
            this.pnlPequenoDomingo.Location = new System.Drawing.Point(1380, 132);
            this.pnlPequenoDomingo.Name = "pnlPequenoDomingo";
            this.pnlPequenoDomingo.Size = new System.Drawing.Size(204, 36);
            this.pnlPequenoDomingo.TabIndex = 62;
            this.pnlPequenoDomingo.Text = "panel7";
            //
            // btnSiguiente
            //
            this.btnSiguiente.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnSiguiente.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnSiguiente.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnSiguiente.BorderWidth = 1F;
            this.btnSiguiente.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnSiguiente.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnSiguiente.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSiguiente.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnSiguiente.Icon = ((System.Drawing.Image)(resources.GetObject("btnSiguiente.Icon")));
            this.btnSiguiente.IconRatio = 1.2F;
            this.btnSiguiente.Location = new System.Drawing.Point(1536, 84);
            this.btnSiguiente.Name = "btnSiguiente";
            this.btnSiguiente.Size = new System.Drawing.Size(60, 48);
            this.btnSiguiente.TabIndex = 63;
            this.btnSiguiente.Click += new System.EventHandler(this.BtnSiguiente_Click);
            //
            // btnAnterior
            //
            this.btnAnterior.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAnterior.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAnterior.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAnterior.BorderWidth = 1F;
            this.btnAnterior.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnAnterior.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnAnterior.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnterior.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnAnterior.Icon = ((System.Drawing.Image)(resources.GetObject("btnAnterior.Icon")));
            this.btnAnterior.IconRatio = 1.2F;
            this.btnAnterior.Location = new System.Drawing.Point(12, 84);
            this.btnAnterior.Name = "btnAnterior";
            this.btnAnterior.Size = new System.Drawing.Size(60, 48);
            this.btnAnterior.TabIndex = 64;
            this.btnAnterior.Click += new System.EventHandler(this.BtnAnterior_Click);
            //
            // btnAgregarTarea
            //
            this.btnAgregarTarea.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAgregarTarea.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAgregarTarea.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAgregarTarea.BorderWidth = 1F;
            this.btnAgregarTarea.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(116)))), ((int)(((byte)(62)))));
            this.btnAgregarTarea.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnAgregarTarea.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAgregarTarea.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnAgregarTarea.IconRatio = 1.2F;
            this.btnAgregarTarea.Location = new System.Drawing.Point(1608, 984);
            this.btnAgregarTarea.Name = "btnAgregarTarea";
            this.btnAgregarTarea.Size = new System.Drawing.Size(324, 48);
            this.btnAgregarTarea.TabIndex = 65;
            this.btnAgregarTarea.Text = "Agregar Tarea";
            this.btnAgregarTarea.Click += new System.EventHandler(this.btnAgregarTarea_Click);
            //
            // btnDetener
            //
            this.btnDetener.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(28)))), ((int)(((byte)(38)))));
            this.btnDetener.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(45)))), ((int)(((byte)(55)))));
            this.btnDetener.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(35)))), ((int)(((byte)(45)))));
            this.btnDetener.BorderWidth = 1F;
            this.btnDetener.DefaultBack = System.Drawing.Color.Brown;
            this.btnDetener.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnDetener.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDetener.ForeColor = System.Drawing.Color.White;
            this.btnDetener.Location = new System.Drawing.Point(1608, 84);
            this.btnDetener.Name = "btnDetener";
            this.btnDetener.Size = new System.Drawing.Size(324, 48);
            this.btnDetener.TabIndex = 95;
            this.btnDetener.Text = "Detener tarea actual";
            //
            // pnlTop
            //
            this.pnlTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.pnlTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTop.Controls.Add(this.btnTopCerrar);
            this.pnlTop.Controls.Add(this.lblBienvenida);
            this.pnlTop.Controls.Add(this.pbFotoDePerfil);
            this.pnlTop.Controls.Add(this.btnTopMinimizar);
            this.pnlTop.Controls.Add(this.btnTopConfiguracion);
            this.pnlTop.Controls.Add(this.btnTopCerrarSesion);
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new System.Windows.Forms.Padding(10);
            this.pnlTop.Size = new System.Drawing.Size(1956, 60);
            this.pnlTop.TabIndex = 75;
            //
            // btnTopCerrar
            //
            this.btnTopCerrar.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopCerrar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopCerrar.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopCerrar.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(156)))), ((int)(((byte)(195)))));
            this.btnTopCerrar.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTopCerrar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnTopCerrar.Icon = ((System.Drawing.Image)(resources.GetObject("btnTopCerrar.Icon")));
            this.btnTopCerrar.IconRatio = 1.5F;
            this.btnTopCerrar.Location = new System.Drawing.Point(1872, 0);
            this.btnTopCerrar.Name = "btnTopCerrar";
            this.btnTopCerrar.Size = new System.Drawing.Size(60, 60);
            this.btnTopCerrar.TabIndex = 81;
            this.btnTopCerrar.Click += new System.EventHandler(this.BtnTopCerrar_Click);
            //
            // lblBienvenida
            //
            this.lblBienvenida.BackColor = System.Drawing.Color.Transparent;
            this.lblBienvenida.Font = new System.Drawing.Font("Inter", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBienvenida.ForeColor = System.Drawing.Color.White;
            this.lblBienvenida.Location = new System.Drawing.Point(192, 0);
            this.lblBienvenida.Name = "lblBienvenida";
            this.lblBienvenida.Size = new System.Drawing.Size(396, 60);
            this.lblBienvenida.TabIndex = 80;
            this.lblBienvenida.Text = "Bienvenido, Nombre";
            this.lblBienvenida.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pbFotoDePerfil
            //
            this.pbFotoDePerfil.BackColor = System.Drawing.Color.Transparent;
            this.pbFotoDePerfil.Location = new System.Drawing.Point(132, 0);
            this.pbFotoDePerfil.Name = "pbFotoDePerfil";
            this.pbFotoDePerfil.Size = new System.Drawing.Size(60, 60);
            this.pbFotoDePerfil.TabIndex = 79;
            //
            // btnTopMinimizar
            //
            this.btnTopMinimizar.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopMinimizar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopMinimizar.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopMinimizar.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(156)))), ((int)(((byte)(195)))));
            this.btnTopMinimizar.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTopMinimizar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnTopMinimizar.Icon = ((System.Drawing.Image)(resources.GetObject("btnTopMinimizar.Icon")));
            this.btnTopMinimizar.IconRatio = 1.2F;
            this.btnTopMinimizar.Location = new System.Drawing.Point(1812, 0);
            this.btnTopMinimizar.Name = "btnTopMinimizar";
            this.btnTopMinimizar.Size = new System.Drawing.Size(60, 60);
            this.btnTopMinimizar.TabIndex = 78;
            this.btnTopMinimizar.Click += new System.EventHandler(this.btnMinimizar_Click);
            //
            // btnTopConfiguracion
            //
            this.btnTopConfiguracion.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopConfiguracion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopConfiguracion.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopConfiguracion.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(156)))), ((int)(((byte)(195)))));
            this.btnTopConfiguracion.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTopConfiguracion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnTopConfiguracion.Icon = ((System.Drawing.Image)(resources.GetObject("btnTopConfiguracion.Icon")));
            this.btnTopConfiguracion.IconRatio = 1.2F;
            this.btnTopConfiguracion.Location = new System.Drawing.Point(0, 0);
            this.btnTopConfiguracion.Name = "btnTopConfiguracion";
            this.btnTopConfiguracion.Size = new System.Drawing.Size(60, 60);
            this.btnTopConfiguracion.TabIndex = 77;
            //
            // btnTopCerrarSesion
            //
            this.btnTopCerrarSesion.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopCerrarSesion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopCerrarSesion.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopCerrarSesion.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(156)))), ((int)(((byte)(195)))));
            this.btnTopCerrarSesion.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTopCerrarSesion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnTopCerrarSesion.Icon = ((System.Drawing.Image)(resources.GetObject("btnTopCerrarSesion.Icon")));
            this.btnTopCerrarSesion.IconRatio = 1.2F;
            this.btnTopCerrarSesion.Location = new System.Drawing.Point(60, 0);
            this.btnTopCerrarSesion.Name = "btnTopCerrarSesion";
            this.btnTopCerrarSesion.Size = new System.Drawing.Size(60, 60);
            this.btnTopCerrarSesion.TabIndex = 76;
            this.btnTopCerrarSesion.Click += new System.EventHandler(this.BtnCerrarSesion_Click);
            //
            // pnlPequenoDiaSeleccionado
            //
            this.pnlPequenoDiaSeleccionado.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.pnlPequenoDiaSeleccionado.BackColor = System.Drawing.Color.Transparent;
            this.pnlPequenoDiaSeleccionado.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlPequenoDiaSeleccionado.BorderWidth = 1F;
            this.pnlPequenoDiaSeleccionado.Controls.Add(this.lblPequenaDiaSeleccionado);
            this.pnlPequenoDiaSeleccionado.Location = new System.Drawing.Point(1608, 140);
            this.pnlPequenoDiaSeleccionado.Name = "pnlPequenoDiaSeleccionado";
            this.pnlPequenoDiaSeleccionado.Radius = 15;
            this.pnlPequenoDiaSeleccionado.Size = new System.Drawing.Size(324, 36);
            this.pnlPequenoDiaSeleccionado.TabIndex = 85;
            this.pnlPequenoDiaSeleccionado.Text = "panel8";
            //
            // pnlLayoutTareas
            //
            this.pnlLayoutTareas.Back = System.Drawing.Color.LightSteelBlue;
            this.pnlLayoutTareas.BackColor = System.Drawing.Color.Transparent;
            this.pnlLayoutTareas.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlLayoutTareas.BorderWidth = 1F;
            this.pnlLayoutTareas.Controls.Add(this.flTareas);
            this.pnlLayoutTareas.Location = new System.Drawing.Point(1608, 180);
            this.pnlLayoutTareas.Name = "pnlLayoutTareas";
            this.pnlLayoutTareas.Radius = 12;
            this.pnlLayoutTareas.Size = new System.Drawing.Size(324, 804);
            this.pnlLayoutTareas.TabIndex = 93;
            this.pnlLayoutTareas.Text = "pnlLista";
            //
            // pnlLayoutCalendario
            //
            this.pnlLayoutCalendario.Back = System.Drawing.Color.LightSteelBlue;
            this.pnlLayoutCalendario.BackColor = System.Drawing.Color.Transparent;
            this.pnlLayoutCalendario.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlLayoutCalendario.BorderWidth = 1F;
            this.pnlLayoutCalendario.Controls.Add(this.tlpCalendario);
            this.pnlLayoutCalendario.Location = new System.Drawing.Point(12, 180);
            this.pnlLayoutCalendario.Name = "pnlLayoutCalendario";
            this.pnlLayoutCalendario.Radius = 12;
            this.pnlLayoutCalendario.Size = new System.Drawing.Size(1584, 852);
            this.pnlLayoutCalendario.TabIndex = 94;
            this.pnlLayoutCalendario.Text = "panel9";
            //
            // PrincipalForm
            //
            this.BackColor = System.Drawing.Color.Lavender;
            this.ClientSize = new System.Drawing.Size(1946, 1050);
            this.Controls.Add(this.pnlPequenoDiaSeleccionado);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.btnAgregarTarea);
            this.Controls.Add(this.btnDetener);
            this.Controls.Add(this.btnAnterior);
            this.Controls.Add(this.btnSiguiente);
            this.Controls.Add(this.pnlPequenoDomingo);
            this.Controls.Add(this.pnlPequenoSabado);
            this.Controls.Add(this.pnlPequenoViernes);
            this.Controls.Add(this.pnlPequenoJueves);
            this.Controls.Add(this.pnlPequenoLunes);
            this.Controls.Add(this.pnlPequenoMiercoles);
            this.Controls.Add(this.pnlPequenoMartes);
            this.Controls.Add(this.lblMesAnio);
            this.Controls.Add(this.pnlLayoutTareas);
            this.Controls.Add(this.pnlLayoutCalendario);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "PrincipalForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.pnlPequenoMartes.ResumeLayout(false);
            this.pnlPequenoMiercoles.ResumeLayout(false);
            this.pnlPequenoLunes.ResumeLayout(false);
            this.pnlPequenoJueves.ResumeLayout(false);
            this.pnlPequenoViernes.ResumeLayout(false);
            this.pnlPequenoSabado.ResumeLayout(false);
            this.pnlPequenoDomingo.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            this.pnlPequenoDiaSeleccionado.ResumeLayout(false);
            this.pnlLayoutTareas.ResumeLayout(false);
            this.pnlLayoutCalendario.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private AntdUI.Panel pnlPequenoMartes;
        private AntdUI.Panel pnlPequenoMiercoles;
        private AntdUI.Panel pnlPequenoLunes;
        private AntdUI.Panel pnlPequenoJueves;
        private AntdUI.Panel pnlPequenoViernes;
        private AntdUI.Panel pnlPequenoSabado;
        private AntdUI.Panel pnlPequenoDomingo;
        private AntdUI.Button btnSiguiente;
        private AntdUI.Button btnAnterior;
        private AntdUI.Button btnAgregarTarea;
        private AntdUI.Button btnDetener;
        private Panel pnlTop;
        private AntdUI.Button btnTopCerrarSesion;
        private AntdUI.Button btnTopConfiguracion;
        private AntdUI.Button btnTopMinimizar;
        private Label lblBienvenida;
        private AntdUI.Image3D pbFotoDePerfil;
        private AntdUI.Panel pnlPequenoDiaSeleccionado;
        private AntdUI.Panel pnlLayoutTareas;
        private AntdUI.Panel pnlLayoutCalendario;
        private AntdUI.Button btnTopCerrar;
    }
}
