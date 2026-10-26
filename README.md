# Programa de Concentración

Aplicación de escritorio para organizar tareas y sesiones de concentración. Este avance incorpora acceso de usuarios y gestión básica de tareas persistidas en SQL Server, conservando Windows Forms y las cuatro capas originales.

## Requisitos

- Windows 10/11.
- Visual Studio 2022, recomendado 17.14 (versión de la solución original), con «Desarrollo de escritorio de .NET».
- .NET Framework 4.8 y Developer Pack/Targeting Pack 4.8.
- SQL Server Express (2019 o 2022 recomendado), SQL Server Management Studio (SSMS) y, para el instalador automático, `sqlcmd`.
- Para integración automática: SQL Server Express LocalDB, independiente de SQLEXPRESS.

No hay paquetes NuGet externos. Las referencias son de .NET Framework y de los proyectos de la solución.

## Arquitectura y estructura

| Carpeta | Responsabilidad |
| --- | --- |
| `src/CapaDatos/CapaPresentacion` | Formularios de registro, login, listado y editor; llama a negocio. |
| `src/CapaDatos/CapaNegocio` | Validaciones, contraseñas y reglas de usuarios/tareas. |
| `src/CapaDatos/CapaEntidades` | Objetos del dominio, incluidos `E_Usuario` y `E_Tarea`. |
| `src/CapaDatos/CapaDatos` | Conexiones, parámetros SQL y procedimientos almacenados. |
| `database` | Instalación y actualización SQL. |
| `tests` | Validaciones, contraseñas e integración SQL en instancia exclusiva. |
| `.github/workflows` | Verificación en Windows. |

La carpeta contenedora `src/CapaDatos` conserva la organización original: incluye los cuatro proyectos y `CapaDatos.sln`. Los formularios no ejecutan consultas SQL.

## Instalación

```powershell
git clone https://github.com/Elmanueld/Programa-de-Concentracion.git
cd Programa-de-Concentracion
git switch entregable-3-avance-funcional
```

Iniciar el servicio SQL Server (SQLEXPRESS). Si **no existe** la base, ejecutar en SSMS, en este orden:

1. `database/01_CrearBaseDeDatos.sql`
2. `database/02_CrearTablas.sql`
3. `database/03_CrearRelaciones.sql`
4. `database/04_CrearProcedimientos.sql`
5. `database/05_CrearIndices.sql`

Alternativamente, desde PowerShell en la raíz:

```powershell
.\database\InstalarBaseDatos.ps1 -Servidor '.\SQLEXPRESS'
```

El instalador existente rechaza una base ya creada para proteger sus datos. Los scripts 01–05 crean objetos y no deben repetirse sobre una base existente.

Si la base **ya estaba instalada con la versión anterior**, respaldarla y ejecutar únicamente `database/06_ActualizarEntregable3.sql` en SSMS. Actualiza cuatro procedimientos sin borrar tablas ni registros. No se necesita para una instalación nueva con los scripts actualizados.

## Configuración de conexión

La aplicación utiliza la entrada `conexion` en `src/CapaDatos/CapaPresentacion/App.config`:

```xml
<add name="conexion"
     connectionString="Server=.\SQLEXPRESS;Database=ProgramaConcentracion;Integrated Security=True;"
     providerName="System.Data.SqlClient" />
```

Cambiar `Server` si la instancia tiene otro nombre. La autenticación integrada usa el usuario Windows que ejecuta el programa; debe tener permisos sobre la base y sus procedimientos. No agregar credenciales privadas al repositorio. Datos obtiene esta entrada mediante `ConfigurationManager`; se copia a `CapaPresentacion.exe.config` al compilar.

## Compilación y ejecución

1. Abrir `src/CapaDatos/CapaDatos.sln` en Visual Studio.
2. Establecer **CapaPresentacion** como proyecto de inicio.
3. Elegir Debug o Release, Any CPU, y **Recompilar solución**.
4. Ejecutar con F5 o Ctrl+F5.

La solución raíz `Programa-de-Concentracion.slnx` ahora incluye los proyectos y pruebas; requiere una versión compatible de Visual Studio. La solución `.sln` original permanece disponible.

Desde Developer PowerShell de Visual Studio:

```powershell
msbuild src/CapaDatos/CapaDatos.sln /restore /t:Rebuild /p:Configuration=Release
msbuild tests/Pruebas.csproj /restore /t:Rebuild /p:Configuration=Release
```

## Funcionalidades y demostración del sprint

### Registro e inicio/cierre de sesión

