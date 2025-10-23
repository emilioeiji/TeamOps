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

        public int Add(FollowUp f)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO FollowUps 
                (Date, ShiftId, OperatorCodigoFJ, ExecutorCodigoFJ, WitnessCodigoFJ,
                 ReasonId, TypeId, LocalId, EquipmentId, Description, Guidance)
                VALUES (@date, @shift, @op, @exec, @wit, @reason, @type, @local, @equip, @desc, @guide);
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
            cmd.Parameters.AddWithValue("@desc", f.Description);
            cmd.Parameters.AddWithValue("@guide", f.Guidance);

            return (int)(long)cmd.ExecuteScalar()!;
        }

        public List<FollowUp> GetByOperator(string codigoFJ)
        {
            var list = new List<FollowUp>();
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, Date, ShiftId, OperatorCodigoFJ, ExecutorCodigoFJ, WitnessCodigoFJ,
                       ReasonId, TypeId, LocalId, EquipmentId, Description, Guidance
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
                    Description = reader.GetString(10),
                    Guidance = reader.GetString(11)
                });
            }
            return list;
        }

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
