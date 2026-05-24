using System.Globalization;
using Dapper;
using TeamOps.Config;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;
using TeamOps.Services;
using TeamOps.UI.Forms.Models;

var settings = new DbSettings();
var factory = new SqliteConnectionFactory(settings);
var machineRepository = new ProductionMachineRepository(factory);
var eventRepository = new ProductionEventRepository(factory);
var importer = new ProductionFileImporter(factory, machineRepository, eventRepository);
var analytics = new ProductionAnalyticsService(factory);
const int DadSectorId = 2;

var command = args.Length > 0
    ? args[0].Trim().ToLowerInvariant()
    : "demo";

switch (command)
{
    case "import":
        RunImport();
        break;

    case "dashboard":
        ShowDashboards();
        break;

    case "status-hardening":
        RunStatusHardeningChecks();
        break;

    case "status-report":
        RunStatusReport(args.Skip(1).ToArray());
        break;

    case "schema-check":
        RunSchemaValidation(repair: false);
        break;

    case "schema-repair":
        RunSchemaValidation(repair: true);
        break;

    case "demo":
    default:
        RunImport();
        Console.WriteLine();
        ShowDashboards();
        break;
}

void RunImport()
{
    var result = importer.ImportLatest();

    Console.WriteLine("=== IMPORT RESULT ===");
    Console.WriteLine($"FilesRead={result.FilesRead}");
    Console.WriteLine($"LinesRead={result.LinesRead}");
    Console.WriteLine($"Imported={result.Imported}");
    Console.WriteLine($"Ignored={result.Ignored}");
    Console.WriteLine($"MachinesCreated={result.MachinesCreated}");
    Console.WriteLine($"BatchExecuted={result.BatchExecuted}");
    Console.WriteLine($"BatchMessage={result.BatchMessage}");

    if (result.Errors.Count > 0)
    {
        Console.WriteLine("Errors:");
        foreach (var error in result.Errors)
        {
            Console.WriteLine($" - {error}");
        }
    }
}

void ShowDashboards()
{
    using var conn = factory.CreateOpenConnection();
    ProductionSchemaMigrator.Ensure(conn);

    var shifts = conn.Query<ShiftRow>(
        @"
            SELECT
                Id,
                COALESCE(NamePt, '') AS NamePt,
                COALESCE(NULLIF(NameJp, ''), NamePt, '') AS NameJp
            FROM Shifts
            ORDER BY Id;"
    ).ToList();

    var dayShift = shifts.FirstOrDefault(shift => MatchesShift(shift, "hiru", "day", "dia"));
    var nightShift = shifts.FirstOrDefault(shift => MatchesShift(shift, "yakin", "night", "noite", "夜"));

    if (dayShift != null)
    {
        ShowDashboard("HIRUKIN", new DateTime(2026, 4, 29), dayShift.Id);
    }

    if (nightShift != null)
    {
        Console.WriteLine();
        ShowDashboard("YAKIN", new DateTime(2026, 4, 28), nightShift.Id);
    }

    if (dayShift == null && nightShift == null)
    {
        var fallbackShift = shifts.FirstOrDefault();
        if (fallbackShift == null)
        {
            Console.WriteLine("Nenhum turno encontrado para o probe.");
            return;
        }

        ShowDashboard("DEFAULT", new DateTime(2026, 4, 29), fallbackShift.Id);
    }
}

void RunSchemaValidation(bool repair)
{
    using var conn = factory.CreateOpenConnection();

    var before = InspectProductionSchema(conn);
    if (repair)
    {
        ProductionSchemaMigrator.Ensure(conn);
    }

    var after = repair
        ? InspectProductionSchema(conn)
        : before;

    Console.WriteLine(repair ? "=== PRODUCTION SCHEMA REPAIR ===" : "=== PRODUCTION SCHEMA CHECK ===");
    Console.WriteLine($"Database={settings.DatabasePath}");
    Console.WriteLine($"JournalMode={after.JournalMode}");
    Console.WriteLine($"QuickCheck={after.QuickCheck}");

    if (repair)
    {
        Console.WriteLine($"IssuesBefore={before.Issues.Count}");
        foreach (var issue in before.Issues)
        {
            Console.WriteLine($"BEFORE: {issue}");
        }
    }

    Console.WriteLine($"IssuesAfter={after.Issues.Count}");
    foreach (var issue in after.Issues)
    {
        Console.WriteLine($"AFTER: {issue}");
    }

    if (after.Issues.Count == 0)
    {
        Console.WriteLine("OK: schema de producao validado.");
        return;
    }

    Environment.ExitCode = 2;
}

