# Registro de incidencias

Incidencias corregidas en la implementación que cubre este plan. La existencia de la corrección se verificó en el código; su regresión queda cubierta por los casos indicados y los resultados de ejecución se registran en `PLAN_PRUEBAS.md`.

| ID | Incidencia | Corrección | Cobertura |
| --- | --- | --- | --- |
| INC-01 | La contraseña podía quedar expuesta si se almacenaba o se devolvía directamente. | Se usa PBKDF2-SHA256 con sal aleatoria; el hash se elimina del objeto de sesión. | U01–U04, I01–I02 |
| INC-02 | El procedimiento de login de instalaciones anteriores no devolvía `Nombre`, lo que podía romper la lectura del usuario. | La lectura tolera la columna ausente y recupera el nombre después de autenticar. | U07, I03 |
| INC-03 | Una configuración SMTP incompleta o SMTP remoto sin TLS podía fallar tarde o usar una conexión insegura. | Se valida la configuración antes del envío; TLS remoto es obligatorio y localhost queda permitido para pruebas. | U09–U10 |
| INC-04 | El código de verificación requería control de expiración, intentos y reenvío sin almacenar el código original. | Se almacena el resumen con sal; se limita vigencia, intentos y frecuencia de envío, y el código es de un solo uso. | U08–U10, I01 |
| INC-05 | Las operaciones de tarea podían aceptar un ID sin comprobar que perteneciera al usuario autenticado. | Las operaciones reciben el ID de usuario y SQL comprueba propietario y resultado afectado. | I04–I05 |
| INC-06 | La edición de tarea solo admitía estado pendiente, aunque el requisito permite editar completadas. | La regla permite editar estados distintos de activo y mantiene la restricción para tareas activas. | I06 |
| INC-07 | Un borrado de tarea inexistente podía terminar sin indicar que no hubo cambios. | El procedimiento informa el resultado fallido y la capa de negocio lo propaga. | I06 |
| INC-08 | La primera integración encontró que `Entregable3Pruebas` no tenía `sp_PrepararVerificacionCorreo` porque su esquema era anterior. | Se aplicó `database/07_VerificacionCorreo.sql`, que crea/actualiza los objetos de verificación sin borrar datos; la repetición completó 62 comprobaciones. | I01; `tests/evidencias/integracion.log` |

Si aparece otra incidencia, añadir pasos para reproducirla, resultado esperado/obtenido, corrección y caso de regresión.
