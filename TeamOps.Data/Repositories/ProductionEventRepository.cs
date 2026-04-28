using System;
using System.Data;
using Dapper;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class ProductionEventRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public ProductionEventRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public bool InsertOrIgnore(IDbConnection conn, IDbTransaction? tx, MachineEvent machineEvent)
        {
            var affected = conn.Execute(
                @"
                    INSERT OR IGNORE INTO MachineEvents
                    (
                        MachineId,
                        MachineCode,
                        LineCode,
                        LocalId,
                        SectorId,
                        RecipeName,
                        LotNo,
                        StatusCode,
                        StatusText,
                        InternalState,
                        EventDateTime,
                        SourceFile,
                        ImportedAt
                    )
                    VALUES
                    (
                        @MachineId,
                        @MachineCode,
                        @LineCode,
                        @LocalId,
                        @SectorId,
                        @RecipeName,
                        @LotNo,
                        @StatusCode,
                        @StatusText,
                        @InternalState,
                        @EventDateTime,
                        @SourceFile,
                        @ImportedAt
                    );",
                new
                {
                    machineEvent.MachineId,
                    machineEvent.MachineCode,
                    machineEvent.LineCode,
                    machineEvent.LocalId,
                    machineEvent.SectorId,
                    machineEvent.RecipeName,
                    machineEvent.LotNo,
                    machineEvent.StatusCode,
                    machineEvent.StatusText,
                    machineEvent.InternalState,
                    EventDateTime = machineEvent.EventDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    machineEvent.SourceFile,
                    ImportedAt = machineEvent.ImportedAt.ToString("yyyy-MM-dd HH:mm:ss")
                },
                tx
            );

            return affected > 0;
        }

        public void RefreshCurrentStatus(IDbConnection conn, IDbTransaction? tx, int machineId)
        {
            var latest = conn.QueryFirstOrDefault<MachineCurrentStatus>(
                @"
                    SELECT
                        MachineId,
                        MachineCode,
                        LineCode,
                        LocalId,
                        SectorId,
                        COALESCE(RecipeName, '') AS RecipeName,
                        COALESCE(LotNo, '') AS LotNo,
                        StatusCode,
                        StatusText,
                        InternalState,
                        EventDateTime,
                        COALESCE(ImportedAt, CURRENT_TIMESTAMP) AS UpdatedAt
                    FROM MachineEvents
                    WHERE MachineId = @machineId
                    ORDER BY datetime(EventDateTime) DESC, Id DESC
                    LIMIT 1;",
                new
                {
                    machineId
                },
                tx
            );

            if (latest == null)
            {
                return;
            }

            conn.Execute(
                @"
                    INSERT INTO MachineCurrentStatus
                    (
                        MachineId,
                        MachineCode,
                        LineCode,
                        LocalId,
                        SectorId,
                        RecipeName,
                        LotNo,
                        StatusCode,
                        StatusText,
                        InternalState,
                        EventDateTime,
                        UpdatedAt
                    )
                    VALUES
                    (
                        @MachineId,
                        @MachineCode,
                        @LineCode,
                        @LocalId,
                        @SectorId,
                        @RecipeName,
                        @LotNo,
                        @StatusCode,
                        @StatusText,
                        @InternalState,
                        @EventDateTime,
                        @UpdatedAt
                    )
                    ON CONFLICT(MachineId) DO UPDATE SET
                        MachineCode = excluded.MachineCode,
                        LineCode = excluded.LineCode,
                        LocalId = excluded.LocalId,
                        SectorId = excluded.SectorId,
                        RecipeName = excluded.RecipeName,
                        LotNo = excluded.LotNo,
                        StatusCode = excluded.StatusCode,
                        StatusText = excluded.StatusText,
                        InternalState = excluded.InternalState,
                        EventDateTime = excluded.EventDateTime,
                        UpdatedAt = excluded.UpdatedAt;",
                new
                {
                    latest.MachineId,
                    latest.MachineCode,
                    latest.LineCode,
                    latest.LocalId,
                    latest.SectorId,
                    latest.RecipeName,
                    latest.LotNo,
                    latest.StatusCode,
                    latest.StatusText,
                    latest.InternalState,
                    EventDateTime = latest.EventDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                },
                tx
            );
        }
    }
}
