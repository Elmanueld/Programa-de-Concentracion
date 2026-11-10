# Base de Datos - Programa de Concentración

## Requisitos

* SQL Server
* SQL Server Management Studio (SSMS)

## Instalación nueva

Ejecuta los archivos en este orden:

1. `01_CrearBaseDeDatos.sql`
2. `02_CrearTablas.sql`
3. `03_CrearRelaciones.sql`
4. `04_CrearProcedimientos.sql`
5. `05_CrearIndices.sql`

`InstalarBaseDatos.ps1` ejecuta estos pasos en orden. La verificación de correo está incluida en `04_CrearProcedimientos.sql`; los scripts de instalación quedan consolidados. Si el certificado del servidor local no es de confianza, ejecuta el instalador con `-ConfiarCertificadoServidor` solo para esa instancia.

## Base de prueba existente

La estructura anterior tenía catálogos globales. Para aplicar el nuevo esquema desde cero, elimina la base `ProgramaConcentracion` y ejecuta la instalación completa con `InstalarBaseDatos.ps1` o los archivos en el orden indicado. Esto elimina las cuentas, tareas y configuraciones guardadas. Conserva una copia si quieres mantener esos datos.

La tabla de aplicaciones y la de webs ahora pertenecen a un usuario. Las tareas, descansos y asociaciones guardan también el propietario, y las claves compuestas impiden relacionar datos de cuentas distintas.

