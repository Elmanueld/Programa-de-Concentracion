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

`03_CrearRelaciones.sql` se mantiene como archivo de compatibilidad; no contiene cambios adicionales porque las relaciones ya se crean en el script 02. La verificación de correo y la recuperación de contraseña están incluidas en el script 04.

## Base existente

Haz una copia de seguridad antes de actualizar. Ejecuta las migraciones 06, 07, 08, 09 y 10 sobre la base existente. La migración 09 prepara sp_RegistrarUsuario para el alta idempotente desde la API y devuelve IdUsuario; ejecútala con la base correcta seleccionada. La instalación nueva ya recibe esta versión del procedimiento desde el script 04. La migración 10 agrega la recuperación de contraseña con códigos de un solo uso; ejecútala con la base correcta seleccionada. No vuelvas a correr los scripts de creación ni elimines la base para aplicar una actualización. Revisa también [`../src/CapaDatos/database/README_SincronizacionOffline.md`](../src/CapaDatos/database/README_SincronizacionOffline.md) para el esquema offline y los contratos de sincronización.

Las tablas de tareas, catálogos y relaciones mantienen `IdUsuario`; las relaciones usan claves foráneas compuestas para impedir asociaciones cruzadas entre cuentas. Los procedimientos verifican el propietario en cada lectura o escritura.

## Base remota en MonsterASP

La aplicación WinForms conserva SQLite para el funcionamiento offline. Las operaciones online de base de datos se realizan mediante el Backend ASP.NET Core, cuya conexión SQL se configura en el servidor con `ConnectionStrings__DefaultConnection`; no configures una cadena SQL en el equipo cliente. Para MonsterASP, selecciona la base ya creada antes de ejecutar los scripts, sigue el orden de instalación nueva solo si la base está vacía, y usa únicamente las migraciones 09 y 10 (además de las migraciones requeridas por la versión instalada) para actualizar una base existente. No ejecutes `01_CrearBaseDeDatos.sql` contra la base alojada ni desde las pruebas automatizadas.
