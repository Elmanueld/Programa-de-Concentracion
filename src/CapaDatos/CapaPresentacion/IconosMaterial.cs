using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Linq;
using WpfDrawingContext = System.Windows.Media.DrawingContext;
using WpfDrawingVisual = System.Windows.Media.DrawingVisual;
using WpfFillRule = System.Windows.Media.FillRule;
using WpfGeometry = System.Windows.Media.Geometry;
using WpfMatrix = System.Windows.Media.Matrix;
using WpfMatrixTransform = System.Windows.Media.MatrixTransform;
using WpfPathGeometry = System.Windows.Media.PathGeometry;
using WpfPixelFormats = System.Windows.Media.PixelFormats;
using WpfRenderTargetBitmap = System.Windows.Media.Imaging.RenderTargetBitmap;
using WpfPngBitmapEncoder = System.Windows.Media.Imaging.PngBitmapEncoder;
using WpfBitmapFrame = System.Windows.Media.Imaging.BitmapFrame;
using WpfSolidColorBrush = System.Windows.Media.SolidColorBrush;
using MediaColor = System.Windows.Media.Color;

namespace CapaPresentacion
{
    /// <summary>Paleta base de la interfaz, reutilizable por formularios y personalización.</summary>
    public static class PaletaAplicacion
    {
        private static readonly Color DefCabecera = Color.FromArgb(71, 104, 180);
        private static readonly Color DefPrimario = Color.FromArgb(75, 119, 209);
        private static readonly Color DefHover = Color.FromArgb(89, 133, 225);
        private static readonly Color DefTarjetaOscura = Color.FromArgb(37, 63, 118);
        private static readonly Color DefFondoClaro = Color.Lavender;
        private static readonly Color DefPanelClaro = Color.LightSteelBlue;
        private static readonly Color DefBotonSumar = Color.FromArgb(89, 133, 225);
        private static readonly Color DefBotonRestar = Color.FromArgb(71, 104, 180);
        private static readonly Color DefDiaSeleccionado = Color.FromArgb(30, 165, 145);
        private static readonly Color DefDiaActual = Color.FromArgb(22, 119, 255);
        private static readonly Color DefDiaFuera = Color.FromArgb(82, 91, 110);
        private static readonly Color DefBorde = Color.FromArgb(190, 205, 232);

        public static Color AzulCabecera { get; private set; } = DefCabecera;
        public static Color AzulPrimario { get; private set; } = DefPrimario;
        public static Color AzulHover { get; private set; } = DefHover;
        public static Color FondoSubTarjeta { get; private set; } = DefPrimario;
        public static Color AzulTarjetaOscura { get; private set; } = DefTarjetaOscura;
        public static Color AzulProfundo { get { return AzulTarjetaOscura; } }
        public static Color FondoClaro { get; private set; } = DefFondoClaro;
        public static Color FondoOscuro { get; private set; } = Color.FromArgb(24, 33, 51);
        public static Color SuperficieOscura { get; private set; } = Color.FromArgb(35, 48, 70);
        public static Color PanelClaro { get; private set; } = DefPanelClaro;
        public static Color SuperficieTarea { get; private set; } = SystemColors.ActiveCaption;
        public static Color BotonSumar { get; private set; } = DefBotonSumar;
        public static Color BotonRestar { get; private set; } = DefBotonRestar;
        public static Color TextoSobreOscuro { get; private set; } = Color.White;
        public static Color TextoSobreClaro { get; private set; } = Color.Black;
        public static Color TextoTarjetaEjemplo { get; private set; } = Color.White;
        public static Color TextoDescripcionTarjeta { get; private set; } = Color.LightSteelBlue;
        public static Color FondoTextoGrande { get; private set; } = DefPrimario;
        public static Color FondoTextoPequeno { get; private set; } = DefPrimario;
        public static Color IconosTarjeta { get; private set; } = Color.White;
        public static Color IconosPrincipal { get; private set; } = Color.White;
        public static Color PanelDia { get; private set; } = Color.White;
        public static Color TextoCabecera { get; private set; } = Color.White;
        public static Color BotonCerrar { get; private set; } = DefCabecera;
        public static Color BotonBorrar { get; private set; } = DefCabecera;
        public static Color BotonAgregarTarea { get; private set; } = Color.FromArgb(36, 116, 62);
        public static Color DiaNormal { get; private set; } = DefPrimario;
        public static Color DiaSeleccionado { get; private set; } = DefDiaSeleccionado;
        public static Color DiaActual { get; private set; } = DefDiaActual;
        public static Color DiaFueraCalendario { get; private set; } = DefDiaFuera;
        public static Color BordeSuave { get; private set; } = DefBorde;
        public static Color AcentoExito { get; private set; } = Color.FromArgb(36, 116, 62);
        public static Color AcentoPeligro { get; private set; } = Color.FromArgb(188, 39, 39);
        public static bool TieneColoresPersonalizados { get; private set; }

