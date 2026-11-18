# Reporte de pruebas

## Resultado de la revisión

| Comprobación | Resultado |
|---|---|
| Recompilación Release de `src/CapaDatos/ProgramaConcentracion.sln` | Aprobada; 0 errores. |
| Build Release de la solución raíz `Programa-de-Concentracion.slnx` | Aprobada; 0 errores. |
| WinForms, suite general | Aprobada; 13 comprobaciones. |
| WinForms, suite con interfaz (`--interfaz`) | Aprobada; 25 comprobaciones. |
| Backend.Tests | Aprobada; 26 comprobaciones, 0 fallos. |
| SQL remoto y procedimientos en MonsterASP | Pendiente; no se conectó ni modificó la base remota. |
| SMTP real | Pendiente; las pruebas usan remitente falso y no enviaron correos. |
| CI en GitHub | Pendiente de ejecutar después de publicar la rama. |

La compilación tuvo 0 errores y dos advertencias C# preexistentes: `CS0649` en `Tipografia.FuenteBase.Fuente` y `CS0169` en `frmSeleccionarApp.flApp`. También apareció `NU1900` al no poder consultar `https://api.nuget.org/v3/index.json` para información de vulnerabilidades. No impidió compilar ni ejecutar las pruebas.

## Cobertura automatizada

`Backend.Tests` utiliza repositorios y remitente de correo falsos. Comprueba envío y confirmación de verificación, hash del código, configuración obligatoria, fallos SMTP, registro idempotente, validación del hash de contraseña, recuperación y metadatos de validación de los records.

La suite WinForms comprueba hashing de contraseñas, validaciones, compatibilidad con sesión, rechazo de entradas inválidas antes de llamar a la API, navegación y controles de interfaz. Las pruebas no contactan MonsterASP ni SQL/SMTP reales y no modifican datos de producción.

## Pruebas manuales pendientes

En un entorno de staging aislado y con cuentas desechables, validar:

1. Aplicar y verificar las migraciones frente al esquema instalado.
2. Alta remota y reintento idempotente con el mismo usuario.
3. Inicio de sesión, aislamiento de consultas y sincronización con dos usuarios.
4. Envío y confirmación de correo, recuperación de contraseña y expiración/límites.
5. Funcionamiento offline, reinicio con operaciones pendientes y resincronización.

No apuntar estas pruebas a la base de producción. La revisión no ejecutó scripts SQL, publicó el Backend ni envió correo real.
