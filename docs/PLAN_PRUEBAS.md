# Plan de pruebas

## Alcance automático

La suite cubre validación de contratos y reglas de registro, envío y confirmación de códigos, recuperación de contraseña y flujos de interfaz. Los servicios del Backend se prueban mediante repositorios y SMTP falsos; por tanto las pruebas son deterministas y no acceden a MonsterASP, bases remotas ni buzones reales.

## Requisitos

- Windows 10/11 y Visual Studio 2022.
- .NET Framework 4.8 Developer Pack para pruebas de WinForms.
- .NET 9 SDK para Backend.Tests.
- NuGet para restaurar dependencias.

No requiere SQL Server Express/LocalDB.

## Ejecución

Desde la raíz del repositorio:

```powershell
nuget restore src/CapaDatos/ProgramaConcentracion.sln -NonInteractive
msbuild src/CapaDatos/ProgramaConcentracion.sln /restore /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU"
msbuild tests/Pruebas.csproj /restore /t:Rebuild /p:Configuration=Release
.\tests\bin\Release\Pruebas.exe
.\tests\bin\Release\Pruebas.exe --interfaz
dotnet run --project src/CapaDatos/Backend.Tests/Backend.Tests.csproj --configuration Release
```

## Casos Backend

- Normalización del correo y envío solo cuando el repositorio prepara una solicitud.
- Código aleatorio de seis dígitos y persistencia del resumen, nunca del código en claro.
- Confirmación del código y rechazo de códigos con formato inválido antes de acceder al repositorio.
- Falta de `Verification__HashKey` y fallo SMTP no reportados como éxito.
- Formato PBKDF2 requerido e idempotencia/conflicto del registro remoto.
- Recuperación: cuenta no elegible no recibe correo; cambio de contraseña solo tras código válido.
- Atributos de validación en parámetros primarios de records para email, código y hash.

## Límites y pruebas manuales pendientes

Las pruebas automáticas no validan la conectividad del sitio MonsterASP, las credenciales SQL/SMTP reales, las migraciones en una base existente, ni el aislamiento entre dos usuarios en un servidor remoto. Para esas pruebas se necesita una instancia de staging y datos desechables. Nunca se debe apuntar la suite a producción.

Las pruebas heredadas `--integracion` y `--correo-real` se retiraron porque dependían de SQL LocalDB y de una clase SMTP anterior a la API. Ya no son comandos soportados.