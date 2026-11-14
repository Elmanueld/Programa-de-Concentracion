using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;
using System.Linq;

namespace CapaNegocio
{
    public class N_Aplicacion
    {
        private readonly D_Aplicacion Datos = new D_Aplicacion();

        public List<E_Aplicacion> BuscarAplicacion(int idUsuario, string buscar)
        {
            ValidarIdUsuario(idUsuario);
            return Datos.BuscarAplicacion(idUsuario, buscar ?? String.Empty);
        }

        public int InsertarAplicacion(int idUsuario, E_Aplicacion aplicacion)
        {
            ValidarIdUsuario(idUsuario);
            ValidarAplicacion(aplicacion);
            ValidarPropietario(idUsuario, aplicacion.IdUsuario);
            aplicacion.IdUsuario = idUsuario;
            return Datos.InsertarAplicacion(idUsuario, aplicacion);
        }

        public void ModificarAplicacion(int idUsuario, E_Aplicacion aplicacion)
        {
            ValidarIdUsuario(idUsuario);
            if (aplicacion == null) throw new ArgumentNullException("aplicacion");
            ValidarPropietario(idUsuario, aplicacion.IdUsuario);
            ValidarId(aplicacion.IdAplicacion);
            ValidarAplicacion(aplicacion);
            Datos.ModificarAplicacion(idUsuario, aplicacion);
        }

        public void EliminarAplicacion(int idUsuario, int idAplicacion)
        {
            ValidarIdUsuario(idUsuario);
            ValidarId(idAplicacion);
            Datos.EliminarAplicacion(idUsuario, idAplicacion);
        }

        private static void ValidarAplicacion(E_Aplicacion aplicacion)
        {
            if (aplicacion == null) throw new ArgumentNullException("aplicacion");
            if (String.IsNullOrWhiteSpace(aplicacion.Nombre))
                throw new ArgumentException("El nombre de la aplicación es obligatorio.");
            if (aplicacion.Nombre.Length > 100)
                throw new ArgumentException("El nombre no puede exceder 100 caracteres.");
            if (String.IsNullOrWhiteSpace(aplicacion.Identificador))
                throw new ArgumentException("El identificador de la aplicación es obligatorio.");
            if (aplicacion.Identificador.Length > 255)
                throw new ArgumentException("El identificador no puede exceder 255 caracteres.");
        }

        private static void ValidarId(int id)
        {
            if (id == 0) throw new ArgumentException("El ID de aplicación no es válido.");
        }

        public List<E_Aplicacion> OrdenarAppsPorBusqueda(
        IEnumerable<E_Aplicacion> aplicaciones,
        string termino)
            {
                return new D_ExploradorAplicaciones()
                    .OrdenarPorRelevancia(aplicaciones, termino);
        }

        public List<E_Aplicacion> BuscarAppsInstaladas(int idUsuario)
        {
            ValidarIdUsuario(idUsuario);
            var instaladas =
                new D_ExploradorAplicaciones().BuscarAppsInstaladas();

            var registradas = Datos.BuscarAplicacion(idUsuario, "");

            foreach (E_Aplicacion app in instaladas)
            {
                app.IdUsuario = idUsuario;
                E_Aplicacion existente = registradas.FirstOrDefault(
                    guardada => String.Equals(
                        guardada.Identificador,
                        app.Identificador,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

                if (existente != null)
                {
                    app.IdAplicacion = existente.IdAplicacion;
                    app.EsCritica = existente.EsCritica;
                }
            }

            return instaladas
                .OrderBy(app => app.Nombre)
                .ToList();
        }

        private static void ValidarIdUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El ID de usuario no es válido.");
        }

        private static void ValidarPropietario(int idUsuario, int propietario)
        {
            if (propietario > 0 && propietario != idUsuario)
                throw new ArgumentException("La aplicación pertenece a otro usuario.");
        }
    }
}
