USE ProgramaConcentracion;
GO

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
GO

CREATE TABLE Configuraciones(
	IdConfiguracion INT IDENTITY(1,1),
	IdUsuario INT NOT NULL,
	AutoSesion BIT NOT NULL DEFAULT 0,

	CONSTRAINT PK_Configuraciones_IdConfiguracion PRIMARY KEY(IdConfiguracion),
	CONSTRAINT FK_Configuraciones_IdUsuario FOREIGN KEY(IdUsuario) REFERENCES Usuarios(IdUsuario),
	CONSTRAINT UQ_Configuraciones_IdUsuario UNIQUE(IdUsuario)
);
GO

-- ESTADO:
	-- P = PROGRAMADA
	-- A = ACTIVA
	-- C = COMPLETA

CREATE TABLE Tareas(
	IdTarea INT IDENTITY(1,1),
	IdUsuario INT NOT NULL,
	Nombre VARCHAR(70) NOT NULL,
	FechaInicio DATETIME NOT NULL,
	FechaFin DATETIME NOT NULL,
	Estado CHAR(1) NOT NULL DEFAULT 'P',
	EsPlantilla BIT NOT NULL DEFAULT 0,

	CONSTRAINT PK_Tareas_IdTarea PRIMARY KEY(IdTarea),
	CONSTRAINT FK_Tareas_IdUsuario FOREIGN KEY(IdUsuario) REFERENCES Usuarios(IdUsuario),
	CONSTRAINT CK_Tareas_Fechas CHECK(FechaInicio < FechaFin),
	CONSTRAINT CK_Tareas_Estado CHECK(Estado IN('P','A','C'))
);
GO

CREATE TABLE Descansos(
	IdDescanso INT IDENTITY(1,1),
	IdTarea	INT NOT NULL,
	IntervaloDescanso INT,
	DuracionDescanso INT,

	CONSTRAINT PK_Descansos_IdDescanso PRIMARY KEY(IdDescanso),
	CONSTRAINT UQ_Descansos_IdTarea UNIQUE(IdTarea),
	CONSTRAINT FK_Descansos_IdTarea FOREIGN KEY(IdTarea) REFERENCES Tareas(IdTarea) ON DELETE CASCADE,
	CONSTRAINT CK_Descansos_Intervalo_Duracion CHECK (
		(IntervaloDescanso IS NULL AND DuracionDescanso IS NULL)
		OR
		(
			IntervaloDescanso IS NOT NULL
			AND DuracionDescanso IS NOT NULL
			AND IntervaloDescanso > 0
			AND DuracionDescanso > 0
		)
	)

);
GO

CREATE TABLE Aplicaciones(
	IdAplicacion INT IDENTITY(1,1),
	Nombre VARCHAR(100) NOT NULL,
	Identificador VARCHAR(255) NOT NULL,
	EsCritica BIT NOT NULL DEFAULT 0,

	CONSTRAINT PK_Aplicaciones_IdAplicacion PRIMARY KEY(IdAplicacion),
	CONSTRAINT UQ_Aplicaciones_Identificador UNIQUE(Identificador)
);
GO

CREATE TABLE Webs(
	IdWeb INT IDENTITY(1,1),
	Nombre VARCHAR(100) NOT NULL,
	Link VARCHAR(2048) NOT NULL,
	HashLink AS CONVERT(VARBINARY(32), HASHBYTES('SHA2_256',Link)) PERSISTED,

	CONSTRAINT PK_Webs_IdWeb PRIMARY KEY(IdWeb),
	CONSTRAINT UQ_Webs_HashLink UNIQUE(HashLink)
);