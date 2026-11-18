# Estado actual del proyecto

El estado validado en esta revisión está descrito en [README](../README.md) y en el [reporte de pruebas](REPORTE_DE_PRUEBAS.md). La aplicación conserva SQLite local como copia offline; el Backend ASP.NET Core usa SQL Server remoto para el servicio online.

## Componentes incluidos

- WinForms con registro, inicio de sesión, tareas, preferencias y pantallas de aplicaciones/webs.
- Capa de datos con SQLite local, cliente HTTP y sincronización offline/online.
- Backend .NET 9 para API remota, verificación y recuperación de contraseña.
- Scripts SQL de instalación/migración y pruebas automatizadas del Backend y cliente.

## Validación de esta revisión

- Compilación Release de `ProgramaConcentracion.sln`: completada sin errores.
- Suite WinForms: 13 comprobaciones generales aprobadas.
- Suite WinForms con interfaz: 25 comprobaciones aprobadas.
- `Backend.Tests`: 26 comprobaciones aprobadas.
- SQL remoto y SMTP real: no probados; requieren entorno de staging y datos desechables.

La compilación reportó dos advertencias C# ya existentes en `Tipografia.cs` y `frmSeleccionarApp.Designer.cs`. NuGet también mostró `NU1900` porque el entorno no pudo consultar su servicio de vulnerabilidades; la restauración, compilación y pruebas terminaron correctamente.

## Pendiente

- Ejecutar una validación manual de SQL, migraciones y aislamiento entre dos usuarios en staging.
- Probar SMTP con un buzón de prueba.
- Revisar visualmente los formularios en los equipos y resoluciones objetivo.
- Confirmar el workflow de GitHub Actions tras subir la rama.
