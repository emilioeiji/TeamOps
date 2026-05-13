-- Upgrade para producao do modulo Producao / Machine Monitor

ALTER TABLE Machines ADD COLUMN MachineCode TEXT;
ALTER TABLE Machines ADD COLUMN MachineKey TEXT;
ALTER TABLE Machines ADD COLUMN LineCode TEXT;
ALTER TABLE Machines ADD COLUMN LocalId INTEGER;
ALTER TABLE Machines ADD COLUMN SectorId INTEGER;
ALTER TABLE Machines ADD COLUMN IsActive INTEGER NOT NULL DEFAULT 1;

UPDATE Machines
SET
    MachineCode = upper(trim(COALESCE(MachineCode, ''))),
    LineCode = upper(trim(COALESCE(LineCode, ''))),
    MachineKey = CASE
        WHEN trim(COALESCE(MachineCode, '')) = '' THEN NULL
        ELSE upper(trim(COALESCE(LineCode, ''))) || ':' || upper(trim(COALESCE(MachineCode, '')))
    END
WHERE trim(COALESCE(MachineCode, '')) <> '';

DROP INDEX IF EXISTS IX_MachineEvents_UniqueEvent;
DROP INDEX IF EXISTS IX_MachineEvents_UniqueRawEvent;
DROP INDEX IF EXISTS IX_MachineCurrentStatus_MachineCode;
DROP INDEX IF EXISTS IX_Machines_MachineKey_Unique;

CREATE TABLE IF NOT EXISTS MachineEvents (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MachineId INTEGER NOT NULL,
    MachineCode TEXT NOT NULL,
    LineCode TEXT NOT NULL,
    LocalId INTEGER,
    SectorId INTEGER,
    RecipeName TEXT,
    LotNo TEXT,
    StatusCode INTEGER NOT NULL,
    StatusText TEXT NOT NULL,
    InternalState TEXT NOT NULL,
    EventDateTime TEXT NOT NULL,
    SourceFile TEXT NOT NULL,
    ImportedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (MachineId) REFERENCES Machines(Id),
    FOREIGN KEY (LocalId) REFERENCES Locals(Id),
    FOREIGN KEY (SectorId) REFERENCES Sectors(Id)
);

CREATE TABLE IF NOT EXISTS MachineCurrentStatus (
    MachineId INTEGER PRIMARY KEY,
    MachineCode TEXT NOT NULL,
    LineCode TEXT NOT NULL,
    LocalId INTEGER,
    SectorId INTEGER,
    RecipeName TEXT,
    LotNo TEXT,
    StatusCode INTEGER NOT NULL,
    StatusText TEXT NOT NULL,
    InternalState TEXT NOT NULL,
    EventDateTime TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    FOREIGN KEY (MachineId) REFERENCES Machines(Id),
    FOREIGN KEY (LocalId) REFERENCES Locals(Id),
    FOREIGN KEY (SectorId) REFERENCES Sectors(Id)
);

CREATE TABLE IF NOT EXISTS MachineStatuses (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    StatusCode INTEGER NOT NULL UNIQUE,
    DisplayCode INTEGER NOT NULL,
    NamePt TEXT NOT NULL,
    NameJp TEXT NOT NULL,
    ColorHex TEXT NOT NULL DEFAULT '#5B88E8',
    TextColorHex TEXT NOT NULL DEFAULT '#FFFFFF',
    SortOrder INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_MachineEvents_UniqueEvent
ON MachineEvents(MachineId, EventDateTime, StatusCode);

CREATE UNIQUE INDEX IF NOT EXISTS IX_MachineEvents_UniqueRawEvent
ON MachineEvents(MachineId, EventDateTime, InternalState);

CREATE INDEX IF NOT EXISTS IX_MachineEvents_EventDateTime
ON MachineEvents(EventDateTime);

CREATE INDEX IF NOT EXISTS IX_MachineEvents_Machine_EventTime
ON MachineEvents(MachineId, EventDateTime);

CREATE INDEX IF NOT EXISTS IX_Machines_MachineCode
ON Machines(MachineCode);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Machines_MachineKey_Unique
ON Machines(MachineKey);

CREATE INDEX IF NOT EXISTS IX_Machines_LocalId
ON Machines(LocalId);

CREATE INDEX IF NOT EXISTS IX_Machines_SectorId
ON Machines(SectorId);

CREATE UNIQUE INDEX IF NOT EXISTS IX_MachineStatuses_StatusCode
ON MachineStatuses(StatusCode);

INSERT OR IGNORE INTO MachineStatuses
(StatusCode, DisplayCode, NamePt, NameJp, ColorHex, TextColorHex, SortOrder, IsActive)
VALUES
(0, 0, 'Rodando', '稼働中', '#5B88E8', '#FFFFFF', 0, 1),
(1, 1, 'Inativo', '非稼働', '#EF6F63', '#FFFFFF', 1, 1),
(3, 3, 'Parado', '停止', '#F2CB58', '#4A3200', 3, 1),
(4, 4, 'Erro', 'エラー', '#FFFFFF', '#516174', 4, 1);
