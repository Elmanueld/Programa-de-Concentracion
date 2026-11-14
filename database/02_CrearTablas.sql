USE ProgramaConcentracion;
GO
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;
GO

CREATE TABLE Usuarios(
	IdUsuario INT IDENTITY(1,1),
	Nombre VARCHAR(50) NOT NULL,
	Correo VARCHAR(254) NOT NULL,
	CorreoVerificado BIT NOT NULL DEFAULT 0,
	ContrasenaHash VARCHAR(255) NOT NULL,
	Eliminado BIT NOT NULL DEFAULT 0,
	SyncId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	SyncUpdatedUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),

	CONSTRAINT PK_Usuarios_IdUsuario PRIMARY KEY(IdUsuario),
	CONSTRAINT UQ_Usuarios_Correo UNIQUE(Correo),
	CONSTRAINT UQ_Usuarios_SyncId UNIQUE(SyncId),
	CONSTRAINT CK_Usuarios_Correo CHECK(Correo LIKE '%@%.%')
);
GO

CREATE TABLE Configuraciones(
	IdConfiguracion INT IDENTITY(1,1),
	IdUsuario INT NOT NULL,
	AutoSesion BIT NOT NULL DEFAULT 0,
	SyncId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	SyncUpdatedUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),

	CONSTRAINT PK_Configuraciones_IdConfiguracion PRIMARY KEY(IdConfiguracion),
	CONSTRAINT FK_Configuraciones_IdUsuario FOREIGN KEY(IdUsuario) REFERENCES Usuarios(IdUsuario),
	CONSTRAINT UQ_Configuraciones_IdUsuario UNIQUE(IdUsuario),
	CONSTRAINT UQ_Configuraciones_IdUsuario_SyncId UNIQUE(IdUsuario, SyncId)
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
	NoBloquearApps BIT NOT NULL DEFAULT 0,
	NoBloquearWebs BIT NOT NULL DEFAULT 0,
	SyncId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	SyncUpdatedUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),

	CONSTRAINT PK_Tareas_IdTarea PRIMARY KEY(IdTarea),
	CONSTRAINT FK_Tareas_IdUsuario FOREIGN KEY(IdUsuario) REFERENCES Usuarios(IdUsuario),
	CONSTRAINT UQ_Tareas_IdTarea_IdUsuario UNIQUE(IdTarea, IdUsuario),
	CONSTRAINT UQ_Tareas_IdUsuario_SyncId UNIQUE(IdUsuario, SyncId),
	CONSTRAINT CK_Tareas_Fechas CHECK(FechaInicio < FechaFin),
	CONSTRAINT CK_Tareas_Estado CHECK(Estado IN('P','A','C','X'))
);
GO

CREATE TABLE Descansos(
	IdDescanso INT IDENTITY(1,1),
	IdTarea INT NOT NULL,
	IdUsuario INT NOT NULL,
	IntervaloDescanso INT,
	DuracionDescanso INT,
	SyncId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	SyncUpdatedUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),

	CONSTRAINT PK_Descansos_IdDescanso PRIMARY KEY(IdDescanso),
	CONSTRAINT UQ_Descansos_IdTarea UNIQUE(IdTarea),
	CONSTRAINT UQ_Descansos_IdUsuario_SyncId UNIQUE(IdUsuario, SyncId),
	CONSTRAINT FK_Descansos_Tarea_Propietario FOREIGN KEY(IdTarea, IdUsuario)
		REFERENCES Tareas(IdTarea, IdUsuario) ON DELETE CASCADE,
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
	IdUsuario INT NOT NULL,
	Nombre VARCHAR(100) NOT NULL,
	Identificador VARCHAR(255) NOT NULL,
	EsCritica BIT NOT NULL DEFAULT 0,
	SyncId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	SyncUpdatedUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),

	CONSTRAINT PK_Aplicaciones_IdAplicacion PRIMARY KEY(IdAplicacion),
	CONSTRAINT FK_Aplicaciones_IdUsuario FOREIGN KEY(IdUsuario) REFERENCES Usuarios(IdUsuario),
	CONSTRAINT UQ_Aplicaciones_Usuario_Identificador UNIQUE(IdUsuario, Identificador),
	CONSTRAINT UQ_Aplicaciones_Id_Usuario UNIQUE(IdAplicacion, IdUsuario),
	CONSTRAINT UQ_Aplicaciones_IdUsuario_SyncId UNIQUE(IdUsuario, SyncId)
);
GO

