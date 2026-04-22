using ClosedXML.Excel;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class HTMLFormFollowReport : Form
    {
        private readonly FollowUpRepository _followRepo;
        private readonly OperatorRepository _opRepo;
        private readonly ShiftRepository _shiftRepo;
        private readonly SectorRepository _sectorRepo;
        private readonly FollowUpReasonRepository _reasonRepo;
        private readonly FollowUpTypeRepository _typeRepo;

        public HTMLFormFollowReport(
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

            _followRepo = followRepo;
            _opRepo = opRepo;
            _shiftRepo = shiftRepo;
            _sectorRepo = sectorRepo;
            _reasonRepo = reasonRepo;
            _typeRepo = typeRepo;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewFollowReport.EnsureCoreWebView2Async(null);

            var core = webViewFollowReport.CoreWebView2;
            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;

            core.WebMessageReceived += WebMessageReceived;

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "follow-report"),
                CoreWebView2HostResourceAccessKind.Allow
            );

            core.Navigate("https://app/index.html");
        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
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
                        LoadInitial();
                        break;

                    case "apply":
                        SendRows(ReadFilter(root));
                        break;

                    case "export":
                        ExportRows(ReadFilter(root));
                        break;

                    case "print":
                        OpenPrint(ReadInt(root, "id"));
                        break;

                    case "operator_report":
                        OpenOperatorReport(ReadString(root, "codigoFJ"));
                        break;
                }
            }
            catch (Exception ex)
            {
                PostJson(new
                {
                    type = "error",
                    message = ex.Message
                });
            }
        }

        private void LoadInitial()
        {
            var filter = CreateDefaultFilter();

            PostJson(new
            {
                type = "init",
                data = new
                {
                    filters = BuildFilterOptions(),
                    defaults = new
                    {
                        dtInicial = filter.Start.ToString("yyyy-MM-dd"),
                        dtFinal = filter.End.ToString("yyyy-MM-dd"),
                        shiftId = 0,
                        typeId = 0,
                        reasonId = 0,
                        sectorId = 0,
                        search = ""
                    }
                }
            });

            SendRows(filter);
        }

        private void SendRows(FollowFilter filter)
        {
            var rows = GetFilteredRows(filter)
                .Select(x => new
                {
                    id = x.Id,
                    operatorCodigoFJ = x.OperatorCodigoFJ,
                    date = x.Date.ToString("yyyy/MM/dd HH:mm"),
                    shiftName = Safe(x.ShiftName),
                    operatorName = Safe(x.OperatorName),
                    executorName = Safe(x.ExecutorName),
                    witnessName = string.IsNullOrWhiteSpace(x.WitnessName) ? "-" : x.WitnessName,
                    reasonName = Safe(x.ReasonName),
                    typeName = Safe(x.TypeName),
                    localName = Safe(x.LocalName),
                    equipmentName = Safe(x.EquipmentName),
                    sectorName = Safe(x.SectorName),
                    description = Safe(x.Description),
                    guidance = Safe(x.Guidance)
                })
                .ToList();

            PostJson(new
            {
                type = "rows",
                data = new
                {
                    total = rows.Count,
                    rows
                }
            });
        }

        private object BuildFilterOptions()
        {
            var shifts = _shiftRepo.GetAll()
                .Select(x => new { id = x.Id, name = x.NamePt })
                .ToList();
            shifts.Insert(0, new { id = 0, name = "Todos" });

            var types = _typeRepo.GetAll()
                .Select(x => new { id = x.Id, name = x.NamePt })
                .ToList();
            types.Insert(0, new { id = 0, name = "Todos" });

            var reasons = _reasonRepo.GetAll()
                .Select(x => new { id = x.Id, name = x.NamePt })
                .ToList();
            reasons.Insert(0, new { id = 0, name = "Todos" });

            var sectors = _sectorRepo.GetAll()
                .Select(x => new { id = x.Id, name = x.NamePt })
                .ToList();
            sectors.Insert(0, new { id = 0, name = "Todos" });

            return new
            {
                shifts,
                types,
                reasons,
                sectors
            };
        }

        private List<FollowUp> GetFilteredRows(FollowFilter filter)
        {
            var list = _followRepo.GetByPeriod(filter.Start.Date, filter.End.Date.AddDays(1));

            if (filter.ShiftId > 0)
                list = list.Where(x => x.ShiftId == filter.ShiftId).ToList();

            if (filter.TypeId > 0)
                list = list.Where(x => x.TypeId == filter.TypeId).ToList();

            if (filter.ReasonId > 0)
                list = list.Where(x => x.ReasonId == filter.ReasonId).ToList();

            if (filter.SectorId > 0)
                list = list.Where(x => x.SectorId == filter.SectorId).ToList();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.Trim();
                list = list.Where(x => MatchesSearch(x, term)).ToList();
            }

            return list;
        }

        private void ExportRows(FollowFilter filter)
        {
            var rows = GetFilteredRows(filter);
            if (rows.Count == 0)
            {
                SendNotify("Nada para exportar", "Nao ha registros para os filtros informados.");
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = $"FollowReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };

            if (sfd.ShowDialog(this) != DialogResult.OK)
                return;

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("FollowReport");

            string[] headers =
            {
                "Id",
                "Data",
                "Turno",
                "Operador",
                "Executor",
                "Testemunha",
                "Motivo",
                "Tipo",
                "Local",
                "Equipamento",
                "Setor",
                "Descricao",
                "Orientacao"
            };

            for (int i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];

            int rowIndex = 2;
            foreach (var item in rows)
            {
                ws.Cell(rowIndex, 1).Value = item.Id;
                ws.Cell(rowIndex, 2).Value = item.Date.ToString("yyyy/MM/dd HH:mm");
                ws.Cell(rowIndex, 3).Value = Safe(item.ShiftName);
                ws.Cell(rowIndex, 4).Value = Safe(item.OperatorName);
                ws.Cell(rowIndex, 5).Value = Safe(item.ExecutorName);
                ws.Cell(rowIndex, 6).Value = Safe(item.WitnessName);
                ws.Cell(rowIndex, 7).Value = Safe(item.ReasonName);
                ws.Cell(rowIndex, 8).Value = Safe(item.TypeName);
                ws.Cell(rowIndex, 9).Value = Safe(item.LocalName);
                ws.Cell(rowIndex, 10).Value = Safe(item.EquipmentName);
                ws.Cell(rowIndex, 11).Value = Safe(item.SectorName);
                ws.Cell(rowIndex, 12).Value = Safe(item.Description);
                ws.Cell(rowIndex, 13).Value = Safe(item.Guidance);
                rowIndex++;
            }

            ws.Columns().AdjustToContents();
            wb.SaveAs(sfd.FileName);

            SendNotify("Exportacao concluida", "Arquivo XLSX gerado com sucesso.");
        }

        private void OpenPrint(int id)
        {
            if (id <= 0)
                return;

            OpenDialog(() => new HTMLFormFollowSingleReport(
                id,
                _followRepo,
                _opRepo
            ));
        }

        private void OpenOperatorReport(string codigoFJ)
        {
            if (string.IsNullOrWhiteSpace(codigoFJ))
                return;

            OpenDialog(() => new HTMLFormFollowOperatorReport(
                codigoFJ,
                _followRepo,
                _opRepo,
                _shiftRepo,
                _sectorRepo,
                _reasonRepo,
                _typeRepo,
                new EquipmentRepository(Program.ConnectionFactory),
                new LocalRepository(Program.ConnectionFactory)
            ));
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
                    if (webViewFollowReport.CoreWebView2 != null)
                        webViewFollowReport.CoreWebView2.PostWebMessageAsJson(json);
                }));
                return;
            }

            if (webViewFollowReport.CoreWebView2 != null)
                webViewFollowReport.CoreWebView2.PostWebMessageAsJson(json);
        }

        private static FollowFilter CreateDefaultFilter()
        {
            return new FollowFilter
            {
                Start = DateTime.Today.AddMonths(-1),
                End = DateTime.Today
            };
        }

        private static FollowFilter ReadFilter(JsonElement root)
        {
            var filter = CreateDefaultFilter();

            if (root.TryGetProperty("dtInicial", out var startProp) &&
                DateTime.TryParse(startProp.GetString(), out var start))
            {
                filter.Start = start;
            }

            if (root.TryGetProperty("dtFinal", out var endProp) &&
                DateTime.TryParse(endProp.GetString(), out var end))
            {
                filter.End = end;
            }

            filter.ShiftId = ReadInt(root, "shiftId");
            filter.TypeId = ReadInt(root, "typeId");
            filter.ReasonId = ReadInt(root, "reasonId");
            filter.SectorId = ReadInt(root, "sectorId");
            filter.Search = root.TryGetProperty("search", out var searchProp)
                ? searchProp.GetString() ?? ""
                : "";

            return filter;
        }

        private static int ReadInt(JsonElement root, string name)
        {
            if (!root.TryGetProperty(name, out var prop))
                return 0;

            if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var value))
                return value;

            if (prop.ValueKind == JsonValueKind.String &&
                int.TryParse(prop.GetString(), out value))
                return value;

            return 0;
        }

        private static string ReadString(JsonElement root, string name)
        {
            return root.TryGetProperty(name, out var prop)
                ? prop.GetString() ?? ""
                : "";
        }

        private static bool MatchesSearch(FollowUp item, string term)
        {
            return Contains(item.OperatorName, term) ||
                   Contains(item.ExecutorName, term) ||
                   Contains(item.WitnessName, term) ||
                   Contains(item.ReasonName, term) ||
                   Contains(item.TypeName, term) ||
                   Contains(item.LocalName, term) ||
                   Contains(item.EquipmentName, term) ||
                   Contains(item.SectorName, term) ||
                   Contains(item.Description, term) ||
                   Contains(item.Guidance, term);
        }

        private static bool Contains(string? value, string term)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string Safe(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }

        private sealed class FollowFilter
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public int ShiftId { get; set; }
            public int TypeId { get; set; }
            public int ReasonId { get; set; }
            public int SectorId { get; set; }
            public string Search { get; set; } = "";
        }
    }
}
