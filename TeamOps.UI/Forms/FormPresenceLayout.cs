using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;
using TeamOps.Services;

namespace TeamOps.UI.Forms
{
    public partial class FormPresenceLayout : Form
    {
        private readonly SqliteConnectionFactory _factory;
        private readonly int _sectorId;
        private readonly string _sectorName;

        private readonly OperatorPresenceRepository _presenceRepo;
        private readonly OperatorPositionsRepository _positionsRepo;
        private readonly OperatorScheduleRepository _scheduleRepo;
        private readonly OperatorRepository _operatorRepo;

        public FormPresenceLayout(int sectorId, string sectorName, SqliteConnectionFactory factory)
        {
            InitializeComponent();

            _factory = factory;
            _sectorId = sectorId;
            _sectorName = sectorName;

            _presenceRepo = new OperatorPresenceRepository(factory);
            _positionsRepo = new OperatorPositionsRepository(factory);
            _scheduleRepo = new OperatorScheduleRepository(factory);
            _operatorRepo = new OperatorRepository(factory);

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewPresence.EnsureCoreWebView2Async(null);

            string folder = Path.Combine(Application.StartupPath, "ui", "presence");

            // Libera acesso a subpastas
            webViewPresence.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "local",
                folder,
                Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow
            );

            webViewPresence.CoreWebView2.WebMessageReceived += WebMessageReceived;

            webViewPresence.Source = new Uri("https://local/index.html");
        }

        private void WebMessageReceived(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            var msg = JsonSerializer.Deserialize<Dictionary<string, object>>(e.WebMessageAsJson);
            if (msg == null) return;

            string type = msg["type"].ToString();

            switch (type)
            {
                case "filtersChanged":
                    {
                        DateTime date = DateTime.Parse(msg["date"].ToString());
                        int shift = int.Parse(msg["shift"].ToString());
                        LoadPresence(date, shift);
                        break;
                    }

                case "import_schedule":
                    {
                        DateTime date = DateTime.Parse(msg["date"].ToString());
                        int shift = int.Parse(msg["shift"].ToString());
                        ImportSchedule(date, shift);
                        break;
                    }
            }
        }

        private void LoadPresence(DateTime date, int shiftId)
        {
            // 1) Presenças reais
            var presences = _presenceRepo.GetLatestByDateSectorShift(date, _sectorId, shiftId);

            var jsonPresence = JsonSerializer.Serialize(new
            {
                type = "select_presence",
                data = presences,
                sectorName = _sectorName
            });

            webViewPresence.CoreWebView2.PostWebMessageAsJson(jsonPresence);

            // 2) Schedule (previsão)
            var schedule = _scheduleRepo.GetByDateShift(date, shiftId);

            var jsonSchedule = JsonSerializer.Serialize(new
            {
                type = "select_schedule",
                data = schedule
            });

            webViewPresence.CoreWebView2.PostWebMessageAsJson(jsonSchedule);

            // 3) Posições
            var positions = _positionsRepo.GetPositionsForSector(_sectorId);

            var jsonPositions = JsonSerializer.Serialize(new
            {
                type = "select_positions",
                data = positions
            });

            webViewPresence.CoreWebView2.PostWebMessageAsJson(jsonPositions);

            // 4) Operadores (para nome Romanji e foto)
            var operators = _operatorRepo.GetAll(); // ou GetBySector(_sectorId)

            var jsonOperators = JsonSerializer.Serialize(new
            {
                type = "select_operators",
                data = operators
            });

            webViewPresence.CoreWebView2.PostWebMessageAsJson(jsonOperators);
        }

        private void ImportSchedule(DateTime date, int shiftId)
        {
            try
            {
                var repo = new OperatorScheduleRepository(_factory);
                var service = new OperatorScheduleImportService(repo);

                service.Import(_sectorId, shiftId, date);

                // Envia mensagem de sucesso para o HTML
                var json = JsonSerializer.Serialize(new
                {
                    type = "import_result",
                    message = "Schedule importado com sucesso!"
                });

                webViewPresence.CoreWebView2.PostWebMessageAsJson(json);

                // Recarrega o layout
                LoadPresence(date, shiftId);
            }
            catch (Exception ex)
            {
                var json = JsonSerializer.Serialize(new
                {
                    type = "import_result",
                    message = "Erro ao importar schedule: " + ex.Message
                });

                webViewPresence.CoreWebView2.PostWebMessageAsJson(json);
            }
        }
    }
}
