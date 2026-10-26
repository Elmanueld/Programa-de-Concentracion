-- Actualización no destructiva para bases instaladas antes del Entregable 3.
-- No ejecutar 01..05 nuevamente en una base existente. Respaldar antes de migrar.
USE ProgramaConcentracion;
GO

ALTER PROCEDURE sp_IniciarSesion
	@Correo VARCHAR(254)

AS
BEGIN
	SELECT IdUsuario, Nombre, Correo, ContrasenaHash
	FROM Usuarios
	WHERE Correo = @Correo AND
		  Eliminado = 0 AND
		  CorreoVerificado = 1;
END;
GO

ALTER PROCEDURE sp_ModificarTarea
	@IdTarea INT,
	@IdUsuario INT,
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
	AND IdUsuario = @IdUsuario AND EsPlantilla = 0 AND Estado <> 'A'
	AND EXISTS (SELECT 1 FROM Usuarios WHERE IdUsuario = @IdUsuario AND Eliminado = 0);
	IF @@ROWCOUNT = 0 THROW 50002, 'No se puede editar: tarea inexistente, ajena o activa.', 1;
END;
GO

ALTER PROCEDURE sp_EliminarTarea
	@IdTarea INT,
	@IdUsuario INT

AS
BEGIN
	DELETE FROM Tareas
	WHERE IdTarea = @IdTarea
	AND IdUsuario = @IdUsuario AND EsPlantilla = 0 AND Estado <> 'A'
	AND EXISTS (SELECT 1 FROM Usuarios WHERE IdUsuario = @IdUsuario AND Eliminado = 0);
	IF @@ROWCOUNT = 0 THROW 50003, 'No se puede eliminar: tarea inexistente, ajena o activa.', 1;
END;
GO

ALTER PROCEDURE sp_CambiarEstadoTarea
	@IdTarea INT,
	@IdUsuario INT,
	@Estado CHAR(1)


AS
BEGIN
	UPDATE Tareas
	SET Estado = @Estado
    WHERE IdTarea = @IdTarea
      AND IdUsuario = @IdUsuario AND EsPlantilla = 0
      AND EXISTS (SELECT 1 FROM Usuarios WHERE IdUsuario = @IdUsuario AND Eliminado = 0)
      AND (
            (Estado = 'P' AND @Estado = 'A')
         OR (Estado = 'A' AND @Estado = 'C')
      );
	IF @@ROWCOUNT = 0 THROW 50004, 'La transición de estado no es válida para esta tarea.', 1;
END;
GO
