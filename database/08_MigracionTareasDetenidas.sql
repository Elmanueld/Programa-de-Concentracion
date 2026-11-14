SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints
           WHERE parent_object_id=OBJECT_ID(N'dbo.Tareas')
             AND name=N'CK_Tareas_Estado')
    ALTER TABLE dbo.Tareas DROP CONSTRAINT CK_Tareas_Estado;
GO
ALTER TABLE dbo.Tareas WITH CHECK
    ADD CONSTRAINT CK_Tareas_Estado CHECK (Estado IN ('P','A','C','X'));
GO

ALTER PROCEDURE dbo.sp_ModificarTarea
    @IdTarea INT, @IdUsuario INT, @Nombre VARCHAR(70),
    @FechaInicio DATETIME, @FechaFin DATETIME,
    @NoBloquearApps BIT, @NoBloquearWebs BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Tareas
    SET Nombre=@Nombre, FechaInicio=@FechaInicio, FechaFin=@FechaFin,
        NoBloquearApps=@NoBloquearApps, NoBloquearWebs=@NoBloquearWebs,
        SyncUpdatedUtc=SYSUTCDATETIME()
    WHERE IdTarea=@IdTarea AND IdUsuario=@IdUsuario
      AND EsPlantilla=0 AND Estado='P'
      AND EXISTS (SELECT 1 FROM dbo.Usuarios WHERE IdUsuario=@IdUsuario AND Eliminado=0);
    IF @@ROWCOUNT=0 THROW 50002, 'No se puede editar: tarea inexistente, ajena, activa, completada o detenida.', 1;
END;
GO

ALTER PROCEDURE dbo.sp_EliminarTarea @IdTarea INT, @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.SyncTombstones(IdUsuario,TipoEntidad,SyncId,EliminadoUtc)
    SELECT T.IdUsuario,'Tarea',T.SyncId,SYSUTCDATETIME() FROM dbo.Tareas T
    WHERE T.IdTarea=@IdTarea AND T.IdUsuario=@IdUsuario
      AND T.EsPlantilla=0 AND T.Estado='P'
      AND EXISTS (SELECT 1 FROM dbo.Usuarios WHERE IdUsuario=@IdUsuario AND Eliminado=0)
      AND NOT EXISTS (SELECT 1 FROM dbo.SyncTombstones S
        WHERE S.IdUsuario=T.IdUsuario AND S.TipoEntidad='Tarea' AND S.SyncId=T.SyncId);
    DELETE dbo.Tareas
    WHERE IdTarea=@IdTarea AND IdUsuario=@IdUsuario
      AND EsPlantilla=0 AND Estado='P'
      AND EXISTS (SELECT 1 FROM dbo.Usuarios WHERE IdUsuario=@IdUsuario AND Eliminado=0);
    IF @@ROWCOUNT=0 THROW 50003, 'No se puede eliminar: tarea inexistente, ajena, activa, completada o detenida.', 1;
END;
GO

ALTER PROCEDURE dbo.sp_CambiarEstadoTarea
    @IdTarea INT, @IdUsuario INT, @Estado CHAR(1)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Tareas SET Estado=@Estado, SyncUpdatedUtc=SYSUTCDATETIME()
    WHERE IdTarea=@IdTarea AND IdUsuario=@IdUsuario AND EsPlantilla=0
      AND EXISTS (SELECT 1 FROM dbo.Usuarios WHERE IdUsuario=@IdUsuario AND Eliminado=0)
      AND ((Estado='P' AND @Estado IN ('A','X'))
        OR (Estado='A' AND @Estado IN ('C','X')));
    IF @@ROWCOUNT=0 THROW 50004, 'La transición de estado no es válida para esta tarea.', 1;
END;
GO
