-- PROCEDURES DE LA TABLA USUARIOS

-- INSERTA
CREATE PROCEDURE sp_InsertarUsuario
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
	@Correo VARCHAR(254),
	@ContrasenaHash VARCHAR(255)

AS
BEGIN
	UPDATE Usuarios
	SET Nombre = @Nombre,
		Correo = @Correo,
		ContrasenaHash = @ContrasenaHash
	WHERE IdUsuario = @IdUsuario;
END;
GO

-- ELIMINA
CREATE PROCEDURE sp_EliminarUsuario
	@IdUsuario INT

AS
BEGIN
	UPDATE Usuarios
	SET Eliminado = 1
	WHERE IdUsuario = @IdUsuario;
END;
GO
