using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class ShiftRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public ShiftRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public int Add(Shift s)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Shifts (NamePt, NameJp) VALUES (@pt, @jp); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@pt", s.NamePt);
            cmd.Parameters.AddWithValue("@jp", s.NameJp);
            return (int)(long)cmd.ExecuteScalar()!;
        }

        public List<Shift> GetAll()
        {
            var list = new List<Shift>();
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NamePt, NameJp FROM Shifts ORDER BY Id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Shift
                {
                    Id = reader.GetInt32(0),
                    NamePt = reader.GetString(1),
                    NameJp = reader.GetString(2)
                });
            }
            return list;
        }
    }
}
