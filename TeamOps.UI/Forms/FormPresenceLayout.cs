using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Dapper;
using Microsoft.Web.WebView2.Core;
using TeamOps.Data.Db;

namespace TeamOps.UI.Forms
{
    public partial class FormPresenceLayout : Form
    {
        private readonly SqliteConnectionFactory _factory;
        private readonly int _sectorId;
        private readonly string _sectorName;

        public FormPresenceLayout(int sectorId, string sectorName, SqliteConnectionFactory factory)
        {
            InitializeComponent();

            _factory = factory;
            _sectorId = sectorId;
            _sectorName = sectorName;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewPresence.EnsureCoreWebView2Async();

            string folder = Path.Combine(Application.StartupPath, "ui", "presence");

            // 1) Registrar domínio virtual ANTES de navegar
            webViewPresence.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app",
                folder,
                CoreWebView2HostResourceAccessKind.Allow
            );

            // 2) Registrar eventos
            webViewPresence.CoreWebView2.WebMessageReceived += WebMessageReceived;

            // 3) Agora sim navegar
            webViewPresence.Source = new Uri("https://app/index.html");

            // 4) Debug
            webViewPresence.NavigationCompleted += (s, e) =>
            {
                Console.WriteLine("WebView2 pronto.");
            };
        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            Console.WriteLine("🔥 RECEBIDO DO JS: " + e.WebMessageAsJson);
            var msg = JsonSerializer.Deserialize<Dictionary<string, object>>(e.WebMessageAsJson);
            if (msg == null) return;

            var type = msg["type"].ToString();

            if (type == "filtersChanged")
            {
                string date = msg["date"].ToString();
                int shift = int.Parse(msg["shift"].ToString());

                LoadPresence(DateTime.Parse(date), shift);
            }
        }

        private void LoadPresence(DateTime date, int shiftId)
        {
            // 1) Presenças (com nome Romanji e Nihongo)
            SendJsonFromSql("select_presence.sql", new
            {
                Date = date.ToString("yyyy-MM-dd"),
                SectorId = _sectorId,
                ShiftId = shiftId
            });

            // 2) Posições do mapa
            SendJsonFromSql("select_positions.sql", new
            {
                SectorId = _sectorId
            });
        }

        private void SendJsonFromSql(string sqlFile, object? param = null)
        {
            var sqlPath = Path.Combine(
                Application.StartupPath,
                "Sql", "presence", sqlFile);

            var sql = File.ReadAllText(sqlPath);

            using var conn = _factory.CreateOpenConnection();
            var rows = conn.Query(sql, param);

            var json = JsonSerializer.Serialize(new
            {
                type = Path.GetFileNameWithoutExtension(sqlFile),
                data = rows,
                sectorName = _sectorName
            });

            Console.WriteLine("🔥 ENVIANDO PARA JS: " + json);
            webViewPresence.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void ExecuteSql(string sqlFile, object param)
        {
            var sqlPath = Path.Combine(
                Application.StartupPath,
                "Sql", "presence", sqlFile);

            var sql = File.ReadAllText(sqlPath);

            using var conn = _factory.CreateOpenConnection();
            conn.Execute(sql, param);
        }
    }
}
