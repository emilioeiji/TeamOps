using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class FollowUpReasonRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public FollowUpReasonRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public int Add(FollowUpReason r)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO FollowUpReasons (NamePt, NameJp) 
                VALUES (@pt, @jp);
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@pt", r.NamePt);
            cmd.Parameters.AddWithValue("@jp", r.NameJp);
            return (int)(long)cmd.ExecuteScalar()!;
        }

        public List<FollowUpReason> GetAll()
        {
            var list = new List<FollowUpReason>();
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NamePt, NameJp FROM FollowUpReasons ORDER BY Id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new FollowUpReason
                {
                    Id = reader.GetInt32(0),
                    NamePt = reader.GetString(1),
                    NameJp = reader.GetString(2)
                });
            }
            return list;
        }

        public FollowUpReason? GetById(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NamePt, NameJp FROM FollowUpReasons WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new FollowUpReason
                {
                    Id = reader.GetInt32(0),
                    NamePt = reader.GetString(1),
                    NameJp = reader.GetString(2)
                };
            }
            return null;
        }

        public void Update(FollowUpReason r)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE FollowUpReasons 
                SET NamePt = @pt, NameJp = @jp
                WHERE Id = @id";
            cmd.Parameters.AddWithValue("@pt", r.NamePt);
            cmd.Parameters.AddWithValue("@jp", r.NameJp);
            cmd.Parameters.AddWithValue("@id", r.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM FollowUpReasons WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
