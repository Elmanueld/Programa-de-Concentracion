using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    internal static class D_Comun
    {
        public static void Ejecutar(string procedimiento, Action<SqlCommand> agregarParametros)
        {
            using (SqlConnection conexion = ConexionDB.Instancia.CrearConexion())
            using (SqlCommand comando = CrearComando(conexion, procedimiento))
            {
                if (agregarParametros != null) agregarParametros(comando);
                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        public static object Escalar(string procedimiento, Action<SqlCommand> agregarParametros)
        {
            using (SqlConnection conexion = ConexionDB.Instancia.CrearConexion())
            using (SqlCommand comando = CrearComando(conexion, procedimiento))
            {
                if (agregarParametros != null) agregarParametros(comando);
                conexion.Open();
                return comando.ExecuteScalar();
            }
        }

        public static List<T> Consultar<T>(
            string procedimiento,
            Action<SqlCommand> agregarParametros,
            Func<SqlDataReader, T> mapear)
        {
            List<T> resultados = new List<T>();
            using (SqlConnection conexion = ConexionDB.Instancia.CrearConexion())
            using (SqlCommand comando = CrearComando(conexion, procedimiento))
            {
                if (agregarParametros != null) agregarParametros(comando);
                conexion.Open();
                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read()) resultados.Add(mapear(lector));
                }
            }
            return resultados;
        }

        private static SqlCommand CrearComando(SqlConnection conexion, string procedimiento)
        {
            SqlCommand comando = conexion.CreateCommand();
            comando.CommandText = procedimiento;
            comando.CommandType = CommandType.StoredProcedure;
            return comando;
        }

        public static void Texto(SqlCommand comando, string nombre, int longitud, string valor)
        {
            comando.Parameters.Add(nombre, SqlDbType.VarChar, longitud).Value =
                (object)valor ?? DBNull.Value;
        }

        public static void Entero(SqlCommand comando, string nombre, int valor)
        {
            comando.Parameters.Add(nombre, SqlDbType.Int).Value = valor;
        }

        public static void EnteroNulo(SqlCommand comando, string nombre, int? valor)
        {
            comando.Parameters.Add(nombre, SqlDbType.Int).Value =
                valor.HasValue ? (object)valor.Value : DBNull.Value;
        }

        public static void Booleano(SqlCommand comando, string nombre, bool valor)
        {
            comando.Parameters.Add(nombre, SqlDbType.Bit).Value = valor;
        }

        public static void Fecha(SqlCommand comando, string nombre, DateTime valor)
        {
            comando.Parameters.Add(nombre, SqlDbType.DateTime).Value = valor;
        }

        public static void Caracter(SqlCommand comando, string nombre, char valor)
        {
            comando.Parameters.Add(nombre, SqlDbType.Char, 1).Value = valor;
        }

        public static int LeerEntero(SqlDataReader lector, string columna)
        {
            return Convert.ToInt32(lector[columna]);
        }

        public static bool LeerBooleano(SqlDataReader lector, string columna)
        {
            return Convert.ToBoolean(lector[columna]);
        }

        public static DateTime LeerFecha(SqlDataReader lector, string columna)
        {
            return Convert.ToDateTime(lector[columna]);
        }

        public static string LeerTexto(SqlDataReader lector, string columna)
        {
            return lector.IsDBNull(lector.GetOrdinal(columna))
                ? null
                : Convert.ToString(lector[columna]);
        }

        public static char LeerCaracter(SqlDataReader lector, string columna)
        {
            string valor = Convert.ToString(lector[columna]);
            return String.IsNullOrEmpty(valor) ? '\0' : valor[0];
        }

        public static int? EnteroNullable(object valor)
        {
            return valor == null || valor == DBNull.Value
                ? (int?)null
                : Convert.ToInt32(valor);
        }
    }
}