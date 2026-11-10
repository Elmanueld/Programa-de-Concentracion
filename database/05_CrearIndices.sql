USE ProgramaConcentracion;
GO
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;
GO

CREATE UNIQUE INDEX UX_Tareas_UnaPlantillaPorUsuario
ON Tareas (IdUsuario)
WHERE EsPlantilla = 1;
GO

CREATE INDEX IX_Tarea_Aplicacion_Usuario_Aplicacion
ON Tarea_Aplicacion (IdUsuario, IdAplicacion);
GO

CREATE INDEX IX_Tarea_Web_Usuario_Web
ON Tarea_Web (IdUsuario, IdWeb);
GO
