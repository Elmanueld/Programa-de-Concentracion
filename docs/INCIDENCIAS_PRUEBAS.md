# Registro de incidencias de pruebas

| ID | Hallazgo | Acción | Estado |
| --- | --- | --- | --- |
| TEST-01 | El test heredado buscaba `CapaNegocio.CorreoVerificacion`, eliminado al mover SMTP y códigos al Backend. | Se sustituyó por pruebas del servicio actual con dobles; no se conecta a un SMTP real. | Corregido |
| TEST-02 | La integración heredada apuntaba a `(localdb)\Entregable3Pruebas` y asumía verificación en SQL desde WinForms. | Se retiró del workflow y se documentó como no soportada por la arquitectura actual. | Corregido en CI; integración de staging pendiente |
| TEST-03 | El workflow manual enviaba correos reales usando secretos del repositorio. | Se eliminó de CI para evitar mensajes externos durante pruebas. | Corregido |
| TEST-04 | Los records de solicitudes deben exponer validación en los parámetros del constructor. | Backend.Tests inspecciona los atributos de Email, Code y PasswordHash. | Cubierto |
| TEST-05 | Los servicios no deben devolver éxito al fallar SMTP ni almacenar el código en claro. | Backend.Tests comprueba excepciones y que solo persista un resumen. | Cubierto |

Los logs de pruebas anteriores se conservan como evidencia histórica y no describen la suite actual. El estado vigente está en `docs/REPORTE_DE_PRUEBAS.md`.