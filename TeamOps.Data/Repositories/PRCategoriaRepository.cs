using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class PRCategoriaRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public PRCategoriaRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<LookupItem> GetAll()
        {
            var list = new List<LookupItem>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = "SELECT Id, NamePt, NameJp FROM PRCategorias ORDER BY Id";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new LookupItem
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
