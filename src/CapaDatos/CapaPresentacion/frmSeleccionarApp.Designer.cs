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
            this.flApps = new System.Windows.Forms.FlowLayoutPanel();
            this.chkAppsAbiertas = new System.Windows.Forms.CheckBox();
            this.btnManual = new AntdUI.Button();
            this.btnAceptar = new AntdUI.Button();
            this.btnCancelar = new AntdUI.Button();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnCerrar = new AntdUI.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtBuscar = new AntdUI.Input();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // flApps
            // 
            this.flApps.AutoScroll = true;
            this.flApps.BackColor = System.Drawing.Color.LightSteelBlue;
            this.flApps.Location = new System.Drawing.Point(12, 120);
            this.flApps.Name = "flApps";
            this.flApps.Size = new System.Drawing.Size(1236, 600);
            this.flApps.TabIndex = 0;
            // 
            // chkAppsAbiertas
            // 
            this.chkAppsAbiertas.AutoSize = true;
            this.chkAppsAbiertas.Font = new System.Drawing.Font("Inter", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkAppsAbiertas.Location = new System.Drawing.Point(684, 84);
            this.chkAppsAbiertas.Name = "chkAppsAbiertas";
            this.chkAppsAbiertas.Size = new System.Drawing.Size(238, 27);
            this.chkAppsAbiertas.TabIndex = 6;
            this.chkAppsAbiertas.Text = "Mostrar solo apps abiertas";
            this.chkAppsAbiertas.UseVisualStyleBackColor = true;
            // 
            // btnManual
            // 
            this.btnManual.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.btnManual.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManual.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnManual.Location = new System.Drawing.Point(12, 72);
            this.btnManual.Name = "btnManual";
            this.btnManual.Size = new System.Drawing.Size(300, 48);
            this.btnManual.TabIndex = 34;
            this.btnManual.Text = "Agregar Manualmente";
            this.btnManual.Click += new System.EventHandler(this.btnManual_Click);
            // 
            // btnAceptar
            // 
            this.btnAceptar.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAceptar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAceptar.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnAceptar.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(156)))), ((int)(((byte)(195)))));
            this.btnAceptar.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAceptar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnAceptar.IconRatio = 1.2F;
            this.btnAceptar.Location = new System.Drawing.Point(936, 720);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(156, 48);
            this.btnAceptar.TabIndex = 64;
            this.btnAceptar.Text = "Aceptar";
            // 
            // btnCancelar
            // 
            this.btnCancelar.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnCancelar.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnCancelar.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(156)))), ((int)(((byte)(195)))));
            this.btnCancelar.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnCancelar.IconRatio = 1.2F;
            this.btnCancelar.Location = new System.Drawing.Point(1092, 720);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(156, 48);
            this.btnCancelar.TabIndex = 65;
            this.btnCancelar.Text = "Cancelar";
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.pnlTop.Controls.Add(this.btnCerrar);
            this.pnlTop.Controls.Add(this.label3);
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new System.Windows.Forms.Padding(10);
            this.pnlTop.Size = new System.Drawing.Size(1272, 60);
            this.pnlTop.TabIndex = 72;
            // 
            // btnCerrar
            // 
            this.btnCerrar.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnCerrar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnCerrar.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnCerrar.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.btnCerrar.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCerrar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnCerrar.Icon = ((System.Drawing.Image)(resources.GetObject("btnCerrar.Icon")));
            this.btnCerrar.IconRatio = 2F;
            this.btnCerrar.Location = new System.Drawing.Point(1200, 0);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(60, 60);
            this.btnCerrar.TabIndex = 72;
            this.btnCerrar.Click += new System.EventHandler(this.BtnCerrar_Click);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Inter", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(24, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(1044, 60);
            this.label3.TabIndex = 7;
            this.label3.Text = "Selector de aplicaciones";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBuscar
            // 
            this.txtBuscar.Font = new System.Drawing.Font("Inter", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBuscar.Location = new System.Drawing.Point(924, 72);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(324, 48);
            this.txtBuscar.TabIndex = 73;
            // 
            // frmSeleccionarApp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lavender;
            this.ClientSize = new System.Drawing.Size(1263, 771);
            this.Controls.Add(this.txtBuscar);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.btnManual);
            this.Controls.Add(this.chkAppsAbiertas);
            this.Controls.Add(this.flApps);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmSeleccionarApp";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmSeleccionarApp";
            this.pnlTop.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flApps;
        private System.Windows.Forms.CheckBox chkAppsAbiertas;
        private AntdUI.Button btnManual;
        private AntdUI.Button btnAceptar;
        private AntdUI.Button btnCancelar;
        private System.Windows.Forms.Panel pnlTop;
        private AntdUI.Button btnCerrar;
        private System.Windows.Forms.Label label3;
        private AntdUI.Input txtBuscar;
    }
}
