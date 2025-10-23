using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class EquipmentRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public EquipmentRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public int Add(Equipment e)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Equipments (NamePt, NameJp) 
                VALUES (@pt, @jp);
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@pt", e.NamePt);
            cmd.Parameters.AddWithValue("@jp", e.NameJp);
            return (int)(long)cmd.ExecuteScalar()!;
        }

        public List<Equipment> GetAll()
        {
            var list = new List<Equipment>();
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NamePt, NameJp FROM Equipments ORDER BY Id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Equipment
                {
                    Id = reader.GetInt32(0),
                    NamePt = reader.GetString(1),
                    NameJp = reader.GetString(2)
                });
            }
            return list;
        }

        public Equipment? GetById(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NamePt, NameJp FROM Equipments WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Equipment
                {
                    Id = reader.GetInt32(0),
                    NamePt = reader.GetString(1),
                    NameJp = reader.GetString(2)
                };
            }
            return null;
        }

        public void Update(Equipment e)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Equipments 
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
            cmd.CommandText = "DELETE FROM Equipments WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
