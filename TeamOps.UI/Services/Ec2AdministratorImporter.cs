using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dapper;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;

namespace TeamOps.Services
{
    internal sealed class Ec2AdministratorImporter
    {
        private static readonly StringComparer TextComparer = StringComparer.OrdinalIgnoreCase;

        private readonly SqliteConnectionFactory _factory;
        private readonly ProductionMachineRepository _machineRepository;

        public Ec2AdministratorImporter(
            SqliteConnectionFactory factory,
            ProductionMachineRepository machineRepository)
        {
            _factory = factory;
            _machineRepository = machineRepository;
        }

        public void ImportIfConfigured(ProductionImportResult result)
        {
            var settings = LoadSettings();
            if (string.IsNullOrWhiteSpace(settings.FilePath))
            {
                result.Ec2ImportSkipped = true;
                result.Ec2ImportMessage = "EC2 Administrator nao configurado.";
                return;
            }

            result.Ec2ImportAttempted = true;

            if (!File.Exists(settings.FilePath))
            {
                result.Ec2ImportSkipped = true;
                result.Ec2ImportMessage = $"Arquivo EC2 Administrator nao encontrado: {settings.FilePath}";
                return;
            }

            try
            {
                ImportFile(settings, result);
            }
            catch (IOException ex)
            {
                result.Ec2ImportSkipped = true;
                result.Ec2ImportMessage = $"EC2 Administrator nao importado: arquivo indisponivel ({ex.Message}).";
                PushError(result, result.Ec2ImportMessage);
            }
            catch (UnauthorizedAccessException ex)
            {
                result.Ec2ImportSkipped = true;
                result.Ec2ImportMessage = $"EC2 Administrator nao importado: acesso negado ({ex.Message}).";
                PushError(result, result.Ec2ImportMessage);
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 5 || ex.SqliteErrorCode == 6)
            {
                result.Ec2ImportSkipped = true;
                result.Ec2ImportMessage = $"EC2 Administrator nao importado: SQLite ocupado ({ex.Message}).";
                PushError(result, result.Ec2ImportMessage);
            }
            catch (Exception ex)
            {
                result.Ec2ImportSkipped = true;
                result.Ec2ImportMessage = $"EC2 Administrator nao importado: {ex.Message}";
                PushError(result, result.Ec2ImportMessage);
            }
        }

