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
	@TareaNombre VARCHAR(70),
	@FechaInicio DATETIME,
	@FechaFin DATETIME

AS
BEGIN
	IF EXISTS(
		SELECT 1
		FROM Usuarios
		WHERE IdUsuario = @IdUsuario
		AND Eliminado = 0
	)
	BEGIN
	INSERT INTO Tareas(IdUsuario,TareaNombre, FechaInicio, FechaFin)
	VALUES(@IdUsuario, @TareaNombre, @FechaInicio, @FechaFin);
	END;
END;
GO

-- BUSCA
CREATE PROCEDURE sp_BuscarTarea
	@IdUsuario INT,
	@Buscar VARCHAR(70)

AS
BEGIN
	SELECT TareaNombre, FechaInicio, FechaFin 
	FROM Tareas
	WHERE TareaNombre LIKE '%' + @Buscar + '%'
		AND IdUsuario = @IdUsuario;
END;
GO

-- MODIFICA
CREATE PROCEDURE sp_ModificarTarea
	@IdTarea INT,
	@TareaNombre VARCHAR(70),
	@FechaInicio DATETIME,
	@FechaFin DATETIME

AS
BEGIN
	UPDATE Tareas
	SET TareaNombre = @TareaNombre,
		FechaInicio = @FechaInicio,
		FechaFin = @FechaFin
	WHERE IdTarea = @IdTarea
END;
GO

-- ELIMINA
CREATE PROCEDURE sp_EliminarTarea
	@IdTarea INT

AS
BEGIN
	DELETE FROM Tareas
	WHERE IdTarea = @IdTarea;
END;
GO
