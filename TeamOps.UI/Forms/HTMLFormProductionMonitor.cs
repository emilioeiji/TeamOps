using Dapper;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;
using TeamOps.Services;
using TeamOps.UI.Forms.Models;

namespace TeamOps.UI.Forms
{
    public partial class HTMLFormProductionMonitor : Form
    {
        private readonly SqliteConnectionFactory _factory;
        private readonly User _currentUser;
        private readonly Operator _currentOperator;
        private readonly ProductionAnalyticsService _analyticsService;
        private readonly ProductionFileImporter _fileImporter;

        public HTMLFormProductionMonitor(
            SqliteConnectionFactory factory,
            User currentUser,
            Operator currentOperator)
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _factory = factory;
            _currentUser = currentUser;
            _currentOperator = currentOperator;

            var machineRepository = new ProductionMachineRepository(factory);
            var eventRepository = new ProductionEventRepository(factory);

            _analyticsService = new ProductionAnalyticsService(factory);
            _fileImporter = new ProductionFileImporter(factory, machineRepository, eventRepository);

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewProductionMonitor.EnsureCoreWebView2Async(null);

            var core = webViewProductionMonitor.CoreWebView2;
            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;

            core.WebMessageReceived += WebMessageReceived;

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "production-monitor"),
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
                var action = ReadString(root, "action");

                switch (action)
                {
                    case "production_init":
                        LoadInitial();
                        break;

                    case "production_load_dashboard":
                        SendDashboard(ReadFilter(root));
                        break;

                    case "production_import":
                        _ = ImportAndRefreshAsync(ReadFilter(root));
                        break;

                    case "production_machine_detail":
                        SendMachineDetail(ReadString(root, "machineCode"));
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
            using var conn = _factory.CreateOpenConnection();
            ProductionSchemaMigrator.Ensure(conn);

            var defaultDate = DateTime.Today;
            var defaultShiftId = _currentOperator.ShiftId;

            PostJson(new
            {
                type = "init",
                data = new
                {
                    locale = Program.CurrentLocale,
                    currentUser = _currentUser.Name,
                    currentOperatorNamePt = _currentOperator.NameRomanji,
                    currentOperatorNameJp = string.IsNullOrWhiteSpace(_currentOperator.NameNihongo)
                        ? _currentOperator.NameRomanji
                        : _currentOperator.NameNihongo,
                    defaults = new
                    {
                        dateIso = defaultDate.ToString("yyyy-MM-dd"),
                        shiftId = defaultShiftId
                    },
                    shifts = conn.Query(
                        @"
                            SELECT
                                Id AS id,
                                COALESCE(NamePt, '') AS namePt,
                                COALESCE(NULLIF(NameJp, ''), NamePt, '') AS nameJp
                            FROM Shifts
                            ORDER BY Id;"
                    ),
                    sectors = conn.Query(
                        @"
                            SELECT
                                Id AS id,
                                COALESCE(NamePt, '') AS namePt,
                                COALESCE(NULLIF(NameJp, ''), NamePt, '') AS nameJp
                            FROM Sectors
                            ORDER BY Id;"
                    ),
                    locals = conn.Query(
                        @"
                            SELECT
                                l.Id AS id,
                                l.SectorId AS sectorId,
                                COALESCE(l.NamePt, '') AS namePt,
                                COALESCE(NULLIF(l.NameJp, ''), l.NamePt, '') AS nameJp
                            FROM Locals l
                            ORDER BY l.SectorId, l.Id;"
                    ),
                    machines = conn.Query(
                        @"
                            SELECT
                                m.Id AS id,
                                COALESCE(m.MachineCode, '') AS machineCode,
                                COALESCE(m.NamePt, '') AS namePt,
                                COALESCE(NULLIF(m.NameJp, ''), m.NamePt, '') AS nameJp,
                                m.SectorId AS sectorId,
                                m.LocalId AS localId
                            FROM Machines m
                            WHERE COALESCE(m.IsActive, 1) = 1
                            ORDER BY COALESCE(m.MachineCode, ''), COALESCE(m.NamePt, ''), m.Id;"
                    )
                }
            });

            SendDashboard(new ProductionDashboardFilter
            {
                Date = defaultDate,
                ShiftId = defaultShiftId
            });
        }