void RunStatusHardeningChecks()
{
    using var conn = factory.CreateOpenConnection();
    ProductionSchemaMigrator.Ensure(conn);

    var indexSql = conn.ExecuteScalar<string>(
        @"
            SELECT COALESCE(sql, '')
            FROM sqlite_master
            WHERE type = 'index'
              AND name = 'IX_MachineStatuses_Sector_StatusCode';"
    ) ?? string.Empty;

    Require(
        indexSql.Contains("COALESCE(SectorId, 0)", StringComparison.OrdinalIgnoreCase)
        && indexSql.Contains("StatusCode", StringComparison.OrdinalIgnoreCase),
        "IX_MachineStatuses_Sector_StatusCode deve usar COALESCE(SectorId, 0), StatusCode.");

    var duplicateKeys = conn.ExecuteScalar<int>(
        @"
            SELECT COUNT(1)
            FROM (
                SELECT COALESCE(SectorId, 0), StatusCode, COUNT(1) AS Total
                FROM MachineStatuses
                GROUP BY COALESCE(SectorId, 0), StatusCode
                HAVING COUNT(1) > 1
            );"
    );
    Require(duplicateKeys == 0, "MachineStatuses possui chaves duplicadas por setor/codigo.");

    var statuses = conn.Query<StatusDefinitionProbeRow>(
        @"
            SELECT
                SectorId,
                StatusCode,
                DisplayCode,
                COALESCE(Classification, '') AS Classification,
                COALESCE(NamePt, '') AS NamePt,
                COALESCE(ColorHex, '') AS ColorHex
            FROM MachineStatuses
            WHERE COALESCE(SectorId, 0) IN (0, 2)
              AND StatusCode IN (0, 1, 3, 4, 5, 10, 17, 18, 19, 99);"
    ).ToList();

    ExpectStatus(statuses, null, 0, 0, "Running");
    ExpectStatus(statuses, null, 3, 3, "StopCounts");
    ExpectStatus(statuses, 2, 0, 0, "Running");
    ExpectStatus(statuses, 2, 3, 3, "StopNoCount");
    ExpectStatus(statuses, 2, 17, 1, "StopNoCount");
    ExpectStatus(statuses, 2, 18, 1, "StopNoCount");
    ExpectStatus(statuses, 2, 19, 1, "StopNoCount");

    var gBareruFive = ResolveStatusForProbe(statuses, 1, 5);
    Require(gBareruFive == null || gBareruFive.Classification == "StopCounts", "GBareru status 5 deve ficar no fallback antigo ou StopCounts.");

    var gBareruTen = ResolveStatusForProbe(statuses, 1, 10);
    Require(gBareruTen == null || gBareruTen.Classification == "StopCounts", "GBareru status 10 deve ficar no fallback antigo ou StopCounts.");

    var dadUnknown = ResolveStatusForProbe(statuses, 2, 99);
    Require(dadUnknown == null || dadUnknown.Classification == "StopCounts", "DAD status 99 deve ficar desconhecido ou conservador StopCounts.");

    Require(CalculateProbeEfficiency(6, 0, 4) == 100d, "StopNoCount deve sair do denominador: 6h running + 4h no-count = 100%.");
    Require(CalculateProbeEfficiency(6, 2, 2) == 75d, "StopCounts deve permanecer no denominador: 6h running + 2h stop + 2h no-count = 75%.");

    Console.WriteLine("=== STATUS HARDENING CHECKS ===");
    Console.WriteLine("OK: indice setorial, seeds DAD, fallback conservador e formula StopNoCount validados.");
}

