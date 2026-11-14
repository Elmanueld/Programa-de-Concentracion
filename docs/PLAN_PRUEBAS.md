# Plan de pruebas

## Alcance

Valida acceso y contraseñas, verificación por correo, navegación WinForms, paginación de layouts, operaciones SQL y aislamiento entre cuentas. La integración solo se conecta a `(localdb)\Entregable3Pruebas` y usa cuentas aleatorias que limpia al terminar. Su copia SQLite usa un archivo temporal por ejecución.

## Requisitos y preparación

Windows x64, Visual Studio 2022, .NET Framework 4.8 Developer Pack y SQL Server Express LocalDB.

Desde PowerShell de Visual Studio, en la raíz del repositorio:

```powershell
msbuild tests/Pruebas.csproj /t:Rebuild /p:Configuration=Release
.\tests\bin\Release\Pruebas.exe
.\tests\bin\Release\Pruebas.exe --interfaz
```

Para la primera integración, prepara la instancia dedicada una sola vez:

```powershell
.\tests\PrepararPruebas.ps1
.\tests\bin\Release\Pruebas.exe --integracion
```

El preparador no borra ni sobrescribe una instancia/base existente. La suite verifica también que la conexión corresponde a `Entregable3Pruebas` antes de insertar datos. No cambies `tests/App.config` para apuntar a una base de usuario.

## Cobertura

| Área | Casos cubiertos |
| --- | --- |
| Contraseñas y validación | PBKDF2, sal distinta, contraseña válida/inválida, hash malformado, campos y fechas inválidas. |
| Cuenta y código | Lectura de login anterior/nuevo, registro, código aleatorio de seis dígitos, hash con sal, expiración, límite de reenvío/intentos, uso único y SMTP localhost de pruebas. |
| Interfaz | Login, registro y verificación; campos de contraseña ocultos; navegación entre vistas; límites; barra de paginación, botones AntdUI no recortados, contador de páginas, encabezado fijado y nombres de tarjetas. |
| Privacidad SQL | Dos usuarios con tareas, aplicaciones y webs de valores iguales; consultas aisladas; SQL rechaza modificación/eliminación cruzada. |
| Reglas de tareas | Creación y edición propia; estados activo y completado impiden editar/borrar; tarea pendiente puede eliminarse; borrado inexistente informa error. |
| Datos offline de prueba | SQLite temporal separado de `%LOCALAPPDATA%`; el test de integración no reemplaza la base offline real. |

## Resultados ejecutados el 14 de noviembre de 2026

| Ejecución | Resultado |
| --- | --- |
| Compilación Release de `tests/Pruebas.csproj` | Aprobada, 0 errores y 2 advertencias ya existentes en `Tipografia.cs` y `frmSeleccionarApp.Designer.cs`. |
| Unitarias y componentes | 22 comprobaciones aprobadas. |
| Interfaz (`--interfaz`) | 34 comprobaciones aprobadas: 22 base y 12 de interfaz. |
| Integración LocalDB (`--integracion`) | 71 comprobaciones aprobadas: 22 base y 49 SQL. Incluye opciones de tarea y rechazo de relaciones con aplicaciones/webs de otro propietario. |

Los logs completos y las imágenes resumen están en `tests/evidencias/`. Las pruebas automatizadas de interfaz no reemplazan una revisión visual manual en las pantallas del equipo destino. No se ha probado el envío a un buzón real.
