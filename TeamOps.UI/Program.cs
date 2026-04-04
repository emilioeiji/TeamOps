// Project: TeamOps.UI
// File: Program.cs
using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using TeamOps.Config;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;
using TeamOps.Core.Entities;

namespace TeamOps.UI
{
    internal static class Program
    {
        public static DbSettings DbSettings { get; private set; } = null!;
        public static SqliteConnectionFactory ConnectionFactory { get; private set; } = null!;
        public static User? CurrentUser { get; set; }

        [STAThread]
        private static void Main()
        {
            var culture = "pt-BR";
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(culture);

            ApplicationConfiguration.Initialize();

            DbSettings = new DbSettings(portableMode: false);
            var initializer = new DbInitializer(DbSettings);
            initializer.EnsureCreated();

            ConnectionFactory = new SqliteConnectionFactory(DbSettings);
            TeamOps.Data.DbSeeder.SeedDefaultAdmin(ConnectionFactory);

            using (var loginForm = new Forms.FormLogin())
            {
                if (loginForm.ShowDialog() == DialogResult.OK && Program.CurrentUser != null)
                {
                    Application.Run(new Forms.FormDashboard(Program.CurrentUser));
                }
            }
        }
    }
}
