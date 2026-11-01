# Reporte de pruebas

**Fecha:** 1 de noviembre de 2026
**Rama:** `reporte-de-pruebas`
**Build:** Release, .NET Framework 4.8, MSBuild de Visual Studio 2022

## Resultado

| Área | Estado | Evidencia |
| --- | --- | --- |
| Compilación de `tests/Pruebas.csproj` | Aprobada | [log](../tests/evidencias/compilacion.log) · [captura](../tests/evidencias/evidencia-compilacion.png) |
| Unitarias y componentes | **22 comprobaciones aprobadas** | [log](../tests/evidencias/unitarias.log) · [captura](../tests/evidencias/evidencia-unitarias.png) |
| Integración con LocalDB | **62 comprobaciones aprobadas** | [log](../tests/evidencias/integracion.log) · [captura](../tests/evidencias/evidencia-integracion.png) |
| Interfaz WinForms | **6 comprobaciones de interfaz aprobadas** (28 en total con los casos base) | [log](../tests/evidencias/interfaz.log) · [captura](../tests/evidencias/evidencia-interfaz.png) |

La integración se ejecutó en `(localdb)\Entregable3Pruebas`. Las pruebas generan cuentas con correos aleatorios y limpian esos registros al finalizar. El envío de códigos se comprobó con SMTP local; no se contactaron buzones reales.

## Capturas de los resultados

Estas imágenes muestran la salida real de los logs con un formato legible. Los logs completos siguen disponibles en texto.

- [Compilación](../tests/evidencias/evidencia-compilacion.png)
- [Pruebas unitarias y de componentes](../tests/evidencias/evidencia-unitarias.png)
- [Integración con LocalDB](../tests/evidencias/evidencia-integracion.png)
- [Prueba de interfaz](../tests/evidencias/evidencia-interfaz.png)

## Incidencia durante la ejecución

La primera integración encontró que la instancia de pruebas no tenía `sp_PrepararVerificacionCorreo`. Se aplicó `database/07_VerificacionCorreo.sql`, que añade o actualiza la tabla y los procedimientos de verificación sin borrar datos. La segunda integración pasó las 62 comprobaciones.

El detalle de casos y comandos reproducibles está en [Plan de pruebas](PLAN_PRUEBAS.md); el registro de correcciones está en [Incidencias](INCIDENCIAS_PRUEBAS.md).
