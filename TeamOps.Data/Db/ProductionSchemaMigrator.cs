using System.Data;
using Dapper;

namespace TeamOps.Data.Db
{
    public static class ProductionSchemaMigrator
    {
        private static readonly object SyncRoot = new();
        private static bool _ensured;

        public static void Ensure(IDbConnection conn)
        {
            if (_ensured)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (_ensured)
                {
                    return;
                }

                EnsureMachineColumns(conn);

                conn.Execute(
                    @"
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

                    CREATE TABLE IF NOT EXISTS ProductionPlanSnapshots (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SourceFile TEXT NOT NULL,
                        ExportedAt TEXT,
                        LastUpdatedAt TEXT,
                        ImportedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS ProductionPlanRows (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SnapshotId INTEGER NOT NULL,
                        AreaLabel TEXT NOT NULL,
                        MachineCode TEXT NOT NULL,
                        AssignmentText TEXT,
                        PlannedProcessMinutes REAL,
                        MachineStatusText TEXT,
                        CapabilityFrame TEXT,
                        WorkType TEXT,
                        TargetKadouritsu REAL,
                        CurrentDifference REAL,
                        LotNo TEXT,
                        CycleEndAt TEXT,
                        DailyPlannedQuantity REAL,
                        DailyEstimatedQuantity REAL,
                        EstimatedKadouritsu REAL,
                        RawColumnsJson TEXT,
                        ImportedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (SnapshotId) REFERENCES ProductionPlanSnapshots(Id)
                    );

                    UPDATE Machines
                    SET
                        MachineCode = upper(trim(COALESCE(MachineCode, ''))),
                        LineCode = upper(trim(COALESCE(LineCode, ''))),
                        MachineKey = CASE
                            WHEN trim(COALESCE(MachineCode, '')) = '' THEN NULL
                            ELSE upper(trim(COALESCE(LineCode, ''))) || ':' || upper(trim(COALESCE(MachineCode, '')))
                        END
                    WHERE trim(COALESCE(MachineCode, '')) <> '';

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

                    CREATE UNIQUE INDEX IF NOT EXISTS IX_ProductionPlanSnapshots_SourceFile_ExportedAt
                    ON ProductionPlanSnapshots(SourceFile, ExportedAt);

                    CREATE UNIQUE INDEX IF NOT EXISTS IX_ProductionPlanRows_Snapshot_Area_Machine
                    ON ProductionPlanRows(SnapshotId, AreaLabel, MachineCode);

                    CREATE INDEX IF NOT EXISTS IX_ProductionPlanRows_MachineCode
                    ON ProductionPlanRows(MachineCode);"
            );

                SeedMachineStatuses(conn);
                NormalizeProductionStatuses(conn);
                _ensured = true;
            }
        }

        private static void EnsureMachineColumns(IDbConnection conn)
        {
            EnsureColumn(conn, "Machines", "MachineCode", "TEXT");
            EnsureColumn(conn, "Machines", "MachineKey", "TEXT");
            EnsureColumn(conn, "Machines", "LineCode", "TEXT");
            EnsureColumn(conn, "Machines", "LocalId", "INTEGER");
            EnsureColumn(conn, "Machines", "SectorId", "INTEGER");
            EnsureColumn(conn, "Machines", "IsActive", "INTEGER NOT NULL DEFAULT 1");
        }

        private static void EnsureColumn(IDbConnection conn, string tableName, string columnName, string definition)
        {
            var exists = conn.ExecuteScalar<int>(
                $@"
                    SELECT COUNT(1)
                    FROM pragma_table_info('{tableName}')
                    WHERE name = @columnName;",
                new
                {
                    columnName
                }
            ) > 0;

            if (!exists)
            {
                conn.Execute($"ALTER TABLE {tableName} ADD COLUMN {columnName} {definition};");
            }
        }

