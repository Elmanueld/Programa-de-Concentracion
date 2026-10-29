> Estado de referencia: la versión `f39cd1d` fue publicada y validada en Windows mediante [Actions 40446666067](https://github.com/Elmanueld/Programa-de-Concentracion/actions/runs/40446666067). El informe de revisión inicial se conserva debajo. Las correcciones locales de acceso, diseñador y correo necesitan su propia validación antes de publicarse.

# Entregable 3 — Avance funcional

## Revisión inicial

Fecha de revisión: 26 de octubre de 2026. Base: `main`, commit `6154e9751a534797313a17e6d71d38260cae63ad`.

Se revisaron el código C#, los proyectos, la solución, la configuración, los scripts SQL 01–05, el instalador, el README, los commits recientes y los issues 1–19. También se consultaron los requerimientos RF-001 a RF-007 y la seguridad de contraseñas del Documento de Requerimientos. No se pudo abrir el tablero Project, por lo que falta revisar sus columnas.

Hallazgos:

- Las cuatro capas ya existían y la solución `src/CapaDatos/CapaDatos.sln` las incluía. La solución raíz `.slnx` estaba vacía.
- Presentación solo contenía un formulario vacío; no había un flujo demostrable.
- Datos ya usaba parámetros y procedimientos almacenados con una conexión central en configuración.
- Negocio validaba datos básicos, pero no calculaba ni verificaba contraseñas ni ofrecía inicio de sesión.
- SQL exigía `CorreoVerificado = 1`; el registro dejaba `0` y no había envío de correo. Se conserva la regla y se documenta verificación administrativa para la demostración.
- Edición y eliminación recibían solo el ID de tarea; no verificaban propietario y podían terminar sin cambios ni error.
- La edición SQL solo permitía `P`, aunque RF-006 permite editar mientras no esté en progreso. Se ajustó a estados distintos de `A`.
- App.config incluía una sección sin declaración. Se retiró para permitir cargar las conexiones.
- Los issues de procedimientos de usuarios/tareas estaban cerrados, mientras Registro (#19) seguía abierto. No se cerraron issues ni se modificó el tablero.

## Cambios realizados

1. Se agregaron el registro y el inicio de sesión en negocio. La contraseña se guarda como hash PBKDF2-SHA256 con una sal aleatoria. Al entrar se compara con el valor guardado en SQL y se quita el hash del usuario que recibe la pantalla principal.
2. Se agregaron formularios para registro, acceso, listado por usuario, creación, edición, eliminación y cierre de sesión. Se muestran mensajes si una operación falla y se guarda el detalle del error en la salida de depuración.
3. Se ajustaron juntos los parámetros de C# y SQL para comprobar el usuario y el estado de la tarea. El script 06 actualiza los procedimientos de una base existente sin borrar sus datos.
4. Se agregaron las referencias de presentación a entidades y System.Configuration. Se incluyeron los archivos nuevos en sus proyectos y los proyectos en la solución raíz.
5. Pruebas ejecutables sin paquetes adicionales, una instancia LocalDB separada y pruebas automáticas en Windows.
6. Se actualizó el README con instalación, configuración, pasos para la demostración y funciones pendientes.

No se sustituyó la arquitectura ni se eliminaron archivos del grupo. No se incorporaron estadísticas, calendarios, sincronización, gamificación ni bloqueo de aplicaciones.

## Revisiones y pruebas

| Comprobación | Resultado |
| --- | --- |
| Árbol inicial y commits de main | Revisados; repositorio local inicialmente limpio. |
| Referencias y archivos de los cinco proyectos | Correctos: todos los archivos declarados y proyectos referenciados existen. |
| XML de configuración y solución raíz | Correcto; conexión única en la configuración de la aplicación. |
| Parámetros enviados desde datos a SQL | 35 llamadas comparadas con sus procedimientos. |
| Script 06 frente al script 04 | Los cuatro procedimientos coinciden, cambiando CREATE por ALTER. |
| Formularios sin consultas/conexiones SQL | Correcto por inspección y búsqueda. |
| Bloques catch vacíos | Ninguno en C#. |
| git diff --check | Correcto. |
| Compilar con msbuild | No ejecutable: comando ausente en el entorno Linux. |
| Restaurar con dotnet | No ejecutable: comando ausente. |
| Consultar SQL Server con sqlcmd | No ejecutable: comando ausente; no hay instancia SQL Server local. |
| Instalar herramientas locales | Bloqueado por permisos del entorno. |
| Pruebas de contraseñas/validaciones | Preparadas; pendientes de ejecución con .NET Framework 4.8. |
| Registro/login/CRUD contra SQL real | Preparados; pendientes de ejecución en Windows/LocalDB. |
| Prueba visual y cierre de sesión WinForms | Pendiente en Windows. |
| Intento anterior de publicar la rama por Git | Falló: no hay credenciales locales HTTPS para escritura. |
| Intento anterior de crear la rama con la conexión GitHub | Rechazado con HTTP 403: Resource not accessible by integration. |
| Pruebas automáticas de Windows | Preparadas; no se han ejecutado porque la rama no está publicada. |

Se revisaron los archivos, pero todavía falta compilar y ejecutar las pruebas para comprobar que el programa guarda y recupera datos correctamente.

## Comprobación final de los requisitos

- [x] Repositorio oficial disponible e historial conservado; commits nuevos creados localmente.
- [x] Proyecto organizado por las cuatro capas existentes.
- [ ] Módulo 1 funcional comprobado: implementado, pendiente de ejecución real.
- [ ] Módulo 2 funcional comprobado: implementado, pendiente de ejecución real.
- [ ] Base de datos conectada y guardado comprobado: parámetros revisados, pendiente de ejecución real.
- [x] README con instalación y ejecución.
- [ ] Proyecto compila: falta ejecutar el compilador Windows.
- [x] No se introdujeron secretos; las conexiones usan autenticación integrada.
- [x] Se conservaron los archivos y los commits anteriores del grupo. Solo se reescribieron los cinco commits locales del Entregable 3 para ajustar sus mensajes e incluir esta revisión de estilo.
- [x] Cambios limitados a los dos módulos, integración, pruebas y documentación.
- [ ] Rama nueva publicada: bloqueada por permisos de escritura.

El código está preparado para probarlo. Las casillas pendientes se deben completar en Windows antes de entregar.

## Última revisión de estilo

Se usaron los commits anteriores del grupo como referencia. Se simplificaron los mensajes de los cinco commits locales, se separaron instrucciones que estaban en una sola línea y se cambió una expresión condicional por if y else. También se revisaron los comentarios nuevos y la redacción de estos documentos.

No se cambiaron las funciones, la seguridad de contraseñas ni los procedimientos SQL. En el script 06 solo se simplificaron dos comentarios. Se conservaron los autores y las fechas de autor; Git registra la hora real de esta revisión como fecha de confirmación de los commits reescritos. Esta versión no se publicó en GitHub.

## Importar la rama local y publicar

Se proporciona un archivo `ProgramaConcentracion_Entregable3.bundle` con el historial y la rama de entrega. Sobre un clon del repositorio oficial, primero comprobar que no hay cambios sin guardar. No descartar ni sobrescribir trabajo local.

```powershell
git status --short
git fetch 'C:\ruta\ProgramaConcentracion_Entregable3.bundle' entregable-3-avance-funcional:entregable-3-avance-funcional
git switch entregable-3-avance-funcional
git log -5 --oneline
```

Si ya existe una rama con ese nombre, detenerse y comparar ambas antes de actualizarla; no usar force. La publicación sigue pendiente. Después de compilar, pasar las pruebas y autorizar la publicación, usar una cuenta con permisos para ejecutar:

```powershell
git push -u origin entregable-3-avance-funcional
```

La ejecución de Windows en Actions debe terminar correctamente antes de declarar comprobadas compilación e integración. No se necesita modificar ni fusionar main para revisar esta entrega.

## Demostración presencial

1. Preparar SQLEXPRESS con 01–05 y 07 o actualizar la base existente con 06 y 07. Ajustar App.config y SMTP según el README.
2. Abrir la solución `.sln`, recompilar e iniciar CapaPresentacion.
3. Registrar una cuenta; comprobar en SSMS que su contraseña no se guarda en claro.
4. Mostrar que la cuenta pendiente no accede. Recibir el código por correo e introducirlo en la pestaña Verificar correo.
5. Probar clave incorrecta, luego correcta y acceso al listado.
6. Crear una tarea con fin posterior al inicio; buscarla, editarla y volver a consultar.
7. Cerrar sesión y entrar de nuevo para comprobar persistencia.
8. Eliminar con confirmación; volver a entrar y comprobar que ya no está.
9. Verificar un segundo usuario y comprobar que no lista tareas del primero.
10. Mostrar el resultado de las pruebas automáticas y la estructura por capas.

No mostrar el bloqueo efectivo del sistema operativo como funcional: queda fuera de este sprint. La verificación por código guarda en SQL el resumen, la vigencia y el número de intentos; el bloqueo de intentos de login sigue siendo local al proceso. Las preferencias completas y la diferencia previa entre Descansos 0..1 en SQL y 0..* en el modelo documental siguen fuera del alcance de este cambio.