        public static void Aplicar(PreferenciasApariencia apariencia)
        {
            if (apariencia == null) return;
            TieneColoresPersonalizados = ColoresDifierenDePlantilla(apariencia);
            AzulCabecera = Color.FromArgb(apariencia.Cabecera);
            AzulPrimario = Color.FromArgb(apariencia.Primario);
            AzulHover = Color.FromArgb(apariencia.PrimarioHover);
            FondoSubTarjeta = Color.FromArgb(apariencia.FondoSubTarjeta);
            AzulTarjetaOscura = Color.FromArgb(apariencia.TarjetaOscura);
            FondoClaro = Color.FromArgb(apariencia.FondoVentana);
            FondoOscuro = Color.FromArgb(apariencia.FondoOscuro);
            SuperficieOscura = Color.FromArgb(apariencia.SuperficieOscura);
            PanelClaro = Color.FromArgb(apariencia.SuperficieClara);
            SuperficieTarea = Color.FromArgb(apariencia.SuperficieTarea);
            BotonSumar = Color.FromArgb(apariencia.BotonSumar);
            BotonRestar = Color.FromArgb(apariencia.BotonRestar);
            TextoSobreOscuro = apariencia.ColorTextoOscuro;
            TextoSobreClaro = apariencia.ColorTextoClaro;
            TextoTarjetaEjemplo = Color.FromArgb(apariencia.TextoTarjetaEjemplo);
            TextoDescripcionTarjeta = Color.FromArgb(apariencia.TextoDescripcionTarjeta);
            FondoTextoGrande = Color.FromArgb(apariencia.FondoTextoGrande);
            FondoTextoPequeno = Color.FromArgb(apariencia.FondoTextoPequeno);
            IconosTarjeta = Color.FromArgb(apariencia.IconosTarjeta);
            IconosPrincipal = Color.FromArgb(apariencia.IconosPrincipal);
            PanelDia = Color.FromArgb(apariencia.PanelDia);
            TextoCabecera = Color.FromArgb(apariencia.TextoCabecera);
            BotonCerrar = Color.FromArgb(apariencia.BotonCerrar);
            BotonBorrar = Color.FromArgb(apariencia.BotonBorrar);
            BotonAgregarTarea = Color.FromArgb(apariencia.BotonAgregarTarea);
            DiaNormal = Color.FromArgb(apariencia.DiaNormal);
            DiaSeleccionado = Color.FromArgb(apariencia.DiaSeleccionado);
            DiaActual = Color.FromArgb(apariencia.DiaActual);
            DiaFueraCalendario = Color.FromArgb(apariencia.DiaFueraDeMes);
            BordeSuave = Color.FromArgb(apariencia.BordeSuave);
            // Descansos conservan su color de éxito propio; la personalización
            // no cambia los colores de los formularios de aviso y descanso.
        }

