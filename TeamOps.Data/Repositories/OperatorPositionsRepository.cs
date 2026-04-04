using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class OperatorPositionsRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public OperatorPositionsRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<OperatorPosition> GetPositionsForSector(int sectorId)
        {
            var list = new List<OperatorPosition>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT Id, SectorId, LocalId, X, Y
                FROM OperatorPositions
                WHERE SectorId = @sector
                ORDER BY LocalId;
            ";

            cmd.Parameters.AddWithValue("@sector", sectorId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new OperatorPosition
                {
                    Id = reader.GetInt32(0),
                    SectorId = reader.GetInt32(1),
                    LocalId = reader.GetInt32(2),
                    X = reader.GetInt32(3),
                    Y = reader.GetInt32(4)
                });
            }

            return list;
        }

        public void InsertOrUpdate(OperatorPosition pos)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                INSERT INTO OperatorPositions (SectorId, LocalId, X, Y)
                VALUES (@sector, @local, @x, @y)
                ON CONFLICT(LocalId) DO UPDATE SET
                    X = excluded.X,
                    Y = excluded.Y;
            ";

            cmd.Parameters.AddWithValue("@sector", pos.SectorId);
            cmd.Parameters.AddWithValue("@local", pos.LocalId);
            cmd.Parameters.AddWithValue("@x", pos.X);
            cmd.Parameters.AddWithValue("@y", pos.Y);

            cmd.ExecuteNonQuery();
        }
    }
}
