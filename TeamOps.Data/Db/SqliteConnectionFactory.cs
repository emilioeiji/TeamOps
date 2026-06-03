// Project: TeamOps.Data
// File: Db/SqliteConnectionFactory.cs
using Microsoft.Data.Sqlite;
using TeamOps.Config;

namespace TeamOps.Data.Db
{
    public sealed class SqliteConnectionFactory
    {
        private static readonly object PragmasLock = new();
        private static bool _journalConfigured;

        private readonly DbSettings _settings;

        public SqliteConnectionFactory(DbSettings settings)
        {
            _settings = settings;
        }

        public SqliteConnection CreateOpenConnection()
        {
            var conn = new SqliteConnection(_settings.ConnectionString);
            conn.Open();

            ConfigureConnection(conn);

            return conn;
        }

        private static void ConfigureConnection(SqliteConnection conn)
        {
            // Keep per-connection pragmas cheap; journal_mode can acquire a write lock,
            // so it is configured once per process instead of on every DB open.
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandTimeout = 30;
                cmd.CommandText = @"
                    PRAGMA foreign_keys = ON;
                    PRAGMA synchronous = NORMAL;    -- bom balanço entre segurança e velocidade
                    PRAGMA busy_timeout = 30000;    -- aguarda locks transitórios em importação/telas concorrentes
                    PRAGMA temp_store = MEMORY;     -- reduz I/O temporário em consultas maiores
                    PRAGMA cache_size = -20000;     -- cerca de 20 MB de cache por conexão
                ";
                cmd.ExecuteNonQuery();
            }

            if (_journalConfigured)
            {
                return;
            }

            lock (PragmasLock)
            {
                if (_journalConfigured)
                {
                    return;
                }

                using var cmd = conn.CreateCommand();
                cmd.CommandTimeout = 30;
                // DELETE avoids persistent .wal/.shm sidecar files and is more compatible
                // with external SQLite tools used to inspect the production database.
                cmd.CommandText = @"
                    PRAGMA wal_checkpoint(TRUNCATE);
                    PRAGMA journal_mode = DELETE;
                    PRAGMA journal_size_limit = 67108864;
                ";
                cmd.ExecuteNonQuery();
                _journalConfigured = true;
            }
        }
    }
}
