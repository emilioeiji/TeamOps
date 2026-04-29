using Dapper;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class ProductionMachineRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public ProductionMachineRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public Machine? GetByMachineCode(string machineCode)
        {
            using var conn = _factory.CreateOpenConnection();
            ProductionSchemaMigrator.Ensure(conn);
            return GetByMachineCode(conn, machineCode);
        }

        public Machine? GetByMachineCode(System.Data.IDbConnection conn, string machineCode)
        {
            return conn.QueryFirstOrDefault<Machine>(
                @"
                    SELECT
                        Id,
                        NamePt,
                        NameJp,
                        MachineCode,
                        LineCode,
                        LocalId,
                        SectorId,
                        COALESCE(IsActive, 1) AS IsActive
                    FROM Machines
                    WHERE MachineCode = @machineCode
                    LIMIT 1;",
                new
                {
                    machineCode
                }
            );
        }

        public Machine EnsureMachine(string machineCode, string lineCode, int? sectorId = null)
        {
            using var conn = _factory.CreateOpenConnection();
            ProductionSchemaMigrator.Ensure(conn);
            return EnsureMachine(conn, machineCode, lineCode, sectorId);
        }

        public Machine EnsureMachine(System.Data.IDbConnection conn, string machineCode, string lineCode, int? sectorId = null)
        {
            var machine = conn.QueryFirstOrDefault<Machine>(
                @"
                    SELECT
                        Id,
                        NamePt,
                        NameJp,
                        MachineCode,
                        LineCode,
                        LocalId,
                        SectorId,
                        COALESCE(IsActive, 1) AS IsActive
                    FROM Machines
                    WHERE MachineCode = @machineCode
                    LIMIT 1;",
                new
                {
                    machineCode
                }
            );

            if (machine != null)
            {
                if (!string.IsNullOrWhiteSpace(lineCode) && !string.Equals(machine.LineCode, lineCode, System.StringComparison.OrdinalIgnoreCase))
                {
                    conn.Execute(
                        @"
                            UPDATE Machines
                            SET LineCode = @lineCode
                            WHERE Id = @id;",
                        new
                        {
                            lineCode,
                            id = machine.Id
                        }
                    );

                    machine.LineCode = lineCode;
                }

                if (sectorId.HasValue && machine.SectorId != sectorId.Value)
                {
                    conn.Execute(
                        @"
                            UPDATE Machines
                            SET SectorId = @sectorId
                            WHERE Id = @id;",
                        new
                        {
                            sectorId,
                            id = machine.Id
                        }
                    );

                    machine.SectorId = sectorId.Value;
                }

                return machine;
            }

            var newId = conn.ExecuteScalar<int>(
                @"
                    INSERT INTO Machines
                    (
                        NamePt,
                        NameJp,
                        MachineCode,
                        LineCode,
                        SectorId,
                        IsActive
                    )
                    VALUES
                    (
                        @namePt,
                        @nameJp,
                        @machineCode,
                        @lineCode,
                        @sectorId,
                        1
                    );
                    SELECT last_insert_rowid();",
                new
                {
                    namePt = machineCode,
                    nameJp = machineCode,
                    machineCode,
                    lineCode,
                    sectorId
                }
            );

            return new Machine
            {
                Id = newId,
                NamePt = machineCode,
                NameJp = machineCode,
                MachineCode = machineCode,
                LineCode = lineCode,
                SectorId = sectorId,
                IsActive = true
            };
        }
    }
}
