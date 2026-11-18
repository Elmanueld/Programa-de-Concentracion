using System.Collections.Generic;
using CapaEntidades;

namespace CapaDatos
{
    public class D_Usuario
    {
        public void RegistrarUsuario(E_Usuario usuario)
        {
            D_Comun.Ejecutar("sp_RegistrarUsuario", comando =>
            {
                D_Comun.Texto(comando, "@Nombre", 50, usuario.Nombre);
                D_Comun.Texto(comando, "@Correo", 254, usuario.Correo);
                D_Comun.Texto(comando, "@ContrasenaHash", 255, usuario.ContrasenaHash);
            });
        }

        public string BuscarNombreUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
                throw new System.ArgumentException("El ID de usuario no es válido.");
            return ClienteApiDatos.BuscarNombreUsuario(idUsuario);
        }

        public E_Usuario IniciarSesion(string correo, string contrasena)
        {
            var respuesta = ClienteApiDatos.IniciarSesion(correo, contrasena);
            return new E_Usuario
            {
                IdUsuario = respuesta.IdUsuario,
                Nombre = respuesta.Nombre,
                Correo = respuesta.Correo
            };
        }

        public E_Usuario IniciarSesionConHash(string correo, string hashGuardado)
        {
            var respuesta = ClienteApiDatos.IniciarSesion(correo, null, hashGuardado);
            return new E_Usuario
            {
                IdUsuario = respuesta.IdUsuario,
                Nombre = respuesta.Nombre,
                Correo = respuesta.Correo
            };
        }

        internal static E_Usuario LeerSesion(System.Data.IDataRecord lector)
        {
            var usuario = new E_Usuario
            {
                IdUsuario = System.Convert.ToInt32(lector["IdUsuario"]),
                Correo = System.Convert.ToString(lector["Correo"]),
                ContrasenaHash = System.Convert.ToString(lector["ContrasenaHash"])
            };
            // Algunas instalaciones conservan el procedimiento anterior, sin Nombre.
            for (int i = 0; i < lector.FieldCount; i++)
            {
                if (System.String.Equals(lector.GetName(i), "Nombre", System.StringComparison.OrdinalIgnoreCase))
                    usuario.Nombre = lector.IsDBNull(i) ? "" : System.Convert.ToString(lector.GetValue(i));
            }
            return usuario;
        }

        public void ModificarUsuario(E_Usuario usuario)
        {
            D_Comun.Ejecutar("sp_ModificarUsuario", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", usuario.IdUsuario);
                D_Comun.Texto(comando, "@Nombre", 50, usuario.Nombre);
                D_Comun.Texto(comando, "@Correo", 254, usuario.Correo);
            });
        }

        public void EliminarUsuario(int idUsuario)
        {
            D_Comun.Ejecutar("sp_EliminarUsuario", comando =>
                D_Comun.Entero(comando, "@IdUsuario", idUsuario));
        }

        public void VerificarCorreo(int idUsuario)
        {
            D_Comun.Ejecutar("sp_VerificarCorreo", comando =>
                D_Comun.Entero(comando, "@IdUsuario", idUsuario));
        }

        public void CambiarContrasena(int idUsuario, string contrasenaHashNueva)
        {
            D_Comun.Ejecutar("sp_CambiarContrasena", comando =>
            {
                D_Comun.Entero(comando, "@IdUsuario", idUsuario);
                D_Comun.Texto(comando, "@ContrasenaHashNueva", 255, contrasenaHashNueva);
            });
        }

        public void RecuperarCuenta(int idUsuario)
        {
            D_Comun.Ejecutar("sp_RecuperarCuenta", comando =>
                D_Comun.Entero(comando, "@IdUsuario", idUsuario));
        }

        public void CerrarSesionRemota()
        {
            ClienteApiDatos.CerrarSesion();
        }
    }
}
