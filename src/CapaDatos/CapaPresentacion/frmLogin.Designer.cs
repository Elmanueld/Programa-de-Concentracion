namespace CapaPresentacion
{
    partial class frmLogin
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogin));
            this.pnlMarca = new AntdUI.Panel();
            this.btnTopCerrar = new AntdUI.Button();
            this.btnTopMinimizar = new AntdUI.Button();
            this.lblMarcaIcono = new AntdUI.Label();
            this.lblMarcaTitulo = new System.Windows.Forms.Label();
            this.lblMarcaSubtitulo = new System.Windows.Forms.Label();
            this.lblMarcaPie = new System.Windows.Forms.Label();
            this.pnlTarjeta = new AntdUI.Panel();
            this.lblBienvenida = new System.Windows.Forms.Label();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.pnlPaginaVerificacion = new System.Windows.Forms.Panel();
            this.lblCorreoVerificacion = new System.Windows.Forms.Label();
            this.correoVerificacion = new AntdUI.Input();
            this.lblCodigo = new System.Windows.Forms.Label();
            this.codigoVerificacion = new AntdUI.Input();
            this.btnEnviarCodigo = new AntdUI.Button();
            this.btnConfirmarCodigo = new AntdUI.Button();
            this.lblVigencia = new System.Windows.Forms.Label();
            this.estadoVerificacion = new System.Windows.Forms.Label();
            this.btnNavAcceso = new AntdUI.Button();
            this.btnNavRegistro = new AntdUI.Button();
            this.pestanas = new System.Windows.Forms.TabControl();
            this.tabAcceso = new System.Windows.Forms.TabPage();
            this.tabRegistro = new System.Windows.Forms.TabPage();
            this.tabVerificacion = new System.Windows.Forms.TabPage();
            this.pnlPaginaAcceso = new System.Windows.Forms.Panel();
            this.lblCorreo = new System.Windows.Forms.Label();
            this.correo = new AntdUI.Input();
            this.lblClave = new System.Windows.Forms.Label();
            this.clave = new AntdUI.Input();
            this.btnEntrar = new AntdUI.Button();
            this.lblAcceso = new System.Windows.Forms.Label();
            this.pnlPaginaRegistro = new System.Windows.Forms.Panel();
            this.lblNombre = new System.Windows.Forms.Label();
            this.nombre = new AntdUI.Input();
            this.lblCorreoRegistro = new System.Windows.Forms.Label();
            this.correoRegistro = new AntdUI.Input();
            this.lblClaveRegistro = new System.Windows.Forms.Label();
            this.contrasenaRegistro = new AntdUI.Input();
            this.lblConfirmar = new System.Windows.Forms.Label();
            this.confirmar = new AntdUI.Input();
            this.btnRegistrar = new AntdUI.Button();
            this.lblRegistro = new System.Windows.Forms.Label();
            this.pnlMarca.SuspendLayout();
            this.pnlTarjeta.SuspendLayout();
            this.pnlPaginaVerificacion.SuspendLayout();
            this.pestanas.SuspendLayout();
            this.pnlPaginaAcceso.SuspendLayout();
            this.pnlPaginaRegistro.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlMarca
            //
            this.pnlMarca.Back = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.pnlMarca.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlMarca.BorderWidth = 1F;
            this.pnlMarca.Controls.Add(this.btnTopCerrar);
            this.pnlMarca.Controls.Add(this.btnTopMinimizar);
            this.pnlMarca.Controls.Add(this.lblMarcaIcono);
            this.pnlMarca.Controls.Add(this.lblMarcaTitulo);
            this.pnlMarca.Controls.Add(this.lblMarcaSubtitulo);
            this.pnlMarca.Controls.Add(this.lblMarcaPie);
            this.pnlMarca.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlMarca.Location = new System.Drawing.Point(670, 0);
            this.pnlMarca.Name = "pnlMarca";
            this.pnlMarca.Radius = 0;
            this.pnlMarca.Size = new System.Drawing.Size(390, 700);
            this.pnlMarca.TabIndex = 0;
            //
            // btnTopCerrar
            //
            this.btnTopCerrar.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopCerrar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopCerrar.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopCerrar.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(156)))), ((int)(((byte)(195)))));
            this.btnTopCerrar.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTopCerrar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnTopCerrar.Icon = ((System.Drawing.Image)(resources.GetObject("btnTopCerrar.Icon")));
            this.btnTopCerrar.IconRatio = 1.5F;
            this.btnTopCerrar.Location = new System.Drawing.Point(324, 0);
            this.btnTopCerrar.Name = "btnTopCerrar";
            this.btnTopCerrar.Size = new System.Drawing.Size(60, 60);
            this.btnTopCerrar.TabIndex = 83;
            this.btnTopCerrar.Click += new System.EventHandler(this.BtnTopCerrar_Click);
            //
            // btnTopMinimizar
            //
            this.btnTopMinimizar.BackActive = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopMinimizar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopMinimizar.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnTopMinimizar.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(156)))), ((int)(((byte)(195)))));
            this.btnTopMinimizar.Font = new System.Drawing.Font("Inter", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTopMinimizar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnTopMinimizar.Icon = ((System.Drawing.Image)(resources.GetObject("btnTopMinimizar.Icon")));
            this.btnTopMinimizar.IconRatio = 1.2F;
            this.btnTopMinimizar.Location = new System.Drawing.Point(264, 0);
            this.btnTopMinimizar.Name = "btnTopMinimizar";
            this.btnTopMinimizar.Size = new System.Drawing.Size(60, 60);
            this.btnTopMinimizar.TabIndex = 82;
            this.btnTopMinimizar.Click += new System.EventHandler(this.BtnTopMinimizar_Click);
            //
            // lblMarcaIcono
            //
            this.lblMarcaIcono.BackColor = System.Drawing.Color.Transparent;
            this.lblMarcaIcono.Font = new System.Drawing.Font("Segoe UI Symbol", 30F, System.Drawing.FontStyle.Bold);
            this.lblMarcaIcono.ForeColor = System.Drawing.Color.White;
            this.lblMarcaIcono.Location = new System.Drawing.Point(288, 72);
            this.lblMarcaIcono.Name = "lblMarcaIcono";
            this.lblMarcaIcono.Size = new System.Drawing.Size(76, 76);
            this.lblMarcaIcono.TabIndex = 0;
            this.lblMarcaIcono.Text = "◎";
            this.lblMarcaIcono.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblMarcaTitulo
            //
            this.lblMarcaTitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblMarcaTitulo.Font = new System.Drawing.Font("Inter", 25F, System.Drawing.FontStyle.Bold);
            this.lblMarcaTitulo.ForeColor = System.Drawing.Color.White;
            this.lblMarcaTitulo.Location = new System.Drawing.Point(24, 168);
            this.lblMarcaTitulo.Name = "lblMarcaTitulo";
            this.lblMarcaTitulo.Size = new System.Drawing.Size(358, 152);
            this.lblMarcaTitulo.TabIndex = 1;
            this.lblMarcaTitulo.Text = "Enfócate en\nlo importante";
            this.lblMarcaTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblMarcaSubtitulo
            //
            this.lblMarcaSubtitulo.BackColor = System.Drawing.Color.Transparent;
            this.lblMarcaSubtitulo.Font = new System.Drawing.Font("Inter", 11F);
            this.lblMarcaSubtitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(233)))), ((int)(((byte)(250)))));
            this.lblMarcaSubtitulo.Location = new System.Drawing.Point(36, 360);
            this.lblMarcaSubtitulo.Name = "lblMarcaSubtitulo";
            this.lblMarcaSubtitulo.Size = new System.Drawing.Size(320, 96);
            this.lblMarcaSubtitulo.TabIndex = 2;
            this.lblMarcaSubtitulo.Text = "Organiza tus tareas y dedica tu tiempo a lo que realmente importa.";
            //
            // lblMarcaPie
            //
            this.lblMarcaPie.BackColor = System.Drawing.Color.Transparent;
            this.lblMarcaPie.Font = new System.Drawing.Font("Inter", 9F);
            this.lblMarcaPie.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(220)))), ((int)(((byte)(247)))));
            this.lblMarcaPie.Location = new System.Drawing.Point(72, 636);
            this.lblMarcaPie.Name = "lblMarcaPie";
            this.lblMarcaPie.Size = new System.Drawing.Size(308, 24);
            this.lblMarcaPie.TabIndex = 3;
            this.lblMarcaPie.Text = "PROGRAMA DE CONCENTRACIÓN";
            //
            // pnlTarjeta
            //
            this.pnlTarjeta.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTarjeta.Back = System.Drawing.Color.White;
            this.pnlTarjeta.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlTarjeta.BorderWidth = 1F;
            this.pnlTarjeta.Controls.Add(this.lblBienvenida);
            this.pnlTarjeta.Controls.Add(this.lblDescripcion);
            this.pnlTarjeta.Controls.Add(this.pnlPaginaVerificacion);
            this.pnlTarjeta.Controls.Add(this.btnNavAcceso);
            this.pnlTarjeta.Controls.Add(this.btnNavRegistro);
            this.pnlTarjeta.Controls.Add(this.pestanas);
            this.pnlTarjeta.Controls.Add(this.pnlPaginaAcceso);
            this.pnlTarjeta.Controls.Add(this.pnlPaginaRegistro);
            this.pnlTarjeta.Location = new System.Drawing.Point(36, 36);
            this.pnlTarjeta.Name = "pnlTarjeta";
            this.pnlTarjeta.Radius = 18;
            this.pnlTarjeta.Size = new System.Drawing.Size(590, 644);
            this.pnlTarjeta.TabIndex = 1;
            //
            // lblBienvenida
            //
            this.lblBienvenida.BackColor = System.Drawing.Color.Transparent;
            this.lblBienvenida.Font = new System.Drawing.Font("Inter", 20F, System.Drawing.FontStyle.Bold);
            this.lblBienvenida.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(45)))), ((int)(((byte)(63)))));
            this.lblBienvenida.Location = new System.Drawing.Point(36, 24);
            this.lblBienvenida.Name = "lblBienvenida";
            this.lblBienvenida.Size = new System.Drawing.Size(520, 56);
            this.lblBienvenida.TabIndex = 0;
            this.lblBienvenida.Text = "Bienvenido";
            //
            // lblDescripcion
            //
            this.lblDescripcion.BackColor = System.Drawing.Color.Transparent;
            this.lblDescripcion.Font = new System.Drawing.Font("Inter", 9.5F);
            this.lblDescripcion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(104)))), ((int)(((byte)(114)))), ((int)(((byte)(133)))));
            this.lblDescripcion.Location = new System.Drawing.Point(48, 72);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Size = new System.Drawing.Size(510, 30);
            this.lblDescripcion.TabIndex = 1;
            this.lblDescripcion.Text = "Inicia sesión para continuar con tus tareas.";
            //
            // pnlPaginaVerificacion
            //
            this.pnlPaginaVerificacion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlPaginaVerificacion.BackColor = System.Drawing.Color.White;
            this.pnlPaginaVerificacion.Controls.Add(this.lblCorreoVerificacion);
            this.pnlPaginaVerificacion.Controls.Add(this.correoVerificacion);
            this.pnlPaginaVerificacion.Controls.Add(this.lblCodigo);
            this.pnlPaginaVerificacion.Controls.Add(this.codigoVerificacion);
            this.pnlPaginaVerificacion.Controls.Add(this.btnEnviarCodigo);
            this.pnlPaginaVerificacion.Controls.Add(this.btnConfirmarCodigo);
            this.pnlPaginaVerificacion.Controls.Add(this.lblVigencia);
            this.pnlPaginaVerificacion.Controls.Add(this.estadoVerificacion);
            this.pnlPaginaVerificacion.Location = new System.Drawing.Point(28, 158);
            this.pnlPaginaVerificacion.Name = "pnlPaginaVerificacion";
            this.pnlPaginaVerificacion.Size = new System.Drawing.Size(534, 460);
            this.pnlPaginaVerificacion.TabIndex = 4;
            this.pnlPaginaVerificacion.Visible = false;
            //
            // lblCorreoVerificacion
            //
            this.lblCorreoVerificacion.BackColor = System.Drawing.Color.Transparent;
            this.lblCorreoVerificacion.Font = new System.Drawing.Font("Inter", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblCorreoVerificacion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblCorreoVerificacion.Location = new System.Drawing.Point(34, 24);
            this.lblCorreoVerificacion.Name = "lblCorreoVerificacion";
            this.lblCorreoVerificacion.Size = new System.Drawing.Size(460, 24);
            this.lblCorreoVerificacion.TabIndex = 0;
            this.lblCorreoVerificacion.Text = "Correo electrónico";
            this.lblCorreoVerificacion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // correoVerificacion
            //
            this.correoVerificacion.BackColor = System.Drawing.Color.White;
            this.correoVerificacion.BorderActive = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.correoVerificacion.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(220)))), ((int)(((byte)(232)))));
            this.correoVerificacion.BorderHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.correoVerificacion.Font = new System.Drawing.Font("Inter", 10F);
            this.correoVerificacion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(45)))), ((int)(((byte)(63)))));
            this.correoVerificacion.Location = new System.Drawing.Point(34, 51);
            this.correoVerificacion.MaxLength = 254;
            this.correoVerificacion.Name = "correoVerificacion";
            this.correoVerificacion.PlaceholderText = "nombre@correo.com";
            this.correoVerificacion.Radius = 9;
            this.correoVerificacion.Size = new System.Drawing.Size(460, 44);
            this.correoVerificacion.TabIndex = 1;
            //
            // lblCodigo
            //
            this.lblCodigo.BackColor = System.Drawing.Color.Transparent;
            this.lblCodigo.Font = new System.Drawing.Font("Inter", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblCodigo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblCodigo.Location = new System.Drawing.Point(34, 116);
            this.lblCodigo.Name = "lblCodigo";
            this.lblCodigo.Size = new System.Drawing.Size(460, 24);
            this.lblCodigo.TabIndex = 2;
            this.lblCodigo.Text = "Código recibido por correo";
            this.lblCodigo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // codigoVerificacion
            //
            this.codigoVerificacion.BackColor = System.Drawing.Color.White;
            this.codigoVerificacion.BorderActive = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.codigoVerificacion.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(220)))), ((int)(((byte)(232)))));
            this.codigoVerificacion.BorderHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.codigoVerificacion.Font = new System.Drawing.Font("Inter", 10F);
            this.codigoVerificacion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(45)))), ((int)(((byte)(63)))));
            this.codigoVerificacion.Location = new System.Drawing.Point(34, 143);
            this.codigoVerificacion.MaxLength = 6;
            this.codigoVerificacion.Name = "codigoVerificacion";
            this.codigoVerificacion.PlaceholderText = "Introduce los 6 dígitos";
            this.codigoVerificacion.Radius = 9;
            this.codigoVerificacion.Size = new System.Drawing.Size(460, 48);
            this.codigoVerificacion.TabIndex = 3;
            //
            // btnEnviarCodigo
            //
            this.btnEnviarCodigo.BackColor = System.Drawing.Color.White;
            this.btnEnviarCodigo.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(253)))));
            this.btnEnviarCodigo.BorderWidth = 1F;
            this.btnEnviarCodigo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEnviarCodigo.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(220)))), ((int)(((byte)(232)))));
            this.btnEnviarCodigo.Font = new System.Drawing.Font("Inter", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnEnviarCodigo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(104)))), ((int)(((byte)(180)))));
            this.btnEnviarCodigo.Location = new System.Drawing.Point(34, 210);
            this.btnEnviarCodigo.Name = "btnEnviarCodigo";
            this.btnEnviarCodigo.Radius = 9;
            this.btnEnviarCodigo.Size = new System.Drawing.Size(460, 42);
            this.btnEnviarCodigo.TabIndex = 4;
            this.btnEnviarCodigo.Text = "Reenviar código al correo";
            this.btnEnviarCodigo.Click += new System.EventHandler(this.btnEnviarCodigo_Click);
            //
            // btnConfirmarCodigo
            //
            this.btnConfirmarCodigo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.btnConfirmarCodigo.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnConfirmarCodigo.BorderWidth = 1F;
            this.btnConfirmarCodigo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConfirmarCodigo.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnConfirmarCodigo.Font = new System.Drawing.Font("Inter", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnConfirmarCodigo.ForeColor = System.Drawing.Color.White;
            this.btnConfirmarCodigo.Location = new System.Drawing.Point(34, 267);
            this.btnConfirmarCodigo.Name = "btnConfirmarCodigo";
            this.btnConfirmarCodigo.Radius = 9;
            this.btnConfirmarCodigo.Size = new System.Drawing.Size(460, 48);
            this.btnConfirmarCodigo.TabIndex = 5;
            this.btnConfirmarCodigo.Text = "Confirmar correo";
            this.btnConfirmarCodigo.Type = AntdUI.TTypeMini.Primary;
            this.btnConfirmarCodigo.Click += new System.EventHandler(this.btnConfirmarCodigo_Click);
            //
            // lblVigencia
            //
            this.lblVigencia.BackColor = System.Drawing.Color.Transparent;
            this.lblVigencia.Font = new System.Drawing.Font("Inter", 9F);
            this.lblVigencia.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(117)))), ((int)(((byte)(137)))));
            this.lblVigencia.Location = new System.Drawing.Point(34, 326);
            this.lblVigencia.Name = "lblVigencia";
            this.lblVigencia.Size = new System.Drawing.Size(460, 58);
            this.lblVigencia.TabIndex = 6;
            this.lblVigencia.Text = "El código vence en 10 minutos. Puedes solicitar otro después de un minuto y tiene" +
    "s hasta cinco intentos por código.";
            this.lblVigencia.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // estadoVerificacion
            //
            this.estadoVerificacion.BackColor = System.Drawing.Color.Transparent;
            this.estadoVerificacion.Font = new System.Drawing.Font("Inter", 9F);
            this.estadoVerificacion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(117)))), ((int)(((byte)(137)))));
            this.estadoVerificacion.Location = new System.Drawing.Point(36, 396);
            this.estadoVerificacion.Name = "estadoVerificacion";
            this.estadoVerificacion.Size = new System.Drawing.Size(480, 40);
            this.estadoVerificacion.TabIndex = 7;
            this.estadoVerificacion.Text = "Escribe el correo de tu cuenta para solicitar un código.";
            this.estadoVerificacion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // btnNavAcceso
            //
            this.btnNavAcceso.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.btnNavAcceso.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnNavAcceso.BorderWidth = 1F;
            this.btnNavAcceso.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavAcceso.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.btnNavAcceso.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnNavAcceso.Font = new System.Drawing.Font("Inter", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnNavAcceso.ForeActive = System.Drawing.Color.White;
            this.btnNavAcceso.ForeColor = System.Drawing.Color.White;
            this.btnNavAcceso.ForeHover = System.Drawing.Color.White;
            this.btnNavAcceso.Location = new System.Drawing.Point(34, 112);
            this.btnNavAcceso.Name = "btnNavAcceso";
            this.btnNavAcceso.Radius = 19;
            this.btnNavAcceso.Size = new System.Drawing.Size(250, 38);
            this.btnNavAcceso.TabIndex = 5;
            this.btnNavAcceso.Text = "Iniciar sesión";
            this.btnNavAcceso.Type = AntdUI.TTypeMini.Primary;
            this.btnNavAcceso.Click += new System.EventHandler(this.btnNavAcceso_Click);
            //
            // btnNavRegistro
            //
            this.btnNavRegistro.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            this.btnNavRegistro.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(238)))), ((int)(((byte)(249)))));
            this.btnNavRegistro.BorderWidth = 1F;
            this.btnNavRegistro.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavRegistro.DefaultBack = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            this.btnNavRegistro.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            this.btnNavRegistro.Font = new System.Drawing.Font("Inter", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnNavRegistro.ForeActive = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(52)))), ((int)(((byte)(75)))));
            this.btnNavRegistro.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(83)))), ((int)(((byte)(96)))), ((int)(((byte)(119)))));
            this.btnNavRegistro.ForeHover = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(52)))), ((int)(((byte)(75)))));
            this.btnNavRegistro.Location = new System.Drawing.Point(292, 112);
            this.btnNavRegistro.Name = "btnNavRegistro";
            this.btnNavRegistro.Radius = 19;
            this.btnNavRegistro.Size = new System.Drawing.Size(250, 38);
            this.btnNavRegistro.TabIndex = 6;
            this.btnNavRegistro.Text = "Crear cuenta";
            this.btnNavRegistro.Click += new System.EventHandler(this.btnNavRegistro_Click);
            //
            // pestanas
            //
            this.pestanas.Controls.Add(this.tabAcceso);
            this.pestanas.Controls.Add(this.tabRegistro);
            this.pestanas.Controls.Add(this.tabVerificacion);
            this.pestanas.Location = new System.Drawing.Point(0, 0);
            this.pestanas.Name = "pestanas";
            this.pestanas.SelectedIndex = 0;
            this.pestanas.Size = new System.Drawing.Size(120, 90);
            this.pestanas.TabIndex = 0;
            this.pestanas.Visible = false;
            this.pestanas.SelectedIndexChanged += new System.EventHandler(this.pestanas_SelectedIndexChanged);
            //
            // tabAcceso
            //
            this.tabAcceso.Location = new System.Drawing.Point(4, 35);
            this.tabAcceso.Name = "tabAcceso";
            this.tabAcceso.Size = new System.Drawing.Size(112, 51);
            this.tabAcceso.TabIndex = 0;
            this.tabAcceso.Text = "Iniciar sesión";
            //
            // tabRegistro
            //
            this.tabRegistro.Location = new System.Drawing.Point(4, 35);
            this.tabRegistro.Name = "tabRegistro";
            this.tabRegistro.Size = new System.Drawing.Size(112, 51);
            this.tabRegistro.TabIndex = 1;
            this.tabRegistro.Text = "Crear cuenta";
            //
            // tabVerificacion
            //
            this.tabVerificacion.Location = new System.Drawing.Point(4, 35);
            this.tabVerificacion.Name = "tabVerificacion";
            this.tabVerificacion.Size = new System.Drawing.Size(112, 51);
            this.tabVerificacion.TabIndex = 2;
            this.tabVerificacion.Text = "Código de correo";
            //
            // pnlPaginaAcceso
            //
            this.pnlPaginaAcceso.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlPaginaAcceso.BackColor = System.Drawing.Color.White;
            this.pnlPaginaAcceso.Controls.Add(this.lblCorreo);
            this.pnlPaginaAcceso.Controls.Add(this.correo);
            this.pnlPaginaAcceso.Controls.Add(this.lblClave);
            this.pnlPaginaAcceso.Controls.Add(this.clave);
            this.pnlPaginaAcceso.Controls.Add(this.btnEntrar);
            this.pnlPaginaAcceso.Controls.Add(this.lblAcceso);
            this.pnlPaginaAcceso.Location = new System.Drawing.Point(28, 158);
            this.pnlPaginaAcceso.Name = "pnlPaginaAcceso";
            this.pnlPaginaAcceso.Size = new System.Drawing.Size(534, 460);
            this.pnlPaginaAcceso.TabIndex = 2;
            //
            // lblCorreo
            //
            this.lblCorreo.BackColor = System.Drawing.Color.Transparent;
            this.lblCorreo.Font = new System.Drawing.Font("Inter", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblCorreo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblCorreo.Location = new System.Drawing.Point(34, 22);
            this.lblCorreo.Name = "lblCorreo";
            this.lblCorreo.Size = new System.Drawing.Size(460, 24);
            this.lblCorreo.TabIndex = 0;
            this.lblCorreo.Text = "Correo electrónico";
            this.lblCorreo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // correo
            //
            this.correo.BackColor = System.Drawing.Color.White;
            this.correo.BorderActive = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.correo.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(220)))), ((int)(((byte)(232)))));
            this.correo.BorderHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.correo.Font = new System.Drawing.Font("Inter", 10F);
            this.correo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(45)))), ((int)(((byte)(63)))));
            this.correo.Location = new System.Drawing.Point(34, 49);
            this.correo.MaxLength = 254;
            this.correo.Name = "correo";
            this.correo.PlaceholderText = "nombre@correo.com";
            this.correo.Radius = 9;
            this.correo.Size = new System.Drawing.Size(460, 44);
            this.correo.TabIndex = 1;
            //
            // lblClave
            //
            this.lblClave.BackColor = System.Drawing.Color.Transparent;
            this.lblClave.Font = new System.Drawing.Font("Inter", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblClave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblClave.Location = new System.Drawing.Point(34, 108);
            this.lblClave.Name = "lblClave";
            this.lblClave.Size = new System.Drawing.Size(460, 24);
            this.lblClave.TabIndex = 2;
            this.lblClave.Text = "Contraseña";
            this.lblClave.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // clave
            //
            this.clave.BackColor = System.Drawing.Color.White;
            this.clave.BorderActive = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.clave.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(220)))), ((int)(((byte)(232)))));
            this.clave.BorderHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.clave.Font = new System.Drawing.Font("Inter", 10F);
            this.clave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(45)))), ((int)(((byte)(63)))));
            this.clave.Location = new System.Drawing.Point(34, 135);
            this.clave.MaxLength = 128;
            this.clave.Name = "clave";
            this.clave.PlaceholderText = "Escribe tu contraseña";
            this.clave.Radius = 9;
            this.clave.Size = new System.Drawing.Size(460, 44);
            this.clave.TabIndex = 3;
            this.clave.UseSystemPasswordChar = true;
            //
            // btnEntrar
            //
            this.btnEntrar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.btnEntrar.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnEntrar.BorderWidth = 1F;
            this.btnEntrar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEntrar.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.btnEntrar.Font = new System.Drawing.Font("Inter", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnEntrar.ForeColor = System.Drawing.Color.White;
            this.btnEntrar.Location = new System.Drawing.Point(34, 206);
            this.btnEntrar.Name = "btnEntrar";
            this.btnEntrar.Radius = 9;
            this.btnEntrar.Size = new System.Drawing.Size(460, 48);
            this.btnEntrar.TabIndex = 4;
            this.btnEntrar.Text = "Iniciar sesión";
            this.btnEntrar.Type = AntdUI.TTypeMini.Primary;
            this.btnEntrar.Click += new System.EventHandler(this.btnEntrar_Click);
            //
            // lblAcceso
            //
            this.lblAcceso.BackColor = System.Drawing.Color.Transparent;
            this.lblAcceso.Font = new System.Drawing.Font("Inter", 9F);
            this.lblAcceso.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(117)))), ((int)(((byte)(137)))));
            this.lblAcceso.Location = new System.Drawing.Point(34, 274);
            this.lblAcceso.Name = "lblAcceso";
            this.lblAcceso.Size = new System.Drawing.Size(460, 62);
            this.lblAcceso.TabIndex = 5;
            this.lblAcceso.Text = "Las cuentas nuevas deben verificar su correo antes de iniciar sesión.";
            this.lblAcceso.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlPaginaRegistro
            //
            this.pnlPaginaRegistro.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlPaginaRegistro.BackColor = System.Drawing.Color.White;
            this.pnlPaginaRegistro.Controls.Add(this.lblNombre);
            this.pnlPaginaRegistro.Controls.Add(this.nombre);
            this.pnlPaginaRegistro.Controls.Add(this.lblCorreoRegistro);
            this.pnlPaginaRegistro.Controls.Add(this.correoRegistro);
            this.pnlPaginaRegistro.Controls.Add(this.lblClaveRegistro);
            this.pnlPaginaRegistro.Controls.Add(this.contrasenaRegistro);
            this.pnlPaginaRegistro.Controls.Add(this.lblConfirmar);
            this.pnlPaginaRegistro.Controls.Add(this.confirmar);
            this.pnlPaginaRegistro.Controls.Add(this.btnRegistrar);
            this.pnlPaginaRegistro.Controls.Add(this.lblRegistro);
            this.pnlPaginaRegistro.Location = new System.Drawing.Point(28, 158);
            this.pnlPaginaRegistro.Name = "pnlPaginaRegistro";
            this.pnlPaginaRegistro.Size = new System.Drawing.Size(534, 460);
            this.pnlPaginaRegistro.TabIndex = 3;
            this.pnlPaginaRegistro.Visible = false;
            //
            // lblNombre
            //
            this.lblNombre.BackColor = System.Drawing.Color.Transparent;
            this.lblNombre.Font = new System.Drawing.Font("Inter", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblNombre.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblNombre.Location = new System.Drawing.Point(34, 16);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(460, 24);
            this.lblNombre.TabIndex = 0;
            this.lblNombre.Text = "Nombre";
            this.lblNombre.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // nombre
            //
            this.nombre.BackColor = System.Drawing.Color.White;
            this.nombre.BorderActive = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.nombre.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(220)))), ((int)(((byte)(232)))));
            this.nombre.BorderHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.nombre.Font = new System.Drawing.Font("Inter", 10F);
            this.nombre.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(45)))), ((int)(((byte)(63)))));
            this.nombre.Location = new System.Drawing.Point(34, 43);
            this.nombre.MaxLength = 50;
            this.nombre.Name = "nombre";
            this.nombre.PlaceholderText = "Tu nombre";
            this.nombre.Radius = 9;
            this.nombre.Size = new System.Drawing.Size(460, 40);
            this.nombre.TabIndex = 1;
            //
            // lblCorreoRegistro
            //
            this.lblCorreoRegistro.BackColor = System.Drawing.Color.Transparent;
            this.lblCorreoRegistro.Font = new System.Drawing.Font("Inter", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblCorreoRegistro.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblCorreoRegistro.Location = new System.Drawing.Point(34, 88);
            this.lblCorreoRegistro.Name = "lblCorreoRegistro";
            this.lblCorreoRegistro.Size = new System.Drawing.Size(460, 24);
            this.lblCorreoRegistro.TabIndex = 2;
            this.lblCorreoRegistro.Text = "Correo electrónico";
            this.lblCorreoRegistro.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // correoRegistro
            //
            this.correoRegistro.BackColor = System.Drawing.Color.White;
            this.correoRegistro.BorderActive = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.correoRegistro.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(220)))), ((int)(((byte)(232)))));
            this.correoRegistro.BorderHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.correoRegistro.Font = new System.Drawing.Font("Inter", 10F);
            this.correoRegistro.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(45)))), ((int)(((byte)(63)))));
            this.correoRegistro.Location = new System.Drawing.Point(34, 115);
            this.correoRegistro.MaxLength = 254;
            this.correoRegistro.Name = "correoRegistro";
            this.correoRegistro.PlaceholderText = "nombre@correo.com";
            this.correoRegistro.Radius = 9;
            this.correoRegistro.Size = new System.Drawing.Size(460, 40);
            this.correoRegistro.TabIndex = 3;
            //
            // lblClaveRegistro
            //
            this.lblClaveRegistro.BackColor = System.Drawing.Color.Transparent;
            this.lblClaveRegistro.Font = new System.Drawing.Font("Inter", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblClaveRegistro.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblClaveRegistro.Location = new System.Drawing.Point(34, 160);
            this.lblClaveRegistro.Name = "lblClaveRegistro";
            this.lblClaveRegistro.Size = new System.Drawing.Size(460, 24);
            this.lblClaveRegistro.TabIndex = 4;
            this.lblClaveRegistro.Text = "Contraseña (8–128 caracteres)";
            this.lblClaveRegistro.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // contrasenaRegistro
            //
            this.contrasenaRegistro.BackColor = System.Drawing.Color.White;
            this.contrasenaRegistro.BorderActive = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.contrasenaRegistro.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(220)))), ((int)(((byte)(232)))));
            this.contrasenaRegistro.BorderHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.contrasenaRegistro.Font = new System.Drawing.Font("Inter", 10F);
            this.contrasenaRegistro.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(45)))), ((int)(((byte)(63)))));
            this.contrasenaRegistro.Location = new System.Drawing.Point(34, 187);
            this.contrasenaRegistro.MaxLength = 128;
            this.contrasenaRegistro.Name = "contrasenaRegistro";
            this.contrasenaRegistro.PlaceholderText = "Crea una contraseña";
            this.contrasenaRegistro.Radius = 9;
            this.contrasenaRegistro.Size = new System.Drawing.Size(460, 40);
            this.contrasenaRegistro.TabIndex = 5;
            this.contrasenaRegistro.UseSystemPasswordChar = true;
            //
            // lblConfirmar
            //
            this.lblConfirmar.BackColor = System.Drawing.Color.Transparent;
            this.lblConfirmar.Font = new System.Drawing.Font("Inter", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblConfirmar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.lblConfirmar.Location = new System.Drawing.Point(34, 232);
            this.lblConfirmar.Name = "lblConfirmar";
            this.lblConfirmar.Size = new System.Drawing.Size(460, 24);
            this.lblConfirmar.TabIndex = 6;
            this.lblConfirmar.Text = "Confirmar contraseña";
            this.lblConfirmar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // confirmar
            //
            this.confirmar.BackColor = System.Drawing.Color.White;
            this.confirmar.BorderActive = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.confirmar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(220)))), ((int)(((byte)(232)))));
            this.confirmar.BorderHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.confirmar.Font = new System.Drawing.Font("Inter", 10F);
            this.confirmar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(45)))), ((int)(((byte)(63)))));
            this.confirmar.Location = new System.Drawing.Point(34, 259);
            this.confirmar.MaxLength = 128;
            this.confirmar.Name = "confirmar";
            this.confirmar.PlaceholderText = "Repite la contraseña";
            this.confirmar.Radius = 9;
            this.confirmar.Size = new System.Drawing.Size(460, 40);
            this.confirmar.TabIndex = 7;
            this.confirmar.UseSystemPasswordChar = true;
            //
            // btnRegistrar
            //
            this.btnRegistrar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.btnRegistrar.BackHover = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(133)))), ((int)(((byte)(225)))));
            this.btnRegistrar.BorderWidth = 1F;
            this.btnRegistrar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRegistrar.DefaultBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(119)))), ((int)(((byte)(209)))));
            this.btnRegistrar.Font = new System.Drawing.Font("Inter", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnRegistrar.ForeColor = System.Drawing.Color.White;
            this.btnRegistrar.Location = new System.Drawing.Point(34, 318);
            this.btnRegistrar.Name = "btnRegistrar";
            this.btnRegistrar.Radius = 9;
            this.btnRegistrar.Size = new System.Drawing.Size(460, 46);
            this.btnRegistrar.TabIndex = 8;
            this.btnRegistrar.Text = "Crear cuenta";
            this.btnRegistrar.Type = AntdUI.TTypeMini.Primary;
            this.btnRegistrar.Click += new System.EventHandler(this.btnRegistrar_Click);
            //
            // lblRegistro
            //
            this.lblRegistro.BackColor = System.Drawing.Color.Transparent;
            this.lblRegistro.Font = new System.Drawing.Font("Inter", 9F);
            this.lblRegistro.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(117)))), ((int)(((byte)(137)))));
            this.lblRegistro.Location = new System.Drawing.Point(34, 376);
            this.lblRegistro.Name = "lblRegistro";
            this.lblRegistro.Size = new System.Drawing.Size(460, 56);
            this.lblRegistro.TabIndex = 9;
            this.lblRegistro.Text = "Se enviará un código para comprobar tu correo electrónico.";
            this.lblRegistro.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // frmLogin
            //
            this.AcceptButton = this.btnEntrar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 26F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lavender;
            this.ClientSize = new System.Drawing.Size(1060, 700);
            this.Controls.Add(this.pnlTarjeta);
            this.Controls.Add(this.pnlMarca);
            this.Font = new System.Drawing.Font("Inter", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(980, 680);
            this.Name = "frmLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Programa de Concentración — Acceso";
            this.pnlMarca.ResumeLayout(false);
            this.pnlTarjeta.ResumeLayout(false);
            this.pnlPaginaVerificacion.ResumeLayout(false);
            this.pestanas.ResumeLayout(false);
            this.pnlPaginaAcceso.ResumeLayout(false);
            this.pnlPaginaRegistro.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private AntdUI.Panel pnlMarca;
        private AntdUI.Label lblMarcaIcono;
        private System.Windows.Forms.Label lblMarcaTitulo;
        private System.Windows.Forms.Label lblMarcaSubtitulo;
        private System.Windows.Forms.Label lblMarcaPie;
        private AntdUI.Panel pnlTarjeta;
        private System.Windows.Forms.Label lblBienvenida;
        private System.Windows.Forms.Label lblDescripcion;
        private AntdUI.Button btnNavAcceso;
        private AntdUI.Button btnNavRegistro;
        private System.Windows.Forms.Panel pnlPaginaAcceso;
        private System.Windows.Forms.Panel pnlPaginaRegistro;
        private System.Windows.Forms.Panel pnlPaginaVerificacion;
        private System.Windows.Forms.Label lblCorreo;
        private AntdUI.Input correo;
        private System.Windows.Forms.Label lblClave;
        private AntdUI.Input clave;
        private AntdUI.Button btnEntrar;
        private System.Windows.Forms.Label lblAcceso;
        private System.Windows.Forms.Label lblNombre;
        private AntdUI.Input nombre;
        private System.Windows.Forms.Label lblCorreoRegistro;
        private AntdUI.Input correoRegistro;
        private System.Windows.Forms.Label lblClaveRegistro;
        private AntdUI.Input contrasenaRegistro;
        private System.Windows.Forms.Label lblConfirmar;
        private AntdUI.Input confirmar;
        private AntdUI.Button btnRegistrar;
        private System.Windows.Forms.Label lblRegistro;
        private System.Windows.Forms.Label lblCorreoVerificacion;
        private AntdUI.Input correoVerificacion;
        private System.Windows.Forms.Label lblCodigo;
        private AntdUI.Input codigoVerificacion;
        private AntdUI.Button btnEnviarCodigo;
        private AntdUI.Button btnConfirmarCodigo;
        private System.Windows.Forms.Label lblVigencia;
        private System.Windows.Forms.Label estadoVerificacion;
        private System.Windows.Forms.TabControl pestanas;
        private System.Windows.Forms.TabPage tabAcceso;
        private System.Windows.Forms.TabPage tabRegistro;
        private System.Windows.Forms.TabPage tabVerificacion;
        private AntdUI.Button btnTopCerrar;
        private AntdUI.Button btnTopMinimizar;
    }
}
