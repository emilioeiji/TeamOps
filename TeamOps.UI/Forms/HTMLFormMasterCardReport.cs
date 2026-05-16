using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using TeamOps.Data.Db;
using TeamOps.UI.Services;

namespace TeamOps.UI.Forms
{
    public partial class HTMLFormMasterCardReport : Form
    {
        private readonly SqliteConnectionFactory _factory;

        public HTMLFormMasterCardReport(SqliteConnectionFactory factory)
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            Text = L("Relatorio de MasterCard", "MasterCard Report");

            _factory = factory;
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewMasterCardReport.EnsureCoreWebView2Async(null);

            var core = webViewMasterCardReport.CoreWebView2;
            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;

            core.WebMessageReceived += WebMessageReceived;
            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "mastercard-report"),
                CoreWebView2HostResourceAccessKind.Allow);

            core.Navigate("https://app/index.html");
        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                using var json = JsonDocument.Parse(e.WebMessageAsJson);
                var root = json.RootElement;
                var action = ReadString(root, "action");

                switch (action)
                {
                    case "load":
                        LoadInitial();
                        break;
                    case "apply":
                        SendRows(ReadFilter(root));
                        break;
                    case "details":
                        SendDetails(ReadInt(root, "id"));
                        break;
                }
            }
            catch (Exception ex)
            {
                PostJson(new { type = "error", message = ex.Message });
            }
        }

        private void LoadInitial()
        {
            using var conn = _factory.CreateOpenConnection();
            MasterCardModuleService.EnsureSchema(conn);

            var filter = MasterCardModuleService.CreateDefaultFilter();

            PostJson(new
            {
                type = "init",
                data = new
                {
                    locale = Program.CurrentLocale,
                    filters = new
                    {
                        statuses = MasterCardModuleService.QueryStatuses().ToList(),
                        sectors = MasterCardModuleService.QuerySectors(conn).ToList(),
                        equipments = MasterCardModuleService.QueryEquipments(conn).ToList(),
                        operators = MasterCardModuleService.QueryOperators(conn).ToList(),
                        trainers = MasterCardModuleService.QueryTrainers(conn).ToList()
                    },
                    defaults = new
                    {
                        dtInicial = filter.Start.ToString("yyyy-MM-dd"),
                        dtFinal = filter.End.ToString("yyyy-MM-dd"),
                        sectorId = 0,
                        equipmentId = 0,
                        status = "",
                        operatorCodigoFJ = "",
                        trainerCodigoFJ = "",
                        search = ""
                    }
                }
            });

            SendRows(filter);
        }

        private void SendRows(MasterCardModuleService.MasterCardReportFilter filter)
        {
            using var conn = _factory.CreateOpenConnection();
            MasterCardModuleService.EnsureSchema(conn);

            var rows = MasterCardModuleService.QueryReportRows(conn, filter);

            PostJson(new
            {
                type = "rows",
                data = new
                {
                    totals = new
                    {
                        total = rows.Count,
                        inProgress = rows.Count(item => item.Status == "in_progress"),
                        follow = rows.Count(item => item.Status == "follow"),
                        completed = rows.Count(item => item.Status == "completed"),
                        overdueFollow = rows.Count(item => item.Status == "follow" && IsPastDate(item.FollowDate)),
                        dueSoon = rows.Count(item => item.Status == "follow" && IsWithinDays(item.FollowDate, 7))
                    },
                    rows = rows.Select(ToRowPayload).ToList()
                }
            });
        }

        private void SendDetails(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            MasterCardModuleService.EnsureSchema(conn);

            var row = MasterCardModuleService.QueryReportRows(conn, new MasterCardModuleService.MasterCardReportFilter
            {
                MasterCardId = id,
                Start = DateTime.MinValue,
                End = DateTime.MaxValue
            }).FirstOrDefault();

            if (row == null)
                throw new InvalidOperationException(L("MasterCard nao encontrado.", "MasterCard が見つかりません。"));

            PostJson(new
            {
                type = "details",
                data = new
                {
                    row = ToRowPayload(row),
                    history = MasterCardModuleService.QueryHistory(conn, id)
                }
            });
        }

        private static object ToRowPayload(MasterCardModuleService.MasterCardRow row)
        {
            return new
            {
                id = row.Id,
                operatorCodigoFJ = row.OperatorCodigoFJ,
                operatorNamePt = row.OperatorNamePt,
                operatorNameJp = row.OperatorNameJp,
                trainerCodigoFJ = row.TrainerCodigoFJ,
                trainerNamePt = row.TrainerNamePt,
                trainerNameJp = row.TrainerNameJp,
                sectorNamePt = row.SectorNamePt,
                sectorNameJp = row.SectorNameJp,
                equipmentNamePt = row.EquipmentNamePt,
                equipmentNameJp = row.EquipmentNameJp,
                description = row.Description,
                notes = row.Notes,
                startDate = row.StartDate,
                masterCardStatus = row.Status,
                concludedAt = row.ConcludedAt,
                followDate = row.FollowDate,
                finalizedAt = row.FinalizedAt,
                createdAt = row.CreatedAt,
                updatedAt = row.UpdatedAt,
                createdByNamePt = row.CreatedByNamePt,
                createdByNameJp = row.CreatedByNameJp,
                historyCount = row.HistoryCount
            };
        }

        private static bool IsPastDate(string value)
        {
            return DateTime.TryParse(value, out var parsed) && parsed.Date < DateTime.Today;
        }

        private static bool IsWithinDays(string value, int days)
        {
            return DateTime.TryParse(value, out var parsed)
                   && parsed.Date >= DateTime.Today
                   && parsed.Date <= DateTime.Today.AddDays(days);
        }

        private static MasterCardModuleService.MasterCardReportFilter ReadFilter(JsonElement root)
        {
            return new MasterCardModuleService.MasterCardReportFilter
            {
                Start = ReadDate(root, "dtInicial", DateTime.Today.AddDays(-90)),
                End = ReadDate(root, "dtFinal", DateTime.Today),
                SectorId = ReadInt(root, "sectorId"),
                EquipmentId = ReadInt(root, "equipmentId"),
                Status = ReadString(root, "status"),
                OperatorCodigoFJ = ReadString(root, "operatorCodigoFJ").Trim().ToUpperInvariant(),
                TrainerCodigoFJ = ReadString(root, "trainerCodigoFJ").Trim().ToUpperInvariant(),
                Search = ReadString(root, "search")
            };
        }

        private void PostJson(object payload)
        {
            webViewMasterCardReport.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(payload));
        }

        private static DateTime ReadDate(JsonElement root, string propertyName, DateTime fallback)
        {
            var raw = ReadString(root, propertyName);
            return DateTime.TryParse(raw, out var parsed) ? parsed.Date : fallback.Date;
        }

        private static int ReadInt(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var prop))
                return 0;

            return prop.ValueKind switch
            {
                JsonValueKind.Number => prop.GetInt32(),
                JsonValueKind.String when int.TryParse(prop.GetString(), out var parsed) => parsed,
                _ => 0
            };
        }

        private static string ReadString(JsonElement root, string propertyName)
        {
            return root.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String
                ? prop.GetString() ?? string.Empty
                : string.Empty;
        }

        private static string L(string pt, string jp)
        {
            return string.Equals(Program.CurrentLocale, "ja-JP", StringComparison.OrdinalIgnoreCase)
                ? jp
                : pt;
        }
    }
}
