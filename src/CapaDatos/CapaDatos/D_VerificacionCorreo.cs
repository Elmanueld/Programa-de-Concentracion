using System;

namespace CapaDatos
{
    public class D_VerificacionCorreo
    {
        public bool Preparar(string correo, string sal, string hash)
        {
            return Convert.ToBoolean(D_Comun.Escalar("sp_PrepararVerificacionCorreo", comando =>
            {
                D_Comun.Texto(comando, "@Correo", 254, correo);
                D_Comun.Texto(comando, "@Sal", 24, sal);
                D_Comun.Texto(comando, "@CodigoHash", 64, hash);
            }));
        }

        public string ConsultarSal(string correo)
        {
            return Convert.ToString(D_Comun.Escalar("sp_ConsultarSalVerificacion", comando =>
                D_Comun.Texto(comando, "@Correo", 254, correo)));
        }

        public bool Confirmar(string correo, string hash)
        {
            return Convert.ToBoolean(D_Comun.Escalar("sp_ConfirmarVerificacionCorreo", comando =>
            {
                D_Comun.Texto(comando, "@Correo", 254, correo);
                D_Comun.Texto(comando, "@CodigoHash", 64, hash);
            }));
        }
    }
}
