-- SQLite local-first store. The app owns the file under LocalAppData and
-- opens it with foreign_keys=ON. Every record is scoped by IdUsuario.
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS UsuariosLocales (
    IdUsuario INTEGER PRIMARY KEY,
    Correo TEXT NOT NULL COLLATE NOCASE,
    Nombre TEXT NOT NULL,
    SyncUpdatedUtc TEXT NOT NULL,
    SyncDeleted INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS EstadoSincronizacion (
    IdUsuario INTEGER PRIMARY KEY,
    UltimaCopiaCompletaUtc TEXT NULL,
    UltimaSincronizacionUtc TEXT NULL,
    FOREIGN KEY (IdUsuario) REFERENCES UsuariosLocales(IdUsuario) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS MetadatosLocal (
    Clave TEXT PRIMARY KEY,
    Valor TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Configuraciones (
    IdConfiguracion INTEGER PRIMARY KEY,
    IdServidor INTEGER NULL,
    IdUsuario INTEGER NOT NULL UNIQUE,
    AutoSesion INTEGER NOT NULL DEFAULT 0,
    SyncId TEXT NOT NULL,
    SyncUpdatedUtc TEXT NOT NULL,
    SyncDeleted INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (IdUsuario) REFERENCES UsuariosLocales(IdUsuario) ON DELETE CASCADE,
    UNIQUE (IdUsuario, SyncId)
);

CREATE TABLE IF NOT EXISTS Tareas (
    IdTarea INTEGER PRIMARY KEY,
    IdServidor INTEGER NULL,
    IdUsuario INTEGER NOT NULL,
    Nombre TEXT NOT NULL,
    FechaInicio TEXT NOT NULL,
    FechaFin TEXT NOT NULL,
    Estado TEXT NOT NULL DEFAULT 'P' CHECK (Estado IN ('P','A','C','X')),
    EsPlantilla INTEGER NOT NULL DEFAULT 0,
    NoBloquearApps INTEGER NOT NULL DEFAULT 0,
    NoBloquearWebs INTEGER NOT NULL DEFAULT 0,
    SyncId TEXT NOT NULL,
    SyncUpdatedUtc TEXT NOT NULL,
    SyncDeleted INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (IdUsuario) REFERENCES UsuariosLocales(IdUsuario) ON DELETE CASCADE,
    UNIQUE (IdUsuario, SyncId),
    UNIQUE (IdTarea, IdUsuario),
    CHECK (FechaInicio < FechaFin)
);

CREATE TABLE IF NOT EXISTS Descansos (
    IdDescanso INTEGER PRIMARY KEY,
    IdServidor INTEGER NULL,
    IdTarea INTEGER NOT NULL,
    IdUsuario INTEGER NOT NULL,
    IntervaloDescanso INTEGER,
    DuracionDescanso INTEGER,
    SyncId TEXT NOT NULL,
    SyncUpdatedUtc TEXT NOT NULL,
    SyncDeleted INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (IdTarea, IdUsuario) REFERENCES Tareas(IdTarea, IdUsuario) ON DELETE CASCADE,
    UNIQUE (IdUsuario, SyncId),
    UNIQUE (IdTarea, IdUsuario),
    CHECK ((IntervaloDescanso IS NULL AND DuracionDescanso IS NULL) OR
           (IntervaloDescanso > 0 AND DuracionDescanso > 0))
);

CREATE TABLE IF NOT EXISTS Aplicaciones (
    IdAplicacion INTEGER PRIMARY KEY,
    IdServidor INTEGER NULL,
    IdUsuario INTEGER NOT NULL,
    Nombre TEXT NOT NULL,
    Identificador TEXT NOT NULL,
    EsCritica INTEGER NOT NULL DEFAULT 0,
    SyncId TEXT NOT NULL,
    SyncUpdatedUtc TEXT NOT NULL,
    SyncDeleted INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (IdUsuario) REFERENCES UsuariosLocales(IdUsuario) ON DELETE CASCADE,
    UNIQUE (IdUsuario, SyncId),
    UNIQUE (IdAplicacion, IdUsuario),
    UNIQUE (IdUsuario, Identificador)
);

CREATE TABLE IF NOT EXISTS Webs (
    IdWeb INTEGER PRIMARY KEY,
    IdServidor INTEGER NULL,
    IdUsuario INTEGER NOT NULL,
    Nombre TEXT NOT NULL,
    Link TEXT NOT NULL,
    SyncId TEXT NOT NULL,
    SyncUpdatedUtc TEXT NOT NULL,
    SyncDeleted INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (IdUsuario) REFERENCES UsuariosLocales(IdUsuario) ON DELETE CASCADE,
    UNIQUE (IdUsuario, SyncId),
    UNIQUE (IdWeb, IdUsuario),
    UNIQUE (IdUsuario, Link)
);

CREATE TABLE IF NOT EXISTS Tarea_Aplicacion (
    IdTarea INTEGER NOT NULL,
    IdUsuario INTEGER NOT NULL,
    IdAplicacion INTEGER NOT NULL,
    SyncUpdatedUtc TEXT NOT NULL,
    SyncDeleted INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (IdTarea, IdAplicacion, IdUsuario),
    FOREIGN KEY (IdTarea, IdUsuario) REFERENCES Tareas(IdTarea, IdUsuario) ON DELETE CASCADE,
    FOREIGN KEY (IdAplicacion, IdUsuario) REFERENCES Aplicaciones(IdAplicacion, IdUsuario) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Tarea_Web (
    IdTarea INTEGER NOT NULL,
    IdUsuario INTEGER NOT NULL,
    IdWeb INTEGER NOT NULL,
    SyncUpdatedUtc TEXT NOT NULL,
    SyncDeleted INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (IdTarea, IdWeb, IdUsuario),
    FOREIGN KEY (IdTarea, IdUsuario) REFERENCES Tareas(IdTarea, IdUsuario) ON DELETE CASCADE,
    FOREIGN KEY (IdWeb, IdUsuario) REFERENCES Webs(IdWeb, IdUsuario) ON DELETE CASCADE
);

-- Changes are applied locally first and acknowledged only after the server
-- accepts them. SyncId makes retries idempotent; IdUsuario scopes every item.
CREATE TABLE IF NOT EXISTS SyncOutbox (
    IdCambio INTEGER PRIMARY KEY AUTOINCREMENT,
    IdUsuario INTEGER NOT NULL,
    TipoEntidad TEXT NOT NULL,
    SyncId TEXT NOT NULL,
    Operacion TEXT NOT NULL CHECK (Operacion IN ('I','U','D')),
    Payload TEXT NOT NULL,
    CreadoUtc TEXT NOT NULL,
    Intentos INTEGER NOT NULL DEFAULT 0,
    UltimoError TEXT,
    FOREIGN KEY (IdUsuario) REFERENCES UsuariosLocales(IdUsuario) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS IX_SyncOutbox_Pendiente ON SyncOutbox(IdUsuario, IdCambio);
CREATE INDEX IF NOT EXISTS IX_Tareas_Usuario_Fecha ON Tareas(IdUsuario, FechaInicio, FechaFin);
CREATE INDEX IF NOT EXISTS IX_Aplicaciones_Usuario_Nombre ON Aplicaciones(IdUsuario, Nombre);
CREATE INDEX IF NOT EXISTS IX_Webs_Usuario_Nombre ON Webs(IdUsuario, Nombre);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Tareas_IdServidor ON Tareas(IdUsuario, IdServidor) WHERE IdServidor IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS IX_Aplicaciones_IdServidor ON Aplicaciones(IdUsuario, IdServidor) WHERE IdServidor IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS IX_Webs_IdServidor ON Webs(IdUsuario, IdServidor) WHERE IdServidor IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS IX_Descansos_IdServidor ON Descansos(IdUsuario, IdServidor) WHERE IdServidor IS NOT NULL;
