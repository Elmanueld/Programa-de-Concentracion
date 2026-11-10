# Programa de Concentración

Aplicación de escritorio para planificar sesiones de concentración, seleccionar aplicaciones y webs y aplicar bloqueos durante las tareas. Está construida con Windows Forms sobre .NET Framework 4.8 y SQL Server.

## Funciones

- Registro, inicio y cierre de sesión.
- Tareas con horario, descansos, aplicaciones y webs asociadas.
- Opciones por tarea para omitir el bloqueo de aplicaciones o de webs.
- Catálogos de aplicaciones y webs privados por usuario. Las tareas y sus asociaciones también incluyen al propietario; las claves foráneas compuestas impiden vincular recursos de cuentas distintas.
- Bloqueo de aplicaciones y webs mientras hay una sesión activa, con avisos y formularios de descanso.
- Interfaz en español con fuente Inter y controles AntdUI.

## Requisitos

- Windows 10/11.
- Visual Studio 2022 con desarrollo de escritorio de .NET y el Developer Pack de .NET Framework 4.8.
- SQL Server Express (por ejemplo, la instancia local `SQLEXPRESS`) y, para instalar desde PowerShell, `sqlcmd`.
- NuGet para restaurar AntdUI 2.4.19. Visual Studio también puede restaurar el paquete al compilar.

## Obtener el proyecto

La rama activa es `main`:

```powershell
git clone https://github.com/Elmanueld/Programa-de-Concentracion.git
cd Programa-de-Concentracion
```

## Instalar la base de datos

La instalación crea una base nueva y se detiene si `ProgramaConcentracion` ya existe. Desde PowerShell, en la raíz del repositorio:

```powershell
.\database\InstalarBaseDatos.ps1 -Servidor '.\SQLEXPRESS'
```

También puedes ejecutar en SSMS, en este orden:

1. `database/01_CrearBaseDeDatos.sql`
2. `database/02_CrearTablas.sql`
3. `database/03_CrearRelaciones.sql`
4. `database/04_CrearProcedimientos.sql`
5. `database/05_CrearIndices.sql`

Los scripts crean el esquema actual desde cero; no son migraciones ni deben ejecutarse sobre una base existente. Para reinstalar, respalda primero los datos y elimina la base por separado. La tabla de usuarios, tareas, aplicaciones, webs, descansos y relaciones está organizada para mantener los datos vinculados a su propietario.

## Configurar y ejecutar

La conexión está en `src/CapaDatos/CapaPresentacion/App.config`. El valor de ejemplo usa autenticación integrada de Windows:

```xml
<add name="conexion"
     connectionString="Server=.\SQLEXPRESS;Database=ProgramaConcentracion;Integrated Security=True;"
     providerName="System.Data.SqlClient" />
```

Cambia el servidor si tu instancia tiene otro nombre. La cuenta de Windows que ejecuta el programa debe tener permisos sobre la base.

El archivo de ejemplo tiene `ModoDemostracion=true`: permite probar el acceso sin SMTP y no marca el correo como verificado. Para usar la verificación normal, configura SMTP mediante las variables de entorno `PC_SMTPHOST`, `PC_SMTPPUERTO`, `PC_SMTPSSL`, `PC_SMTPREMITENTE`, `PC_SMTPUSUARIO` y `PC_SMTPCLAVE`, y cambia `ModoDemostracion` a `false`. No guardes credenciales en Git.

Abre `src/CapaDatos/CapaDatos.sln` en Visual Studio, selecciona `CapaPresentacion` como proyecto de inicio y compila. Desde Developer PowerShell también puedes ejecutar:

```powershell
nuget restore src/CapaDatos/CapaDatos.sln
msbuild src/CapaDatos/CapaDatos.sln /t:Rebuild /p:Configuration=Release
```

## Pruebas

Las pruebas usan una instancia LocalDB separada llamada `Entregable3Pruebas`; no apuntan a `SQLEXPRESS` ni a la base de desarrollo.

```powershell
nuget restore src/CapaDatos/CapaDatos.sln
msbuild tests/Pruebas.csproj /t:Rebuild /p:Configuration=Release
.\tests\bin\Release\Pruebas.exe
.\tests\bin\Release\Pruebas.exe --interfaz
.\tests\PrepararPruebas.ps1
.\tests\bin\Release\Pruebas.exe --integracion
```

`PrepararPruebas.ps1` crea la instancia y la base de pruebas, y ejecuta los cinco scripts SQL actuales. Si la instancia ya existe, se detiene sin borrar ni reemplazar sus datos. La integración comprueba registro y acceso, privacidad entre cuentas, operaciones de tareas y el envío de verificación a un servidor SMTP local de prueba. La prueba `--interfaz` abre los formularios sin consultar SQL.

GitHub Actions compila el proyecto y las pruebas en Windows y ejecuta las pruebas unitarias, de interfaz e integración en cada cambio a `main` y en solicitudes de incorporación a esa rama.
