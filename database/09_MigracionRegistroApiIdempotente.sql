SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
ALTER PROCEDURE dbo.sp_RegistrarUsuario
    @Nombre VARCHAR(50),
    @Correo VARCHAR(254),
    @ContrasenaHash VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdUsuario INT;
    DECLARE @HashExistente VARCHAR(255);
    DECLARE @Eliminado BIT;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT
            @IdUsuario = IdUsuario,
            @HashExistente = ContrasenaHash,
            @Eliminado = Eliminado
        FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK)
        WHERE Correo = @Correo;

        IF @IdUsuario IS NULL
        BEGIN
            INSERT dbo.Usuarios (Nombre, Correo, ContrasenaHash)
            VALUES (@Nombre, @Correo, @ContrasenaHash);

            SET @IdUsuario = CONVERT(INT, SCOPE_IDENTITY());
        END
        ELSE IF @Eliminado = 1
             OR CONVERT(VARBINARY(255), @HashExistente) <> CONVERT(VARBINARY(255), @ContrasenaHash)
        BEGIN
            THROW 50019, 'No se pudo completar el registro.', 1;
        END;

        COMMIT TRANSACTION;
        SELECT @IdUsuario AS IdUsuario;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO