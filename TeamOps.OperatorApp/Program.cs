using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;
using TeamOps.Config;
using TeamOps.Data.Db;

namespace TeamOps.OperatorApp
{
    internal static class Program
    {
        public static DbSettings DbSettings { get; private set; } = null!;
        public static SqliteConnectionFactory ConnectionFactory { get; private set; } = null!;
        public static string AttachmentsFolder =
            ConfigurationManager.AppSettings["HikitsuguiAttachmentPath"] ?? "";

        [STAThread]
        static void Main()
        {
            // Cultura igual ao UI
            var culture = "pt-BR";
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(culture);

            ApplicationConfiguration.Initialize();

            // Lê o caminho do banco do app.config
            DbSettings = new DbSettings(portableMode: false);

            // Garante que o banco existe
            var initializer = new DbInitializer(DbSettings);
            initializer.EnsureCreated();

            // Cria a connection factory
            ConnectionFactory = new SqliteConnectionFactory(DbSettings);

            // Abre o app do operador
            Application.Run(new FormHikitsuguiOperatorRead());
        }
    }
}
