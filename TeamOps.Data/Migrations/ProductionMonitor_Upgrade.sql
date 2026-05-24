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
WHERE trim(COALESCE(MachineCode, '')) <> ''
  AND (
      MachineCode <> upper(trim(COALESCE(MachineCode, '')))
      OR LineCode <> upper(trim(COALESCE(LineCode, '')))
      OR COALESCE(MachineKey, '') <> upper(trim(COALESCE(LineCode, ''))) || ':' || upper(trim(COALESCE(MachineCode, '')))
  );

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
    SectorId INTEGER,
    StatusCode INTEGER NOT NULL,
    DisplayCode INTEGER NOT NULL,
    Classification TEXT NOT NULL DEFAULT 'StopCounts',
    NamePt TEXT NOT NULL,
    NameJp TEXT NOT NULL,
    ColorHex TEXT NOT NULL DEFAULT '#5B88E8',
    TextColorHex TEXT NOT NULL DEFAULT '#FFFFFF',
    SortOrder INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (SectorId) REFERENCES Sectors(Id)
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_MachineEvents_UniqueEvent
ON MachineEvents(MachineId, EventDateTime, StatusCode);

CREATE UNIQUE INDEX IF NOT EXISTS IX_MachineEvents_UniqueRawEvent
ON MachineEvents(MachineId, EventDateTime, InternalState);

CREATE INDEX IF NOT EXISTS IX_MachineEvents_EventDateTime
ON MachineEvents(EventDateTime);

CREATE INDEX IF NOT EXISTS IX_MachineEvents_Machine_EventTime
ON MachineEvents(MachineId, EventDateTime);

DROP INDEX IF EXISTS IX_MachineEvents_Sector_EventTime;
DROP INDEX IF EXISTS IX_MachineEvents_StatusCode_EventTime;
DROP INDEX IF EXISTS IX_MachineEvents_Sector_Status_EventTime;

CREATE INDEX IF NOT EXISTS IX_Machines_MachineCode
ON Machines(MachineCode);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Machines_MachineKey_Unique
ON Machines(MachineKey);

CREATE INDEX IF NOT EXISTS IX_Machines_MachineCode_LineCode
ON Machines(MachineCode, LineCode);

CREATE INDEX IF NOT EXISTS IX_Machines_LocalId
ON Machines(LocalId);

CREATE INDEX IF NOT EXISTS IX_Machines_SectorId
ON Machines(SectorId);

CREATE UNIQUE INDEX IF NOT EXISTS IX_MachineStatuses_Sector_StatusCode
ON MachineStatuses(COALESCE(SectorId, 0), StatusCode);

CREATE INDEX IF NOT EXISTS IX_MachineStatuses_SectorId
ON MachineStatuses(SectorId);

CREATE INDEX IF NOT EXISTS IX_MachineStatuses_StatusCode
ON MachineStatuses(StatusCode);

INSERT OR IGNORE INTO MachineStatuses
(SectorId, StatusCode, DisplayCode, Classification, NamePt, NameJp, ColorHex, TextColorHex, SortOrder, IsActive)
VALUES
(NULL, 0, 0, 'Running', 'Rodando', 'Running', '#5B88E8', '#FFFFFF', 0, 1),
(NULL, 1, 1, 'StopCounts', 'Inativo', 'Inactive', '#EF6F63', '#FFFFFF', 1, 1),
(NULL, 3, 3, 'StopCounts', 'Parado', 'Stop', '#F2CB58', '#4A3200', 3, 1),
(NULL, 4, 4, 'Error', 'Erro', 'Error', '#FFFFFF', '#516174', 4, 1),
(2, 0, 0, 'Running', 'Rodando', '運転', '#5B88E8', '#FFFFFF', 0, 1),
(2, 1, 3, 'StopCounts', 'Parado DAD', '停止中', '#F2CB58', '#4A3200', 1, 1),
(2, 3, 3, 'StopNoCount', 'Limpeza programada', '清掃', '#8EC5A8', '#123524', 3, 1),
(2, 4, 4, 'Error', 'Erro', '異常', '#FFFFFF', '#516174', 4, 1),
(2, 17, 1, 'StopNoCount', 'Intervalo', 'レス処理', '#8EC5A8', '#123524', 17, 1),
(2, 18, 1, 'StopNoCount', 'Limpeza programada', '吸引時間', '#8EC5A8', '#123524', 18, 1),
(2, 19, 1, 'StopNoCount', 'Amostra', 'サンプル', '#8EC5A8', '#123524', 19, 1);
