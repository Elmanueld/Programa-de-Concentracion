# Programa de Concentración

Aplicación de escritorio Windows Forms para organizar tareas, gestionar aplicaciones y webs asociadas, configurar preferencias y mantener datos utilizables sin conexión.

## Requisitos

- Windows 10/11 de 64 bits.
- Visual Studio 2022 con desarrollo de escritorio .NET.
- .NET Framework 4.8 Developer Pack.
- SQL Server Express (instancia local predeterminada `SQLEXPRESS`) para el almacén principal.
- SQL Server Express LocalDB para preparar y ejecutar las pruebas de integración.

Los proyectos usan AntdUI, Svg y sus dependencias NuGet. Restaura los paquetes desde Visual Studio o MSBuild antes de compilar.

## Solución

Abre `Programa-de-Concentracion.slnx` en Visual Studio o `src/CapaDatos/CapaDatos.sln` en una versión compatible. `CapaPresentacion` es el proyecto de inicio. La solución conserva las cuatro capas:

- `src/CapaDatos/CapaPresentacion`: formularios y controles de interfaz.
- `src/CapaDatos/CapaNegocio`: validación y reglas.
- `src/CapaDatos/CapaEntidades`: entidades del dominio.
- `src/CapaDatos/CapaDatos`: acceso a SQL Server, SQLite local y sincronización.

## Base de datos

Para una instalación nueva, ejecuta desde PowerShell en la raíz:

```powershell
.\database\InstalarBaseDatos.ps1 -Servidor '.\SQLEXPRESS'
```

También puedes ejecutar en SSMS, en este orden: `01_CrearBaseDeDatos.sql`, `02_CrearTablas.sql`, `04_CrearProcedimientos.sql`, `05_CrearIndices.sql`, `06_MigracionSincronizacionOffline.sql`, `07_MigracionPlantillaPredeterminada.sql` y `08_MigracionTareasDetenidas.sql`. El archivo `03_CrearRelaciones.sql` se conserva por compatibilidad; las relaciones se crean en el script 02 actualizado.

El instalador se detiene si `ProgramaConcentracion` ya existe. Para una base existente, respáldala y aplica las migraciones 06–08 según las notas en [`database/00_Leeme_Antes_de_Empezar.md`](database/00_Leeme_Antes_de_Empezar.md). No vuelvas a ejecutar scripts de creación sobre una base con datos.

El esquema actual incluye identificadores estables para sincronizar, tablas de propietario y relaciones con claves compuestas por `IdUsuario`. Los procedimientos y las capas de datos validan el propietario antes de leer o cambiar registros. Las pruebas cubren que dos cuentas con valores iguales en tareas, aplicaciones y webs no se accedan mutuamente.

## Conexión y funcionamiento sin conexión

La conexión SQL principal se configura en `src/CapaDatos/CapaPresentacion/App.config`, en la entrada `conexion`. El valor del repositorio apunta a `Server=.\SQLEXPRESS`. Cambia la instancia si tu instalación usa otro nombre.

La copia local SQLite se guarda en `%LOCALAPPDATA%\Programa-de-Concentracion\DatosOffline\ProgramaConcentracion.sqlite`. Al iniciar sesión se obtiene una copia de los datos de esa cuenta. Cuando SQL Server deja de responder, las operaciones compatibles usan SQLite y guardan cambios pendientes en la cola local; al reconectar, la aplicación intenta sincronizarlos. La base remota en la nube no está configurada todavía.

El registro usa actualmente `ModoDemostracion=true` en `App.config`, así que el flujo de demostración no envía correo y no verifica automáticamente la cuenta. El transporte SMTP y la configuración de nube quedan pendientes para una fase posterior; no agregues credenciales al repositorio.

## Compilar

Desde Developer PowerShell de Visual Studio:

```powershell
msbuild src/CapaDatos/CapaDatos.sln /restore /t:Rebuild /p:Configuration=Release
msbuild tests/Pruebas.csproj /restore /t:Rebuild /p:Configuration=Release
```

También puedes abrir la solución, restaurar paquetes y compilar Release desde Visual Studio.

## Pruebas

Consulta el [plan de pruebas](docs/PLAN_PRUEBAS.md), el [reporte](docs/REPORTE_DE_PRUEBAS.md), el [registro de incidencias](docs/INCIDENCIAS_PRUEBAS.md) y los [logs](tests/evidencias/README.md).

```powershell
.\tests\bin\Release\Pruebas.exe
.\tests\bin\Release\Pruebas.exe --interfaz
.\tests\bin\Release\Pruebas.exe --integracion
```

Las pruebas de integración están limitadas a `(localdb)\Entregable3Pruebas`. Si esa instancia no existe, prepararla una vez con `tests/PrepararPruebas.ps1`. El preparador conserva una instancia existente y se niega a sobrescribir una base de pruebas existente. No apunta a `SQLEXPRESS` ni a la base del programa.

## Estado de configuración externa

El flujo de acceso y las preferencias se encuentran implementados en el proyecto. Antes de distribuirlo, configura el servidor de correo real y define el endpoint remoto futuro; estos datos no forman parte de este push. Revisa los pasos y límites conocidos en [`docs/ESTADO_ACTUAL.md`](docs/ESTADO_ACTUAL.md).
