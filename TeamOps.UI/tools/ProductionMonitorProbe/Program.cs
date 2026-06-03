using System.Globalization;
using System.Diagnostics;
using System.Configuration;
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

    case "import-profile":
        RunImportProfile(args.Skip(1).ToArray());
        break;

    case "db-index-check":
        RunDbIndexCheck();
        break;

    case "ec2-diagnostics":
        RunEc2Diagnostics();
        break;

    case "ec2-reset-latest":
        RunEc2ResetLatest();
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

    case "production-diagnostics":
        RunProductionDiagnostics();
        break;

    case "machine-cleanup":
        RunMachineCleanup(args.Skip(1).ToArray());
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
    PrintEc2ImportResult(result);
    PrintImportPerformance(result);

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
    PrintRuntimeConfigDiagnostics();
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

void RunImportProfile(string[] profileArgs)
{
    var options = ImportProfileOptions.Parse(profileArgs);
    var totalWatch = Stopwatch.StartNew();
    PrintRuntimeConfigDiagnostics();
    var result = importer.ImportLatest();
    totalWatch.Stop();

    Console.WriteLine("=== IMPORT PROFILE ===");
    Console.WriteLine($"Database={settings.DatabasePath}");
    Console.WriteLine($"FilesRead={result.FilesRead}");
    Console.WriteLine($"LinesRead={result.LinesRead}");
    Console.WriteLine($"Imported={result.Imported}");
    Console.WriteLine($"Ignored={result.Ignored}");
    Console.WriteLine($"MachinesCreated={result.MachinesCreated}");
    PrintEc2ImportResult(result);
    Console.WriteLine($"ProbeTotal={totalWatch.ElapsedMilliseconds}ms");
    PrintImportPerformance(result);

    if (options.Date.HasValue)
    {
        var analyticsWatch = Stopwatch.StartNew();
        using var conn = factory.CreateOpenConnection();
        ProductionSchemaMigrator.Ensure(conn);

        var shifts = conn.Query<ShiftRow>(
            @"
                SELECT
                    Id,
                    COALESCE(NamePt, '') AS NamePt,
                    COALESCE(NULLIF(NameJp, ''), NamePt, '') AS NameJp
                FROM Shifts
                ORDER BY Id;")
            .ToList();

        var dayShift = shifts.FirstOrDefault(shift => MatchesShift(shift, "hiru", "day", "dia"));
        var nightShift = shifts.FirstOrDefault(shift => MatchesShift(shift, "yakin", "night", "noite", "夜"));
        var selectedShift = dayShift ?? nightShift ?? shifts.FirstOrDefault();
        if (selectedShift == null)
        {
            Console.WriteLine("=== POST IMPORT ANALYTICS PROFILE ===");
            Console.WriteLine("Shift=none");
            Console.WriteLine("Reason=no-shift-found");
            return;
        }

        var dashboard = analytics.BuildDashboard(new ProductionDashboardFilter
        {
            Date = options.Date.Value.Date,
            SectorId = options.SectorId,
            ShiftId = selectedShift.Id
        });
        analyticsWatch.Stop();

        Console.WriteLine();
        Console.WriteLine("=== POST IMPORT ANALYTICS PROFILE ===");
        Console.WriteLine($"Date={options.Date.Value:yyyy-MM-dd}");
        Console.WriteLine($"Sector={options.SectorLabel}");
        Console.WriteLine($"Shift={selectedShift.Id} {selectedShift.NamePt}");
        Console.WriteLine($"Analytics={analyticsWatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Machines={dashboard.Machines.Count}");
        Console.WriteLine($"Areas={dashboard.Areas.Count}");
        Console.WriteLine($"Kadouritsu={dashboard.ProductionPercent:F1}");
        Console.WriteLine($"Running={dashboard.MachinesRunning}");
        Console.WriteLine($"Stopped={dashboard.MachinesStopped}");
        Console.WriteLine($"Ignored={dashboard.MachinesIgnored}");
        Console.WriteLine($"AverageOperatingProcessMinutes={dashboard.AverageOperatingProcessMinutes:F1}");
    }

    if (result.Errors.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine("Errors:");
        foreach (var error in result.Errors)
        {
            Console.WriteLine($" - {error}");
        }
    }
}

void PrintEc2ImportResult(TeamOps.Core.Entities.ProductionImportResult result)
{
    Console.WriteLine($"Ec2Attempted={result.Ec2ImportAttempted}");
    Console.WriteLine($"Ec2Skipped={result.Ec2ImportSkipped}");
    Console.WriteLine($"Ec2Message={result.Ec2ImportMessage}");
    Console.WriteLine($"Ec2AdministratorFilePath={result.Ec2FilePath}");
    Console.WriteLine($"Ec2ResolvedFullPath={result.Ec2ResolvedFullPath}");
    Console.WriteLine($"Ec2FileExists={result.Ec2FileExists}");
    Console.WriteLine($"Ec2FileSize={result.Ec2FileSize}");
    Console.WriteLine($"Ec2LastWriteTime={result.Ec2FileLastWriteTime}");
    Console.WriteLine($"Ec2EncodingDetected={result.Ec2EncodingDetected}");
    Console.WriteLine($"Ec2DelimiterDetected={result.Ec2DelimiterDetected}");
    Console.WriteLine($"Ec2RawLinePreview={result.Ec2RawLinePreview}");
    Console.WriteLine($"Ec2DecodedLinePreview={result.Ec2DecodedLinePreview}");
    Console.WriteLine($"Ec2ContainsReplacementChar={result.Ec2ContainsReplacementChar}");
    Console.WriteLine($"Ec2FirstLinePreview={result.Ec2FirstLinePreview}");
    Console.WriteLine($"Ec2HeaderLinePreview={result.Ec2HeaderLinePreview}");
    Console.WriteLine($"Ec2FirstDataLinePreview={result.Ec2FirstDataLinePreview}");
    Console.WriteLine($"Ec2RowsRead={result.Ec2RowsRead}");
    Console.WriteLine($"Ec2RowsCandidate={result.Ec2RowsCandidate}");
    Console.WriteLine($"Ec2RowsImported={result.Ec2RowsImported}");
    Console.WriteLine($"Ec2RowsIgnored={result.Ec2RowsIgnored}");
    Console.WriteLine($"Ec2IgnoredByEmptyLine={result.Ec2IgnoredByEmptyLine}");
    Console.WriteLine($"Ec2IgnoredByNotAreaBlock={result.Ec2IgnoredByNotAreaBlock}");
    Console.WriteLine($"Ec2IgnoredByTooFewColumns={result.Ec2IgnoredByTooFewColumns}");
    Console.WriteLine($"Ec2IgnoredByMissingMachine={result.Ec2IgnoredByMissingMachine}");
    Console.WriteLine($"Ec2IgnoredByInvalidStatus={result.Ec2IgnoredByInvalidStatus}");
    Console.WriteLine($"Ec2IgnoredByInvalidTime={result.Ec2IgnoredByInvalidTime}");
    Console.WriteLine($"Ec2IgnoredByUnknownFormat={result.Ec2IgnoredByUnknownFormat}");
    Console.WriteLine($"Ec2Areas={result.Ec2AreaCount}");
    Console.WriteLine($"Ec2Running={result.Ec2RunningCount}");
    Console.WriteLine($"Ec2Stopped={result.Ec2StoppedCount}");
    Console.WriteLine($"Ec2Ignored={result.Ec2IgnoredCount}");

    if (result.Ec2DiscardSamples.Count > 0)
    {
        Console.WriteLine("Ec2DiscardSamples:");
        foreach (var sample in result.Ec2DiscardSamples)
        {
            Console.WriteLine($" - {sample}");
        }
    }

    if (result.Ec2ImportedSamples.Count > 0)
    {
        Console.WriteLine("Ec2ImportedSamples:");
        foreach (var sample in result.Ec2ImportedSamples)
        {
            Console.WriteLine($" - {sample}");
        }
    }
}

void RunEc2Diagnostics()
{
    PrintRuntimeConfigDiagnostics();
    using var conn = factory.CreateOpenConnection();
    ProductionSchemaMigrator.Ensure(conn);

    var latestImport = conn.QueryFirstOrDefault<Ec2ImportProbeRow>(
        @"
            SELECT
                Id,
                COALESCE(SourceType, 'Administrator') AS SourceType,
                COALESCE(SourceFile, '') AS SourceFile,
                COALESCE(FileHash, '') AS FileHash,
                COALESCE(FileLastWriteTime, '') AS FileLastWriteTime,
                COALESCE(ImportedAt, '') AS ImportedAt,
                RowsRead,
                RowsImported,
                RowsIgnored
            FROM Ec2AdministratorImports
            ORDER BY datetime(ImportedAt) DESC, Id DESC
            LIMIT 1;");

    var current = conn.Query<Ec2CurrentProbeRow>(
            @"
                SELECT
                    COALESCE(AreaLabel, '') AS AreaLabel,
                    COALESCE(MachineCode, '') AS MachineCode,
                    COALESCE(ImportId, 0) AS ImportId,
                    COALESCE(SourceType, 'Administrator') AS SourceType,
                    COALESCE(IsStale, 0) AS IsStale,
                    COALESCE(StatusText, '') AS StatusText,
                    COALESCE(IsRunning, 0) AS IsRunning,
                    COALESCE(IsIgnored, 0) AS IsIgnored,
                    COALESCE(IgnoreReason, '') AS IgnoreReason,
                    COALESCE(PartCode, '') AS PartCode,
                    ProcessMinutes,
                    COALESCE(LotNo, '') AS LotNo,
                    COALESCE(SnapshotAt, '') AS SnapshotAt,
                    COALESCE(LastSeenAt, '') AS LastSeenAt,
                    COALESCE(StoppedSinceAt, '') AS StoppedSinceAt
                FROM Ec2MachineCurrentState
                ORDER BY AreaLabel, MachineCode;")
        .ToList();
    var latestImportId = latestImport?.Id ?? 0;
    var latestMachineCodes = latestImportId <= 0
        ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        : conn.Query<string>(
                "SELECT upper(trim(MachineCode)) FROM Ec2MachineSnapshots WHERE ImportId = @importId;",
                new { importId = latestImportId })
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    var lastImportMachines = latestMachineCodes.Count;
    var staleRows = current
        .Where(row => row.SourceType.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
        .Where(row => latestImportId > 0 && !latestMachineCodes.Contains(NormalizeProbeCode(row.MachineCode)))
        .ToList();

    var areaCount = current
        .Select(row => row.AreaLabel)
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Count();
    var running = current.Count(row => row.IsRunning == 1);
    var ignored = current.Count(row => row.IsIgnored == 1);
    var stopped = Math.Max(0, current.Count - running);
    var auditRows = current
        .Select(row => BuildEc2AverageAuditRow(row, latestMachineCodes, latestImportId))
        .ToList();
    var operatingProcessRows = auditRows
        .Where(row => row.EnteredAverage)
        .ToList();
    var operatingProcessSum = operatingProcessRows.Sum(row => row.TimeRead!.Value);
    var averageProcess = operatingProcessRows.Count == 0
        ? 0d
        : Math.Round(operatingProcessSum / operatingProcessRows.Count, 1);
    var currentStateStrictRows = current
        .Where(row => row.IsRunning == 1
            && row.IsIgnored == 0
            && row.ProcessMinutes.HasValue
            && double.IsFinite(row.ProcessMinutes.Value)
            && row.ProcessMinutes.Value > 0)
        .ToList();
    var currentStateStrictSum = currentStateStrictRows.Sum(row => row.ProcessMinutes!.Value);
    var currentStateStrictAverage = currentStateStrictRows.Count == 0
        ? 0d
        : Math.Round(currentStateStrictSum / currentStateStrictRows.Count, 1);

    Console.WriteLine("=== EC2 ADMINISTRATOR DIAGNOSTICS ===");
    Console.WriteLine($"Database={settings.DatabasePath}");
    Console.WriteLine($"LastImportId={latestImport?.Id.ToString(CultureInfo.InvariantCulture) ?? "(none)"}");
    Console.WriteLine($"LastImportSourceType={latestImport?.SourceType ?? "(none)"}");
    Console.WriteLine($"LastFile={latestImport?.SourceFile ?? "(none)"}");
    Console.WriteLine($"LastHash={latestImport?.FileHash ?? "(none)"}");
    Console.WriteLine($"LastFileWriteTime={latestImport?.FileLastWriteTime ?? "(none)"}");
    Console.WriteLine($"LastImportedAt={latestImport?.ImportedAt ?? "(none)"}");
    Console.WriteLine($"RowsRead={latestImport?.RowsRead ?? 0}");
    Console.WriteLine($"RowsImported={latestImport?.RowsImported ?? 0}");
    Console.WriteLine($"RowsIgnored={latestImport?.RowsIgnored ?? 0}");
    Console.WriteLine($"CurrentStateTotalMachines={current.Count}");
    Console.WriteLine($"LastImportMachines={lastImportMachines}");
    Console.WriteLine($"StaleMachinesFromOlderImports={staleRows.Count}");
    Console.WriteLine($"Areas={areaCount}");
    Console.WriteLine($"Machines={current.Count}");
    Console.WriteLine($"Running={running}");
    Console.WriteLine($"Stopped={stopped}");
    Console.WriteLine($"Ignored={ignored}");
    Console.WriteLine($"AverageOperatingProcessMinutes={averageProcess:F1}");
    Console.WriteLine("AverageRule=IsRunning=1 AND IsIgnored=0 AND ProcessMinutes>0 AND finite AND PresentInLastAdministratorImport");
    Console.WriteLine($"Sum={operatingProcessSum:F1}");
    Console.WriteLine($"Count={operatingProcessRows.Count}");
    Console.WriteLine($"Excluded={auditRows.Count - operatingProcessRows.Count}");
    Console.WriteLine($"AverageCurrentStateStrict={currentStateStrictAverage:F1}");
    Console.WriteLine($"CurrentStateStrictSum={currentStateStrictSum:F1}");
    Console.WriteLine($"CurrentStateStrictCount={currentStateStrictRows.Count}");

    Console.WriteLine();
    Console.WriteLine("AverageAuditRows:");
    foreach (var row in auditRows)
    {
        Console.WriteLine($"  Machine={row.MachineCode} Status={row.StatusText} TimeRead={row.TimeRead?.ToString("0.0", CultureInfo.InvariantCulture) ?? "null"} EnteredAverage={(row.EnteredAverage ? "sim" : "nao")} Reason={row.Reason}");
    }

    Console.WriteLine();
    Console.WriteLine("StaleMachines:");
    if (staleRows.Count == 0)
    {
        Console.WriteLine("  (none)");
    }
    else
    {
        foreach (var row in staleRows)
        {
            var sourceFile = conn.ExecuteScalar<string>(
                    "SELECT COALESCE(SourceFile, '') FROM Ec2AdministratorImports WHERE Id = @importId;",
                    new { importId = row.ImportId }) ?? string.Empty;
            Console.WriteLine($"  Machine={row.MachineCode} ImportId={(row.ImportId <= 0 ? "(unknown)" : row.ImportId.ToString(CultureInfo.InvariantCulture))} SourceFile={(string.IsNullOrWhiteSpace(sourceFile) ? "(unknown)" : sourceFile)} LastSeenAt={row.LastSeenAt}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("CurrentStateSample:");
    foreach (var row in current.Take(20))
    {
        Console.WriteLine($"  Area={row.AreaLabel} Machine={row.MachineCode} ImportId={(row.ImportId <= 0 ? "(unknown)" : row.ImportId.ToString(CultureInfo.InvariantCulture))} Stale={row.IsStale} Status={row.StatusText} Running={row.IsRunning} Ignored={row.IsIgnored} Part={row.PartCode} Lot={row.LotNo} Process={row.ProcessMinutes?.ToString("0.0", CultureInfo.InvariantCulture) ?? ""} Snapshot={row.SnapshotAt} Reason={row.IgnoreReason}");
    }

    Console.WriteLine();
    Console.WriteLine("PartCodeStyles:");
    foreach (var style in conn.Query<PartCodeStyleProbeRow>(
        @"
            SELECT
                PartCode,
                ColorHex,
                TextColorHex,
                COALESCE(Description, '') AS Description,
                COALESCE(IsActive, 1) AS IsActive
            FROM ProductionPartCodeStyles
            ORDER BY PartCode;"))
    {
        Console.WriteLine($"  {style.PartCode}: {style.ColorHex}/{style.TextColorHex} Active={style.IsActive} {style.Description}");
    }
}

static Ec2AverageAuditProbeRow BuildEc2AverageAuditRow(
    Ec2CurrentProbeRow row,
    IReadOnlySet<string> latestMachineCodes,
    long latestImportId)
{
    var normalizedMachine = NormalizeProbeCode(row.MachineCode);
    var presentInLatestImport = latestImportId <= 0 || latestMachineCodes.Contains(normalizedMachine);

    if (!presentInLatestImport)
    {
        return new Ec2AverageAuditProbeRow(row.MachineCode, row.StatusText, row.ProcessMinutes, false, "stale_from_older_import");
    }

    if (row.IsStale == 1)
    {
        return new Ec2AverageAuditProbeRow(row.MachineCode, row.StatusText, row.ProcessMinutes, false, "stale_current_state");
    }

    if (row.IsIgnored == 1)
    {
        var reason = string.IsNullOrWhiteSpace(row.IgnoreReason) ? "no_reason" : row.IgnoreReason;
        return new Ec2AverageAuditProbeRow(row.MachineCode, row.StatusText, row.ProcessMinutes, false, $"ec2_ignored:{reason}");
    }

    if (row.IsRunning != 1)
    {
        return new Ec2AverageAuditProbeRow(row.MachineCode, row.StatusText, row.ProcessMinutes, false, "machine_not_running");
    }

    if (!row.ProcessMinutes.HasValue)
    {
        return new Ec2AverageAuditProbeRow(row.MachineCode, row.StatusText, row.ProcessMinutes, false, "process_time_null");
    }

    var processMinutes = row.ProcessMinutes.Value;
    if (!double.IsFinite(processMinutes))
    {
        return new Ec2AverageAuditProbeRow(row.MachineCode, row.StatusText, row.ProcessMinutes, false, "process_time_not_finite");
    }

    if (processMinutes <= 0)
    {
        return new Ec2AverageAuditProbeRow(row.MachineCode, row.StatusText, row.ProcessMinutes, false, "process_time_not_positive");
    }

    return new Ec2AverageAuditProbeRow(row.MachineCode, row.StatusText, row.ProcessMinutes, true, "ok");
}

static string NormalizeProbeCode(string value)
{
    return string.IsNullOrWhiteSpace(value)
        ? string.Empty
        : value.Trim().ToUpperInvariant();
}

void RunEc2ResetLatest()
{
    PrintRuntimeConfigDiagnostics();

    var ec2FilePath = ConfigurationManager.AppSettings["Ec2AdministratorFilePath"] ?? string.Empty;
    if (string.IsNullOrWhiteSpace(ec2FilePath))
    {
        Console.WriteLine("Ec2AdministratorFilePath vazio; nada para limpar.");
        return;
    }

    using var conn = factory.CreateOpenConnection();
    ProductionSchemaMigrator.Ensure(conn);

    var latest = conn.QueryFirstOrDefault<Ec2ImportProbeRow>(
        @"
            SELECT
                Id,
                COALESCE(SourceType, 'Administrator') AS SourceType,
                COALESCE(SourceFile, '') AS SourceFile,
                COALESCE(FileHash, '') AS FileHash,
                COALESCE(FileLastWriteTime, '') AS FileLastWriteTime,
                COALESCE(ImportedAt, '') AS ImportedAt,
                RowsRead,
                RowsImported,
                RowsIgnored
            FROM Ec2AdministratorImports
            WHERE SourceFile = @sourceFile
            ORDER BY datetime(ImportedAt) DESC, Id DESC
            LIMIT 1;",
        new
        {
            sourceFile = ec2FilePath
        });

    if (latest == null)
    {
        Console.WriteLine($"Nenhum import EC2 encontrado para {ec2FilePath}.");
        return;
    }

    using var tx = conn.BeginTransaction();
    var snapshotsDeleted = conn.Execute("DELETE FROM Ec2MachineSnapshots WHERE ImportId = @importId;", new { importId = latest.Id }, tx);
    var importsDeleted = conn.Execute("DELETE FROM Ec2AdministratorImports WHERE Id = @importId;", new { importId = latest.Id }, tx);
    tx.Commit();

    Console.WriteLine("=== EC2 RESET LATEST ===");
    Console.WriteLine($"SourceFile={latest.SourceFile}");
    Console.WriteLine($"ImportId={latest.Id}");
    Console.WriteLine($"FileHash={latest.FileHash}");
    Console.WriteLine($"SnapshotsDeleted={snapshotsDeleted}");
    Console.WriteLine($"ImportsDeleted={importsDeleted}");
}

void PrintImportPerformance(TeamOps.Core.Entities.ProductionImportResult result)
{
    if (result.PerformanceMs.Count == 0)
    {
        return;
    }

    Console.WriteLine("Import performance:");
    foreach (var item in result.PerformanceMs.OrderBy(item => item.Key.Equals("Total", StringComparison.OrdinalIgnoreCase) ? 1 : 0).ThenBy(item => item.Key))
    {
        Console.WriteLine($"{item.Key}={item.Value}ms");
    }
}

void RunDbIndexCheck()
{
    using var conn = factory.CreateOpenConnection();
    ProductionSchemaMigrator.Ensure(conn);

    var requiredIndexes = new[]
    {
        "IX_MachineStatuses_Sector_StatusCode",
        "IX_MachineStatuses_SectorId",
        "IX_MachineStatuses_StatusCode",
        "IX_MachineEvents_UniqueEvent",
        "IX_MachineEvents_UniqueRawEvent",
        "IX_MachineEvents_EventDateTime",
        "IX_MachineEvents_Machine_EventTime",
        "IX_Machines_MachineKey_Unique",
        "IX_Machines_MachineCode_LineCode",
        "IX_Ec2AdministratorImports_FileHash",
        "IX_Ec2MachineSnapshots_Machine",
        "IX_Ec2MachineSnapshots_Area",
        "IX_Ec2MachineSnapshots_Status",
        "IX_Ec2MachineSnapshots_PartCode",
        "IX_Ec2MachineCurrentState_MachineId",
        "IX_Ec2MachineCurrentState_PartCode"
    };

    var indexes = conn.Query<IndexProbeRow>(
            @"
                SELECT
                    name AS Name,
                    tbl_name AS TableName,
                    COALESCE(sql, '') AS Sql
                FROM sqlite_master
                WHERE type = 'index'
                  AND name NOT LIKE 'sqlite_autoindex%'
                ORDER BY tbl_name, name;")
        .ToList();

    Console.WriteLine("=== DB INDEX CHECK ===");
    Console.WriteLine($"Database={settings.DatabasePath}");
    foreach (var indexName in requiredIndexes)
    {
        Console.WriteLine($"{indexName}={(indexes.Any(item => item.Name.Equals(indexName, StringComparison.OrdinalIgnoreCase)) ? "OK" : "MISSING")}");
    }

    Console.WriteLine();
    Console.WriteLine("Indexes:");
    foreach (var index in indexes)
    {
        Console.WriteLine($"{index.TableName}.{index.Name}: {index.Sql}");
    }
}

void RunSchemaValidation(bool repair)
{
    using var conn = factory.CreateOpenConnection();

    var before = InspectProductionSchema(conn);
    if (repair)
    {
        ProductionSchemaMigrator.Ensure(conn);
        RepairProductionStatusSeeds(conn);
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
    RequireTable(conn, "Ec2AdministratorImports", issues);
    RequireTable(conn, "Ec2MachineSnapshots", issues);
    RequireTable(conn, "Ec2MachineCurrentState", issues);
    RequireTable(conn, "ProductionPartCodeStyles", issues);

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
    RequireColumn(conn, "Ec2MachineCurrentState", "MachineCode", issues);
    RequireColumn(conn, "Ec2AdministratorImports", "SourceType", issues);
    RequireColumn(conn, "Ec2MachineCurrentState", "ImportId", issues);
    RequireColumn(conn, "Ec2MachineCurrentState", "SourceType", issues);
    RequireColumn(conn, "Ec2MachineCurrentState", "IsStale", issues);
    RequireColumn(conn, "Ec2MachineCurrentState", "MachineId", issues);
    RequireColumn(conn, "Ec2MachineCurrentState", "StatusText", issues);
    RequireColumn(conn, "Ec2MachineCurrentState", "IsRunning", issues);
    RequireColumn(conn, "Ec2MachineCurrentState", "IsIgnored", issues);
    RequireColumn(conn, "Ec2MachineCurrentState", "StoppedSinceAt", issues);
    RequireColumn(conn, "ProductionPartCodeStyles", "PartCode", issues);
    RequireColumn(conn, "ProductionPartCodeStyles", "ColorHex", issues);

    RequireIndex(conn, "IX_MachineStatuses_Sector_StatusCode", issues);
    RequireIndex(conn, "IX_MachineEvents_UniqueEvent", issues);
    RequireIndex(conn, "IX_MachineEvents_UniqueRawEvent", issues);
    RequireIndex(conn, "IX_MachineEvents_EventDateTime", issues);
    RequireIndex(conn, "IX_MachineEvents_Machine_EventTime", issues);
    RequireIndex(conn, "IX_Machines_MachineKey_Unique", issues);
    RequireIndex(conn, "IX_Machines_MachineCode_LineCode", issues);
    RequireIndex(conn, "IX_Ec2AdministratorImports_FileHash", issues);
    RequireIndex(conn, "IX_Ec2MachineSnapshots_Machine", issues);
    RequireIndex(conn, "IX_Ec2MachineCurrentState_MachineId", issues);

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
        var dadNoCountRows = conn.Query<StatusDefinitionProbeRow>(
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
                WHERE SectorId = @DadSectorId
                  AND StatusCode IN (3, 17, 18, 19)
                ORDER BY StatusCode;",
            new
            {
                DadSectorId
            })
            .Select(row => $"{row.StatusCode}:{row.Classification}")
            .DefaultIfEmpty("(none)");

        issues.Add($"BAD_DAD_STOP_NO_COUNT_SEEDS: found {dadNoCountCount}/4 [{string.Join(", ", dadNoCountRows)}]");
    }

    return new ProductionSchemaInspection(journalMode, quickCheck, issues);
}

void RepairProductionStatusSeeds(System.Data.IDbConnection conn)
{
    conn.Execute(
        @"
            INSERT OR IGNORE INTO MachineStatuses
            (SectorId, StatusCode, DisplayCode, Classification, NamePt, NameJp, ColorHex, TextColorHex, SortOrder, IsActive)
            VALUES
            (@DadSectorId, 0, 0, 'Running', 'Rodando', 'Rodando', '#5B88E8', '#FFFFFF', 0, 1),
            (@DadSectorId, 1, 3, 'StopCounts', 'Parado DAD', 'Parado DAD', '#F2CB58', '#4A3200', 1, 1),
            (@DadSectorId, 3, 3, 'StopNoCount', 'Limpeza programada', 'Limpeza programada', '#8EC5A8', '#123524', 3, 1),
            (@DadSectorId, 4, 4, 'Error', 'Erro', 'Erro', '#FFFFFF', '#516174', 4, 1),
            (@DadSectorId, 17, 1, 'StopNoCount', 'Intervalo', 'Intervalo', '#8EC5A8', '#123524', 17, 1),
            (@DadSectorId, 18, 1, 'StopNoCount', 'Limpeza programada', 'Limpeza programada', '#8EC5A8', '#123524', 18, 1),
            (@DadSectorId, 19, 1, 'StopNoCount', 'Amostra', 'Amostra', '#8EC5A8', '#123524', 19, 1);

            UPDATE MachineStatuses
            SET DisplayCode = 0, Classification = 'Running', IsActive = 1
            WHERE SectorId = @DadSectorId AND StatusCode = 0;

            UPDATE MachineStatuses
            SET DisplayCode = 3, Classification = 'StopCounts', IsActive = 1
            WHERE SectorId = @DadSectorId AND StatusCode = 1;

            UPDATE MachineStatuses
            SET DisplayCode = 3, Classification = 'StopNoCount', IsActive = 1
            WHERE SectorId = @DadSectorId AND StatusCode = 3;

            UPDATE MachineStatuses
            SET DisplayCode = 4, Classification = 'Error', IsActive = 1
            WHERE SectorId = @DadSectorId AND StatusCode = 4;

            UPDATE MachineStatuses
            SET DisplayCode = 1, Classification = 'StopNoCount', IsActive = 1
            WHERE SectorId = @DadSectorId AND StatusCode IN (17, 18, 19);",
        new
        {
            DadSectorId
        });
}

void RunProductionDiagnostics()
{
    using var conn = factory.CreateOpenConnection();
    ProductionSchemaMigrator.Ensure(conn);

    var eventCount = conn.ExecuteScalar<long>("SELECT COUNT(1) FROM MachineEvents;");
    var currentCount = conn.ExecuteScalar<long>("SELECT COUNT(1) FROM MachineCurrentStatus;");
    var statusCount = conn.ExecuteScalar<long>("SELECT COUNT(1) FROM MachineStatuses;");
    var machineCount = conn.ExecuteScalar<long>("SELECT COUNT(1) FROM Machines WHERE COALESCE(IsActive, 1) = 1;");
    var minEvent = conn.ExecuteScalar<string>("SELECT MIN(EventDateTime) FROM MachineEvents;") ?? string.Empty;
    var maxEvent = conn.ExecuteScalar<string>("SELECT MAX(EventDateTime) FROM MachineEvents;") ?? string.Empty;

    Console.WriteLine("=== PRODUCTION DIAGNOSTICS ===");
    Console.WriteLine($"Database={settings.DatabasePath}");
    Console.WriteLine($"MachineEvents={eventCount}");
    Console.WriteLine($"MachineCurrentStatus={currentCount}");
    Console.WriteLine($"MachineStatuses={statusCount}");
    Console.WriteLine($"ActiveMachines={machineCount}");
    Console.WriteLine($"FirstEvent={minEvent}");
    Console.WriteLine($"LastEvent={maxEvent}");

    Console.WriteLine();
    Console.WriteLine("EventsByDate:");
    foreach (var row in conn.Query<EventDateCountRow>(
        @"
            SELECT substr(EventDateTime, 1, 10) AS Day, COUNT(1) AS Total
            FROM MachineEvents
            GROUP BY substr(EventDateTime, 1, 10)
            ORDER BY Day DESC
            LIMIT 10;"))
    {
        Console.WriteLine($"  {row.Day}: {row.Total}");
    }

    Console.WriteLine();
    Console.WriteLine("DadStatuses:");
    foreach (var row in conn.Query<StatusDefinitionProbeRow>(
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
            WHERE SectorId = @DadSectorId
            ORDER BY StatusCode;",
        new
        {
            DadSectorId
        }))
    {
        Console.WriteLine($"  Sector={row.SectorId} Code={row.StatusCode} Display={row.DisplayCode} Classification={row.Classification} Name={row.NamePt}");
    }

    var latestDate = conn.ExecuteScalar<string>("SELECT substr(MAX(EventDateTime), 1, 10) FROM MachineEvents;") ?? string.Empty;
    if (!DateTime.TryParse(latestDate, out var dashboardDate))
    {
        Console.WriteLine();
        Console.WriteLine("DashboardLatestDate=none");
        return;
    }

    var shift = conn.QueryFirstOrDefault<ShiftRow>(
        @"
            SELECT Id, COALESCE(NamePt, '') AS NamePt, COALESCE(NULLIF(NameJp, ''), NamePt, '') AS NameJp
            FROM Shifts
            ORDER BY Id;");
    if (shift == null)
    {
        Console.WriteLine("DashboardLatestDate=no-shift");
        return;
    }

    var dashboard = analytics.BuildDashboard(new ProductionDashboardFilter
    {
        Date = dashboardDate.Date,
        ShiftId = shift.Id
    });

    Console.WriteLine();
    Console.WriteLine($"DashboardLatestDate={dashboardDate:yyyy-MM-dd}");
    Console.WriteLine($"DashboardShift={shift.Id} {shift.NamePt}");
    Console.WriteLine($"DashboardMachines={dashboard.Machines.Count}");
    Console.WriteLine($"DashboardAreas={dashboard.Areas.Count}");
    Console.WriteLine($"DashboardKadouritsu={dashboard.ProductionPercent:F1}");
    Console.WriteLine($"DashboardRunning={dashboard.MachinesRunning}");
    Console.WriteLine($"DashboardStopped={dashboard.MachinesStopped}");
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

void RunMachineCleanup(string[] cleanupArgs)
{
    var apply = cleanupArgs.Any(arg => arg.Equals("--apply", StringComparison.OrdinalIgnoreCase));
    using var conn = factory.CreateOpenConnection();
    ProductionSchemaMigrator.Ensure(conn);

    var rows = conn.Query<MachineCleanupProbeRow>(
            @"
                SELECT
                    m.Id,
                    COALESCE(m.MachineCode, '') AS MachineCode,
                    COALESCE(m.MachineKey, '') AS MachineKey,
                    COALESCE(m.LineCode, '') AS LineCode,
                    COALESCE(m.NamePt, '') AS NamePt,
                    COALESCE(m.NameJp, '') AS NameJp,
                    COALESCE(m.IsActive, 1) AS IsActive,
                    (
                        SELECT COUNT(1)
                        FROM MachineEvents e
                        WHERE e.MachineId = m.Id
                    ) AS EventCount,
                    (
                        SELECT COUNT(1)
                        FROM MachineCurrentStatus cs
                        WHERE cs.MachineId = m.Id
                    ) AS CurrentStatusCount
                FROM Machines m
                WHERE COALESCE(m.IsActive, 1) = 1
                  AND (
                        trim(COALESCE(NULLIF(m.MachineCode, ''), m.NamePt, '')) = ''
                        OR trim(COALESCE(NULLIF(m.MachineCode, ''), m.NamePt, '')) NOT GLOB '*[A-Za-z]*'
                        OR trim(COALESCE(NULLIF(m.MachineCode, ''), m.NamePt, '')) GLOB '*.*'
                        OR trim(COALESCE(NULLIF(m.MachineCode, ''), m.NamePt, '')) GLOB '*/*'
                        OR trim(COALESCE(NULLIF(m.MachineCode, ''), m.NamePt, '')) GLOB '*,*'
                  )
                ORDER BY m.Id;")
        .ToList();

    Console.WriteLine("=== MACHINE CLEANUP ===");
    Console.WriteLine($"Database={settings.DatabasePath}");
    Console.WriteLine($"Mode={(apply ? "APPLY" : "DRY-RUN")}");
    Console.WriteLine($"InvalidActiveMachines={rows.Count}");

    foreach (var row in rows.Take(100))
    {
        Console.WriteLine($"  Id={row.Id} Code={row.MachineCode} Key={row.MachineKey} Line={row.LineCode} NamePt={row.NamePt} Active={row.IsActive} Events={row.EventCount} Current={row.CurrentStatusCount}");
    }

    if (rows.Count > 100)
    {
        Console.WriteLine($"  ... {rows.Count - 100} registros omitidos no console.");
    }

    if (!apply || rows.Count == 0)
    {
        Console.WriteLine(apply ? "OK: nada para inativar." : "DRY-RUN: rode com --apply para inativar as maquinas invalidas.");
        return;
    }

    using var tx = conn.BeginTransaction();
    var affected = conn.Execute(
        @"
            UPDATE Machines
            SET IsActive = 0
            WHERE COALESCE(IsActive, 1) = 1
              AND (
                    trim(COALESCE(NULLIF(MachineCode, ''), NamePt, '')) = ''
                    OR trim(COALESCE(NULLIF(MachineCode, ''), NamePt, '')) NOT GLOB '*[A-Za-z]*'
                    OR trim(COALESCE(NULLIF(MachineCode, ''), NamePt, '')) GLOB '*.*'
                    OR trim(COALESCE(NULLIF(MachineCode, ''), NamePt, '')) GLOB '*/*'
                    OR trim(COALESCE(NULLIF(MachineCode, ''), NamePt, '')) GLOB '*,*'
              );",
        transaction: tx);
    tx.Commit();

    Console.WriteLine($"InactiveMachines={affected}");
    Console.WriteLine("OK: maquinas invalidas foram inativadas; eventos historicos foram preservados.");
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

void PrintRuntimeConfigDiagnostics()
{
    var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
    var ec2FilePath = ConfigurationManager.AppSettings["Ec2AdministratorFilePath"] ?? string.Empty;
    var ignoreAfter = ConfigurationManager.AppSettings["MachineStoppedIgnoreAfterMinutes"] ?? string.Empty;
    var runningKeywords = ConfigurationManager.AppSettings["Ec2AdministratorRunningStatusKeywords"] ?? string.Empty;

    Console.WriteLine("=== RUNTIME CONFIG DIAGNOSTICS ===");
    Console.WriteLine($"BaseDirectory={baseDir}");
    Console.WriteLine($"ConfigurationFile={configFile}");
    Console.WriteLine($"Ec2AdministratorFilePath={ec2FilePath}");
    var ec2ResolvedPath = string.IsNullOrWhiteSpace(ec2FilePath)
        ? string.Empty
        : Path.GetFullPath(ec2FilePath);
    var ec2Exists = !string.IsNullOrWhiteSpace(ec2FilePath) && File.Exists(ec2FilePath);
    Console.WriteLine($"Ec2AdministratorResolvedFullPath={ec2ResolvedPath}");
    Console.WriteLine($"Ec2AdministratorFileExists={(ec2Exists ? "yes" : "no")}");
    if (ec2Exists)
    {
        var fileInfo = new FileInfo(ec2ResolvedPath);
        Console.WriteLine($"Ec2AdministratorFileSize={fileInfo.Length}");
        Console.WriteLine($"Ec2AdministratorLastWriteTime={fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
    }
    Console.WriteLine($"MachineStoppedIgnoreAfterMinutes={ignoreAfter}");
    Console.WriteLine($"Ec2AdministratorRunningStatusKeywords={runningKeywords}");
    Console.WriteLine();
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

sealed class ImportProfileOptions
{
    public DateTime? Date { get; private init; }
    public int SectorId { get; private init; }
    public string SectorLabel => SectorId <= 0 ? "(all)" : SectorId == 2 ? "dad" : SectorId == 1 ? "gbareru" : SectorId.ToString(CultureInfo.InvariantCulture);

    public static ImportProfileOptions Parse(string[] args)
    {
        DateTime? date = null;
        var sectorId = 0;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index].Trim();
            if (arg.Equals("--date", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
            {
                date = DateTime.Parse(args[++index], CultureInfo.InvariantCulture);
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
        }

        return new ImportProfileOptions
        {
            Date = date,
            SectorId = sectorId
        };
    }
}

sealed class IndexProbeRow
{
    public string Name { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string Sql { get; set; } = string.Empty;
}

sealed class Ec2ImportProbeRow
{
    public long Id { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public string SourceFile { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty;
    public string FileLastWriteTime { get; set; } = string.Empty;
    public string ImportedAt { get; set; } = string.Empty;
    public int RowsRead { get; set; }
    public int RowsImported { get; set; }
    public int RowsIgnored { get; set; }
}

sealed class Ec2CurrentProbeRow
{
    public string AreaLabel { get; set; } = string.Empty;
    public string MachineCode { get; set; } = string.Empty;
    public long ImportId { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public int IsStale { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public int IsRunning { get; set; }
    public int IsIgnored { get; set; }
    public string IgnoreReason { get; set; } = string.Empty;
    public string PartCode { get; set; } = string.Empty;
    public double? ProcessMinutes { get; set; }
    public string LotNo { get; set; } = string.Empty;
    public string SnapshotAt { get; set; } = string.Empty;
    public string LastSeenAt { get; set; } = string.Empty;
    public string StoppedSinceAt { get; set; } = string.Empty;
}

sealed record Ec2AverageAuditProbeRow(
    string MachineCode,
    string StatusText,
    double? TimeRead,
    bool EnteredAverage,
    string Reason);

sealed class PartCodeStyleProbeRow
{
    public string PartCode { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public string TextColorHex { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int IsActive { get; set; }
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

sealed class EventDateCountRow
{
    public string Day { get; set; } = string.Empty;
    public long Total { get; set; }
}

sealed class MachineCleanupProbeRow
{
    public int Id { get; set; }
    public string MachineCode { get; set; } = string.Empty;
    public string MachineKey { get; set; } = string.Empty;
    public string LineCode { get; set; } = string.Empty;
    public string NamePt { get; set; } = string.Empty;
    public string NameJp { get; set; } = string.Empty;
    public int IsActive { get; set; }
    public long EventCount { get; set; }
    public long CurrentStatusCount { get; set; }
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
