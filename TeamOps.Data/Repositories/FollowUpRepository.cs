using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class FollowUpRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public FollowUpRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        // ---------------------------------------------------------
        // ADD
        // ---------------------------------------------------------
        public int Add(FollowUp f)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                INSERT INTO FollowUps
                (Date, ShiftId, OperatorCodigoFJ, ExecutorCodigoFJ, WitnessCodigoFJ,
                 ReasonId, TypeId, LocalId, EquipmentId, SectorId, Description, Guidance)
                VALUES
                (@date, @shift, @op, @exec, @wit, @reason, @type, @local, @equip, @sector, @desc, @guide);
                SELECT last_insert_rowid();";

            cmd.Parameters.AddWithValue("@date", f.Date);
            cmd.Parameters.AddWithValue("@shift", f.ShiftId);
            cmd.Parameters.AddWithValue("@op", f.OperatorCodigoFJ);
            cmd.Parameters.AddWithValue("@exec", f.ExecutorCodigoFJ);
            cmd.Parameters.AddWithValue("@wit", (object?)f.WitnessCodigoFJ ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@reason", f.ReasonId);
            cmd.Parameters.AddWithValue("@type", f.TypeId);
            cmd.Parameters.AddWithValue("@local", f.LocalId);
            cmd.Parameters.AddWithValue("@equip", f.EquipmentId);
            cmd.Parameters.AddWithValue("@sector", f.SectorId);
            cmd.Parameters.AddWithValue("@desc", f.Description);
            cmd.Parameters.AddWithValue("@guide", f.Guidance);

            return (int)(long)cmd.ExecuteScalar()!;
        }

        // ---------------------------------------------------------
        // GET BY PERIOD (COM JOINs COMPLETOS)
        // ---------------------------------------------------------
        public List<FollowUp> GetByPeriod(DateTime start, DateTime end)
        {
            var list = new List<FollowUp>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT 
                    f.Id,
                    f.Date,
                    f.ShiftId,
                    f.OperatorCodigoFJ,
                    f.ExecutorCodigoFJ,
                    f.WitnessCodigoFJ,
                    f.ReasonId,
                    f.TypeId,
                    f.LocalId,
                    f.EquipmentId,
                    f.SectorId,
                    f.Description,
                    f.Guidance,

                    -- JOINs
                    s.NamePt AS ShiftName,
                    op.NameRomanji AS OperatorName,
                    ex.NameRomanji AS ExecutorName,
                    wi.NameRomanji AS WitnessName,
                    r.NamePt AS ReasonName,
                    t.NamePt AS TypeName,
                    l.NamePt AS LocalName,
                    e.NamePt AS EquipmentName,
                    sc.NamePt AS SectorName

                FROM FollowUps f
                LEFT JOIN Shifts s ON s.Id = f.ShiftId
                LEFT JOIN Operators op ON op.CodigoFJ = f.OperatorCodigoFJ
                LEFT JOIN Operators ex ON ex.CodigoFJ = f.ExecutorCodigoFJ
                LEFT JOIN Operators wi ON wi.CodigoFJ = f.WitnessCodigoFJ
                LEFT JOIN FollowUpReasons r ON r.Id = f.ReasonId
                LEFT JOIN FollowUpTypes t ON t.Id = f.TypeId
                LEFT JOIN Locals l ON l.Id = f.LocalId
                LEFT JOIN Equipments e ON e.Id = f.EquipmentId
                LEFT JOIN Sectors sc ON sc.Id = f.SectorId

                WHERE f.Date >= @start AND f.Date < @end
                ORDER BY f.Date DESC";

            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new FollowUp
                {
                    Id = reader.GetInt32(0),
                    Date = reader.GetDateTime(1),
                    ShiftId = reader.GetInt32(2),
                    OperatorCodigoFJ = reader.GetString(3),
                    ExecutorCodigoFJ = reader.GetString(4),
                    WitnessCodigoFJ = reader.IsDBNull(5) ? null : reader.GetString(5),
                    ReasonId = reader.GetInt32(6),
                    TypeId = reader.GetInt32(7),
                    LocalId = reader.GetInt32(8),
                    EquipmentId = reader.GetInt32(9),
                    SectorId = reader.GetInt32(10),
                    Description = reader.GetString(11),
                    Guidance = reader.GetString(12),

                    // JOINs
                    ShiftName = reader.IsDBNull(13) ? "" : reader.GetString(13),
                    OperatorName = reader.IsDBNull(14) ? "" : reader.GetString(14),
                    ExecutorName = reader.IsDBNull(15) ? "" : reader.GetString(15),
                    WitnessName = reader.IsDBNull(16) ? "" : reader.GetString(16),
                    ReasonName = reader.IsDBNull(17) ? "" : reader.GetString(17),
                    TypeName = reader.IsDBNull(18) ? "" : reader.GetString(18),
                    LocalName = reader.IsDBNull(19) ? "" : reader.GetString(19),
                    EquipmentName = reader.IsDBNull(20) ? "" : reader.GetString(20),
                    SectorName = reader.IsDBNull(21) ? "" : reader.GetString(21)
                });
            }

            return list;
        }

        // ---------------------------------------------------------
        // GET BY OPERATOR (SEM JOINs — mantém compatibilidade)
        // ---------------------------------------------------------
        public List<FollowUp> GetByOperator(string codigoFJ)
        {
            var list = new List<FollowUp>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT Id, Date, ShiftId, OperatorCodigoFJ, ExecutorCodigoFJ, WitnessCodigoFJ,
                       ReasonId, TypeId, LocalId, EquipmentId, SectorId, Description, Guidance
                FROM FollowUps
                WHERE OperatorCodigoFJ = @op
                ORDER BY Date DESC";

            cmd.Parameters.AddWithValue("@op", codigoFJ);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new FollowUp
                {
                    Id = reader.GetInt32(0),
                    Date = reader.GetDateTime(1),
                    ShiftId = reader.GetInt32(2),
                    OperatorCodigoFJ = reader.GetString(3),
                    ExecutorCodigoFJ = reader.GetString(4),
                    WitnessCodigoFJ = reader.IsDBNull(5) ? null : reader.GetString(5),
                    ReasonId = reader.GetInt32(6),
                    TypeId = reader.GetInt32(7),
                    LocalId = reader.GetInt32(8),
                    EquipmentId = reader.GetInt32(9),
                    SectorId = reader.GetInt32(10),
                    Description = reader.GetString(11),
                    Guidance = reader.GetString(12)
                });
            }

            return list;
        }

        // ---------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------
        public void Delete(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = "DELETE FROM FollowUps WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
