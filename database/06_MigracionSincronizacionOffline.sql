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
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.Usuarios', 'SyncId') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios ADD SyncId UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Usuarios_SyncId DEFAULT NEWID();
    ALTER TABLE dbo.Usuarios ADD SyncUpdatedUtc DATETIME2(3) NOT NULL
        CONSTRAINT DF_Usuarios_SyncUpdatedUtc DEFAULT SYSUTCDATETIME();
    ALTER TABLE dbo.Usuarios ADD CONSTRAINT UQ_Usuarios_SyncId UNIQUE(SyncId);
END;

IF COL_LENGTH('dbo.Tareas', 'SyncId') IS NULL
BEGIN
    ALTER TABLE dbo.Tareas ADD SyncId UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Tareas_SyncId DEFAULT NEWID();
    ALTER TABLE dbo.Tareas ADD SyncUpdatedUtc DATETIME2(3) NOT NULL
        CONSTRAINT DF_Tareas_SyncUpdatedUtc DEFAULT SYSUTCDATETIME();
    ALTER TABLE dbo.Tareas ADD CONSTRAINT UQ_Tareas_IdUsuario_SyncId UNIQUE(IdUsuario, SyncId);
END;

IF COL_LENGTH('dbo.Aplicaciones', 'SyncId') IS NULL
BEGIN
    ALTER TABLE dbo.Aplicaciones ADD SyncId UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Aplicaciones_SyncId DEFAULT NEWID();
    ALTER TABLE dbo.Aplicaciones ADD SyncUpdatedUtc DATETIME2(3) NOT NULL
        CONSTRAINT DF_Aplicaciones_SyncUpdatedUtc DEFAULT SYSUTCDATETIME();
    ALTER TABLE dbo.Aplicaciones ADD CONSTRAINT UQ_Aplicaciones_IdUsuario_SyncId UNIQUE(IdUsuario, SyncId);
END;

IF COL_LENGTH('dbo.Webs', 'SyncId') IS NULL
BEGIN
    ALTER TABLE dbo.Webs ADD SyncId UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Webs_SyncId DEFAULT NEWID();
    ALTER TABLE dbo.Webs ADD SyncUpdatedUtc DATETIME2(3) NOT NULL
        CONSTRAINT DF_Webs_SyncUpdatedUtc DEFAULT SYSUTCDATETIME();
    ALTER TABLE dbo.Webs ADD CONSTRAINT UQ_Webs_IdUsuario_SyncId UNIQUE(IdUsuario, SyncId);
END;

IF COL_LENGTH('dbo.Descansos', 'SyncId') IS NULL
BEGIN
    ALTER TABLE dbo.Descansos ADD SyncId UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Descansos_SyncId DEFAULT NEWID();
    ALTER TABLE dbo.Descansos ADD SyncUpdatedUtc DATETIME2(3) NOT NULL
        CONSTRAINT DF_Descansos_SyncUpdatedUtc DEFAULT SYSUTCDATETIME();
    ALTER TABLE dbo.Descansos ADD CONSTRAINT UQ_Descansos_IdUsuario_SyncId UNIQUE(IdUsuario, SyncId);
END;

IF COL_LENGTH('dbo.Configuraciones', 'SyncId') IS NULL
BEGIN
    ALTER TABLE dbo.Configuraciones ADD SyncId UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Configuraciones_SyncId DEFAULT NEWID();
    ALTER TABLE dbo.Configuraciones ADD SyncUpdatedUtc DATETIME2(3) NOT NULL
        CONSTRAINT DF_Configuraciones_SyncUpdatedUtc DEFAULT SYSUTCDATETIME();
    ALTER TABLE dbo.Configuraciones ADD CONSTRAINT UQ_Configuraciones_IdUsuario_SyncId UNIQUE(IdUsuario, SyncId);
END;

