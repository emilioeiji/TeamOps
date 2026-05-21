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
                    PRAGMA journal_mode = WAL;      -- melhora leitura concorrente entre múltiplas telas/processos
                    PRAGMA synchronous = NORMAL;    -- bom balanço entre segurança e velocidade
                    PRAGMA busy_timeout = 5000;     -- reduz falhas transitórias de lock em multiacesso
                    PRAGMA temp_store = MEMORY;     -- reduz I/O temporário em consultas maiores
                ";
                cmd.ExecuteNonQuery();
            }

            return conn;
        }
    }
}
