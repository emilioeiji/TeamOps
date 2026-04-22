using Microsoft.Web.WebView2.Core;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class HTMLFormFollowOperatorReport : Form
    {
        private readonly string _codigoFJ;
        private readonly FollowUpRepository _followRepo;
        private readonly OperatorRepository _opRepo;

        public HTMLFormFollowOperatorReport(
            string codigoFJ,
            FollowUpRepository followRepo,
            OperatorRepository opRepo,
            ShiftRepository shiftRepo,
            SectorRepository sectorRepo,
            FollowUpReasonRepository reasonRepo,
            FollowUpTypeRepository typeRepo,
            EquipmentRepository equipRepo,
            LocalRepository localRepo)
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _codigoFJ = codigoFJ;
            _followRepo = followRepo;
            _opRepo = opRepo;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewFollowOperator.EnsureCoreWebView2Async(null);

            var core = webViewFollowOperator.CoreWebView2;
            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;

            core.WebMessageReceived += WebMessageReceived;

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "follow-operator-report"),
                CoreWebView2HostResourceAccessKind.Allow
            );

            core.SetVirtualHostNameToFolderMapping(
                "assets",
                Path.Combine(Application.StartupPath, "Assets"),
                CoreWebView2HostResourceAccessKind.Allow
            );

            core.Navigate("https://app/index.html");
        }

        private async void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                using var json = JsonDocument.Parse(e.WebMessageAsJson);
                var root = json.RootElement;
                var action = root.TryGetProperty("action", out var actionProp)
                    ? actionProp.GetString() ?? ""
                    : "";

                switch (action)
                {
                    case "load":
                        LoadReport();
                        break;

                    case "print":
                        await PrintAsync();
                        break;

                    case "save_pdf":
                        await SavePdfAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                SendNotify("Erro", ex.Message);
            }
        }

        private void LoadReport()
        {
            var op = _opRepo.GetByCodigoFJ(_codigoFJ);
            if (op == null)
            {
                SendNotify("Nao encontrado", "Operador nao encontrado.");
                return;
            }

            var list = _followRepo.GetByOperatorWithJoins(_codigoFJ);
            var lastDate = list.Count == 0 ? "-" : list.Max(x => x.Date).ToString("yyyy/MM/dd HH:mm");

            PostJson(new
            {
                type = "init",
                data = new
                {
                    generatedAt = DateTime.Now.ToString("yyyy/MM/dd HH:mm"),
                    logoUrl = "https://assets/logo_rodape.png",
                    operatorInfo = new
                    {
                        codigoFJ = op.CodigoFJ,
                        nameRomanji = op.NameRomanji,
                        nameNihongo = op.NameNihongo,
                        startDate = op.StartDate.ToString("yyyy/MM/dd"),
                        total = list.Count,
                        lastDate
                    },
                    records = list.Select(x => new
                    {
                        date = x.Date.ToString("yyyy/MM/dd HH:mm"),
                        shift = Safe(x.ShiftName),
                        executor = JoinNames(x.ExecutorNamePt, x.ExecutorNameJp),
                        witness = JoinNames(x.WitnessNamePt, x.WitnessNameJp),
                        reason = Safe(x.ReasonName),
                        type = Safe(x.TypeName),
                        local = Safe(x.LocalName),
                        equipment = Safe(x.EquipmentName),
                        sector = Safe(x.SectorName),
                        description = Safe(x.Description),
                        guidance = Safe(x.Guidance)
                    }).ToList()
                }
            });
        }

        private async Task PrintAsync()
        {
            if (webViewFollowOperator.CoreWebView2 == null)
                return;

            await webViewFollowOperator.CoreWebView2.ExecuteScriptAsync("window.print();");
        }

        private async Task SavePdfAsync()
        {
            if (webViewFollowOperator.CoreWebView2 == null)
                return;

            using var sfd = new SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"FollowHistory_{_codigoFJ}.pdf"
            };

            if (sfd.ShowDialog(this) != DialogResult.OK)
                return;

            bool ok = await webViewFollowOperator.CoreWebView2.PrintToPdfAsync(sfd.FileName);

            if (ok)
                SendNotify("PDF gerado", "Arquivo PDF salvo com sucesso.");
            else
                SendNotify("Falha", "Nao foi possivel gerar o PDF.");
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
                    if (webViewFollowOperator.CoreWebView2 != null)
                        webViewFollowOperator.CoreWebView2.PostWebMessageAsJson(json);
                }));
                return;
            }

            if (webViewFollowOperator.CoreWebView2 != null)
                webViewFollowOperator.CoreWebView2.PostWebMessageAsJson(json);
        }

        private static string JoinNames(string pt, string jp)
        {
            var left = string.IsNullOrWhiteSpace(pt) ? "-" : pt;
            var right = string.IsNullOrWhiteSpace(jp) ? "-" : jp;
            return $"{left} / {right}";
        }

        private static string Safe(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }
    }
}
