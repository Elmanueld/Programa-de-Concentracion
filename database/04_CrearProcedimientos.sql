USE ProgramaConcentracion;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ===============================
   CUENTAS
   =============================== */
CREATE PROCEDURE dbo.sp_RegistrarUsuario
    @Nombre VARCHAR(50),
    @Correo VARCHAR(254),
    @ContrasenaHash VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Usuarios (Nombre, Correo, ContrasenaHash)
    VALUES (@Nombre, @Correo, @ContrasenaHash);
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* No existe un procedimiento de búsqueda general de usuarios. */
CREATE PROCEDURE dbo.sp_ModificarUsuario
    @IdUsuario INT,
    @Nombre VARCHAR(50),
    @Correo VARCHAR(254)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Usuarios
    SET Nombre=@Nombre,
        CorreoVerificado=CASE WHEN Correo<>@Correo THEN 0 ELSE CorreoVerificado END,
        Correo=@Correo
    WHERE IdUsuario=@IdUsuario AND Eliminado=0;
    IF @@ROWCOUNT=0 THROW 50010, 'La cuenta no existe o está eliminada.', 1;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_EliminarUsuario @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Usuarios SET Eliminado=1, CorreoVerificado=0
    WHERE IdUsuario=@IdUsuario AND Eliminado=0;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_IniciarSesion @Correo VARCHAR(254)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdUsuario, Nombre, Correo, ContrasenaHash
    FROM dbo.Usuarios
    WHERE Correo=@Correo AND Eliminado=0 AND CorreoVerificado=1;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_VerificarCorreo @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Usuarios SET CorreoVerificado=1
    WHERE IdUsuario=@IdUsuario AND Eliminado=0;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_CambiarContrasena
    @IdUsuario INT,
    @ContrasenaHashNueva VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Usuarios SET ContrasenaHash=@ContrasenaHashNueva
    WHERE IdUsuario=@IdUsuario AND Eliminado=0;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_RecuperarCuenta @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Usuarios SET Eliminado=0, CorreoVerificado=0
    WHERE IdUsuario=@IdUsuario AND Eliminado=1;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ===============================
   TAREAS
   =============================== */
CREATE PROCEDURE dbo.sp_InsertarTarea
    @IdUsuario INT,
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

        IF @EsPlantilla=1
            DELETE dbo.Tareas WHERE IdUsuario=@IdUsuario AND EsPlantilla=1;

        INSERT dbo.Tareas (
            IdUsuario, Nombre, FechaInicio, FechaFin, EsPlantilla, Estado,
            NoBloquearApps, NoBloquearWebs
        )
        VALUES (
            @IdUsuario, @Nombre, @FechaInicio, @FechaFin, @EsPlantilla, 'P',
            @NoBloquearApps, @NoBloquearWebs
        );
        SELECT CONVERT(INT, SCOPE_IDENTITY()) AS IdTarea;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_BuscarTarea
    @IdUsuario INT,
    @Buscar VARCHAR(70)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdTarea, Nombre, FechaInicio, FechaFin, Estado,
        NoBloquearApps, NoBloquearWebs
    FROM dbo.Tareas
    WHERE IdUsuario=@IdUsuario AND EsPlantilla=0
      AND Nombre LIKE '%' + @Buscar + '%';
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_ModificarTarea
    @IdTarea INT, @IdUsuario INT, @Nombre VARCHAR(70),
    @FechaInicio DATETIME, @FechaFin DATETIME,
    @NoBloquearApps BIT, @NoBloquearWebs BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Tareas
    SET Nombre=@Nombre, FechaInicio=@FechaInicio, FechaFin=@FechaFin,
        NoBloquearApps=@NoBloquearApps, NoBloquearWebs=@NoBloquearWebs
    WHERE IdTarea=@IdTarea AND IdUsuario=@IdUsuario
      AND EsPlantilla=0 AND Estado<>'A'
      AND EXISTS (SELECT 1 FROM dbo.Usuarios WHERE IdUsuario=@IdUsuario AND Eliminado=0);
    IF @@ROWCOUNT=0 THROW 50002, 'No se puede editar: tarea inexistente, ajena o activa.', 1;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_EliminarTarea @IdTarea INT, @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE dbo.Tareas
    WHERE IdTarea=@IdTarea AND IdUsuario=@IdUsuario
      AND EsPlantilla=0 AND Estado<>'A'
      AND EXISTS (SELECT 1 FROM dbo.Usuarios WHERE IdUsuario=@IdUsuario AND Eliminado=0);
    IF @@ROWCOUNT=0 THROW 50003, 'No se puede eliminar: tarea inexistente, ajena o activa.', 1;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_CambiarEstadoTarea
    @IdTarea INT, @IdUsuario INT, @Estado CHAR(1)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Tareas SET Estado=@Estado
    WHERE IdTarea=@IdTarea AND IdUsuario=@IdUsuario AND EsPlantilla=0
      AND EXISTS (SELECT 1 FROM dbo.Usuarios WHERE IdUsuario=@IdUsuario AND Eliminado=0)
      AND ((Estado='P' AND @Estado='A') OR (Estado='A' AND @Estado='C'));
    IF @@ROWCOUNT=0 THROW 50004, 'La transición de estado no es válida para esta tarea.', 1;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_BuscarPlantilla @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdTarea FROM dbo.Tareas WHERE EsPlantilla=1 AND IdUsuario=@IdUsuario;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ===============================
   DESCANSOS
   =============================== */