        private void ImportFile(Ec2ImportSettings settings, ProductionImportResult result)
        {
            var totalWatch = Stopwatch.StartNew();
            var fileInfo = new FileInfo(settings.FilePath);

            var readWatch = Stopwatch.StartNew();
            var bytes = File.ReadAllBytes(settings.FilePath);
            var fileHash = Convert.ToHexString(SHA256.HashData(bytes));
            var lines = DecodeLines(bytes);
            Record(result, "Ec2ReadFile", readWatch);

            if (LooksLikeProductionEventFile(fileInfo, lines))
            {
                result.Ec2ImportSkipped = true;
                result.Ec2ImportMessage = "Arquivo ignorado: parece ser arquivo de eventos de producao, nao EC2 Administrator.";
                PushError(result, $"EC2 Administrator nao importado: {fileInfo.FullName} parece arquivo de eventos.");
                return;
            }

            if (lines.Count == 0)
            {
                result.Ec2ImportSkipped = true;
                result.Ec2ImportMessage = "Arquivo EC2 Administrator vazio.";
                return;
            }

            var parseWatch = Stopwatch.StartNew();
            var parsedRows = LooksLikeAdministratorDatSample(lines)
                ? ParseAdministratorDatSampleRows(lines, fileInfo.LastWriteTime, settings, result)
                : ParseRows(lines, fileInfo.LastWriteTime, settings, result);
            Record(result, "Ec2Parse", parseWatch);
            result.Ec2AreaCount = parsedRows
                .Select(row => row.AreaLabel)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            using var conn = _factory.CreateOpenConnection();
            ProductionSchemaMigrator.Ensure(conn);

            var existingImportId = conn.ExecuteScalar<long?>(
                "SELECT Id FROM Ec2AdministratorImports WHERE FileHash = @fileHash AND COALESCE(SourceType, 'Administrator') = 'Administrator' ORDER BY Id DESC LIMIT 1;",
                new
                {
                    fileHash
                });

            if (existingImportId.HasValue)
            {
                ReconcileCurrentStateForImport(conn, null, existingImportId.Value, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                result.Ec2ImportSkipped = true;
                result.Ec2ImportMessage = "EC2 Administrator sem alteracao.";
                return;
            }

            var dbWatch = Stopwatch.StartNew();
            using var tx = conn.BeginTransaction();

            var importId = conn.ExecuteScalar<long>(
                @"
                    INSERT INTO Ec2AdministratorImports
                    (
                        SourceType,
                        SourceFile,
                        FileLastWriteTime,
                        FileLength,
                        FileHash,
                        ImportedAt,
                        RowsRead,
                        RowsImported,
                        RowsIgnored
                    )
                    VALUES
                    (
                        'Administrator',
                        @sourceFile,
                        @fileLastWriteTime,
                        @fileLength,
                        @fileHash,
                        @importedAt,
                        @rowsRead,
                        0,
                        @rowsIgnored
                    );
                    SELECT last_insert_rowid();",
                new
                {
                    sourceFile = settings.FilePath,
                    fileLastWriteTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    fileLength = fileInfo.Length,
                    fileHash,
                    importedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    rowsRead = result.Ec2RowsRead,
                    rowsIgnored = result.Ec2RowsIgnored
                },
                tx);

            var existingStates = conn.Query<Ec2CurrentStateRow>(
                    @"
                        SELECT
                            MachineCode,
                            IsRunning,
                            StoppedSinceAt,
                            SnapshotAt
                        FROM Ec2MachineCurrentState;",
                    transaction: tx)
                .ToDictionary(row => NormalizeCode(row.MachineCode), StringComparer.OrdinalIgnoreCase);

            var snapshots = new List<Ec2WriteRow>();
            var currentStates = new List<Ec2WriteRow>();
            var snapshotAt = fileInfo.LastWriteTime;

            foreach (var parsed in parsedRows)
            {
                var machine = _machineRepository.GetByMachineCode(conn, parsed.MachineCode)
                    ?? _machineRepository.EnsureMachine(conn, parsed.MachineCode, "EC2", parsed.SectorId);

                var machineCode = NormalizeCode(parsed.MachineCode);
                existingStates.TryGetValue(machineCode, out var previousState);
                var stoppedSince = ResolveStoppedSince(parsed, previousState, snapshotAt);
                var isIgnored = ShouldIgnore(parsed, stoppedSince, snapshotAt, settings.StoppedIgnoreAfterMinutes);
                var ignoreReason = isIgnored
                    ? $"EC2_STOPPED_{settings.StoppedIgnoreAfterMinutes}_MIN"
                    : string.Empty;
                var rawJson = JsonSerializer.Serialize(parsed.RawColumns);

                var row = new Ec2WriteRow
                {
                    ImportId = importId,
                    MachineId = machine.Id,
                    SectorId = parsed.SectorId ?? machine.SectorId,
                    LocalId = machine.LocalId,
                    AreaLabel = parsed.AreaLabel,
                    MachineCode = machineCode,
                    MachineName = parsed.MachineName,
                    StatusText = parsed.StatusText,
                    IsRunning = parsed.IsRunning ? 1 : 0,
                    IsIgnored = isIgnored ? 1 : 0,
                    IgnoreReason = ignoreReason,
                    PartCode = parsed.PartCode,
                    PlannedProcessMinutes = parsed.PlannedProcessMinutes,
                    CapabilityType = parsed.CapabilityType,
                    OperationRate = parsed.OperationRate,
                    CurrentDifference = parsed.CurrentDifference,
                    LotNo = parsed.LotNo,
                    PlannedEndAt = parsed.PlannedEndAt?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    ProcessMinutes = parsed.ProcessMinutes,
                    DailyProduction = parsed.DailyProduction,
                    RawColumnsJson = rawJson,
                    SnapshotAt = snapshotAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    ImportedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    StoppedSinceAt = stoppedSince?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                };

                snapshots.Add(row);
                currentStates.Add(row);
            }

            if (snapshots.Count > 0)
            {
                conn.Execute(
                    @"
                        INSERT INTO Ec2MachineSnapshots
                        (
                            ImportId,
                            MachineId,
                            SectorId,
                            LocalId,
                            AreaLabel,
                            MachineCode,
                            MachineName,
                            StatusText,
                            IsRunning,
                            IsIgnored,
                            IgnoreReason,
                            PartCode,
                            PlannedProcessMinutes,
                            CapabilityType,
                            OperationRate,
                            CurrentDifference,
                            LotNo,
                            PlannedEndAt,
                            ProcessMinutes,
                            DailyProduction,
                            RawColumnsJson,
                            SnapshotAt,
                            ImportedAt
                        )
                        VALUES
                        (
                            @ImportId,
                            @MachineId,
                            @SectorId,
                            @LocalId,
                            @AreaLabel,
                            @MachineCode,
                            @MachineName,
                            @StatusText,
                            @IsRunning,
                            @IsIgnored,
                            @IgnoreReason,
                            @PartCode,
                            @PlannedProcessMinutes,
                            @CapabilityType,
                            @OperationRate,
                            @CurrentDifference,
                            @LotNo,
                            @PlannedEndAt,
                            @ProcessMinutes,
                            @DailyProduction,
                            @RawColumnsJson,
                            @SnapshotAt,
                            @ImportedAt
                        );",
                    snapshots,
                    tx);

                conn.Execute(
                    @"
                        INSERT INTO Ec2MachineCurrentState
                        (
                            MachineCode,
                            ImportId,
                            SourceType,
                            IsStale,
                            MachineId,
                            SectorId,
                            LocalId,
                            AreaLabel,
                            MachineName,
                            StatusText,
                            IsRunning,
                            IsIgnored,
                            IgnoreReason,
                            PartCode,
                            PlannedProcessMinutes,
                            CapabilityType,
                            OperationRate,
                            CurrentDifference,
                            LotNo,
                            PlannedEndAt,
                            ProcessMinutes,
                            DailyProduction,
                            RawColumnsJson,
                            SnapshotAt,
                            ImportedAt,
                            LastSeenAt,
                            StoppedSinceAt
                        )
                        VALUES
                        (
                            @MachineCode,
                            @ImportId,
                            @SourceType,
                            @IsStale,
                            @MachineId,
                            @SectorId,
                            @LocalId,
                            @AreaLabel,
                            @MachineName,
                            @StatusText,
                            @IsRunning,
                            @IsIgnored,
                            @IgnoreReason,
                            @PartCode,
                            @PlannedProcessMinutes,
                            @CapabilityType,
                            @OperationRate,
                            @CurrentDifference,
                            @LotNo,
                            @PlannedEndAt,
                            @ProcessMinutes,
                            @DailyProduction,
                            @RawColumnsJson,
                            @SnapshotAt,
                            @ImportedAt,
                            @SnapshotAt,
                            @StoppedSinceAt
                        )
                        ON CONFLICT(MachineCode) DO UPDATE SET
                            ImportId = excluded.ImportId,
                            SourceType = excluded.SourceType,
                            IsStale = excluded.IsStale,
                            MachineId = excluded.MachineId,
                            SectorId = excluded.SectorId,
                            LocalId = excluded.LocalId,
                            AreaLabel = excluded.AreaLabel,
                            MachineName = excluded.MachineName,
                            StatusText = excluded.StatusText,
                            IsRunning = excluded.IsRunning,
                            IsIgnored = excluded.IsIgnored,
                            IgnoreReason = excluded.IgnoreReason,
                            PartCode = excluded.PartCode,
                            PlannedProcessMinutes = excluded.PlannedProcessMinutes,
                            CapabilityType = excluded.CapabilityType,
                            OperationRate = excluded.OperationRate,
                            CurrentDifference = excluded.CurrentDifference,
                            LotNo = excluded.LotNo,
                            PlannedEndAt = excluded.PlannedEndAt,
                            ProcessMinutes = excluded.ProcessMinutes,
                            DailyProduction = excluded.DailyProduction,
                            RawColumnsJson = excluded.RawColumnsJson,
                            SnapshotAt = excluded.SnapshotAt,
                            ImportedAt = excluded.ImportedAt,
                            LastSeenAt = excluded.LastSeenAt,
                            StoppedSinceAt = excluded.StoppedSinceAt;",
                    currentStates,
                    tx);

                MarkMachinesMissingFromLatestSnapshot(conn, tx, importId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            }

            conn.Execute(
                @"
                    UPDATE Ec2AdministratorImports
                    SET
                        RowsImported = @rowsImported,
                        RowsIgnored = @rowsIgnored
                    WHERE Id = @importId;",
                new
                {
                    importId,
                    rowsImported = currentStates.Count,
                    rowsIgnored = result.Ec2RowsIgnored
                },
                tx);

            tx.Commit();
            result.Ec2RowsImported = currentStates.Count;
            result.Ec2RunningCount = currentStates.Count(item => item.IsRunning == 1);
            result.Ec2IgnoredCount = currentStates.Count(item => item.IsIgnored == 1);
            result.Ec2StoppedCount = Math.Max(0, currentStates.Count - result.Ec2RunningCount);
            result.Ec2ImportMessage = $"EC2 Administrator: {currentStates.Count} linha(s) importada(s).";
            Record(result, "Ec2WriteDb", dbWatch);
            Record(result, "Ec2Total", totalWatch);
        }

        private static DateTime? ResolveStoppedSince(
            Ec2ParsedRow row,
            Ec2CurrentStateRow? previousState,
            DateTime snapshotAt)
        {
            if (row.IsRunning)
            {
                return null;
            }

            if (previousState != null
                && previousState.IsRunning == 0
                && DateTime.TryParse(previousState.StoppedSinceAt, out var parsedStoppedSince))
            {
                return parsedStoppedSince;
            }

            if (previousState != null
                && previousState.IsRunning == 0
                && DateTime.TryParse(previousState.SnapshotAt, out var parsedSnapshotAt))
            {
                return parsedSnapshotAt;
            }

            return snapshotAt;
        }

        private static bool ShouldIgnore(
            Ec2ParsedRow row,
            DateTime? stoppedSince,
            DateTime snapshotAt,
            int stoppedIgnoreAfterMinutes)
        {
            if (row.IsRunning || !stoppedSince.HasValue)
            {
                return false;
            }

            return (snapshotAt - stoppedSince.Value).TotalMinutes >= stoppedIgnoreAfterMinutes;
        }

        private static void ReconcileCurrentStateForImport(
            System.Data.IDbConnection conn,
            System.Data.IDbTransaction? tx,
            long importId,
            string importedAt)
        {
            conn.Execute(
                @"
                    UPDATE Ec2MachineCurrentState
                    SET
                        ImportId = @importId,
                        SourceType = 'Administrator',
                        IsStale = 0
                    WHERE EXISTS (
                        SELECT 1
                        FROM Ec2MachineSnapshots s
                        WHERE s.ImportId = @importId
                          AND upper(trim(s.MachineCode)) = upper(trim(Ec2MachineCurrentState.MachineCode))
                    );",
                new { importId },
                tx);

            MarkMachinesMissingFromLatestSnapshot(conn, tx, importId, importedAt);
        }

        private static void MarkMachinesMissingFromLatestSnapshot(
            System.Data.IDbConnection conn,
            System.Data.IDbTransaction? tx,
            long importId,
            string importedAt)
        {
            conn.Execute(
                @"
                    UPDATE Ec2MachineCurrentState
                    SET
                        ImportId = COALESCE((
                            SELECT s.ImportId
                            FROM Ec2MachineSnapshots s
                            WHERE upper(trim(s.MachineCode)) = upper(trim(Ec2MachineCurrentState.MachineCode))
                            ORDER BY s.ImportId DESC
                            LIMIT 1
                        ), ImportId),
                        SourceType = 'Administrator',
                        IsStale = 1,
                        IsIgnored = 1,
                        IgnoreReason = 'NOT_PRESENT_IN_LATEST_ADMINISTRATOR_SNAPSHOT',
                        IsRunning = 0,
                        ImportedAt = @importedAt
                    WHERE COALESCE(SourceType, 'Administrator') = 'Administrator'
                      AND NOT EXISTS (
                          SELECT 1
                          FROM Ec2MachineSnapshots s
                          WHERE s.ImportId = @importId
                            AND upper(trim(s.MachineCode)) = upper(trim(Ec2MachineCurrentState.MachineCode))
                      );",
                new
                {
                    importId,
                    importedAt
                },
                tx);
        }

        private static bool LooksLikeProductionEventFile(FileInfo fileInfo, IReadOnlyList<string> lines)
        {
            if (Regex.IsMatch(fileInfo.Name, @"^\d{6}_\d{4}_E\.txt$", RegexOptions.IgnoreCase))
            {
                return true;
            }

            var firstDataLine = lines.FirstOrDefault(line => !string.IsNullOrWhiteSpace(line));
            if (string.IsNullOrWhiteSpace(firstDataLine))
            {
                return false;
            }

            var columns = firstDataLine.Split('|');
            return columns.Length >= 12
                && string.Equals(columns[0].Trim(), "E", StringComparison.OrdinalIgnoreCase)
                && DateTime.TryParse(columns[1].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }

        private static List<Ec2ParsedRow> ParseRows(
            IReadOnlyList<string> lines,
            DateTime snapshotAt,
            Ec2ImportSettings settings,
            ProductionImportResult result)
        {
            var rows = new List<Ec2ParsedRow>();
            var header = SplitCsvLine(lines[0]);
            var hasHeader = LooksLikeHeader(header);
            var map = hasHeader
                ? BuildHeaderMap(header)
                : new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var startIndex = hasHeader ? 1 : 0;

            for (var index = startIndex; index < lines.Count; index++)
            {
                result.Ec2RowsRead++;
                var columns = SplitCsvLine(lines[index]);
                if (columns.Count == 0 || columns.All(string.IsNullOrWhiteSpace))
                {
                    result.Ec2RowsIgnored++;
                    continue;
                }

                // Real EC2 layout used in production:
                // 0=type, 1=date, 2=time, 3=line code, 4=machine code, 5=internal state,
                // 7=event date, 8=event time, 9=status text, 10=recipe/area, 11=time value, 12=lot/code.
                var machineCode = Pick(columns, map, 4, "equipment", "machine", "machinecode", "equipamento", "maquina", "machine code");
                if (string.IsNullOrWhiteSpace(machineCode))
                {
                    result.Ec2RowsIgnored++;
                    PushError(result, $"EC2 linha {index + 1}: equipamento vazio.");
                    continue;
                }

                var statusText = Pick(columns, map, 9, "status", "machinestatus", "estado", "status text", "internal state");
                var areaLabel = Pick(columns, map, 10, "area", "local", "recipe", "recipeName", "part", "partcode", "code", "codigo", "código", "instruction");
                var partCode = Pick(columns, map, 10, "part", "partcode", "code", "codigo", "código", "instruction", "recipe", "recipeName");

                rows.Add(new Ec2ParsedRow
                {
                    AreaLabel = areaLabel,
                    MachineCode = machineCode,
                    MachineName = machineCode,
                    StatusText = statusText,
                    IsRunning = IsRunningStatus(statusText, settings.RunningKeywords),
                    SectorId = ResolveSectorId(areaLabel),
                    PartCode = partCode,
                    PlannedProcessMinutes = ParseDouble(Pick(columns, map, 2, "plannedprocessminutes", "tempo previsto", "tempo")),
                    CapabilityType = Pick(columns, map, 5, "capability", "capacity", "tipo"),
                    OperationRate = ParseDouble(Pick(columns, map, 6, "operationrate", "taxa", "稼働率")),
                    CurrentDifference = ParseDouble(Pick(columns, map, 6, "difference", "diferença", "差")),
                    LotNo = Pick(columns, map, 12, "lot", "lotno", "lote", "ロット"),
                    PlannedEndAt = ParseDateTime(Pick(columns, map, 7, "plannedend", "end", "termino", "término", "終了予定"), snapshotAt),
                    ProcessMinutes = ParseDouble(Pick(columns, map, 11, "processminutes", "processingtime", "tempo processamento", "処理時間")),
                    DailyProduction = ParseDouble(Pick(columns, map, 11, "dailyproduction", "produção diária", "daily")),
                    RawColumns = BuildRawColumns(header, columns, hasHeader)
                });
            }

            return rows;
        }

        private static List<string> DecodeLines(byte[] bytes)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            foreach (var encoding in new Encoding[] { new UTF8Encoding(false, true), Encoding.GetEncoding(932), Encoding.Default })
            {
                try
                {
                    var text = encoding.GetString(bytes);
                    if (text.Contains(',') || text.Contains('\t') || text.Contains(';') || text.Contains('|'))
                    {
                        return text
                            .Replace("\r\n", "\n", StringComparison.Ordinal)
                            .Replace('\r', '\n')
                            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                            .ToList();
                    }
                }
                catch (DecoderFallbackException)
                {
                    // Try the next encoding.
                }
            }

            return new List<string>();
        }

        private static List<string> SplitCsvLine(string line)
        {
            var separator = line.Contains('\t')
                ? '\t'
                : line.Contains('|')
                    ? '|'
                : line.Contains(';') && !line.Contains(',')
                    ? ';'
                    : ',';
            var values = new List<string>();
            var current = new StringBuilder();
            var quoted = false;

            for (var index = 0; index < line.Length; index++)
            {
                var ch = line[index];
                if (ch == '"')
                {
                    if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                    {
                        current.Append('"');
                        index++;
                    }
                    else
                    {
                        quoted = !quoted;
                    }
                    continue;
                }

                if (ch == separator && !quoted)
                {
                    values.Add(current.ToString().Trim());
                    current.Clear();
                    continue;
                }

                current.Append(ch);
            }

            values.Add(current.ToString().Trim());
            return values;
        }

        private static bool LooksLikeHeader(IReadOnlyList<string> columns)
        {
            var joined = string.Join(" ", columns).ToLowerInvariant();
            return joined.Contains("area", StringComparison.Ordinal)
                || joined.Contains("equipment", StringComparison.Ordinal)
                || joined.Contains("status", StringComparison.Ordinal)
                || joined.Contains("maquina", StringComparison.Ordinal)
                || joined.Contains("máquina", StringComparison.Ordinal)
                || joined.Contains("設備", StringComparison.Ordinal)
                || joined.Contains("状態", StringComparison.Ordinal);
        }

        private static Dictionary<string, int> BuildHeaderMap(IReadOnlyList<string> header)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < header.Count; index++)
            {
                var key = NormalizeHeader(header[index]);
                if (!string.IsNullOrWhiteSpace(key) && !map.ContainsKey(key))
                {
                    map[key] = index;
                }
            }

            return map;
        }

