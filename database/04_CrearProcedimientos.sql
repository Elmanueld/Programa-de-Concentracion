USE ProgramaConcentracion
GO

-- ===============================
-- PROCEDURES DE LA TABLA USUARIOS
-- ===============================

-- REGISTRA
CREATE PROCEDURE sp_RegistrarUsuario
	@Nombre VARCHAR(50),
	@Correo VARCHAR(254),
	@ContrasenaHash VARCHAR(255)

AS
BEGIN
	INSERT INTO Usuarios(Nombre, Correo, ContrasenaHash)
	VALUES(@Nombre,@Correo, @ContrasenaHash);

END;
GO

-- BUSCA
CREATE PROCEDURE sp_BuscarUsuario
	@Buscar VARCHAR(254)

AS
BEGIN
	SELECT IdUsuario, Nombre, Correo FROM Usuarios
	WHERE (
		  Nombre LIKE '%' + @Buscar + '%' OR 
		  Correo LIKE '%' + @Buscar + '%'
	)
	AND Eliminado = 0;

END;
GO

-- MODIFICA
CREATE PROCEDURE sp_ModificarUsuario
	@IdUsuario INT,
	@Nombre VARCHAR(50),
	@Correo VARCHAR(254)

AS
BEGIN
UPDATE Usuarios
SET Nombre = @Nombre,
    CorreoVerificado = CASE
        WHEN Correo <> @Correo THEN 0
        ELSE CorreoVerificado
    END,
    Correo = @Correo
WHERE IdUsuario = @IdUsuario
  AND Eliminado = 0;
END;
GO

-- ELIMINA
CREATE PROCEDURE sp_EliminarUsuario
	@IdUsuario INT

AS
BEGIN
	UPDATE Usuarios
	SET Eliminado = 1,
		CorreoVerificado = 0
	WHERE IdUsuario = @IdUsuario;
END;
GO

-- INICIA SESIÓN
CREATE PROCEDURE sp_IniciarSesion
	@Correo VARCHAR(254)

AS
BEGIN
	SELECT IdUsuario, Correo, ContrasenaHash 
	FROM Usuarios
	WHERE Correo = @Correo AND
		  Eliminado = 0 AND
		  CorreoVerificado = 1;
END;
GO

-- VERIFICA CORREO
CREATE PROCEDURE sp_VerificarCorreo
	@IdUsuario INT

AS
BEGIN
	UPDATE Usuarios
	SET CorreoVerificado = 1
	WHERE IdUsuario = @IdUsuario
	  AND Eliminado = 0;
END;
GO

-- CAMBIA CONTRASEÑA
CREATE PROCEDURE sp_CambiarContrasena
	@IdUsuario INT,
	@ContrasenaHashNueva VARCHAR(255)

AS
BEGIN
	UPDATE Usuarios
	SET ContrasenaHash = @ContrasenaHashNueva
	WHERE IdUsuario = @IdUsuario;
END;
GO

-- RECUPERA CUENTA
CREATE PROCEDURE sp_RecuperarCuenta
    @IdUsuario INT
AS
BEGIN
    UPDATE Usuarios
    SET Eliminado = 0,
        CorreoVerificado = 0
    WHERE IdUsuario = @IdUsuario
      AND Eliminado = 1;
END;
GO


-- ===============================
-- PROCEDURES DE LA TABLA TAREAS
-- ===============================

-- INSERTA
CREATE PROCEDURE sp_InsertarTarea
    @IdUsuario INT,
    @Nombre VARCHAR(70),
    @FechaInicio DATETIME,
    @FechaFin DATETIME,
    @EsPlantilla BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1
            FROM Usuarios
            WHERE IdUsuario = @IdUsuario
              AND Eliminado = 0
        )
            THROW 50001, 'El usuario no existe o está eliminado.', 1;

        IF @EsPlantilla = 1
        BEGIN
            DELETE FROM Tareas
            WHERE IdUsuario = @IdUsuario
              AND EsPlantilla = 1;
        END;

        INSERT INTO Tareas (
            IdUsuario, Nombre, FechaInicio, FechaFin, EsPlantilla, Estado
        )
        VALUES (
            @IdUsuario, @Nombre, @FechaInicio, @FechaFin, @EsPlantilla, 'P'
        );

        SELECT CONVERT(INT, SCOPE_IDENTITY()) AS IdTarea;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO

-- BUSCA
CREATE PROCEDURE sp_BuscarTarea
	@IdUsuario INT,
	@Buscar VARCHAR(70)

AS
BEGIN
	SELECT IdTarea, Nombre, FechaInicio, FechaFin, Estado
	FROM Tareas
	WHERE Nombre LIKE '%' + @Buscar + '%'
		AND IdUsuario = @IdUsuario
		AND EsPlantilla = 0;
