using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class MachineRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public MachineRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public int Add(Machine e)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Machines (NamePt, NameJp) 
                VALUES (@pt, @jp);
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@pt", e.NamePt);
            cmd.Parameters.AddWithValue("@jp", e.NameJp);
            return (int)(long)cmd.ExecuteScalar()!;
        }

        public List<Machine> GetAll()
        {
            var list = new List<Machine>();
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NamePt, NameJp FROM Machines ORDER BY NamePt";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Machine
                {
                    Id = reader.GetInt32(0),
                    NamePt = reader.GetString(1),
                    NameJp = reader.GetString(2)
                });
            }
            return list;
        }

        public Machine? GetById(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NamePt, NameJp FROM Machines WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Machine
                {
                    Id = reader.GetInt32(0),
                    NamePt = reader.GetString(1),
                    NameJp = reader.GetString(2)
                };
            }
            return null;
        }

        public void Update(Machine e)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Machines 
                SET NamePt = @pt, NameJp = @jp
                WHERE Id = @id";
            cmd.Parameters.AddWithValue("@pt", e.NamePt);
            cmd.Parameters.AddWithValue("@jp", e.NameJp);
            cmd.Parameters.AddWithValue("@id", e.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Machines WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public bool ExistsByName(string namePt)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Machines WHERE NamePt = @pt";
            cmd.Parameters.AddWithValue("@pt", namePt);
            return (long)cmd.ExecuteScalar()! > 0;
        }
    }
}
