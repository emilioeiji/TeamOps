using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dapper;
using TeamOps.Core.Entities;

namespace TeamOps.Services
{
    internal sealed class ProductionPlanDatImporter
    {
        private static readonly Regex MachineCodeRegex = new(@"^[A-Z]\d{2,3}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex AreaRegex = new(@"(エリア|AREA)\s*\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly string[] ExportTimestampLabels =
        {
            "最終出力日時",
            "最新出力日時"
        };
        private static readonly string[] LastUpdatedLabels =
        {
            "最終更新日時",
            "最新更新日時"
        };

        public void ImportLatestPlanFiles(
            IDbConnection conn,
            IDbTransaction tx,
            string eventsDirectory,
            string sourceDatDirectory,
            ProductionImportResult result)
        {
            var files = EnumerateCandidateFiles(eventsDirectory, sourceDatDirectory);
            foreach (var filePath in files)
            {
                result.PlanFilesRead++;
                ImportSingleFile(conn, tx, filePath, result);
            }
        }

        private static List<string> EnumerateCandidateFiles(string eventsDirectory, string sourceDatDirectory)
        {
            var candidates = new List<string>();

            if (!string.IsNullOrWhiteSpace(sourceDatDirectory) && Directory.Exists(sourceDatDirectory))
            {
                candidates.AddRange(Directory.GetFiles(sourceDatDirectory, "*.dat", SearchOption.TopDirectoryOnly));
            }

            if (!string.IsNullOrWhiteSpace(eventsDirectory) && Directory.Exists(eventsDirectory))
            {
                candidates.AddRange(Directory.GetFiles(eventsDirectory, "*.dat", SearchOption.TopDirectoryOnly));
            }

            return candidates
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(path => File.GetLastWriteTime(path))
                .Take(6)
                .ToList();
        }

        private static void ImportSingleFile(IDbConnection conn, IDbTransaction tx, string filePath, ProductionImportResult result)
        {
            var rows = ReadTabularLines(filePath);
            if (rows.Count == 0)
            {
                result.PlanRowsIgnored++;
                PushError(result, $"{Path.GetFileName(filePath)}: arquivo DAT vazio ou sem tabulacao.");
                return;
            }

            var exportedAt = ExtractTimestamp(rows, ExportTimestampLabels);
            var lastUpdatedAt = ExtractTimestamp(rows, LastUpdatedLabels);
            var snapshotId = EnsureSnapshot(conn, tx, filePath, exportedAt, lastUpdatedAt);
            if (snapshotId <= 0)
            {
                return;
            }

            string currentArea = string.Empty;
            Dictionary<string, int>? headerMap = null;

            foreach (var row in rows)
            {
                if (TryResolveAreaLabel(row, out var areaLabel))
                {
                    currentArea = areaLabel;
                    headerMap = null;
                    continue;
                }

                if (TryBuildHeaderMap(row, out var candidateHeader))
                {
                    headerMap = candidateHeader;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(currentArea) || headerMap == null)
                {
                    continue;
                }

                if (!TryBuildPlanRow(currentArea, row, headerMap, out var parsed))
                {
                    continue;
                }

                var affected = conn.Execute(
                    @"
                        INSERT OR IGNORE INTO ProductionPlanRows
                        (
                            SnapshotId,
                            AreaLabel,
                            MachineCode,
                            AssignmentText,
                            PlannedProcessMinutes,
                            MachineStatusText,
                            CapabilityFrame,
                            WorkType,
                            TargetKadouritsu,
                            CurrentDifference,
                            LotNo,
                            CycleEndAt,
                            DailyPlannedQuantity,
                            DailyEstimatedQuantity,
                            EstimatedKadouritsu,
                            RawColumnsJson,
                            ImportedAt
                        )
                        VALUES
                        (
                            @SnapshotId,
                            @AreaLabel,
                            @MachineCode,
                            @AssignmentText,
                            @PlannedProcessMinutes,
                            @MachineStatusText,
                            @CapabilityFrame,
                            @WorkType,
                            @TargetKadouritsu,
                            @CurrentDifference,
                            @LotNo,
                            @CycleEndAt,
                            @DailyPlannedQuantity,
                            @DailyEstimatedQuantity,
                            @EstimatedKadouritsu,
                            @RawColumnsJson,
                            @ImportedAt
                        );",
                    new
                    {
                        SnapshotId = snapshotId,
                        parsed.AreaLabel,
                        parsed.MachineCode,
                        parsed.AssignmentText,
                        parsed.PlannedProcessMinutes,
                        parsed.MachineStatusText,
                        parsed.CapabilityFrame,
                        parsed.WorkType,
                        parsed.TargetKadouritsu,
                        parsed.CurrentDifference,
                        parsed.LotNo,
                        CycleEndAt = parsed.CycleEndAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                        parsed.DailyPlannedQuantity,
                        parsed.DailyEstimatedQuantity,
                        parsed.EstimatedKadouritsu,
                        parsed.RawColumnsJson,
                        ImportedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    },
                    tx
                );

                if (affected > 0)
                {
                    result.PlanRowsImported++;
                }
                else
                {
                    result.PlanRowsIgnored++;
                }
            }
        }

        private static int EnsureSnapshot(IDbConnection conn, IDbTransaction tx, string filePath, DateTime? exportedAt, DateTime? lastUpdatedAt)
        {
            var sourceFile = Path.GetFileName(filePath);
            var exportIso = exportedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty;

            conn.Execute(
                @"
                    INSERT OR IGNORE INTO ProductionPlanSnapshots
                    (
                        SourceFile,
                        ExportedAt,
                        LastUpdatedAt,
                        ImportedAt
                    )
                    VALUES
                    (
                        @SourceFile,
                        @ExportedAt,
                        @LastUpdatedAt,
                        @ImportedAt
                    );",
                new
                {
                    SourceFile = sourceFile,
                    ExportedAt = string.IsNullOrWhiteSpace(exportIso) ? null : exportIso,
                    LastUpdatedAt = lastUpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                    ImportedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                },
                tx
            );

            return conn.ExecuteScalar<int>(
                @"
                    SELECT Id
                    FROM ProductionPlanSnapshots
                    WHERE SourceFile = @SourceFile
                      AND COALESCE(ExportedAt, '') = COALESCE(@ExportedAt, '')
                    ORDER BY Id DESC
                    LIMIT 1;",
                new
                {
                    SourceFile = sourceFile,
                    ExportedAt = string.IsNullOrWhiteSpace(exportIso) ? null : exportIso
                },
                tx
            );
        }

        private static List<string[]> ReadTabularLines(string filePath)
        {
            var raw = File.ReadAllBytes(filePath);
            foreach (var encoding in new[] { Encoding.Unicode, Encoding.UTF8, Encoding.GetEncoding(932), Encoding.Default })
            {
                var text = encoding.GetString(raw);
                if (!text.Contains('\t'))
                {
                    continue;
                }

                return text
                    .Replace("\r\n", "\n", StringComparison.Ordinal)
                    .Replace('\r', '\n')
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Split('\t').Select(value => (value ?? string.Empty).Trim()).ToArray())
                    .ToList();
            }

            return new List<string[]>();
        }

