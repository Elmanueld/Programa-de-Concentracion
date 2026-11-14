# Sincronización offline

La aplicación usa SQLite local automáticamente. Su base se crea en `%LOCALAPPDATA%\Programa-de-Concentracion\DatosOffline\ProgramaConcentracion.sqlite`; el esquema se mantiene en `../CapaDatos/SQLiteLocal.cs`. Los datos y la cola de sincronización están limitados por `IdUsuario`.

SQL Server sigue siendo la base principal local (`SQLEXPRESS`). Cuando no responde, las operaciones compatibles leen y guardan en la copia local. Al reconectar, la aplicación procesa la cola y descarga datos del mismo usuario. El servidor remoto en la nube no está configurado aún.

Para instalación nueva o actualización SQL Server, usa los scripts canónicos en [`../../../database`](../../../database) y sigue [`../../../database/00_Leeme_Antes_de_Empezar.md`](../../../database/00_Leeme_Antes_de_Empezar.md). Las migraciones de sincronización, plantilla y estado detenido se numeran 06–08 en esa carpeta.

`offline/01_CrearBaseLocal.sql` documenta el esquema lógico del almacenamiento local. La aplicación crea y migra SQLite al iniciar; no es necesario ejecutar ese archivo manualmente.
