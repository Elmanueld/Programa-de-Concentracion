# Programa de Concentración

Aplicación de escritorio para organizar tareas y sesiones de concentración. En este avance se agregaron el registro, el inicio de sesión y el manejo básico de tareas guardadas en SQL Server. Se mantienen Windows Forms y las cuatro capas del grupo.

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
| `tests` | Pruebas de validaciones, contraseñas y operaciones SQL en una instancia separada. |
| `.github/workflows` | Verificación en Windows. |

La carpeta contenedora `src/CapaDatos` conserva la organización original: incluye los cuatro proyectos y `CapaDatos.sln`. Los formularios no ejecutan consultas SQL.

## Instalación

Si se recibió el archivo `.bundle`, seguir primero los pasos de `docs/ENTREGABLE_3.md` para importar la rama. Los siguientes comandos son para cuando la rama esté publicada.

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
6. `database/07_VerificacionCorreo.sql`

Alternativamente, desde PowerShell en la raíz:

```powershell
.\database\InstalarBaseDatos.ps1 -Servidor '.\SQLEXPRESS'
```

El instalador se detiene si la base ya existe para no afectar sus datos. Los scripts 01–05 crean objetos y no deben repetirse sobre una base existente.

Si la base **ya estaba instalada con la versión anterior**, respaldarla y ejecutar `database/06_ActualizarEntregable3.sql` y después `database/07_VerificacionCorreo.sql` en SSMS. Actualiza cuatro procedimientos sin borrar tablas ni registros. El script 06 no se necesita para una instalación nueva con los scripts actualizados. El 07 agrega los códigos de verificación y se ejecuta también en instalaciones nuevas.

Si crear y consultar tareas funciona pero editar o eliminar devuelve SQL 8144, compruebe que se ejecutó el script 06 en la misma instancia y base de `App.config`. Actualizar el código desde Git no actualiza por sí solo los procedimientos de una base ya instalada.

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
2. Al registrar la cuenta se abre **Verificar correo** y se intenta enviar un código de seis dígitos por SMTP. Introducir el código y pulsar **Verificar correo**. También se puede entrar desde el botón **Verificar mi correo** del login.
   El código vence en 10 minutos, tiene un máximo de cinco intentos y es de un solo uso. **Solicitar / reenviar código** permite otro envío después de un minuto. El nuevo código reemplaza al anterior. Si falla SMTP, la cuenta permanece pendiente y se puede reintentar; no se verifica automáticamente.
3. Probar contraseña incorrecta: debe rechazarse. Iniciar con la correcta: debe abrir la pantalla principal.
4. **Cerrar sesión** devuelve al login, limpia la contraseña y descarta el usuario de la ventana anterior.

La contraseña no se guarda directamente. Se usa PBKDF2-SHA256 para calcular un hash con 600 000 repeticiones, una sal aleatoria de 16 bytes y un resultado de 32 bytes. La sal hace que dos cuentas con la misma contraseña tengan hashes diferentes. Al iniciar sesión, se vuelve a calcular el valor y se compara con el guardado. Todo se hace con las clases de seguridad de .NET.

El hash no se pasa a la pantalla principal. Después de cinco fallos se esperan 30 segundos para intentar de nuevo. Ese contador se reinicia al cerrar el programa. Si existen cuentas antiguas con otro formato de hash, habrá que adaptar su lectura; no se aceptan como contraseñas en texto plano.

### Configuración del correo

El envío usa SMTP con STARTTLS, normalmente en el puerto 587. El servidor debe permitir ese tipo de conexión; no se usa TLS implícito del puerto 465. Los datos se leen de `appSettings` en `CapaPresentacion/App.config`. Las variables de entorno tienen prioridad:

| Configuración | Variable de entorno |
|---|---|
| SmtpHost | PC_SMTPHOST |
| SmtpPuerto | PC_SMTPPUERTO |
| SmtpSsl | PC_SMTPSSL |
| SmtpRemitente | PC_SMTPREMITENTE |
| SmtpUsuario | PC_SMTPUSUARIO |
| Contraseña | PC_SMTPCLAVE (solo variable de entorno) |

Host, remitente y usuario están vacíos en el repositorio. Configurarlos con los datos del servidor utilizado por el grupo. `SmtpSsl` debe ser `true` para servidores remotos. Solo se admite `false` en localhost para pruebas. No guardar contraseñas SMTP en Git. Reiniciar Visual Studio después de establecer las variables para que el programa las reciba. El remitente debe estar autorizado por el proveedor de correo.