CREATE PROCEDURE dbo.sp_InsertarDescanso
    @IdUsuario INT, @IdTarea INT,
    @IntervaloDescanso INT, @DuracionDescanso INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Descansos (IdTarea, IdUsuario, IntervaloDescanso, DuracionDescanso)
    SELECT @IdTarea, @IdUsuario, @IntervaloDescanso, @DuracionDescanso
    WHERE EXISTS (SELECT 1 FROM dbo.Tareas WHERE IdTarea=@IdTarea AND IdUsuario=@IdUsuario);
    IF @@ROWCOUNT=0 THROW 50011, 'La tarea no existe o pertenece a otro usuario.', 1;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_BuscarDescanso @IdUsuario INT, @IdTarea INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdDescanso, IdTarea, IntervaloDescanso, DuracionDescanso
    FROM dbo.Descansos WHERE IdTarea=@IdTarea AND IdUsuario=@IdUsuario;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_ModificarDescanso
    @IdUsuario INT, @IdDescanso INT,
    @IntervaloDescanso INT, @DuracionDescanso INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Descansos SET IntervaloDescanso=@IntervaloDescanso,
        DuracionDescanso=@DuracionDescanso
    WHERE IdDescanso=@IdDescanso AND IdUsuario=@IdUsuario;
    IF @@ROWCOUNT=0 THROW 50012, 'El descanso no existe o pertenece a otro usuario.', 1;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_EliminarDescanso @IdUsuario INT, @IdDescanso INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE dbo.Descansos WHERE IdDescanso=@IdDescanso AND IdUsuario=@IdUsuario;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ===============================
   CONFIGURACIONES
   =============================== */
CREATE PROCEDURE dbo.sp_InsertarConfiguracion @IdUsuario INT, @AutoSesion BIT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Configuraciones (IdUsuario, AutoSesion)
    SELECT @IdUsuario, @AutoSesion
    WHERE EXISTS (SELECT 1 FROM dbo.Usuarios WHERE IdUsuario=@IdUsuario AND Eliminado=0);
    IF @@ROWCOUNT=0 THROW 50013, 'El usuario no existe o está eliminado.', 1;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_BuscarConfiguracion @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdConfiguracion, IdUsuario, AutoSesion
    FROM dbo.Configuraciones WHERE IdUsuario=@IdUsuario;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_ModificarConfiguracion
    @IdUsuario INT, @IdConfiguracion INT, @AutoSesion BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Configuraciones SET AutoSesion=@AutoSesion
    WHERE IdConfiguracion=@IdConfiguracion AND IdUsuario=@IdUsuario;
    IF @@ROWCOUNT=0 THROW 50014, 'La configuración no existe o pertenece a otro usuario.', 1;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ===============================
   CATÁLOGOS PRIVADOS
   =============================== */
