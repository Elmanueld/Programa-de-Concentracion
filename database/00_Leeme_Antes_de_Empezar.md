# Base de datos del programa

## Instalación nueva

Usa `InstalarBaseDatos.ps1` o ejecuta en SSMS estos scripts en orden:

1. `01_CrearBaseDeDatos.sql`
2. `02_CrearTablas.sql` (incluye relaciones y claves compuestas por propietario)
3. `04_CrearProcedimientos.sql`
4. `05_CrearIndices.sql`
5. `06_MigracionSincronizacionOffline.sql`
6. `07_MigracionPlantillaPredeterminada.sql`
7. `08_MigracionTareasDetenidas.sql`

`03_CrearRelaciones.sql` se mantiene como archivo de compatibilidad; no contiene cambios adicionales porque las relaciones ya se crean en el script 02. La verificación de correo está incluida en el script 04.

## Base existente

Haz una copia de seguridad antes de actualizar. Ejecuta las migraciones 06, 07 y 08 sobre la base existente. No vuelvas a correr los scripts de creación ni elimines la base para aplicar una actualización. Revisa también [`../src/CapaDatos/database/README_SincronizacionOffline.md`](../src/CapaDatos/database/README_SincronizacionOffline.md) para el esquema offline y los contratos de sincronización.

Las tablas de tareas, catálogos y relaciones mantienen `IdUsuario`; las relaciones usan claves foráneas compuestas para impedir asociaciones cruzadas entre cuentas. Los procedimientos verifican el propietario en cada lectura o escritura.
