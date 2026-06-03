using Dapper;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;
using TeamOps.Services;
using TeamOps.UI.Forms.Models;
using TeamOps.UI.Services;

namespace TeamOps.UI.Forms
{
    public partial class HTMLFormProductionMonitor : Form
    {
        private readonly SqliteConnectionFactory _factory;
        private readonly User _currentUser;
        private readonly Operator _currentOperator;
        private readonly ProductionAnalyticsService _analyticsService;
        private readonly GBareruCapacityForecastService _forecastService;
        private readonly ProductionFileImporter _fileImporter;
        private readonly SemaphoreSlim _databaseOperationGate = new(1, 1);
        private volatile bool _isImportingProduction;

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
            _forecastService = new GBareruCapacityForecastService(factory);
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
                        _ = RunAsync(LoadInitialAsync());
                        break;

                    case "production_load_dashboard":
                        if (!_isImportingProduction)
                        {
                            _ = RunAsync(SendDashboardAsync(ReadFilter(root)));
                        }
                        break;

                    case "production_import":
                        _ = ImportAndRefreshAsync(ReadFilter(root));
                        break;

                    case "production_machine_detail":
                        if (!_isImportingProduction)
                        {
                            _ = RunAsync(SendMachineDetailAsync(ReadInt(root, "machineId", 0)));
                        }
                        break;

                    case "production_operator_detail":
                        if (!_isImportingProduction)
                        {
                            _ = RunAsync(SendOperatorDetailAsync(ReadFilter(root), ReadString(root, "operatorCodigoFJ")));
                        }
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

        private async Task RunAsync(Task task)
        {
            try
            {
                await task;
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

        private async Task LoadInitialAsync()
        {
            var defaultDate = DateTime.Today;
            var defaultShiftId = _currentOperator.ShiftId;

            await _databaseOperationGate.WaitAsync();
            try
            {
                var initPayload = await Task.Run(() =>
                {
                    using var conn = _factory.CreateOpenConnection();
                    ProductionSchemaMigrator.Ensure(conn);

                    return new
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
                            ).ToList(),
                            sectors = conn.Query(
                                @"
                                    SELECT
                                        Id AS id,
                                        COALESCE(NamePt, '') AS namePt,
                                        COALESCE(NULLIF(NameJp, ''), NamePt, '') AS nameJp
                                    FROM Sectors
                                    ORDER BY Id;"
                            ).ToList(),
                            locals = conn.Query(
                                @"
                                    SELECT
                                        l.Id AS id,
                                        l.SectorId AS sectorId,
                                        COALESCE(l.NamePt, '') AS namePt,
                                        COALESCE(NULLIF(l.NameJp, ''), l.NamePt, '') AS nameJp
                                    FROM Locals l
                                    ORDER BY l.SectorId, l.Id;"
                            ).ToList(),
                            machines = conn.Query(
                                @"
                                    SELECT
                                        m.Id AS id,
                                        COALESCE(m.MachineCode, '') AS machineCode,
                                        COALESCE(m.LineCode, '') AS lineCode,
                                        COALESCE(m.NamePt, '') AS namePt,
                                        COALESCE(NULLIF(m.NameJp, ''), m.NamePt, '') AS nameJp,
                                        m.SectorId AS sectorId,
                                        m.LocalId AS localId
                                    FROM Machines m
                                    WHERE COALESCE(m.IsActive, 1) = 1
                                    ORDER BY COALESCE(m.SectorId, 0), COALESCE(m.LocalId, 0), COALESCE(m.LineCode, ''), COALESCE(m.MachineCode, ''), COALESCE(m.NamePt, ''), m.Id;"
                            ).ToList(),
                            statuses = LoadStatuses(conn)
                        }
                    };
                });

                PostJson(initPayload);
            }
            finally
            {
                _databaseOperationGate.Release();
            }

            await SendDashboardAsync(new ProductionDashboardFilter
            {
                Date = defaultDate,
                ShiftId = defaultShiftId
            });
        }