        private static bool ColoresDifierenDePlantilla(PreferenciasApariencia actual)
        {
            PreferenciasApariencia baseDesigner = PreferenciasApariencia.PorDefecto;
            return actual.FondoVentana != baseDesigner.FondoVentana ||
                actual.FondoOscuro != baseDesigner.FondoOscuro ||
                actual.SuperficieOscura != baseDesigner.SuperficieOscura ||
                actual.Cabecera != baseDesigner.Cabecera || actual.Primario != baseDesigner.Primario ||
                actual.PrimarioHover != baseDesigner.PrimarioHover ||
                actual.FondoSubTarjeta != baseDesigner.FondoSubTarjeta ||
                actual.TarjetaOscura != baseDesigner.TarjetaOscura ||
                actual.SuperficieClara != baseDesigner.SuperficieClara ||
                actual.SuperficieTarea != baseDesigner.SuperficieTarea ||
                actual.BotonSumar != baseDesigner.BotonSumar ||
                actual.BotonRestar != baseDesigner.BotonRestar ||
                actual.TextoCabecera != baseDesigner.TextoCabecera ||
                actual.ColorTextoFondoOscuro != baseDesigner.ColorTextoFondoOscuro ||
                actual.ColorTextoFondoClaro != baseDesigner.ColorTextoFondoClaro ||
                actual.TextoTarjetaEjemplo != baseDesigner.TextoTarjetaEjemplo ||
                actual.TextoDescripcionTarjeta != baseDesigner.TextoDescripcionTarjeta ||
                actual.FondoTextoGrande != baseDesigner.FondoTextoGrande ||
                actual.FondoTextoPequeno != baseDesigner.FondoTextoPequeno ||
                actual.IconosTarjeta != baseDesigner.IconosTarjeta ||
                actual.IconosPrincipal != baseDesigner.IconosPrincipal ||
                actual.PanelDia != baseDesigner.PanelDia || actual.BotonCerrar != baseDesigner.BotonCerrar ||
                actual.BotonBorrar != baseDesigner.BotonBorrar ||
                actual.BotonAgregarTarea != baseDesigner.BotonAgregarTarea ||
                actual.DiaNormal != baseDesigner.DiaNormal ||
                actual.DiaSeleccionado != baseDesigner.DiaSeleccionado ||
                actual.DiaActual != baseDesigner.DiaActual ||
                actual.DiaFueraDeMes != baseDesigner.DiaFueraDeMes ||
                actual.Exito != baseDesigner.Exito ||
                actual.BordeSuave != baseDesigner.BordeSuave;
        }

        public static Color AjustarFondo(Color color)
        {
            if (color == DefCabecera) return AzulCabecera;
            if (color == DefPrimario) return AzulPrimario;
            if (color == DefHover) return AzulHover;
            if (color == DefTarjetaOscura) return AzulTarjetaOscura;
            if (color == DefFondoClaro) return FondoClaro;
            if (color == DefPanelClaro) return PanelClaro;
            if (color == SystemColors.ActiveCaption) return SuperficieTarea;
            if (color == DefBotonSumar) return BotonSumar;
            if (color == DefBotonRestar) return BotonRestar;
            if (color == SystemColors.Window) return PanelClaro;
            if (color == SystemColors.Control) return FondoClaro;
            if (color == Color.White) return PanelDia;
            if (color == DefDiaSeleccionado) return DiaSeleccionado;
            if (color == DefDiaActual) return DiaActual;
            if (color == DefDiaFuera) return DiaFueraCalendario;
            if (color == DefBorde) return BordeSuave;
            if (color == Color.FromArgb(36, 116, 62)) return BotonAgregarTarea;
            if (color == Color.FromArgb(24, 33, 51)) return FondoOscuro;
            if (color == Color.FromArgb(35, 48, 70)) return SuperficieOscura;
            return color;
        }

        public static bool EsColorBase(Color color)
        {
            return color == DefCabecera || color == DefPrimario || color == DefHover ||
                color == DefTarjetaOscura || color == DefFondoClaro || color == DefPanelClaro ||
                color == DefDiaSeleccionado || color == DefDiaActual || color == DefDiaFuera ||
                color == DefBorde || color == Color.White || color == SystemColors.ActiveCaption ||
                color == DefBotonSumar || color == DefBotonRestar ||
                color == SystemColors.Window || color == SystemColors.Control ||
                color == Color.FromArgb(36, 116, 62) || color == Color.FromArgb(24, 33, 51) ||
                color == Color.FromArgb(35, 48, 70);
        }

