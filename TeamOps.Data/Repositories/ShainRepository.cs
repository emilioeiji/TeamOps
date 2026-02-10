using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class ShainRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public ShainRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public int Add(Shain s)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Shain (NameRomanji, NameNihongo)
                VALUES (@rj, @jp);
                SELECT last_insert_rowid();";

            cmd.Parameters.AddWithValue("@rj", s.NameRomanji);
            cmd.Parameters.AddWithValue("@jp", s.NameNihongo);

            return (int)(long)cmd.ExecuteScalar()!;
        }

        public List<Shain> GetAll()
        {
            var list = new List<Shain>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NameRomanji, NameNihongo FROM Shain ORDER BY NameRomanji";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Shain
                {
                    Id = reader.GetInt32(0),
                    NameRomanji = reader.GetString(1),
                    NameNihongo = reader.GetString(2)
                });
            }

            return list;
        }

        public Shain? GetById(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NameRomanji, NameNihongo FROM Shain WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Shain
                {
                    Id = reader.GetInt32(0),
                    NameRomanji = reader.GetString(1),
                    NameNihongo = reader.GetString(2)
                };
            }

            return null;
        }

        public void Update(Shain s)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Shain
                SET NameRomanji = @rj, NameNihongo = @jp
                WHERE Id = @id";

            cmd.Parameters.AddWithValue("@rj", s.NameRomanji);
            cmd.Parameters.AddWithValue("@jp", s.NameNihongo);
            cmd.Parameters.AddWithValue("@id", s.Id);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Shain WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