ProductionSchemaInspection InspectProductionSchema(System.Data.IDbConnection conn)
{
    var issues = new List<string>();
    var journalMode = conn.ExecuteScalar<string>("PRAGMA journal_mode;") ?? string.Empty;
    var quickCheck = conn.ExecuteScalar<string>("PRAGMA quick_check;") ?? string.Empty;

    if (!quickCheck.Equals("ok", StringComparison.OrdinalIgnoreCase))
    {
        issues.Add($"DB_QUICK_CHECK_FAILED: {quickCheck}");
    }

    RequireTable(conn, "Machines", issues);
    RequireTable(conn, "MachineEvents", issues);
    RequireTable(conn, "MachineCurrentStatus", issues);
    RequireTable(conn, "MachineStatuses", issues);

    RequireColumn(conn, "Machines", "MachineCode", issues);
    RequireColumn(conn, "Machines", "MachineKey", issues);
    RequireColumn(conn, "Machines", "LineCode", issues);
    RequireColumn(conn, "Machines", "LocalId", issues);
    RequireColumn(conn, "Machines", "SectorId", issues);
    RequireColumn(conn, "Machines", "IsActive", issues);

    RequireColumn(conn, "MachineEvents", "MachineId", issues);
    RequireColumn(conn, "MachineEvents", "SectorId", issues);
    RequireColumn(conn, "MachineEvents", "StatusCode", issues);
    RequireColumn(conn, "MachineEvents", "EventDateTime", issues);
    RequireColumn(conn, "MachineEvents", "InternalState", issues);

    RequireColumn(conn, "MachineCurrentStatus", "MachineId", issues);
    RequireColumn(conn, "MachineCurrentStatus", "SectorId", issues);
    RequireColumn(conn, "MachineCurrentStatus", "StatusCode", issues);
    RequireColumn(conn, "MachineCurrentStatus", "EventDateTime", issues);

    RequireColumn(conn, "MachineStatuses", "SectorId", issues);
    RequireColumn(conn, "MachineStatuses", "StatusCode", issues);
    RequireColumn(conn, "MachineStatuses", "DisplayCode", issues);
    RequireColumn(conn, "MachineStatuses", "Classification", issues);

    RequireIndex(conn, "IX_MachineStatuses_Sector_StatusCode", issues);
    RequireIndex(conn, "IX_MachineEvents_UniqueEvent", issues);
    RequireIndex(conn, "IX_MachineEvents_UniqueRawEvent", issues);
    RequireIndex(conn, "IX_MachineEvents_EventDateTime", issues);
    RequireIndex(conn, "IX_MachineEvents_Machine_EventTime", issues);
    RequireIndex(conn, "IX_Machines_MachineKey_Unique", issues);
    RequireIndex(conn, "IX_Machines_MachineCode_LineCode", issues);

    WarnIfIndexExists(conn, "IX_MachineEvents_Sector_EventTime", issues);
    WarnIfIndexExists(conn, "IX_MachineEvents_StatusCode_EventTime", issues);
    WarnIfIndexExists(conn, "IX_MachineEvents_Sector_Status_EventTime", issues);

    var statusIndexSql = conn.ExecuteScalar<string>(
        @"
            SELECT COALESCE(sql, '')
            FROM sqlite_master
            WHERE type = 'index'
              AND name = 'IX_MachineStatuses_Sector_StatusCode';"
    ) ?? string.Empty;

    if (!statusIndexSql.Contains("COALESCE(SectorId, 0)", StringComparison.OrdinalIgnoreCase)
        || !statusIndexSql.Contains("StatusCode", StringComparison.OrdinalIgnoreCase))
    {
        issues.Add("BAD_INDEX_SQL: IX_MachineStatuses_Sector_StatusCode deve usar COALESCE(SectorId, 0), StatusCode.");
    }

    var duplicateStatusKeys = conn.ExecuteScalar<int>(
        @"
            SELECT COUNT(1)
            FROM (
                SELECT COALESCE(SectorId, 0), StatusCode, COUNT(1) AS Total
                FROM MachineStatuses
                GROUP BY COALESCE(SectorId, 0), StatusCode
                HAVING COUNT(1) > 1
            );"
    );
    if (duplicateStatusKeys > 0)
    {
        issues.Add($"DUPLICATE_MACHINE_STATUSES: {duplicateStatusKeys}");
    }

    var globalSeedCount = conn.ExecuteScalar<int>(
        @"
            SELECT COUNT(1)
            FROM MachineStatuses
            WHERE SectorId IS NULL
              AND StatusCode IN (0, 1, 3, 4);"
    );
    if (globalSeedCount < 4)
    {
        issues.Add($"MISSING_GLOBAL_STATUS_SEEDS: found {globalSeedCount}/4");
    }

    var dadSeedCount = conn.ExecuteScalar<int>(
        @"
            SELECT COUNT(1)
            FROM MachineStatuses
            WHERE SectorId = @DadSectorId
              AND StatusCode IN (0, 1, 3, 4, 17, 18, 19);",
        new
        {
            DadSectorId
        }
    );
    if (dadSeedCount < 7)
    {
        issues.Add($"MISSING_DAD_STATUS_SEEDS: found {dadSeedCount}/7");
    }

    var dadNoCountCount = conn.ExecuteScalar<int>(
        @"
            SELECT COUNT(1)
            FROM MachineStatuses
            WHERE SectorId = @DadSectorId
              AND StatusCode IN (3, 17, 18, 19)
              AND Classification = 'StopNoCount';",
        new
        {
            DadSectorId
        }
    );
    if (dadNoCountCount < 4)
    {
        issues.Add($"BAD_DAD_STOP_NO_COUNT_SEEDS: found {dadNoCountCount}/4");
    }

    return new ProductionSchemaInspection(journalMode, quickCheck, issues);
}

