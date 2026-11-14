using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaEntidades;

namespace CapaPresentacion
{
    public partial class ucAplicacion : UserControl
    {
        private static readonly ConcurrentDictionary<string, Lazy<Task<byte[]>>> cacheIconos =
            new ConcurrentDictionary<string, Lazy<Task<byte[]>>>(StringComparer.OrdinalIgnoreCase);
        private static readonly SemaphoreSlim limiteExtraccionIconos = new SemaphoreSlim(3, 3);
        private readonly Image imagenIconoDisenador;

        public E_Aplicacion Aplicacion { get; private set; }
        public bool Seleccionada { get; private set; }
        private bool permitirSeleccion = true;

        public event EventHandler SeleccionCambiada;
        public event EventHandler QuitarSolicitado;

        public ucAplicacion()
        {
            InitializeComponent();
            if (FormularioSuave.EnModoDiseno) return;
            BackColor = Color.Transparent;
            panel1.BackColor = Color.Transparent;
            pbIcono.BackColor = Color.Transparent;
            lblNombreApp.BackColor = Color.Transparent;
            TemaAplicacion.Preparar(this);
            imagenIconoDisenador = pbIcono.Image;
            ControlesPresentacion.Optimizar(panel1);
            panel1.BackColor = Color.Transparent;
            btnQuitar.Click += (s, e) => QuitarSolicitado?.Invoke(this, EventArgs.Empty);

            EnlazarClick(this);
        }

        public void Configurar(E_Aplicacion aplicacion, bool mostrarBotonQuitar = false)
        {
            Aplicacion = aplicacion;
            permitirSeleccion = !mostrarBotonQuitar;
            btnQuitar.Visible = mostrarBotonQuitar;
            lblNombreApp.Text = aplicacion.Nombre;

            // Restablece el recurso del diseñador mientras se carga el icono
            // propio de esta aplicación, sin cambiar su color original.
            ReemplazarImagen(imagenIconoDisenador);
            string clave = (aplicacion.RutaIcono ?? String.Empty) + "|" +
                aplicacion.IndiceIcono + "|" + (aplicacion.Identificador ?? String.Empty);
            Lazy<Task<byte[]>> carga = cacheIconos.GetOrAdd(clave,
                _ => new Lazy<Task<byte[]>>(() => Task.Run(async () =>
                {
                    await limiteExtraccionIconos.WaitAsync().ConfigureAwait(false);
                    try { return ExtraerIconoBytes(aplicacion); }
                    finally { limiteExtraccionIconos.Release(); }
                })));
            CargarIconoAsync(aplicacion, carga.Value);
        }

        private static byte[] ExtraerIconoBytes(E_Aplicacion aplicacion)
        {
            Image imagen = ExtraerIcono(aplicacion.RutaIcono, aplicacion.IndiceIcono);
            if (imagen == null) imagen = ExtraerIconoDelShell(aplicacion.Identificador);
            if (imagen == null && !String.IsNullOrWhiteSpace(aplicacion.Identificador) &&
                File.Exists(aplicacion.Identificador))
            {
                try
                {
                    using (Icon icono = Icon.ExtractAssociatedIcon(aplicacion.Identificador))
                        if (icono != null) imagen = icono.ToBitmap();
                }
                catch { }
            }
            if (imagen == null) imagen = SystemIcons.Application.ToBitmap();
            using (imagen)
            using (var flujo = new MemoryStream())
            {
                imagen.Save(flujo, System.Drawing.Imaging.ImageFormat.Png);
                return flujo.ToArray();
            }
        }

        private async void CargarIconoAsync(E_Aplicacion aplicacion, Task<byte[]> carga)
        {
            try
            {
                byte[] datos = await carga;
                if (datos == null || IsDisposed || Aplicacion != aplicacion) return;
                using (var memoria = new MemoryStream(datos))
                using (var temporal = Image.FromStream(memoria))
                    ReemplazarImagen(new Bitmap(temporal));
            }
            catch
            {
                // Se conserva el icono genérico si Windows no encuentra el recurso.
            }
        }

        private void ReemplazarImagen(Image nueva)
        {
            Image anterior = pbIcono.Image;
            pbIcono.Image = nueva;
            if (anterior != null && !ReferenceEquals(anterior, imagenIconoDisenador) &&
                !ReferenceEquals(anterior, nueva))
                anterior.Dispose();
        }

