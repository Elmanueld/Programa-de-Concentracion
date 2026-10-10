USE ProgramaConcentracion;
GO

CREATE UNIQUE INDEX UX_Tareas_UnaPlantillaPorUsuario
ON Tareas (IdUsuario)
WHERE EsPlantilla = 1;
GO