CREATE PROCEDURE dbo.sp_InsertarAplicacion
    @IdUsuario INT, @Nombre VARCHAR(100),
    @Identificador VARCHAR(255), @EsCritica BIT=0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    DECLARE @IdAplicacion INT;
    SELECT @IdAplicacion=IdAplicacion
    FROM dbo.Aplicaciones WITH (UPDLOCK, HOLDLOCK)
    WHERE IdUsuario=@IdUsuario AND Identificador=@Identificador;
    IF @IdAplicacion IS NULL
    BEGIN
        INSERT dbo.Aplicaciones (IdUsuario, Nombre, Identificador, EsCritica)
        VALUES (@IdUsuario, @Nombre, @Identificador, @EsCritica);
        SET @IdAplicacion=CONVERT(INT, SCOPE_IDENTITY());
    END;
    SELECT @IdAplicacion AS IdAplicacion;
    COMMIT TRANSACTION;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_BuscarAplicacion @IdUsuario INT, @Buscar VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdAplicacion, Nombre, Identificador, EsCritica
    FROM dbo.Aplicaciones
    WHERE IdUsuario=@IdUsuario
      AND (Nombre LIKE '%' + @Buscar + '%' OR Identificador LIKE '%' + @Buscar + '%');
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_ModificarAplicacion
    @IdUsuario INT, @IdAplicacion INT,
    @Nombre VARCHAR(100), @Identificador VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Aplicaciones SET Nombre=@Nombre, Identificador=@Identificador
    WHERE IdAplicacion=@IdAplicacion AND IdUsuario=@IdUsuario;
    IF @@ROWCOUNT=0 THROW 50015, 'La aplicación no existe o pertenece a otro usuario.', 1;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_EliminarAplicacion @IdUsuario INT, @IdAplicacion INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE dbo.Aplicaciones WHERE IdAplicacion=@IdAplicacion AND IdUsuario=@IdUsuario;
    IF @@ROWCOUNT=0 THROW 50016, 'La aplicación no existe o pertenece a otro usuario.', 1;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_InsertarWeb @IdUsuario INT, @Nombre VARCHAR(100), @Link VARCHAR(2048)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    DECLARE @IdWeb INT;
    SELECT @IdWeb=IdWeb FROM dbo.Webs WITH (UPDLOCK, HOLDLOCK)
    WHERE IdUsuario=@IdUsuario
      AND HashLink=CONVERT(VARBINARY(32), HASHBYTES('SHA2_256', @Link));
    IF @IdWeb IS NULL
    BEGIN
        INSERT dbo.Webs (IdUsuario, Nombre, Link) VALUES (@IdUsuario, @Nombre, @Link);
        SET @IdWeb=CONVERT(INT, SCOPE_IDENTITY());
    END;
    SELECT @IdWeb AS IdWeb;
    COMMIT TRANSACTION;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_BuscarWeb @IdUsuario INT, @Buscar VARCHAR(2048)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdWeb, Nombre, Link FROM dbo.Webs
    WHERE IdUsuario=@IdUsuario
      AND (Nombre LIKE '%' + @Buscar + '%' OR Link LIKE '%' + @Buscar + '%');
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_ModificarWeb
    @IdUsuario INT, @IdWeb INT, @Nombre VARCHAR(100), @Link VARCHAR(2048)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Webs SET Nombre=@Nombre, Link=@Link
    WHERE IdWeb=@IdWeb AND IdUsuario=@IdUsuario;
    IF @@ROWCOUNT=0 THROW 50017, 'La web no existe o pertenece a otro usuario.', 1;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_EliminarWeb @IdUsuario INT, @IdWeb INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE dbo.Webs WHERE IdWeb=@IdWeb AND IdUsuario=@IdUsuario;
    IF @@ROWCOUNT=0 THROW 50018, 'La web no existe o pertenece a otro usuario.', 1;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ===============================
   RELACIONES PRIVADAS
   =============================== */
