namespace CapaPresentacion
{
    partial class ucTareas
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblHoraFin = new System.Windows.Forms.Label();
            this.lblHoraInicio = new System.Windows.Forms.Label();
            this.lblTarea = new System.Windows.Forms.Label();
            this.btnBorrar = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.panel1.Controls.Add(this.lblHoraFin);
            this.panel1.Controls.Add(this.lblHoraInicio);
            this.panel1.Controls.Add(this.lblTarea);
            this.panel1.Location = new System.Drawing.Point(0, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(252, 108);
            this.panel1.TabIndex = 0;
            // 
            // lblHoraFin
            // 
            this.lblHoraFin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHoraFin.Location = new System.Drawing.Point(12, 72);
            this.lblHoraFin.Name = "lblHoraFin";
            this.lblHoraFin.Size = new System.Drawing.Size(228, 24);
            this.lblHoraFin.TabIndex = 2;
            this.lblHoraFin.Text = "Fin";
            // 
            // lblHoraInicio
            // 
            this.lblHoraInicio.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHoraInicio.Location = new System.Drawing.Point(12, 48);
            this.lblHoraInicio.Name = "lblHoraInicio";
            this.lblHoraInicio.Size = new System.Drawing.Size(228, 24);
            this.lblHoraInicio.TabIndex = 1;
            this.lblHoraInicio.Text = "Inicio";
            // 
            // lblTarea
            // 
            this.lblTarea.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTarea.Location = new System.Drawing.Point(12, 12);
            this.lblTarea.Name = "lblTarea";
            this.lblTarea.Size = new System.Drawing.Size(228, 24);
            this.lblTarea.TabIndex = 0;
            this.lblTarea.Text = "Tarea";
            this.lblTarea.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnBorrar
            // 
            this.btnBorrar.BackColor = System.Drawing.Color.Brown;
            this.btnBorrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBorrar.Location = new System.Drawing.Point(240, 0);
            this.btnBorrar.Name = "btnBorrar";
            this.btnBorrar.Size = new System.Drawing.Size(24, 24);
            this.btnBorrar.TabIndex = 3;
            this.btnBorrar.Text = "-";
            this.btnBorrar.UseVisualStyleBackColor = false;
            // 
            // ucTareas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnBorrar);
            this.Controls.Add(this.panel1);
            this.Name = "ucTareas";
            this.Size = new System.Drawing.Size(266, 129);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        internal System.Windows.Forms.Label lblTarea;
        internal System.Windows.Forms.Label lblHoraFin;
        internal System.Windows.Forms.Label lblHoraInicio;
        private System.Windows.Forms.Button btnBorrar;
    }
}
