using Microsoft.Web.WebView2.Core;
using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class HTMLFormReports : Form
    {
        private readonly Operator _currentOperator;
        private readonly Shift _currentShift;
        private readonly HikitsuguiRepository _hikRepo;
        private readonly HikitsuguiReadRepository _readRepo;
        private readonly OperatorRepository _opRepo;
        private readonly SqliteConnectionFactory _factory;

        public HTMLFormReports(
            Operator currentOperator,
            Shift currentShift,
            HikitsuguiRepository hikRepo,
            HikitsuguiReadRepository readRepo,
            OperatorRepository opRepo,
            SqliteConnectionFactory factory)
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _currentOperator = currentOperator;
            _currentShift = currentShift;
            _hikRepo = hikRepo;
            _readRepo = readRepo;
            _opRepo = opRepo;
            _factory = factory;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewReports.EnsureCoreWebView2Async(null);

            var core = webViewReports.CoreWebView2;

            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;

            core.WebMessageReceived += WebMessageReceived;

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "reports"),
                CoreWebView2HostResourceAccessKind.Allow
            );

            core.Navigate("https://app/index.html");

            core.NavigationCompleted += (_, __) =>
            {
                PostJson(new
                {
                    type = "init",
                    data = new
                    {
                        operatorName = _currentOperator.NameRomanji,
                        shiftName = _currentShift.NamePt,
                        date = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                        availableCount = 3,
                        totalCount = 7
                    }
                });
            };
        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string? action = null;

            try
            {
                using var json = JsonDocument.Parse(e.WebMessageAsJson);
                var root = json.RootElement;

                if (root.ValueKind == JsonValueKind.String)
                {
                    action = root.GetString();
                }
                else if (root.ValueKind == JsonValueKind.Object &&
                         root.TryGetProperty("action", out var actionProp))
                {
                    action = actionProp.GetString();
                }
            }
            catch
            {
                action = null;
            }

            if (string.IsNullOrWhiteSpace(action))
                return;

            switch (action)
            {
                case "open:hikitsugui":
                    OpenDialog(() =>
                    {
                        var shiftRepo = new ShiftRepository(_factory);
                        var sectorRepo = new SectorRepository(_factory);

                        return new FormHikitsuguiReader(
                            _hikRepo,
                            _readRepo,
                            _opRepo,
                            shiftRepo,
                            sectorRepo
                        );
                    });
                    break;

                case "open:follow_report":
                    OpenDialog(() => new HTMLFormFollowReport(
                        new FollowUpRepository(_factory),
                        new OperatorRepository(_factory),
                        new ShiftRepository(_factory),
                        new SectorRepository(_factory),
                        new FollowUpReasonRepository(_factory),
                        new FollowUpTypeRepository(_factory),
                        new EquipmentRepository(_factory),
                        new LocalRepository(_factory)
                    ));
                    break;

                case "open:follow_chart":
                    OpenDialog(() => new HTMLFormFollowChart(
                        new FollowUpRepository(_factory),
                        new OperatorRepository(_factory),
                        new ShiftRepository(_factory),
                        new SectorRepository(_factory),
                        new FollowUpReasonRepository(_factory),
                        new FollowUpTypeRepository(_factory),
                        new EquipmentRepository(_factory),
                        new LocalRepository(_factory)
                    ));
                    break;

                case "todo:operadores":
                    SendNotify("Em desenvolvimento", "O relatório de Operadores ainda não foi migrado.");
                    break;

                case "todo:pr":
                    SendNotify("Em desenvolvimento", "O relatório de PR ainda não foi migrado.");
                    break;

                case "todo:cl":
                    SendNotify("Em desenvolvimento", "O relatório de CL ainda não foi migrado.");
                    break;

                case "todo:sobra":
                    SendNotify("Em desenvolvimento", "O relatório de Sobra de Peça ainda não foi migrado.");
                    break;
            }
        }

        private void OpenDialog(Func<Form> factory)
        {
            if (IsDisposed)
                return;

            BeginInvoke(new Action(() =>
            {
                if (IsDisposed)
                    return;

                using var form = factory();
                form.ShowDialog(this);
            }));
        }

        private void SendNotify(string title, string message)
        {
            PostJson(new
            {
                type = "notify",
                data = new
                {
                    title,
                    message
                }
            });
        }

        private void PostJson(object payload)
        {
            var json = JsonSerializer.Serialize(payload);

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    if (webViewReports.CoreWebView2 != null)
                        webViewReports.CoreWebView2.PostWebMessageAsJson(json);
                }));

                return;
            }

            if (webViewReports.CoreWebView2 != null)
                webViewReports.CoreWebView2.PostWebMessageAsJson(json);
        }
    }
}
