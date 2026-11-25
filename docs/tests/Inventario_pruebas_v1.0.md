# Inventario de pruebas - Programa de Concentración 1.0

## Alcance y resultado de ejecución

El repositorio contiene un ejecutable de comprobaciones propio en `Backend.Tests/Program.cs`. No contiene proyectos con xUnit, NUnit, MSTest, pruebas de interfaz ni pruebas de integración contra una base de datos. El ejecutable usa repositorios y remitentes de correo falsos para probar los servicios del backend.

Última ejecución: `dotnet run --project .\Backend.Tests\Backend.Tests.csproj` en Windows con .NET SDK 9.0.320. Resultado: **38 comprobaciones correctas, 0 fallidas**. No se contactó Azure SQL ni SMTP.

## Verificación de correo - servicio backend

Tipo: comprobación unitaria dentro del ejecutable de pruebas. Componente: `Backend.Verification.VerificationService`.

- `normaliza el correo antes de preparar el código` (existente).
- `envía al mismo correo preparado en el repositorio` (existente).
- `genera y envía un código de seis dígitos` (existente).
- `persiste el resumen del código, no el código en claro` (existente).
- `confirma usando el mismo resumen derivado del código` (existente).
- `rechaza un código no numérico sin consultar el repositorio` (existente).
- `falla claramente si falta la clave de hash` (existente).
- `no informa éxito cuando falla la entrega SMTP` (existente).

Estas pruebas ayudan a detectar cambios de formato, exposición de códigos y falsas confirmaciones de envío.

## Registro de cuentas - servicio backend

Tipo: comprobación unitaria con repositorio falso. Componente: `AccountRegistrationService` y reglas de nombre de usuario.

- `recorta los datos y conserva el IdUsuario idempotente del repositorio` (existente; el nombre ahora se valida bajo las reglas nuevas).
- `transfiere únicamente el hash de contraseña recibido` (existente).
- `acepta el nombre de usuario válido 'Ana'` (nueva).
- `acepta el nombre de usuario válido 'Ana_2026'` (nueva).
- `acepta el nombre de usuario válido 'Élodie'` (nueva).
- `acepta el nombre de usuario válido '用户12'` (nueva).
- `rechaza un nombre vacío, con espacios/caracteres especiales o demasiado largo`: valor `Ana María` (nueva).
- Mismo test para `Ana!` (nueva).
- Mismo test para cadena vacía (nueva).
- Mismo test para solo espacios (nueva).
- Mismo test para 51 letras (nueva).
- `rechaza nombres no válidos antes de acceder al repositorio` (nueva).
- `no intenta registrar un nombre no válido en el repositorio` (nueva).
- `rechaza una contraseña que no llega en formato PBKDF2 esperado` (existente).
- `rechaza el hash inválido antes de acceder al repositorio` (existente).
- `mantiene el conflicto cuando no hay registro idempotente coincidente` (existente).

Detecta regresiones en validación, normalización, idempotencia y el límite entre servicio y persistencia.

## Recuperación de contraseña - servicio backend

Tipo: comprobación unitaria con repositorio y correo falsos. Componente: `PasswordRecoveryService`.

- `no revela ni envía código cuando la cuenta no existe o no puede recuperarse` (existente).
- `envía un código de recuperación al correo normalizado` (existente).
- `guarda el resumen del código de recuperación y no su texto` (existente).
- `actualiza la contraseña solo después de confirmar el código válido` (existente).
- `rechaza una contraseña que no está protegida en el formato esperado` (existente).

Detecta enumeración de cuentas, almacenamiento inseguro y cambios de contraseña sin confirmación.

## Contratos HTTP - validación de solicitudes

Tipo: inspección de metadatos de validación por reflexión. Componente: records de solicitudes API.

- `SendVerificationRequest.Email` conserva metadatos de validación (existente).
- `ConfirmVerificationRequest.Email` conserva metadatos de validación (existente).
- `ConfirmVerificationRequest.Code` conserva metadatos de validación (existente).
- `RegisterAccountRequest.Name` conserva metadatos de validación (existente).
- `RegisterAccountRequest.Email` conserva metadatos de validación (existente).
- `RegisterAccountRequest.PasswordHash` conserva metadatos de validación (existente).
- `SendPasswordRecoveryRequest.Email` conserva metadatos de validación (existente).
- `ConfirmPasswordRecoveryRequest.Code` conserva metadatos de validación (existente).
- `ConfirmPasswordRecoveryRequest.PasswordHash` conserva metadatos de validación (existente).

La nueva política de nombres se prueba en el servicio backend, que devuelve mensajes claros y no llama al repositorio con un nombre inválido.

## Pruebas de interfaz, bandeja y base de datos

No hay pruebas automatizadas de WinForms, de la bandeja del sistema, de timers ni de base de datos. La corrección se revisó en el flujo de `PrincipalForm`; la validación interactiva requiere ejecutar la aplicación en Windows con su configuración y base local/remota. La interfaz y el comportamiento de la bandeja no se declaran verificados en ejecución.

## Comprobaciones de compilación

- `dotnet restore .\ProgramaConcentracion.sln`: restauración completada.
- `dotnet build .\ProgramaConcentracion.sln --no-restore`: fallida en el proyecto clásico por tipos SQLite no resueltos; 0 advertencias, 5 errores.
- `MSBuild.exe .\ProgramaConcentracion.sln -restore -verbosity:minimal -p:OutputPath=bin\V1Review\`: solución completa compilada correctamente en una carpeta separada. Reportó dos advertencias existentes: un campo de formulario sin usar y una fuente tipográfica sin inicializar. Se usó una salida separada porque la instancia abierta tenía bloqueado el ejecutable de Debug.
- `dotnet run --project .\Backend.Tests\Backend.Tests.csproj`: 38 correctas, 0 fallidas.
- No hay configuración de lint o analizador estático identificada en la solución.
