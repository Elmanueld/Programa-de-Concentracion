namespace CapaPresentacion
{
    partial class frmSeleccionarWeb
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSeleccionarWeb));
            this.pnlPequenoLink = new AntdUI.Panel();
            this.lblPequenaLink = new System.Windows.Forms.Label();
            this.pnlLayoutSeleccionadas = new AntdUI.Panel();
            this.flWebSeleccionadas = new BufferedFlowLayoutPanel();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnCerrar = new AntdUI.Button();
            this.lblPanelTop = new System.Windows.Forms.Label();
            this.txtLink = new AntdUI.Input();
            this.txtBuscar = new AntdUI.Input();
            this.btnManual = new AntdUI.Button();
            this.btnCancelar = new AntdUI.Button();
            this.btnAceptar = new AntdUI.Button();
            this.pnlLayoutBusqueda = new AntdUI.Panel();
            this.flBusqueda = new BufferedFlowLayoutPanel();
            this.pnlPequenoBuscar = new AntdUI.Panel();
            this.lblPequenaBuscar = new System.Windows.Forms.Label();
            this.pnlPequenoLink.SuspendLayout();
            this.pnlLayoutSeleccionadas.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.pnlLayoutBusqueda.SuspendLayout();
            this.pnlPequenoBuscar.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlPequenoLink
            //
            this.pnlPequenoLink.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.pnlPequenoLink.BackColor = System.Drawing.Color.Transparent;
            this.pnlPequenoLink.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlPequenoLink.BorderWidth = 1F;
            this.pnlPequenoLink.Controls.Add(this.lblPequenaLink);
            this.pnlPequenoLink.Location = new System.Drawing.Point(804, 96);
            this.pnlPequenoLink.Name = "pnlPequenoLink";
            this.pnlPequenoLink.Radius = 15;
            this.pnlPequenoLink.Size = new System.Drawing.Size(170, 36);
            this.pnlPequenoLink.TabIndex = 95;
            //
            // lblPequenaLink
            //
            this.lblPequenaLink.Font = new System.Drawing.Font("Inter", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPequenaLink.ForeColor = System.Drawing.Color.White;
            this.lblPequenaLink.Location = new System.Drawing.Point(4, 4);
            this.lblPequenaLink.Name = "lblPequenaLink";
            this.lblPequenaLink.Size = new System.Drawing.Size(176, 24);
            this.lblPequenaLink.TabIndex = 2;
            this.lblPequenaLink.Text = "Insertar con link";
            this.lblPequenaLink.Click += new System.EventHandler(this.Label4_Click);
            //
            // pnlLayoutSeleccionadas
            //
            this.pnlLayoutSeleccionadas.Back = System.Drawing.Color.LightSteelBlue;
            this.pnlLayoutSeleccionadas.BackColor = System.Drawing.Color.Transparent;
            this.pnlLayoutSeleccionadas.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlLayoutSeleccionadas.BorderWidth = 1F;
            this.pnlLayoutSeleccionadas.Controls.Add(this.flWebSeleccionadas);
            this.pnlLayoutSeleccionadas.Location = new System.Drawing.Point(804, 136);
            this.pnlLayoutSeleccionadas.Name = "pnlLayoutSeleccionadas";
            this.pnlLayoutSeleccionadas.Radius = 12;
            this.pnlLayoutSeleccionadas.Size = new System.Drawing.Size(552, 524);
            this.pnlLayoutSeleccionadas.TabIndex = 93;
            this.pnlLayoutSeleccionadas.Text = "panel1";
            //
            // flWebSeleccionadas
            //
            this.flWebSeleccionadas.AutoScroll = true;
            this.flWebSeleccionadas.BackColor = System.Drawing.Color.Transparent;
            this.flWebSeleccionadas.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flWebSeleccionadas.Location = new System.Drawing.Point(12, 12);
            this.flWebSeleccionadas.Name = "flWebSeleccionadas";
            this.flWebSeleccionadas.Size = new System.Drawing.Size(528, 504);
            this.flWebSeleccionadas.TabIndex = 4;
            this.flWebSeleccionadas.WrapContents = true;
            //
            // pnlTop
            //
            this.pnlTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.pnlTop.Controls.Add(this.btnCerrar);
            this.pnlTop.Controls.Add(this.lblPanelTop);
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new System.Windows.Forms.Padding(10);
            this.pnlTop.Size = new System.Drawing.Size(1380, 60);
            this.pnlTop.TabIndex = 71;
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
            this.btnCerrar.Location = new System.Drawing.Point(1296, 0);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(60, 60);
            this.btnCerrar.TabIndex = 72;
            this.btnCerrar.Click += new System.EventHandler(this.BtnCerrar_Click);
            //
            // lblPanelTop
            //
            this.lblPanelTop.BackColor = System.Drawing.Color.Transparent;
            this.lblPanelTop.Font = new System.Drawing.Font("Inter", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPanelTop.ForeColor = System.Drawing.Color.White;
            this.lblPanelTop.Location = new System.Drawing.Point(24, 0);
            this.lblPanelTop.Name = "lblPanelTop";
            this.lblPanelTop.Size = new System.Drawing.Size(696, 60);
            this.lblPanelTop.TabIndex = 7;
            this.lblPanelTop.Text = "Selector de páginas web";
            this.lblPanelTop.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtLink
            //
            this.txtLink.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.txtLink.Font = new System.Drawing.Font("Inter", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLink.Location = new System.Drawing.Point(972, 88);
            this.txtLink.Name = "txtLink";
            this.txtLink.Size = new System.Drawing.Size(324, 48);
            this.txtLink.TabIndex = 70;
            //
            // txtBuscar
            //
            this.txtBuscar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.txtBuscar.Font = new System.Drawing.Font("Inter", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBuscar.Location = new System.Drawing.Point(228, 88);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(336, 48);
            this.txtBuscar.TabIndex = 69;
            //
            // btnManual
            //
            this.btnManual.BorderWidth = 1F;
            this.btnManual.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.btnManual.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnManual.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManual.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnManual.Icon = ((System.Drawing.Image)(resources.GetObject("btnManual.Icon")));
            this.btnManual.IconRatio = 1F;
            this.btnManual.Location = new System.Drawing.Point(1296, 84);
            this.btnManual.Name = "btnManual";
            this.btnManual.Size = new System.Drawing.Size(48, 48);
            this.btnManual.TabIndex = 68;
            this.btnManual.Click += new System.EventHandler(this.btnAgregar_Click);
            //
            // btnCancelar
            //
            this.btnCancelar.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnCancelar.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnCancelar.BorderWidth = 1F;
            this.btnCancelar.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnCancelar.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnCancelar.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnCancelar.IconRatio = 1.2F;
            this.btnCancelar.Location = new System.Drawing.Point(1200, 660);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(156, 48);
            this.btnCancelar.TabIndex = 67;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            //
            // btnAceptar
            //
            this.btnAceptar.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAceptar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAceptar.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAceptar.BorderWidth = 1F;
            this.btnAceptar.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(64)))), ((int)(((byte)(82)))));
            this.btnAceptar.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnAceptar.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAceptar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnAceptar.IconRatio = 1.2F;
            this.btnAceptar.Location = new System.Drawing.Point(1044, 660);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(156, 48);
            this.btnAceptar.TabIndex = 66;
            this.btnAceptar.Text = "Aceptar";
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            //
            // pnlLayoutBusqueda
            //
            this.pnlLayoutBusqueda.Back = System.Drawing.Color.LightSteelBlue;
            this.pnlLayoutBusqueda.BackColor = System.Drawing.Color.Transparent;
            this.pnlLayoutBusqueda.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlLayoutBusqueda.BorderWidth = 1F;
            this.pnlLayoutBusqueda.Controls.Add(this.flBusqueda);
            this.pnlLayoutBusqueda.Location = new System.Drawing.Point(24, 136);
            this.pnlLayoutBusqueda.Name = "pnlLayoutBusqueda";
            this.pnlLayoutBusqueda.Radius = 12;
            this.pnlLayoutBusqueda.Size = new System.Drawing.Size(768, 524);
            this.pnlLayoutBusqueda.TabIndex = 92;
            this.pnlLayoutBusqueda.Text = "pnlLista";
            //
            // flBusqueda
            //
            this.flBusqueda.AutoScroll = true;
            this.flBusqueda.BackColor = System.Drawing.Color.Transparent;
            this.flBusqueda.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flBusqueda.Location = new System.Drawing.Point(12, 12);
            this.flBusqueda.Name = "flBusqueda";
            this.flBusqueda.Size = new System.Drawing.Size(744, 504);
            this.flBusqueda.TabIndex = 5;
            this.flBusqueda.WrapContents = true;
            //
            // pnlPequenoBuscar
            //
            this.pnlPequenoBuscar.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.pnlPequenoBuscar.BackColor = System.Drawing.Color.Transparent;
            this.pnlPequenoBuscar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlPequenoBuscar.BorderWidth = 1F;
            this.pnlPequenoBuscar.Controls.Add(this.lblPequenaBuscar);
            this.pnlPequenoBuscar.Location = new System.Drawing.Point(24, 96);
            this.pnlPequenoBuscar.Name = "pnlPequenoBuscar";
            this.pnlPequenoBuscar.Radius = 15;
            this.pnlPequenoBuscar.Size = new System.Drawing.Size(204, 36);
            this.pnlPequenoBuscar.TabIndex = 94;
            this.pnlPequenoBuscar.Text = "panel8";
            //
            // lblPequenaBuscar
            //
            this.lblPequenaBuscar.Font = new System.Drawing.Font("Inter", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPequenaBuscar.ForeColor = System.Drawing.Color.White;
            this.lblPequenaBuscar.Location = new System.Drawing.Point(4, 4);
            this.lblPequenaBuscar.Name = "lblPequenaBuscar";
            this.lblPequenaBuscar.Size = new System.Drawing.Size(204, 24);
            this.lblPequenaBuscar.TabIndex = 2;
            this.lblPequenaBuscar.Text = "Buscar por nombre";
            //
            // frmSeleccionarWeb
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lavender;
            this.ClientSize = new System.Drawing.Size(1377, 721);
            this.Controls.Add(this.pnlPequenoLink);
            this.Controls.Add(this.pnlLayoutSeleccionadas);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.txtLink);
            this.Controls.Add(this.txtBuscar);
            this.Controls.Add(this.btnManual);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.pnlLayoutBusqueda);
            this.Controls.Add(this.pnlPequenoBuscar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmSeleccionarWeb";
            this.Text = "frmSeleccionarWeb";
            this.pnlPequenoLink.ResumeLayout(false);
            this.pnlLayoutSeleccionadas.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            this.pnlLayoutBusqueda.ResumeLayout(false);
            this.pnlPequenoBuscar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblPequenaBuscar;
        private BufferedFlowLayoutPanel flWebSeleccionadas;
        private BufferedFlowLayoutPanel flBusqueda;
        private System.Windows.Forms.Label lblPanelTop;
        private AntdUI.Button btnCancelar;
        private AntdUI.Button btnAceptar;
        private AntdUI.Button btnManual;
        private AntdUI.Input txtBuscar;
        private AntdUI.Input txtLink;
        private System.Windows.Forms.Panel pnlTop;
        private AntdUI.Button btnCerrar;
        private AntdUI.Panel pnlLayoutBusqueda;
        private AntdUI.Panel pnlLayoutSeleccionadas;
        private AntdUI.Panel pnlPequenoBuscar;
        private AntdUI.Panel pnlPequenoLink;
        private System.Windows.Forms.Label lblPequenaLink;
    }
}
