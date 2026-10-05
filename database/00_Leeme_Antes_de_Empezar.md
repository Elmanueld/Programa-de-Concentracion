# Base de Datos - Programa de Concentración

## Requisitos

* SQL Server
* SQL Server Management Studio (SSMS)

## Orden de ejecución

Los archivos SQL deben ejecutarse en el siguiente orden:

### 1. `01_CrearBaseDeDatos.sql`

Crea la base de datos **ProgramaConcentracion** y establece esta base de datos como contexto para los siguientes scripts.

### 2. `02_CrearTablas.sql`

Crea todas las tablas necesarias para el funcionamiento de la aplicación, incluyendo:

* Usuarios
* Tareas
* Descansos
* Configuraciones
* Aplicaciones
* Webs
* Tablas intermedias para las relaciones entre tareas, aplicaciones y sitios web.

### 3. `03_CrearRelaciones.sql`

Define las relaciones entre las tablas mediante claves foráneas, claves primarias compuestas, restricciones `UNIQUE` y otras restricciones necesarias para mantener la integridad de los datos.

### 4. `04_CrearProcedimientos.sql`

Crea los procedimientos almacenados (`Stored Procedures`) utilizados por la aplicación para realizar operaciones sobre la base de datos, como registrar, buscar, modificar y eliminar información.

## Importante

Los scripts deben ejecutarse en el orden indicado, ya que algunos archivos dependen de objetos creados por los archivos anteriores.

Todos los scripts pertenecen a la misma base de datos:

**ProgramaConcentracion**