        public static Color AjustarFondoOscuro(Color color, Color superficieAlternativa)
        {
            if (color == Color.Transparent) return Color.Transparent;
            if (color == DefCabecera || color == DefPrimario || color == DefHover ||
                color == DefTarjetaOscura || color == DefDiaSeleccionado ||
                color == DefDiaActual || color == DefDiaFuera || color == DefBorde ||
                color == DefBotonSumar || color == DefBotonRestar ||
                color == Color.FromArgb(36, 116, 62) || color == Color.FromArgb(24, 33, 51) ||
                color == Color.FromArgb(35, 48, 70))
                return AjustarFondo(color);
            return superficieAlternativa;
        }

        public static Color AjustarTexto(Color color)
        {
            if (color == Color.White) return TextoSobreOscuro;
            if (color == Color.Black || color == DefTarjetaOscura) return TextoSobreClaro;
            return color;
        }

        public static Color ColorIcono(bool sobreFondoOscuro)
        {
            return sobreFondoOscuro ? TextoSobreOscuro : TextoSobreClaro;
        }

        /// <summary>Elige entre ambos colores de texto según el contraste real con el fondo.</summary>
        public static Color ColorTextoContraste(Color fondo)
        {
            if (fondo.IsEmpty) fondo = FondoClaro;
            if (fondo.A < 255) fondo = Componer(fondo, FondoClaro);

            double luminanciaFondo = Luminancia(fondo);
            double luminanciaTextoClaro = Luminancia(Componer(TextoSobreClaro, fondo));
            double luminanciaTextoOscuro = Luminancia(Componer(TextoSobreOscuro, fondo));
            double contrasteClaro = RatioContraste(luminanciaFondo, luminanciaTextoClaro);
            double contrasteOscuro = RatioContraste(luminanciaFondo, luminanciaTextoOscuro);
            return contrasteClaro >= contrasteOscuro ? TextoSobreClaro : TextoSobreOscuro;
        }

        public static Color ColorTextoContraste(Control control)
        {
            return ColorTextoContraste(ObtenerFondoVisible(control));
        }

        /// <summary>
        /// Calcula el color que realmente queda detrás de un control. Recorre
        /// la jerarquía desde el Form hasta el control y compone fondos con
        /// transparencia, incluyendo la propiedad Back nullable de AntdUI.
        /// </summary>
        public static Color ObtenerFondoVisible(Control control)
        {
            var jerarquia = new Stack<Control>();
            for (Control actual = control; actual != null; actual = actual.Parent)
                jerarquia.Push(actual);

            Color fondo = FondoClaro.IsEmpty ? Color.White : FondoClaro;
            while (jerarquia.Count > 0)
            {
                Control actual = jerarquia.Pop();
                Color colorPropio;
                if (TryObtenerFondoPropio(actual, out colorPropio))
                    fondo = Componer(colorPropio, fondo);
            }
            return fondo;
        }

        private static bool TryObtenerFondoPropio(Control control, out Color color)
        {
            color = Color.Empty;
            if (control == null) return false;

            // AntdUI dibuja los paneles con Back; en botones DefaultBack es
            // el fondo que se muestra en reposo. Ambos pueden ser Color?.
            if (control is AntdUI.Panel && TryLeerColor(control, "Back", out color) &&
                color.A > 0 && !color.IsEmpty) return true;
            if (control is AntdUI.Button && TryLeerColor(control, "DefaultBack", out color) &&
                color.A > 0 && !color.IsEmpty) return true;

            color = control.BackColor;
            if (color.A > 0 && !color.IsEmpty) return true;

            string[] propiedades = { "Back", "Fill", "Surface" };
            foreach (string nombre in propiedades)
                if (TryLeerColor(control, nombre, out color) && color.A > 0 && !color.IsEmpty)
                    return true;

            color = Color.Empty;
            return false;
        }

