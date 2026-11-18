using System;
using System.Collections.Generic;

namespace CapaDatos
{
    public sealed class RegistroPendiente
    {
        public int IdUsuarioLocal { get; set; }
        public string Correo { get; set; }
        public string Nombre { get; set; }
        public string HashProtegido { get; set; }
        public int? IdUsuarioRemoto { get; set; }
    }

    public sealed class D_RegistroPendiente
    {
        public RegistroPendiente BuscarPorCorreo(string correo)
        {
            ValidarCorreo(correo);
            List<RegistroPendiente> registros = SQLiteLocal.Consultar(
                @"SELECT IdUsuarioLocal,Correo,Nombre,HashProtegido,IdUsuarioRemoto
                  FROM RegistrosPendientes WHERE Correo=? LIMIT 1",
                fila => new RegistroPendiente
                {
                    IdUsuarioLocal = fila.Entero("IdUsuarioLocal"),
                    Correo = fila.Texto("Correo"),
                    Nombre = fila.Texto("Nombre"),
                    HashProtegido = fila.Texto("HashProtegido"),
                    IdUsuarioRemoto = fila["IdUsuarioRemoto"] == null
                        ? (int?)null
                        : Convert.ToInt32(fila["IdUsuarioRemoto"])
                }, correo.Trim());
            return registros.Count == 0 ? null : registros[0];
        }

        public int Crear(string correo, string nombre, string hashProtegido)
        {
            ValidarCorreo(correo);
            if (String.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 50)
                throw new ArgumentException("El nombre del registro no es válido.");
            if (String.IsNullOrWhiteSpace(hashProtegido))
                throw new ArgumentException("El registro local protegido no es válido.");

            return SQLiteLocal.EnTransaccion(() =>
            {
                object siguiente = SQLiteLocal.Escalar(
                    "SELECT COALESCE(MIN(IdUsuarioLocal),0)-1 FROM RegistrosPendientes");
                int idUsuarioLocal = Convert.ToInt32(siguiente);
                SQLiteLocal.Ejecutar(@"INSERT INTO RegistrosPendientes
                    (IdUsuarioLocal,Correo,Nombre,HashProtegido,IdUsuarioRemoto)
                    VALUES(?,?,?,?,NULL)", idUsuarioLocal, correo.Trim(), nombre.Trim(), hashProtegido);
                return idUsuarioLocal;
            });
        }

        public void AsociarUsuarioRemoto(int idUsuarioLocal, int idUsuarioRemoto)
        {
            ValidarIds(idUsuarioLocal, idUsuarioRemoto);
            SQLiteLocal.EnTransaccion(() =>
            {
                var registros = SQLiteLocal.Consultar(
                    "SELECT Correo,Nombre FROM RegistrosPendientes WHERE IdUsuarioLocal=?",
                    fila => new { Correo = fila.Texto("Correo"), Nombre = fila.Texto("Nombre") }, idUsuarioLocal);
                if (registros.Count == 0)
                    throw new InvalidOperationException("El registro local pendiente no existe.");

                var cuentasLocales = SQLiteLocal.Consultar(
                    "SELECT Correo FROM UsuariosLocales WHERE IdUsuario=?",
                    fila => fila.Texto("Correo"), idUsuarioRemoto);
                if (cuentasLocales.Count > 0 &&
                    !String.Equals(cuentasLocales[0], registros[0].Correo, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("El identificador remoto ya está asociado a otra cuenta local.");

                DateTime ahora = DateTime.UtcNow;
                SQLiteLocal.Ejecutar(@"INSERT OR IGNORE INTO UsuariosLocales
                    (IdUsuario,Correo,Nombre,SyncUpdatedUtc,SyncDeleted) VALUES(?,?,?,?,0)",
                    idUsuarioRemoto, registros[0].Correo, registros[0].Nombre, ahora);
                SQLiteLocal.Ejecutar(@"UPDATE RegistrosPendientes SET IdUsuarioRemoto=?
                    WHERE IdUsuarioLocal=?", idUsuarioRemoto, idUsuarioLocal);
            });
        }

        public void Eliminar(int idUsuarioLocal)
        {
            if (idUsuarioLocal >= 0) throw new ArgumentException("El ID del registro local no es válido.");
            SQLiteLocal.Ejecutar("DELETE FROM RegistrosPendientes WHERE IdUsuarioLocal=?", idUsuarioLocal);
        }

        private static void ValidarIds(int idUsuarioLocal, int idUsuarioRemoto)
        {
            if (idUsuarioLocal >= 0) throw new ArgumentException("El ID del registro local no es válido.");
            if (idUsuarioRemoto <= 0) throw new ArgumentException("El ID remoto no es válido.");
        }

        private static void ValidarCorreo(string correo)
        {
            if (String.IsNullOrWhiteSpace(correo) || correo.Trim().Length > 254)
                throw new ArgumentException("El correo del registro local no es válido.");
        }
    }
}