        private void SendDashboard(ProductionDashboardFilter filter)
        {
            var dashboard = _analyticsService.BuildDashboard(filter);

            PostJson(new
            {
                type = "dashboard",
                data = new
                {
                    locale = Program.CurrentLocale,
                    dateIso = filter.Date.ToString("yyyy-MM-dd"),
                    shiftId = filter.ShiftId,
                    sectorId = filter.SectorId,
                    localId = filter.LocalId,
                    machineCode = filter.MachineCode,
                    periodStart = dashboard.Period.Start.ToString("yyyy-MM-dd HH:mm:ss"),
                    periodEnd = dashboard.Period.End.ToString("yyyy-MM-dd HH:mm:ss"),
                    productionPercent = dashboard.ProductionPercent,
                    machinesRunning = dashboard.MachinesRunning,
                    machinesStopped = dashboard.MachinesStopped,
                    errorMinutes = dashboard.ErrorMinutes,
                    inactiveMinutes = dashboard.InactiveMinutes,
                    machines = dashboard.Machines.Select(machine => new
                    {
                        machineId = machine.MachineId,
                        machineCode = machine.MachineCode,
                        machineNamePt = machine.MachineNamePt,
                        machineNameJp = machine.MachineNameJp,
                        sectorId = machine.SectorId,
                        sectorNamePt = machine.SectorNamePt,
                        sectorNameJp = machine.SectorNameJp,
                        localId = machine.LocalId,
                        localNamePt = machine.LocalNamePt,
                        localNameJp = machine.LocalNameJp,
                        statusCode = machine.StatusCode,
                        statusText = machine.StatusText,
                        recipeName = machine.RecipeName,
                        lotNo = machine.LotNo,
                        lastUpdate = machine.LastUpdate?.ToString("yyyy-MM-dd HH:mm:ss"),
                        runningMinutes = machine.RunningMinutes,
                        stoppedMinutes = machine.StoppedMinutes,
                        inactiveMinutes = machine.InactiveMinutes,
                        errorMinutes = machine.ErrorMinutes,
                        totalMinutes = machine.TotalMinutes,
                        productionPercent = machine.ProductionPercent,
                        scheduledOperatorsPt = machine.ScheduledOperatorsPt,
                        scheduledOperatorsJp = machine.ScheduledOperatorsJp
                    }),
                    ranking = dashboard.Ranking.Select(item => new
                    {
                        localId = item.LocalId,
                        localNamePt = item.LocalNamePt,
                        localNameJp = item.LocalNameJp,
                        machineCode = item.MachineCode,
                        machineNamePt = item.MachineNamePt,
                        machineNameJp = item.MachineNameJp,
                        stopMinutes = item.StopMinutes,
                        errorMinutes = item.ErrorMinutes,
                        totalImpactMinutes = item.TotalImpactMinutes
                    }),
                    timeline = dashboard.Timeline.Select(row => new
                    {
                        localId = row.LocalId,
                        localNamePt = row.LocalNamePt,
                        localNameJp = row.LocalNameJp,
                        machineCode = row.MachineCode,
                        machineNamePt = row.MachineNamePt,
                        machineNameJp = row.MachineNameJp,
                        cells = row.Cells.Select(cell => new
                        {
                            timeLabel = cell.TimeLabel,
                            dateTime = cell.DateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            statusCode = cell.StatusCode,
                            cssClass = cell.CssClass
                        })
                    }),
                    areas = dashboard.Areas.Select(area => new
                    {
                        localId = area.LocalId,
                        sectorId = area.SectorId,
                        localNamePt = area.LocalNamePt,
                        localNameJp = area.LocalNameJp,
                        sectorNamePt = area.SectorNamePt,
                        sectorNameJp = area.SectorNameJp,
                        machineCount = area.MachineCount,
                        machinesRunning = area.MachinesRunning,
                        machinesStopped = area.MachinesStopped,
                        runningMinutes = area.RunningMinutes,
                        stoppedMinutes = area.StoppedMinutes,
                        inactiveMinutes = area.InactiveMinutes,
                        errorMinutes = area.ErrorMinutes,
                        totalMinutes = area.TotalMinutes,
                        productionPercent = area.ProductionPercent,
                        lastUpdate = area.LastUpdate?.ToString("yyyy-MM-dd HH:mm:ss"),
                        scheduledOperatorsPt = area.ScheduledOperatorsPt,
                        scheduledOperatorsJp = area.ScheduledOperatorsJp
                    }),
                    shiftComparisons = dashboard.ShiftComparisons.Select(item => new
                    {
                        shiftId = item.ShiftId,
                        shiftNamePt = item.ShiftNamePt,
                        shiftNameJp = item.ShiftNameJp,
                        start = item.Start.ToString("yyyy-MM-dd HH:mm:ss"),
                        end = item.End.ToString("yyyy-MM-dd HH:mm:ss"),
                        productionPercent = item.ProductionPercent,
                        runningMinutes = item.RunningMinutes,
                        stoppedMinutes = item.StoppedMinutes,
                        inactiveMinutes = item.InactiveMinutes,
                        errorMinutes = item.ErrorMinutes,
                        machineCount = item.MachineCount
                    }),
                    dailyTrend = dashboard.DailyTrend.Select(item => new
                    {
                        date = item.Date.ToString("yyyy-MM-dd"),
                        label = item.Label,
                        productionPercent = item.ProductionPercent,
                        runningMinutes = item.RunningMinutes,
                        stoppedMinutes = item.StoppedMinutes,
                        inactiveMinutes = item.InactiveMinutes,
                        errorMinutes = item.ErrorMinutes
                    }),
                    areaHistory = dashboard.AreaHistory.Select(item => new
                    {
                        localId = item.LocalId,
                        localNamePt = item.LocalNamePt,
                        localNameJp = item.LocalNameJp,
                        days = item.Days.Select(day => new
                        {
                            date = day.Date.ToString("yyyy-MM-dd"),
                            label = day.Label,
                            productionPercent = day.ProductionPercent,
                            runningMinutes = day.RunningMinutes,
                            stoppedMinutes = day.StoppedMinutes,
                            inactiveMinutes = day.InactiveMinutes,
                            errorMinutes = day.ErrorMinutes
                        })
                    }),
                    operatorRanking = dashboard.OperatorRanking.Select(item => new
                    {
                        operatorCodigoFJ = item.OperatorCodigoFJ,
                        operatorNamePt = item.OperatorNamePt,
                        operatorNameJp = item.OperatorNameJp,
                        estimatedRunningMinutes = item.EstimatedRunningMinutes,
                        estimatedKadouritsuPercent = item.EstimatedKadouritsuPercent,
                        localNamesPt = item.LocalNamesPt,
                        localNamesJp = item.LocalNamesJp
                    })
                }
            });
        }

