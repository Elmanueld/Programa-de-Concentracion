namespace CapaPresentacion
{
    partial class ucWeb
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
            this.lblNombre = new System.Windows.Forms.Label();
            this.btnQuitar = new System.Windows.Forms.Button();
            this.lblLink = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcono)).BeginInit();
            this.SuspendLayout();
            // 
            // pbIcono
            // 
            this.pbIcono.Location = new System.Drawing.Point(12, 24);
            this.pbIcono.Name = "pbIcono";
            this.pbIcono.Size = new System.Drawing.Size(132, 120);
            this.pbIcono.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbIcono.TabIndex = 0;
            this.pbIcono.TabStop = false;
            // 
            // lblNombre
            // 
            this.lblNombre.Location = new System.Drawing.Point(12, 144);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(132, 24);
            this.lblNombre.TabIndex = 1;
            this.lblNombre.Text = "Nombre";
            this.lblNombre.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnQuitar
            // 
            this.btnQuitar.BackColor = System.Drawing.Color.Brown;
            this.btnQuitar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnQuitar.Location = new System.Drawing.Point(132, 12);
            this.btnQuitar.Name = "btnQuitar";
            this.btnQuitar.Size = new System.Drawing.Size(24, 24);
            this.btnQuitar.TabIndex = 2;
            this.btnQuitar.Text = "-";
            this.btnQuitar.UseVisualStyleBackColor = false;
            // 
            // lblLink
            // 
            this.lblLink.Location = new System.Drawing.Point(0, 168);
            this.lblLink.Name = "lblLink";
            this.lblLink.Size = new System.Drawing.Size(168, 24);
            this.lblLink.TabIndex = 3;
            this.lblLink.Text = "https://www.Link.com";
            this.lblLink.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ucWeb
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblLink);
            this.Controls.Add(this.btnQuitar);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.pbIcono);
            this.Name = "ucWeb";
            this.Size = new System.Drawing.Size(164, 197);
            ((System.ComponentModel.ISupportInitialize)(this.pbIcono)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbIcono;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.Button btnQuitar;
        private System.Windows.Forms.Label lblLink;
    }
}
