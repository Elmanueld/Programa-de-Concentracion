using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.Win32;
using CapaEntidades;
using CapaNegocio;

namespace CapaPresentacion
{
    public partial class frmAjustes : FormularioSuave
    {
        private readonly E_Usuario usuario;
        private readonly N_Configuracion negocioConfiguracion = new N_Configuracion();
        private readonly N_Usuario negocioUsuario = new N_Usuario();
        private PreferenciasUsuario preferencias;
        private string fotoPendiente;
        private bool autoSesionOriginal;
        private bool ajustesCargados;

        // Constructor conservado para que el diseñador de Windows Forms pueda abrir el formulario.
        public frmAjustes() : this(null) { }

        public frmAjustes(E_Usuario usuario)
        {
            InitializeComponent();
            if (EnModoDiseno) return;
            IconosGoogle.Aplicar(pbPanelTop, "settings");
            IconosGoogle.Aplicar(pbSubTargetaNotiPrevia, "notification_important");
            IconosGoogle.Aplicar(pbTargetaAnuncios, "notifications");
            IconosGoogle.Aplicar(pbTargetaCuenta, "manage_accounts");
            IconosGoogle.Aplicar(pbTargetaInicio, "home");
            IconosGoogle.Aplicar(pbTargetaApariencia, "palette");
            IconosGoogle.Aplicar(btnCerrar, "close");
            IconosGoogle.Aplicar(btnGuardar, "save");
            this.usuario = usuario;
            ArrastreFormulario.Habilitar(this, pnlTop);
            btnGuardar.Click += BtnGuardar_Click;
            btnCancelar.Click += BtnCancelar_Click;
            btnCerrar.Click += (s, e) => Close();
            btnSumar.Click += (s, e) => AjustarMinutos(5);
            btnRestar.Click += (s, e) => AjustarMinutos(-5);
            btnPlantilla.Click += BtnPlantilla_Click;
            btnCambiarNombre.Click += (s, e) => CambiarDatoCuenta(true);
            btnCambiarCorreo.Click += (s, e) => CambiarDatoCuenta(false);
            btnCambiarContrasena.Click += BtnCambiarContrasena_Click;
            btnEliminarCuenta.Click += BtnEliminarCuenta_Click;
            btnCambiarFoto.Click += BtnCambiarFoto_Click;
            cbMostrarAnuncios.CheckedChanged += (s, e) => ActualizarOpcionesAnuncios();
            cbMostrarComoNoti.CheckedChanged += (s, e) => ActualizarOpcionesAnuncios();
            cbIniciarApp.CheckedChanged += (s, e) => ActualizarOpcionesInicio();
            cbMinimalizado.CheckedChanged += (s, e) => ActualizarOpcionesInicio();
            btnPersonalizar.Click += BtnPersonalizar_Click;
            cbIniciarApp.Checked = true;
            rbCalendario.Checked = true;
            cbMinimalizado.Checked = true;
            nudNotiPrevia.Text = "Avisar 0 min antes";
            ActualizarOpcionesInicio();
            if (usuario != null) CargarPreferencias();
        }


