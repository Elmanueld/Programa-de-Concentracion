# Programa de Concentración

Aplicación Windows Forms para planificar tareas, administrar aplicaciones y sitios asociados, y continuar trabajando sin conexión mediante SQLite. El Backend ASP.NET Core es el acceso remoto a SQL Server y gestiona autenticación, sincronización, verificación de correo y recuperación de contraseña.

## Requisitos de desarrollo

- Windows 10/11.
- Visual Studio 2022 con desarrollo de escritorio .NET.
- .NET Framework 4.8 Developer Pack para WinForms y .NET 9 SDK para el Backend y sus pruebas.
- Conexión a Internet para restaurar dependencias NuGet.

SQL Server Express/LocalDB no es requisito de la aplicación ni de las pruebas automáticas actuales. No se necesita instalar SQL Server en los equipos cliente.

## Soluciones

Abre `src/CapaDatos/ProgramaConcentracion.sln` o la solución de Visual Studio `Programa-de-Concentracion.slnx` en la raíz. Ambas incluyen las capas de escritorio, el Backend .NET 9 y `Backend.Tests`. `CapaPresentacion` es el proyecto de inicio para ejecutar el programa.

- `src/CapaDatos/CapaPresentacion`: formularios y controles WinForms.
- `src/CapaDatos/CapaNegocio`: reglas y flujo offline/online.
- `src/CapaDatos/CapaEntidades`: entidades.
- `src/CapaDatos/CapaDatos`: SQLite local, cliente HTTP y sincronización.
- `src/CapaDatos/Backend`: API ASP.NET Core y acceso a SQL Server remoto.
- `src/CapaDatos/Backend.Tests`: pruebas del Backend con repositorios falsos.
- `tests`: validaciones del cliente y pruebas de interfaz.

## Datos y configuración

WinForms conserva SQLite en `%LOCALAPPDATA%\Programa-de-Concentracion\DatosOffline\ProgramaConcentracion.sqlite`. La API remota usa `ConnectionStrings__DefaultConnection`, SMTP y `Verification__HashKey`, configurados en el entorno del Backend. No agregues credenciales, cadenas de conexión, contraseñas de publicación ni códigos al repositorio.

La API pública configurada para WinForms es `https://concentracion-api.runasp.net/`. La conexión SQL no se distribuye a los equipos cliente.

## Base de datos

Selecciona la base asignada por MonsterASP antes de ejecutar los scripts. Para una instalación nueva sigue el orden indicado en `database/00_Leeme_Antes_de_Empezar.md`. Para una base existente aplica únicamente las migraciones requeridas y respaldadas. Las migraciones de registro idempotente y recuperación de contraseña son `database/09_MigracionRegistroApiIdempotente.sql` y `database/10_MigracionRecuperacionContrasena.sql`.

Los procedimientos y tablas deben conservar el aislamiento por `IdUsuario`; no ejecutes scripts contra producción desde las pruebas automatizadas.

## Compilar

Desde Developer PowerShell de Visual Studio, en la raíz del repositorio:

```powershell
nuget restore src/CapaDatos/ProgramaConcentracion.sln -NonInteractive
msbuild src/CapaDatos/ProgramaConcentracion.sln /restore /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU"
msbuild tests/Pruebas.csproj /restore /t:Rebuild /p:Configuration=Release
```

## Pruebas automáticas

```powershell
dotnet run --project src/CapaDatos/Backend.Tests/Backend.Tests.csproj --configuration Release
.\tests\bin\Release\Pruebas.exe
.\tests\bin\Release\Pruebas.exe --interfaz
```

Las pruebas del Backend usan repositorios y SMTP falsos. Las pruebas WinForms ejercitan validación y controles. No contactan MonsterASP, SQL remoto ni servidores SMTP reales; no escriben en la base del programa. El workflow de GitHub ejecuta esas mismas pruebas al abrir un pull request o al hacer push a `main`.

La integración con SQL/SMTP remoto debe hacerse en un entorno de prueba aislado, con base, credenciales y buzón de prueba; no se ejecuta automáticamente para evitar modificar datos o enviar correos reales. Consulta el plan y el último reporte en `docs/PLAN_PRUEBAS.md` y `docs/REPORTE_DE_PRUEBAS.md`.

## Publicación

La compilación del Backend y el push a GitHub no publican automáticamente en MonsterASP. La publicación requiere ejecutar el perfil de Web Deploy de forma manual y configurar las variables del sitio. No publiques el perfil local `.pubxml.user`.