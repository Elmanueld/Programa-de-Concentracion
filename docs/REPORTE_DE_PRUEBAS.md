# Reporte de pruebas

**Fecha:** 14 de noviembre de 2026
**Rama:** `main`
**Build:** Release, .NET Framework 4.8, MSBuild de Visual Studio 2022

## Resultado

| Área | Resultado | Evidencia |
| --- | --- | --- |
| Compilación de `tests/Pruebas.csproj` | Aprobada, 0 errores, 2 advertencias de código existentes | [log](../tests/evidencias/compilacion.log) · [resumen](../tests/evidencias/evidencia-compilacion.png) |
| Unitarias/componentes | **22 aprobadas** | [log](../tests/evidencias/unitarias.log) · [resumen](../tests/evidencias/evidencia-unitarias.png) |
| Interfaz WinForms | **34 aprobadas** (22 base + 12 de interfaz) | [log](../tests/evidencias/interfaz.log) · [resumen](../tests/evidencias/evidencia-interfaz.png) |
| Integración LocalDB | **68 aprobadas** (22 base + 46 SQL) | [log](../tests/evidencias/integracion.log) · [resumen](../tests/evidencias/evidencia-integracion.png) |

La integración corrió únicamente en `(localdb)\Entregable3Pruebas`. Las cuentas y datos temporales se limpian al terminar. La prueba SMTP utiliza un servidor TCP local y no envía correo a cuentas reales. La base SQLite de prueba usa una ruta temporal configurada por el ejecutable y se elimina al finalizar.

## Correcciones verificadas

- La paginación conserva el total de tarjetas y no pierde elementos al alternar de página; el contador muestra `1 / 2` cuando corresponde.
- Las flechas AntdUI caben completas en la barra y los nombres de tarjetas quedan visibles.
- Los scripts de instalación ya pasan los parámetros de sincronización que envía la capa de datos; se eliminó el desajuste que causaba SQL 8144 al guardar una plantilla.
- Dos propietarios pueden tener los mismos valores de catálogo/tarea sin consultar ni modificar filas del otro.
- Las tareas activas y completadas no se editan ni eliminan, conforme al comportamiento actual del formulario.
- Las pruebas se ejecutan sin preferencia de proceso de 32 bits, alineadas con la aplicación y con `winsqlite3.dll`.

## Límites de esta revisión

La automatización no sustituye la comprobación visual manual de todos los formularios. El SMTP real y el destino en la nube todavía no están configurados, tal como queda planificado para una fase posterior.

Consulta el [plan](PLAN_PRUEBAS.md), las [incidencias](INCIDENCIAS_PRUEBAS.md) y las [evidencias completas](../tests/evidencias/README.md).
