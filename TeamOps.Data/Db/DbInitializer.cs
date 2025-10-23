// Project: TeamOps.Data
// File: Db/DbInitializer.cs
using Microsoft.Data.Sqlite;
using System;
using System.IO;
using TeamOps.Config;

namespace TeamOps.Data.Db
{
    public sealed class DbInitializer
    {
        private readonly DbSettings _settings;

        public DbInitializer(DbSettings settings)
        {
            _settings = settings;
        }

        public void EnsureCreated()
        {
            var dbPath = _settings.DatabasePath;
            var dbDir = Path.GetDirectoryName(dbPath)!;
            Directory.CreateDirectory(dbDir);

            var isNew = !File.Exists(dbPath);
            if (isNew)
            {
                // Cria o arquivo abrindo uma conexão
                using (var conn = new SqliteConnection(_settings.ConnectionString))
                {
                    conn.Open();
                    ApplyInitialSchema(conn);
                }
            }
        }

        private static void ApplyInitialSchema(SqliteConnection conn)
        {
            // Caminho relativo ao diretório de saída (bin/Debug/netX/)
            var sqlPath = Path.Combine(AppContext.BaseDirectory, "Migrations", "InitialSchema.sql");

            if (!File.Exists(sqlPath))
                throw new FileNotFoundException($"Arquivo de schema não encontrado: {sqlPath}");

            var sql = File.ReadAllText(sqlPath);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }
}
