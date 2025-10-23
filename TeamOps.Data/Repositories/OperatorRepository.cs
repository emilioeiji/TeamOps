using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class OperatorRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public OperatorRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public void Add(Operator op)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Operators 
                (CodigoFJ, NameRomanji, NameNihongo, ShiftId, GroupId, SectorId, StartDate, EndDate, Trainer, Status)
                VALUES (@c, @r, @n, @s, @g, @sec, @start, @end, @t, @st)";
            cmd.Parameters.AddWithValue("@c", op.CodigoFJ);
            cmd.Parameters.AddWithValue("@r", op.NameRomanji);
            cmd.Parameters.AddWithValue("@n", op.NameNihongo);
            cmd.Parameters.AddWithValue("@s", op.ShiftId);
            cmd.Parameters.AddWithValue("@g", op.GroupId);
            cmd.Parameters.AddWithValue("@sec", op.SectorId);
            cmd.Parameters.AddWithValue("@start", op.StartDate);
            cmd.Parameters.AddWithValue("@end", (object?)op.EndDate ?? DBNull.Value);   
            cmd.Parameters.AddWithValue("@t", op.Trainer ? 1 : 0);
            cmd.Parameters.AddWithValue("@st", op.Status ? 1 : 0);
            cmd.ExecuteNonQuery();
        }

        public Operator? GetByCodigoFJ(string codigoFJ)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT CodigoFJ, NameRomanji, NameNihongo, ShiftId, GroupId, SectorId, StartDate, EndDate, Trainer, Status, CreatedAt FROM Operators WHERE CodigoFJ = @c";
            cmd.Parameters.AddWithValue("@c", codigoFJ);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return Map(reader);
        }

        public List<Operator> GetAll()
        {
            var list = new List<Operator>();
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT CodigoFJ, NameRomanji, NameNihongo, ShiftId, GroupId, SectorId, StartDate, EndDate, Trainer, Status, CreatedAt FROM Operators WHERE Status = 1 ORDER BY NameRomanji";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(Map(reader));
            return list;
        }

        public void Update(Operator op)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Operators
                SET NameRomanji=@r, NameNihongo=@n, ShiftId=@s, GroupId=@g, SectorId=@sec,
                    StartDate=@start, EndDate=@end, Trainer=@t, Status=@st
                WHERE CodigoFJ=@c";
            cmd.Parameters.AddWithValue("@c", op.CodigoFJ);
            cmd.Parameters.AddWithValue("@r", op.NameRomanji);
            cmd.Parameters.AddWithValue("@n", op.NameNihongo);
            cmd.Parameters.AddWithValue("@s", op.ShiftId);
            cmd.Parameters.AddWithValue("@g", op.GroupId);
            cmd.Parameters.AddWithValue("@sec", op.SectorId);
            cmd.Parameters.AddWithValue("@start", op.StartDate);
            cmd.Parameters.AddWithValue("@end", (object?)op.EndDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@t", op.Trainer ? 1 : 0);
            cmd.Parameters.AddWithValue("@st", op.Status ? 1 : 0);
            cmd.ExecuteNonQuery();
        }

        public void Delete(string codigoFJ)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Operators WHERE CodigoFJ=@c";
            cmd.Parameters.AddWithValue("@c", codigoFJ);
            cmd.ExecuteNonQuery();
        }

        private Operator Map(SqliteDataReader reader)
        {
            return new Operator
            {
                CodigoFJ = reader.GetString(0),
                NameRomanji = reader.GetString(1),
                NameNihongo = reader.GetString(2),
                ShiftId = reader.GetInt32(3),
                GroupId = reader.GetInt32(4),
                SectorId = reader.GetInt32(5),
                StartDate = reader.GetDateTime(6),
                EndDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                Trainer = reader.GetBoolean(8),
                Status = reader.GetBoolean(9),
                CreatedAt = reader.GetDateTime(10)
            };
        }
    }
}
