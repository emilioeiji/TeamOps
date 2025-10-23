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
            // - Cache=Shared: melhor para múltiplas conexões dentro do processo
            // - Mode=ReadWriteCreate: cria se não existir
            // - Foreign Keys: será habilitado por PRAGMA após abrir conexão
            ConnectionString = $"Data Source={DatabasePath};Cache=Shared;Mode=ReadWriteCreate";
        }

        public override string ToString() => $"DB: {DatabasePath} | Portable: {PortableMode}";
    }
}