        private static DateTime? ExtractTimestamp(IEnumerable<string[]> rows, IEnumerable<string> labels)
        {
            foreach (var row in rows)
            {
                for (var i = 0; i < row.Length; i++)
                {
                    var cell = row[i];
                    if (string.IsNullOrWhiteSpace(cell))
                    {
                        continue;
                    }

                    foreach (var label in labels)
                    {
                        if (!cell.Contains(label, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var candidate = cell.Replace(label, string.Empty, StringComparison.OrdinalIgnoreCase)
                            .Replace(":", string.Empty, StringComparison.Ordinal)
                            .Trim();

                        if (TryParseDateTime(candidate, out var parsed))
                        {
                            return parsed;
                        }

                        if (i + 1 < row.Length && TryParseDateTime(row[i + 1], out parsed))
                        {
                            return parsed;
                        }
                    }
                }
            }

            return null;
        }

        private static bool TryResolveAreaLabel(string[] row, out string areaLabel)
        {
            foreach (var cell in row)
            {
                if (string.IsNullOrWhiteSpace(cell))
                {
                    continue;
                }

                var match = AreaRegex.Match(cell);
                if (match.Success)
                {
                    areaLabel = match.Value.Replace("AREA", "Area", StringComparison.OrdinalIgnoreCase);
                    return true;
                }
            }

            areaLabel = string.Empty;
            return false;
        }

        private static bool TryBuildHeaderMap(string[] row, out Dictionary<string, int> headerMap)
        {
            headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < row.Length; i++)
            {
                var key = NormalizeHeader(row[i]);
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                headerMap[key] = i;
            }

            return headerMap.ContainsKey("設備号表示")
                   || headerMap.ContainsKey("予定加工時間")
                   || headerMap.ContainsKey("加工ロットNo")
                   || headerMap.ContainsKey("設定稼働率");
        }

        private static bool TryBuildPlanRow(
            string areaLabel,
            string[] row,
            IReadOnlyDictionary<string, int> headerMap,
            out ParsedPlanRow parsed)
        {
            parsed = default;

            var machineCode = row.FirstOrDefault(cell => MachineCodeRegex.IsMatch(cell ?? string.Empty))?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(machineCode))
            {
                return false;
            }

            var rawColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in headerMap)
            {
                rawColumns[item.Key] = GetValue(row, item.Value);
            }

