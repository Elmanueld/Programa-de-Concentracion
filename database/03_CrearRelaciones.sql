USE ProgramaConcentracion;
GO

CREATE TABLE Tarea_Aplicacion(
	IdTarea INT NOT NULL,
	IdAplicacion INT NOT NULL,

	CONSTRAINT PK_Tarea_Aplicacion_IdTarea_IdAplicacion PRIMARY KEY(IdTarea,IdAplicacion),
	CONSTRAINT FK_Tarea_Aplicacion_IdTarea FOREIGN KEY(IdTarea) REFERENCES Tareas(IdTarea) ON DELETE CASCADE,
	CONSTRAINT FK_Tarea_Aplicacion_IdAplicacion FOREIGN KEY(IdAplicacion) REFERENCES Aplicaciones(IdAplicacion)
);
GO

CREATE TABLE Tarea_Web(
	IdTarea INT NOT NULL,
	IdWeb INT NOT NULL,

	CONSTRAINT PK_Tarea_Web_IdTarea_IdWeb PRIMARY KEY(IdTarea, IdWeb),
	CONSTRAINT FK_Tarea_Web_IdTarea FOREIGN KEY(IdTarea) REFERENCES Tareas(IdTarea) ON DELETE CASCADE,
	CONSTRAINT FK_Tarea_Web_IdWeb FOREIGN KEY(IdWeb) REFERENCES Webs(IdWeb)
);
GO