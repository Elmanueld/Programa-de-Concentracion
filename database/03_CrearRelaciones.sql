USE ProgramaConcentracion;
GO

CREATE TABLE Tarea_Aplicacion(
	IdTarea INT NOT NULL,
	IdUsuario INT NOT NULL,
	IdAplicacion INT NOT NULL,

	CONSTRAINT PK_Tarea_Aplicacion_IdTarea_IdAplicacion PRIMARY KEY(IdTarea,IdAplicacion),
	CONSTRAINT FK_Tarea_Aplicacion_Tarea_Propietario FOREIGN KEY(IdTarea, IdUsuario)
		REFERENCES Tareas(IdTarea, IdUsuario) ON DELETE CASCADE,
	CONSTRAINT FK_Tarea_Aplicacion_App_Propietario FOREIGN KEY(IdAplicacion, IdUsuario)
		REFERENCES Aplicaciones(IdAplicacion, IdUsuario)
);
GO

CREATE TABLE Tarea_Web(
	IdTarea INT NOT NULL,
	IdUsuario INT NOT NULL,
	IdWeb INT NOT NULL,

	CONSTRAINT PK_Tarea_Web_IdTarea_IdWeb PRIMARY KEY(IdTarea, IdWeb),
	CONSTRAINT FK_Tarea_Web_Tarea_Propietario FOREIGN KEY(IdTarea, IdUsuario)
		REFERENCES Tareas(IdTarea, IdUsuario) ON DELETE CASCADE,
	CONSTRAINT FK_Tarea_Web_Web_Propietario FOREIGN KEY(IdWeb, IdUsuario)
		REFERENCES Webs(IdWeb, IdUsuario)
);
GO