        private static bool TryLeerColor(Control control, string nombre, out Color color)
        {
            color = Color.Empty;
            try
            {
                var propiedad = control.GetType().GetProperty(nombre);
                if (propiedad == null ||
                    (propiedad.PropertyType != typeof(Color) &&
                     Nullable.GetUnderlyingType(propiedad.PropertyType) != typeof(Color)))
                    return false;

                object valor = propiedad.GetValue(control, null);
                if (valor is Color)
                {
                    color = (Color)valor;
                    return true;
                }
            }
            catch { }
            return false;
        }

        private static Color Componer(Color frente, Color fondo)
        {
            if (frente.IsEmpty || frente.A == 0) return fondo;
            if (frente.A == 255) return frente;

            double alphaFrente = frente.A / 255.0;
            double alphaFondo = fondo.A / 255.0;
            double alphaSalida = alphaFrente + alphaFondo * (1.0 - alphaFrente);
            if (alphaSalida <= 0) return Color.Transparent;

            int rojo = (int)Math.Round((frente.R * alphaFrente + fondo.R * alphaFondo * (1.0 - alphaFrente)) / alphaSalida);
            int verde = (int)Math.Round((frente.G * alphaFrente + fondo.G * alphaFondo * (1.0 - alphaFrente)) / alphaSalida);
            int azul = (int)Math.Round((frente.B * alphaFrente + fondo.B * alphaFondo * (1.0 - alphaFrente)) / alphaSalida);
            int alpha = (int)Math.Round(alphaSalida * 255.0);
            return Color.FromArgb(alpha, rojo, verde, azul);
        }

        private static double Luminancia(Color color)
        {
            Func<int, double> canal = valor =>
            {
                double s = valor / 255.0;
                return s <= 0.04045 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
            };
            return 0.2126 * canal(color.R) + 0.7152 * canal(color.G) + 0.0722 * canal(color.B);
        }

        private static double RatioContraste(double a, double b)
        {
            double claro = Math.Max(a, b);
            double oscuro = Math.Min(a, b);
            return (claro + 0.05) / (oscuro + 0.05);
        }



        /// <summary>Recalcula los textos de una ventana después de aplicar sus fondos.</summary>



    }

    /// <summary>Reemplaza solo el dibujo Material por su SVG editable, usando el color del recurso del diseñador.</summary>
    internal static class IconosGoogle
    {
        private sealed class EstadoIcono
        {
            public Color Color;
            public Color ColorActual;
            public string Nombre;
            public int Ancho;
            public int Alto;
            public Image Generado;
            public bool ImagenPersonalizada;
        }

        private static readonly ConditionalWeakTable<Control, EstadoIcono> Estados =
            new ConditionalWeakTable<Control, EstadoIcono>();

        public static void Aplicar(Control control, string nombre)
        {
            if (control == null || String.IsNullOrWhiteSpace(nombre)) return;
            Image recurso = ObtenerImagen(control);
            if (recurso == null) return;

            EstadoIcono estado;
            if (!Estados.TryGetValue(control, out estado))
            {
                Color color = ObtenerColorOriginal(recurso);
                if (color.IsEmpty) return;
                estado = new EstadoIcono
                {
                    Color = color,
                    ColorActual = color,
                    Nombre = nombre,
                    Ancho = recurso.Width,
                    Alto = recurso.Height
                };
                Estados.Add(control, estado);
                control.Disposed += (s, e) =>
                {
                    EstadoIcono eliminado;
                    if (Estados.TryGetValue(control, out eliminado) && eliminado.Generado != null)
                        eliminado.Generado.Dispose();
                    Estados.Remove(control);
                };
            }

            Image nuevo;
            try { nuevo = Renderizar(nombre, recurso.Width, recurso.Height, estado.Color); }
            catch { return; }
            if (nuevo == null) return;
            Image anteriorGenerado = estado.Generado;
            AsignarImagen(control, nuevo);
            estado.Generado = nuevo;
            estado.ColorActual = estado.Color;
            if (anteriorGenerado != null && !ReferenceEquals(anteriorGenerado, nuevo))
                anteriorGenerado.Dispose();
        }

