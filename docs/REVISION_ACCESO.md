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
| Integración SQL Server 2022 en Linux | 50 comprobaciones correctas en total (20 normales y 30 SQL), salida 0. Incluye códigos, ambos procedimientos de login, CRUD, estados, cuenta eliminada y aislamiento de usuarios. |
| Windows, LocalDB y diseñador de Visual Studio | Pendientes para esta revisión. Los resultados de Actions de la versión base no validan estos cambios nuevos. |
| Entrega a un buzón real | Pendiente de configurar y probar el servidor SMTP del grupo. Las pruebas realizadas solo usan localhost. |

La prueba se ejecutó en SQL Server 2022 Developer, en un contenedor aislado. Se instalaron los scripts 01–05 y 07 en ProgramaConcentracionPruebas. Se usó una copia temporal del ejecutable de pruebas con una guardia exclusiva para 127.0.0.1:14333 y esa base. No se modificó la guardia LocalDB del repositorio ni el código de la aplicación para conectarla. Las contraseñas de esa instancia no se incorporaron al repositorio.

La primera ejecución comenzó antes de terminar el arranque del motor. Al reintentar se encontró un error real del test del procedimiento anterior: enviaba parámetros innecesarios al ejecutar ALTER y no contemplaba los comentarios iniciales al restaurar la definición. Se separó esa ejecución sin parámetros y se corrigió la búsqueda de CREATE al comienzo de cada línea. Después pasó toda la integración.

La base existente de un integrante no se modificó. Windows/LocalDB, el diseñador de Visual Studio y la entrega mediante SMTP real permanecen pendientes. No se publicó ningún cambio.
