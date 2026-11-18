using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    internal static class D_Comun
    {
        public static bool EsFalloDeConexion(SqlException excepcion)
        {
            if (excepcion == null) return false;
            foreach (SqlError error in excepcion.Errors)
            {
                switch (error.Number)
                {
                    case -2: case 2: case 53: case 64: case 121: case 233:
                    case 258: case 10053: case 10054: case 10060: case 11001:
                    case 4060:
                    // Azure SQL: base pausada, ocupada o con el firewall cerrado
                    case 40613: case 40197: case 40501: case 40615:
                    case 49918: case 49919: case 49920:
                        return true;
                }
            }
            return false;
        }

        public static void Ejecutar(string procedimiento, Action<SqlCommand> agregarParametros)
        {
            using (SqlCommand comando = CrearComando(procedimiento))
            {
                if (agregarParametros != null) agregarParametros(comando);
                ClienteApiDatos.Ejecutar(procedimiento, comando);
            }
        }

        public static object Escalar(string procedimiento, Action<SqlCommand> agregarParametros)
        {
            using (SqlCommand comando = CrearComando(procedimiento))
            {
                if (agregarParametros != null) agregarParametros(comando);
                return ClienteApiDatos.Escalar(procedimiento, comando);
            }
        }

        public static List<T> Consultar<T>(
            string procedimiento,
            Action<SqlCommand> agregarParametros,
            Func<IDataRecord, T> mapear)
        {
            using (SqlCommand comando = CrearComando(procedimiento))
            {
                if (agregarParametros != null) agregarParametros(comando);
                var tabla = ClienteApiDatos.Consultar(procedimiento, comando);
                var resultados = new List<T>();
                using (var lector = tabla.CreateDataReader())
                    while (lector.Read()) resultados.Add(mapear(lector));
                return resultados;
            }
        }

        private static SqlCommand CrearComando(string procedimiento)
        {
            SqlCommand comando = new SqlCommand();
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

        public static int LeerEntero(IDataRecord lector, string columna)
        {
            return Convert.ToInt32(lector[columna]);
        }

        public static bool LeerBooleano(IDataRecord lector, string columna)
        {
            return Convert.ToBoolean(lector[columna]);
        }

        public static DateTime LeerFecha(IDataRecord lector, string columna)
        {
            return Convert.ToDateTime(lector[columna]);
        }

        public static string LeerTexto(IDataRecord lector, string columna)
        {
            return lector.IsDBNull(lector.GetOrdinal(columna))
                ? null
                : Convert.ToString(lector[columna]);
        }

        public static char LeerCaracter(IDataRecord lector, string columna)
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
