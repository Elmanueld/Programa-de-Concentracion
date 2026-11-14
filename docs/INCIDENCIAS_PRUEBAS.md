# Registro de incidencias

Incidencias encontradas y cubiertas por una corrección o una prueba de regresión en este estado del proyecto.

| ID | Incidencia | Corrección | Cobertura |
| --- | --- | --- | --- |
| INC-01 | La contraseña podía almacenarse o salir de la capa de negocio directamente. | Hash PBKDF2-SHA256 con sal aleatoria; el hash se retira del objeto de sesión. | Unitarias e integración de acceso. |
| INC-02 | Instalaciones antiguas devolvían un resultado de login sin `Nombre`. | El lector admite ambos resultados y obtiene el nombre usando el mismo `IdUsuario`. | Pruebas de compatibilidad de login. |
| INC-03 | SMTP incompleto o remoto sin TLS fallaba tarde o permitía una conexión insegura. | Validación previa, TLS obligatorio para host remoto y SMTP localhost exclusivo para pruebas. | Unitarias y servidor SMTP local. |
| INC-04 | El código de verificación no controlaba expiración, intentos y reenvío de forma persistente. | Se guarda un hash con sal, se aplican vencimiento/límites y el código es de un uso. | Integración SQL. |
| INC-05 | Una operación podía aceptar un ID de fila perteneciente a otra cuenta. | Las llamadas y procedimientos llevan `IdUsuario`, y las relaciones compuestas mantienen el propietario. | Dos usuarios con valores idénticos en tareas, apps y webs. |
| INC-06 | Un procedimiento de instalación tenía una firma antigua y recibía más argumentos que los enviados por C#. | Se alinearon los scripts de instalación con los parámetros actuales y las migraciones. | Preparación desde cero e integración de tareas. |
| INC-07 | El paginador reconstruía su lista usando solo los controles visibles y perdía las tarjetas de otras páginas. | Mantiene un conjunto completo de elementos y actualiza la página sin volver a inferir el origen desde los controles visibles. | Prueba de interfaz con contador `1 / 2`. |
| INC-08 | La prueba LocalDB tenía una ruta MDF predeterminada que colisionaba con otra instalación. | El preparador crea la base de pruebas en una carpeta exclusiva y conserva instancias/bases existentes. | Preparación controlada de `Entregable3Pruebas`. |
| INC-09 | El EXE de pruebas prefería 32 bits aunque la aplicación no lo hace; `winsqlite3.dll` fallaba al leer texto SQLite en el proceso x86. | El proyecto de pruebas desactiva `Prefer32Bit` y la integración usa un archivo offline temporal. | Inicialización SQLite durante integración; suite completa x64. |
| INC-10 | El caso de regresión asumía que se pueden editar tareas completadas, contrario al comportamiento solicitado. | Se cambió el caso para verificar que las tareas completadas no permiten editar/eliminar. | Integración SQL. |

Los comandos y resultados reproducibles están en el [plan de pruebas](PLAN_PRUEBAS.md); los resultados finales en el [reporte](REPORTE_DE_PRUEBAS.md).
