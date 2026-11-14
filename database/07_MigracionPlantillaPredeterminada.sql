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

IF OBJECT_ID('dbo.sp_ObtenerPlantilla', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_ObtenerPlantilla @IdUsuario INT AS BEGIN SET NOCOUNT ON; END;');
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
ALTER PROCEDURE dbo.sp_ObtenerPlantilla @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1) IdTarea,Nombre,FechaInicio,FechaFin,Estado,EsPlantilla,
        NoBloquearApps,NoBloquearWebs,SyncId
    FROM dbo.Tareas
    WHERE IdUsuario=@IdUsuario AND EsPlantilla=1
      AND EXISTS (
          SELECT 1 FROM dbo.Usuarios U
          WHERE U.IdUsuario=@IdUsuario AND U.Eliminado=0
      );
END;
GO
