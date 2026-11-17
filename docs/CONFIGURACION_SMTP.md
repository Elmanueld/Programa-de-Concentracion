# Configuración del correo (SMTP)

El programa envía el código de verificación con un servidor SMTP. Los datos del servidor se leen de variables de entorno (o de `App.config`), y **la contraseña solo se lee de la variable `PC_SMTPCLAVE`**. Nunca se escribe en el código ni en el repositorio.

## Variables

| Variable | Ejemplo | Para qué sirve |
|---|---|---|
| `PC_SMTPHOST` | `mail.ejemplo.com` | Servidor SMTP |
| `PC_SMTPPUERTO` | `587` | Puerto (STARTTLS) |
| `PC_SMTPSSL` | `true` | Activa el cifrado |
| `PC_SMTPUSUARIO` | `correo@ejemplo.com` | Usuario SMTP |
| `PC_SMTPCLAVE` | *(contraseña de aplicación)* | Solo en variable de entorno |
| `PC_SMTPREMITENTE` | `correo@ejemplo.com` | Dirección que envía el correo |
| `PC_SMTPNOMBRE` | `Programa de Concentración` | Nombre del remitente (opcional) |
| `PC_MODO_DEMOSTRACION` | `false` | Con `true` no se envía correo ni se verifica (por defecto es `false`) |

## Ejemplo en PowerShell (solo para la sesión actual)

```powershell
$env:PC_SMTPHOST = "mail.ejemplo.com"
$env:PC_SMTPPUERTO = "587"
$env:PC_SMTPSSL = "true"
$env:PC_SMTPUSUARIO = "correo@ejemplo.com"
$env:PC_SMTPREMITENTE = "correo@ejemplo.com"
$env:PC_SMTPCLAVE = Read-Host "Contraseña SMTP"
$env:PC_MODO_DEMOSTRACION = "false"
```

Si el SMTP no está configurado, la aplicación sigue funcionando, avisa que el envío no está disponible y no verifica la cuenta automáticamente.

## Prueba con el SMTP real en GitHub Actions

En `Actions > Validar proyecto > Run workflow` se ejecuta el trabajo `correo-real`. Necesita dos secretos del repositorio: `PC_SMTPUSUARIO` (la cuenta que envía y recibe la prueba) y `PC_SMTPCLAVE` (la contraseña de aplicación). Envía un correo real y comprueba que una contraseña incorrecta sea rechazada. La recepción se confirma revisando el buzón.

## Importante

- Es una aplicación de escritorio: no distribuyas la contraseña SMTP junto con el ejecutable.
- Para GitHub Actions, la contraseña debe guardarse como secreto del repositorio (`Settings > Secrets and variables > Actions`).