        private void CargarPreferencias()
        {
            try
            {
                preferencias = PreferenciasUsuarioApp.Cargar(usuario.IdUsuario);
                E_Configuracion configuracion = negocioConfiguracion.BuscarConfiguracion(usuario.IdUsuario);
                if (configuracion == null)
                {
                    configuracion = new E_Configuracion { IdUsuario = usuario.IdUsuario, AutoSesion = false };
                    negocioConfiguracion.InsertarConfiguracion(configuracion);
                }
                autoSesionOriginal = configuracion.AutoSesion;
                cbAutoSesion.Checked = configuracion.AutoSesion;
                cbMostrarAnuncios.Checked = preferencias.NoMostrarAnuncios;
                cbMostrarComoVentana.Checked = preferencias.MostrarComoVentana;
                cbMostrarComoNoti.Checked = preferencias.MostrarComoNotificacion;
                cbMostrarAviso.Checked = preferencias.NoMostrarAvisos;
                cbMostrarDescanso.Checked = preferencias.NoMostrarDescansos;
                nudNotiPrevia.Text = "Avisar " + preferencias.MinutosAnticipacion + " min antes";
                cbIniciarApp.Checked = preferencias.IniciarConWindows;
                cbMinimalizado.Checked = preferencias.IniciarMinimizado;
                rbCalendario.Checked = preferencias.PantallaInicio == "Calendario";
                rbTarea.Checked = preferencias.PantallaInicio == "Tarea";
                rbUltimaPantalla.Checked = preferencias.PantallaInicio == "Ultima";
                sClaroOscuro.Checked = preferencias.ModoOscuro;
                fotoPendiente = preferencias.RutaFoto;
                ActualizarOpcionesAnuncios();
                ActualizarOpcionesInicio();
                ajustesCargados = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudieron cargar los ajustes: " + ex.Message,
                    "Ajustes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (usuario == null)
            {
                MessageBox.Show(this, "No se pudo identificar la cuenta actual.", "Ajustes",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int minutos = LeerMinutos();
                E_Configuracion configuracion = negocioConfiguracion.BuscarConfiguracion(usuario.IdUsuario);
                if (configuracion == null)
                    negocioConfiguracion.InsertarConfiguracion(new E_Configuracion { IdUsuario = usuario.IdUsuario, AutoSesion = cbAutoSesion.Checked });
                else
                {
                    configuracion.AutoSesion = cbAutoSesion.Checked;
                    negocioConfiguracion.ModificarConfiguracion(configuracion);
                }

                preferencias.NoMostrarAnuncios = cbMostrarAnuncios.Checked;
                preferencias.MostrarComoVentana = cbMostrarComoVentana.Checked;
                preferencias.MostrarComoNotificacion = cbMostrarComoNoti.Checked;
                preferencias.NoMostrarAvisos = cbMostrarAviso.Checked;
                preferencias.NoMostrarDescansos = cbMostrarDescanso.Checked;
                preferencias.MinutosAnticipacion = minutos;
                preferencias.IniciarConWindows = cbIniciarApp.Checked;
                preferencias.IniciarMinimizado = cbIniciarApp.Checked && cbMinimalizado.Checked;
                preferencias.PantallaInicio = rbTarea.Checked ? "Tarea" : rbUltimaPantalla.Checked ? "Ultima" : "Calendario";
                preferencias.ModoOscuro = sClaroOscuro.Checked;
                preferencias.RutaFoto = fotoPendiente;
                PreferenciasUsuarioApp.Guardar(usuario.IdUsuario, preferencias);
                ConfigurarInicioWindows(preferencias.IniciarConWindows);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudieron guardar los ajustes: " + ex.Message,
                    "Ajustes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarInicioWindows(bool habilitado)
        {
            const string nombre = "ProgramaDeConcentracion";
            using (RegistryKey inicio = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (inicio == null) throw new InvalidOperationException("Windows no permitió actualizar el inicio automático.");
                if (habilitado) inicio.SetValue(nombre, "\"" + Application.ExecutablePath + "\" --inicio-windows");
                else inicio.DeleteValue(nombre, false);
            }
        }

        private void BtnPlantilla_Click(object sender, EventArgs e)
        {
            if (usuario == null) return;
            try
            {
                if (new N_Tarea().BuscarPlantilla(usuario.IdUsuario).HasValue &&
                    MessageBox.Show(this, "Ya existe una plantilla. Al guardar, se reemplazará por la nueva.",
                        "Plantilla de tareas", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK)
                    return;
                using (var formulario = new frmTareas(usuario.IdUsuario, DateTime.Today, true))
                    NavegacionFormularios.MostrarModal(this, formulario);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Plantilla de tareas", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CambiarDatoCuenta(bool nombre)
        {
            if (usuario == null) return;
            string valor = SolicitarTexto(nombre ? "Nuevo nombre:" : "Nuevo correo:", nombre ? usuario.Nombre : usuario.Correo);
            if (valor == null) return;
            try
            {
                var actualizado = new E_Usuario { IdUsuario = usuario.IdUsuario, Nombre = nombre ? valor.Trim() : usuario.Nombre, Correo = nombre ? usuario.Correo : valor.Trim() };
                negocioUsuario.ModificarUsuario(actualizado);
                usuario.Nombre = actualizado.Nombre;
                usuario.Correo = actualizado.Correo;
                MessageBox.Show(this, nombre ? "Nombre actualizado." : "Correo actualizado. Debes verificar el nuevo correo para volver a iniciar sesión.",
                    "Cuenta", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Cuenta", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnCambiarContrasena_Click(object sender, EventArgs e)
        {
            if (usuario == null) return;
            string actual = SolicitarTexto("Contraseña actual:", "", true);
            if (actual == null) return;
            string nueva = SolicitarTexto("Nueva contraseña (mínimo 8 caracteres):", "", true);
            if (nueva == null) return;
            string confirmacion = SolicitarTexto("Confirma la nueva contraseña:", "", true);
            if (confirmacion == null) return;

            try
            {
                negocioUsuario.IniciarSesion(usuario.Correo, actual);
            }
            catch (ArgumentException ex)
            {
                string mensaje = ex.Message.StartsWith("Credenciales incorrectas",
                    StringComparison.OrdinalIgnoreCase)
                    ? "La contraseña actual no es correcta."
                    : ex.Message;
                MessageBox.Show(this, mensaje, "Cambiar contraseña",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo verificar la contraseña actual: " + ex.Message,
                    "Cambiar contraseña", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (nueva.Length < 8) throw new ArgumentException("La contraseña debe tener al menos 8 caracteres.");
                if (nueva != confirmacion) throw new ArgumentException("Las contraseñas nuevas no coinciden.");
                negocioUsuario.CambiarContrasena(usuario.IdUsuario, SeguridadContrasena.CrearHash(nueva));
                MessageBox.Show(this, "Contraseña actualizada.", "Cuenta", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Cambiar contraseña", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnEliminarCuenta_Click(object sender, EventArgs e)
        {
            if (usuario == null) return;
            if (MessageBox.Show(this, "¿Seguro que quieres eliminar tu cuenta? Esta acción cerrará la sesión.", "Eliminar cuenta",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            string contrasena = SolicitarTexto("Confirma tu contraseña:", "", true);
            if (contrasena == null) return;
            try
            {
                negocioUsuario.IniciarSesion(usuario.Correo, contrasena);
                negocioUsuario.EliminarUsuario(usuario.IdUsuario);
                DialogResult = DialogResult.Abort;
                Close();
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "No se pudo eliminar la cuenta", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnCambiarFoto_Click(object sender, EventArgs e)
        {
            using (var dialogo = new System.Windows.Forms.OpenFileDialog { Title = "Seleccionar foto de perfil", Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp;*.gif" })
            {
                if (dialogo.ShowDialog(this) != DialogResult.OK) return;
                fotoPendiente = dialogo.FileName;
                MessageBox.Show(this, "La foto se aplicará al guardar los ajustes.", "Foto de perfil", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnPersonalizar_Click(object sender, EventArgs e)
        {
            using (var apariencia = new frmApariencia(usuario))
            {
                DialogResult resultado = NavegacionFormularios.MostrarModal(this, apariencia);
                if (resultado == DialogResult.OK && usuario != null)
                {
                    AparienciaUsuarioApp.Aplicar(usuario.IdUsuario);
                    TemaAplicacion.Aplicar(this, TemaAplicacion.ModoOscuroActual);
                }
            }
        }

        private void ActualizarOpcionesAnuncios()
        {
            bool habilitado = !cbMostrarAnuncios.Checked;
            cbMostrarComoVentana.Enabled = habilitado;
            cbMostrarComoNoti.Enabled = habilitado;
            nudNotiPrevia.Enabled = habilitado && cbMostrarComoNoti.Checked;
            btnSumar.Enabled = nudNotiPrevia.Enabled;
            btnRestar.Enabled = nudNotiPrevia.Enabled;
        }

        private void ActualizarOpcionesInicio()
        {
            cbMinimalizado.Enabled = cbIniciarApp.Checked;
            bool habilitado = cbIniciarApp.Checked && cbMinimalizado.Checked;
            rbCalendario.Enabled = habilitado;
            rbTarea.Enabled = habilitado;
            rbUltimaPantalla.Enabled = habilitado;
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (HayCambiosPendientes() && MessageBox.Show(this,
                "¿Seguro que quieres cancelar? Se perderán todos los cambios que no hayas guardado.",
                "Cancelar cambios", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            if (preferencias != null)
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool HayCambiosPendientes()
        {
            if (!ajustesCargados || preferencias == null) return false;

            int minutos;
            try { minutos = LeerMinutos(); }
            catch { return true; }

            string pantalla = rbTarea.Checked ? "Tarea" : rbUltimaPantalla.Checked ? "Ultima" : "Calendario";
            return cbAutoSesion.Checked != autoSesionOriginal ||
                cbMostrarAnuncios.Checked != preferencias.NoMostrarAnuncios ||
                cbMostrarComoVentana.Checked != preferencias.MostrarComoVentana ||
                cbMostrarComoNoti.Checked != preferencias.MostrarComoNotificacion ||
                cbMostrarAviso.Checked != preferencias.NoMostrarAvisos ||
                cbMostrarDescanso.Checked != preferencias.NoMostrarDescansos ||
                minutos != preferencias.MinutosAnticipacion ||
                cbIniciarApp.Checked != preferencias.IniciarConWindows ||
                cbMinimalizado.Checked != preferencias.IniciarMinimizado ||
                pantalla != preferencias.PantallaInicio ||
                sClaroOscuro.Checked != preferencias.ModoOscuro ||
                !String.Equals(fotoPendiente, preferencias.RutaFoto, StringComparison.OrdinalIgnoreCase);
        }

        private void AjustarMinutos(int cambio)
        {
            int valor = LeerMinutos();
            nudNotiPrevia.Text = "Avisar " + Math.Max(0, Math.Min(1440, valor + cambio)) + " min antes";
        }

        private int LeerMinutos()
        {
            string texto = (nudNotiPrevia.Text ?? "").Trim().ToLowerInvariant()
                .Replace("avisar", "").Replace("antes", "").Replace("min", "").Trim();
            int valor;
            if (!Int32.TryParse(texto, out valor) || valor < 0 || valor > 1440)
                throw new ArgumentException("Los minutos de anticipación deben estar entre 0 y 1440.");
            return valor;
        }

        private string SolicitarTexto(string mensaje, string valorInicial, bool ocultar = false)
        {
            using (var dialogo = new Form { Text = "Cuenta", FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent, MinimizeBox = false, MaximizeBox = false, ClientSize = new Size(380, 132) })
            using (var etiqueta = new Label { Text = mensaje, AutoSize = false, Location = new Point(12, 12), Size = new Size(350, 24) })
            using (var entrada = new TextBox { Text = valorInicial ?? "", Location = new Point(12, 42), Width = 350, UseSystemPasswordChar = ocultar })
            using (var aceptar = new Button { Text = "Aceptar", DialogResult = DialogResult.OK, Location = new Point(196, 84), Width = 80 })
            using (var cancelar = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Location = new Point(282, 84), Width = 80 })
            {
                dialogo.Controls.AddRange(new Control[] { etiqueta, entrada, aceptar, cancelar });
                dialogo.AcceptButton = aceptar;
                dialogo.CancelButton = cancelar;
                AjustePantalla.Preparar(dialogo);
                return dialogo.ShowDialog(this) == DialogResult.OK ? entrada.Text : null;
            }
        }
    }

    [Serializable]
    public sealed class PreferenciasUsuario
    {
        public bool NoMostrarAnuncios { get; set; }
        public bool MostrarComoVentana { get; set; } = true;
        public bool MostrarComoNotificacion { get; set; }
        public bool NoMostrarAvisos { get; set; }
        public bool NoMostrarDescansos { get; set; }
        public int MinutosAnticipacion { get; set; }
        public bool IniciarConWindows { get; set; } = true;
        public bool IniciarMinimizado { get; set; } = true;
        public string PantallaInicio { get; set; } = "Calendario";
        public DateTime UltimaFechaSeleccionada { get; set; } = DateTime.MinValue;
        public bool ModoOscuro { get; set; }
        public string RutaFoto { get; set; }
    }

    public static class PreferenciasUsuarioApp
    {
        private static string Ruta(int idUsuario)
        {
            if (idUsuario <= 0) throw new ArgumentException("El usuario no es válido.");
            string carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programa-de-Concentracion", "Usuarios", idUsuario.ToString());
            Directory.CreateDirectory(carpeta);
            return Path.Combine(carpeta, "ajustes.xml");
        }

        public static PreferenciasUsuario Cargar(int idUsuario)
        {
            string ruta = Ruta(idUsuario);
            if (!File.Exists(ruta)) return new PreferenciasUsuario();
            using (var archivo = File.OpenRead(ruta))
                return (PreferenciasUsuario)new XmlSerializer(typeof(PreferenciasUsuario)).Deserialize(archivo);
        }

        public static void Guardar(int idUsuario, PreferenciasUsuario preferencias)
        {
            string ruta = Ruta(idUsuario);
            string temporal = ruta + ".tmp";
            using (var archivo = File.Create(temporal))
                new XmlSerializer(typeof(PreferenciasUsuario)).Serialize(archivo, preferencias);
            if (File.Exists(ruta)) File.Delete(ruta);
            File.Move(temporal, ruta);
        }
    }

}
