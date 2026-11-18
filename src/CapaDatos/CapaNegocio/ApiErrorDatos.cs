using System;
using CapaDatos;

namespace CapaNegocio
{
    public sealed class ApiErrorDatos
    {
        public int CodigoHttp { get; private set; }
        public string Mensaje { get; private set; }
        public string Referencia { get; private set; }

        private ApiErrorDatos(ApiDatosException excepcion)
        {
            CodigoHttp = (int)excepcion.StatusCode;
            Mensaje = excepcion.Message;
            Referencia = excepcion.Referencia;
        }

        public static ApiErrorDatos Desde(Exception excepcion)
        {
            var error = excepcion as ApiDatosException;
            return error == null ? null : new ApiErrorDatos(error);
        }
    }
}
