using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace CapaPresentacion
{
    /// <summary>Carga Inter desde los archivos distribuidos junto a la aplicación.</summary>
    internal static class Tipografia
    {
        private static readonly PrivateFontCollection FuentesPrivadas;
        private static readonly FontFamily[] FamiliasInter;
        private static readonly Dictionary<string, Font> Cache = new Dictionary<string, Font>();
        private static readonly ConditionalWeakTable<Control, FuenteBase> FuentesBase =
            new ConditionalWeakTable<Control, FuenteBase>();
        private static string familiaActual = "Inter";
        private static float escalaActual = 1f;
        private static bool configuracionPersonalizada;
        public static float EscalaActual { get { return escalaActual; } }
        public static bool TienePersonalizacion { get { return configuracionPersonalizada; } }

        private sealed class FuenteBase
        {
            public Font Fuente;
        }

        static Tipografia()
        {
            try
            {
                FuentesPrivadas = CargarFuentes();
                FamiliasInter = FuentesPrivadas.Families
                    .Where(familia => String.Equals(familia.Name, "Inter", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
            }
            catch (Exception ex)
            {
                // El diseñador de Visual Studio puede inicializar controles desde
                // una carpeta donde todavía no están los archivos de salida.
                System.Diagnostics.Trace.TraceWarning("No se pudo cargar Inter: " + ex.Message);
                if (FuentesPrivadas != null) FuentesPrivadas.Dispose();
                FuentesPrivadas = null;
                FamiliasInter = new FontFamily[0];
            }
        }

        public static void Aplicar(Control control)
        {
            // La sustitución de fuentes queda desactivada para respetar el diseñador.
        }

        public static void Configurar(string familia, string tamano)
        {
            familiaActual = String.IsNullOrWhiteSpace(familia) ? "Inter" : familia.Trim();
            configuracionPersonalizada = !String.Equals(familiaActual, "Inter", StringComparison.OrdinalIgnoreCase) ||
                !String.Equals((tamano ?? "Normal").Trim(), "Normal", StringComparison.OrdinalIgnoreCase);
            switch ((tamano ?? "Normal").Trim().ToLowerInvariant())
            {
                case "pequeño":
                case "pequeno":
                    escalaActual = 0.92f;
                    break;
                case "grande":
                    escalaActual = 1.10f;
                    break;
                default:
                    escalaActual = 1f;
                    break;
            }
        }

        private static Font Obtener(float tamano, FontStyle estilo, GraphicsUnit unidad)
        {
            FontStyle estiloBase = estilo & (FontStyle.Bold | FontStyle.Italic);
            string clave = familiaActual + "|" + tamano.ToString("R", CultureInfo.InvariantCulture) + "|" +
                ((int)estilo).ToString(CultureInfo.InvariantCulture) + "|" + ((int)unidad).ToString(CultureInfo.InvariantCulture);
            Font fuente;
            lock (Cache)
            {
                if (Cache.TryGetValue(clave, out fuente)) return fuente;
                FontFamily familia = ObtenerFamilia(familiaActual);
                if (familia == null) return null;
                FontStyle estiloFinal = familia.IsStyleAvailable(estiloBase)
                    ? estilo
                    : (estilo & ~(FontStyle.Bold | FontStyle.Italic));
                fuente = new Font(familia, Math.Max(7f, Math.Min(28f, tamano)), estiloFinal, unidad);
                Cache.Add(clave, fuente);
                return fuente;
            }
        }

        private static FontFamily ObtenerFamilia(string nombre)
        {
            if (String.Equals(nombre, "Inter", StringComparison.OrdinalIgnoreCase))
                return FamiliasInter.FirstOrDefault(f => f.IsStyleAvailable(FontStyle.Regular));
            try
            {
                return FontFamily.Families.FirstOrDefault(f =>
                    String.Equals(f.Name, nombre, StringComparison.OrdinalIgnoreCase));
            }
            catch { return null; }
        }

        private static PrivateFontCollection CargarFuentes()
        {
            string carpeta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts");
            var fuentes = new PrivateFontCollection();
            foreach (string archivo in new[]
            {
                "Inter-Regular.ttf", "Inter-Bold.ttf", "Inter-Italic.ttf", "Inter-BoldItalic.ttf"
            })
            {
                string ruta = Path.Combine(carpeta, archivo);
                if (!File.Exists(ruta))
                    throw new FileNotFoundException("Falta una fuente Inter distribuida con la aplicación.", ruta);
                fuentes.AddFontFile(ruta);
            }
            return fuentes;
        }
    }
}
