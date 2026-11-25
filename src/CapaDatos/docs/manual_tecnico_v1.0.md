# Manual técnico - Programa de Concentración

Versión documental: 1.0 | Idioma: español

## Índice

1. Introducción
2. Requisitos e instalación
3. Configuración
4. Arquitectura
5. Ejecución y diagnóstico
6. Pruebas
7. Seguridad, datos y límites operativos
8. Actualizaciones de escritorio

## 1. Introducción

Programa de Concentración es una aplicación de escritorio Windows para planificar tareas y descansos, asociar aplicaciones y sitios web a tareas, y aplicar bloqueos durante los periodos configurados. La solución combina una interfaz WinForms, capas de negocio/datos, una copia local SQLite para trabajo sin conexión y una API ASP.NET Core para el alta, verificación y recuperación de cuentas.

Funciones principales: calendario y tareas; selección de aplicaciones y webs; descansos y avisos; reglas de bloqueo local; preferencias y apariencia; cuenta con sesión local sin conexión y sincronización posterior; alta/verificación y recuperación mediante API.

## 2. Requisitos e instalación

### Requisitos comprobados en el repositorio

- Windows para la aplicación WinForms.
- .NET Framework 4.8 para `CapaPresentacion`, `CapaNegocio`, `CapaDatos` y `CapaEntidades`.
- .NET SDK 9 para `Backend` y `Backend.Tests`.
- Visual Studio 2022 o herramientas de compilación compatibles con los proyectos clásicos de .NET Framework; la solución es `ProgramaConcentracion.sln`.
- SQL Server/Azure SQL con los procedimientos descritos en `Backend/README.md` para operaciones remotas. El backend no crea ni migra la base al iniciar.
- SMTP TLS para correo de verificación/recuperación.

### Preparar el código

1. Abrir `ProgramaConcentracion.sln` en Visual Studio con el targeting pack de .NET Framework 4.8.
2. Restaurar paquetes NuGet desde Visual Studio. Para los proyectos SDK de .NET también se puede usar `dotnet restore .\ProgramaConcentracion.sln`.
3. Restaurar y compilar en Debug con MSBuild de Visual Studio. Confirmar que `System.Data.SQLite.Core` y el ensamblado `System.Data.SQLite` se resuelven en el proyecto clásico `CapaDatos`.
4. Para crear el paquete de Windows, compilar `CapaPresentacion` en Release. El destino `CopySQLiteInterop` ejecuta `CapaPresentacion/Installer/CrearInstalador.ps1`, que empaqueta el ejecutable ya compilado.
5. Para instalar el paquete creado, descomprimir el ZIP completo y ejecutar `Instalar.vbs`. El instalador incluido puede descargar .NET Framework 4.8 desde Microsoft si falta y agrega el programa al menú Inicio.

En esta revisión, `dotnet build .\ProgramaConcentracion.sln --no-restore` no resolvió la referencia SQLite del proyecto clásico, mientras que MSBuild de Visual Studio con `-restore` compiló la solución completa en una carpeta de verificación separada. Use Visual Studio/MSBuild para compilar esta solución en Windows.

### Errores frecuentes

- `System.Data.SQLite` no encontrado: restaurar el paquete y confirmar el targeting pack/herramientas de MSBuild para proyecto clásico.
- API sin URL: configurar `ApiBaseUrl` en `CapaPresentacion/App.config` o `PC_API_BASE_URL` con una URL HTTPS.
- Correo no enviado: comprobar la configuración SMTP del backend y sus secretos; el cliente conserva el registro pendiente para reintentar.
- Conexión SQL: confirmar cadena, firewall y que los procedimientos requeridos ya están instalados.

## 3. Configuración

### Cliente WinForms

`CapaPresentacion/App.config` contiene ajustes de cliente no secretos. La URL de API se lee de `PC_API_BASE_URL` o `ApiBaseUrl`. El modo de demostración puede activarse con `PC_MODO_DEMOSTRACION` o la clave `ModoDemostracion`. SQLite crea su base local en el perfil del usuario; `PC_SQLITE_LOCAL_PATH` se usa como ruta de prueba.

El cliente nunca debe incluir contraseñas de SMTP ni la clave HMAC. La cuenta local pendiente protege el hash con DPAPI en el contexto del usuario de Windows.

### API ASP.NET Core

El proyecto usa .NET 9 con `MailKit 5.0.0`, `Microsoft.EntityFrameworkCore.SqlServer 9.0.23` y `Swashbuckle.AspNetCore 10.3.1`. La conexión y servicios se configuran mediante variables de entorno / configuración estándar de ASP.NET Core:

- `ConnectionStrings__DefaultConnection`
- `Smtp__Host`, `Smtp__Port`, `Smtp__FromEmail`, `Smtp__Username`, `Smtp__Password`
- `Verification__HashKey` (secreto persistente para los HMAC)
- `Authentication__TokenSigningKey` para firma de sesiones cuando aplique
- `ASPNETCORE_ENVIRONMENT`

No se incluyen valores de secretos en este manual. Para desarrollo, usar Secret Manager o variables de entorno; para producción, un almacén de secretos del proveedor.

### Base de datos

La API consume procedimientos SQL preinstalados. Consulte `Backend/README.md` para contratos de registro idempotente, verificación y recuperación, y las rutas de scripts de base documentadas allí. La API no realiza migraciones al arrancar. Antes de actualizar una instalación, aplicar la migración correspondiente en la base correcta y revisar el estado de procedimientos. La copia SQLite local contiene `IdUsuario` en las tablas privadas y restringe consultas por usuario; no poner datos de usuarios distintos en rutas locales compartidas.

