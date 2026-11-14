using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaEntidades;
using CapaNegocio;

namespace CapaPresentacion
{
    public partial class frmSeleccionarApp : FormularioSuave
    {
        private readonly List<E_Aplicacion> _appsInstaladas = new List<E_Aplicacion>();
        private readonly List<E_Aplicacion> _appsManuales = new List<E_Aplicacion>();
        private readonly List<E_Aplicacion> _appsActivas = new List<E_Aplicacion>();
        private readonly HashSet<string> _idsSeleccionados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly N_Aplicacion _negocioAplicaciones = new N_Aplicacion();
        private readonly int _idUsuario;
        private readonly System.Windows.Forms.Timer _timerBusqueda;
        private readonly IndicadorCarga _estadoCarga;
        private bool _cargaIniciada;
        private int _versionAppsActivas;

        public frmSeleccionarApp() : this(0)
        {
        }

        public frmSeleccionarApp(int idUsuario)
        {
            InitializeComponent();
            if (EnModoDiseno) return;
            if (idUsuario <= 0)
                throw new ArgumentException("El ID de usuario no es válido.");
            _idUsuario = idUsuario;
            _timerBusqueda = new System.Windows.Forms.Timer();
            _estadoCarga = new IndicadorCarga();
            IconosGoogle.Aplicar(btnCerrar, "close");
            ControlesPresentacion.Optimizar(flApps);
            // El FlowLayoutPanel transparente no repinta bien el área que deja
            // libre al desplazarse sobre el panel redondeado de AntdUI.
            ActualizarFondoLista();
            ArrastreFormulario.Habilitar(this, pnlTop);
            SendMessage(txtBuscar.Handle, EM_SETCUEBANNER, 0, "Busca una aplicación...");

            btnManual.Click += btnManual_Click;
            btnAceptar.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Shown += frmSeleccionarApp_Shown;
            FormClosing += (s, e) => _timerBusqueda.Stop();

            _timerBusqueda.Interval = 300;
            _timerBusqueda.Tick += (s, e) =>
            {
                _timerBusqueda.Stop();
                MostrarAplicaciones(FiltrarAplicaciones(txtBuscar.Text));
            };
            txtBuscar.TextChanged += (s, e) =>
            {
                _timerBusqueda.Stop();
                _timerBusqueda.Start();
            };
            chkAppsAbiertas.CheckedChanged += async (s, e) =>
            {

                if (chkAppsAbiertas.Checked)
                    await ActualizarAppsActivasAsync();
                else
                {
                    _versionAppsActivas++;
                    _appsActivas.Clear();
                    MostrarAplicaciones(FiltrarAplicaciones(txtBuscar.Text));
                }
            };

            _estadoCarga.Mensaje = "Buscando aplicaciones instaladas...";
            flApps.Controls.Add(_estadoCarga);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        public List<E_Aplicacion> AplicacionesSeleccionadas
        {
            get
            {
                return _appsManuales.Concat(_appsActivas).Concat(_appsInstaladas)
                    .Where(app => _idsSeleccionados.Contains(app.Identificador))
                    .GroupBy(app => app.Identificador, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First()).ToList();
            }
        }

        private void frmSeleccionarApp_Shown(object sender, EventArgs e)
        {
            if (_cargaIniciada) return;
            _cargaIniciada = true;

            // Shell.Application es COM: ejecutamos la enumeración en un hilo STA.
            var hilo = new Thread(() =>
            {
                List<E_Aplicacion> resultado = null;
                Exception error = null;
                try { resultado = _negocioAplicaciones.BuscarAppsInstaladas(_idUsuario); }
                catch (Exception ex) { error = ex; }

                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke((Action)(async () =>
                {
                    if (flApps.Controls.Contains(_estadoCarga))
                        flApps.Controls.Remove(_estadoCarga);
                    if (!_estadoCarga.IsDisposed) _estadoCarga.Dispose();
                    if (error != null)
                    {
                        MessageBox.Show(this, error.Message, "Error al buscar aplicaciones",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    _appsInstaladas.AddRange(resultado ?? new List<E_Aplicacion>());
                    if (chkAppsAbiertas.Checked)
                        await ActualizarAppsActivasAsync();
                    else
                        MostrarAplicaciones(FiltrarAplicaciones(txtBuscar.Text));
                }));
            });
            hilo.IsBackground = true;
            hilo.Name = "Carga de aplicaciones AppsFolder";
            hilo.SetApartmentState(ApartmentState.STA);
            hilo.Start();
        }

        private IEnumerable<E_Aplicacion> FiltrarAplicaciones(string termino)
        {
            if (chkAppsAbiertas.Checked)
                return _appsActivas.OrderBy(a => a.Nombre).ToList();

            List<E_Aplicacion> resultado = String.IsNullOrWhiteSpace(termino)
                ? _appsInstaladas.OrderBy(a => a.Nombre).ToList()
                : _negocioAplicaciones.OrdenarAppsPorBusqueda(_appsInstaladas, termino);
            return _appsManuales.Concat(resultado)
                .GroupBy(a => a.Identificador, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First());
        }

        private async Task ActualizarAppsActivasAsync()
        {
            int version = ++_versionAppsActivas;
            var indicador = new IndicadorCarga { Mensaje = "Buscando aplicaciones abiertas..." };
            ControlesPresentacion.Reemplazar(flApps, indicador);
            List<E_Aplicacion> resultado = await Task.Run(ObtenerAppsActivas);
            if (IsDisposed || version != _versionAppsActivas) return;
            _appsActivas.Clear();
            _appsActivas.AddRange(resultado);
            MostrarAplicaciones(FiltrarAplicaciones(txtBuscar.Text));
        }

        private List<E_Aplicacion> ObtenerAppsActivas()
        {
            var resultado = new List<E_Aplicacion>();

            int procesoActual = Process.GetCurrentProcess().Id;
            int sesionActual = Process.GetCurrentProcess().SessionId;
            var identificadoresVistos = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);
            var instaladasPorRuta = _appsInstaladas
                .Where(app => !String.IsNullOrWhiteSpace(app.Identificador))
                .GroupBy(app => app.Identificador, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(grupo => grupo.Key, grupo => grupo.First(), StringComparer.OrdinalIgnoreCase);

            foreach (Process proceso in Process.GetProcesses())
            {
                using (proceso)
                {
                    try
                    {
                        if (proceso.Id == procesoActual ||
                            proceso.SessionId != sesionActual ||
                            proceso.MainWindowHandle == IntPtr.Zero ||
                            !IsWindowVisible(proceso.MainWindowHandle))
                        {
                            continue;
                        }

                        string ruta;
                        try
                        {
                            ruta = proceso.MainModule == null
                                ? null
                                : proceso.MainModule.FileName;
                        }
                        catch
                        {
                            // Omite procesos cuyo ejecutable no se puede leer.
                            continue;
                        }

                        if (String.IsNullOrWhiteSpace(ruta) || ruta.Length > 255 ||
                            !identificadoresVistos.Add(ruta))
                        {
                            continue;
                        }

                        E_Aplicacion registrada;
                        instaladasPorRuta.TryGetValue(ruta, out registrada);

                        resultado.Add(registrada ?? new E_Aplicacion
                        {
                            Nombre = proceso.ProcessName,
                            Identificador = ruta,
                            EsCritica = false,
                            RutaIcono = ruta,
                            IndiceIcono = 0
                        });
                    }
                    catch
                    {
                        // El proceso pudo cerrarse durante la enumeración.
                    }
                }
            }
            return resultado;
        }

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        private void MostrarAplicaciones(IEnumerable<E_Aplicacion> aplicaciones)
        {
            List<E_Aplicacion> lista = aplicaciones.ToList();
            var controles = new List<Control>(lista.Count);
            if (lista.Count == 0 && chkAppsAbiertas.Checked)
            {
                controles.Add(new Label
                {
                    AutoSize = true,
                    Margin = new Padding(12),
                    Text = "No se encontraron aplicaciones abiertas con ventana visible."
                });
            }

            foreach (E_Aplicacion app in lista)
            {
                var tarjeta = new ucAplicacion { Margin = new Padding(6) };
                tarjeta.Configurar(app);
                tarjeta.SeleccionCambiada += (s, e) =>
                {
                    var control = (ucAplicacion)s;
                    if (control.Seleccionada) _idsSeleccionados.Add(control.Aplicacion.Identificador);
                    else _idsSeleccionados.Remove(control.Aplicacion.Identificador);
                };
                if (_idsSeleccionados.Contains(app.Identificador))
                    tarjeta.EstablecerSeleccionada(true);
                controles.Add(tarjeta);
            }
            ControlesPresentacion.Reemplazar(flApps, controles.ToArray());
            foreach (ucAplicacion tarjeta in flApps.Controls.OfType<ucAplicacion>())
                if (tarjeta.Aplicacion != null && _idsSeleccionados.Contains(tarjeta.Aplicacion.Identificador))
                    tarjeta.EstablecerSeleccionada(true);
        }

        private void ActualizarFondoLista()
        {
            // Conserva la superficie visible del panel del diseñador. Solo la
            // lista y sus tarjetas permanecen transparentes sobre ella.
            pnlLayoutApp.Back = Color.LightSteelBlue;
            pnlLayoutApp.BackColor = Color.Transparent;
            pnlLayoutApp.BorderColor = Color.Black;
            flApps.BackColor = Color.Transparent;
        }

        private void btnManual_Click(object sender, EventArgs e)
        {
            using (var dialogo = new OpenFileDialog())
            {
                dialogo.Title = "Selecciona el ejecutable de la aplicación";
                dialogo.Filter = "Aplicaciones (*.exe)|*.exe";
                dialogo.CheckFileExists = true;
                dialogo.Multiselect = false;
                if (dialogo.ShowDialog(this) != DialogResult.OK) return;

                string ruta = dialogo.FileName;
                if (ruta.Length > 255)
                {
                    MessageBox.Show(this, "La ruta supera los 255 caracteres permitidos para el identificador.");
                    return;
                }
                if (_appsManuales.Concat(_appsInstaladas).Any(a =>
                    String.Equals(a.Identificador, ruta, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show(this, "Esa aplicación ya está en la lista.");
                    return;
                }

                var app = new E_Aplicacion
                {
                    Nombre = Path.GetFileNameWithoutExtension(ruta),
                    Identificador = ruta,
                    EsCritica = false,
                    RutaIcono = ruta,
                    IndiceIcono = 0
                };
                _appsManuales.Insert(0, app);
                MostrarAplicaciones(FiltrarAplicaciones(txtBuscar.Text));
            }
        }

        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
