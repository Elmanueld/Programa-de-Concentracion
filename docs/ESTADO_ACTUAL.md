# Estado actual del proyecto

## Lo que queda incluido en este push

- Inicio de sesión, creación de cuenta y vista de código de verificación.
- Ajustes, apariencia, tareas y pantallas de selección de aplicaciones/webs implementadas en los formularios actuales.
- Catálogos y tareas con acceso filtrado por `IdUsuario` en las capas de datos/negocio y en los procedimientos.
- Copia SQLite offline con cola de cambios y sincronización al recuperar la conexión SQL Server.
- Scripts de instalación y migraciones sincronizados con las firmas de procedimientos usadas por el código.
- Paginación con controles AntdUI y cobertura de regresión para no perder tarjetas entre páginas.

## Pruebas

Compilación Release: 0 errores. Pruebas ejecutadas el 14/11/2026: 22 unitarias, 34 de interfaz y 71 de integración. Ver [reporte](REPORTE_DE_PRUEBAS.md) y [logs](../tests/evidencias/README.md).

La integración se limitó a `(localdb)\Entregable3Pruebas`. No se ejecutó contra la base del usuario.

## Pendiente para otra fase

- Configurar y validar el SMTP real con una cuenta remitente del grupo.
- Configurar el servidor/base de datos en nube y las credenciales seguras para ese destino.
- Hacer una revisión visual manual de todas las pantallas en los tamaños de pantalla del equipo de entrega.

El proyecto puede compilar y ejecutar localmente con la configuración disponible; estos puntos son tareas de despliegue y validación final, no bloquean la publicación del código actual.
