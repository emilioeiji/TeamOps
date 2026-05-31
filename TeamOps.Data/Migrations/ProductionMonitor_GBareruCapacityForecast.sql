CREATE TABLE IF NOT EXISTS ProductionProcedureTimes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SectorId INTEGER NOT NULL,
    LocalId INTEGER,
    ProcedureCode TEXT NOT NULL,
    StandardMinutes REAL NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (SectorId) REFERENCES Sectors(Id),
    FOREIGN KEY (LocalId) REFERENCES Locals(Id)
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_ProductionProcedureTimes_Unique
ON ProductionProcedureTimes(SectorId, COALESCE(LocalId, 0), ProcedureCode);