        public static void AplicarPaleta(Control raiz)
        {
            if (raiz == null) return;
            EstadoIcono estado;
            if (Estados.TryGetValue(raiz, out estado))
            {
                if (estado.ImagenPersonalizada)
                {
                    foreach (Control hijoPersonalizado in raiz.Controls)
                        AplicarPaleta(hijoPersonalizado);
                    return;
                }
                Color color = !PaletaAplicacion.TieneColoresPersonalizados
                    ? estado.Color
                    : (raiz is AntdUI.Button || raiz is Button
                        ? PaletaAplicacion.IconosPrincipal
                        : PaletaAplicacion.IconosTarjeta);
                Image actual = ObtenerImagen(raiz);
                if (color == estado.ColorActual || actual == null)
                {
                    foreach (Control hijoSinCambio in raiz.Controls)
                        AplicarPaleta(hijoSinCambio);
                    return;
                }
                Image nuevo = null;
                try
                {
                    nuevo = Renderizar(estado.Nombre, estado.Ancho, estado.Alto, color);
                }
                catch (ArgumentException) { return; }
                catch (InvalidDataException) { return; }
                catch (ExternalException) { return; }
                catch (InvalidOperationException) { return; }
                catch (FormatException) { return; }
                if (nuevo != null)
                {
                    Image anterior = estado.Generado;
                    AsignarImagen(raiz, nuevo);
                    estado.Generado = nuevo;
                    estado.ColorActual = color;
                    if (anterior != null && !ReferenceEquals(anterior, nuevo)) anterior.Dispose();
                }
            }
            foreach (Control hijo in raiz.Controls) AplicarPaleta(hijo);
        }

        public static void EstablecerImagenPersonalizada(Control control, Image imagen)
        {
            if (control == null || imagen == null) return;
            EstadoIcono estado;
            if (Estados.TryGetValue(control, out estado))
                estado.ImagenPersonalizada = true;
            AsignarImagen(control, imagen);
        }

        public static void RestaurarImagenPredeterminada(Control control)
        {
            if (control == null) return;
            EstadoIcono estado;
            if (Estados.TryGetValue(control, out estado))
            {
                estado.ImagenPersonalizada = false;
                AsignarImagen(control, estado.Generado);
                AplicarPaleta(control);
                return;
            }
            AsignarImagen(control, null);
        }

        private static Image ObtenerImagen(Control control)
        {
            if (control is AntdUI.Button boton) return boton.Icon;
            if (control is AntdUI.Image3D imagen3D) return imagen3D.Image;
            if (control is PictureBox pictureBox) return pictureBox.Image;
            if (control is Button botonWinForms) return botonWinForms.Image;
            if (control is Label etiqueta) return etiqueta.Image;
            return null;
        }

        private static void AsignarImagen(Control control, Image imagen)
        {
            if (control is AntdUI.Button boton) boton.Icon = imagen;
            else if (control is AntdUI.Image3D imagen3D) imagen3D.Image = imagen;
            else if (control is PictureBox pictureBox) pictureBox.Image = imagen;
            else if (control is Button botonWinForms) botonWinForms.Image = imagen;
            else if (control is Label etiqueta) etiqueta.Image = imagen;
        }

