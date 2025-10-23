// Project: TeamOps.Config
// File: AppPaths.cs
using System;
using System.Configuration;
using System.IO;

namespace TeamOps.Config
{
    public static class AppPaths
    {
        private const string CompanyName = "TeamOps";
        private const string ProductName = "TeamOps";

        public static string GetUserDataDirectory()
        {
            var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDir = Path.Combine(baseDir, CompanyName, ProductName);
            Directory.CreateDirectory(appDir);
            return appDir;
        }

        public static string GetPortableDataDirectory()
        {
            var exeDir = AppContext.BaseDirectory;
            var dataDir = Path.Combine(exeDir, "data");
            Directory.CreateDirectory(dataDir);
            return dataDir;
        }

        public static string GetDatabasePath(bool portableMode)
        {
            // 1) Tenta ler do app.config
            var configPath = ConfigurationManager.AppSettings["DatabasePath"];
            if (!string.IsNullOrWhiteSpace(configPath))
            {
                // Se for relativo, resolve em relação ao executável
                if (!Path.IsPathRooted(configPath))
                    configPath = Path.Combine(AppContext.BaseDirectory, configPath);

                return configPath;
            }

            // 2) Se não tiver no config, usa a lógica padrão
            var dir = portableMode ? GetPortableDataDirectory() : GetUserDataDirectory();
            return Path.Combine(dir, "teamops.db");
        }

        public static string GetLogsDirectory(bool portableMode)
        {
            var dir = portableMode ? GetPortableDataDirectory() : GetUserDataDirectory();
            var logs = Path.Combine(dir, "logs");
            Directory.CreateDirectory(logs);
            return logs;
        }
    }
}
