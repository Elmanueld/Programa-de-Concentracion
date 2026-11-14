namespace CapaPresentacion
{
    partial class ucDia
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlListTareas = new AntdUI.Panel();
            this.pnlDia = new AntdUI.Panel();
            this.lbldia = new AntdUI.Label();
            this.listTareas = new System.Windows.Forms.ListBox();
            this.pnlListTareas.SuspendLayout();
            this.pnlDia.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlListTareas
            //
            this.pnlListTareas.Back = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.pnlListTareas.BackColor = System.Drawing.Color.Transparent;
            this.pnlListTareas.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(205)))), ((int)(((byte)(232)))));
            this.pnlListTareas.BorderWidth = 0.75F;
            this.pnlListTareas.Controls.Add(this.pnlDia);
            this.pnlListTareas.Controls.Add(this.listTareas);
            this.pnlListTareas.Location = new System.Drawing.Point(0, 0);
            this.pnlListTareas.Name = "pnlListTareas";
            this.pnlListTareas.Radius = 8;
            this.pnlListTareas.Size = new System.Drawing.Size(216, 132);
            this.pnlListTareas.TabIndex = 53;
            this.pnlListTareas.Text = "panel1";
            //
            // pnlDia
            //
            this.pnlDia.Back = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.pnlDia.BackColor = System.Drawing.Color.Transparent;
            this.pnlDia.Controls.Add(this.lbldia);
            this.pnlDia.Location = new System.Drawing.Point(3, 3);
            this.pnlDia.Name = "pnlDia";
            this.pnlDia.Radius = 15;
            this.pnlDia.Size = new System.Drawing.Size(30, 30);
            this.pnlDia.TabIndex = 54;
            this.pnlDia.Text = "pnlDia";
            //
            // lbldia
            //
            this.lbldia.BackColor = System.Drawing.Color.Transparent;
            this.lbldia.Font = new System.Drawing.Font("Inter", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbldia.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lbldia.Location = new System.Drawing.Point(5, 5);
            this.lbldia.Name = "lbldia";
            this.lbldia.Size = new System.Drawing.Size(20, 20);
            this.lbldia.TabIndex = 0;
            this.lbldia.Text = "";
            this.lbldia.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbldia.Click += new System.EventHandler(this.Lbldia_Click);
            //
            // listTareas
            //
            this.listTareas.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listTareas.FormattingEnabled = true;
            this.listTareas.ItemHeight = 20;
            this.listTareas.Location = new System.Drawing.Point(12, 36);
            this.listTareas.Name = "listTareas";
            this.listTareas.Size = new System.Drawing.Size(200, 80);
            this.listTareas.TabIndex = 0;
            //
            // ucDia
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlListTareas);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ucDia";
            this.Size = new System.Drawing.Size(218, 142);
            this.pnlListTareas.ResumeLayout(false);
            this.pnlDia.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private AntdUI.Panel pnlListTareas;
        private AntdUI.Panel pnlDia;
        internal AntdUI.Label lbldia;
        internal System.Windows.Forms.ListBox listTareas;
    }
}