1. Abrir **Registro** y completar nombre, correo, contraseña de 8–128 caracteres y confirmación. Se validan los campos y SQL impide correos duplicados.
2. La cuenta queda pendiente de verificación según el procedimiento existente. Este sprint **no envía correos**. Para demostrar el acceso, el administrador verifica la cuenta recién creada en SSMS:

   ```sql
   USE ProgramaConcentracion;
   DECLARE @IdUsuario INT;
   SELECT @IdUsuario = IdUsuario FROM Usuarios
   WHERE Correo = 'correo-registrado@example.com' AND Eliminado = 0;
   IF @IdUsuario IS NULL THROW 50005, 'No se encontró la cuenta.', 1;
   EXEC dbo.sp_VerificarCorreo @IdUsuario = @IdUsuario;
   ```

   Sustituir el correo por el registrado. Es un paso administrativo de demostración, no una comprobación real del buzón.
3. Probar contraseña incorrecta: debe rechazarse. Iniciar con la correcta: debe abrir la pantalla principal.
4. **Cerrar sesión** devuelve al login, limpia la contraseña y descarta el usuario de la ventana anterior.

La contraseña se almacena mediante PBKDF2-SHA256, 600 000 iteraciones, sal aleatoria de 16 bytes y hash de 32 bytes. El hash no se entrega a la pantalla principal. Cinco fallos causan una espera local de 30 segundos; se reinicia al cerrar el proceso y no sustituye un control persistente. Los hashes antiguos de otros formatos necesitan una política de migración; no se interpretan como contraseñas en texto plano.

### Gestión de tareas

1. En **Mis tareas**, pulsar **Crear**, indicar nombre, inicio y fin y guardar.
2. Comprobar el registro en el listado. **Buscar** vuelve a consultar SQL Server.
3. Seleccionar, **Editar**, cambiar nombre/horario y guardar.
4. Cerrar sesión, volver a entrar y comprobar que la edición persiste.
5. Seleccionar y **Eliminar**, confirmar y volver a consultar.
6. Registrar y verificar un segundo usuario: no debe ver las tareas del primero.

Las tareas nuevas se guardan con estado `P`. Las `A` (activas) no se pueden editar ni eliminar; las `C` (completadas) son editables según RF-006. SQL verifica propietario y estado en la misma operación. Un registro ajeno, inexistente o activo informa error.

Este avance gestiona nombre y horario. Bloqueo efectivo de aplicaciones/webs, descansos automáticos, recursos permitidos y preferencias completas quedan para avances posteriores. Se conservan las entidades y procedimientos previos. Este CRUD todavía no ejecuta las restricciones de concentración del sistema operativo.

## Pruebas

Después de compilar:

```powershell
.\tests\bin\Release\Pruebas.exe
```

Prueba contraseñas correctas/incorrectas, sal aleatoria, hashes malformados y validaciones. No declara aprobada la persistencia sin integración.

En una máquina de pruebas con LocalDB:

```powershell
.\tests\PrepararPruebas.ps1
.\tests\bin\Release\Pruebas.exe --integracion
```

El instalador se detiene si ya existe la instancia exclusiva. `tests/App.config` utiliza `(localdb)\Entregable3Pruebas`; la integración rechaza cualquier otra instancia. Se ejecutan los mismos scripts 01–05. Las cuentas de pruebas tienen correos aleatorios `example.com`. Se prueban registro, duplicados, cuenta pendiente, login correcto/incorrecto, persistencia, aislamiento de usuarios, edición/eliminación, estados y cuenta eliminada. Se limpian únicamente los registros de esa ejecución, sin borrar base ni instancia. Para repetir, ejecutar el EXE sobre la instancia preparada, sin repetir el instalador.

La rama incluye la ejecución **Verificar Entregable 3** en **Actions**. El entorno Linux de preparación no permite ejecutar WinForms ni SQL Server Express; las pruebas visuales y cierre de sesión se deben completar en Windows con la demostración anterior. El estado concreto y las limitaciones se documentan en `docs/ENTREGABLE_3.md`.

## Correcciones de compatibilidad

- Se retiró de `App.config` una sección no declarada que podía impedir leer la configuración.
- `sp_IniciarSesion` devuelve también el nombre y conserva los filtros de cuenta activa y correo verificado.
- Modificar, eliminar y cambiar estado de una tarea reciben `@IdUsuario`; se actualizaron juntos los consumidores C#. Las llamadas externas antiguas deben agregar ese parámetro.
- Edición antes exigía `P`; ahora admite estados distintos de `A`, conforme a RF-006. Las operaciones sin registros afectados informan error.
- Se conservaron tablas, columnas, claves y nombres de procedimientos. `06_ActualizarEntregable3.sql` permite actualizar instalaciones existentes.