CREATE PROCEDURE dbo.sp_InsertarTareaAplicacion
    @IdUsuario INT, @IdTarea INT, @IdAplicacion INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Tarea_Aplicacion (IdTarea, IdUsuario, IdAplicacion)
    SELECT @IdTarea, @IdUsuario, @IdAplicacion
    WHERE EXISTS (SELECT 1 FROM dbo.Tareas WHERE IdTarea=@IdTarea AND IdUsuario=@IdUsuario)
      AND EXISTS (SELECT 1 FROM dbo.Aplicaciones WHERE IdAplicacion=@IdAplicacion AND IdUsuario=@IdUsuario);
    IF @@ROWCOUNT=0 THROW 50019, 'La tarea o aplicación no existe o pertenece a otro usuario.', 1;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_BuscarTareaAplicacion @IdUsuario INT, @IdTarea INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TA.IdTarea, TA.IdAplicacion, A.Nombre AS Aplicacion, T.Nombre AS Tarea
    FROM dbo.Tarea_Aplicacion TA
    INNER JOIN dbo.Tareas T ON T.IdTarea=TA.IdTarea AND T.IdUsuario=TA.IdUsuario
    INNER JOIN dbo.Aplicaciones A ON A.IdAplicacion=TA.IdAplicacion AND A.IdUsuario=TA.IdUsuario
    WHERE TA.IdUsuario=@IdUsuario AND TA.IdTarea=@IdTarea;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_EliminarTareaAplicacion
    @IdUsuario INT, @IdTarea INT, @IdAplicacion INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE dbo.Tarea_Aplicacion
    WHERE IdUsuario=@IdUsuario AND IdTarea=@IdTarea AND IdAplicacion=@IdAplicacion;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_InsertarTareaWeb @IdUsuario INT, @IdTarea INT, @IdWeb INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Tarea_Web (IdTarea, IdUsuario, IdWeb)
    SELECT @IdTarea, @IdUsuario, @IdWeb
    WHERE EXISTS (SELECT 1 FROM dbo.Tareas WHERE IdTarea=@IdTarea AND IdUsuario=@IdUsuario)
      AND EXISTS (SELECT 1 FROM dbo.Webs WHERE IdWeb=@IdWeb AND IdUsuario=@IdUsuario);
    IF @@ROWCOUNT=0 THROW 50020, 'La tarea o web no existe o pertenece a otro usuario.', 1;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_BuscarTareaWeb @IdUsuario INT, @IdTarea INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TW.IdTarea, TW.IdWeb, T.Nombre AS Tarea, W.Nombre AS Web
    FROM dbo.Tarea_Web TW
    INNER JOIN dbo.Tareas T ON T.IdTarea=TW.IdTarea AND T.IdUsuario=TW.IdUsuario
    INNER JOIN dbo.Webs W ON W.IdWeb=TW.IdWeb AND W.IdUsuario=TW.IdUsuario
    WHERE TW.IdUsuario=@IdUsuario AND TW.IdTarea=@IdTarea;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.sp_EliminarTareaWeb @IdUsuario INT, @IdTarea INT, @IdWeb INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE dbo.Tarea_Web
    WHERE IdUsuario=@IdUsuario AND IdTarea=@IdTarea AND IdWeb=@IdWeb;
END;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
GO

