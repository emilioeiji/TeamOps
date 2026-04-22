using Microsoft.Web.WebView2.Core;
using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class HTMLFormFollowSingleReport : Form
    {
        private readonly int _followId;
        private readonly FollowUpRepository _followRepo;
        private readonly OperatorRepository _opRepo;

        public HTMLFormFollowSingleReport(
            int followId,
            FollowUpRepository followRepo,
            OperatorRepository opRepo)
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _followId = followId;
            _followRepo = followRepo;
            _opRepo = opRepo;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewFollowSingle.EnsureCoreWebView2Async(null);

            var core = webViewFollowSingle.CoreWebView2;
            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;

            core.WebMessageReceived += WebMessageReceived;

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "follow-single-report"),
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
            var follow = _followRepo.GetByIdWithJoins(_followId);
            if (follow == null)
            {
                SendNotify("Nao encontrado", "FollowUp nao encontrado.");
                return;
            }

            var op = _opRepo.GetByCodigoFJ(follow.OperatorCodigoFJ);
            var ex = _opRepo.GetByCodigoFJ(follow.ExecutorCodigoFJ);
            var wi = !string.IsNullOrWhiteSpace(follow.WitnessCodigoFJ)
                ? _opRepo.GetByCodigoFJ(follow.WitnessCodigoFJ)
                : null;

            if (op == null || ex == null)
            {
                SendNotify("Erro", "Nao foi possivel carregar os dados do operador.");
                return;
            }

            PostJson(new
            {
                type = "init",
                data = new
                {
                    id = follow.Id,
                    generatedAt = DateTime.Now.ToString("yyyy/MM/dd HH:mm"),
                    logoUrl = "https://assets/logo_rodape.png",
                    header = new
                    {
                        title = "FOLLOW-UP REPORT",
                        subtitle = "Formulario individual para impressao"
                    },
                    operatorInfo = new
                    {
                        codigoFJ = op.CodigoFJ,
                        nameRomanji = op.NameRomanji,
                        nameNihongo = op.NameNihongo,
                        startDate = op.StartDate.ToString("yyyy/MM/dd")
                    },
                    follow = new
                    {
                        date = follow.Date.ToString("yyyy/MM/dd HH:mm"),
                        shift = Safe(follow.ShiftName),
                        executor = $"{ex.NameRomanji} / {ex.NameNihongo}",
                        witness = wi != null ? $"{wi.NameRomanji} / {wi.NameNihongo}" : "-",
                        reason = Safe(follow.ReasonName),
                        type = Safe(follow.TypeName),
                        local = Safe(follow.LocalName),
                        equipment = Safe(follow.EquipmentName),
                        sector = Safe(follow.SectorName),
                        description = Safe(follow.Description),
                        guidance = Safe(follow.Guidance)
                    }
                }
            });
        }

        private async Task PrintAsync()
        {
            if (webViewFollowSingle.CoreWebView2 == null)
                return;

            await webViewFollowSingle.CoreWebView2.ExecuteScriptAsync("window.print();");
        }

        private async Task SavePdfAsync()
        {
            if (webViewFollowSingle.CoreWebView2 == null)
                return;

            using var sfd = new SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"FollowUp_{_followId}.pdf"
            };

            if (sfd.ShowDialog(this) != DialogResult.OK)
                return;

            bool ok = await webViewFollowSingle.CoreWebView2.PrintToPdfAsync(sfd.FileName);

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
                    if (webViewFollowSingle.CoreWebView2 != null)
                        webViewFollowSingle.CoreWebView2.PostWebMessageAsJson(json);
                }));
                return;
            }

            if (webViewFollowSingle.CoreWebView2 != null)
                webViewFollowSingle.CoreWebView2.PostWebMessageAsJson(json);
        }

        private static string Safe(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }
    }
}
