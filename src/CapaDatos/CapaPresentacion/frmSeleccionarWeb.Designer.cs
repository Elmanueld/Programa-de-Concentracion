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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.flWebSeleccionadas = new System.Windows.Forms.FlowLayoutPanel();
            this.flBusqueda = new System.Windows.Forms.FlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.btnCancelar = new AntdUI.Button();
            this.btnAceptar = new AntdUI.Button();
            this.btnManual = new AntdUI.Button();
            this.txtBuscar = new AntdUI.Input();
            this.txtLink = new AntdUI.Input();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnCerrar = new AntdUI.Button();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(780, 96);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(216, 36);
            this.label1.TabIndex = 0;
            this.label1.Text = "Insertar con link";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(24, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(204, 36);
            this.label2.TabIndex = 2;
            this.label2.Text = "Buscar por nombre";
            // 
            // flWebSeleccionadas
            // 
            this.flWebSeleccionadas.BackColor = System.Drawing.Color.LightSteelBlue;
            this.flWebSeleccionadas.Location = new System.Drawing.Point(780, 136);
            this.flWebSeleccionadas.Name = "flWebSeleccionadas";
            this.flWebSeleccionadas.Size = new System.Drawing.Size(528, 528);
            this.flWebSeleccionadas.TabIndex = 4;
            // 
            // flBusqueda
            // 
            this.flBusqueda.AutoScroll = true;
            this.flBusqueda.BackColor = System.Drawing.Color.LightSteelBlue;
            this.flBusqueda.Location = new System.Drawing.Point(24, 136);
            this.flBusqueda.Name = "flBusqueda";
            this.flBusqueda.Size = new System.Drawing.Size(744, 528);
            this.flBusqueda.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Inter", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(24, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(1296, 60);
            this.label3.TabIndex = 7;
            this.label3.Text = "Selector de páginas web";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.btnCancelar.Location = new System.Drawing.Point(1152, 664);
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
            this.btnAceptar.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(156)))), ((int)(((byte)(195)))));
            this.btnAceptar.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAceptar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnAceptar.IconRatio = 1.2F;
            this.btnAceptar.Location = new System.Drawing.Point(996, 664);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(156, 48);
            this.btnAceptar.TabIndex = 66;
            this.btnAceptar.Text = "Aceptar";
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            // 
            // btnManual
            // 
            this.btnManual.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.btnManual.Font = new System.Drawing.Font("Inter", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManual.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnManual.Icon = ((System.Drawing.Image)(resources.GetObject("btnManual.Icon")));
            this.btnManual.IconRatio = 1F;
            this.btnManual.Location = new System.Drawing.Point(1260, 84);
            this.btnManual.Name = "btnManual";
            this.btnManual.Size = new System.Drawing.Size(48, 48);
            this.btnManual.TabIndex = 68;
            this.btnManual.Click += new System.EventHandler(this.btnAgregar_Click);
            // 
            // txtBuscar
            // 
            this.txtBuscar.Font = new System.Drawing.Font("Inter", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBuscar.Location = new System.Drawing.Point(216, 88);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(336, 48);
            this.txtBuscar.TabIndex = 69;
            // 
            // txtLink
            // 
            this.txtLink.Font = new System.Drawing.Font("Inter", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLink.Location = new System.Drawing.Point(948, 84);
            this.txtLink.Name = "txtLink";
            this.txtLink.Size = new System.Drawing.Size(312, 48);
            this.txtLink.TabIndex = 70;
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.pnlTop.Controls.Add(this.btnCerrar);
            this.pnlTop.Controls.Add(this.label3);
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new System.Windows.Forms.Padding(10);
            this.pnlTop.Size = new System.Drawing.Size(1320, 60);
            this.pnlTop.TabIndex = 71;
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
            this.btnCerrar.Location = new System.Drawing.Point(1248, 0);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(60, 60);
            this.btnCerrar.TabIndex = 72;
            this.btnCerrar.Click += new System.EventHandler(this.BtnCerrar_Click);
            // 
            // frmSeleccionarWeb
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lavender;
            this.ClientSize = new System.Drawing.Size(1320, 721);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.txtLink);
            this.Controls.Add(this.txtBuscar);
            this.Controls.Add(this.btnManual);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.flBusqueda);
            this.Controls.Add(this.flWebSeleccionadas);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmSeleccionarWeb";
            this.Text = "frmSeleccionarWeb";
            this.pnlTop.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FlowLayoutPanel flWebSeleccionadas;
        private System.Windows.Forms.FlowLayoutPanel flBusqueda;
        private System.Windows.Forms.Label label3;
        private AntdUI.Button btnCancelar;
        private AntdUI.Button btnAceptar;
        private AntdUI.Button btnManual;
        private AntdUI.Input txtBuscar;
        private AntdUI.Input txtLink;
        private System.Windows.Forms.Panel pnlTop;
        private AntdUI.Button btnCerrar;
    }
}
