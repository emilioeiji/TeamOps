// Project: TeamOps.Config
// File: DbSettings.cs
using System;

namespace TeamOps.Config
{
    public sealed class DbSettings
    {
        public bool PortableMode { get; }
        public string DatabasePath { get; }
        public string ConnectionString { get; }

        // Exemplo: permitir Shared Cache e criação caso não exista
        // Microsoft.Data.Sqlite usa "Data Source=...", e aceita opções no connection string.
        public DbSettings(bool portableMode = false)
        {
            PortableMode = portableMode;
            DatabasePath = AppPaths.GetDatabasePath(PortableMode);

            // Flags úteis:
            // - Mode=ReadWriteCreate: cria se não existir
            // - Pooling=True: reaproveita conexões abertas pelo app
            // - Default Timeout: dá tempo para importações e telas concorrentes liberarem locks curtos
            // - Foreign Keys: será habilitado por PRAGMA após abrir conexão
            ConnectionString = $"Data Source={DatabasePath};Mode=ReadWriteCreate;Pooling=True;Default Timeout=30";
        }

        public override string ToString() => $"DB: {DatabasePath} | Portable: {PortableMode}";
    }
}
