CREATE TABLE Usuarios(
	IdUsuario INT IDENTITY(1,1),
	Nombre VARCHAR(50) NOT NULL,
	Correo VARCHAR(254) NOT NULL,
	CorreoVerificado BIT NOT NULL DEFAULT 0,
	ContrasenaHash VARCHAR(255) NOT NULL,
	Eliminado BIT NOT NULL DEFAULT 0,

	CONSTRAINT PK_Usuarios_IdUsuario PRIMARY KEY(IdUsuario),
	CONSTRAINT UQ_Usuarios_Correo UNIQUE(Correo),
	CONSTRAINT CK_Usuarios_Correo CHECK(Correo LIKE '%@%.%')
);

CREATE TABLE Configuraciones(
	IdConfiguracion INT IDENTITY(1,1),
	IdUsuario INT NOT NULL,
	AutoSesion BIT NOT NULL DEFAULT 0,

	CONSTRAINT PK_Configuraciones_IdConfiguracion PRIMARY KEY(IdConfiguracion),
	CONSTRAINT FK_Configuraciones_IdUsuario FOREIGN KEY(IdUsuario) REFERENCES Usuarios(IdUsuario),
	CONSTRAINT UQ_Configuraciones_IdUsuario UNIQUE(IdUsuario)
);

CREATE TABLE Tareas(
	IdTarea INT IDENTITY(1,1),
	IdUsuario INT NOT NULL,
	TareaNombre VARCHAR(70) NOT NULL,
	FechaInicio DATETIME NOT NULL,
	FechaFin DATETIME NOT NULL,

	CONSTRAINT PK_Tareas_IdTarea PRIMARY KEY(IdTarea),
	CONSTRAINT FK_Tareas_IdUsuario FOREIGN KEY(IdUsuario) REFERENCES Usuarios(IdUsuario),
	CONSTRAINT CK_Tareas_Fechas CHECK(FechaInicio < FechaFin)
);