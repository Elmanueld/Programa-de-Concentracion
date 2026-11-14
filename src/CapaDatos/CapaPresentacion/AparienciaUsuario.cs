using System;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace CapaPresentacion
{
    [Serializable]
    public sealed class PreferenciasApariencia
    {
        // Apariencia base original de la aplicación. Se usa como plantilla
        // estable para usuarios que todavía no han guardado una personalización.
        public static PreferenciasApariencia PorDefecto
        {
            get
            {
                return new PreferenciasApariencia
                {
                    // Valores del diseñador de frmApariencia, conservados como
                    // plantilla inmutable para iniciar y restablecer la vista.
                    SuperficieTarea = SystemColors.ActiveCaption.ToArgb(),
                    SuperficieClara = Color.LightSteelBlue.ToArgb(),
                    BotonSumar = Color.FromArgb(89, 133, 225).ToArgb(),
                    BotonRestar = Color.FromArgb(71, 104, 180).ToArgb(),
                    BotonCerrar = Color.FromArgb(89, 133, 225).ToArgb(),
                    DiaNormal = Color.FromArgb(75, 119, 209).ToArgb(),
                    DiaActual = Color.FromArgb(22, 119, 255).ToArgb()
                };
            }
        }
        public string FuenteActual { get; set; } = "Inter";
        public string TamanoFuente { get; set; } = "Normal";
        public int ColorTextoFondoOscuro { get; set; } = Color.White.ToArgb();
        public int ColorTextoFondoClaro { get; set; } = Color.Black.ToArgb();
        public int TextoTarjetaEjemplo { get; set; } = Color.White.ToArgb();
        public int TextoDescripcionTarjeta { get; set; } = Color.LightSteelBlue.ToArgb();
        public int FondoTextoGrande { get; set; } = Color.FromArgb(75, 119, 209).ToArgb();
        public int FondoTextoPequeno { get; set; } = Color.FromArgb(75, 119, 209).ToArgb();
        public int IconosTarjeta { get; set; } = Color.White.ToArgb();
        public int IconosPrincipal { get; set; } = Color.White.ToArgb();
        public int FondoVentana { get; set; } = Color.Lavender.ToArgb();
        public int FondoOscuro { get; set; } = Color.FromArgb(24, 33, 51).ToArgb();
        public int SuperficieOscura { get; set; } = Color.FromArgb(35, 48, 70).ToArgb();
        public int Cabecera { get; set; } = Color.FromArgb(71, 104, 180).ToArgb();
        public int TextoCabecera { get; set; } = Color.White.ToArgb();
        public int Primario { get; set; } = Color.FromArgb(75, 119, 209).ToArgb();
        public int PrimarioHover { get; set; } = Color.FromArgb(89, 133, 225).ToArgb();
        public int FondoSubTarjeta { get; set; } = Color.FromArgb(75, 119, 209).ToArgb();
        public int TarjetaOscura { get; set; } = Color.FromArgb(37, 63, 118).ToArgb();
        public int SuperficieClara { get; set; } = Color.LightSteelBlue.ToArgb();
        public int SuperficieTarea { get; set; } = SystemColors.ActiveCaption.ToArgb();
        public int BotonSumar { get; set; } = Color.FromArgb(89, 133, 225).ToArgb();
        public int BotonRestar { get; set; } = Color.FromArgb(71, 104, 180).ToArgb();
        public int Exito { get; set; } = Color.FromArgb(36, 116, 62).ToArgb();
        public int BotonCerrar { get; set; } = Color.FromArgb(71, 104, 180).ToArgb();
        public int BotonBorrar { get; set; } = Color.FromArgb(71, 104, 180).ToArgb();
        public int BotonAgregarTarea { get; set; } = Color.FromArgb(36, 116, 62).ToArgb();
        public int PanelDia { get; set; } = Color.White.ToArgb();
        public int DiaNormal { get; set; } = Color.FromArgb(75, 119, 209).ToArgb();
        public int DiaSeleccionado { get; set; } = Color.FromArgb(30, 165, 145).ToArgb();
        public int DiaActual { get; set; } = Color.FromArgb(22, 119, 255).ToArgb();
        public int DiaFueraDeMes { get; set; } = Color.FromArgb(82, 91, 110).ToArgb();
        public int BordeSuave { get; set; } = Color.FromArgb(190, 205, 232).ToArgb();

        public PreferenciasApariencia Copiar()
        {
            return new PreferenciasApariencia
            {
                FuenteActual = FuenteActual,
                TamanoFuente = TamanoFuente,
                ColorTextoFondoOscuro = ColorTextoFondoOscuro,
                ColorTextoFondoClaro = ColorTextoFondoClaro,
                TextoTarjetaEjemplo = TextoTarjetaEjemplo,
                TextoDescripcionTarjeta = TextoDescripcionTarjeta,
                FondoTextoGrande = FondoTextoGrande,
                FondoTextoPequeno = FondoTextoPequeno,
                IconosTarjeta = IconosTarjeta,
                IconosPrincipal = IconosPrincipal,
                FondoVentana = FondoVentana,
                FondoOscuro = FondoOscuro,
                SuperficieOscura = SuperficieOscura,
                Cabecera = Cabecera,
                TextoCabecera = TextoCabecera,
                Primario = Primario,
                PrimarioHover = PrimarioHover,
                FondoSubTarjeta = FondoSubTarjeta,
                TarjetaOscura = TarjetaOscura,
                SuperficieClara = SuperficieClara,
                SuperficieTarea = SuperficieTarea,
                BotonSumar = BotonSumar,
                BotonRestar = BotonRestar,
                Exito = Exito,
                BotonCerrar = BotonCerrar,
                BotonBorrar = BotonBorrar,
                BotonAgregarTarea = BotonAgregarTarea,
                PanelDia = PanelDia,
                DiaNormal = DiaNormal,
                DiaSeleccionado = DiaSeleccionado,
                DiaActual = DiaActual,
                DiaFueraDeMes = DiaFueraDeMes,
                BordeSuave = BordeSuave
            };
        }

        public Color ColorTextoOscuro { get { return Color.FromArgb(ColorTextoFondoOscuro); } }
        public Color ColorTextoClaro { get { return Color.FromArgb(ColorTextoFondoClaro); } }
    }

    public static class AparienciaUsuarioApp
    {
        private static string Ruta(int idUsuario)
        {
            if (idUsuario <= 0) throw new ArgumentException("El usuario no es válido.");
            string carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programa-de-Concentracion", "Usuarios", idUsuario.ToString());
            Directory.CreateDirectory(carpeta);
            return Path.Combine(carpeta, "apariencia.xml");
        }

        public static PreferenciasApariencia Cargar(int idUsuario)
        {
            if (idUsuario <= 0) return PreferenciasApariencia.PorDefecto;
            string ruta = Ruta(idUsuario);
            if (!File.Exists(ruta)) return PreferenciasApariencia.PorDefecto;
            using (var archivo = File.OpenRead(ruta))
                return (PreferenciasApariencia)new XmlSerializer(typeof(PreferenciasApariencia)).Deserialize(archivo);
        }

        public static void Guardar(int idUsuario, PreferenciasApariencia preferencias)
        {
            string ruta = Ruta(idUsuario);
            string temporal = ruta + ".tmp";
            using (var archivo = File.Create(temporal))
                new XmlSerializer(typeof(PreferenciasApariencia)).Serialize(archivo, preferencias);
            if (File.Exists(ruta)) File.Delete(ruta);
            File.Move(temporal, ruta);
        }

        public static PreferenciasApariencia Aplicar(int idUsuario)
        {
            PreferenciasApariencia preferencias = Cargar(idUsuario);
            PaletaAplicacion.Aplicar(preferencias);
            return preferencias;
        }
    }
}
