# Entregable 3 — Avance funcional

## Auditoría inicial

Fecha de revisión: 26 de octubre de 2026. Base: `main`, commit `6154e9751a534797313a17e6d71d38260cae63ad`.

Se revisaron los archivos C#, proyectos, solución, configuración, scripts SQL 01–05, instalador, README, commits recientes e issues 1–19. Se contrastaron los flujos con RF-001 a RF-007 y la seguridad de contraseñas del Documento de Requerimientos. El tablero Project no pudo recuperarse; no se atribuyen estados de columnas que no se pudieron consultar.

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

1. Registro desde negocio con contraseña en texto únicamente como entrada; sal aleatoria y hash PBKDF2-SHA256. Login compara el hash recuperado desde SQL, rechaza credenciales incorrectas y descarta el hash antes de entregar el usuario a presentación.
2. Formularios de acceso/registro, listado por usuario, crear/editar/eliminar y cierre de sesión, con manejo de errores visible y registro de excepciones en depuración.
3. Contratos C#/SQL coordinados para propietario, estados y errores explícitos. Se añadió migración 06 para bases existentes; se conservaron las tablas y los procedimientos restantes.
4. Referencia directa de presentación a entidades y System.Configuration; registro de nuevos archivos de compilación; solución raíz completa.
5. Pruebas ejecutables sin dependencias externas, instancia LocalDB exclusiva y verificación automatizada en Windows.
6. README con instalación, configuración, demostración y alcance real.

No se sustituyó la arquitectura ni se eliminaron archivos del grupo. No se incorporaron estadísticas, calendarios, sincronización, gamificación ni bloqueo de aplicaciones.

## Evidencia de verificación

| Comprobación | Resultado |
| --- | --- |
| Árbol inicial y commits de main | Revisados; repositorio local inicialmente limpio. |
| Referencias y archivos de los cinco proyectos | Correctos: todos los archivos declarados y proyectos referenciados existen. |
| XML de configuración y solución raíz | Correcto; conexión única en la configuración de la aplicación. |
| Contratos de parámetros de datos con SQL | 35 llamadas contrastadas con sus procedimientos. |
| Migración 06 frente a instalación nueva 04 | Los cuatro cuerpos coinciden, cambiando CREATE por ALTER. |
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
| Publicar rama por Git | Falló: no hay credenciales locales HTTPS para escritura. |
| Crear rama mediante la conexión GitHub | Rechazado con HTTP 403: Resource not accessible by integration. |
| Verificación automatizada de Windows | Preparada; no iniciada al no poder publicar la rama. |

Una revisión estática no equivale a compilación ni a persistencia operativa. No se declaran aprobadas las pruebas dinámicas pendientes.

## Comprobación final de los requisitos

- [x] Repositorio oficial disponible e historial conservado; commits nuevos creados localmente.
- [x] Proyecto organizado por las cuatro capas existentes.
- [ ] Módulo 1 funcional comprobado: implementado, pendiente de ejecución real.
- [ ] Módulo 2 funcional comprobado: implementado, pendiente de ejecución real.
- [ ] Base de datos conectada y persistencia comprobada: contratos integrados, pendiente de ejecución real.
- [x] README con instalación y ejecución.
- [ ] Proyecto compila: falta ejecutar el compilador Windows.
- [x] No se introdujeron secretos; las conexiones usan autenticación integrada.
- [x] No se eliminó trabajo previo ni se reescribió historial.
- [x] Cambios limitados a los dos módulos, integración, pruebas y documentación.
- [ ] Rama nueva publicada: bloqueada por permisos de escritura.

La entrega queda implementada y preparada para verificación, pero no certificada como funcional mientras no se completen las casillas pendientes.

## Importar la rama local y publicar

Se proporciona un archivo `ProgramaConcentracion_Entregable3.bundle` con el historial y la rama de entrega. Sobre un clon del repositorio oficial, primero comprobar que no hay cambios sin guardar. No descartar ni sobrescribir trabajo local.

```powershell
git status --short
git fetch 'C:\ruta\ProgramaConcentracion_Entregable3.bundle' entregable-3-avance-funcional:entregable-3-avance-funcional
git switch entregable-3-avance-funcional
git log -5 --oneline
```

Si ya existe una rama con ese nombre, detenerse y comparar ambas antes de actualizarla; no usar force. Compilar y ejecutar las pruebas del README, corregir cualquier fallo y luego, con una cuenta autorizada:

```powershell
git push -u origin entregable-3-avance-funcional
```

La ejecución de Windows en Actions debe terminar correctamente antes de declarar comprobadas compilación e integración. No se necesita modificar ni fusionar main para revisar esta entrega.

## Demostración presencial

1. Preparar SQLEXPRESS con 01–05 o actualizar la base existente con 06. Ajustar App.config.
2. Abrir la solución `.sln`, recompilar e iniciar CapaPresentacion.
3. Registrar una cuenta; comprobar en SSMS que su contraseña no se guarda en claro.
4. Verificar administrativamente el correo usando el bloque del README. Mostrar que una cuenta pendiente no accede.
5. Probar clave incorrecta, luego correcta y acceso al listado.
6. Crear una tarea con fin posterior al inicio; buscarla, editarla y volver a consultar.
7. Cerrar sesión y entrar de nuevo para comprobar persistencia.
8. Eliminar con confirmación; volver a entrar y comprobar que ya no está.
9. Verificar un segundo usuario y comprobar que no lista tareas del primero.
10. Mostrar el resultado de las pruebas automáticas y la estructura por capas.

No mostrar el bloqueo efectivo del sistema operativo como funcional: queda fuera de este sprint. La verificación de correo es manual y el límite de intentos es local al proceso. Las preferencias completas y la diferencia previa entre Descansos 0..1 en SQL y 0..* en el modelo documental siguen fuera del alcance de este cambio.
