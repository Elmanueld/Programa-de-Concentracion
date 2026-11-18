# Backend de verificación de correo

API REST en ASP.NET Core Web API / .NET 9. Gestiona el alta remota desde WinForms, la verificación del correo y la recuperación de contraseña; no implementa inicio de sesión.

## Acceso a SQL

La API llama procedimientos SQL ya instalados. Verificación usa `sp_PrepararVerificacionCorreo`, `sp_ConsultarSalVerificacion` y `sp_ConfirmarVerificacionCorreo`; recuperación usa `sp_PrepararRecuperacionContrasena`, `sp_ConsultarSalRecuperacionContrasena` y `sp_ConfirmarRecuperacionContrasena`. El contrato se define en `../../../database/04_CrearProcedimientos.sql` y la actualización para una base existente está en `../../../database/10_MigracionRecuperacionContrasena.sql`. La recuperación requiere `Usuarios(IdUsuario, Correo, CorreoVerificado, ContrasenaHash, Eliminado)` y `CodigosRecuperacionContrasena(IdUsuario, Correo, Sal, CodigoHash, ExpiraUtc, UltimoEnvioUtc, Intentos)`. La API no crea ni migra tablas al iniciar.

El procedimiento existente aplica expiración de 10 minutos, un intervalo de 60 segundos entre envíos y un máximo de 5 intentos. La API almacena un HMAC-SHA256 del código y el valor de sal usando `Verification__HashKey`; el código claro solo vive durante el envío. La clave HMAC debe ser un secreto estable del servicio para poder confirmar los códigos. Para producción genera una clave aleatoria larga y guárdala en Secret Manager; no la cambies mientras haya códigos pendientes.

La respuesta del endpoint de envío es igual tanto si el correo corresponde a una cuenta elegible como si no, para evitar enumerar usuarios. La confirmación solo devuelve éxito o un error genérico y no expone datos de cuenta.

## Recuperación de contraseña

WinForms llama `POST /api/password-recovery/send` para pedir un código y `POST /api/password-recovery/confirm` para validarlo y guardar la nueva contraseña. El correo de envío siempre devuelve una respuesta genérica, exista o no una cuenta elegible. Solo las cuentas activas con correo verificado reciben el mensaje SMTP. El código vence en 10 minutos, solo puede usarse una vez, tiene un máximo de cinco intentos y se limita a un envío por minuto. SQL almacena únicamente la sal y el HMAC del código. WinForms deriva el hash PBKDF2 de la nueva contraseña y lo envía por HTTPS; la contraseña en claro no sale del cliente. Ambos correos usan la misma configuración SMTP de `SmtpEmailSender`.

Una base existente necesita la migración `../../../database/10_MigracionRecuperacionContrasena.sql`, ejecutada con la base de datos correcta seleccionada. La migración no crea otra base ni cambia `Usuarios`; agrega la tabla de códigos y los tres procedimientos de recuperación.

## Registro remoto desde WinForms

WinForms guarda primero la cuenta pendiente en su SQLite local. Luego llama `POST /api/accounts/register` por HTTPS para crear u obtener idempotentemente la cuenta remota y recibir `idUsuario`. El cuerpo contiene `name`, `email` y `passwordHash`; el cliente envía el hash PBKDF2-SHA256 que ya genera la aplicación, nunca la contraseña en claro. El backend valida el formato y llama `dbo.sp_RegistrarUsuario`; no consulta directamente la tabla `Usuarios`.

Para reintentos seguros, `sp_RegistrarUsuario` devuelve el mismo `IdUsuario` solo cuando el correo ya existe activo con exactamente el mismo hash; un correo existente con otra contraseña o eliminado produce conflicto, sin insertar duplicados. En una base ya instalada, ejecuta `../../../database/09_MigracionRegistroApiIdempotente.sql` con la base de datos correcta seleccionada antes de usar el nuevo flujo. En una instalación nueva, el procedimiento actualizado viene en `../../../database/04_CrearProcedimientos.sql`. Tras obtener el ID remoto, WinForms solicita el código a `POST /api/email-verification/send`; la confirmación continúa por `POST /api/email-verification/confirm`.