END;
GO

-- MODIFICA
CREATE PROCEDURE sp_ModificarTarea
	@IdTarea INT,
	@Nombre VARCHAR(70),
	@FechaInicio DATETIME,
	@FechaFin DATETIME

AS
BEGIN
	UPDATE Tareas
	SET Nombre = @Nombre,
		FechaInicio = @FechaInicio,
		FechaFin = @FechaFin
	WHERE IdTarea = @IdTarea
	AND Estado = 'P';
END;
GO

-- ELIMINA
CREATE PROCEDURE sp_EliminarTarea
	@IdTarea INT

AS
BEGIN
	DELETE FROM Tareas
	WHERE IdTarea = @IdTarea
	AND Estado <> 'A'
END;
GO

-- CAMBIA ESTADO
CREATE PROCEDURE sp_CambiarEstadoTarea
	@IdTarea INT,
	@Estado CHAR(1)


AS
BEGIN
	UPDATE Tareas
	SET Estado = @Estado
    WHERE IdTarea = @IdTarea
      AND (
            (Estado = 'P' AND @Estado = 'A')
         OR (Estado = 'A' AND @Estado = 'C')
      );
END;
GO

-- BUSCA PLANTILLA
CREATE PROCEDURE sp_BuscarPlantilla
	@IdUsuario INT

AS
BEGIN 
	SELECT IdTarea
	FROM Tareas
	WHERE EsPlantilla = 1
	AND IdUsuario = @IdUsuario;

END;
GO

-- =================================
-- PROCEDURES DE LA TABLA DESCANSOS
-- =================================

-- INSERTA
CREATE PROCEDURE sp_InsertarDescanso
	@IdTarea INT,
	@IntervaloDescanso INT,
	@DuracionDescanso INT

AS
BEGIN
	INSERT INTO Descansos(IdTarea, IntervaloDescanso, DuracionDescanso)
	VALUES(@IdTarea, @IntervaloDescanso, @DuracionDescanso)
END;
GO

-- BUSCA
CREATE PROCEDURE sp_BuscarDescanso
	@IdTarea INT

AS
BEGIN
	SELECT IdDescanso, IdTarea, IntervaloDescanso, DuracionDescanso
	FROM Descansos
	WHERE IdTarea = @IdTarea;
END;
GO

-- MODIFICA
CREATE PROCEDURE sp_ModificarDescanso
	@IdDescanso INT,
	@IntervaloDescanso INT,
	@DuracionDescanso INT

AS
BEGIN
	UPDATE Descansos
	SET IntervaloDescanso = @IntervaloDescanso,
		DuracionDescanso = @DuracionDescanso
	WHERE IdDescanso = @IdDescanso;
END;
GO

-- ELIMINA
CREATE PROCEDURE sp_EliminarDescanso
	@IdDescanso INT

AS
BEGIN
	DELETE Descansos
	WHERE IdDescanso = @IdDescanso;
END;
GO

-- =======================================
-- PROCEDURES DE LA TABLA CONFIGURACIONES
-- =======================================

-- INSERTA
CREATE PROCEDURE sp_InsertarConfiguracion
	@IdUsuario INT,
	@AutoSesion BIT
	
AS
BEGIN
	INSERT INTO Configuraciones(IdUsuario, AutoSesion)
	VALUES(@IdUsuario, @AutoSesion)
END;
GO

-- BUSCA
CREATE PROCEDURE sp_BuscarConfiguracion
	@IdUsuario INT

AS
BEGIN
	SELECT IdConfiguracion, IdUsuario, AutoSesion
	FROM Configuraciones
	WHERE IdUsuario = @IdUsuario;
END;
GO

-- MODIFICA
CREATE PROCEDURE sp_ModificarConfiguracion
	@IdConfiguracion INT,
	@AutoSesion BIT

AS
BEGIN
	UPDATE Configuraciones
	SET AutoSesion = @AutoSesion
	WHERE IdConfiguracion = @IdConfiguracion;
END;
GO

-- ====================================
-- PROCEDURES DE LA TABLA APLICACIONES
-- ====================================

-- INSERTA
CREATE PROCEDURE sp_InsertarAplicacion
	@Nombre VARCHAR(100),
	@Identificador VARCHAR(255),
	@EsCritica BIT = 0

AS
BEGIN
	INSERT INTO Aplicaciones(Nombre, Identificador, EsCritica)
	VALUES(@Nombre, @Identificador, @EsCritica)
END;
GO

-- BUSCA
CREATE PROCEDURE sp_BuscarAplicacion
	@Buscar VARCHAR(255)

