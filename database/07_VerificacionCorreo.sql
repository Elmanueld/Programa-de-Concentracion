-- Agrega códigos de verificación sin borrar los datos existentes.
USE ProgramaConcentracion;
GO
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
CREATE OR ALTER PROCEDURE dbo.sp_PrepararVerificacionCorreo
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
CREATE OR ALTER PROCEDURE dbo.sp_ConsultarSalVerificacion
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
CREATE OR ALTER PROCEDURE dbo.sp_ConfirmarVerificacionCorreo
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