Si la red o el servicio remoto falla, SQLite conserva el registro y el hash de contraseña protegido por DPAPI para el usuario de Windows actual. WinForms reintenta mientras la pantalla de verificación permanezca abierta; después de reiniciar la aplicación, se puede reanudar con el mismo correo y la acción de volver a enviar el código. El registro local pendiente se elimina cuando la API acepta el envío. Configura la base URL del cliente con `ApiBaseUrl` en `CapaPresentacion/App.config` o con la variable de entorno `PC_API_BASE_URL`; debe ser una URL HTTPS.

## Configuración local

Requiere .NET 9 SDK. Define las variables de entorno en la sesión local antes de iniciar el proyecto; no guardes credenciales en este repositorio:

```powershell
$env:ConnectionStrings__DefaultConnection = '<cadena de conexión de Azure SQL>'
$env:Smtp__Host = 'mail.privateemail.com'
$env:Smtp__Port = '465'
$env:Smtp__FromEmail = '<buzón remitente>'
$env:Smtp__Username = '<usuario SMTP>'
$env:Smtp__Password = '<contraseña SMTP>'
$env:Verification__HashKey = '<secreto aleatorio persistente de al menos 32 bytes>'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project .\Backend.csproj
```

Swagger UI: `https://localhost:<puerto>/swagger` (si se usa el perfil HTTPS del entorno) o `http://localhost:<puerto>/swagger`. Liveness: `/health/live`.

Solicitar código:

```http
POST /api/email-verification/send
Content-Type: application/json

{"email":"persona@dominio.com"}
```

Confirmar:

```http
POST /api/email-verification/confirm
Content-Type: application/json

{"email":"persona@dominio.com","code":"123456"}
```

El código solo se entrega por correo; el endpoint nunca lo devuelve.

## Límites

Hay límites por IP en memoria por instancia (5 solicitudes de envío y 20 confirmaciones cada 10 minutos), además del intervalo y los intentos que aplica SQL por cuenta. El limitador en memoria no coordina varias instancias; antes de una exposición pública con tráfico importante, se debe añadir limitación compartida en la capa de entrada. No se devuelve información sobre si una cuenta existe.

## Docker

Desde esta carpeta:

```powershell
docker build -t programa-concentracion-api .
docker run --rm -p 8080:8080 -e PORT=8080 `
  -e ConnectionStrings__DefaultConnection='<cadena>' `
  -e Smtp__Username='<usuario>' -e Smtp__Password='<contraseña>' `
  -e Smtp__FromEmail='<buzón remitente>' `
  -e Verification__HashKey='<secreto estable>' `
  programa-concentracion-api
