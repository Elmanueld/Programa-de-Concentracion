using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CapaPresentacion
{
    /// <summary>Carga Inter desde los archivos distribuidos junto a la aplicación.</summary>
    internal static class Tipografia
    {
        private static readonly PrivateFontCollection FuentesPrivadas;
        private static readonly FontFamily[] FamiliasInter;
        private static readonly Dictionary<string, Font> Cache = new Dictionary<string, Font>();

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
            if (control == null || FamiliasInter.Length == 0) return;
            try
            {
                Font fuenteActual = control.Font;
                Font fuenteInter = Obtener(fuenteActual.Size, fuenteActual.Style, fuenteActual.Unit);
                if (fuenteInter != null) control.Font = fuenteInter;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("No se pudo aplicar Inter a un control: " + ex.Message);
                return;
            }
            foreach (Control hijo in control.Controls)
                Aplicar(hijo);
        }

        private static Font Obtener(float tamano, FontStyle estilo, GraphicsUnit unidad)
        {
            FontStyle estiloBase = estilo & (FontStyle.Bold | FontStyle.Italic);
            string clave = tamano.ToString("R", CultureInfo.InvariantCulture) + "|" +
                ((int)estilo).ToString(CultureInfo.InvariantCulture) + "|" + ((int)unidad).ToString(CultureInfo.InvariantCulture);
            Font fuente;
            lock (Cache)
            {
                if (Cache.TryGetValue(clave, out fuente)) return fuente;
                FontFamily familia = FamiliasInter.FirstOrDefault(f => f.IsStyleAvailable(estiloBase))
                    ?? FamiliasInter.FirstOrDefault(f => f.IsStyleAvailable(FontStyle.Regular));
                if (familia == null) return null;
                fuente = new Font(familia, tamano, estilo, unidad);
                Cache.Add(clave, fuente);
                return fuente;
            }
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