        private static Color ObtenerColorOriginal(Image imagen)
        {
            var colores = new Dictionary<int, int>();
            using (var bitmap = new Bitmap(imagen))
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Color color = bitmap.GetPixel(x, y);
                        if (color.A < 240) continue;
                        int clave = Color.FromArgb(255, color.R, color.G, color.B).ToArgb();
                        int cantidad;
                        colores.TryGetValue(clave, out cantidad);
                        colores[clave] = cantidad + 1;
                    }
                }
            }

            if (colores.Count == 0) return Color.Empty;
            int dominante = colores.OrderByDescending(par => par.Value).First().Key;
            return Color.FromArgb(dominante);
        }

        private static Image Renderizar(string nombre, int ancho, int alto, Color color)
        {
            string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "src", "icons", nombre + ".svg");
            if (!File.Exists(ruta))
                ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "CapaPresentacion", "src", "icons", nombre + ".svg");
            if (!File.Exists(ruta)) return null;

            XDocument documento = XDocument.Load(ruta);
            XElement raiz = documento.Root;
            if (raiz == null)
                throw new InvalidDataException("El SVG está vacío.");

            string viewBox = (string)raiz.Attribute("viewBox");
            string[] caja = viewBox == null
                ? null
                : viewBox.Split(new[] { ' ', '\t', '\r', '\n', ',' },
                    StringSplitOptions.RemoveEmptyEntries);
            if (caja == null || caja.Length != 4)
                throw new InvalidDataException("El SVG no tiene un viewBox válido.");

            double xVista = Double.Parse(caja[0], CultureInfo.InvariantCulture);
            double yVista = Double.Parse(caja[1], CultureInfo.InvariantCulture);
            double anchoVista = Double.Parse(caja[2], CultureInfo.InvariantCulture);
            double altoVista = Double.Parse(caja[3], CultureInfo.InvariantCulture);
            if (anchoVista <= 0 || altoVista <= 0 ||
                Double.IsNaN(anchoVista) || Double.IsNaN(altoVista) ||
                Double.IsInfinity(anchoVista) || Double.IsInfinity(altoVista))
                throw new InvalidDataException("El viewBox del SVG no tiene dimensiones válidas.");
            int anchoObjetivo = Math.Max(1, ancho);
            int altoObjetivo = Math.Max(1, alto);
            if (anchoObjetivo > 4096 || altoObjetivo > 4096)
                throw new InvalidDataException("El tamaño solicitado para el icono es demasiado grande.");
            double escala = Math.Min(anchoObjetivo / anchoVista,
                altoObjetivo / altoVista);
            double x = (anchoObjetivo - anchoVista * escala) / 2d;
            double y = (altoObjetivo - altoVista * escala) / 2d;

            var visual = new WpfDrawingVisual();
            using (WpfDrawingContext dibujo = visual.RenderOpen())
            {
                var matriz = new WpfMatrix(escala, 0, 0, escala,
                    x - xVista * escala, y - yVista * escala);
                dibujo.PushTransform(new WpfMatrixTransform(matriz));
                var pincel = new WpfSolidColorBrush(MediaColor.FromArgb(
                    color.A, color.R, color.G, color.B));
                pincel.Freeze();

                XNamespace espacioSvg = raiz.Name.Namespace;
                foreach (XElement elemento in raiz.Descendants(espacioSvg + "path"))
                {
                    string datos = (string)elemento.Attribute("d");
                    if (String.IsNullOrWhiteSpace(datos)) continue;

                    WpfGeometry trazado = WpfGeometry.Parse(datos);
                    WpfPathGeometry geometria = WpfPathGeometry.CreateFromGeometry(trazado);
                    string regla = (string)elemento.Attribute("fill-rule");
                    if (String.Equals(regla, "evenodd", StringComparison.OrdinalIgnoreCase))
                        geometria.FillRule = WpfFillRule.EvenOdd;
                    dibujo.DrawGeometry(pincel, null, geometria);
                }
                dibujo.Pop();
            }

            var bitmapWpf = new WpfRenderTargetBitmap(anchoObjetivo, altoObjetivo,
                96d, 96d, WpfPixelFormats.Pbgra32);
            bitmapWpf.Render(visual);
            var codificador = new WpfPngBitmapEncoder();
            codificador.Frames.Add(WpfBitmapFrame.Create(bitmapWpf));
            using (var flujo = new MemoryStream())
            {
                codificador.Save(flujo);
                flujo.Position = 0;
                using (Image temporal = Image.FromStream(flujo))
                    return new Bitmap(temporal);
            }
        }
    }

}
