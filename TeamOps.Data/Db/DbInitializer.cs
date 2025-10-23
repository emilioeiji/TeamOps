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
            var sql = @"
                CREATE TABLE IF NOT EXISTS Operators (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    BadgeCode TEXT UNIQUE NOT NULL,
                    Status TEXT NOT NULL,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS GroupLeaders (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Login TEXT UNIQUE NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS Assignments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    GLId INTEGER NOT NULL,
                    OperatorId INTEGER NOT NULL,
                    AssignedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (GLId) REFERENCES GroupLeaders(Id) ON DELETE CASCADE,
                    FOREIGN KEY (OperatorId) REFERENCES Operators(Id) ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS IX_Operators_BadgeCode ON Operators(BadgeCode);
                CREATE INDEX IF NOT EXISTS IX_GL_Login ON GroupLeaders(Login);
                CREATE INDEX IF NOT EXISTS IX_Assignments_GL_Operator ON Assignments(GLId, OperatorId);
            ";

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }
}