/* Verificación de correo */
IF OBJECT_ID('dbo.CodigosVerificacion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CodigosVerificacion (
        IdUsuario INT NOT NULL PRIMARY KEY,
        Correo VARCHAR(254) NOT NULL,
        Sal VARCHAR(24) NOT NULL,
        CodigoHash CHAR(64) NOT NULL,
        ExpiraUtc DATETIME2 NOT NULL,
        UltimoEnvioUtc DATETIME2 NOT NULL,
        Intentos TINYINT NOT NULL DEFAULT 0,
        CONSTRAINT FK_CodigosVerificacion_Usuario FOREIGN KEY (IdUsuario)
            REFERENCES dbo.Usuarios(IdUsuario) ON DELETE CASCADE
    );
END;
GO
CREATE PROCEDURE dbo.sp_PrepararVerificacionCorreo
    @Correo VARCHAR(254), @Sal VARCHAR(24), @CodigoHash CHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    DECLARE @Id INT;
    SELECT @Id = IdUsuario FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK)
    WHERE Correo = @Correo AND Eliminado = 0 AND CorreoVerificado = 0;
    IF @Id IS NULL
    BEGIN
        COMMIT;
        SELECT CAST(0 AS BIT);
        RETURN;
    END;
    IF EXISTS (SELECT 1 FROM dbo.CodigosVerificacion WITH (UPDLOCK, HOLDLOCK)
               WHERE IdUsuario = @Id AND UltimoEnvioUtc > DATEADD(SECOND, -60, SYSUTCDATETIME()))
    BEGIN
        ROLLBACK;
        THROW 50006, 'Espere un minuto antes de solicitar otro código.', 1;
    END;
    DELETE FROM dbo.CodigosVerificacion WHERE IdUsuario = @Id;
    INSERT INTO dbo.CodigosVerificacion
        (IdUsuario, Correo, Sal, CodigoHash, ExpiraUtc, UltimoEnvioUtc)
    VALUES (@Id, @Correo, @Sal, @CodigoHash,
            DATEADD(MINUTE, 10, SYSUTCDATETIME()), SYSUTCDATETIME());
    COMMIT;
    SELECT CAST(1 AS BIT);
END;
GO
CREATE PROCEDURE dbo.sp_ConsultarSalVerificacion
    @Correo VARCHAR(254)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.Sal FROM dbo.CodigosVerificacion c
    INNER JOIN dbo.Usuarios u ON u.IdUsuario = c.IdUsuario AND u.Correo = c.Correo
    WHERE u.Correo = @Correo AND u.Eliminado = 0 AND u.CorreoVerificado = 0
      AND c.ExpiraUtc > SYSUTCDATETIME() AND c.Intentos < 5;
END;
GO
CREATE PROCEDURE dbo.sp_ConfirmarVerificacionCorreo
    @Correo VARCHAR(254), @CodigoHash CHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    DECLARE @Id INT, @Hash CHAR(64), @Expira DATETIME2, @Intentos TINYINT;
    SELECT @Id = u.IdUsuario FROM dbo.Usuarios u WITH (UPDLOCK, HOLDLOCK)
    WHERE u.Correo = @Correo AND u.Eliminado = 0 AND u.CorreoVerificado = 0;
    SELECT @Hash = CodigoHash, @Expira = ExpiraUtc, @Intentos = Intentos
    FROM dbo.CodigosVerificacion WITH (UPDLOCK, HOLDLOCK)
    WHERE IdUsuario = @Id AND Correo = @Correo;
    IF @Hash IS NULL OR @Expira <= SYSUTCDATETIME() OR @Intentos >= 5
    BEGIN
        COMMIT;
        SELECT CAST(0 AS BIT);
        RETURN;
    END;
    IF @Hash <> @CodigoHash
    BEGIN
        UPDATE dbo.CodigosVerificacion SET Intentos = Intentos + 1 WHERE IdUsuario = @Id;
        COMMIT;
        SELECT CAST(0 AS BIT);
        RETURN;
    END;
    UPDATE dbo.Usuarios SET CorreoVerificado = 1 WHERE IdUsuario = @Id;
    DELETE FROM dbo.CodigosVerificacion WHERE IdUsuario = @Id;
    COMMIT;
    SELECT CAST(1 AS BIT);
END;
GO