static void RequireTable(System.Data.IDbConnection conn, string tableName, List<string> issues)
{
    var exists = conn.ExecuteScalar<int>(
        @"
            SELECT COUNT(1)
            FROM sqlite_master
            WHERE type = 'table'
              AND name = @tableName;",
        new
        {
            tableName
        }) > 0;

    if (!exists)
    {
        issues.Add($"MISSING_TABLE: {tableName}");
    }
}

static void RequireColumn(System.Data.IDbConnection conn, string tableName, string columnName, List<string> issues)
{
    var exists = conn.ExecuteScalar<int>(
        $@"
            SELECT COUNT(1)
            FROM pragma_table_info('{tableName}')
            WHERE name = @columnName;",
        new
        {
            columnName
        }) > 0;

    if (!exists)
    {
        issues.Add($"MISSING_COLUMN: {tableName}.{columnName}");
    }
}

static void RequireIndex(System.Data.IDbConnection conn, string indexName, List<string> issues)
{
    if (!IndexExists(conn, indexName))
    {
        issues.Add($"MISSING_INDEX: {indexName}");
    }
}

static void WarnIfIndexExists(System.Data.IDbConnection conn, string indexName, List<string> issues)
{
    if (IndexExists(conn, indexName))
    {
        issues.Add($"EXTRA_INDEX_PERFORMANCE_RISK: {indexName}");
    }
}

static bool IndexExists(System.Data.IDbConnection conn, string indexName)
{
    return conn.ExecuteScalar<int>(
        @"
            SELECT COUNT(1)
            FROM sqlite_master
            WHERE type = 'index'
              AND name = @indexName;",
        new
        {
            indexName
        }) > 0;
}

