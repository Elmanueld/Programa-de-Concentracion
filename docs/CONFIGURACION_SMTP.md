# Configuración SMTP del Backend

El correo de verificación y recuperación se envía desde `Backend` mediante `SmtpEmailSender`. Las opciones se leen con la configuración estándar de ASP.NET Core. En MonsterASP configura las variables en **Websites → sitio → Scripting → Environment variables** y reinicia el sitio.

| Variable de entorno | Uso |
|---|---|
| `Smtp__Host` | Servidor SMTP. Namecheap Private Email: `mail.privateemail.com`. |
| `Smtp__Port` | Puerto. `465` para TLS implícito; `587` para STARTTLS si el Backend se configura para ello. |
| `Smtp__FromEmail` | Dirección remitente autorizada. |
| `Smtp__FromName` | Nombre visible del remitente (opcional, configurado por defecto). |
| `Smtp__Username` | Nombre de usuario del buzón. |
| `Smtp__Password` | Contraseña del buzón. |

Los nombres usan `__` para representar la separación de secciones de `IConfiguration`; las variables de entorno tienen prioridad sobre `appsettings.json`. El archivo versionado no contiene credenciales reales. No guardes la contraseña SMTP en el cliente WinForms, en el repositorio ni en artefactos de publicación.

Para Private Email habilita el acceso de aplicaciones de terceros en la cuenta del buzón. Usa el puerto 465 con SSL/TLS implícito, que es la configuración actual del Backend. No cambies el puerto a 587 sin comprobar que el código/configuración SMTP usa STARTTLS.

Las pruebas automatizadas usan un remitente falso y nunca contactan un servidor SMTP. Para probar el envío real utiliza un buzón de prueba y una cuenta no crítica; no ejecutes una prueba de correo real desde CI.

La API registra fallos SMTP en los logs del Backend sin devolver trazas ni secretos al cliente. En MonsterASP consulta **Websites → sitio → Logs → ASP.NET Core debug**; activa temporalmente `Status: Enabled`, reproduce el problema y pulsa **Reload log**. Descarga el log si hace falta y después desactiva stdout para evitar consumo innecesario de disco. Los logs no deben contener la contraseña ni códigos de verificación.
