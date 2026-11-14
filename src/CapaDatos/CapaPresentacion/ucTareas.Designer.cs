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
            this.pnlTargetaTarea = new AntdUI.Panel();
            this.btnBorrar = new AntdUI.Button();
            this.lblTarea = new System.Windows.Forms.Label();
            this.lblHoraFin = new System.Windows.Forms.Label();
            this.lblHoraInicio = new System.Windows.Forms.Label();
            this.btnEditar = new AntdUI.Button();
            this.pnlTargetaTarea.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlTargetaTarea
            //
            this.pnlTargetaTarea.Back = System.Drawing.SystemColors.ActiveCaption;
            this.pnlTargetaTarea.BackColor = System.Drawing.Color.Transparent;
            this.pnlTargetaTarea.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlTargetaTarea.BorderWidth = 0.75F;
            this.pnlTargetaTarea.Controls.Add(this.btnEditar);
            this.pnlTargetaTarea.Controls.Add(this.btnBorrar);
            this.pnlTargetaTarea.Controls.Add(this.lblTarea);
            this.pnlTargetaTarea.Controls.Add(this.lblHoraFin);
            this.pnlTargetaTarea.Controls.Add(this.lblHoraInicio);
            this.pnlTargetaTarea.Location = new System.Drawing.Point(0, 12);
            this.pnlTargetaTarea.Name = "pnlTargetaTarea";
            this.pnlTargetaTarea.Radius = 8;
            this.pnlTargetaTarea.Size = new System.Drawing.Size(288, 108);
            this.pnlTargetaTarea.TabIndex = 54;
            this.pnlTargetaTarea.Text = "panel2";
            //
            // btnBorrar
            //
            this.btnBorrar.DefaultBack = System.Drawing.Color.Brown;
            this.btnBorrar.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBorrar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnBorrar.Location = new System.Drawing.Point(264, -12);
            this.btnBorrar.Name = "btnBorrar";
            this.btnBorrar.Size = new System.Drawing.Size(36, 36);
            this.btnBorrar.TabIndex = 34;
            this.btnBorrar.Text = "-";
            //
            // lblTarea
            //
            this.lblTarea.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTarea.Location = new System.Drawing.Point(12, 12);
            this.lblTarea.Name = "lblTarea";
            this.lblTarea.Size = new System.Drawing.Size(276, 24);
            this.lblTarea.TabIndex = 0;
            this.lblTarea.Text = "Tarea";
            this.lblTarea.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblHoraFin
            //
            this.lblHoraFin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHoraFin.Location = new System.Drawing.Point(21, 72);
            this.lblHoraFin.Name = "lblHoraFin";
            this.lblHoraFin.Size = new System.Drawing.Size(228, 24);
            this.lblHoraFin.TabIndex = 2;
            this.lblHoraFin.Text = "Fin";
            //
            // lblHoraInicio
            //
            this.lblHoraInicio.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHoraInicio.Location = new System.Drawing.Point(21, 48);
            this.lblHoraInicio.Name = "lblHoraInicio";
            this.lblHoraInicio.Size = new System.Drawing.Size(228, 24);
            this.lblHoraInicio.TabIndex = 1;
            this.lblHoraInicio.Text = "Inicio";
            //
            // btnEditar
            //
            this.btnEditar.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(193)))), ((int)(((byte)(7)))));
            this.btnEditar.Font = new System.Drawing.Font("Inter", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEditar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnEditar.Location = new System.Drawing.Point(264, 84);
            this.btnEditar.Name = "btnEditar";
            this.btnEditar.Size = new System.Drawing.Size(36, 36);
            this.btnEditar.TabIndex = 35;
            this.btnEditar.Text = "✏️";
            this.btnEditar.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            //
            // ucTareas
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlTargetaTarea);
            this.Name = "ucTareas";
            this.Size = new System.Drawing.Size(292, 129);
            this.pnlTargetaTarea.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private AntdUI.Panel pnlTargetaTarea;
        internal System.Windows.Forms.Label lblTarea;
        internal System.Windows.Forms.Label lblHoraFin;
        internal System.Windows.Forms.Label lblHoraInicio;
        private AntdUI.Button btnBorrar;
        private AntdUI.Button btnEditar;
    }
}
