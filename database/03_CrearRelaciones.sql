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

CREATE TABLE Configuracion_Aplicacion(
	IdConfiguracion INT NOT NULL,
	IdAplicacion INT NOT NULL,

	CONSTRAINT PK_Configuracion_Aplicacion_IdConfiguracion_IdAplicacion PRIMARY KEY(IdConfiguracion,IdAplicacion),
	CONSTRAINT FK_Configuracion_Aplicacion_IdConfiguracion FOREIGN KEY(IdConfiguracion) REFERENCES Configuraciones(IdConfiguracion) ON DELETE CASCADE,
	CONSTRAINT FK_Configuracion_Aplicacion_IdAplicacion FOREIGN KEY(IdAplicacion) REFERENCES Aplicaciones(IdAplicacion)
);
GO

CREATE TABLE Configuracion_Web(
	IdConfiguracion INT NOT NULL,
	IdWeb INT NOT NULL,

	CONSTRAINT PK_Configuracion_Web_IdConfiguracion_IdWeb PRIMARY KEY(IdConfiguracion, IdWeb),
	CONSTRAINT FK_Configuracion_Web_IdConfiguracion FOREIGN KEY(IdConfiguracion) REFERENCES Configuraciones(IdConfiguracion) ON DELETE CASCADE,
	CONSTRAINT FK_Configuracion_Web_IdWeb FOREIGN KEY(IdWeb) REFERENCES Webs(IdWeb)
);