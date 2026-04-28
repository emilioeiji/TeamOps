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
                    ON Machines(SectorId);"
            );
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
    }
}
