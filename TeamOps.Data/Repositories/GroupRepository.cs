using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class GroupRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public GroupRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public int Add(Group g)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Groups (NamePt, NameJp) VALUES (@pt, @jp); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@pt", g.NamePt);
            cmd.Parameters.AddWithValue("@jp", g.NameJp);
            return (int)(long)cmd.ExecuteScalar()!;
        }

        public List<Group> GetAll()
        {
            var list = new List<Group>();
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NamePt, NameJp FROM Groups ORDER BY Id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Group
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
