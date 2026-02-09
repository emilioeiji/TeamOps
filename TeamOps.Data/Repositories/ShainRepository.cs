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
                INSERT INTO Shain (NomeRomanji, NomeNihongo)
                VALUES (@rj, @jp);
                SELECT last_insert_rowid();";

            cmd.Parameters.AddWithValue("@rj", s.NomeRomanji);
            cmd.Parameters.AddWithValue("@jp", s.NomeNihongo);

            return (int)(long)cmd.ExecuteScalar()!;
        }

        public List<Shain> GetAll()
        {
            var list = new List<Shain>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NomeRomanji, NomeNihongo FROM Shain ORDER BY NomeRomanji";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Shain
                {
                    Id = reader.GetInt32(0),
                    NomeRomanji = reader.GetString(1),
                    NomeNihongo = reader.GetString(2)
                });
            }

            return list;
        }

        public Shain? GetById(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NomeRomanji, NomeNihongo FROM Shain WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Shain
                {
                    Id = reader.GetInt32(0),
                    NomeRomanji = reader.GetString(1),
                    NomeNihongo = reader.GetString(2)
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
                SET NomeRomanji = @rj, NomeNihongo = @jp
                WHERE Id = @id";

            cmd.Parameters.AddWithValue("@rj", s.NomeRomanji);
            cmd.Parameters.AddWithValue("@jp", s.NomeNihongo);
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