        private void EnlazarClick(Control control)
        {
            if (control == btnQuitar) return;
            control.Click += (sender, e) =>
            {
                if (permitirSeleccion) CambiarSeleccion();
            };

            foreach (Control hijo in control.Controls)
                EnlazarClick(hijo);
        }

        // Restaura la selección al reconstruir las tarjetas al filtrar.
        public void EstablecerSeleccionada(bool seleccionada)
        {
            Seleccionada = seleccionada;
            AplicarColorSeleccion();
        }

        private void CambiarSeleccion()
        {
            Seleccionada = !Seleccionada;
            AplicarColorSeleccion();
            SeleccionCambiada?.Invoke(this, EventArgs.Empty);
        }

        private void AplicarColorSeleccion()
        {
            panel1.BackColor = permitirSeleccion && Seleccionada
                ? PaletaAplicacion.AzulHover : Color.Transparent;
            lblNombreApp.ForeColor = PaletaAplicacion.ColorTextoContraste(panel1);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern uint ExtractIconEx(
            string archivo,
            int indice,
            IntPtr[] iconosGrandes,
            IntPtr[] iconosPequenos,
            uint cantidad
        );

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr icono);

        private static Image ExtraerIcono(string ruta, int indice)
        {
            if (String.IsNullOrWhiteSpace(ruta))
                return null;

            ruta = Environment.ExpandEnvironmentVariables(
                ruta.Trim().Trim('"')
            );

            if (!File.Exists(ruta))
                return null;

            string extension = Path.GetExtension(ruta);

            // PNG del manifiesto de una app empaquetada.
            if (String.Equals(
                extension,
                ".png",
                StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using (var original = new Bitmap(ruta))
                        return new Bitmap(original);
                }
                catch
                {
                    return null;
                }
            }

            // Icono .ico guardado en disco.
            if (String.Equals(
                extension,
                ".ico",
                StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using (var icono = new Icon(ruta))
                        return icono.ToBitmap();
                }
                catch
                {
                    return null;
                }
            }

            // Ejecutable o DLL con recursos de icono.
            IntPtr[] grandes = new IntPtr[1];
            IntPtr[] pequenos = new IntPtr[1];

            try
            {
                uint cantidad = ExtractIconEx(
                    ruta,
                    indice,
                    grandes,
                    pequenos,
                    1
                );

                IntPtr handle = grandes[0] != IntPtr.Zero
                    ? grandes[0]
                    : pequenos[0];

                if (cantidad == 0 || handle == IntPtr.Zero)
                    return null;

                using (Icon icono =
                    (Icon)Icon.FromHandle(handle).Clone())
                {
                    return icono.ToBitmap();
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                if (grandes[0] != IntPtr.Zero)
                    DestroyIcon(grandes[0]);

                if (pequenos[0] != IntPtr.Zero)
                    DestroyIcon(pequenos[0]);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SIZE
        {
            public int cx;
            public int cy;
        }

        [Flags]
        private enum SIIGBF
        {
            BIGGERSIZEOK = 0x00000001,
            ICONONLY = 0x00000004
        }

        [ComImport]
        [Guid("BCC18B79-BA16-442F-80C4-8A59C30C463B")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItemImageFactory
        {
            [PreserveSig]
            int GetImage(
                SIZE size,
                SIIGBF flags,
                out IntPtr phbm
            );
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHCreateItemFromParsingName(
            string pszPath,
            IntPtr pbc,
            ref Guid riid,
            out IShellItemImageFactory ppv
        );

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            public uint bmiColors;
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern int GetDIBits(
            IntPtr hdc,
            IntPtr hBitmap,
            uint uStartScan,
            uint cScanLines,
            IntPtr lpvBits,
            ref BITMAPINFO lpBitmapInfo,
            uint uUsage
        );

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(
            IntPtr hWnd,
            IntPtr hDC
        );

        private static Bitmap ConvertirHBitmapConTransparencia(
            IntPtr hBitmap)
        {
            const int ancho = 256;
            const int alto = 256;
            const int bytesPorPixel = 4;

            var bitmap = new Bitmap(
                ancho,
                alto,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

            System.Drawing.Imaging.BitmapData datos = null;

            try
            {
                datos = bitmap.LockBits(
                    new Rectangle(0, 0, ancho, alto),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );

                var info = new BITMAPINFO
                {
                    bmiHeader = new BITMAPINFOHEADER
                    {
                        biSize = (uint)Marshal.SizeOf(
                            typeof(BITMAPINFOHEADER)
                        ),
                        biWidth = ancho,
                        biHeight = -alto,
                        biPlanes = 1,
                        biBitCount = 32,
                        biCompression = 0,
                        biSizeImage = ancho * alto * bytesPorPixel
                    }
                };

                IntPtr dc = GetDC(IntPtr.Zero);

                try
                {
                    int filas = GetDIBits(
                        dc,
                        hBitmap,
                        0,
                        (uint)alto,
                        datos.Scan0,
                        ref info,
                        0
                    );

                    if (filas == 0)
                    {
                        throw new InvalidOperationException(
                            "Windows no pudo leer la imagen del icono."
                        );
                    }
                }
                finally
                {
                    ReleaseDC(IntPtr.Zero, dc);
                }

                int cantidad = Math.Abs(datos.Stride) * alto;
                byte[] pixeles = new byte[cantidad];

                Marshal.Copy(datos.Scan0, pixeles, 0, cantidad);

                bool tieneAlfa = false;

                for (int i = 3; i < pixeles.Length; i += 4)
                {
                    if (pixeles[i] != 0)
                    {
                        tieneAlfa = true;
                        break;
                    }
                }

                // Si no hay alfa en el bitmap, no se convierte
                // toda la imagen en transparente.
                if (!tieneAlfa)
                {
                    throw new InvalidOperationException(
                        "El bitmap del Shell no contiene canal alfa."
                    );
                }

                for (int i = 0; i < pixeles.Length; i += 4)
                {
                    byte alfa = pixeles[i + 3];

                    if (alfa == 0)
                    {
                        pixeles[i] = 0;
                        pixeles[i + 1] = 0;
                        pixeles[i + 2] = 0;
                        continue;
                    }

                    if (alfa < 255)
                    {
                        pixeles[i] = (byte)Math.Min(
                            255,
                            pixeles[i] * 255 / alfa
                        );

                        pixeles[i + 1] = (byte)Math.Min(
                            255,
                            pixeles[i + 1] * 255 / alfa
                        );

                        pixeles[i + 2] = (byte)Math.Min(
                            255,
                            pixeles[i + 2] * 255 / alfa
                        );
                    }
                }

                Marshal.Copy(pixeles, 0, datos.Scan0, cantidad);
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
            finally
            {
                if (datos != null)
                    bitmap.UnlockBits(datos);
            }

            return bitmap;
        }

        private static Image ExtraerIconoDelShell(
            string identificador)
        {
            if (String.IsNullOrWhiteSpace(identificador))
                return null;

            string nombreAnalisis = identificador.Trim();

            if (!Path.IsPathRooted(nombreAnalisis) &&
                !nombreAnalisis.StartsWith(
                    "shell:",
                    StringComparison.OrdinalIgnoreCase) &&
                !nombreAnalisis.StartsWith(
                    "::{",
                    StringComparison.OrdinalIgnoreCase))
            {
                nombreAnalisis =
                    "shell:AppsFolder\\" + nombreAnalisis;
            }

            IShellItemImageFactory fabrica = null;
            IntPtr bitmapNativo = IntPtr.Zero;

            try
            {
                Guid iid = new Guid(
                    "BCC18B79-BA16-442F-80C4-8A59C30C463B"
                );

                int resultadoCrear = SHCreateItemFromParsingName(
                    nombreAnalisis,
                    IntPtr.Zero,
                    ref iid,
                    out fabrica
                );

                if (resultadoCrear != 0 || fabrica == null)
                    return null;

                int resultadoImagen = fabrica.GetImage(
                    new SIZE { cx = 256, cy = 256 },
                    SIIGBF.ICONONLY | SIIGBF.BIGGERSIZEOK,
                    out bitmapNativo
                );

                if (resultadoImagen != 0 ||
                    bitmapNativo == IntPtr.Zero)
                {
                    return null;
                }

                return ConvertirHBitmapConTransparencia(bitmapNativo);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (bitmapNativo != IntPtr.Zero)
                    DeleteObject(bitmapNativo);

                if (fabrica != null &&
                    Marshal.IsComObject(fabrica))
                {
                    Marshal.ReleaseComObject(fabrica);
                }
            }
        }
    }
}
