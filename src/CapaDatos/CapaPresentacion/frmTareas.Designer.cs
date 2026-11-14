namespace CapaPresentacion
{
    partial class frmTareas
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTareas));
            this.lblTargetaNombre = new System.Windows.Forms.Label();
            this.lblSubTargetaTiempoDescanso = new System.Windows.Forms.Label();
            this.lblSubTargetaAgregarWeb = new System.Windows.Forms.Label();
            this.lblSubTargetaAgregarApp = new System.Windows.Forms.Label();
            this.lblTargetaHoraFin = new System.Windows.Forms.Label();
            this.lblTargetaHoraInicio = new System.Windows.Forms.Label();
            this.cbNoBloquearWebs = new System.Windows.Forms.CheckBox();
            this.cbNoBloquearApps = new System.Windows.Forms.CheckBox();
            this.flLista = new CapaPresentacion.BufferedFlowLayoutPanel();
            this.pnlLayoutLista = new AntdUI.Panel();
            this.pnlTargetaTiempoDescanso = new AntdUI.Panel();
            this.btnSumarDescanso = new AntdUI.Button();
            this.btnRestarDescanso = new AntdUI.Button();
            this.pbSubTargetaTiempoDescanso = new AntdUI.Image3D();
            this.nudDescanso = new AntdUI.Input();
            this.pnlTargetaDescansos = new AntdUI.Panel();
            this.lblDescripcionDescanso = new System.Windows.Forms.Label();
            this.lblTargetaDescanso = new System.Windows.Forms.Label();
            this.pbTargetaDescanso = new AntdUI.Image3D();
            this.pnlTargetaIntervalo = new AntdUI.Panel();
            this.btnSumarIntervalo = new AntdUI.Button();
            this.btnRestarIntervalo = new AntdUI.Button();
            this.pbTargetaIntervalo = new AntdUI.Image3D();
            this.nudIntervalo = new AntdUI.Input();
            this.lblTargetaIntervalo = new System.Windows.Forms.Label();
            this.pnlTargetaAgregar = new AntdUI.Panel();
            this.pnlSubTargetaAgregarWeb = new AntdUI.Panel();
            this.btnAgregarWeb = new AntdUI.Button();
            this.lblDescripcionAgregar = new System.Windows.Forms.Label();
            this.lblTargetaAgregar = new System.Windows.Forms.Label();
            this.pbTargetaAgregar = new AntdUI.Image3D();
            this.pnlSubTargetaAgregaApp = new AntdUI.Panel();
            this.btnAgregarApp = new AntdUI.Button();
            this.pnlTargetaTarea = new AntdUI.Panel();
            this.txtNombreTarea = new AntdUI.Input();
            this.dtpHoraFin = new AntdUI.TimePicker();
            this.dtpHoraInicio = new AntdUI.TimePicker();
            this.lblDescripcionTarea = new System.Windows.Forms.Label();
            this.lblTargetaTarea = new System.Windows.Forms.Label();
            this.pbTargetaTarea = new AntdUI.Image3D();
            this.btnGuardar = new AntdUI.Button();
            this.btnCerrar = new AntdUI.Button();
            this.lblPanelTop = new System.Windows.Forms.Label();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnCancelar = new AntdUI.Button();
            this.pnlLayoutLista.SuspendLayout();
            this.pnlTargetaTiempoDescanso.SuspendLayout();
            this.pnlTargetaDescansos.SuspendLayout();
            this.pnlTargetaIntervalo.SuspendLayout();
            this.pnlTargetaAgregar.SuspendLayout();
            this.pnlSubTargetaAgregarWeb.SuspendLayout();
            this.pnlSubTargetaAgregaApp.SuspendLayout();
            this.pnlTargetaTarea.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTargetaNombre
            //
            this.lblTargetaNombre.BackColor = System.Drawing.Color.Transparent;
            this.lblTargetaNombre.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTargetaNombre.ForeColor = System.Drawing.Color.White;
            this.lblTargetaNombre.Location = new System.Drawing.Point(12, 108);
            this.lblTargetaNombre.Name = "lblTargetaNombre";
            this.lblTargetaNombre.Size = new System.Drawing.Size(156, 32);
            this.lblTargetaNombre.TabIndex = 39;
            this.lblTargetaNombre.Text = "Nombre";
            this.lblTargetaNombre.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblSubTargetaTiempoDescanso
            //
            this.lblSubTargetaTiempoDescanso.BackColor = System.Drawing.Color.Transparent;
            this.lblSubTargetaTiempoDescanso.Font = new System.Drawing.Font("Inter", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSubTargetaTiempoDescanso.ForeColor = System.Drawing.Color.White;
            this.lblSubTargetaTiempoDescanso.Location = new System.Drawing.Point(60, 12);
            this.lblSubTargetaTiempoDescanso.Name = "lblSubTargetaTiempoDescanso";
            this.lblSubTargetaTiempoDescanso.Size = new System.Drawing.Size(180, 36);
            this.lblSubTargetaTiempoDescanso.TabIndex = 30;
            this.lblSubTargetaTiempoDescanso.Text = "Tiempo de descanso";
            this.lblSubTargetaTiempoDescanso.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblSubTargetaAgregarWeb
            //
            this.lblSubTargetaAgregarWeb.BackColor = System.Drawing.Color.Transparent;
            this.lblSubTargetaAgregarWeb.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSubTargetaAgregarWeb.ForeColor = System.Drawing.Color.White;
            this.lblSubTargetaAgregarWeb.Location = new System.Drawing.Point(0, 12);
            this.lblSubTargetaAgregarWeb.Name = "lblSubTargetaAgregarWeb";
            this.lblSubTargetaAgregarWeb.Size = new System.Drawing.Size(240, 37);
            this.lblSubTargetaAgregarWeb.TabIndex = 29;
            this.lblSubTargetaAgregarWeb.Text = "Agregar Web";
            this.lblSubTargetaAgregarWeb.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblSubTargetaAgregarApp
            //
            this.lblSubTargetaAgregarApp.BackColor = System.Drawing.Color.Transparent;
            this.lblSubTargetaAgregarApp.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSubTargetaAgregarApp.ForeColor = System.Drawing.Color.White;
            this.lblSubTargetaAgregarApp.Location = new System.Drawing.Point(0, 12);
            this.lblSubTargetaAgregarApp.Name = "lblSubTargetaAgregarApp";
            this.lblSubTargetaAgregarApp.Size = new System.Drawing.Size(240, 37);
            this.lblSubTargetaAgregarApp.TabIndex = 28;
            this.lblSubTargetaAgregarApp.Text = "Agregar aplicación";
            this.lblSubTargetaAgregarApp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblTargetaHoraFin
            //
            this.lblTargetaHoraFin.BackColor = System.Drawing.Color.Transparent;
            this.lblTargetaHoraFin.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTargetaHoraFin.ForeColor = System.Drawing.Color.White;
            this.lblTargetaHoraFin.Location = new System.Drawing.Point(12, 204);
            this.lblTargetaHoraFin.Name = "lblTargetaHoraFin";
            this.lblTargetaHoraFin.Size = new System.Drawing.Size(156, 36);
            this.lblTargetaHoraFin.TabIndex = 26;
            this.lblTargetaHoraFin.Text = "Hora de fin";
            this.lblTargetaHoraFin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblTargetaHoraInicio
            //
            this.lblTargetaHoraInicio.BackColor = System.Drawing.Color.Transparent;
            this.lblTargetaHoraInicio.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTargetaHoraInicio.ForeColor = System.Drawing.Color.White;
            this.lblTargetaHoraInicio.Location = new System.Drawing.Point(12, 156);
            this.lblTargetaHoraInicio.Name = "lblTargetaHoraInicio";
            this.lblTargetaHoraInicio.Size = new System.Drawing.Size(156, 30);
            this.lblTargetaHoraInicio.TabIndex = 25;
            this.lblTargetaHoraInicio.Text = "Hora de inicio";
            this.lblTargetaHoraInicio.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // cbNoBloquearWebs
            //
            this.cbNoBloquearWebs.AutoSize = true;
            this.cbNoBloquearWebs.BackColor = System.Drawing.Color.Transparent;
            this.cbNoBloquearWebs.Font = new System.Drawing.Font("Inter", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbNoBloquearWebs.ForeColor = System.Drawing.Color.White;
            this.cbNoBloquearWebs.Location = new System.Drawing.Point(24, 108);
            this.cbNoBloquearWebs.Name = "cbNoBloquearWebs";
            this.cbNoBloquearWebs.Size = new System.Drawing.Size(183, 21);
            this.cbNoBloquearWebs.TabIndex = 46;
            this.cbNoBloquearWebs.Text = "No bloquear páginas web";
            this.cbNoBloquearWebs.UseVisualStyleBackColor = false;
            //
            // cbNoBloquearApps
            //
            this.cbNoBloquearApps.AutoSize = true;
            this.cbNoBloquearApps.BackColor = System.Drawing.Color.Transparent;
            this.cbNoBloquearApps.Font = new System.Drawing.Font("Inter", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbNoBloquearApps.ForeColor = System.Drawing.Color.White;
            this.cbNoBloquearApps.Location = new System.Drawing.Point(24, 108);
            this.cbNoBloquearApps.Name = "cbNoBloquearApps";
            this.cbNoBloquearApps.Size = new System.Drawing.Size(181, 21);
            this.cbNoBloquearApps.TabIndex = 47;
            this.cbNoBloquearApps.Text = "No bloquear aplicaciones";
            this.cbNoBloquearApps.UseVisualStyleBackColor = false;
            //
            // flLista
            //
            this.flLista.AutoScroll = true;
            this.flLista.BackColor = System.Drawing.Color.Transparent;
            this.flLista.Font = new System.Drawing.Font("Inter", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.flLista.Location = new System.Drawing.Point(12, 12);
            this.flLista.Name = "flLista";
            this.flLista.Size = new System.Drawing.Size(684, 288);
            this.flLista.TabIndex = 47;
            //
            // pnlLayoutLista
            //
            this.pnlLayoutLista.Back = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(205)))), ((int)(((byte)(232)))));
            this.pnlLayoutLista.BackColor = System.Drawing.Color.Transparent;
            this.pnlLayoutLista.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlLayoutLista.BorderWidth = 1F;
            this.pnlLayoutLista.Controls.Add(this.flLista);
            this.pnlLayoutLista.Location = new System.Drawing.Point(552, 360);
            this.pnlLayoutLista.Name = "pnlLayoutLista";
            this.pnlLayoutLista.Radius = 12;
            this.pnlLayoutLista.Size = new System.Drawing.Size(708, 312);
            this.pnlLayoutLista.TabIndex = 55;
            this.pnlLayoutLista.Text = "pnlLista";
            this.pnlLayoutLista.Click += new System.EventHandler(this.PnlLayoutLista_Click);
            //
            // pnlTargetaTiempoDescanso
            //
            this.pnlTargetaTiempoDescanso.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.pnlTargetaTiempoDescanso.BackColor = System.Drawing.Color.Transparent;
            this.pnlTargetaTiempoDescanso.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlTargetaTiempoDescanso.BorderWidth = 1F;
            this.pnlTargetaTiempoDescanso.Controls.Add(this.btnSumarDescanso);
            this.pnlTargetaTiempoDescanso.Controls.Add(this.btnRestarDescanso);
            this.pnlTargetaTiempoDescanso.Controls.Add(this.pbSubTargetaTiempoDescanso);
            this.pnlTargetaTiempoDescanso.Controls.Add(this.nudDescanso);
            this.pnlTargetaTiempoDescanso.Controls.Add(this.lblSubTargetaTiempoDescanso);
            this.pnlTargetaTiempoDescanso.Location = new System.Drawing.Point(24, 120);
            this.pnlTargetaTiempoDescanso.Name = "pnlTargetaTiempoDescanso";
            this.pnlTargetaTiempoDescanso.Size = new System.Drawing.Size(324, 142);
            this.pnlTargetaTiempoDescanso.TabIndex = 49;
            this.pnlTargetaTiempoDescanso.Text = "panel2";
            //
            // btnSumarDescanso
            //
            this.btnSumarDescanso.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnSumarDescanso.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnSumarDescanso.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnSumarDescanso.BorderWidth = 1F;
            this.btnSumarDescanso.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.btnSumarDescanso.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(66)))), ((int)(((byte)(120)))));
            this.btnSumarDescanso.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSumarDescanso.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnSumarDescanso.Location = new System.Drawing.Point(156, 84);
            this.btnSumarDescanso.Name = "btnSumarDescanso";
            this.btnSumarDescanso.Size = new System.Drawing.Size(84, 48);
            this.btnSumarDescanso.TabIndex = 34;
            this.btnSumarDescanso.Text = "+";
            //
            // btnRestarDescanso
            //
            this.btnRestarDescanso.BorderWidth = 1F;
            this.btnRestarDescanso.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.btnRestarDescanso.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(66)))), ((int)(((byte)(120)))));
            this.btnRestarDescanso.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRestarDescanso.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnRestarDescanso.Location = new System.Drawing.Point(72, 84);
            this.btnRestarDescanso.Name = "btnRestarDescanso";
            this.btnRestarDescanso.Size = new System.Drawing.Size(84, 48);
            this.btnRestarDescanso.TabIndex = 33;
            this.btnRestarDescanso.Text = "-";
            //
            // pbSubTargetaTiempoDescanso
            //
            this.pbSubTargetaTiempoDescanso.BackColor = System.Drawing.Color.Transparent;
            this.pbSubTargetaTiempoDescanso.Image = ((System.Drawing.Image)(resources.GetObject("pbSubTargetaTiempoDescanso.Image")));
            this.pbSubTargetaTiempoDescanso.Location = new System.Drawing.Point(12, 12);
            this.pbSubTargetaTiempoDescanso.Name = "pbSubTargetaTiempoDescanso";
            this.pbSubTargetaTiempoDescanso.Size = new System.Drawing.Size(40, 38);
            this.pbSubTargetaTiempoDescanso.TabIndex = 32;
            this.pbSubTargetaTiempoDescanso.Text = "image3D1";
            //
            // nudDescanso
            //
            this.nudDescanso.BackColor = System.Drawing.Color.Transparent;
            this.nudDescanso.BorderActive = System.Drawing.Color.Transparent;
            this.nudDescanso.BorderColor = System.Drawing.Color.Transparent;
            this.nudDescanso.BorderHover = System.Drawing.Color.Transparent;
            this.nudDescanso.BorderWidth = 0F;
            this.nudDescanso.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudDescanso.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.nudDescanso.Location = new System.Drawing.Point(0, 48);
            this.nudDescanso.Name = "nudDescanso";
            this.nudDescanso.Size = new System.Drawing.Size(324, 36);
            this.nudDescanso.TabIndex = 31;
            this.nudDescanso.Text = "0 min";
            this.nudDescanso.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            //
            // pnlTargetaDescansos
            //
            this.pnlTargetaDescansos.Back = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(63)))), ((int)(((byte)(118)))));
            this.pnlTargetaDescansos.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlTargetaDescansos.BorderWidth = 1F;
            this.pnlTargetaDescansos.Controls.Add(this.lblDescripcionDescanso);
            this.pnlTargetaDescansos.Controls.Add(this.lblTargetaDescanso);
            this.pnlTargetaDescansos.Controls.Add(this.pbTargetaDescanso);
            this.pnlTargetaDescansos.Controls.Add(this.pnlTargetaIntervalo);
            this.pnlTargetaDescansos.Controls.Add(this.pnlTargetaTiempoDescanso);
            this.pnlTargetaDescansos.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.pnlTargetaDescansos.Location = new System.Drawing.Point(552, 72);
            this.pnlTargetaDescansos.Name = "pnlTargetaDescansos";
            this.pnlTargetaDescansos.Size = new System.Drawing.Size(708, 276);
            this.pnlTargetaDescansos.TabIndex = 50;
            this.pnlTargetaDescansos.Text = "panel1";
            //
            // lblDescripcionDescanso
            //
            this.lblDescripcionDescanso.BackColor = System.Drawing.Color.Transparent;
            this.lblDescripcionDescanso.Font = new System.Drawing.Font("Inter", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDescripcionDescanso.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.lblDescripcionDescanso.Location = new System.Drawing.Point(108, 60);
            this.lblDescripcionDescanso.Name = "lblDescripcionDescanso";
            this.lblDescripcionDescanso.Size = new System.Drawing.Size(372, 36);
            this.lblDescripcionDescanso.TabIndex = 35;
            this.lblDescripcionDescanso.Text = "Configura cuánto tiempo descansas y cuándo";
            //
            // lblTargetaDescanso
            //
            this.lblTargetaDescanso.BackColor = System.Drawing.Color.Transparent;
            this.lblTargetaDescanso.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTargetaDescanso.ForeColor = System.Drawing.Color.White;
            this.lblTargetaDescanso.Location = new System.Drawing.Point(108, 24);
            this.lblTargetaDescanso.Name = "lblTargetaDescanso";
            this.lblTargetaDescanso.Size = new System.Drawing.Size(384, 36);
            this.lblTargetaDescanso.TabIndex = 35;
            this.lblTargetaDescanso.Text = "Descansos";
            //
            // pbTargetaDescanso
            //
            this.pbTargetaDescanso.BackColor = System.Drawing.Color.Transparent;
            this.pbTargetaDescanso.Image = ((System.Drawing.Image)(resources.GetObject("pbTargetaDescanso.Image")));
            this.pbTargetaDescanso.Location = new System.Drawing.Point(24, 24);
            this.pbTargetaDescanso.Name = "pbTargetaDescanso";
            this.pbTargetaDescanso.Size = new System.Drawing.Size(72, 72);
            this.pbTargetaDescanso.TabIndex = 35;
            this.pbTargetaDescanso.Text = "image3D3";
            //
            // pnlTargetaIntervalo
            //
            this.pnlTargetaIntervalo.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.pnlTargetaIntervalo.BackColor = System.Drawing.Color.Transparent;
            this.pnlTargetaIntervalo.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlTargetaIntervalo.BorderWidth = 1F;
            this.pnlTargetaIntervalo.Controls.Add(this.btnSumarIntervalo);
            this.pnlTargetaIntervalo.Controls.Add(this.btnRestarIntervalo);
            this.pnlTargetaIntervalo.Controls.Add(this.pbTargetaIntervalo);
            this.pnlTargetaIntervalo.Controls.Add(this.nudIntervalo);
            this.pnlTargetaIntervalo.Controls.Add(this.lblTargetaIntervalo);
            this.pnlTargetaIntervalo.Location = new System.Drawing.Point(360, 120);
            this.pnlTargetaIntervalo.Name = "pnlTargetaIntervalo";
            this.pnlTargetaIntervalo.Size = new System.Drawing.Size(324, 142);
            this.pnlTargetaIntervalo.TabIndex = 50;
            this.pnlTargetaIntervalo.Text = "panel3";
            //
            // btnSumarIntervalo
            //
            this.btnSumarIntervalo.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnSumarIntervalo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnSumarIntervalo.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnSumarIntervalo.BorderWidth = 1F;
            this.btnSumarIntervalo.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.btnSumarIntervalo.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(66)))), ((int)(((byte)(120)))));
            this.btnSumarIntervalo.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSumarIntervalo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnSumarIntervalo.Location = new System.Drawing.Point(156, 84);
            this.btnSumarIntervalo.Name = "btnSumarIntervalo";
            this.btnSumarIntervalo.Size = new System.Drawing.Size(84, 48);
            this.btnSumarIntervalo.TabIndex = 34;
            this.btnSumarIntervalo.Text = "+";
            //
            // btnRestarIntervalo
            //
            this.btnRestarIntervalo.BorderWidth = 1F;
            this.btnRestarIntervalo.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.btnRestarIntervalo.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(66)))), ((int)(((byte)(120)))));
            this.btnRestarIntervalo.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRestarIntervalo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnRestarIntervalo.Location = new System.Drawing.Point(72, 84);
            this.btnRestarIntervalo.Name = "btnRestarIntervalo";
            this.btnRestarIntervalo.Size = new System.Drawing.Size(84, 48);
            this.btnRestarIntervalo.TabIndex = 33;
            this.btnRestarIntervalo.Text = "-";
            //
            // pbTargetaIntervalo
            //
            this.pbTargetaIntervalo.BackColor = System.Drawing.Color.Transparent;
            this.pbTargetaIntervalo.Image = ((System.Drawing.Image)(resources.GetObject("pbTargetaIntervalo.Image")));
            this.pbTargetaIntervalo.Location = new System.Drawing.Point(12, 12);
            this.pbTargetaIntervalo.Name = "pbTargetaIntervalo";
            this.pbTargetaIntervalo.Size = new System.Drawing.Size(40, 38);
            this.pbTargetaIntervalo.TabIndex = 32;
            this.pbTargetaIntervalo.Text = "image3D2";
            //
            // nudIntervalo
            //
            this.nudIntervalo.BackColor = System.Drawing.Color.Transparent;
            this.nudIntervalo.BorderActive = System.Drawing.Color.Transparent;
            this.nudIntervalo.BorderColor = System.Drawing.Color.Transparent;
            this.nudIntervalo.BorderHover = System.Drawing.Color.Transparent;
            this.nudIntervalo.BorderWidth = 0F;
            this.nudIntervalo.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudIntervalo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.nudIntervalo.Location = new System.Drawing.Point(0, 48);
            this.nudIntervalo.Name = "nudIntervalo";
            this.nudIntervalo.Size = new System.Drawing.Size(324, 36);
            this.nudIntervalo.TabIndex = 31;
            this.nudIntervalo.Text = "0 min";
            this.nudIntervalo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            //
            // lblTargetaIntervalo
            //
            this.lblTargetaIntervalo.BackColor = System.Drawing.Color.Transparent;
            this.lblTargetaIntervalo.Font = new System.Drawing.Font("Inter", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTargetaIntervalo.ForeColor = System.Drawing.Color.White;
            this.lblTargetaIntervalo.Location = new System.Drawing.Point(48, 12);
            this.lblTargetaIntervalo.Name = "lblTargetaIntervalo";
            this.lblTargetaIntervalo.Size = new System.Drawing.Size(204, 36);
            this.lblTargetaIntervalo.TabIndex = 30;
            this.lblTargetaIntervalo.Text = "Cada cuánto descansar";
            this.lblTargetaIntervalo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // pnlTargetaAgregar
            //
            this.pnlTargetaAgregar.Back = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(63)))), ((int)(((byte)(118)))));
            this.pnlTargetaAgregar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlTargetaAgregar.BorderWidth = 1F;
            this.pnlTargetaAgregar.Controls.Add(this.pnlSubTargetaAgregarWeb);
            this.pnlTargetaAgregar.Controls.Add(this.lblDescripcionAgregar);
            this.pnlTargetaAgregar.Controls.Add(this.lblTargetaAgregar);
            this.pnlTargetaAgregar.Controls.Add(this.pbTargetaAgregar);
            this.pnlTargetaAgregar.Controls.Add(this.pnlSubTargetaAgregaApp);
            this.pnlTargetaAgregar.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.pnlTargetaAgregar.Location = new System.Drawing.Point(12, 360);
            this.pnlTargetaAgregar.Name = "pnlTargetaAgregar";
            this.pnlTargetaAgregar.Size = new System.Drawing.Size(528, 312);
            this.pnlTargetaAgregar.TabIndex = 51;
            this.pnlTargetaAgregar.Text = "panel4";
            //
            // pnlSubTargetaAgregarWeb
            //
            this.pnlSubTargetaAgregarWeb.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.pnlSubTargetaAgregarWeb.BackColor = System.Drawing.Color.Transparent;
            this.pnlSubTargetaAgregarWeb.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlSubTargetaAgregarWeb.BorderWidth = 1F;
            this.pnlSubTargetaAgregarWeb.Controls.Add(this.lblSubTargetaAgregarWeb);
            this.pnlSubTargetaAgregarWeb.Controls.Add(this.btnAgregarWeb);
            this.pnlSubTargetaAgregarWeb.Controls.Add(this.cbNoBloquearWebs);
            this.pnlSubTargetaAgregarWeb.Location = new System.Drawing.Point(264, 132);
            this.pnlSubTargetaAgregarWeb.Name = "pnlSubTargetaAgregarWeb";
            this.pnlSubTargetaAgregarWeb.Size = new System.Drawing.Size(236, 142);
            this.pnlSubTargetaAgregarWeb.TabIndex = 51;
            this.pnlSubTargetaAgregarWeb.Text = "panel7";
            //
            // btnAgregarWeb
            //
            this.btnAgregarWeb.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(119)))), ((int)(((byte)(255)))));
            this.btnAgregarWeb.BorderWidth = 1F;
            this.btnAgregarWeb.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.btnAgregarWeb.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(66)))), ((int)(((byte)(120)))));
            this.btnAgregarWeb.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAgregarWeb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnAgregarWeb.Icon = ((System.Drawing.Image)(resources.GetObject("btnAgregarWeb.Icon")));
            this.btnAgregarWeb.Location = new System.Drawing.Point(84, 48);
            this.btnAgregarWeb.Name = "btnAgregarWeb";
            this.btnAgregarWeb.Size = new System.Drawing.Size(60, 60);
            this.btnAgregarWeb.TabIndex = 36;
            this.btnAgregarWeb.Click += new System.EventHandler(this.btnAgregarWeb_Click);
            //
            // lblDescripcionAgregar
            //
            this.lblDescripcionAgregar.BackColor = System.Drawing.Color.Transparent;
            this.lblDescripcionAgregar.Font = new System.Drawing.Font("Inter", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDescripcionAgregar.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.lblDescripcionAgregar.Location = new System.Drawing.Point(108, 60);
            this.lblDescripcionAgregar.Name = "lblDescripcionAgregar";
            this.lblDescripcionAgregar.Size = new System.Drawing.Size(384, 48);
            this.lblDescripcionAgregar.TabIndex = 35;
            this.lblDescripcionAgregar.Text = "Agrega las aplicaciones y páginas webs que necesites";
            //
            // lblTargetaAgregar
            //
            this.lblTargetaAgregar.BackColor = System.Drawing.Color.Transparent;
            this.lblTargetaAgregar.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTargetaAgregar.ForeColor = System.Drawing.Color.White;
            this.lblTargetaAgregar.Location = new System.Drawing.Point(108, 24);
            this.lblTargetaAgregar.Name = "lblTargetaAgregar";
            this.lblTargetaAgregar.Size = new System.Drawing.Size(348, 36);
            this.lblTargetaAgregar.TabIndex = 35;
            this.lblTargetaAgregar.Text = "Agregar";
            //
            // pbTargetaAgregar
            //
            this.pbTargetaAgregar.BackColor = System.Drawing.Color.Transparent;
            this.pbTargetaAgregar.Image = ((System.Drawing.Image)(resources.GetObject("pbTargetaAgregar.Image")));
            this.pbTargetaAgregar.Location = new System.Drawing.Point(24, 24);
            this.pbTargetaAgregar.Name = "pbTargetaAgregar";
            this.pbTargetaAgregar.Size = new System.Drawing.Size(72, 72);
            this.pbTargetaAgregar.TabIndex = 35;
            this.pbTargetaAgregar.Text = "image3D4";
            //
            // pnlSubTargetaAgregaApp
            //
            this.pnlSubTargetaAgregaApp.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.pnlSubTargetaAgregaApp.BackColor = System.Drawing.Color.Transparent;
            this.pnlSubTargetaAgregaApp.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlSubTargetaAgregaApp.BorderWidth = 1F;
            this.pnlSubTargetaAgregaApp.Controls.Add(this.lblSubTargetaAgregarApp);
            this.pnlSubTargetaAgregaApp.Controls.Add(this.btnAgregarApp);
            this.pnlSubTargetaAgregaApp.Controls.Add(this.cbNoBloquearApps);
            this.pnlSubTargetaAgregaApp.Location = new System.Drawing.Point(24, 132);
            this.pnlSubTargetaAgregaApp.Name = "pnlSubTargetaAgregaApp";
            this.pnlSubTargetaAgregaApp.Size = new System.Drawing.Size(236, 142);
            this.pnlSubTargetaAgregaApp.TabIndex = 50;
            this.pnlSubTargetaAgregaApp.Text = "panel6";
            //
            // btnAgregarApp
            //
            this.btnAgregarApp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(119)))), ((int)(((byte)(255)))));
            this.btnAgregarApp.BorderWidth = 1F;
            this.btnAgregarApp.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.btnAgregarApp.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(66)))), ((int)(((byte)(120)))));
            this.btnAgregarApp.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAgregarApp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnAgregarApp.Icon = ((System.Drawing.Image)(resources.GetObject("btnAgregarApp.Icon")));
            this.btnAgregarApp.Location = new System.Drawing.Point(84, 48);
            this.btnAgregarApp.Name = "btnAgregarApp";
            this.btnAgregarApp.Size = new System.Drawing.Size(60, 60);
            this.btnAgregarApp.TabIndex = 35;
            this.btnAgregarApp.Click += new System.EventHandler(this.btnAgregarApp_Click);
            //
            // pnlTargetaTarea
            //
            this.pnlTargetaTarea.Back = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(63)))), ((int)(((byte)(118)))));
            this.pnlTargetaTarea.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlTargetaTarea.BorderWidth = 1F;
            this.pnlTargetaTarea.Controls.Add(this.txtNombreTarea);
            this.pnlTargetaTarea.Controls.Add(this.dtpHoraFin);
            this.pnlTargetaTarea.Controls.Add(this.dtpHoraInicio);
            this.pnlTargetaTarea.Controls.Add(this.lblDescripcionTarea);
            this.pnlTargetaTarea.Controls.Add(this.lblTargetaTarea);
            this.pnlTargetaTarea.Controls.Add(this.pbTargetaTarea);
            this.pnlTargetaTarea.Controls.Add(this.lblTargetaHoraFin);
            this.pnlTargetaTarea.Controls.Add(this.lblTargetaNombre);
            this.pnlTargetaTarea.Controls.Add(this.lblTargetaHoraInicio);
            this.pnlTargetaTarea.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.pnlTargetaTarea.Location = new System.Drawing.Point(12, 72);
            this.pnlTargetaTarea.Name = "pnlTargetaTarea";
            this.pnlTargetaTarea.Size = new System.Drawing.Size(528, 276);
            this.pnlTargetaTarea.TabIndex = 52;
            this.pnlTargetaTarea.Text = "panel5";
            //
            // txtNombreTarea
            //
            this.txtNombreTarea.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.txtNombreTarea.Font = new System.Drawing.Font("Inter", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNombreTarea.Location = new System.Drawing.Point(180, 96);
            this.txtNombreTarea.Name = "txtNombreTarea";
            this.txtNombreTarea.Size = new System.Drawing.Size(324, 48);
            this.txtNombreTarea.TabIndex = 53;
            //
            // dtpHoraFin
            //
            this.dtpHoraFin.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.dtpHoraFin.Font = new System.Drawing.Font("Inter", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpHoraFin.Location = new System.Drawing.Point(180, 192);
            this.dtpHoraFin.Name = "dtpHoraFin";
            this.dtpHoraFin.Size = new System.Drawing.Size(324, 48);
            this.dtpHoraFin.TabIndex = 54;
            //
            // dtpHoraInicio
            //
            this.dtpHoraInicio.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.dtpHoraInicio.Font = new System.Drawing.Font("Inter", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpHoraInicio.Location = new System.Drawing.Point(180, 144);
            this.dtpHoraInicio.Name = "dtpHoraInicio";
            this.dtpHoraInicio.Size = new System.Drawing.Size(324, 48);
            this.dtpHoraInicio.TabIndex = 53;
            //
            // lblDescripcionTarea
            //
            this.lblDescripcionTarea.BackColor = System.Drawing.Color.Transparent;
            this.lblDescripcionTarea.Font = new System.Drawing.Font("Inter", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDescripcionTarea.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.lblDescripcionTarea.Location = new System.Drawing.Point(108, 60);
            this.lblDescripcionTarea.Name = "lblDescripcionTarea";
            this.lblDescripcionTarea.Size = new System.Drawing.Size(384, 48);
            this.lblDescripcionTarea.TabIndex = 35;
            this.lblDescripcionTarea.Text = "Nombra tu tarea y programala";
            //
            // lblTargetaTarea
            //
            this.lblTargetaTarea.BackColor = System.Drawing.Color.Transparent;
            this.lblTargetaTarea.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTargetaTarea.ForeColor = System.Drawing.Color.White;
            this.lblTargetaTarea.Location = new System.Drawing.Point(108, 24);
            this.lblTargetaTarea.Name = "lblTargetaTarea";
            this.lblTargetaTarea.Size = new System.Drawing.Size(348, 36);
            this.lblTargetaTarea.TabIndex = 35;
            this.lblTargetaTarea.Text = "Crear tarea";
            //
            // pbTargetaTarea
            //
            this.pbTargetaTarea.BackColor = System.Drawing.Color.Transparent;
            this.pbTargetaTarea.Image = ((System.Drawing.Image)(resources.GetObject("pbTargetaTarea.Image")));
            this.pbTargetaTarea.Location = new System.Drawing.Point(24, 24);
            this.pbTargetaTarea.Name = "pbTargetaTarea";
            this.pbTargetaTarea.Size = new System.Drawing.Size(72, 72);
            this.pbTargetaTarea.TabIndex = 35;
            this.pbTargetaTarea.Text = "image3D5";
            //
            // btnGuardar
            //
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(119)))), ((int)(((byte)(255)))));
            this.btnGuardar.BorderWidth = 1F;
            this.btnGuardar.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.btnGuardar.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnGuardar.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnGuardar.Icon = ((System.Drawing.Image)(resources.GetObject("btnGuardar.Icon")));
            this.btnGuardar.Location = new System.Drawing.Point(516, 696);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(276, 60);
            this.btnGuardar.TabIndex = 47;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            //
            // btnCerrar
            //
            this.btnCerrar.BackActive = System.Drawing.Color.Transparent;
            this.btnCerrar.BackColor = System.Drawing.Color.Transparent;
            this.btnCerrar.BackHover = System.Drawing.Color.Transparent;
            this.btnCerrar.DefaultBack = System.Drawing.Color.Transparent;
            this.btnCerrar.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCerrar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnCerrar.Icon = ((System.Drawing.Image)(resources.GetObject("btnCerrar.Icon")));
            this.btnCerrar.IconRatio = 2F;
            this.btnCerrar.Location = new System.Drawing.Point(1188, 0);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(60, 60);
            this.btnCerrar.TabIndex = 73;
            this.btnCerrar.Click += new System.EventHandler(this.BtnCerrar_Click);
            //
            // lblPanelTop
            //
            this.lblPanelTop.BackColor = System.Drawing.Color.Transparent;
            this.lblPanelTop.Font = new System.Drawing.Font("Inter", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPanelTop.ForeColor = System.Drawing.Color.White;
            this.lblPanelTop.Location = new System.Drawing.Point(24, 0);
            this.lblPanelTop.Name = "lblPanelTop";
            this.lblPanelTop.Size = new System.Drawing.Size(720, 60);
            this.lblPanelTop.TabIndex = 7;
            this.lblPanelTop.Text = "Creador de tareas";
            this.lblPanelTop.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlTop
            //
            this.pnlTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.pnlTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTop.Controls.Add(this.btnCerrar);
            this.pnlTop.Controls.Add(this.lblPanelTop);
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new System.Windows.Forms.Padding(10);
            this.pnlTop.Size = new System.Drawing.Size(1284, 60);
            this.pnlTop.TabIndex = 74;
            //
            // btnCancelar
            //
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(119)))), ((int)(((byte)(255)))));
            this.btnCancelar.BorderWidth = 1F;
            this.btnCancelar.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnCancelar.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnCancelar.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnCancelar.Location = new System.Drawing.Point(1092, 696);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(168, 60);
            this.btnCancelar.TabIndex = 85;
            this.btnCancelar.Text = "Cancelar";
            //
            // frmTareas
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lavender;
            this.ClientSize = new System.Drawing.Size(1276, 756);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.pnlTargetaTarea);
            this.Controls.Add(this.pnlTargetaAgregar);
            this.Controls.Add(this.pnlTargetaDescansos);
            this.Controls.Add(this.pnlLayoutLista);
            this.Font = new System.Drawing.Font("Symbol", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmTareas";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " ";
            this.pnlLayoutLista.ResumeLayout(false);
            this.pnlTargetaTiempoDescanso.ResumeLayout(false);
            this.pnlTargetaDescansos.ResumeLayout(false);
            this.pnlTargetaIntervalo.ResumeLayout(false);
            this.pnlTargetaAgregar.ResumeLayout(false);
            this.pnlSubTargetaAgregarWeb.ResumeLayout(false);
            this.pnlSubTargetaAgregarWeb.PerformLayout();
            this.pnlSubTargetaAgregaApp.ResumeLayout(false);
            this.pnlSubTargetaAgregaApp.PerformLayout();
            this.pnlTargetaTarea.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblTargetaNombre;
        private System.Windows.Forms.Label lblSubTargetaTiempoDescanso;
        private System.Windows.Forms.Label lblSubTargetaAgregarWeb;
        private System.Windows.Forms.Label lblSubTargetaAgregarApp;
        private System.Windows.Forms.Label lblTargetaHoraFin;
        private System.Windows.Forms.Label lblTargetaHoraInicio;
        private System.Windows.Forms.CheckBox cbNoBloquearWebs;
        private System.Windows.Forms.CheckBox cbNoBloquearApps;
        private BufferedFlowLayoutPanel flLista;
        private AntdUI.Panel pnlLayoutLista;
        private AntdUI.Panel pnlTargetaTiempoDescanso;
        private AntdUI.Input nudDescanso;
        private AntdUI.Image3D pbSubTargetaTiempoDescanso;
        private AntdUI.Button btnSumarDescanso;
        private AntdUI.Button btnRestarDescanso;
        private AntdUI.Panel pnlTargetaDescansos;
        private AntdUI.Panel pnlTargetaIntervalo;
        private AntdUI.Button btnSumarIntervalo;
        private AntdUI.Button btnRestarIntervalo;
        private AntdUI.Image3D pbTargetaIntervalo;
        private AntdUI.Input nudIntervalo;
        private System.Windows.Forms.Label lblTargetaIntervalo;
        private System.Windows.Forms.Label lblDescripcionDescanso;
        private System.Windows.Forms.Label lblTargetaDescanso;
        private AntdUI.Image3D pbTargetaDescanso;
        private AntdUI.Panel pnlTargetaAgregar;
        private AntdUI.Button btnAgregarApp;
        private System.Windows.Forms.Label lblDescripcionAgregar;
        private System.Windows.Forms.Label lblTargetaAgregar;
        private AntdUI.Image3D pbTargetaAgregar;
        private AntdUI.Button btnAgregarWeb;
        private AntdUI.Panel pnlTargetaTarea;
        private System.Windows.Forms.Label lblDescripcionTarea;
        private System.Windows.Forms.Label lblTargetaTarea;
        private AntdUI.Image3D pbTargetaTarea;
        private AntdUI.Button btnGuardar;
        private AntdUI.TimePicker dtpHoraInicio;
        private AntdUI.Input txtNombreTarea;
        private AntdUI.TimePicker dtpHoraFin;
        private AntdUI.Panel pnlSubTargetaAgregarWeb;
        private AntdUI.Panel pnlSubTargetaAgregaApp;
        private System.Windows.Forms.Label lblPanelTop;
        private AntdUI.Button btnCerrar;
        private System.Windows.Forms.Panel pnlTop;
        private AntdUI.Button btnCancelar;
    }
}