        private static void NormalizeProductionStatuses(IDbConnection conn)
        {
            conn.Execute(
                @"
                    UPDATE MachineEvents
                    SET StatusCode = CAST(trim(InternalState) AS INTEGER)
                    WHERE trim(COALESCE(InternalState, '')) GLOB '[0-9]*';

                    UPDATE MachineCurrentStatus
                    SET StatusCode = CAST(trim(InternalState) AS INTEGER)
                    WHERE trim(COALESCE(InternalState, '')) GLOB '[0-9]*';

                    UPDATE MachineStatuses
                    SET
                        DisplayCode = 0,
                        NamePt = CASE WHEN trim(COALESCE(NamePt, '')) = '' THEN 'Rodando' ELSE NamePt END,
                        NameJp = CASE WHEN trim(COALESCE(NameJp, '')) = '' THEN '稼働中' ELSE NameJp END,
                        ColorHex = CASE WHEN trim(COALESCE(ColorHex, '')) = '' THEN '#5B88E8' ELSE ColorHex END,
                        TextColorHex = CASE WHEN trim(COALESCE(TextColorHex, '')) = '' THEN '#FFFFFF' ELSE TextColorHex END
                    WHERE StatusCode = 0;

                    UPDATE MachineStatuses
                    SET
                        DisplayCode = 1,
                        NamePt = CASE WHEN trim(COALESCE(NamePt, '')) = '' THEN 'Inativo' ELSE NamePt END,
                        NameJp = CASE WHEN trim(COALESCE(NameJp, '')) = '' THEN '非稼働' ELSE NameJp END,
                        ColorHex = CASE WHEN trim(COALESCE(ColorHex, '')) = '' THEN '#EF6F63' ELSE ColorHex END,
                        TextColorHex = CASE WHEN trim(COALESCE(TextColorHex, '')) = '' THEN '#FFFFFF' ELSE TextColorHex END
                    WHERE StatusCode = 1;

                    UPDATE MachineStatuses
                    SET
                        DisplayCode = 3,
                        NamePt = CASE WHEN trim(COALESCE(NamePt, '')) = '' THEN 'Parado' ELSE NamePt END,
                        NameJp = CASE WHEN trim(COALESCE(NameJp, '')) = '' THEN '停止' ELSE NameJp END,
                        ColorHex = CASE WHEN trim(COALESCE(ColorHex, '')) = '' THEN '#F2CB58' ELSE ColorHex END,
                        TextColorHex = CASE WHEN trim(COALESCE(TextColorHex, '')) = '' THEN '#4A3200' ELSE TextColorHex END
                    WHERE StatusCode = 3;

                    UPDATE MachineStatuses
                    SET
                        DisplayCode = 4,
                        NamePt = CASE WHEN trim(COALESCE(NamePt, '')) = '' THEN 'Erro' ELSE NamePt END,
                        NameJp = CASE WHEN trim(COALESCE(NameJp, '')) = '' THEN 'エラー' ELSE NameJp END,
                        ColorHex = CASE WHEN trim(COALESCE(ColorHex, '')) = '' THEN '#FFFFFF' ELSE ColorHex END,
                        TextColorHex = CASE WHEN trim(COALESCE(TextColorHex, '')) = '' THEN '#516174' ELSE TextColorHex END
                    WHERE StatusCode = 4;"
            );
        }

        private static void SeedMachineStatuses(IDbConnection conn)
        {
            conn.Execute(
                @"
                    INSERT OR IGNORE INTO MachineStatuses
                    (StatusCode, DisplayCode, NamePt, NameJp, ColorHex, TextColorHex, SortOrder, IsActive)
                    VALUES
                    (0, 0, 'Rodando', '稼働中', '#5B88E8', '#FFFFFF', 0, 1),
                    (1, 1, 'Inativo', '非稼働', '#EF6F63', '#FFFFFF', 1, 1),
                    (3, 3, 'Parado', '停止', '#F2CB58', '#4A3200', 3, 1),
                    (4, 4, 'Erro', 'エラー', '#FFFFFF', '#516174', 4, 1);"
            );
        }
    }
}