```

El contenedor escucha en `0.0.0.0:$PORT`, con `8080` como valor local predeterminado. En Cloud Run, HTTPS termina en la plataforma; configura los secretos mediante Secret Manager/variables de entorno del servicio. Swagger se desactiva automáticamente en `Production`.

## Publicar en MonsterASP.NET (IIS / Web Deploy)

MonsterASP.NET documenta soporte para .NET 9, ASP.NET Core Web API, IIS, Web Deploy y variables de entorno también en el plan Free. Ese plan está destinado a hobby, desarrollo y pruebas; ofrece 256 MB de RAM, 5 GB de disco y un subdominio gratuito. La API depende de Azure SQL externo, así que el límite de la base SQL incluida en MonsterASP no aplica. Valida el consumo de memoria y la disponibilidad con tráfico real antes de usarlo como servicio de producción.

1. En el panel de MonsterASP crea un sitio Free con la opción `My own application` y copia el nombre de servidor del sitio.
2. En Azure SQL agrega a la regla de firewall el IP de salida de MonsterASP. Se obtiene resolviendo con `nslookup` el hostname `siteXXXX.siteasp.net` que aparece en Overview. Ese IP es compartido y cambia si el sitio se mueve a otro servidor o región.
3. En MonsterASP abre el sitio → `Scripting` → `Environment variables`, guarda las variables listadas abajo y reinicia la aplicación. No subas el `.publishSettings` ni el perfil `.pubxml.user` al repositorio; contienen credenciales de despliegue.
4. En `Deploy` habilita WebDeploy, descarga el `.publishSettings` y guárdalo localmente.
5. En Visual Studio abre `ProgramaConcentracion.sln`, en el Explorador de soluciones haz clic derecho únicamente en el proyecto `Backend` → `Publish` → `Import profile`, selecciona el `.publishSettings` y publica en `Release`. No publiques el proyecto WinForms ni `CapaDatos`.
6. MonsterASP ejecuta la aplicación detrás de IIS y genera el `web.config` requerido durante la publicación del proyecto Web SDK. IIS administra el puerto; Docker conserva el puerto `PORT` en su comando para uso local/Cloud Run.
7. Para consultar errores de la API, abre **Websites → tu sitio → Logs → ASP.NET Core debug**, cambia **Status** a **Enabled**, y vuelve a reproducir la solicitud. Pulsa **Reload log** (o **Restart application & reload log** si está vacío); los mensajes `ILogger` de nivel Error y sus excepciones completas aparecerán al final. También puedes leer/descargar los archivos desde `/wwwroot/logs` por WebFTP/FTP. Cuando termines, vuelve a poner **Status** en **Disabled**: MonsterASP advierte que stdout consume disco y puede ralentizar el sitio. La pestaña **Event logs** conserva eventos del sitio por 7 días, pero los errores manejados por la API se consultan en **ASP.NET Core debug**.

Variables en el panel (usa los valores secretos de Azure SQL y del buzón, no los pongas aquí ni en el repositorio):

| Nombre | Valor |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Cadena de conexión de Azure SQL |
| `Authentication__TokenSigningKey` | Clave aleatoria de al menos 32 caracteres para firmar sesiones del cliente; manténgala privada |
| `Smtp__Host` | `mail.privateemail.com` |
| `Smtp__Port` | `465` |
| `Smtp__FromEmail` | Buzón remitente de Private Email |
| `Smtp__Username` | Usuario completo del buzón |
| `Smtp__Password` | Contraseña de aplicación del buzón |
| `Verification__HashKey` | Secreto aleatorio persistente de al menos 32 bytes |

Namecheap documenta el puerto 465 con SSL/TLS implícito y 587 con STARTTLS; el backend usa actualmente 465. Activa `Connect third-party apps` en la configuración de Private Email. MonsterASP indica que el Free plan permite SMTP externo y tiene abiertos los puertos salientes requeridos; el relay SMTP propio de MonsterASP requiere Premium. Swagger queda deshabilitado en Production, por lo que prueba los endpoints con un cliente HTTP externo. La URL HTTPS depende del subdominio/sitio creado en el panel.

Para verificar tras publicar, solicita `GET https://<sitio>/health/live` y espera `200` con `{"status":"ok"}`. Un `POST /api/email-verification/send` con `{}` debe responder `400 Bad Request` por validación y no intentar enviar correo. Después, desde una computadora fuera del hosting, llama `POST https://<sitio>/api/email-verification/send` con `{"email":"<correo existente sin verificar>"}` y confirma con `POST https://<sitio>/api/email-verification/confirm` con `{"email":"<correo>","code":"<código recibido>"}`. Revisa los logs de ASP.NET Core en el panel si el sitio devuelve 500/503. No uses una cuenta real para la primera prueba si no puedes recibir su mensaje.

## Alcance y dependencias

Antes de conectarlo a una base real, confirma que los procedimientos y columnas indicados están instalados en esa base de Azure. No se intentó conexión con Azure ni se ejecutó ningún script SQL. La API requiere SMTP TLS implícito de Private Email en el puerto 465. Las claves SMTP y HMAC no deben incluirse en el ejecutable WinForms, `appsettings.json` versionado, ni imágenes Docker.