## 4. Arquitectura real

```text
WinForms (CapaPresentacion, .NET Framework 4.8)
  -> CapaNegocio (validación, tareas, cuenta, bloqueo, sincronización)
      -> CapaDatos (procedimientos remotos, HTTP, SQLite local)
      -> CapaEntidades (modelos)
API ASP.NET Core (.NET 9)
  -> registro, verificación, recuperación y sesión
  -> SQL Server/Azure SQL y SMTP
```

### Proyectos y responsabilidades

- `CapaPresentacion`: formularios, calendario, selección, preferencias, bandeja, mensajes y composición visual.
- `CapaNegocio`: reglas de validación y coordinación; mantiene mecanismos de inicio de sesión y sincronización sin conexión.
- `CapaDatos`: acceso a API, procedimientos SQL y persistencia SQLite.
- `CapaEntidades`: entidades compartidas entre capas.
- `Backend`: API .NET 9 para registro idempotente, correo, recuperación, sesión y endpoints de sincronización.
- `Backend.Tests`: ejecutable pequeño que prueba servicios backend usando dobles en memoria.
- `database`: scripts SQL y documentación de sincronización local.

### Flujo de registro y autenticación

WinForms valida y guarda un registro pendiente local. El hash PBKDF2 viaja por HTTPS al endpoint de registro; la API valida y ejecuta el procedimiento SQL idempotente. Verificación y recuperación envían código por correo, guardan resumen HMAC y exponen respuestas que evitan enumerar cuentas. El inicio de sesión del cliente consulta el servicio de datos y puede recurrir a la copia local protegida cuando no hay conexión.

### Tareas en segundo plano y bandeja

`PrincipalForm` posee timers WinForms para sincronización (20 s) y avisos de tareas (15 s), además de supervisores de bloqueo de negocio. Cerrar con la X o usar minimizar oculta el formulario principal, quita su botón de la barra y conserva el proceso; el menú/contexto o doble clic del icono restaura la ventana. Al estar oculto, los avisos de tipo ventana no deben abrir formularios emergentes desde timers o callbacks; siguen funcionando sincronización y supervisión, y las notificaciones del icono pueden seguir mostrándose según preferencias. Cuando la sincronización detecta que no hay red, conserva los cambios locales y reintenta en el siguiente ciclo sin mostrar un aviso rutinario. Los errores que sí requieren atención se traducen a mensajes breves; los detalles técnicos permanecen en los registros locales.

## 5. Ejecución y diagnóstico

### Ejecutar la API

Desde `Backend`, configure las variables de entorno requeridas y use `dotnet run --project .\Backend.csproj`. `GET /health/live` comprueba vida. Swagger aparece en desarrollo y se desactiva automáticamente en producción.

### Ejecutar escritorio

Compilar e iniciar `CapaPresentacion` desde Visual Studio en Windows. Para parar el proceso, seleccionar **Salir** en el menú del icono de bandeja. Cerrar la X lo oculta; no termina el proceso. La tarea de bloqueo y sincronización puede continuar ejecutándose.

### Diagnóstico

Los errores de UI se registran en `%LOCALAPPDATA%\Programa-de-Concentracion\Logs`. En la API, revisar los logs del proceso ASP.NET Core/hosting. Ante cambios pendientes sin red, confirmar el estado de conexión y dejar que el cliente reintente; los datos locales pendientes no deben borrarse manualmente.

## 6. Pruebas

No hay un framework unitario tradicional en el repositorio: `Backend.Tests` es un ejecutable .NET 9. Ejecutar:

```powershell
dotnet run --project .\Backend.Tests\Backend.Tests.csproj
```

La suite ejecutada en esta revisión tuvo 38 comprobaciones correctas. No existe selector por categoría ni suite de UI. La compilación general debe hacerse con herramientas que resuelvan los proyectos clásicos y SQLite. El inventario detallado y resultados están en `docs/tests/Inventario_pruebas_v1.0.md`.

## 7. Seguridad, datos y límites

No publicar archivos `.pubxml.user`, perfiles Web Deploy, secretos SMTP, cadenas SQL o claves HMAC. Los métodos de datos privados deben conservar su alcance por `IdUsuario`. Aplicar scripts de base con una cuenta autorizada y verificar aislamiento entre usuarios antes de promover un cambio de esquema. La base real, SMTP y el comportamiento visual de tray no se validaron en esta ejecución.

## 8. Actualizaciones de escritorio

Al mostrarse la pantalla principal o el acceso en un proceso nuevo, `ActualizadorGithub` consulta la publicación más reciente de `Elmanueld/Programa-de-Concentracion` mediante la API pública de GitHub. Solo ofrece actualizar si el tag es una versión numérica superior a `AssemblyFileVersion` y la publicación contiene `ProgramaConcentracion-Instalador.zip` con digest SHA-256. La aplicación descarga el instalador tras la aceptación, compara el digest de GitHub, valida los nombres del ZIP y solicita elevación para ejecutar `Instalar.ps1`. El instalador espera a que cierre el proceso, copia los archivos y vuelve a iniciar la aplicación. Los errores de red o la ausencia de una publicación se omiten sin interrumpir el inicio.

Para crear una publicación válida, empuja un tag como `v1.0.1`. El workflow `.github/workflows/release.yml` compila el instalador, estampa la versión del tag en el ejecutable y crea un GitHub Release con el ZIP esperado. El repositorio debe tener habilitadas GitHub Actions y permisos de escritura de releases para el token del workflow.