IF OBJECT_ID('dbo.Tarea_Aplicacion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tarea_Aplicacion (
        IdTarea INT NOT NULL,
        IdUsuario INT NOT NULL,
        IdAplicacion INT NOT NULL,
        SyncUpdatedUtc DATETIME2(3) NOT NULL CONSTRAINT DF_Tarea_Aplicacion_SyncUpdatedUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Tarea_Aplicacion PRIMARY KEY(IdTarea, IdAplicacion, IdUsuario),
        CONSTRAINT FK_Tarea_Aplicacion_Tarea FOREIGN KEY(IdTarea, IdUsuario)
            REFERENCES dbo.Tareas(IdTarea, IdUsuario) ON DELETE CASCADE,
        CONSTRAINT FK_Tarea_Aplicacion_Aplicacion FOREIGN KEY(IdAplicacion, IdUsuario)
            REFERENCES dbo.Aplicaciones(IdAplicacion, IdUsuario) ON DELETE CASCADE
    );
END;

IF OBJECT_ID('dbo.Tarea_Web', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tarea_Web (
        IdTarea INT NOT NULL,
        IdUsuario INT NOT NULL,
        IdWeb INT NOT NULL,
        SyncUpdatedUtc DATETIME2(3) NOT NULL CONSTRAINT DF_Tarea_Web_SyncUpdatedUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Tarea_Web PRIMARY KEY(IdTarea, IdWeb, IdUsuario),
        CONSTRAINT FK_Tarea_Web_Tarea FOREIGN KEY(IdTarea, IdUsuario)
            REFERENCES dbo.Tareas(IdTarea, IdUsuario) ON DELETE CASCADE,
        CONSTRAINT FK_Tarea_Web_Web FOREIGN KEY(IdWeb, IdUsuario)
            REFERENCES dbo.Webs(IdWeb, IdUsuario) ON DELETE CASCADE
    );
END;

IF OBJECT_ID('dbo.SyncTombstones', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SyncTombstones (
        IdUsuario INT NOT NULL,
        TipoEntidad VARCHAR(32) NOT NULL,
        SyncId UNIQUEIDENTIFIER NOT NULL,
        EliminadoUtc DATETIME2(3) NOT NULL CONSTRAINT DF_SyncTombstones_EliminadoUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_SyncTombstones PRIMARY KEY(IdUsuario, TipoEntidad, SyncId),
        CONSTRAINT FK_SyncTombstones_Usuario FOREIGN KEY(IdUsuario)
            REFERENCES dbo.Usuarios(IdUsuario) ON DELETE CASCADE
    );
END;

IF OBJECT_ID('dbo.SyncDispositivos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SyncDispositivos (
        IdUsuario INT NOT NULL,
        IdDispositivo UNIQUEIDENTIFIER NOT NULL,
        NombreDispositivo NVARCHAR(100) NOT NULL,
        UltimaSincronizacionUtc DATETIME2(3) NULL,
        CONSTRAINT PK_SyncDispositivos PRIMARY KEY(IdUsuario, IdDispositivo),
        CONSTRAINT FK_SyncDispositivos_Usuario FOREIGN KEY(IdUsuario)
            REFERENCES dbo.Usuarios(IdUsuario) ON DELETE CASCADE
    );
END;

IF OBJECT_ID('dbo.SyncCambiosAplicados', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SyncCambiosAplicados (
        IdUsuario INT NOT NULL,
        IdDispositivo UNIQUEIDENTIFIER NOT NULL,
        IdCambioLocal BIGINT NOT NULL,
        AplicadoUtc DATETIME2(3) NOT NULL CONSTRAINT DF_SyncCambios_AplicadoUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_SyncCambiosAplicados PRIMARY KEY(IdUsuario, IdDispositivo, IdCambioLocal),
        CONSTRAINT FK_SyncCambios_Usuario FOREIGN KEY(IdUsuario)
            REFERENCES dbo.Usuarios(IdUsuario) ON DELETE CASCADE
    );
END;

COMMIT TRANSACTION;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Keep regular online writes visible to other devices using last-write-wins.
-- These procedures retain their owner filters and add tombstones before hard deletes.
ALTER PROCEDURE dbo.sp_InsertarTarea
    @IdUsuario INT,
    @SyncId UNIQUEIDENTIFIER,
    @Nombre VARCHAR(70),
    @FechaInicio DATETIME,
    @FechaFin DATETIME,
    @EsPlantilla BIT,
    @NoBloquearApps BIT,
    @NoBloquearWebs BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE IdUsuario=@IdUsuario AND Eliminado=0)
            THROW 50001, 'El usuario no existe o está eliminado.', 1;

        DECLARE @IdTareaExistente INT;
        SELECT @IdTareaExistente=IdTarea FROM dbo.Tareas WITH (UPDLOCK,HOLDLOCK)
        WHERE IdUsuario=@IdUsuario AND SyncId=@SyncId;
        IF @IdTareaExistente IS NOT NULL
        BEGIN
            SELECT @IdTareaExistente AS IdTarea;
            COMMIT TRANSACTION;
            RETURN;
        END;

        IF @EsPlantilla=1
        BEGIN
            INSERT dbo.SyncTombstones(IdUsuario,TipoEntidad,SyncId,EliminadoUtc)
            SELECT T.IdUsuario,'Tarea',T.SyncId,SYSUTCDATETIME()
            FROM dbo.Tareas T
            WHERE T.IdUsuario=@IdUsuario AND T.EsPlantilla=1
              AND NOT EXISTS (SELECT 1 FROM dbo.SyncTombstones S
                  WHERE S.IdUsuario=T.IdUsuario AND S.TipoEntidad='Tarea' AND S.SyncId=T.SyncId);
            DELETE dbo.Tareas WHERE IdUsuario=@IdUsuario AND EsPlantilla=1;
        END;

        INSERT dbo.Tareas (IdUsuario,Nombre,FechaInicio,FechaFin,EsPlantilla,Estado,NoBloquearApps,NoBloquearWebs,SyncId)
        VALUES (@IdUsuario,@Nombre,@FechaInicio,@FechaFin,@EsPlantilla,'P',@NoBloquearApps,@NoBloquearWebs,@SyncId);
        SELECT CONVERT(INT,SCOPE_IDENTITY()) AS IdTarea;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

ALTER PROCEDURE dbo.sp_InsertarAplicacion
    @IdUsuario INT, @SyncId UNIQUEIDENTIFIER, @Nombre VARCHAR(100),
    @Identificador VARCHAR(255), @EsCritica BIT=0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    DECLARE @IdAplicacion INT;
    SELECT @IdAplicacion=IdAplicacion FROM dbo.Aplicaciones WITH (UPDLOCK,HOLDLOCK)
    WHERE IdUsuario=@IdUsuario AND SyncId=@SyncId;
    IF @IdAplicacion IS NULL
        SELECT @IdAplicacion=IdAplicacion FROM dbo.Aplicaciones WITH (UPDLOCK,HOLDLOCK)
        WHERE IdUsuario=@IdUsuario AND Identificador=@Identificador;
    IF @IdAplicacion IS NULL
    BEGIN
        INSERT dbo.Aplicaciones(IdUsuario,Nombre,Identificador,EsCritica,SyncId)
        VALUES(@IdUsuario,@Nombre,@Identificador,@EsCritica,@SyncId);
        SET @IdAplicacion=CONVERT(INT,SCOPE_IDENTITY());
    END;
    SELECT @IdAplicacion AS IdAplicacion;
    COMMIT TRANSACTION;
END;
GO

ALTER PROCEDURE dbo.sp_InsertarWeb
    @IdUsuario INT, @SyncId UNIQUEIDENTIFIER, @Nombre VARCHAR(100), @Link VARCHAR(2048)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    DECLARE @IdWeb INT;
    SELECT @IdWeb=IdWeb FROM dbo.Webs WITH (UPDLOCK,HOLDLOCK)
    WHERE IdUsuario=@IdUsuario AND SyncId=@SyncId;
    IF @IdWeb IS NULL
        SELECT @IdWeb=IdWeb FROM dbo.Webs WITH (UPDLOCK,HOLDLOCK)
        WHERE IdUsuario=@IdUsuario
          AND HashLink=CONVERT(VARBINARY(32),HASHBYTES('SHA2_256',@Link));
    IF @IdWeb IS NULL
    BEGIN
        INSERT dbo.Webs(IdUsuario,Nombre,Link,SyncId) VALUES(@IdUsuario,@Nombre,@Link,@SyncId);
        SET @IdWeb=CONVERT(INT,SCOPE_IDENTITY());
    END;
    SELECT @IdWeb AS IdWeb;
    COMMIT TRANSACTION;
END;
GO

ALTER PROCEDURE dbo.sp_InsertarDescanso
    @IdUsuario INT, @SyncId UNIQUEIDENTIFIER, @IdTarea INT,
    @IntervaloDescanso INT, @DuracionDescanso INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS(SELECT 1 FROM dbo.Descansos WHERE IdUsuario=@IdUsuario AND SyncId=@SyncId) RETURN;
    INSERT dbo.Descansos(IdTarea,IdUsuario,IntervaloDescanso,DuracionDescanso,SyncId)
    SELECT @IdTarea,@IdUsuario,@IntervaloDescanso,@DuracionDescanso,@SyncId
    WHERE EXISTS(SELECT 1 FROM dbo.Tareas WHERE IdTarea=@IdTarea AND IdUsuario=@IdUsuario);
    IF @@ROWCOUNT=0 THROW 50011,'La tarea no existe o pertenece a otro usuario.',1;
END;
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
    IF @@ROWCOUNT=0 THROW 50002, 'No se puede editar: tarea inexistente, ajena o activa.', 1;
END;
GO

ALTER PROCEDURE dbo.sp_EliminarTarea @IdTarea INT, @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.SyncTombstones(IdUsuario,TipoEntidad,SyncId,EliminadoUtc)
    SELECT T.IdUsuario,'Tarea',T.SyncId,SYSUTCDATETIME() FROM dbo.Tareas T
    WHERE T.IdTarea=@IdTarea AND T.IdUsuario=@IdUsuario AND T.EsPlantilla=0 AND T.Estado='P'
      AND EXISTS (SELECT 1 FROM dbo.Usuarios WHERE IdUsuario=@IdUsuario AND Eliminado=0)
      AND NOT EXISTS (SELECT 1 FROM dbo.SyncTombstones S WHERE S.IdUsuario=T.IdUsuario AND S.TipoEntidad='Tarea' AND S.SyncId=T.SyncId);
    DELETE dbo.Tareas WHERE IdTarea=@IdTarea AND IdUsuario=@IdUsuario
      AND EsPlantilla=0 AND Estado='P'
      AND EXISTS (SELECT 1 FROM dbo.Usuarios WHERE IdUsuario=@IdUsuario AND Eliminado=0);
    IF @@ROWCOUNT=0 THROW 50003, 'No se puede eliminar: tarea inexistente, ajena o activa.', 1;
END;
GO

ALTER PROCEDURE dbo.sp_CambiarEstadoTarea
    @IdTarea INT, @IdUsuario INT, @Estado CHAR(1)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Tareas SET Estado=@Estado,SyncUpdatedUtc=SYSUTCDATETIME()
    WHERE IdTarea=@IdTarea AND IdUsuario=@IdUsuario AND EsPlantilla=0
      AND EXISTS (SELECT 1 FROM dbo.Usuarios WHERE IdUsuario=@IdUsuario AND Eliminado=0)
      AND ((Estado='P' AND @Estado IN('A','X')) OR (Estado='A' AND @Estado IN('C','X')));
    IF @@ROWCOUNT=0 THROW 50004, 'La transición de estado no es válida para esta tarea.', 1;
END;
GO

ALTER PROCEDURE dbo.sp_ModificarDescanso
    @IdUsuario INT, @IdDescanso INT, @IntervaloDescanso INT, @DuracionDescanso INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Descansos SET IntervaloDescanso=@IntervaloDescanso,
        DuracionDescanso=@DuracionDescanso,SyncUpdatedUtc=SYSUTCDATETIME()
    WHERE IdDescanso=@IdDescanso AND IdUsuario=@IdUsuario;
    IF @@ROWCOUNT=0 THROW 50012, 'El descanso no existe o pertenece a otro usuario.', 1;
END;
GO

ALTER PROCEDURE dbo.sp_EliminarDescanso @IdUsuario INT, @IdDescanso INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.SyncTombstones(IdUsuario,TipoEntidad,SyncId,EliminadoUtc)
    SELECT D.IdUsuario,'Descanso',D.SyncId,SYSUTCDATETIME() FROM dbo.Descansos D
    WHERE D.IdDescanso=@IdDescanso AND D.IdUsuario=@IdUsuario
      AND NOT EXISTS (SELECT 1 FROM dbo.SyncTombstones S WHERE S.IdUsuario=D.IdUsuario AND S.TipoEntidad='Descanso' AND S.SyncId=D.SyncId);
    DELETE dbo.Descansos WHERE IdDescanso=@IdDescanso AND IdUsuario=@IdUsuario;
END;
GO

ALTER PROCEDURE dbo.sp_ModificarConfiguracion
    @IdUsuario INT, @IdConfiguracion INT, @AutoSesion BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Configuraciones SET AutoSesion=@AutoSesion,SyncUpdatedUtc=SYSUTCDATETIME()
    WHERE IdConfiguracion=@IdConfiguracion AND IdUsuario=@IdUsuario;
    IF @@ROWCOUNT=0 THROW 50014, 'La configuración no existe o pertenece a otro usuario.', 1;
END;
GO

ALTER PROCEDURE dbo.sp_ModificarAplicacion
    @IdUsuario INT, @IdAplicacion INT, @Nombre VARCHAR(100), @Identificador VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Aplicaciones SET Nombre=@Nombre,Identificador=@Identificador,
        SyncUpdatedUtc=SYSUTCDATETIME()
    WHERE IdAplicacion=@IdAplicacion AND IdUsuario=@IdUsuario;
    IF @@ROWCOUNT=0 THROW 50015, 'La aplicación no existe o pertenece a otro usuario.', 1;
END;
GO

ALTER PROCEDURE dbo.sp_EliminarAplicacion @IdUsuario INT, @IdAplicacion INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.SyncTombstones(IdUsuario,TipoEntidad,SyncId,EliminadoUtc)
    SELECT A.IdUsuario,'Aplicacion',A.SyncId,SYSUTCDATETIME() FROM dbo.Aplicaciones A
    WHERE A.IdAplicacion=@IdAplicacion AND A.IdUsuario=@IdUsuario
      AND NOT EXISTS (SELECT 1 FROM dbo.SyncTombstones S WHERE S.IdUsuario=A.IdUsuario AND S.TipoEntidad='Aplicacion' AND S.SyncId=A.SyncId);
    DELETE dbo.Aplicaciones WHERE IdAplicacion=@IdAplicacion AND IdUsuario=@IdUsuario;
    IF @@ROWCOUNT=0 THROW 50016, 'La aplicación no existe o pertenece a otro usuario.', 1;
END;
GO

ALTER PROCEDURE dbo.sp_ModificarWeb
    @IdUsuario INT, @IdWeb INT, @Nombre VARCHAR(100), @Link VARCHAR(2048)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Webs SET Nombre=@Nombre,Link=@Link,SyncUpdatedUtc=SYSUTCDATETIME()
    WHERE IdWeb=@IdWeb AND IdUsuario=@IdUsuario;
    IF @@ROWCOUNT=0 THROW 50017, 'La web no existe o pertenece a otro usuario.', 1;
END;
GO

ALTER PROCEDURE dbo.sp_EliminarWeb @IdUsuario INT, @IdWeb INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.SyncTombstones(IdUsuario,TipoEntidad,SyncId,EliminadoUtc)
    SELECT W.IdUsuario,'Web',W.SyncId,SYSUTCDATETIME() FROM dbo.Webs W
    WHERE W.IdWeb=@IdWeb AND W.IdUsuario=@IdUsuario
      AND NOT EXISTS (SELECT 1 FROM dbo.SyncTombstones S WHERE S.IdUsuario=W.IdUsuario AND S.TipoEntidad='Web' AND S.SyncId=W.SyncId);
    DELETE dbo.Webs WHERE IdWeb=@IdWeb AND IdUsuario=@IdUsuario;
    IF @@ROWCOUNT=0 THROW 50018, 'La web no existe o pertenece a otro usuario.', 1;
END;
GO
