# Revisión de acceso y verificación de correo

Base: `f39cd1da1af91990efa5f886f74cfd039b99d8ac`. Cambios preparados en una rama local, sin publicar.

## Login

`Nombre` no se envía como parámetro de entrada: `sp_IniciarSesion` recibe solamente `@Correo`. El problema aparece cuando una instalación conserva el procedimiento anterior, que devuelve IdUsuario, Correo y ContrasenaHash, mientras el código intenta leer también Nombre.

Ahora se admiten ambos resultados. Si falta Nombre, se consulta mediante el procedimiento de búsqueda existente después de comprobar la contraseña y se selecciona el mismo ID de usuario. No se permite el acceso a una cuenta pendiente ni se cambia la seguridad de las contraseñas. El script 06 sigue siendo necesario para actualizar los procedimientos de tareas de una base anterior.

## Formulario

Los controles de acceso, registro y verificación se declaran en Form1.Designer.cs. Form1.cs conserva los eventos y las llamadas a negocio. El constructor no consulta SQL ni envía correos, de modo que el diseñador puede crear el formulario sin servicios externos.

## Correo

El script 07 agrega CodigosVerificacion y tres procedimientos. Se conservan las tablas y procedimientos anteriores. El código aleatorio tiene seis dígitos, vence en 10 minutos, permite cinco intentos y se consume al verificar. SQL guarda su resumen con sal, no el código original. El reenvío requiere un minuto y reemplaza el código previo.

SMTP se configura según el README. El repositorio no contiene host, remitente, usuario ni contraseña reales. Un fallo de envío deja la cuenta pendiente y permite volver a solicitar el código. Las contraseñas SMTP se leen exclusivamente de la variable PC_SMTPCLAVE.

## Comprobaciones de esta revisión

| Comprobación | Resultado |
| --- | --- |
| Compilar solución y pruebas con Mono en Linux | Correcto. El compilador usado fue XBuild/Mono, no MSBuild de Windows. |
| Pruebas normales | 20 correctas, incluyendo ambos resultados de login y un intercambio SMTP completo en localhost. |
| Prueba de interfaz con Mono y Xvfb | 26 correctas en total: las 20 anteriores y seis comprobaciones de navegación, contraseñas ocultas y límites de controles. |
| Capturas de las tres pestañas | Revisadas visualmente; controles y textos visibles, sin superposición. |
| Pruebas de códigos en SQL y login con el procedimiento antiguo | Agregadas al modo --integracion; pendientes de ejecutar con la nueva migración. |
| Windows, LocalDB y diseñador de Visual Studio | Pendientes para esta revisión. Los resultados de Actions de la versión base no validan estos cambios nuevos. |
| Entrega a un buzón real | Pendiente de configurar y probar el servidor SMTP del grupo. Las pruebas realizadas solo usan localhost. |

La base existente de un integrante no se modificó. La migración 07 y los cambios locales todavía requieren esa comprobación de integración antes de publicarse.