CREATE TABLE Webs(
	IdWeb INT IDENTITY(1,1),
	IdUsuario INT NOT NULL,
	Nombre VARCHAR(100) NOT NULL,
	Link VARCHAR(2048) NOT NULL,
	SyncId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	SyncUpdatedUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
	HashLink AS CONVERT(VARBINARY(32), HASHBYTES('SHA2_256',Link)) PERSISTED,

	CONSTRAINT PK_Webs_IdWeb PRIMARY KEY(IdWeb),
	CONSTRAINT FK_Webs_IdUsuario FOREIGN KEY(IdUsuario) REFERENCES Usuarios(IdUsuario),
	CONSTRAINT UQ_Webs_Usuario_HashLink UNIQUE(IdUsuario, HashLink),
	CONSTRAINT UQ_Webs_Id_Usuario UNIQUE(IdWeb, IdUsuario),
	CONSTRAINT UQ_Webs_IdUsuario_SyncId UNIQUE(IdUsuario, SyncId)
);

CREATE TABLE Tarea_Aplicacion(
	IdTarea INT NOT NULL,
	IdUsuario INT NOT NULL,
	IdAplicacion INT NOT NULL,
	SyncUpdatedUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
	CONSTRAINT PK_Tarea_Aplicacion PRIMARY KEY(IdTarea, IdAplicacion, IdUsuario),
	CONSTRAINT FK_Tarea_Aplicacion_Tarea FOREIGN KEY(IdTarea, IdUsuario)
		REFERENCES Tareas(IdTarea, IdUsuario) ON DELETE CASCADE,
	CONSTRAINT FK_Tarea_Aplicacion_Aplicacion FOREIGN KEY(IdAplicacion, IdUsuario)
		REFERENCES Aplicaciones(IdAplicacion, IdUsuario) ON DELETE CASCADE
);
GO

CREATE TABLE Tarea_Web(
	IdTarea INT NOT NULL,
	IdUsuario INT NOT NULL,
	IdWeb INT NOT NULL,
	SyncUpdatedUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
	CONSTRAINT PK_Tarea_Web PRIMARY KEY(IdTarea, IdWeb, IdUsuario),
	CONSTRAINT FK_Tarea_Web_Tarea FOREIGN KEY(IdTarea, IdUsuario)
		REFERENCES Tareas(IdTarea, IdUsuario) ON DELETE CASCADE,
	CONSTRAINT FK_Tarea_Web_Web FOREIGN KEY(IdWeb, IdUsuario)
		REFERENCES Webs(IdWeb, IdUsuario) ON DELETE CASCADE
);
GO

CREATE TABLE SyncDispositivos(
	IdUsuario INT NOT NULL,
	IdDispositivo UNIQUEIDENTIFIER NOT NULL,
	NombreDispositivo NVARCHAR(100) NOT NULL,
	UltimaSincronizacionUtc DATETIME2(3) NULL,
	CONSTRAINT PK_SyncDispositivos PRIMARY KEY(IdUsuario, IdDispositivo),
	CONSTRAINT FK_SyncDispositivos_Usuario FOREIGN KEY(IdUsuario)
		REFERENCES Usuarios(IdUsuario) ON DELETE CASCADE
);
GO

CREATE TABLE SyncCambiosAplicados(
	IdUsuario INT NOT NULL,
	IdDispositivo UNIQUEIDENTIFIER NOT NULL,
	IdCambioLocal BIGINT NOT NULL,
	AplicadoUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
	CONSTRAINT PK_SyncCambiosAplicados PRIMARY KEY(IdUsuario, IdDispositivo, IdCambioLocal),
	CONSTRAINT FK_SyncCambios_Usuario FOREIGN KEY(IdUsuario)
		REFERENCES Usuarios(IdUsuario) ON DELETE CASCADE
);
GO

CREATE TABLE SyncTombstones(
	IdUsuario INT NOT NULL,
	TipoEntidad VARCHAR(32) NOT NULL,
	SyncId UNIQUEIDENTIFIER NOT NULL,
	EliminadoUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
	CONSTRAINT PK_SyncTombstones PRIMARY KEY(IdUsuario, TipoEntidad, SyncId),
	CONSTRAINT FK_SyncTombstones_Usuario FOREIGN KEY(IdUsuario)
		REFERENCES Usuarios(IdUsuario) ON DELETE CASCADE
);
GO
