using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class FormPresenceLayout : Form
    {
        private readonly SqliteConnectionFactory _factory;
        private readonly int _sectorId;
        private readonly string _sectorName;

        private readonly OperatorPresenceRepository _presenceRepo;
        private readonly OperatorPositionsRepository _positionsRepo;

        public FormPresenceLayout(int sectorId, string sectorName, SqliteConnectionFactory factory)
        {
            InitializeComponent();

            _factory = factory;
            _sectorId = sectorId;
            _sectorName = sectorName;

            _presenceRepo = new OperatorPresenceRepository(factory);
            _positionsRepo = new OperatorPositionsRepository(factory);

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewPresence.EnsureCoreWebView2Async(null);

            string folder = Path.Combine(Application.StartupPath, "ui", "presence");

            // 🔥 Libera acesso a subpastas
            webViewPresence.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "local",
                folder,
                Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow
            );

            // 🔥 Agora o JS → C# funciona
            webViewPresence.CoreWebView2.WebMessageReceived += WebMessageReceived;

            // 🔥 Carrega via domínio virtual (NÃO file:///)
            webViewPresence.Source = new Uri("https://local/index.html");
        }

        private void WebMessageReceived(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            Console.WriteLine("🔥 RECEBIDO DO JS: " + e.WebMessageAsJson);

            var msg = JsonSerializer.Deserialize<Dictionary<string, object>>(e.WebMessageAsJson);
            if (msg == null) return;

            if (msg["type"].ToString() == "filtersChanged")
            {
                DateTime date = DateTime.Parse(msg["date"].ToString());
                int shift = int.Parse(msg["shift"].ToString());

                LoadPresence(date, shift);
            }
        }

        private void LoadPresence(DateTime date, int shiftId)
        {
            Console.WriteLine(">>> SECTOR RECEBIDO NO FORM: " + _sectorId);

            // 🔥 1) Presenças
            var presences = _presenceRepo.GetLatestByDateSectorShift(date, _sectorId, shiftId);

            Console.WriteLine("DEBUG PRESENCES JSON:");
            Console.WriteLine(JsonSerializer.Serialize(presences));

            var jsonPresence = JsonSerializer.Serialize(new
            {
                type = "select_presence",
                data = presences,
                sectorName = _sectorName
            });

            webViewPresence.CoreWebView2.PostWebMessageAsJson(jsonPresence);

            // 🔥 2) Posições
            var positions = _positionsRepo.GetPositionsForSector(_sectorId);

            var jsonPositions = JsonSerializer.Serialize(new
            {
                type = "select_positions",
                data = positions
            });

            webViewPresence.CoreWebView2.PostWebMessageAsJson(jsonPositions);
        }
    }
}
