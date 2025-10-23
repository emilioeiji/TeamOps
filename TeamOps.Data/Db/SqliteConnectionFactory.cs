// Project: TeamOps.Data
// File: Db/SqliteConnectionFactory.cs
using Microsoft.Data.Sqlite;
using TeamOps.Config;

namespace TeamOps.Data.Db
{
    public sealed class SqliteConnectionFactory
    {
        private readonly DbSettings _settings;

        public SqliteConnectionFactory(DbSettings settings)
        {
            _settings = settings;
        }

        public SqliteConnection CreateOpenConnection()
        {
            var conn = new SqliteConnection(_settings.ConnectionString);
            conn.Open();

            // Boas práticas para integridade e desempenho local
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    PRAGMA foreign_keys = ON;
                    PRAGMA journal_mode = WAL;      -- melhor concorrência em apps desktop
                    PRAGMA synchronous = NORMAL;    -- bom balanço entre segurança e velocidade
                ";
                cmd.ExecuteNonQuery();
            }

            return conn;
        }
    }
}
