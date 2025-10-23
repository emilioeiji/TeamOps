using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class SectorRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public SectorRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public int Add(Sector s)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Sectors (NamePt, NameJp) VALUES (@pt, @jp); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@pt", s.NamePt);
            cmd.Parameters.AddWithValue("@jp", s.NameJp);
            return (int)(long)cmd.ExecuteScalar()!;
        }

        public Sector? GetById(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NamePt, NameJp FROM Sectors WHERE Id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new Sector
            {
                Id = reader.GetInt32(0),
                NamePt = reader.GetString(1),
                NameJp = reader.GetString(2)
            };
        }

        public List<Sector> GetAll()
        {
            var list = new List<Sector>();
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NamePt, NameJp FROM Sectors ORDER BY Id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Sector
                {
                    Id = reader.GetInt32(0),
                    NamePt = reader.GetString(1),
                    NameJp = reader.GetString(2)
                });
            }
            return list;
        }

        public void Update(Sector s)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Sectors SET NamePt=@pt, NameJp=@jp WHERE Id=@id";
            cmd.Parameters.AddWithValue("@pt", s.NamePt);
            cmd.Parameters.AddWithValue("@jp", s.NameJp);
            cmd.Parameters.AddWithValue("@id", s.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Sectors WHERE Id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
