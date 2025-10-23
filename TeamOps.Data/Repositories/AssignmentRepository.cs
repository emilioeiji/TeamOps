using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class AssignmentRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public AssignmentRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        // 🔹 Retorna todos os assignments de um GL específico, já trazendo os dados do operador
        public List<Operator> GetByGroupLeader(int glId)
        {
            var list = new List<Operator>();
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT o.CodigoFJ, o.NameRomanji, o.NameNihongo, o.ShiftId, o.GroupId, o.SectorId,
                       o.StartDate, o.EndDate, o.Trainer, o.Status, o.CreatedAt
                FROM Assignments a
                INNER JOIN Operators o ON a.OperatorCodigoFJ = o.CodigoFJ
                WHERE a.GLId = @glId AND o.Status = 1";
            cmd.Parameters.AddWithValue("@glId", glId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Operator
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
                });
            }
            return list;
        }

        // 🔹 Adiciona um assignment (GL + Operador)
        public void Add(int glId, string operatorCodigoFJ)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Assignments (GLId, OperatorCodigoFJ, AssignedAt)
                VALUES (@glId, @op, CURRENT_TIMESTAMP)";
            cmd.Parameters.AddWithValue("@glId", glId);
            cmd.Parameters.AddWithValue("@op", operatorCodigoFJ);
            cmd.ExecuteNonQuery();
        }

        // 🔹 Remove um assignment (GL + Operador)
        public void Remove(int glId, string operatorCodigoFJ)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Assignments WHERE GLId=@glId AND OperatorCodigoFJ=@op";
            cmd.Parameters.AddWithValue("@glId", glId);
            cmd.Parameters.AddWithValue("@op", operatorCodigoFJ);
            cmd.ExecuteNonQuery();
        }
    }
}