        private async Task ImportAndRefreshAsync(ProductionDashboardFilter filter)
        {
            try
            {
                var result = await Task.Run(() => _fileImporter.ImportLatest());

                PostJson(new
                {
                    type = "import_result",
                    data = new
                    {
                        success = true,
                        message = BuildImportMessage(result),
                        filesRead = result.FilesRead,
                        linesRead = result.LinesRead,
                        imported = result.Imported,
                        ignored = result.Ignored,
                        machinesCreated = result.MachinesCreated,
                        errors = result.Errors
                    }
                });

                SendDashboard(filter);
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

        private void SendMachineDetail(string machineCode)
        {
            if (string.IsNullOrWhiteSpace(machineCode))
            {
                PostJson(new
                {
                    type = "machine_detail",
                    data = new
                    {
                        machineCode = string.Empty,
                        events = Array.Empty<object>()
                    }
                });
                return;
            }

            using var conn = _factory.CreateOpenConnection();
            ProductionSchemaMigrator.Ensure(conn);

            var rows = conn.Query(
                @"
                    SELECT
                        e.MachineCode AS machineCode,
                        COALESCE(m.NamePt, e.MachineCode) AS machineNamePt,
                        COALESCE(NULLIF(m.NameJp, ''), m.NamePt, e.MachineCode) AS machineNameJp,
                        substr(e.EventDateTime, 1, 16) AS eventDateTime,
                        e.StatusCode AS statusCode,
                        COALESCE(e.StatusText, '') AS statusText,
                        COALESCE(e.RecipeName, '') AS recipeName,
                        COALESCE(e.LotNo, '') AS lotNo,
                        COALESCE(e.SourceFile, '') AS sourceFile
                    FROM MachineEvents e
                    LEFT JOIN Machines m ON m.Id = e.MachineId
                    WHERE e.MachineCode = @machineCode
                    ORDER BY datetime(e.EventDateTime) DESC, e.Id DESC
                    LIMIT 80;",
                new
                {
                    machineCode = machineCode.Trim()
                }
            );

            PostJson(new
            {
                type = "machine_detail",
                data = new
                {
                    machineCode = machineCode.Trim(),
                    events = rows
                }
            });
        }

        private static string BuildImportMessage(ProductionImportResult result)
        {
            var message = $"Arquivos: {result.FilesRead} | Linhas: {result.LinesRead} | Importadas: {result.Imported} | Ignoradas: {result.Ignored}";

            if (result.MachinesCreated > 0)
            {
                message += $" | Maquinas novas: {result.MachinesCreated}";
            }

            if (result.PlanFilesRead > 0)
            {
                message += $" | DAT: {result.PlanFilesRead} arquivo(s), {result.PlanRowsImported} linha(s) de plano";
                if (result.PlanRowsIgnored > 0)
                {
                    message += $", {result.PlanRowsIgnored} ignorada(s)";
                }
            }

            if (result.BatchExecuted && !string.IsNullOrWhiteSpace(result.BatchMessage))
            {
                message += $" | BAT: {result.BatchMessage}";
            }

            return message;
        }

        private ProductionDashboardFilter ReadFilter(JsonElement root)
        {
            return new ProductionDashboardFilter
            {
                Date = ReadDate(root, "date", DateTime.Today),
                ShiftId = ReadInt(root, "shiftId", _currentOperator.ShiftId),
                SectorId = ReadInt(root, "sectorId", 0),
                LocalId = ReadInt(root, "localId", 0),
                MachineCode = ReadString(root, "machineCode").Trim()
            };
        }

        private void PostJson(object payload)
        {
            var json = JsonSerializer.Serialize(payload);

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    if (webViewProductionMonitor.CoreWebView2 != null)
                    {
                        webViewProductionMonitor.CoreWebView2.PostWebMessageAsJson(json);
                    }
                }));
                return;
            }

            if (webViewProductionMonitor.CoreWebView2 != null)
            {
                webViewProductionMonitor.CoreWebView2.PostWebMessageAsJson(json);
            }
        }

        private static string ReadString(JsonElement root, string propertyName)
        {
            return root.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String
                ? prop.GetString() ?? string.Empty
                : string.Empty;
        }

        private static int ReadInt(JsonElement root, string propertyName, int fallback = 0)
        {
            if (!root.TryGetProperty(propertyName, out var prop))
            {
                return fallback;
            }

            return prop.ValueKind switch
            {
                JsonValueKind.Number => prop.GetInt32(),
                JsonValueKind.String when int.TryParse(prop.GetString(), out var parsed) => parsed,
                _ => fallback
            };
        }

        private static DateTime ReadDate(JsonElement root, string propertyName, DateTime fallback)
        {
            if (!root.TryGetProperty(propertyName, out var prop))
            {
                return fallback.Date;
            }

            var text = prop.ValueKind == JsonValueKind.String
                ? prop.GetString()
                : null;

            return DateTime.TryParse(text, out var parsed)
                ? parsed.Date
                : fallback.Date;
        }
    }
}