El login admite el resultado anterior de `sp_IniciarSesion` que no incluía `Nombre`: recupera el nombre por ID después de validar la contraseña. Esto no sustituye la actualización 06 de los procedimientos de tareas. `Nombre` no es un parámetro de entrada del login.

El formulario `Form1` contiene acceso, registro y verificación. Sus controles están declarados en `Form1.Designer.cs` y los eventos en `Form1.cs`, para poder editarlos con el diseñador de Windows Forms.

### Gestión de tareas

1. En **Mis tareas**, pulsar **Crear**, indicar nombre, inicio y fin y guardar.
2. Comprobar el registro en el listado. **Buscar** vuelve a consultar SQL Server.
3. Seleccionar, **Editar**, cambiar nombre/horario y guardar.
4. Cerrar sesión, volver a entrar y comprobar que la edición persiste.
5. Seleccionar y **Eliminar**, confirmar y volver a consultar.
6. Registrar y verificar un segundo usuario: no debe ver las tareas del primero.

Las tareas nuevas se guardan con estado `P`. Las `A` (activas) no se pueden editar ni eliminar; las `C` (completadas) son editables según RF-006. El procedimiento SQL comprueba que la tarea pertenezca al usuario y que su estado permita el cambio. Si no se puede realizar la operación, se muestra un mensaje.

Este avance gestiona nombre y horario. Bloqueo efectivo de aplicaciones/webs, descansos automáticos, recursos permitidos y preferencias completas quedan para avances posteriores. Se conservan las entidades y procedimientos previos. Este CRUD todavía no ejecuta las restricciones de concentración del sistema operativo.

## Pruebas

Después de compilar:

```powershell
.\tests\bin\Release\Pruebas.exe
```

Estas pruebas revisan contraseñas correctas e incorrectas, sal aleatoria, hashes con formato incorrecto y validaciones. También prueban la lectura del login con el resultado SQL antiguo y actualizado, los códigos y un servidor SMTP local de pruebas. No se envían mensajes a buzones reales. Para comprobar el guardado de datos también hay que ejecutar las pruebas con SQL.

En una máquina de pruebas con LocalDB:

```powershell
.\tests\PrepararPruebas.ps1
.\tests\bin\Release\Pruebas.exe --integracion
```

El instalador se detiene si ya existe la instancia de pruebas. `tests/App.config` usa `(localdb)\Entregable3Pruebas`; las pruebas no aceptan otra instancia para no tocar los datos del proyecto. Se ejecutan los scripts 01–05 y 07. Las cuentas de pruebas tienen correos aleatorios `example.com`. Se revisan registro, correos duplicados, cuenta pendiente, login correcto e incorrecto, guardado de datos, tareas de cada usuario, edición, eliminación, estados y cuenta eliminada. Al terminar se borran solo los registros creados por esa ejecución, sin borrar la base ni la instancia. Para repetir, ejecutar el EXE sobre la instancia preparada, sin repetir el instalador.

Se dejó preparada la ejecución **Verificar Entregable 3** para **Actions**, que podrá usarse después de publicar la rama. Las correcciones de acceso y verificación deben volver a probarse en Windows antes de publicarlas. También hay que revisar las pantallas y el cierre de sesión siguiendo los pasos anteriores. Los resultados y las pruebas pendientes están en `docs/ENTREGABLE_3.md`.

## Ajustes realizados

- Se retiró de `App.config` una sección no declarada que podía impedir leer la configuración.
- `sp_IniciarSesion` devuelve también el nombre y conserva los filtros de cuenta activa y correo verificado.
- Modificar, eliminar y cambiar estado de una tarea reciben `@IdUsuario`; se actualizaron juntos los consumidores C#. Las llamadas externas antiguas deben agregar ese parámetro.
- Edición antes exigía `P`; ahora admite estados distintos de `A`, conforme a RF-006. Las operaciones sin registros afectados informan error.
- Se conservaron tablas, columnas, claves y nombres de procedimientos. `06_ActualizarEntregable3.sql` permite actualizar instalaciones existentes.

Para la prueba de navegación y capturas de acceso, registro y verificación:

```powershell
.\tests\bin\Release\Pruebas.exe --interfaz
```

Las capturas se guardan en `tests/bin/Release/capturas`. Esta prueba abre formularios sin consultar la base; no sustituye la revisión del diseñador en Visual Studio ni la prueba completa con SQL y el SMTP real.
