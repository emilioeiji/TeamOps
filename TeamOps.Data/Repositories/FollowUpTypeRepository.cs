// Project: TeamOps.Data
// File: Repositories/FollowUpTypeRepository.cs
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class FollowUpTypeRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public FollowUpTypeRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public int Add(FollowUpType t)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO FollowUpTypes (NamePt, NameJp)
                VALUES (@pt, @jp);
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@pt", t.NamePt);
            cmd.Parameters.AddWithValue("@jp", t.NameJp);
            return (int)(long)cmd.ExecuteScalar()!;
        }

        public List<FollowUpType> GetAll()
        {
            var list = new List<FollowUpType>();
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NamePt, NameJp FROM FollowUpTypes ORDER BY Id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new FollowUpType
                {
                    Id = reader.GetInt32(0),
                    NamePt = reader.GetString(1),
                    NameJp = reader.GetString(2)
                });
            }
            return list;
        }

        public FollowUpType? GetById(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NamePt, NameJp FROM FollowUpTypes WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new FollowUpType
                {
                    Id = reader.GetInt32(0),
                    NamePt = reader.GetString(1),
                    NameJp = reader.GetString(2)
                };
            }
            return null;
        }

        public void Update(FollowUpType t)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE FollowUpTypes
                SET NamePt = @pt, NameJp = @jp
                WHERE Id = @id";
            cmd.Parameters.AddWithValue("@pt", t.NamePt);
            cmd.Parameters.AddWithValue("@jp", t.NameJp);
            cmd.Parameters.AddWithValue("@id", t.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM FollowUpTypes WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