        private static string Pick(
            IReadOnlyList<string> columns,
            IReadOnlyDictionary<string, int> map,
            int fallbackIndex,
            params string[] names)
        {
            foreach (var name in names)
            {
                if (map.TryGetValue(NormalizeHeader(name), out var index)
                    && index >= 0
                    && index < columns.Count)
                {
                    return columns[index].Trim();
                }
            }

            return fallbackIndex >= 0 && fallbackIndex < columns.Count
                ? columns[fallbackIndex].Trim()
                : string.Empty;
        }

        private static Dictionary<string, string> BuildRawColumns(
            IReadOnlyList<string> header,
            IReadOnlyList<string> columns,
            bool hasHeader)
        {
            var raw = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < columns.Count; index++)
            {
                var key = hasHeader && index < header.Count && !string.IsNullOrWhiteSpace(header[index])
                    ? header[index].Trim()
                    : $"Column{index + 1}";
                raw[key] = columns[index];
            }

            return raw;
        }

        private static string NormalizeHeader(string value)
        {
            return (value ?? string.Empty)
                .Trim()
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .ToLowerInvariant();
        }

        private static bool IsRunningStatus(string statusText, IReadOnlyCollection<string> runningKeywords)
        {
            var normalized = (statusText ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            return runningKeywords.Any(keyword => normalized.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private static double? ParseDouble(string value)
        {
            var normalized = (value ?? string.Empty)
                .Trim()
                .Replace("%", string.Empty, StringComparison.Ordinal)
                .Replace(",", ".", StringComparison.Ordinal);

            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }

        private static DateTime? ParseDateTime(string value, DateTime snapshotAt)
        {
            var normalized = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return null;
            }

            if (DateTime.TryParse(normalized, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                return parsed.Year <= 1
                    ? snapshotAt.Date.Add(parsed.TimeOfDay)
                    : parsed;
            }

            return TimeSpan.TryParse(normalized, CultureInfo.InvariantCulture, out var time)
                ? snapshotAt.Date.Add(time)
                : null;
        }

        private static int? ResolveSectorId(string areaLabel)
        {
            var normalized = (areaLabel ?? string.Empty).Trim();
            if (normalized.StartsWith("エリア", StringComparison.OrdinalIgnoreCase)
                || normalized.StartsWith("AREA", StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            if (normalized.Contains("DAD", StringComparison.OrdinalIgnoreCase)
                || normalized.Contains("211D", StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            if (normalized.Contains("GBARERU", StringComparison.OrdinalIgnoreCase)
                || normalized.Contains("G-BARERU", StringComparison.OrdinalIgnoreCase)
                || normalized.Contains("2400", StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            return null;
        }

        private static string NormalizeCode(string value)
        {
            return (value ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static void Record(ProductionImportResult result, string name, Stopwatch stopwatch)
        {
            stopwatch.Stop();
            result.PerformanceMs[name] = stopwatch.ElapsedMilliseconds;
        }

        private static void PushError(ProductionImportResult result, string message)
        {
            if (!string.IsNullOrWhiteSpace(message) && result.Errors.Count < 30)
            {
                result.Errors.Add(message);
            }
        }

        private static bool LooksLikeAdministratorDatSample(IReadOnlyList<string> lines)
        {
            foreach (var rawLine in lines)
            {
                var line = (rawLine ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith("※", StringComparison.Ordinal)
                    || line.StartsWith("最終", StringComparison.Ordinal)
                    || line.StartsWith("エリア", StringComparison.OrdinalIgnoreCase)
                    || line.StartsWith("AREA", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (line.Contains("設備号機", StringComparison.Ordinal)
                    && line.Contains("加工時間", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<Ec2ParsedRow> ParseAdministratorDatSampleRows(
            IReadOnlyList<string> lines,
            DateTime snapshotAt,
            Ec2ImportSettings settings,
            ProductionImportResult result)
        {
            var rows = new List<Ec2ParsedRow>();
            var currentArea = string.Empty;
            List<string> header = new();
            Dictionary<string, int>? headerMap = null;

            foreach (var rawLine in lines)
            {
                var line = (rawLine ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith("※", StringComparison.Ordinal) || line.StartsWith("最終", StringComparison.Ordinal))
                {
                    continue;
                }

                if (line.StartsWith("エリア", StringComparison.OrdinalIgnoreCase) || line.StartsWith("AREA", StringComparison.OrdinalIgnoreCase))
                {
                    currentArea = line;
                    header = new List<string>();
                    headerMap = null;
                    continue;
                }

                var columns = SplitCsvLine(line);
                if (columns.Count == 0 || columns.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                if (LooksLikeHeader(columns))
                {
                    header = columns.ToList();
                    headerMap = BuildHeaderMap(header);
                    continue;
                }

                if (headerMap == null || string.IsNullOrWhiteSpace(currentArea))
                {
                    continue;
                }

                result.Ec2RowsRead++;

                var machineCode = Pick(columns, headerMap, 1, "設備号機", "equipment", "machine", "machinecode", "machine code", "号機");
                if (string.IsNullOrWhiteSpace(machineCode))
                {
                    result.Ec2RowsIgnored++;
                    PushError(result, $"EC2 linha {result.Ec2RowsRead}: equipamento vazio.");
                    continue;
                }

                var statusText = Pick(columns, headerMap, 5, "設備状態", "status", "machinestatus", "状態", "稼働状態");
                var partCode = Pick(columns, headerMap, 3, "配合指示", "指示", "recipe", "recipeName", "part", "partcode", "code");
                var plannedProcessRaw = Pick(columns, headerMap, 4, "予定加工時間", "plannedprocessminutes", "planned process time");
                var processRaw = Pick(columns, headerMap, 12, "加工時間", "processminutes", "processingtime", "tempo processamento");
                var plannedProcessMinutes = ParseDouble(plannedProcessRaw);
                var processMinutes = ParseDouble(processRaw);

                if (!processMinutes.HasValue || processMinutes <= 0)
                {
                    processMinutes = plannedProcessMinutes;
                }

                var lotNo = Pick(columns, headerMap, 10, "加工ロットNo.", "lot", "lotno", "lote", "ロット");
                var plannedEndText = Pick(columns, headerMap, 11, "サイクル終了予定時刻", "plannedend", "end", "termino", "término");
                var dailyProduction = ParseDouble(Pick(columns, headerMap, 13, "day生産数量(K個)", "dailyproduction", "day"));
                var operationRate = ParseDouble(Pick(columns, headerMap, 8, "設定稼働率", "operationrate", "稼働率"));
                var currentDifference = ParseDouble(Pick(columns, headerMap, 9, "現状差異", "difference", "差"));
                var isRunning = IsRunningStatus(statusText, settings.RunningKeywords);
                var includeInAverage = isRunning
                    && processMinutes.HasValue
                    && double.IsFinite(processMinutes.Value)
                    && processMinutes.Value > 0;
                var exclusionReason = includeInAverage
                    ? "ok"
                    : !isRunning
                        ? "machine_not_running"
                        : !processMinutes.HasValue
                            ? "process_time_null"
                            : !double.IsFinite(processMinutes.Value)
                                ? "process_time_not_finite"
                                : "process_time_not_positive";

                Console.WriteLine(
                    $"[EC2 Administrator Sample] Machine={machineCode} Status={statusText} Code={partCode} Lot={lotNo} TimeRaw={(string.IsNullOrWhiteSpace(processRaw) ? plannedProcessRaw : processRaw)} TimeConverted={(processMinutes.HasValue ? processMinutes.Value.ToString("0.0", CultureInfo.InvariantCulture) : "null")} EnteredAverage={(includeInAverage ? "sim" : "nao")} Reason={(includeInAverage ? "ok" : exclusionReason)}");

                rows.Add(new Ec2ParsedRow
                {
                    AreaLabel = currentArea,
                    MachineCode = machineCode,
                    MachineName = machineCode,
                    StatusText = statusText,
                    IsRunning = isRunning,
                    SectorId = ResolveSectorId(currentArea),
                    PartCode = partCode,
                    PlannedProcessMinutes = plannedProcessMinutes,
                    CapabilityType = Pick(columns, headerMap, 6, "能力枠", "capability", "capacity", "tipo"),
                    OperationRate = operationRate,
                    CurrentDifference = currentDifference,
                    LotNo = lotNo,
                    PlannedEndAt = ParseDateTime(plannedEndText, snapshotAt),
                    ProcessMinutes = processMinutes,
                    DailyProduction = dailyProduction,
                    RawColumns = BuildRawColumns(header, columns, true)
                });
            }

            return rows;
        }

        private static Ec2ImportSettings LoadSettings()
        {
            var filePath = ConfigurationManager.AppSettings["Ec2AdministratorFilePath"] ?? string.Empty;
            var ignoreAfterText = ConfigurationManager.AppSettings["MachineStoppedIgnoreAfterMinutes"] ?? "0";
            var keywordsText = ConfigurationManager.AppSettings["Ec2AdministratorRunningStatusKeywords"]
                ?? "operando,rodando,running,run,稼働,運転";

            if (!int.TryParse(ignoreAfterText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ignoreAfter)
                || ignoreAfter < 0)
            {
                ignoreAfter = 0;
            }

            var keywords = keywordsText
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new Ec2ImportSettings(filePath, ignoreAfter, keywords);
        }

        private readonly record struct Ec2ImportSettings(
            string FilePath,
            int StoppedIgnoreAfterMinutes,
            IReadOnlyCollection<string> RunningKeywords);

        private sealed class Ec2ParsedRow
        {
            public string AreaLabel { get; set; } = string.Empty;
            public string MachineCode { get; set; } = string.Empty;
            public string MachineName { get; set; } = string.Empty;
            public string StatusText { get; set; } = string.Empty;
            public bool IsRunning { get; set; }
            public int? SectorId { get; set; }
            public string PartCode { get; set; } = string.Empty;
            public double? PlannedProcessMinutes { get; set; }
            public string CapabilityType { get; set; } = string.Empty;
            public double? OperationRate { get; set; }
            public double? CurrentDifference { get; set; }
            public string LotNo { get; set; } = string.Empty;
            public DateTime? PlannedEndAt { get; set; }
            public double? ProcessMinutes { get; set; }
            public double? DailyProduction { get; set; }
            public Dictionary<string, string> RawColumns { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        }

        private sealed class Ec2CurrentStateRow
        {
            public string MachineCode { get; set; } = string.Empty;
            public int IsRunning { get; set; }
            public string StoppedSinceAt { get; set; } = string.Empty;
            public string SnapshotAt { get; set; } = string.Empty;
        }

        private sealed class Ec2WriteRow
        {
            public long ImportId { get; set; }
            public string SourceType { get; set; } = "Administrator";
            public int IsStale { get; set; }
            public int MachineId { get; set; }
            public int? SectorId { get; set; }
            public int? LocalId { get; set; }
            public string AreaLabel { get; set; } = string.Empty;
            public string MachineCode { get; set; } = string.Empty;
            public string MachineName { get; set; } = string.Empty;
            public string StatusText { get; set; } = string.Empty;
            public int IsRunning { get; set; }
            public int IsIgnored { get; set; }
            public string IgnoreReason { get; set; } = string.Empty;
            public string PartCode { get; set; } = string.Empty;
            public double? PlannedProcessMinutes { get; set; }
            public string CapabilityType { get; set; } = string.Empty;
            public double? OperationRate { get; set; }
            public double? CurrentDifference { get; set; }
            public string LotNo { get; set; } = string.Empty;
            public string? PlannedEndAt { get; set; }
            public double? ProcessMinutes { get; set; }
            public double? DailyProduction { get; set; }
            public string RawColumnsJson { get; set; } = string.Empty;
            public string SnapshotAt { get; set; } = string.Empty;
            public string ImportedAt { get; set; } = string.Empty;
            public string? StoppedSinceAt { get; set; }
        }
    }
}