void RunStatusReport(string[] reportArgs)
{
    var options = StatusReportOptions.Parse(reportArgs);

    using var conn = factory.CreateOpenConnection();
    ProductionSchemaMigrator.Ensure(conn);

    var statuses = conn.Query<StatusDefinitionProbeRow>(
        @"
            SELECT
                SectorId,
                StatusCode,
                DisplayCode,
                COALESCE(Classification, '') AS Classification,
                COALESCE(NamePt, '') AS NamePt,
                COALESCE(NameJp, '') AS NameJp,
                COALESCE(ColorHex, '') AS ColorHex
            FROM MachineStatuses
            WHERE COALESCE(IsActive, 1) = 1;"
    ).ToList();

    var events = conn.Query<StatusEventProbeRow>(
        @"
            SELECT
                e.MachineId,
                e.MachineCode,
                e.LineCode,
                e.SectorId,
                COALESCE(s.NamePt, '') AS SectorName,
                e.StatusCode,
                COALESCE(e.StatusText, '') AS StatusText,
                e.EventDateTime
            FROM MachineEvents e
            LEFT JOIN Sectors s ON s.Id = e.SectorId
            WHERE (@startDate = '' OR datetime(e.EventDateTime) >= datetime(@startDate))
              AND (@endDate = '' OR datetime(e.EventDateTime) < datetime(@endDate))
              AND (@sectorId <= 0 OR COALESCE(e.SectorId, 0) = @sectorId)
            ORDER BY e.MachineId, datetime(e.EventDateTime), e.Id;",
        new
        {
            startDate = options.StartDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty,
            endDate = options.EndDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty,
            sectorId = options.SectorId
        }
    ).ToList();

    var rows = BuildStatusReportRows(events, statuses);

    Console.WriteLine("=== PRODUCTION STATUS REPORT ===");
    Console.WriteLine($"Rows={rows.Count} Events={events.Count} Start={options.StartDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "(all)"} End={options.EndDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "(all)"} Sector={options.SectorIdLabel}");
    Console.WriteLine("Sector,StatusCode,Description,Classification,Source,Occurrences,TotalMinutes,CountsInEfficiencyDenominator,UsedFallback,EstimatedEfficiencyImpact,Warning");

    foreach (var row in rows)
    {
        Console.WriteLine(row.ToCsvLine());
    }

    var dadWarnings = rows
        .Where(row => row.SectorId == DadSectorId && !string.IsNullOrWhiteSpace(row.Warning))
        .ToList();

    Console.WriteLine();
    Console.WriteLine("=== DAD FOCUS ===");
    Console.WriteLine($"KnownCodesFound={string.Join(",", rows.Where(row => row.SectorId == DadSectorId && IsDadSeedStatusCode(row.StatusCode)).Select(row => row.StatusCode).Distinct().OrderBy(code => code))}");
    Console.WriteLine($"UnexpectedCodesFound={string.Join(",", rows.Where(row => row.SectorId == DadSectorId && !IsDadSeedStatusCode(row.StatusCode)).Select(row => row.StatusCode).Distinct().OrderBy(code => code))}");
    Console.WriteLine($"WarningRows={dadWarnings.Count}");

    if (options.CsvPath.Length > 0)
    {
        var csvLines = new[]
            {
                "Sector,StatusCode,Description,Classification,Source,Occurrences,TotalMinutes,CountsInEfficiencyDenominator,UsedFallback,EstimatedEfficiencyImpact,Warning"
            }
            .Concat(rows.Select(row => row.ToCsvLine()));

        File.WriteAllLines(options.CsvPath, csvLines);
        Console.WriteLine($"CSV={Path.GetFullPath(options.CsvPath)}");
    }
}