            parsed = new ParsedPlanRow
            {
                AreaLabel = areaLabel,
                MachineCode = machineCode,
                AssignmentText = GetValue(row, headerMap, "配台指示"),
                PlannedProcessMinutes = ParseNullableDouble(GetValue(row, headerMap, "予定加工時間")),
                MachineStatusText = GetValue(row, headerMap, "設備状態"),
                CapabilityFrame = GetValue(row, headerMap, "能力枠"),
                WorkType = GetValue(row, headerMap, "タイプ"),
                TargetKadouritsu = ParseNullableDouble(GetValue(row, headerMap, "設定稼働率")),
                CurrentDifference = ParseNullableDouble(GetValue(row, headerMap, "現状差異")),
                LotNo = GetValue(row, headerMap, "加工ロットNo"),
                CycleEndAt = ParseNullableDateTime(GetValue(row, headerMap, "サイクル終了予定加工時間")),
                DailyPlannedQuantity = ParseNullableDouble(
                    GetValue(row, headerMap, "day加工可能総数量")
                    ?? GetValue(row, headerMap, "day生産数量(K個)")
                ),
                DailyEstimatedQuantity = ParseNullableDouble(
                    GetValue(row, headerMap, "day生産数量(K個)")
                    ?? GetValue(row, headerMap, "day生産数量")
                ),
                EstimatedKadouritsu = ParseNullableDouble(
                    GetValue(row, headerMap, "推定稼働率")
                    ?? GetValue(row, headerMap, "day生産数量(K個)推定稼働率")
                ),
                RawColumnsJson = JsonSerializer.Serialize(rawColumns)
            };

            return true;
        }

        private static string GetValue(string[] row, IReadOnlyDictionary<string, int> headerMap, string key)
        {
            if (!headerMap.TryGetValue(key, out var index))
            {
                return string.Empty;
            }

            return GetValue(row, index);
        }

        private static string GetValue(string[] row, int index)
        {
            return index >= 0 && index < row.Length
                ? (row[index] ?? string.Empty).Trim()
                : string.Empty;
        }

        private static string NormalizeHeader(string value)
        {
            return (value ?? string.Empty)
                .Trim()
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .Replace("　", string.Empty, StringComparison.Ordinal)
                .Replace(".", string.Empty, StringComparison.Ordinal)
                .Replace("№", "No", StringComparison.Ordinal);
        }

        private static bool TryParseDateTime(string value, out DateTime parsed)
        {
            return DateTime.TryParseExact(
                       value?.Trim() ?? string.Empty,
                       "yyyy/MM/dd H:mm:ss",
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out parsed)
                   || DateTime.TryParseExact(
                       value?.Trim() ?? string.Empty,
                       "yyyy/MM/dd HH:mm:ss",
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out parsed);
        }

        private static DateTime? ParseNullableDateTime(string value)
        {
            return TryParseDateTime(value, out var parsed) ? parsed : null;
        }

        private static double? ParseNullableDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim().Replace(",", ".", StringComparison.Ordinal);
            return double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }

        private static void PushError(ProductionImportResult result, string message)
        {
            if (!string.IsNullOrWhiteSpace(message) && result.Errors.Count < 30)
            {
                result.Errors.Add(message);
            }
        }

        private struct ParsedPlanRow
        {
            public string AreaLabel { get; set; }
            public string MachineCode { get; set; }
            public string AssignmentText { get; set; }
            public double? PlannedProcessMinutes { get; set; }
            public string MachineStatusText { get; set; }
            public string CapabilityFrame { get; set; }
            public string WorkType { get; set; }
            public double? TargetKadouritsu { get; set; }
            public double? CurrentDifference { get; set; }
            public string LotNo { get; set; }
            public DateTime? CycleEndAt { get; set; }
            public double? DailyPlannedQuantity { get; set; }
            public double? DailyEstimatedQuantity { get; set; }
            public double? EstimatedKadouritsu { get; set; }
            public string RawColumnsJson { get; set; }
        }
    }
}
