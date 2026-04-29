using System.Data;
using Dapper;

namespace TeamOps.Data.Db
{
    public static class ProductionSchemaMigrator
    {
        public static void Ensure(IDbConnection conn)
        {
            EnsureMachineColumns(conn);

            conn.Execute(
                @"
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

                    CREATE UNIQUE INDEX IF NOT EXISTS IX_MachineEvents_UniqueEvent
                    ON MachineEvents(MachineCode, EventDateTime, StatusCode);

                    CREATE INDEX IF NOT EXISTS IX_MachineEvents_EventDateTime
                    ON MachineEvents(EventDateTime);

                    CREATE INDEX IF NOT EXISTS IX_MachineEvents_Machine_EventTime
                    ON MachineEvents(MachineId, EventDateTime);

                    CREATE UNIQUE INDEX IF NOT EXISTS IX_MachineCurrentStatus_MachineCode
                    ON MachineCurrentStatus(MachineCode);

                    CREATE INDEX IF NOT EXISTS IX_Machines_MachineCode
                    ON Machines(MachineCode);

                    CREATE INDEX IF NOT EXISTS IX_Machines_LocalId
                    ON Machines(LocalId);

                    CREATE INDEX IF NOT EXISTS IX_Machines_SectorId
                    ON Machines(SectorId);

                    CREATE UNIQUE INDEX IF NOT EXISTS IX_ProductionPlanSnapshots_SourceFile_ExportedAt
                    ON ProductionPlanSnapshots(SourceFile, ExportedAt);

                    CREATE UNIQUE INDEX IF NOT EXISTS IX_ProductionPlanRows_Snapshot_Area_Machine
                    ON ProductionPlanRows(SnapshotId, AreaLabel, MachineCode);

                    CREATE INDEX IF NOT EXISTS IX_ProductionPlanRows_MachineCode
                    ON ProductionPlanRows(MachineCode);"
            );

            NormalizeProductionStatuses(conn);
        }

        private static void EnsureMachineColumns(IDbConnection conn)
        {
            EnsureColumn(conn, "Machines", "MachineCode", "TEXT");
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
                    SET StatusCode =
                        CASE
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%稼動中%' THEN 0
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%運転%' THEN 0
                            WHEN upper(trim(COALESCE(StatusText, ''))) LIKE '%RUN%' THEN 0
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%停止中%' THEN 1
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%停止%' THEN 1
                            WHEN upper(trim(COALESCE(StatusText, ''))) LIKE '%STOP%' THEN 1
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%トラブル%' THEN 3
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%異常%' THEN 3
                            WHEN upper(trim(COALESCE(StatusText, ''))) LIKE '%ERROR%' THEN 3
                            WHEN upper(trim(COALESCE(StatusText, ''))) LIKE '%ALARM%' THEN 3
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%サンプル%' THEN 2
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%レス処理%' THEN 2
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%吸引時間%' THEN 2
                            WHEN trim(COALESCE(InternalState, '')) = '0' THEN 0
                            WHEN trim(COALESCE(InternalState, '')) = '1' THEN 1
                            WHEN trim(COALESCE(InternalState, '')) = '2' THEN 2
                            WHEN trim(COALESCE(InternalState, '')) = '3' THEN 3
                            ELSE StatusCode
                        END;

                    UPDATE MachineCurrentStatus
                    SET StatusCode =
                        CASE
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%稼動中%' THEN 0
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%運転%' THEN 0
                            WHEN upper(trim(COALESCE(StatusText, ''))) LIKE '%RUN%' THEN 0
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%停止中%' THEN 1
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%停止%' THEN 1
                            WHEN upper(trim(COALESCE(StatusText, ''))) LIKE '%STOP%' THEN 1
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%トラブル%' THEN 3
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%異常%' THEN 3
                            WHEN upper(trim(COALESCE(StatusText, ''))) LIKE '%ERROR%' THEN 3
                            WHEN upper(trim(COALESCE(StatusText, ''))) LIKE '%ALARM%' THEN 3
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%サンプル%' THEN 2
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%レス処理%' THEN 2
                            WHEN trim(COALESCE(StatusText, '')) LIKE '%吸引時間%' THEN 2
                            WHEN trim(COALESCE(InternalState, '')) = '0' THEN 0
                            WHEN trim(COALESCE(InternalState, '')) = '1' THEN 1
                            WHEN trim(COALESCE(InternalState, '')) = '2' THEN 2
                            WHEN trim(COALESCE(InternalState, '')) = '3' THEN 3
                            ELSE StatusCode
                        END;"
            );
        }
    }
}
