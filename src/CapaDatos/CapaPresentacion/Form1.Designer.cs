namespace CapaPresentacion
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pestanas = new System.Windows.Forms.TabControl();
            this.tabAcceso = new System.Windows.Forms.TabPage();
            this.tabRegistro = new System.Windows.Forms.TabPage();
            this.tabVerificacion = new System.Windows.Forms.TabPage();
            this.lblCorreo = new System.Windows.Forms.Label();
            this.correo = new System.Windows.Forms.TextBox();
            this.lblClave = new System.Windows.Forms.Label();
            this.clave = new System.Windows.Forms.TextBox();
            this.btnEntrar = new System.Windows.Forms.Button();
            this.lblAcceso = new System.Windows.Forms.Label();
            this.btnVerificarCorreo = new System.Windows.Forms.Button();
            this.lblNombre = new System.Windows.Forms.Label();
            this.nombre = new System.Windows.Forms.TextBox();
            this.lblCorreoRegistro = new System.Windows.Forms.Label();
            this.correoRegistro = new System.Windows.Forms.TextBox();
            this.lblClaveRegistro = new System.Windows.Forms.Label();
            this.contrasenaRegistro = new System.Windows.Forms.TextBox();
            this.lblConfirmar = new System.Windows.Forms.Label();
            this.confirmar = new System.Windows.Forms.TextBox();
            this.btnRegistrar = new System.Windows.Forms.Button();
            this.lblRegistro = new System.Windows.Forms.Label();
            this.lblCorreoVerificacion = new System.Windows.Forms.Label();
            this.correoVerificacion = new System.Windows.Forms.TextBox();
            this.lblCodigo = new System.Windows.Forms.Label();
            this.codigoVerificacion = new System.Windows.Forms.TextBox();
            this.btnEnviarCodigo = new System.Windows.Forms.Button();
            this.btnConfirmarCodigo = new System.Windows.Forms.Button();
            this.lblVigencia = new System.Windows.Forms.Label();
            this.estadoVerificacion = new System.Windows.Forms.Label();
            this.pestanas.SuspendLayout();
            this.tabAcceso.SuspendLayout();
            this.tabRegistro.SuspendLayout();
            this.tabVerificacion.SuspendLayout();
            this.SuspendLayout();
            // pestanas
            this.pestanas.Controls.Add(this.tabAcceso);
            this.pestanas.Controls.Add(this.tabRegistro);
            this.pestanas.Controls.Add(this.tabVerificacion);
            this.pestanas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pestanas.Location = new System.Drawing.Point(0, 0);
            this.pestanas.Name = "pestanas";
            this.pestanas.SelectedIndex = 0;
            this.pestanas.Size = new System.Drawing.Size(560, 600);
            this.pestanas.TabIndex = 0;
            this.pestanas.SelectedIndexChanged += new System.EventHandler(this.pestanas_SelectedIndexChanged);
            // tabAcceso
            this.tabAcceso.AutoScroll = true;
            this.tabAcceso.Name = "tabAcceso";
            this.tabAcceso.Padding = new System.Windows.Forms.Padding(3);
            this.tabAcceso.Size = new System.Drawing.Size(552, 566);
            this.tabAcceso.TabIndex = 0;
            this.tabAcceso.Text = "Iniciar sesión";
            this.tabAcceso.UseVisualStyleBackColor = true;
            this.tabAcceso.Controls.Add(this.lblCorreo);
            this.tabAcceso.Controls.Add(this.correo);
            this.tabAcceso.Controls.Add(this.lblClave);
            this.tabAcceso.Controls.Add(this.clave);
            this.tabAcceso.Controls.Add(this.btnEntrar);
            this.tabAcceso.Controls.Add(this.lblAcceso);
            this.tabAcceso.Controls.Add(this.btnVerificarCorreo);
            // tabRegistro
            this.tabRegistro.AutoScroll = true;
            this.tabRegistro.Name = "tabRegistro";
            this.tabRegistro.Padding = new System.Windows.Forms.Padding(3);
            this.tabRegistro.Size = new System.Drawing.Size(552, 566);
            this.tabRegistro.TabIndex = 1;
            this.tabRegistro.Text = "Registro";
            this.tabRegistro.UseVisualStyleBackColor = true;
            this.tabRegistro.Controls.Add(this.lblNombre);
            this.tabRegistro.Controls.Add(this.nombre);
            this.tabRegistro.Controls.Add(this.lblCorreoRegistro);
            this.tabRegistro.Controls.Add(this.correoRegistro);
            this.tabRegistro.Controls.Add(this.lblClaveRegistro);
            this.tabRegistro.Controls.Add(this.contrasenaRegistro);
            this.tabRegistro.Controls.Add(this.lblConfirmar);
            this.tabRegistro.Controls.Add(this.confirmar);
            this.tabRegistro.Controls.Add(this.btnRegistrar);
            this.tabRegistro.Controls.Add(this.lblRegistro);
            // tabVerificacion
            this.tabVerificacion.AutoScroll = true;
            this.tabVerificacion.Name = "tabVerificacion";
            this.tabVerificacion.Padding = new System.Windows.Forms.Padding(3);
            this.tabVerificacion.Size = new System.Drawing.Size(552, 566);
            this.tabVerificacion.TabIndex = 2;
            this.tabVerificacion.Text = "Verificar correo";
            this.tabVerificacion.UseVisualStyleBackColor = true;
            this.tabVerificacion.Controls.Add(this.lblCorreoVerificacion);
            this.tabVerificacion.Controls.Add(this.correoVerificacion);
            this.tabVerificacion.Controls.Add(this.lblCodigo);
            this.tabVerificacion.Controls.Add(this.codigoVerificacion);
            this.tabVerificacion.Controls.Add(this.btnEnviarCodigo);
            this.tabVerificacion.Controls.Add(this.btnConfirmarCodigo);
            this.tabVerificacion.Controls.Add(this.lblVigencia);
            this.tabVerificacion.Controls.Add(this.estadoVerificacion);
            // lblCorreo
            this.lblCorreo.Location = new System.Drawing.Point(24, 24);
            this.lblCorreo.Name = "lblCorreo";
            this.lblCorreo.Size = new System.Drawing.Size(490, 24);
            this.lblCorreo.TabIndex = 0;
            this.lblCorreo.Text = "Correo electrónico";
            this.lblCorreo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            // correo
            this.correo.Location = new System.Drawing.Point(24, 54);
            this.correo.Name = "correo";
            this.correo.Size = new System.Drawing.Size(490, 28);
            this.correo.TabIndex = 1;
            this.correo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.correo.MaxLength = 254;
            // lblClave
            this.lblClave.Location = new System.Drawing.Point(24, 106);
            this.lblClave.Name = "lblClave";
            this.lblClave.Size = new System.Drawing.Size(490, 24);
            this.lblClave.TabIndex = 2;
            this.lblClave.Text = "Contraseña";
            this.lblClave.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            // clave
            this.clave.Location = new System.Drawing.Point(24, 136);
            this.clave.Name = "clave";
            this.clave.Size = new System.Drawing.Size(490, 28);
            this.clave.TabIndex = 3;
            this.clave.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.clave.MaxLength = 128;
            this.clave.UseSystemPasswordChar = true;
            // btnEntrar
            this.btnEntrar.Location = new System.Drawing.Point(24, 196);
            this.btnEntrar.Name = "btnEntrar";
            this.btnEntrar.Size = new System.Drawing.Size(190, 38);
            this.btnEntrar.TabIndex = 4;
            this.btnEntrar.Text = "Iniciar sesión";
            this.btnEntrar.UseVisualStyleBackColor = true;
            this.btnEntrar.Click += new System.EventHandler(this.btnEntrar_Click);
            // lblAcceso
            this.lblAcceso.Location = new System.Drawing.Point(24, 260);
            this.lblAcceso.Name = "lblAcceso";
            this.lblAcceso.Size = new System.Drawing.Size(490, 60);
            this.lblAcceso.TabIndex = 5;
            this.lblAcceso.Text = "Las cuentas nuevas deben verificar su correo antes de iniciar sesión.";
            this.lblAcceso.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            // btnVerificarCorreo
            this.btnVerificarCorreo.Location = new System.Drawing.Point(24, 330);
            this.btnVerificarCorreo.Name = "btnVerificarCorreo";
            this.btnVerificarCorreo.Size = new System.Drawing.Size(190, 38);
            this.btnVerificarCorreo.TabIndex = 6;
            this.btnVerificarCorreo.Text = "Verificar mi correo";
            this.btnVerificarCorreo.UseVisualStyleBackColor = true;
            this.btnVerificarCorreo.Click += new System.EventHandler(this.btnVerificarCorreo_Click);
            // lblNombre
            this.lblNombre.Location = new System.Drawing.Point(24, 20);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(490, 24);
            this.lblNombre.TabIndex = 7;
            this.lblNombre.Text = "Nombre";
            this.lblNombre.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            // nombre
            this.nombre.Location = new System.Drawing.Point(24, 48);
            this.nombre.Name = "nombre";
            this.nombre.Size = new System.Drawing.Size(490, 28);
            this.nombre.TabIndex = 8;
            this.nombre.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.nombre.MaxLength = 50;
            // lblCorreoRegistro
            this.lblCorreoRegistro.Location = new System.Drawing.Point(24, 92);
            this.lblCorreoRegistro.Name = "lblCorreoRegistro";
            this.lblCorreoRegistro.Size = new System.Drawing.Size(490, 24);
            this.lblCorreoRegistro.TabIndex = 9;
            this.lblCorreoRegistro.Text = "Correo electrónico";
            this.lblCorreoRegistro.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            // correoRegistro
            this.correoRegistro.Location = new System.Drawing.Point(24, 120);
            this.correoRegistro.Name = "correoRegistro";
            this.correoRegistro.Size = new System.Drawing.Size(490, 28);
            this.correoRegistro.TabIndex = 10;
            this.correoRegistro.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.correoRegistro.MaxLength = 254;
            // lblClaveRegistro
            this.lblClaveRegistro.Location = new System.Drawing.Point(24, 164);
            this.lblClaveRegistro.Name = "lblClaveRegistro";
            this.lblClaveRegistro.Size = new System.Drawing.Size(490, 24);
            this.lblClaveRegistro.TabIndex = 11;
            this.lblClaveRegistro.Text = "Contraseña (8–128 caracteres)";
            this.lblClaveRegistro.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            // contrasenaRegistro
            this.contrasenaRegistro.Location = new System.Drawing.Point(24, 192);
            this.contrasenaRegistro.Name = "contrasenaRegistro";
            this.contrasenaRegistro.Size = new System.Drawing.Size(490, 28);
            this.contrasenaRegistro.TabIndex = 12;
            this.contrasenaRegistro.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.contrasenaRegistro.MaxLength = 128;
            this.contrasenaRegistro.UseSystemPasswordChar = true;
            // lblConfirmar
            this.lblConfirmar.Location = new System.Drawing.Point(24, 236);
            this.lblConfirmar.Name = "lblConfirmar";
            this.lblConfirmar.Size = new System.Drawing.Size(490, 24);
            this.lblConfirmar.TabIndex = 13;
            this.lblConfirmar.Text = "Confirmar contraseña";
            this.lblConfirmar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            // confirmar
            this.confirmar.Location = new System.Drawing.Point(24, 264);
            this.confirmar.Name = "confirmar";
            this.confirmar.Size = new System.Drawing.Size(490, 28);
            this.confirmar.TabIndex = 14;
            this.confirmar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.confirmar.MaxLength = 128;
            this.confirmar.UseSystemPasswordChar = true;
            // btnRegistrar
            this.btnRegistrar.Location = new System.Drawing.Point(24, 324);
            this.btnRegistrar.Name = "btnRegistrar";
            this.btnRegistrar.Size = new System.Drawing.Size(190, 38);
            this.btnRegistrar.TabIndex = 15;
            this.btnRegistrar.Text = "Registrar usuario";
            this.btnRegistrar.UseVisualStyleBackColor = true;
            this.btnRegistrar.Click += new System.EventHandler(this.btnRegistrar_Click);
            // lblRegistro
            this.lblRegistro.Location = new System.Drawing.Point(24, 390);
            this.lblRegistro.Name = "lblRegistro";
            this.lblRegistro.Size = new System.Drawing.Size(490, 60);
            this.lblRegistro.TabIndex = 16;
            this.lblRegistro.Text = "Se enviará un código para comprobar su correo electrónico.";
            this.lblRegistro.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            // lblCorreoVerificacion
            this.lblCorreoVerificacion.Location = new System.Drawing.Point(24, 24);
            this.lblCorreoVerificacion.Name = "lblCorreoVerificacion";
            this.lblCorreoVerificacion.Size = new System.Drawing.Size(490, 24);
            this.lblCorreoVerificacion.TabIndex = 17;
            this.lblCorreoVerificacion.Text = "Correo electrónico de la cuenta";
            this.lblCorreoVerificacion.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            // correoVerificacion
            this.correoVerificacion.Location = new System.Drawing.Point(24, 54);
            this.correoVerificacion.Name = "correoVerificacion";
            this.correoVerificacion.Size = new System.Drawing.Size(490, 28);
            this.correoVerificacion.TabIndex = 18;
            this.correoVerificacion.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.correoVerificacion.MaxLength = 254;
            // lblCodigo
            this.lblCodigo.Location = new System.Drawing.Point(24, 106);
            this.lblCodigo.Name = "lblCodigo";
            this.lblCodigo.Size = new System.Drawing.Size(490, 24);
            this.lblCodigo.TabIndex = 19;
            this.lblCodigo.Text = "Código de seis dígitos";
            this.lblCodigo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            // codigoVerificacion
            this.codigoVerificacion.Location = new System.Drawing.Point(24, 136);
            this.codigoVerificacion.Name = "codigoVerificacion";
            this.codigoVerificacion.Size = new System.Drawing.Size(490, 28);
            this.codigoVerificacion.TabIndex = 20;
            this.codigoVerificacion.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.codigoVerificacion.MaxLength = 6;
            // btnEnviarCodigo
            this.btnEnviarCodigo.Location = new System.Drawing.Point(24, 196);
            this.btnEnviarCodigo.Name = "btnEnviarCodigo";
            this.btnEnviarCodigo.Size = new System.Drawing.Size(240, 38);
            this.btnEnviarCodigo.TabIndex = 21;
            this.btnEnviarCodigo.Text = "Solicitar / reenviar código";
            this.btnEnviarCodigo.UseVisualStyleBackColor = true;
            this.btnEnviarCodigo.Click += new System.EventHandler(this.btnEnviarCodigo_Click);
            // btnConfirmarCodigo
            this.btnConfirmarCodigo.Location = new System.Drawing.Point(24, 250);
            this.btnConfirmarCodigo.Name = "btnConfirmarCodigo";
            this.btnConfirmarCodigo.Size = new System.Drawing.Size(240, 38);
            this.btnConfirmarCodigo.TabIndex = 22;
            this.btnConfirmarCodigo.Text = "Verificar correo";
            this.btnConfirmarCodigo.UseVisualStyleBackColor = true;
            this.btnConfirmarCodigo.Click += new System.EventHandler(this.btnConfirmarCodigo_Click);
            // lblVigencia
            this.lblVigencia.Location = new System.Drawing.Point(24, 320);
            this.lblVigencia.Name = "lblVigencia";
            this.lblVigencia.Size = new System.Drawing.Size(490, 80);
            this.lblVigencia.TabIndex = 23;
            this.lblVigencia.Text = "El código vence en 10 minutos. Puede solicitar otro después de un minuto. Tiene un máximo de cinco intentos por código.";
            this.lblVigencia.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            // estadoVerificacion
            this.estadoVerificacion.Location = new System.Drawing.Point(24, 416);
            this.estadoVerificacion.Name = "estadoVerificacion";
            this.estadoVerificacion.Size = new System.Drawing.Size(490, 100);
            this.estadoVerificacion.TabIndex = 24;
            this.estadoVerificacion.Text = "Introduzca el correo de la cuenta para solicitar un código.";
            this.estadoVerificacion.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            // Form1
            this.AcceptButton = this.btnEntrar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 600);
            this.Controls.Add(this.pestanas);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(540, 620);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Programa de Concentración — Acceso";
            this.pestanas.ResumeLayout(false);
            this.tabAcceso.ResumeLayout(false);
            this.tabAcceso.PerformLayout();
            this.tabRegistro.ResumeLayout(false);
            this.tabRegistro.PerformLayout();
            this.tabVerificacion.ResumeLayout(false);
            this.tabVerificacion.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl pestanas;
        private System.Windows.Forms.TabPage tabAcceso;
        private System.Windows.Forms.TabPage tabRegistro;
        private System.Windows.Forms.TabPage tabVerificacion;
        private System.Windows.Forms.Label lblCorreo;
        private System.Windows.Forms.TextBox correo;
        private System.Windows.Forms.Label lblClave;
        private System.Windows.Forms.TextBox clave;
        private System.Windows.Forms.Button btnEntrar;
        private System.Windows.Forms.Label lblAcceso;
        private System.Windows.Forms.Button btnVerificarCorreo;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox nombre;
        private System.Windows.Forms.Label lblCorreoRegistro;
        private System.Windows.Forms.TextBox correoRegistro;
        private System.Windows.Forms.Label lblClaveRegistro;
        private System.Windows.Forms.TextBox contrasenaRegistro;
        private System.Windows.Forms.Label lblConfirmar;
        private System.Windows.Forms.TextBox confirmar;
        private System.Windows.Forms.Button btnRegistrar;
        private System.Windows.Forms.Label lblRegistro;
        private System.Windows.Forms.Label lblCorreoVerificacion;
        private System.Windows.Forms.TextBox correoVerificacion;
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox codigoVerificacion;
        private System.Windows.Forms.Button btnEnviarCodigo;
        private System.Windows.Forms.Button btnConfirmarCodigo;
        private System.Windows.Forms.Label lblVigencia;
        private System.Windows.Forms.Label estadoVerificacion;
    }
}