void ShowDashboard(string label, DateTime date, int shiftId)
{
    var dashboard = analytics.BuildDashboard(new ProductionDashboardFilter
    {
        Date = date,
        ShiftId = shiftId,
        MachineCode = "E01"
    });

    Console.WriteLine($"=== DASHBOARD {label} ===");
    Console.WriteLine($"Date={date:yyyy-MM-dd}");
    Console.WriteLine($"Period={dashboard.Period.Start:yyyy-MM-dd HH:mm:ss} -> {dashboard.Period.End:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"Kadouritsu={dashboard.ProductionPercent:F1}");
    Console.WriteLine($"MachinesRunning={dashboard.MachinesRunning}");
    Console.WriteLine($"MachinesStopped={dashboard.MachinesStopped}");
    Console.WriteLine($"ErrorMinutes={dashboard.ErrorMinutes:F1}");
    Console.WriteLine($"InactiveMinutes={dashboard.InactiveMinutes:F1}");
    Console.WriteLine($"Areas={dashboard.Areas.Count}");
    Console.WriteLine($"OperatorRanking={dashboard.OperatorRanking.Count}");

    foreach (var machine in dashboard.Machines)
    {
        Console.WriteLine($"Machine={machine.MachineCode} Status={machine.StatusText} Kadouritsu={machine.ProductionPercent:F1}% Running={machine.RunningMinutes:F1} Stop={machine.StoppedMinutes:F1} Error={machine.ErrorMinutes:F1} Inactive={machine.InactiveMinutes:F1} Updated={machine.LastUpdate:yyyy-MM-dd HH:mm:ss}");
    }
}

static bool MatchesShift(ShiftRow shift, params string[] keywords)
{
    var values = new[] { shift.NamePt, shift.NameJp }
        .Where(value => !string.IsNullOrWhiteSpace(value));

    foreach (var value in values)
    {
        foreach (var keyword in keywords)
        {
            if (value.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
    }

    return false;
}

static void ExpectStatus(
    IReadOnlyList<StatusDefinitionProbeRow> statuses,
    int? sectorId,
    int statusCode,
    int displayCode,
    string classification)
{
    var status = statuses.FirstOrDefault(item =>
        item.SectorId == sectorId
        && item.StatusCode == statusCode);

    Require(status != null, $"Status esperado nao encontrado: setor={sectorId?.ToString() ?? "global"} codigo={statusCode}.");
    Require(status!.DisplayCode == displayCode, $"DisplayCode invalido para setor={sectorId?.ToString() ?? "global"} codigo={statusCode}.");
    Require(string.Equals(status.Classification, classification, StringComparison.OrdinalIgnoreCase), $"Classification invalida para setor={sectorId?.ToString() ?? "global"} codigo={statusCode}.");
}

static StatusDefinitionProbeRow? ResolveStatusForProbe(
    IReadOnlyList<StatusDefinitionProbeRow> statuses,
    int sectorId,
    int statusCode)
{
    return statuses.FirstOrDefault(item => item.SectorId == sectorId && item.StatusCode == statusCode)
        ?? statuses.FirstOrDefault(item => !item.SectorId.HasValue && item.StatusCode == statusCode);
}

static List<StatusReportRow> BuildStatusReportRows(
    IReadOnlyList<StatusEventProbeRow> events,
    IReadOnlyList<StatusDefinitionProbeRow> statuses)
{
    var accumulators = new Dictionary<string, StatusReportAccumulator>(StringComparer.OrdinalIgnoreCase);

    foreach (var machineEvents in events.GroupBy(item => item.MachineId))
    {
        var orderedEvents = machineEvents
            .OrderBy(item => item.EventDateTime)
            .ToList();

        for (var index = 0; index < orderedEvents.Count; index++)
        {
            var current = orderedEvents[index];
            var next = index + 1 < orderedEvents.Count
                ? orderedEvents[index + 1]
                : null;
            var minutes = next == null
                ? 0d
                : Math.Max(0d, (next.EventDateTime - current.EventDateTime).TotalMinutes);
            var description = string.IsNullOrWhiteSpace(current.StatusText)
                ? "-"
                : current.StatusText.Trim();
            var key = $"{current.SectorId.GetValueOrDefault()}|{current.StatusCode}|{description}";

            if (!accumulators.TryGetValue(key, out var accumulator))
            {
                var resolved = ResolveStatusForReport(statuses, current.SectorId, current.StatusCode);
                accumulator = new StatusReportAccumulator
                {
                    SectorId = current.SectorId,
                    SectorName = ResolveSectorLabel(current.SectorId, current.SectorName, current.LineCode),
                    StatusCode = current.StatusCode,
                    Description = description,
                    Classification = NormalizeReportClassification(resolved.Definition?.Classification, resolved.Definition?.DisplayCode),
                    Source = resolved.Source,
                    UsedFallback = resolved.UsedFallback
                };
                accumulators[key] = accumulator;
            }

            accumulator.Occurrences++;
            accumulator.TotalMinutes += minutes;
        }
    }

    return accumulators.Values
        .Select(CreateStatusReportRow)
        .OrderBy(row => row.SectorId.GetValueOrDefault())
        .ThenBy(row => row.StatusCode)
        .ThenBy(row => row.Description, StringComparer.OrdinalIgnoreCase)
        .ToList();
}

static StatusReportRow CreateStatusReportRow(StatusReportAccumulator accumulator)
{
    var countsInDenominator = CountsInEfficiencyDenominator(accumulator.Classification);
    var estimatedImpact = countsInDenominator && !accumulator.Classification.Equals("Running", StringComparison.OrdinalIgnoreCase)
        ? Math.Round(accumulator.TotalMinutes, 1)
        : 0d;

    return new StatusReportRow
    {
        SectorId = accumulator.SectorId,
        Sector = accumulator.SectorName,
        StatusCode = accumulator.StatusCode,
        Description = accumulator.Description,
        Classification = accumulator.Classification,
        Source = accumulator.Source,
        Occurrences = accumulator.Occurrences,
        TotalMinutes = Math.Round(accumulator.TotalMinutes, 1),
        CountsInEfficiencyDenominator = countsInDenominator,
        UsedFallback = accumulator.UsedFallback,
        EstimatedEfficiencyImpact = estimatedImpact,
        Warning = BuildWarning(accumulator)
    };
}

static StatusDefinitionResolution ResolveStatusForReport(
    IReadOnlyList<StatusDefinitionProbeRow> statuses,
    int? sectorId,
    int statusCode)
{
    var exact = statuses.FirstOrDefault(item => item.SectorId == sectorId && item.StatusCode == statusCode);
    if (exact != null)
    {
        return new StatusDefinitionResolution(exact, ResolveStatusSource(sectorId, statusCode, false), false);
    }

    var global = statuses.FirstOrDefault(item => !item.SectorId.HasValue && item.StatusCode == statusCode);
    if (global != null)
    {
        return new StatusDefinitionResolution(global, ResolveStatusSource(null, statusCode, sectorId.HasValue), sectorId.HasValue);
    }

    return new StatusDefinitionResolution(null, "unknown-conservative", false);
}

static string ResolveStatusSource(int? sectorId, int statusCode, bool fallback)
{
    if (fallback)
    {
        return "fallback-global";
    }

    if (!sectorId.HasValue)
    {
        return IsGlobalSeedStatusCode(statusCode)
            ? "global-seed"
            : "auto-created-or-manual-global";
    }

    if (sectorId == DadSectorId)
    {
        return IsDadSeedStatusCode(statusCode)
            ? "setorial-seed"
            : "auto-created-or-manual-setorial";
    }

    return "setorial";
}

static string NormalizeReportClassification(string? classification, int? displayCode)
{
    var normalized = (classification ?? string.Empty).Trim();
    if (normalized.Equals("Running", StringComparison.OrdinalIgnoreCase))
    {
        return "Running";
    }

    if (normalized.Equals("StopNoCount", StringComparison.OrdinalIgnoreCase))
    {
        return "StopNoCount";
    }

    if (normalized.Equals("Error", StringComparison.OrdinalIgnoreCase))
    {
        return "Error";
    }

    return displayCode == 0
        ? "Running"
        : displayCode == 4
            ? "Error"
            : "StopCounts";
}

static string ResolveSectorLabel(int? sectorId, string sectorName, string lineCode)
{
    if (!string.IsNullOrWhiteSpace(sectorName))
    {
        return sectorName.Trim();
    }

    return sectorId switch
    {
        1 => "G-Bareru",
        2 => "DAD",
        _ => string.IsNullOrWhiteSpace(lineCode) ? $"Sector {sectorId.GetValueOrDefault()}" : lineCode.Trim()
    };
}

static bool CountsInEfficiencyDenominator(string classification)
{
    return !classification.Equals("StopNoCount", StringComparison.OrdinalIgnoreCase);
}

static string BuildWarning(StatusReportAccumulator accumulator)
{
    var warnings = new List<string>();

    if (accumulator.SectorId == DadSectorId && !IsDadSeedStatusCode(accumulator.StatusCode))
    {
        warnings.Add("UNKNOWN_DAD_STATUS");
    }

    if (accumulator.SectorId == DadSectorId && accumulator.UsedFallback)
    {
        warnings.Add("FALLBACK_USED_FOR_DAD");
    }

    if (accumulator.Source.Contains("auto-created", StringComparison.OrdinalIgnoreCase))
    {
        warnings.Add("AUTO_CREATED_STATUS");
    }

    if (accumulator.SectorId == DadSectorId
        && !accumulator.Classification.Equals("Running", StringComparison.OrdinalIgnoreCase)
        && CountsInEfficiencyDenominator(accumulator.Classification)
        && accumulator.TotalMinutes > 0)
    {
        warnings.Add("POSSIBLE_EFFICIENCY_IMPACT");
    }

    return string.Join("|", warnings.Distinct(StringComparer.OrdinalIgnoreCase));
}

static double CalculateProbeEfficiency(double runningHours, double stopCountsHours, double stopNoCountHours)
{
    var denominator = runningHours + stopCountsHours;
    return denominator <= 0
        ? 0
        : Math.Round((runningHours / denominator) * 100d, 1);
}

static void Require(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static bool IsGlobalSeedStatusCode(int statusCode)
{
    return statusCode is 0 or 1 or 3 or 4;
}

static bool IsDadSeedStatusCode(int statusCode)
{
    return statusCode is 0 or 1 or 3 or 4 or 17 or 18 or 19;
}

sealed class ShiftRow
{
    public int Id { get; set; }
    public string NamePt { get; set; } = string.Empty;
    public string NameJp { get; set; } = string.Empty;
}

sealed class StatusReportOptions
{
    public DateTime? StartDate { get; private init; }
    public DateTime? EndDate { get; private init; }
    public int SectorId { get; private init; }
    public string CsvPath { get; private init; } = string.Empty;
    public string SectorIdLabel => SectorId <= 0 ? "(all)" : SectorId.ToString();

    public static StatusReportOptions Parse(string[] args)
    {
        DateTime? startDate = null;
        DateTime? endDate = null;
        var sectorId = 0;
        var csvPath = string.Empty;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index].Trim();
            if (arg.Equals("--start", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
            {
                startDate = DateTime.Parse(args[++index], CultureInfo.InvariantCulture);
            }
            else if (arg.Equals("--end", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
            {
                endDate = DateTime.Parse(args[++index], CultureInfo.InvariantCulture);
            }
            else if (arg.Equals("--sector", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
            {
                var sectorValue = args[++index].Trim();
                sectorId = sectorValue.Equals("dad", StringComparison.OrdinalIgnoreCase)
                    ? 2
                    : sectorValue.Equals("gbareru", StringComparison.OrdinalIgnoreCase) || sectorValue.Equals("g-bareru", StringComparison.OrdinalIgnoreCase)
                        ? 1
                        : int.Parse(sectorValue, CultureInfo.InvariantCulture);
            }
            else if (arg.Equals("--csv", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
            {
                csvPath = args[++index].Trim();
            }
        }

        return new StatusReportOptions
        {
            StartDate = startDate,
            EndDate = endDate,
            SectorId = sectorId,
            CsvPath = csvPath
        };
    }
}

sealed class StatusDefinitionProbeRow
{
    public int? SectorId { get; set; }
    public int StatusCode { get; set; }
    public int DisplayCode { get; set; }
    public string Classification { get; set; } = string.Empty;
    public string NamePt { get; set; } = string.Empty;
    public string NameJp { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
}

sealed class StatusEventProbeRow
{
    public int MachineId { get; set; }
    public string MachineCode { get; set; } = string.Empty;
    public string LineCode { get; set; } = string.Empty;
    public int? SectorId { get; set; }
    public string SectorName { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public DateTime EventDateTime { get; set; }
}

sealed class StatusReportAccumulator
{
    public int? SectorId { get; set; }
    public string SectorName { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public bool UsedFallback { get; set; }
    public int Occurrences { get; set; }
    public double TotalMinutes { get; set; }
}

sealed class StatusReportRow
{
    public int? SectorId { get; set; }
    public string Sector { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public int Occurrences { get; set; }
    public double TotalMinutes { get; set; }
    public bool CountsInEfficiencyDenominator { get; set; }
    public bool UsedFallback { get; set; }
    public double EstimatedEfficiencyImpact { get; set; }
    public string Warning { get; set; } = string.Empty;

    public string ToCsvLine()
    {
        return string.Join(
            ",",
            EscapeCsv(Sector),
            StatusCode.ToString(CultureInfo.InvariantCulture),
            EscapeCsv(Description),
            EscapeCsv(Classification),
            EscapeCsv(Source),
            Occurrences.ToString(CultureInfo.InvariantCulture),
            TotalMinutes.ToString("0.0", CultureInfo.InvariantCulture),
            CountsInEfficiencyDenominator ? "true" : "false",
            UsedFallback ? "true" : "false",
            EstimatedEfficiencyImpact.ToString("0.0", CultureInfo.InvariantCulture),
            EscapeCsv(Warning));
    }

    private static string EscapeCsv(string value)
    {
        var safe = value ?? string.Empty;
        return safe.Contains(',') || safe.Contains('"') || safe.Contains('\n') || safe.Contains('\r')
            ? $"\"{safe.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : safe;
    }
}

readonly record struct StatusDefinitionResolution(
    StatusDefinitionProbeRow? Definition,
    string Source,
    bool UsedFallback);

readonly record struct ProductionSchemaInspection(
    string JournalMode,
    string QuickCheck,
    List<string> Issues);
