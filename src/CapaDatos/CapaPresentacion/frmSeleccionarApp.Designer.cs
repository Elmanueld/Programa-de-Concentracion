namespace CapaPresentacion
{
    partial class frmSeleccionarApp
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSeleccionarApp));
            this.txtBuscar = new AntdUI.Input();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnCerrar = new AntdUI.Button();
            this.lblPanelTop = new System.Windows.Forms.Label();
            this.btnCancelar = new AntdUI.Button();
            this.btnAceptar = new AntdUI.Button();
            this.btnManual = new AntdUI.Button();
            this.chkAppsAbiertas = new System.Windows.Forms.CheckBox();
            this.pnlLayoutApp = new AntdUI.Panel();
            this.flApps = new BufferedFlowLayoutPanel();
            this.pnlTop.SuspendLayout();
            this.pnlLayoutApp.SuspendLayout();
            this.SuspendLayout();
            //
            // txtBuscar
            //
            this.txtBuscar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.txtBuscar.Font = new System.Drawing.Font("Inter", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBuscar.Location = new System.Drawing.Point(732, 72);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(372, 48);
            this.txtBuscar.TabIndex = 73;
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
            this.pnlTop.Size = new System.Drawing.Size(1200, 60);
            this.pnlTop.TabIndex = 72;
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
            this.btnCerrar.Location = new System.Drawing.Point(1128, 0);
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
            this.lblPanelTop.Size = new System.Drawing.Size(552, 60);
            this.lblPanelTop.TabIndex = 7;
            this.lblPanelTop.Text = "Selector de aplicaciones";
            this.lblPanelTop.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.btnCancelar.Location = new System.Drawing.Point(1020, 720);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(156, 48);
            this.btnCancelar.TabIndex = 65;
            this.btnCancelar.Text = "Cancelar";
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
            this.btnAceptar.Location = new System.Drawing.Point(864, 720);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(156, 48);
            this.btnAceptar.TabIndex = 64;
            this.btnAceptar.Text = "Aceptar";
            //
            // btnManual
            //
            this.btnManual.BorderWidth = 1F;
            this.btnManual.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.btnManual.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnManual.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManual.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnManual.Location = new System.Drawing.Point(12, 72);
            this.btnManual.Name = "btnManual";
            this.btnManual.Size = new System.Drawing.Size(300, 48);
            this.btnManual.TabIndex = 34;
            this.btnManual.Text = "Agregar Manualmente";
            this.btnManual.Click += new System.EventHandler(this.btnManual_Click);
            //
            // chkAppsAbiertas
            //
            this.chkAppsAbiertas.AutoSize = true;
            this.chkAppsAbiertas.Font = new System.Drawing.Font("Inter", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkAppsAbiertas.Location = new System.Drawing.Point(492, 84);
            this.chkAppsAbiertas.Name = "chkAppsAbiertas";
            this.chkAppsAbiertas.Size = new System.Drawing.Size(238, 27);
            this.chkAppsAbiertas.TabIndex = 6;
            this.chkAppsAbiertas.Text = "Mostrar solo apps abiertas";
            this.chkAppsAbiertas.UseVisualStyleBackColor = true;
            //
            // pnlLayoutApp
            //
            this.pnlLayoutApp.Back = System.Drawing.Color.LightSteelBlue;
            this.pnlLayoutApp.BackColor = System.Drawing.Color.Transparent;
            this.pnlLayoutApp.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlLayoutApp.BorderWidth = 1F;
            this.pnlLayoutApp.Controls.Add(this.flApps);
            this.pnlLayoutApp.Location = new System.Drawing.Point(12, 120);
            this.pnlLayoutApp.Name = "pnlLayoutApp";
            this.pnlLayoutApp.Radius = 12;
            this.pnlLayoutApp.Size = new System.Drawing.Size(1164, 600);
            this.pnlLayoutApp.TabIndex = 93;
            this.pnlLayoutApp.Text = "pnlLista";
            //
            // flApps
            //
            this.flApps.AutoScroll = true;
            this.flApps.BackColor = System.Drawing.Color.Transparent;
            this.flApps.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flApps.Location = new System.Drawing.Point(12, 12);
            this.flApps.Name = "flApps";
            this.flApps.Size = new System.Drawing.Size(1140, 576);
            this.flApps.TabIndex = 0;
            this.flApps.WrapContents = true;
            //
            // frmSeleccionarApp
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lavender;
            this.ClientSize = new System.Drawing.Size(1195, 771);
            this.Controls.Add(this.txtBuscar);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.btnManual);
            this.Controls.Add(this.chkAppsAbiertas);
            this.Controls.Add(this.pnlLayoutApp);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmSeleccionarApp";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmSeleccionarApp";
            this.pnlTop.ResumeLayout(false);
            this.pnlLayoutApp.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkAppsAbiertas;
        private AntdUI.Button btnManual;
        private AntdUI.Button btnAceptar;
        private AntdUI.Button btnCancelar;
        private System.Windows.Forms.Panel pnlTop;
        private AntdUI.Button btnCerrar;
        private System.Windows.Forms.Label lblPanelTop;
        private AntdUI.Input txtBuscar;
        private AntdUI.Panel pnlLayoutApp;
        private System.Windows.Forms.FlowLayoutPanel flApp;
        private BufferedFlowLayoutPanel flApps;
    }
}
