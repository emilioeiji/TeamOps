-- EC2 Administrator snapshots for Production Monitor.
-- Idempotent and safe for existing SQLite databases.

CREATE TABLE IF NOT EXISTS Ec2AdministratorImports (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SourceFile TEXT NOT NULL,
    FileLastWriteTime TEXT,
    FileLength INTEGER NOT NULL DEFAULT 0,
    FileHash TEXT NOT NULL,
    ImportedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    RowsRead INTEGER NOT NULL DEFAULT 0,
    RowsImported INTEGER NOT NULL DEFAULT 0,
    RowsIgnored INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS Ec2MachineSnapshots (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ImportId INTEGER NOT NULL,
    MachineId INTEGER,
    SectorId INTEGER,
    LocalId INTEGER,
    AreaLabel TEXT,
    MachineCode TEXT NOT NULL,
    MachineName TEXT,
    StatusText TEXT,
    IsRunning INTEGER NOT NULL DEFAULT 0,
    IsIgnored INTEGER NOT NULL DEFAULT 0,
    IgnoreReason TEXT,
    PartCode TEXT,
    PlannedProcessMinutes REAL,
    CapabilityType TEXT,
    OperationRate REAL,
    CurrentDifference REAL,
    LotNo TEXT,
    PlannedEndAt TEXT,
    ProcessMinutes REAL,
    DailyProduction REAL,
    RawColumnsJson TEXT,
    SnapshotAt TEXT NOT NULL,
    ImportedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ImportId) REFERENCES Ec2AdministratorImports(Id),
    FOREIGN KEY (MachineId) REFERENCES Machines(Id),
    FOREIGN KEY (SectorId) REFERENCES Sectors(Id),
    FOREIGN KEY (LocalId) REFERENCES Locals(Id)
);

CREATE TABLE IF NOT EXISTS Ec2MachineCurrentState (
    MachineCode TEXT PRIMARY KEY,
    MachineId INTEGER,
    SectorId INTEGER,
    LocalId INTEGER,
    AreaLabel TEXT,
    MachineName TEXT,
    StatusText TEXT,
    IsRunning INTEGER NOT NULL DEFAULT 0,
    IsIgnored INTEGER NOT NULL DEFAULT 0,
    IgnoreReason TEXT,
    PartCode TEXT,
    PlannedProcessMinutes REAL,
    CapabilityType TEXT,
    OperationRate REAL,
    CurrentDifference REAL,
    LotNo TEXT,
    PlannedEndAt TEXT,
    ProcessMinutes REAL,
    DailyProduction REAL,
    RawColumnsJson TEXT,
    SnapshotAt TEXT NOT NULL,
    ImportedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastSeenAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    StoppedSinceAt TEXT,
    FOREIGN KEY (MachineId) REFERENCES Machines(Id),
    FOREIGN KEY (SectorId) REFERENCES Sectors(Id),
    FOREIGN KEY (LocalId) REFERENCES Locals(Id)
);

CREATE TABLE IF NOT EXISTS ProductionPartCodeStyles (
    PartCode TEXT PRIMARY KEY,
    ColorHex TEXT NOT NULL DEFAULT '#D93F3F',
    TextColorHex TEXT NOT NULL DEFAULT '#FFFFFF',
    Description TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Ec2AdministratorImports_FileHash
ON Ec2AdministratorImports(FileHash);

CREATE INDEX IF NOT EXISTS IX_Ec2MachineSnapshots_ImportId
ON Ec2MachineSnapshots(ImportId);

CREATE INDEX IF NOT EXISTS IX_Ec2MachineSnapshots_Machine
ON Ec2MachineSnapshots(MachineCode, SnapshotAt);

CREATE INDEX IF NOT EXISTS IX_Ec2MachineSnapshots_Area
ON Ec2MachineSnapshots(AreaLabel);

CREATE INDEX IF NOT EXISTS IX_Ec2MachineSnapshots_Status
ON Ec2MachineSnapshots(StatusText);

CREATE INDEX IF NOT EXISTS IX_Ec2MachineSnapshots_PartCode
ON Ec2MachineSnapshots(PartCode);

CREATE INDEX IF NOT EXISTS IX_Ec2MachineSnapshots_LotNo
ON Ec2MachineSnapshots(LotNo);

CREATE INDEX IF NOT EXISTS IX_Ec2MachineCurrentState_MachineId
ON Ec2MachineCurrentState(MachineId);

CREATE INDEX IF NOT EXISTS IX_Ec2MachineCurrentState_Area
ON Ec2MachineCurrentState(AreaLabel);

CREATE INDEX IF NOT EXISTS IX_Ec2MachineCurrentState_Status
ON Ec2MachineCurrentState(StatusText);

CREATE INDEX IF NOT EXISTS IX_Ec2MachineCurrentState_PartCode
ON Ec2MachineCurrentState(PartCode);

INSERT OR IGNORE INTO ProductionPartCodeStyles
(PartCode, ColorHex, TextColorHex, Description, IsActive)
VALUES
('RJ2A7', '#D93F3F', '#FFFFFF', 'Destaque EC2 Administrator', 1);
