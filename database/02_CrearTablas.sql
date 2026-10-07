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

CREATE TABLE Descansos(
	IdDescanso INT IDENTITY(1,1),
	IdTarea	INT NOT NULL,
	IntervaloDescanso INT,
	DuracionDescanso INT,

	CONSTRAINT PK_Descansos_IdDescanso PRIMARY KEY(IdDescanso),
	CONSTRAINT FK_Descansos_IdTarea FOREIGN KEY(IdTarea) REFERENCES Tareas(IdTarea),
	CONSTRAINT CK_Descansos_Intervalo_Duracion CHECK(
		(IntervaloDescanso IS NULL AND DuracionDescanso IS NULL)
		OR
		(IntervaloDescanso > 0 AND DuracionDescanso > 0))

);

CREATE TABLE Aplicaciones(
	IdAplicacion INT IDENTITY(1,1),
	Nombre VARCHAR(100) NOT NULL,
	Identificador VARCHAR(255) NOT NULL,
	Prioridad CHAR(1) NOT NULL,

	CONSTRAINT PK_Aplicaciones_IdAplicacion PRIMARY KEY(IdAplicacion),
	CONSTRAINT UQ_Aplicaciones_Identificador UNIQUE(Identificador),
	CONSTRAINT CK_Aplicaciones_Prioridad CHECK(Prioridad IN('P','D','T'))
);

CREATE TABLE Webs(
	IdWeb INT IDENTITY(1,1),
	Nombre VARCHAR(100) NOT NULL,
	Link VARCHAR(2048) NOT NULL,
	Prioridad CHAR(1) NOT NULL,
	HashLink AS CONVERT(VARBINARY(32), HASHBYTES('SHA2_256',Link)) PERSISTED,

	CONSTRAINT PK_Webs_IdWeb PRIMARY KEY(IdWeb),
	CONSTRAINT UQ_Webs_HashLink UNIQUE(HashLink),
	CONSTRAINT CK_Webs_Prioridad CHECK(Prioridad IN('D','T'))
);