# Plan de pruebas

## Alcance

Validar las reglas de negocio de acceso, contraseña y verificación de correo; la persistencia y aislamiento de tareas en SQL Server; y la navegación básica de los formularios. Las pruebas SQL usan exclusivamente la instancia `(localdb)\Entregable3Pruebas`. No se envían correos reales: se usa un servidor SMTP TCP local de prueba.

## Preparación y ejecución

Requisitos: Windows, Visual Studio 2022 con .NET Framework 4.8 Developer Pack y SQL Server Express LocalDB para integración. Desde la raíz, en Developer PowerShell:

```powershell
msbuild tests/Pruebas.csproj /t:Rebuild /p:Configuration=Release
New-Item -ItemType Directory -Force tests/evidencias | Out-Null
& tests/bin/Release/Pruebas.exe 2>&1 | Tee-Object tests/evidencias/unitarias.log
& tests/bin/Release/Pruebas.exe --interfaz 2>&1 | Tee-Object tests/evidencias/interfaz.log
```

Para integración, preparar una instancia de pruebas vacía una sola vez con `tests/PrepararPruebas.ps1`, luego:

```powershell
& tests/bin/Release/Pruebas.exe --integracion 2>&1 | Tee-Object tests/evidencias/integracion.log
```

La prueba de interfaz no guarda capturas de las pantallas. Las imágenes de evidencia de esta entrega presentan los resultados reales de los logs; los archivos de texto completos están en `tests/evidencias/`. No ejecutar el preparador si ya existe la instancia `Entregable3Pruebas`: se detiene deliberadamente para preservar sus datos.

## Casos unitarios y de componente

Los casos se ejecutan con `Pruebas.exe`; un caso aprobado imprime `CORRECTO:` y uno fallido termina con código distinto de cero.

| ID | Caso y resultado esperado | Implementación |
| --- | --- | --- |
| U01 | El hash PBKDF2 no revela la contraseña y respeta el límite de almacenamiento. | `tests/Program.cs` |
| U02 | La misma contraseña produce hashes distintos por su sal aleatoria. | `tests/Program.cs` |
| U03 | La contraseña correcta verifica y una incorrecta no. | `tests/Program.cs` |
| U04 | Un hash con formato inválido se rechaza sin excepción no controlada. | `tests/Program.cs` |
| U05 | Contraseñas cortas, nombre vacío y correo con nombre de presentación se rechazan. | `tests/Program.cs` |
| U06 | Fechas de tarea inválidas y un ID de usuario inválido se rechazan. | `tests/Program.cs` |
| U07 | El lector de sesión admite el resultado SQL anterior sin `Nombre` y conserva `Nombre` cuando existe. | `tests/PruebasAcceso.cs` |
| U08 | Los códigos son numéricos de seis dígitos, varían y se persiste su hash con sal. | `tests/PruebasAcceso.cs` |
| U09 | Código corto/no numérico, SMTP incompleto y SMTP remoto sin TLS se rechazan. | `tests/PruebasAcceso.cs` |
| U10 | El servidor SMTP local recibe el código y su vigencia; un puerto cerrado expone el fallo. | `tests/PruebasAcceso.cs` |
| U11 | La configuración de demostración habilita el flujo previsto sin marcar como verificada la cuenta. | `tests/Program.cs` |

## Pruebas de integración

| ID | Proceso completo | Validaciones |
| --- | --- | --- |
| I01 | Registrar cuenta y verificar por código recibido en SMTP local. | Hash almacenado; correo duplicado rechazado; cuenta pendiente no inicia sesión; expiración, cinco intentos, éxito y uso único. |
| I02 | Iniciar sesión en una cuenta verificada. | Credenciales válidas; incorrectas y cuenta eliminada rechazadas; el objeto de sesión no expone el hash. |
| I03 | Compatibilidad con procedimiento anterior de login. | Se simula resultado sin `Nombre`, el negocio recupera el nombre y restaura la definición del procedimiento al terminar. |
| I04 | Crear y consultar tarea. | Persistencia, estado inicial y aislamiento por usuario. |
| I05 | Intentar editar y eliminar tarea de otro usuario. | SQL rechaza ambos accesos cruzados. |
| I06 | Editar, cambiar estado y eliminar tarea propia. | Cambios recuperables; estado activo impide edición/eliminación; completada admite edición; borrado persiste. |
| I07 | Limpiar los datos temporales del escenario. | Se borran solo cuentas de correo aleatorio creadas por esa ejecución. |
| I08 | Probar el modo demostración con una cuenta pendiente. | Credenciales correctas admitidas solo en demostración; contraseña incorrecta rechazada y cuenta aún pendiente; modo normal sigue rechazándola. |
| I09 | Probar compatibilidad con procedimientos originales de tareas. | Propiedad de usuario y estado activo protegidos antes de la migración; operaciones propias compatibles. |

## Prueba de interfaz y evidencias

`Pruebas.exe --interfaz` recorre las tres pestañas y verifica campos de contraseña ocultos, navegación a verificación y límites de controles. El resultado queda en el log; esta prueba no sustituye una revisión visual manual en Windows ni integra la interfaz con SQL.

## Resultados

| Ejecución | Resultado |
| --- | --- |
| Compilación Release de `tests/Pruebas.csproj` | Aprobada con MSBuild de Visual Studio 2022 y .NET Framework 4.8. Evidencia: `tests/evidencias/compilacion.log`. |
| Unitarias/componentes (`Pruebas.exe`) | Aprobadas: **22 comprobaciones**. Evidencia: `tests/evidencias/unitarias.log`. |
| Integración LocalDB (`--integracion`) | Aprobada: **62 comprobaciones** sobre `(localdb)\Entregable3Pruebas`. Evidencia: `tests/evidencias/integracion.log`. |
| Interfaz (`--interfaz`) | Aprobada: **28 comprobaciones** (22 unitarias/componentes + 6 de interfaz). Evidencia: `tests/evidencias/interfaz.log` y `tests/evidencias/evidencia-interfaz.png`. |

La primera integración detectó que la instancia existente no tenía `sp_PrepararVerificacionCorreo`. Se aplicó el script aditivo `database/07_VerificacionCorreo.sql` a la instancia de pruebas y se repitió el flujo completo con éxito. No se ejecutó el instalador `PrepararPruebas.ps1`, ya que detiene la ejecución si la instancia ya existe.

Actualizar esta tabla y conservar los logs e imágenes solo después de ejecutar el comando correspondiente. Un caso aprobado requiere una línea `CORRECTO:`; el total aparece al final como `Total correcto: N`.
