using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace CapaPresentacion
{
    /// <summary>
    /// Aplica el modo oscuro a partir de los valores que los controles recibieron
    /// del diseñador y restaura esos valores al volver al modo claro.
    /// </summary>
    internal static class TemaAplicacion
    {
        private sealed class ValorColor
        {
            public PropertyInfo Propiedad;
            public object ValorOriginal;
        }

        private sealed class EstadoControl
        {
            public readonly List<ValorColor> Colores = new List<ValorColor>();
        }

        private sealed class EstadoFormulario
        {
            public readonly Dictionary<Control, EstadoControl> Controles =
                new Dictionary<Control, EstadoControl>();
            public bool ModoOscuro;
        }

        private static readonly ConditionalWeakTable<Form, EstadoFormulario> Estados =
            new ConditionalWeakTable<Form, EstadoFormulario>();

        // Los colores originales se guardan por control y se capturan una sola
        // vez, antes de que la paleta personalizada toque sus propiedades.
        private static readonly ConditionalWeakTable<Control, EstadoControl> Originales =
            new ConditionalWeakTable<Control, EstadoControl>();

        public static bool ModoOscuroActual { get; private set; }

        public static void EstablecerModoOscuro(bool modoOscuro)
        {
            ModoOscuroActual = modoOscuro;
        }

        /// <summary>
        /// Guarda los colores del diseñador antes de que el control reciba la
        /// paleta personalizada. Debe llamarse al construir controles que
        /// pintan sus propios colores (por ejemplo, una tarjeta de día).
        /// </summary>
        public static void Preparar(Control control)
        {
            if (control == null || control.IsDisposed) return;
            Originales.GetValue(control, CapturarControl);
            foreach (Control hijo in control.Controls) Preparar(hijo);
        }

        /// <summary>Devuelve un control a los colores exactos del diseñador.</summary>
        public static void Restablecer(Control control)
        {
            if (control == null || control.IsDisposed) return;
            EstadoControl original;
            if (Originales.TryGetValue(control, out original))
            {
                foreach (ValorColor valor in original.Colores)
                {
                    try { valor.Propiedad.SetValue(control, valor.ValorOriginal, null); }
                    catch { }
                }
            }
            foreach (Control hijo in control.Controls) Restablecer(hijo);
        }

        /// <summary>Ajusta una superficie neutra para que no deslumbre en modo oscuro.</summary>
        public static Color SuperficieParaModo(Color color)
        {
            if (!ModoOscuroActual) return color;
            return TransformarColor(null, color, "Back");
        }

        /// <summary>Atenúa un acento de la plantilla cuando se usa el modo oscuro.</summary>
        public static Color AcentoParaModo(Color color)
        {
            if (!ModoOscuroActual) return color;
            if (Luminancia(color) < 0.10) return color;
            return Mezclar(Color.FromArgb(24, 33, 51), color, 0.45);
        }

        public static Color BordeParaModo(Color color)
        {
            return ModoOscuroActual ? Color.FromArgb(61, 75, 96) : color;
        }

        private static Color Mezclar(Color baseColor, Color color, double pesoColor)
        {
            double pesoBase = 1 - pesoColor;
            return Color.FromArgb(
                Math.Min(255, (int)Math.Round(baseColor.R * pesoBase + color.R * pesoColor)),
                Math.Min(255, (int)Math.Round(baseColor.G * pesoBase + color.G * pesoColor)),
                Math.Min(255, (int)Math.Round(baseColor.B * pesoBase + color.B * pesoColor)));
        }

        public static void Aplicar(Form formulario, bool modoOscuro)
        {
            if (formulario == null || formulario.IsDisposed) return;

            EstadoFormulario estado = Estados.GetValue(formulario,
                _ => new EstadoFormulario());
            estado.ModoOscuro = modoOscuro;
            ModoOscuroActual = modoOscuro;
            ControlesPresentacion.IniciarActualizacionVisual(formulario);
            formulario.SuspendLayout();
            try
            {
                CapturarArbol(formulario, estado);
                foreach (KeyValuePair<Control, EstadoControl> par in estado.Controles)
                    Aplicar(par.Key, par.Value, modoOscuro, false);
                foreach (KeyValuePair<Control, EstadoControl> par in estado.Controles)
                    Aplicar(par.Key, par.Value, modoOscuro, true);
                IconosGoogle.AplicarPaleta(formulario);
                ActualizarControlesPropios(formulario);
                frmTareas tareas = formulario as frmTareas;
                if (tareas != null) tareas.AplicarTemaCategoria();
            }
            finally
            {
                formulario.ResumeLayout(false);
                formulario.PerformLayout();
                ControlesPresentacion.FinalizarActualizacionVisual(formulario);
            }
        }

        private static void ActualizarControlesPropios(Control control)
        {
            if (control == null || control.IsDisposed) return;
            ucDia dia = control as ucDia;
            if (dia != null)
            {
                dia.ActualizarTema();
                return;
            }
            ucTareas tarjetaTarea = control as ucTareas;
            if (tarjetaTarea != null) tarjetaTarea.ActualizarApariencia();
            foreach (Control hijo in control.Controls)
                ActualizarControlesPropios(hijo);
        }

        private static EstadoControl CapturarControl(Control control)
        {
            var captura = new EstadoControl();
            foreach (PropertyInfo propiedad in control.GetType().GetProperties(
                BindingFlags.Instance | BindingFlags.Public))
            {
                if (!propiedad.CanRead || !propiedad.CanWrite ||
                    propiedad.GetIndexParameters().Length != 0 ||
                    !EsPropiedadDeColor(propiedad.PropertyType))
                    continue;

                try
                {
                    captura.Colores.Add(new ValorColor
                    {
                        Propiedad = propiedad,
                        ValorOriginal = propiedad.GetValue(control, null)
                    });
                }
                catch { }
            }
            return captura;
        }

        private static void CapturarArbol(Control control, EstadoFormulario estado)
        {
            if (control == null || control.IsDisposed) return;

            // Las tarjetas de día calculan sus propios colores según el modo y
            // la plantilla vigente, por lo que no deben capturarse aquí.
            if (control is ucDia) return;

            if (!estado.Controles.ContainsKey(control))
            {
                EstadoControl captura = Originales.GetValue(control, CapturarControl);
                estado.Controles.Add(control, captura);
                control.ControlAdded += (s, e) =>
                {
                    CapturarArbol(e.Control, estado);
                    AplicarArbol(e.Control, estado, estado.ModoOscuro);
                };
            }

            foreach (Control hijo in control.Controls)
                CapturarArbol(hijo, estado);
        }

        private static void AplicarArbol(Control control, EstadoFormulario estado,
            bool modoOscuro)
        {
            if (control is ucDia)
            {
                ((ucDia)control).ActualizarTema();
                return;
            }

            EstadoControl captura;
            if (control == null || control.IsDisposed ||
                !estado.Controles.TryGetValue(control, out captura)) return;
            Aplicar(control, captura, modoOscuro, false);
            Aplicar(control, captura, modoOscuro, true);
            foreach (Control hijo in control.Controls)
                AplicarArbol(hijo, estado, modoOscuro);
            ucTareas tarjetaTarea = control as ucTareas;
            if (tarjetaTarea != null) tarjetaTarea.ActualizarApariencia();
        }

        private static bool EsPropiedadDeColor(Type tipo)
        {
            return tipo == typeof(Color) || Nullable.GetUnderlyingType(tipo) == typeof(Color);
        }

        private static void Aplicar(Control control, EstadoControl estado, bool modoOscuro,
            bool soloPrimerPlano)
        {
            if (control == null || control.IsDisposed) return;

            foreach (ValorColor valor in estado.Colores)
            {
                try
                {
                    if (control.Name == "btnEliminarCuenta")
                    {
                        valor.Propiedad.SetValue(control, valor.ValorOriginal, null);
                        continue;
                    }
                    bool esPrimerPlano = EsPropiedadDePrimerPlano(valor.Propiedad.Name);
                    if (esPrimerPlano != soloPrimerPlano) continue;
                    object colorOriginal = valor.ValorOriginal;
                    if (!modoOscuro)
                    {
                        if (PaletaAplicacion.TieneColoresPersonalizados && colorOriginal is Color)
                        {
                            Color original = (Color)colorOriginal;
                            if (original.IsEmpty || original.A == 0)
                            {
                                valor.Propiedad.SetValue(control, colorOriginal, null);
                                continue;
                            }
                            Color colorClaro = TransformarPaleta(control,
                                original, valor.Propiedad.Name);
                            valor.Propiedad.SetValue(control, colorClaro, null);
                        }
                        else valor.Propiedad.SetValue(control, colorOriginal, null);
                        continue;
                    }

                    if (!(colorOriginal is Color)) continue;
                    Color color = (Color)colorOriginal;
                    if (color.IsEmpty || color.A == 0) continue;

                    if (PaletaAplicacion.TieneColoresPersonalizados)
                        color = TransformarPaleta(control, color, valor.Propiedad.Name);

                    Color nuevo = TransformarColor(control, color, valor.Propiedad.Name);
                    valor.Propiedad.SetValue(control, nuevo, null);
                }
                catch { }
            }
        }

        private static Color TransformarPaleta(Control control, Color original, string nombrePropiedad)
        {
            string nombre = control.Name;

            // El botón de eliminar cuenta conserva siempre su color de peligro
            // definido en el diseñador, sin importar la plantilla activa.
            if (nombre == "btnEliminarCuenta") return original;

            if (EsPropiedadDeFondo(nombrePropiedad))
            {
                bool esPropiedadVisual = control is AntdUI.Panel
                    ? nombrePropiedad == "Back"
                    : nombrePropiedad == "BackColor" || nombrePropiedad == "Back" ||
                        nombrePropiedad == "BackHover" || nombrePropiedad == "BackActive" ||
                        nombrePropiedad == "DefaultBack";
                if (esPropiedadVisual)
                {
                    if (control is AntdUI.Button || control is Button)
                        return ObtenerColorBoton(control, nombrePropiedad);
                    Color? fondoNombrado = ObtenerFondoPorNombre(control);
                    if (fondoNombrado.HasValue) return fondoNombrado.Value;
                    if (nombre == "btnCerrar")
                    {
                        if (nombrePropiedad == "BackHover") return PaletaAplicacion.AzulHover;
                        if (nombrePropiedad == "BackActive") return PaletaAplicacion.AzulPrimario;
                        return PaletaAplicacion.BotonCerrar;
                    }
                    if (nombrePropiedad == "BackHover") return PaletaAplicacion.AzulHover;
                    if (nombrePropiedad == "BackActive") return PaletaAplicacion.AzulPrimario;
                }
            }
            if (nombrePropiedad.IndexOf("Border", StringComparison.OrdinalIgnoreCase) >= 0)
                return original == PaletaAplicacion.BordeSuave || original == Color.FromArgb(190, 205, 232)
                    ? PaletaAplicacion.BordeSuave : original;
            if (nombrePropiedad.IndexOf("Back", StringComparison.OrdinalIgnoreCase) >= 0 ||
                nombrePropiedad.IndexOf("Fill", StringComparison.OrdinalIgnoreCase) >= 0 ||
                nombrePropiedad.IndexOf("Surface", StringComparison.OrdinalIgnoreCase) >= 0)
                return PaletaAplicacion.AjustarFondo(original);
            if (EsPropiedadDePrimerPlano(nombrePropiedad))
                return PaletaAplicacion.ColorTextoContraste(control);
            return original;
        }

        /// <summary>
        /// Asigna roles de plantilla por los nombres definidos en los Designer.
        /// Los colores base del recurso ya no determinan el tipo de control.
        /// </summary>
        private static Color? ObtenerFondoPorNombre(Control control)
        {
            if (control == null) return null;
            string nombre = control.Name ?? String.Empty;

            if (control is Form) return PaletaAplicacion.FondoClaro;
            if (nombre == "pnlTop") return PaletaAplicacion.AzulCabecera;

            // La tarjeta que muestra una tarea tiene una superficie propia;
            // las demás tarjetas principales usan el color de tarjeta.
            if (nombre == "pnlTargetaTarea" &&
                (control.FindForm() is frmApariencia || control.Parent is ucTareas))
                return PaletaAplicacion.SuperficieTarea;
            if (nombre == "pnlTargetaTarea" && control.FindForm() is frmTareas)
                return PaletaAplicacion.AzulTarjetaOscura;

            if (nombre == "pnlTextoGrande") return PaletaAplicacion.FondoTextoGrande;
            if (nombre == "pnlTextoPequeño") return PaletaAplicacion.FondoTextoPequeno;
            if (nombre.StartsWith("pnlPequeno", StringComparison.OrdinalIgnoreCase) ||
                nombre.StartsWith("pnlPequeño", StringComparison.OrdinalIgnoreCase))
                return PaletaAplicacion.FondoTextoPequeno;

            if (nombre.StartsWith("pnlSubTargeta", StringComparison.OrdinalIgnoreCase) ||
                nombre.StartsWith("pnlSubTarjeta", StringComparison.OrdinalIgnoreCase) ||
                EsSubTarjeta(control))
                return PaletaAplicacion.FondoSubTarjeta;

            if (nombre == "pnlDiaActual") return PaletaAplicacion.DiaActual;
            if (nombre == "pnlDiaMes") return PaletaAplicacion.DiaNormal;
            if (nombre == "pnlDiaSeleccionado") return PaletaAplicacion.DiaSeleccionado;
            if (nombre == "pnlDiaFueraMes") return PaletaAplicacion.DiaFueraCalendario;
            if (nombre.StartsWith("pnlTareaDia", StringComparison.OrdinalIgnoreCase) ||
                nombre == "pnlTareaFueraDelMes")
                return PaletaAplicacion.PanelDia;

            if (nombre.StartsWith("pnlTargeta", StringComparison.OrdinalIgnoreCase) ||
                nombre == "pnlTarjeta")
                return PaletaAplicacion.AzulTarjetaOscura;

            if (nombre.StartsWith("pnlLayout", StringComparison.OrdinalIgnoreCase) ||
                nombre == "pnlLista" || nombre == "pnlEditor")
                return PaletaAplicacion.PanelClaro;

            return null;
        }

        private static Color ObtenerColorBoton(Control control, string nombrePropiedad)
        {
            Color? color = ObtenerColorBotonPorNombre(control);
            string nombre = control == null ? String.Empty : control.Name ?? String.Empty;
            if ((nombre == "btnBorrar" || nombre == "btnQuitar") && color.HasValue)
                return color.Value;
            if (nombrePropiedad == "BackHover") return PaletaAplicacion.AzulHover;
            if (nombrePropiedad == "BackActive") return PaletaAplicacion.AzulPrimario;
            return color ?? PaletaAplicacion.AzulPrimario;
        }

        private static Color? ObtenerColorBotonPorNombre(Control control)
        {
            if (control == null) return null;
            string nombre = control.Name ?? String.Empty;
            if (nombre == "btnEliminarCuenta") return null;
            if (nombre == "btnCerrar") return PaletaAplicacion.BotonCerrar;
            if (nombre == "btnCancelar") return PaletaAplicacion.AzulPrimario;
            if (nombre == "btnBorrar" || nombre == "btnQuitar")
                return PaletaAplicacion.BotonBorrar;
            if (nombre == "btnAgregarTarea") return PaletaAplicacion.BotonAgregarTarea;
            if (nombre.StartsWith("btnSumar", StringComparison.Ordinal))
                return PaletaAplicacion.BotonSumar;
            if (nombre.StartsWith("btnRestar", StringComparison.Ordinal))
                return PaletaAplicacion.BotonRestar;
            if (nombre == "btnAgregarApp" || nombre == "btnAgregarWeb")
                return PaletaAplicacion.AjustarFondo(PaletaAplicacion.AzulPrimario);
            if (control is AntdUI.Button || control is Button)
                return PaletaAplicacion.AzulPrimario;
            return null;
        }

        private static bool EsSubTarjeta(Control control)
        {
            if (!(control is AntdUI.Panel)) return false;
            string nombre = control.Name ?? String.Empty;
            if (nombre.StartsWith("pnlSubTargeta", StringComparison.OrdinalIgnoreCase) ||
                nombre.StartsWith("pnlSubTarjeta", StringComparison.OrdinalIgnoreCase)) return true;
            return nombre.StartsWith("pnlTargeta", StringComparison.OrdinalIgnoreCase) &&
                control.Parent is AntdUI.Panel && control.Name != "pnlTargetaTarea";
        }

        private static bool EsPropiedadDePrimerPlano(string nombrePropiedad)
        {
            return nombrePropiedad.IndexOf("Fore", StringComparison.OrdinalIgnoreCase) >= 0 ||
                nombrePropiedad.IndexOf("Text", StringComparison.OrdinalIgnoreCase) >= 0 ||
                nombrePropiedad.IndexOf("Icon", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool EsPropiedadDeFondo(string nombrePropiedad)
        {
            return nombrePropiedad.IndexOf("Back", StringComparison.OrdinalIgnoreCase) >= 0 ||
                nombrePropiedad.IndexOf("Fill", StringComparison.OrdinalIgnoreCase) >= 0 ||
                nombrePropiedad.IndexOf("Surface", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Color TransformarColor(Control control, Color original, string nombrePropiedad)
        {
            if (control != null && control.Name == "btnEliminarCuenta") return original;

            if (nombrePropiedad.IndexOf("Border", StringComparison.OrdinalIgnoreCase) >= 0)
                return Color.FromArgb(61, 75, 96);

            if (nombrePropiedad.IndexOf("Fore", StringComparison.OrdinalIgnoreCase) >= 0 ||
                nombrePropiedad.IndexOf("Text", StringComparison.OrdinalIgnoreCase) >= 0 ||
                nombrePropiedad.IndexOf("Icon", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return PaletaAplicacion.ColorTextoContraste(control);
            }

            if (nombrePropiedad.IndexOf("Back", StringComparison.OrdinalIgnoreCase) >= 0 ||
                nombrePropiedad.IndexOf("Fill", StringComparison.OrdinalIgnoreCase) >= 0 ||
                nombrePropiedad.IndexOf("Surface", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                double saturacion = Saturacion(original);
                double luminancia = Luminancia(original);

                // Los colores de marca y los estados (azul, verde, rojo) siguen
                // siendo acentos; solo se sustituyen superficies neutrales.
                if (saturacion > 0.28) return original;
                if (luminancia < 0.08) return Color.FromArgb(21, 29, 44);
                if (luminancia < 0.28) return Color.FromArgb(28, 39, 57);
                if (luminancia < 0.72) return Color.FromArgb(39, 52, 72);
                return Color.FromArgb(31, 42, 60);
            }

            return original;
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

        private static double Saturacion(Color color)
        {
            double maximo = Math.Max(color.R, Math.Max(color.G, color.B)) / 255.0;
            double minimo = Math.Min(color.R, Math.Min(color.G, color.B)) / 255.0;
            if (maximo == 0) return 0;
            return (maximo - minimo) / maximo;
        }
    }
}
