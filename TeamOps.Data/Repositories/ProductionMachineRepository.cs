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
            var normalizedMachineCode = NormalizeCode(machineCode);
            return conn.QueryFirstOrDefault<Machine>(
                @"
                    SELECT
                        Id,
                        NamePt,
                        NameJp,
                        MachineCode,
                        MachineKey,
                        LineCode,
                        LocalId,
                        SectorId,
                        COALESCE(IsActive, 1) AS IsActive
                    FROM Machines
                    WHERE upper(trim(COALESCE(MachineCode, ''))) = @machineCode
                    LIMIT 1;",
                new
                {
                    machineCode = normalizedMachineCode
                }
            );
        }

        public Machine? GetByMachineKey(System.Data.IDbConnection conn, string machineCode, string lineCode)
        {
            var machineKey = BuildMachineKey(machineCode, lineCode);
            var normalizedMachineCode = NormalizeCode(machineCode);
            var normalizedLineCode = NormalizeCode(lineCode);

            return conn.QueryFirstOrDefault<Machine>(
                @"
                    SELECT
                        Id,
                        NamePt,
                        NameJp,
                        MachineCode,
                        MachineKey,
                        LineCode,
                        LocalId,
                        SectorId,
                        COALESCE(IsActive, 1) AS IsActive
                    FROM Machines
                    WHERE upper(trim(COALESCE(MachineKey, ''))) = @machineKey
                       OR (
                            upper(trim(COALESCE(MachineCode, ''))) = @machineCode
                            AND upper(trim(COALESCE(LineCode, ''))) = @lineCode
                       )
                    ORDER BY CASE WHEN upper(trim(COALESCE(MachineKey, ''))) = @machineKey THEN 0 ELSE 1 END, Id
                    LIMIT 1;",
                new
                {
                    machineKey,
                    machineCode = normalizedMachineCode,
                    lineCode = normalizedLineCode
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
            var normalizedMachineCode = NormalizeCode(machineCode);
            var normalizedLineCode = NormalizeCode(lineCode);
            var machineKey = BuildMachineKey(normalizedMachineCode, normalizedLineCode);

            var machine = GetByMachineKey(conn, normalizedMachineCode, normalizedLineCode);

            if (machine != null)
            {
                if (!string.Equals(machine.MachineCode, normalizedMachineCode, System.StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(machine.LineCode, normalizedLineCode, System.StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(machine.MachineKey, machineKey, System.StringComparison.OrdinalIgnoreCase))
                {
                    conn.Execute(
                        @"
                            UPDATE Machines
                            SET
                                MachineCode = @machineCode,
                                LineCode = @lineCode,
                                MachineKey = @machineKey
                            WHERE Id = @id;",
                        new
                        {
                            machineCode = normalizedMachineCode,
                            lineCode = normalizedLineCode,
                            machineKey,
                            id = machine.Id
                        }
                    );

                    machine.MachineCode = normalizedMachineCode;
                    machine.LineCode = normalizedLineCode;
                    machine.MachineKey = machineKey;
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
                        MachineKey,
                        LineCode,
                        SectorId,
                        IsActive
                    )
                    VALUES
                    (
                        @namePt,
                        @nameJp,
                        @machineCode,
                        @machineKey,
                        @lineCode,
                        @sectorId,
                        1
                    );
                    SELECT last_insert_rowid();",
                new
                {
                    namePt = normalizedMachineCode,
                    nameJp = normalizedMachineCode,
                    machineCode = normalizedMachineCode,
                    machineKey,
                    lineCode = normalizedLineCode,
                    sectorId
                }
            );

            return new Machine
            {
                Id = newId,
                NamePt = normalizedMachineCode,
                NameJp = normalizedMachineCode,
                MachineCode = normalizedMachineCode,
                MachineKey = machineKey,
                LineCode = normalizedLineCode,
                SectorId = sectorId,
                IsActive = true
            };
        }

        public static string BuildMachineKey(string machineCode, string lineCode)
        {
            return $"{NormalizeCode(lineCode)}:{NormalizeCode(machineCode)}";
        }

        private static string NormalizeCode(string value)
        {
            return (value ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}
