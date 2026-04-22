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
    public partial class HTMLFormFollowChart : Form
    {
        private readonly FollowUpRepository _followRepo;
        private readonly ShiftRepository _shiftRepo;
        private readonly SectorRepository _sectorRepo;
        private readonly FollowUpReasonRepository _reasonRepo;
        private readonly FollowUpTypeRepository _typeRepo;

        public HTMLFormFollowChart(
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
            _shiftRepo = shiftRepo;
            _sectorRepo = sectorRepo;
            _reasonRepo = reasonRepo;
            _typeRepo = typeRepo;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewFollowChart.EnsureCoreWebView2Async(null);

            var core = webViewFollowChart.CoreWebView2;
            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;

            core.WebMessageReceived += WebMessageReceived;

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "follow-chart"),
                CoreWebView2HostResourceAccessKind.Allow
            );

            core.Navigate("https://app/index.html");
        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string action = "";

            try
            {
                using var json = JsonDocument.Parse(e.WebMessageAsJson);
                var root = json.RootElement;
                action = root.TryGetProperty("action", out var actionProp)
                    ? actionProp.GetString() ?? ""
                    : "";

                if (action == "load")
                {
                    LoadInitial();
                    return;
                }

                if (action == "apply")
                {
                    var filter = ReadFilter(root);
                    SendDashboard(filter);
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
                        reasonId = 0,
                        typeId = 0,
                        sectorId = 0
                    }
                }
            });

            SendDashboard(filter);
        }

        private void SendDashboard(FollowFilter filter)
        {
            var list = GetFilteredRows(filter);
            var total = list.Count;
            var errorCount = list.Count(x => ContainsToken(x.TypeName, "erro"));
            var guidanceCount = list.Count(x => ContainsToken(x.TypeName, "orient"));
            var sectorCount = list
                .Select(x => x.SectorName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            PostJson(new
            {
                type = "dashboard",
                data = new
                {
                    summary = new
                    {
                        total,
                        errorCount,
                        guidanceCount,
                        sectorCount
                    },
                    periodLabel = $"{filter.Start:yyyy/MM/dd} - {filter.End:yyyy/MM/dd}",
                    charts = new
                    {
                        byType = BuildGroups(list, x => x.TypeName, 8),
                        byReason = BuildGroups(list, x => x.ReasonName, 8),
                        byOperator = BuildGroups(list, x => x.OperatorName, 10),
                        byShift = BuildGroups(list, x => x.ShiftName, 6),
                        bySector = BuildGroups(list, x => x.SectorName, 6)
                    }
                }
            });
        }

        private object BuildFilterOptions()
        {
            var shifts = _shiftRepo.GetAll()
                .Select(x => new { id = x.Id, name = x.NamePt })
                .ToList();
            shifts.Insert(0, new { id = 0, name = "Todos" });

            var reasons = _reasonRepo.GetAll()
                .Select(x => new { id = x.Id, name = x.NamePt })
                .ToList();
            reasons.Insert(0, new { id = 0, name = "Todos" });

            var types = _typeRepo.GetAll()
                .Select(x => new { id = x.Id, name = x.NamePt })
                .ToList();
            types.Insert(0, new { id = 0, name = "Todos" });

            var sectors = _sectorRepo.GetAll()
                .Select(x => new { id = x.Id, name = x.NamePt })
                .ToList();
            sectors.Insert(0, new { id = 0, name = "Todos" });

            return new
            {
                shifts,
                reasons,
                types,
                sectors
            };
        }

        private List<FollowUp> GetFilteredRows(FollowFilter filter)
        {
            var endExclusive = filter.End.Date.AddDays(1);
            var list = _followRepo.GetByPeriod(filter.Start.Date, endExclusive);

            if (filter.ShiftId > 0)
                list = list.Where(x => x.ShiftId == filter.ShiftId).ToList();

            if (filter.ReasonId > 0)
                list = list.Where(x => x.ReasonId == filter.ReasonId).ToList();

            if (filter.TypeId > 0)
                list = list.Where(x => x.TypeId == filter.TypeId).ToList();

            if (filter.SectorId > 0)
                list = list.Where(x => x.SectorId == filter.SectorId).ToList();

            return list;
        }

        private static List<object> BuildGroups(
            IEnumerable<FollowUp> list,
            Func<FollowUp, string?> selector,
            int take)
        {
            return list
                .GroupBy(x =>
                {
                    var value = selector(x);
                    return string.IsNullOrWhiteSpace(value) ? "Nao informado" : value.Trim();
                })
                .Select(g => new
                {
                    label = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(x => x.count)
                .ThenBy(x => x.label)
                .Take(take)
                .Cast<object>()
                .ToList();
        }

        private static bool ContainsToken(string? value, string token)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
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
            filter.ReasonId = ReadInt(root, "reasonId");
            filter.TypeId = ReadInt(root, "typeId");
            filter.SectorId = ReadInt(root, "sectorId");

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

        private void PostJson(object payload)
        {
            var json = JsonSerializer.Serialize(payload);

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    if (webViewFollowChart.CoreWebView2 != null)
                        webViewFollowChart.CoreWebView2.PostWebMessageAsJson(json);
                }));
                return;
            }

            if (webViewFollowChart.CoreWebView2 != null)
                webViewFollowChart.CoreWebView2.PostWebMessageAsJson(json);
        }

        private sealed class FollowFilter
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public int ShiftId { get; set; }
            public int ReasonId { get; set; }
            public int TypeId { get; set; }
            public int SectorId { get; set; }
        }
    }
}