        private async Task SendDashboardAsync(ProductionDashboardFilter filter)
        {
            await _databaseOperationGate.WaitAsync();
            try
            {
                var dashboard = await Task.Run(() =>
                {
                    var builtDashboard = _analyticsService.BuildDashboard(filter);
                    builtDashboard.GBareruCapacityForecast = _forecastService.BuildForecast(filter, builtDashboard);
                    return builtDashboard;
                });

                LogDashboardDiagnostics(dashboard);

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
                        machineId = filter.MachineId,
                        machineCode = filter.MachineCode,
                        periodStart = dashboard.Period.Start.ToString("yyyy-MM-dd HH:mm:ss"),
                        periodEnd = dashboard.Period.End.ToString("yyyy-MM-dd HH:mm:ss"),
                        productionPercent = dashboard.ProductionPercent,
                        machinesRunning = dashboard.MachinesRunning,
                        machinesStopped = dashboard.MachinesStopped,
                        machinesIgnored = dashboard.MachinesIgnored,
                        machinesTotal = dashboard.MachinesTotal,
                        averageOperatingProcessMinutes = dashboard.AverageOperatingProcessMinutes,
                        errorMinutes = dashboard.ErrorMinutes,
                        inactiveMinutes = dashboard.InactiveMinutes,
                        machines = dashboard.Machines.Select(machine => new
                        {
                            machineId = machine.MachineId,
                            machineCode = machine.MachineCode,
                            machine = machine.Machine,
                            lineCode = machine.LineCode,
                            machineNamePt = machine.MachineNamePt,
                            machineNameJp = machine.MachineNameJp,
                            sectorId = machine.SectorId,
                            sectorNamePt = machine.SectorNamePt,
                            sectorNameJp = machine.SectorNameJp,
                            localId = machine.LocalId,
                            area = machine.Area,
                            localNamePt = machine.LocalNamePt,
                            localNameJp = machine.LocalNameJp,
                            statusCode = machine.StatusCode,
                            displayCode = machine.DisplayCode,
                            statusText = machine.StatusText,
                            recipeName = machine.RecipeName,
                            lotNo = machine.LotNo,
                            ec2StatusText = machine.Ec2StatusText,
                            ec2Status = machine.Ec2Status,
                            ec2PartCode = machine.Ec2PartCode,
                            partCode = machine.PartCode,
                            ec2PartColorHex = machine.Ec2PartColorHex,
                            partCodeColorHex = machine.PartCodeColorHex,
                            ec2PartTextColorHex = machine.Ec2PartTextColorHex,
                            partCodeTextColorHex = machine.PartCodeTextColorHex,
                            partCodeDescription = machine.PartCodeDescription,
                            ec2IgnoreReason = machine.Ec2IgnoreReason,
                            isEc2Running = machine.IsEc2Running,
                            isEc2Ignored = machine.IsEc2Ignored,
                            ec2ProcessMinutes = machine.Ec2ProcessMinutes,
                            ec2SettingRate = machine.Ec2SettingRate,
                            ec2OperationRate = machine.Ec2OperationRate,
                            ec2SnapshotAt = machine.Ec2SnapshotAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                            lastUpdate = machine.LastUpdate?.ToString("yyyy-MM-dd HH:mm:ss"),
                            runningMinutes = machine.RunningMinutes,
                            stoppedMinutes = machine.StoppedMinutes,
                            inactiveMinutes = machine.InactiveMinutes,
                            errorMinutes = machine.ErrorMinutes,
                            totalMinutes = machine.TotalMinutes,
                            productionPercent = machine.ProductionPercent,
                            enteredAreaAverage = machine.EnteredAreaAverage,
                            areaAverageReason = machine.AreaAverageReason,
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
                            sectorId = row.SectorId,
                            localNamePt = row.LocalNamePt,
                            localNameJp = row.LocalNameJp,
                            machineCode = row.MachineCode,
                            lineCode = row.LineCode,
                            machineNamePt = row.MachineNamePt,
                            machineNameJp = row.MachineNameJp,
                            cells = row.Cells.Select(cell => new
                            {
                                timeLabel = cell.TimeLabel,
                                dateTime = cell.DateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                statusCode = cell.StatusCode,
                                displayCode = cell.DisplayCode,
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
                            machinesIgnored = area.MachinesIgnored,
                            averageOperatingProcessMinutes = area.AverageOperatingProcessMinutes,
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
                            shiftId = item.ShiftId,
                            shiftNamePt = item.ShiftNamePt,
                            shiftNameJp = item.ShiftNameJp,
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
                        }),
                        gBareruCapacityForecast = MapForecast(dashboard.GBareruCapacityForecast),
                        statuses = LoadStatuses(),
                        partCodeStyles = LoadPartCodeStyles()
                    }
                });
            }
            finally
            {
                _databaseOperationGate.Release();
            }
        }

        private async Task ImportAndRefreshAsync(ProductionDashboardFilter filter)
        {
            if (_isImportingProduction)
            {
                return;
            }

            _isImportingProduction = true;
            try
            {
                await _databaseOperationGate.WaitAsync();
                ProductionImportResult result;
                try
                {
                    result = await Task.Run(() => _fileImporter.ImportLatest());
                }
                finally
                {
                    _databaseOperationGate.Release();
                }

                var dashboardRefreshWatch = Stopwatch.StartNew();
                await SendDashboardAsync(ResolvePostImportDashboardFilter(filter));
                dashboardRefreshWatch.Stop();
                result.PerformanceMs["DashboardRefresh"] = dashboardRefreshWatch.ElapsedMilliseconds;

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
                        ec2ImportAttempted = result.Ec2ImportAttempted,
                        ec2ImportSkipped = result.Ec2ImportSkipped,
                        ec2ImportMessage = result.Ec2ImportMessage,
                        ec2RowsRead = result.Ec2RowsRead,
                        ec2RowsImported = result.Ec2RowsImported,
                        ec2RowsIgnored = result.Ec2RowsIgnored,
                        performanceMs = result.PerformanceMs,
                        errors = result.Errors
                    }
                });
            }
            catch (Exception ex)
            {
                PostJson(new
                {
                    type = "error",
                    message = ex.Message
                });
            }
            finally
            {
                _isImportingProduction = false;
            }
        }

        private async Task SendMachineDetailAsync(int machineId)
        {
            if (machineId <= 0)
            {
                PostJson(new
                {
                    type = "machine_detail",
                    data = new
                    {
                        machineId = 0,
                        machineCode = string.Empty,
                        lineCode = string.Empty,
                        events = Array.Empty<object>()
                    }
                });
                return;
            }

            await _databaseOperationGate.WaitAsync();
            try
            {
                var rows = await Task.Run(() =>
                {
                    using var conn = _factory.CreateOpenConnection();
                    ProductionSchemaMigrator.Ensure(conn);

                    return conn.Query(
                        @"
                            SELECT
                                e.MachineId AS machineId,
                                e.SectorId AS sectorId,
                                e.MachineCode AS machineCode,
                                COALESCE(e.LineCode, '') AS lineCode,
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
                            WHERE e.MachineId = @machineId
                            ORDER BY e.EventDateTime DESC, e.Id DESC
                            LIMIT 80;",
                        new
                        {
                            machineId
                        }
                    ).ToList();
                });

                PostJson(new
                {
                    type = "machine_detail",
                    data = new
                    {
                        machineId,
                        machineCode = rows.FirstOrDefault()?.machineCode ?? string.Empty,
                        lineCode = rows.FirstOrDefault()?.lineCode ?? string.Empty,
                        events = rows
                    }
                });
            }
            finally
            {
                _databaseOperationGate.Release();
            }
        }

        private async Task SendOperatorDetailAsync(ProductionDashboardFilter filter, string operatorCodigoFJ)
        {
            await _databaseOperationGate.WaitAsync();
            try
            {
                var detail = await Task.Run(() => _analyticsService.GetOperatorDetail(filter, operatorCodigoFJ));

                PostJson(new
                {
                    type = "operator_detail",
                    data = new
                    {
                        operatorCodigoFJ = detail.OperatorCodigoFJ,
                        operatorNamePt = detail.OperatorNamePt,
                        operatorNameJp = detail.OperatorNameJp,
                        shiftNamePt = detail.ShiftNamePt,
                        shiftNameJp = detail.ShiftNameJp,
                        averageKadouritsuPercent = detail.AverageKadouritsuPercent,
                        totalRunningMinutes = detail.TotalRunningMinutes,
                        assignedAreaCount = detail.AssignedAreaCount,
                        localNamesPt = detail.LocalNamesPt,
                        localNamesJp = detail.LocalNamesJp,
                        entries = detail.Entries.Select(entry => new
                        {
                            date = entry.Date.ToString("yyyy-MM-dd"),
                            label = entry.Label,
                            localId = entry.LocalId,
                            localNamePt = entry.LocalNamePt,
                            localNameJp = entry.LocalNameJp,
                            runningMinutes = entry.RunningMinutes,
                            stoppedMinutes = entry.StoppedMinutes,
                            inactiveMinutes = entry.InactiveMinutes,
                            errorMinutes = entry.ErrorMinutes,
                            eligibleMinutes = entry.EligibleMinutes,
                            kadouritsuPercent = entry.KadouritsuPercent,
                            coverageMode = entry.CoverageMode,
                            isPartialCoverage = entry.IsPartialCoverage,
                            effectiveMinutes = entry.EffectiveMinutes,
                            plannedMinutes = entry.PlannedMinutes
                        })
                    }
                });
            }
            finally
            {
                _databaseOperationGate.Release();
            }
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

            if (result.Ec2ImportAttempted || !string.IsNullOrWhiteSpace(result.Ec2ImportMessage))
            {
                message += $" | {result.Ec2ImportMessage}";
                if (result.Ec2RowsRead > 0)
                {
                    message += $" ({result.Ec2RowsImported}/{result.Ec2RowsRead})";
                }
            }

            if (result.PerformanceMs.Count > 0)
            {
                message += " | Perf: " + string.Join(
                    ", ",
                    result.PerformanceMs
                        .Where(item => item.Key is "Batch" or "DiscoverFiles" or "ReadFiles" or "Parse" or "OpenConnection" or "EnsureSchema" or "LoadMachines" or "LoadStatuses" or "DuplicateCheck" or "InsertEvents" or "Commit" or "Ec2Import" or "Ec2Total" or "DashboardRefresh" or "Total")
                        .Select(item => $"{item.Key}={item.Value}ms"));
            }

            return message;
        }

        private ProductionDashboardFilter ResolvePostImportDashboardFilter(ProductionDashboardFilter filter)
        {
            using var conn = _factory.CreateOpenConnection();
            ProductionSchemaMigrator.Ensure(conn);

            var selectedDateHasEvents = conn.ExecuteScalar<int>(
                @"
                    SELECT COUNT(1)
                    FROM MachineEvents
                    WHERE EventDateTime >= @start
                      AND EventDateTime < @end;",
                new
                {
                    start = filter.Date.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                    end = filter.Date.Date.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss")
                }) > 0;

            if (selectedDateHasEvents)
            {
                return filter;
            }

            var latestEventDate = conn.ExecuteScalar<string>(
                @"
                    SELECT substr(MAX(EventDateTime), 1, 10)
                    FROM MachineEvents;"
            );

            if (DateTime.TryParse(latestEventDate, out var parsedLatestDate))
            {
                filter.Date = parsedLatestDate.Date;
            }

            return filter;
        }

        private object LoadStatuses()
        {
            using var conn = _factory.CreateOpenConnection();
            ProductionSchemaMigrator.Ensure(conn);

            return LoadStatuses(conn);
        }

        private object LoadPartCodeStyles()
        {
            using var conn = _factory.CreateOpenConnection();
            ProductionSchemaMigrator.Ensure(conn);

            return conn.Query(
                @"
                    SELECT
                        PartCode AS partCode,
                        COALESCE(ColorHex, '#D93F3F') AS colorHex,
                        COALESCE(TextColorHex, '#FFFFFF') AS textColorHex,
                        COALESCE(Description, '') AS description
                    FROM ProductionPartCodeStyles
                    WHERE COALESCE(IsActive, 1) = 1
                    ORDER BY PartCode;"
            ).ToList();
        }

        private static object MapForecast(GBareruCapacityForecastDto forecast)
        {
            return new
            {
                isAvailable = forecast.IsAvailable,
                message = forecast.Message,
                eciiMinutes = forecast.EciiMinutes,
                bunkatsuMinutes = forecast.BunkatsuMinutes,
                dcsMinutes = forecast.DcsMinutes,
                peopleCount = forecast.PeopleCount,
                cycleMode = forecast.CycleMode,
                cycleMinutes = forecast.CycleMinutes,
                block1Minutes = forecast.Block1Minutes,
                block2Minutes = forecast.Block2Minutes,
                bottleneck = forecast.Bottleneck,
                availableMinutes = forecast.AvailableMinutes,
                forecastCapacity = forecast.ForecastCapacity,
                forecastKadouritsuPercent = forecast.ForecastKadouritsuPercent,
                realKadouritsuPercent = forecast.RealKadouritsuPercent,
                differencePercent = forecast.DifferencePercent,
                calculationMs = forecast.CalculationMs,
                areas = forecast.Areas.Select(area => new
                {
                    localId = area.LocalId,
                    localNamePt = area.LocalNamePt,
                    localNameJp = area.LocalNameJp,
                    peopleCount = area.PeopleCount,
                    cycleMode = area.CycleMode,
                    cycleMinutes = area.CycleMinutes,
                    forecastCapacity = area.ForecastCapacity,
                    forecastKadouritsuPercent = area.ForecastKadouritsuPercent,
                    realKadouritsuPercent = area.RealKadouritsuPercent,
                    message = area.Message
                })
            };
        }

        private static object LoadStatuses(System.Data.IDbConnection conn)
        {
            return conn.Query(
                @"
                    SELECT
                        COALESCE(SectorId, 0) AS sectorId,
                        StatusCode AS statusCode,
                        DisplayCode AS displayCode,
                        COALESCE(Classification, '') AS classification,
                        COALESCE(NamePt, '') AS namePt,
                        COALESCE(NULLIF(NameJp, ''), NamePt, '') AS nameJp,
                        COALESCE(ColorHex, '#5B88E8') AS colorHex,
                        COALESCE(TextColorHex, '#FFFFFF') AS textColorHex
                    FROM MachineStatuses
                    WHERE COALESCE(IsActive, 1) = 1
                    ORDER BY SortOrder, StatusCode;"
            ).ToList();
        }

        private ProductionDashboardFilter ReadFilter(JsonElement root)
        {
            return new ProductionDashboardFilter
            {
                Date = ReadDate(root, "date", DateTime.Today),
                ShiftId = ReadInt(root, "shiftId", _currentOperator.ShiftId),
                SectorId = ReadInt(root, "sectorId", 0),
                LocalId = ReadInt(root, "localId", 0),
                MachineId = ReadInt(root, "machineId", 0),
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

        private static void LogDashboardDiagnostics(ProductionDashboardDto dashboard)
        {
            foreach (var machine in dashboard.Machines)
            {
                WriteDiagnostic(
                    $"[ProductionMonitor][MachinePayload] Machine={machine.MachineCode} Equipment={machine.MachineNamePt} Lot={machine.LotNo} Code={machine.Ec2PartCode} Time={FormatNullableDouble(machine.Ec2ProcessMinutes)} Percent={machine.ProductionPercent.ToString("0.0")} Status={machine.StatusText} PartColor={machine.Ec2PartColorHex} TextColor={machine.Ec2PartTextColorHex}");
            }
        }

        private static string FormatNullableDouble(double? value)
        {
            if (!value.HasValue)
            {
                return "null";
            }

            return double.IsFinite(value.Value)
                ? value.Value.ToString("0.0")
                : value.Value.ToString();
        }

        private static void WriteDiagnostic(string message)
        {
            Debug.WriteLine(message);

            try
            {
                Console.WriteLine(message);
            }
            catch
            {
            }
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
