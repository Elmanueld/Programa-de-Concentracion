SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.CodigosRecuperacionContrasena', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CodigosRecuperacionContrasena (
        IdUsuario INT NOT NULL PRIMARY KEY,
        Correo VARCHAR(254) NOT NULL,
        Sal VARCHAR(24) NOT NULL,
        CodigoHash CHAR(64) NOT NULL,
        ExpiraUtc DATETIME2 NOT NULL,
        UltimoEnvioUtc DATETIME2 NOT NULL,
        Intentos TINYINT NOT NULL DEFAULT 0,
        CONSTRAINT FK_CodigosRecuperacionContrasena_Usuario FOREIGN KEY (IdUsuario)
            REFERENCES dbo.Usuarios(IdUsuario) ON DELETE CASCADE
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_PrepararRecuperacionContrasena
    @Correo VARCHAR(254), @Sal VARCHAR(24), @CodigoHash CHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    DECLARE @IdUsuario INT;
    SELECT @IdUsuario = IdUsuario
    FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK)
    WHERE Correo = @Correo AND Eliminado = 0 AND CorreoVerificado = 1;

    IF @IdUsuario IS NULL
    BEGIN
        COMMIT;
        SELECT CAST(0 AS BIT);
        RETURN;
    END;

    IF EXISTS (
        SELECT 1 FROM dbo.CodigosRecuperacionContrasena WITH (UPDLOCK, HOLDLOCK)
        WHERE IdUsuario = @IdUsuario AND UltimoEnvioUtc > DATEADD(SECOND, -60, SYSUTCDATETIME()))
    BEGIN
        COMMIT;
        SELECT CAST(0 AS BIT);
        RETURN;
    END;

    DELETE dbo.CodigosRecuperacionContrasena WHERE IdUsuario = @IdUsuario;
    INSERT dbo.CodigosRecuperacionContrasena
        (IdUsuario, Correo, Sal, CodigoHash, ExpiraUtc, UltimoEnvioUtc)
    VALUES
        (@IdUsuario, @Correo, @Sal, @CodigoHash,
         DATEADD(MINUTE, 10, SYSUTCDATETIME()), SYSUTCDATETIME());

    COMMIT;
    SELECT CAST(1 AS BIT);
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ConsultarSalRecuperacionContrasena
    @Correo VARCHAR(254)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT C.Sal
    FROM dbo.CodigosRecuperacionContrasena C
    INNER JOIN dbo.Usuarios U ON U.IdUsuario = C.IdUsuario AND U.Correo = C.Correo
    WHERE U.Correo = @Correo AND U.Eliminado = 0 AND U.CorreoVerificado = 1
      AND C.ExpiraUtc > SYSUTCDATETIME() AND C.Intentos < 5;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ConfirmarRecuperacionContrasena
    @Correo VARCHAR(254),
    @CodigoHash CHAR(64),
    @ContrasenaHashNueva VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    DECLARE @IdUsuario INT, @Hash CHAR(64), @Expira DATETIME2, @Intentos TINYINT;
    SELECT @IdUsuario = IdUsuario
    FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK)
    WHERE Correo = @Correo AND Eliminado = 0 AND CorreoVerificado = 1;

    IF @IdUsuario IS NULL
    BEGIN
        COMMIT;
        SELECT CAST(0 AS BIT);
        RETURN;
    END;

    SELECT @Hash = CodigoHash, @Expira = ExpiraUtc, @Intentos = Intentos
    FROM dbo.CodigosRecuperacionContrasena WITH (UPDLOCK, HOLDLOCK)
    WHERE IdUsuario = @IdUsuario AND Correo = @Correo;

    IF @Hash IS NULL OR @Expira <= SYSUTCDATETIME() OR @Intentos >= 5
    BEGIN
        COMMIT;
        SELECT CAST(0 AS BIT);
        RETURN;
    END;

    IF @Hash <> @CodigoHash
    BEGIN
        UPDATE dbo.CodigosRecuperacionContrasena
        SET Intentos = Intentos + 1
        WHERE IdUsuario = @IdUsuario AND Correo = @Correo;
        COMMIT;
        SELECT CAST(0 AS BIT);
        RETURN;
    END;

    UPDATE dbo.Usuarios
    SET ContrasenaHash = @ContrasenaHashNueva
    WHERE IdUsuario = @IdUsuario AND Correo = @Correo
      AND Eliminado = 0 AND CorreoVerificado = 1;

    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK;
        SELECT CAST(0 AS BIT);
        RETURN;
    END;

    DELETE dbo.CodigosRecuperacionContrasena
    WHERE IdUsuario = @IdUsuario AND Correo = @Correo;

    COMMIT;
    SELECT CAST(1 AS BIT);
END;
GO