AS
BEGIN
	SELECT IdAplicacion, Nombre, Identificador, EsCritica
	FROM Aplicaciones
	WHERE Nombre LIKE '%' + @Buscar + '%'
	OR Identificador LIKE '%' + @Buscar + '%'
END;
GO

-- MODIFICA
CREATE PROCEDURE sp_ModificarAplicacion
	@IdAplicacion INT,
	@Nombre VARCHAR(100),
	@Identificador VARCHAR(255)

AS
BEGIN
	UPDATE Aplicaciones
	SET Nombre = @Nombre,
		Identificador = @Identificador
	WHERE IdAplicacion = @IdAplicacion;
END;
GO

CREATE PROCEDURE sp_EliminarAplicacion
	@IdAplicacion INT
AS
BEGIN
	DELETE FROM Aplicaciones
	WHERE IdAplicacion = @IdAplicacion;
END;
GO

-- ============================
-- PROCEDURES DE LA TABLA WEBS
-- ============================

-- INSERTA
CREATE PROCEDURE sp_InsertarWeb
	@Nombre VARCHAR(100),
	@Link VARCHAR(2048)

AS
BEGIN
	INSERT INTO Webs(Nombre, Link)
	VALUES(@Nombre, @Link);
END;
GO

-- BUSCA
CREATE PROCEDURE sp_BuscarWeb
	@Buscar VARCHAR(2048)

AS
BEGIN
	SELECT IdWeb, Nombre, Link
	FROM Webs
	WHERE Nombre LIKE '%' + @Buscar + '%'
	OR Link LIKE '%' + @Buscar + '%';
END;
GO

-- MODIFICA
CREATE PROCEDURE sp_ModificarWeb
	@IdWeb INT,
	@Nombre VARCHAR(100),
	@Link VARCHAR(2048)

AS
BEGIN
	UPDATE Webs
	SET Nombre = @Nombre,
		Link = @Link
	WHERE IdWeb = @IdWeb;
END;
GO

CREATE PROCEDURE sp_EliminarWeb
	@IdWeb INT
AS
BEGIN
	DELETE FROM Webs
	WHERE IdWeb = @IdWeb;
END;
GO

-- ==========================================
-- PROCEDURES DE LA TABLA TAREA_APLICACIONES
-- ==========================================

-- INSERTA
CREATE PROCEDURE sp_InsertarTareaAplicacion
	@IdTarea INT,
	@IdAplicacion INT

AS
BEGIN
	INSERT INTO Tarea_Aplicacion(IdTarea, IdAplicacion)
	VALUES(@IdTarea, @IdAplicacion)
END;
GO

-- BUSCA
CREATE PROCEDURE sp_BuscarTareaAplicacion
	@IdUsuario INT,
	@IdTarea INT

AS
BEGIN
	SELECT TA.IdTarea, TA.IdAplicacion, A.Nombre Aplicacion, T.Nombre Tarea
	FROM Tarea_Aplicacion TA
	INNER JOIN Tareas T ON TA.IdTarea = T.IdTarea
	INNER JOIN Aplicaciones A ON TA.IdAplicacion = A.IdAplicacion
	WHERE TA.IdTarea = @IdTarea
	AND T.IdUsuario = @IdUsuario;
END;
GO

-- ELIMINA
CREATE PROCEDURE sp_EliminarTareaAplicacion
	@IdTarea INT,
	@IdAplicacion INT

AS
BEGIN
	DELETE Tarea_Aplicacion
	WHERE IdTarea = @IdTarea
	AND IdAplicacion = @IdAplicacion;
END;
GO

-- ==========================================
-- PROCEDURES DE LA TABLA TAREA_WEB
-- ==========================================

-- INSERTA
CREATE PROCEDURE sp_InsertarTareaWeb
	@IdTarea INT,
	@IdWeb INT

AS
BEGIN
	INSERT INTO Tarea_Web(IdTarea, IdWeb)
	VALUES(@IdTarea, @IdWeb);
END;
GO

-- BUSCA
CREATE PROCEDURE sp_BuscarTareaWeb
	@IdUsuario INT,
	@IdTarea INT
AS 
BEGIN
	SELECT TW.IdTarea, TW.IdWeb, T.Nombre Tarea, W.Nombre Web
	FROM Tarea_Web TW
	INNER JOIN Tareas T ON TW.IdTarea = T.IdTarea
	INNER JOIN Webs W ON TW.IdWeb = W.IdWeb
	WHERE TW.IdTarea = @IdTarea
	AND T.IdUsuario = @IdUsuario;
END;
GO

-- ELIMINA
CREATE PROCEDURE sp_EliminarTareaWeb
	@IdTarea INT,
	@IdWeb INT

AS
BEGIN
	DELETE Tarea_Web
	WHERE IdTarea = @IdTarea
	AND IdWeb = @IdWeb;
END;
GO
