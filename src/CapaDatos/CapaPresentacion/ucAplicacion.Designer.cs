namespace CapaPresentacion
{
    partial class ucAplicacion
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
            this.pbIcono = new System.Windows.Forms.PictureBox();
            this.lblNombreApp = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnQuitar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcono)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbIcono
            // 
            this.pbIcono.Location = new System.Drawing.Point(24, 12);
            this.pbIcono.Name = "pbIcono";
            this.pbIcono.Size = new System.Drawing.Size(120, 120);
            this.pbIcono.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbIcono.TabIndex = 0;
            this.pbIcono.TabStop = false;
            // 
            // lblNombreApp
            // 
            this.lblNombreApp.BackColor = System.Drawing.Color.Transparent;
            this.lblNombreApp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNombreApp.Location = new System.Drawing.Point(0, 132);
            this.lblNombreApp.Name = "lblNombreApp";
            this.lblNombreApp.Size = new System.Drawing.Size(168, 36);
            this.lblNombreApp.TabIndex = 1;
            this.lblNombreApp.Text = "Ejemplo App";
            this.lblNombreApp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.btnQuitar);
            this.panel1.Controls.Add(this.lblNombreApp);
            this.panel1.Controls.Add(this.pbIcono);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(168, 192);
            this.panel1.TabIndex = 2;
            // 
            // btnQuitar
            // 
            this.btnQuitar.BackColor = System.Drawing.Color.Brown;
            this.btnQuitar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnQuitar.Location = new System.Drawing.Point(132, 0);
            this.btnQuitar.Name = "btnQuitar";
            this.btnQuitar.Size = new System.Drawing.Size(24, 24);
            this.btnQuitar.TabIndex = 3;
            this.btnQuitar.Text = "-";
            this.btnQuitar.UseVisualStyleBackColor = false;
            // 
            // ucAplicacion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "ucAplicacion";
            this.Size = new System.Drawing.Size(164, 197);
            ((System.ComponentModel.ISupportInitialize)(this.pbIcono)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.PictureBox pbIcono;
        internal System.Windows.Forms.Label lblNombreApp;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnQuitar;
    }
}
