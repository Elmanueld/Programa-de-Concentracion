using System;
using System.Configuration;
using System.Data.SqlClient;

namespace CapaDatos
{
    public sealed class ConexionDB
    {
        private readonly string cadenaConexion;
        private static readonly Lazy<ConexionDB> instancia =
            new Lazy<ConexionDB>(() => new ConexionDB());

        public static ConexionDB Instancia
        {
            get { return instancia.Value; }
        }

        public string CadenaConexion
        {
            get { return cadenaConexion; }
        }

        private ConexionDB()
        {
            ConnectionStringSettings configuracion =
                ConfigurationManager.ConnectionStrings["conexion"];

            if (configuracion == null || String.IsNullOrWhiteSpace(configuracion.ConnectionString))
                throw new InvalidOperationException("No se encontró la cadena de conexión 'conexion' en el archivo de configuración.");

            cadenaConexion = configuracion.ConnectionString;
        }

        public SqlConnection CrearConexion()
        {
            return new SqlConnection(cadenaConexion);
        }
    }
}