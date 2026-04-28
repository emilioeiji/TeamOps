using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;

namespace TeamOps.Services
{
    public sealed class ProductionFileImporter
    {
        private static readonly string[] FileSuffixes = { "211D", "2400" };

        private readonly SqliteConnectionFactory _factory;
        private readonly ProductionMachineRepository _machineRepository;
        private readonly ProductionEventRepository _eventRepository;

        public ProductionFileImporter(
            SqliteConnectionFactory factory,
            ProductionMachineRepository machineRepository,
            ProductionEventRepository eventRepository)
        {
            _factory = factory;
            _machineRepository = machineRepository;
            _eventRepository = eventRepository;
        }

        public ProductionImportResult ImportLatest()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var settings = LoadSettings();
            var result = new ProductionImportResult();

            ExecuteBatchIfConfigured(settings, result);

            var files = new[]
                {
                    DateTime.Today.AddDays(-1),
                    DateTime.Today
                }
                .SelectMany(date => FileSuffixes.Select(suffix => Path.Combine(settings.EventsDirectory, $"{date:yyMMdd}_{suffix}_E.txt")))
                .Where(File.Exists)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            using var conn = _factory.CreateOpenConnection();
            ProductionSchemaMigrator.Ensure(conn);
            using var tx = conn.BeginTransaction();

            foreach (var filePath in files)
            {
                result.FilesRead++;

                foreach (var line in ReadAllLines(filePath))
                {
                    result.LinesRead++;

                    if (!TryParseLine(filePath, line, out var parsed, out var ignoreReason))
                    {
                        result.Ignored++;
                        PushError(result, ignoreReason);
                        continue;
                    }

                    try
                    {
                        var existed = _machineRepository.GetByMachineCode(conn, parsed.MachineCode) != null;
                        var machine = _machineRepository.EnsureMachine(conn, parsed.MachineCode, parsed.LineCode);

                        if (!existed)
                        {
                            result.MachinesCreated++;
                        }

                        var machineEvent = new MachineEvent
                        {
                            MachineId = machine.Id,
                            MachineCode = parsed.MachineCode,
                            LineCode = parsed.LineCode,
                            LocalId = machine.LocalId,
                            SectorId = machine.SectorId,
                            RecipeName = parsed.RecipeName,
                            LotNo = parsed.LotNo,
                            StatusCode = parsed.StatusCode,
                            StatusText = parsed.StatusText,
                            InternalState = parsed.InternalState,
                            EventDateTime = parsed.EventDateTime,
                            SourceFile = Path.GetFileName(filePath),
                            ImportedAt = DateTime.Now
                        };

                        if (_eventRepository.InsertOrIgnore(conn, tx, machineEvent))
                        {
                            result.Imported++;
                        }
                        else
                        {
                            result.Ignored++;
                        }

                        _eventRepository.RefreshCurrentStatus(conn, tx, machine.Id);
                    }
                    catch (Exception ex)
                    {
                        result.Ignored++;
                        PushError(result, $"{Path.GetFileName(filePath)}: {ex.Message}");
                    }
                }
            }

            tx.Commit();
            return result;
        }

        private static string[] ReadAllLines(string filePath)
        {
            var raw = File.ReadAllBytes(filePath);

            foreach (var encoding in new[] { Encoding.UTF8, Encoding.GetEncoding(932), Encoding.Default })
            {
                var text = encoding.GetString(raw);
                if (text.Contains('|'))
                {
                    return text
                        .Replace("\r\n", "\n", StringComparison.Ordinal)
                        .Replace('\r', '\n')
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries);
                }
            }

            return Array.Empty<string>();
        }

        private static bool TryParseLine(
            string filePath,
            string line,
            out ParsedLine parsed,
            out string ignoreReason)
        {
            parsed = default;
            ignoreReason = string.Empty;

            if (string.IsNullOrWhiteSpace(line))
            {
                ignoreReason = $"{Path.GetFileName(filePath)}: linha vazia.";
                return false;
            }

            var parts = line.Split('|');
            if (parts.Length < 10)
            {
                ignoreReason = $"{Path.GetFileName(filePath)}: linha invalida.";
                return false;
            }

            var lineCode = Safe(parts, 3);
            var machineCode = Safe(parts, 4);
            var internalState = Safe(parts, 5);
            var eventDate = Safe(parts, 7);
            var eventTime = Safe(parts, 8);
            var statusText = Safe(parts, 9);
            var recipeName = Safe(parts, 10);
            var lotNo = Safe(parts, 12);

            if (string.IsNullOrWhiteSpace(lineCode) || string.IsNullOrWhiteSpace(machineCode))
            {
                ignoreReason = $"{Path.GetFileName(filePath)}: codigo de linha ou maquina vazio.";
                return false;
            }

            if (!DateTime.TryParseExact(
                    $"{eventDate} {eventTime}",
                    "yyyy/MM/dd HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var eventDateTime))
            {
                ignoreReason = $"{Path.GetFileName(filePath)}: data/hora invalida para {machineCode}.";
                return false;
            }

            parsed = new ParsedLine
            {
                LineCode = lineCode,
                MachineCode = machineCode,
                InternalState = internalState,
                StatusCode = MapStatusCode(internalState, statusText),
                StatusText = statusText,
                RecipeName = recipeName,
                LotNo = lotNo,
                EventDateTime = eventDateTime
            };

            return true;
        }

        private static int MapStatusCode(string internalState, string statusText)
        {
            if (int.TryParse(internalState, out var parsed))
            {
                return parsed switch
                {
                    0 => 0,
                    1 => 1,
                    2 => 2,
                    3 => 3,
                    _ => 2
                };
            }

            return statusText switch
            {
                "稼動中" => 0,
                "運転" => 0,
                "停止" => 1,
                "トラブル" => 3,
                _ => 2
            };
        }

        private static string Safe(string[] parts, int index)
        {
            return index >= 0 && index < parts.Length
                ? (parts[index] ?? string.Empty).Trim()
                : string.Empty;
        }

        private static void PushError(ProductionImportResult result, string message)
        {
            if (!string.IsNullOrWhiteSpace(message) && result.Errors.Count < 30)
            {
                result.Errors.Add(message);
            }
        }

        private static ProductionImportSettings LoadSettings()
        {
            var eventsDirectory = ConfigurationManager.AppSettings["ProductionEventsDirectory"] ?? string.Empty;
            var batchPath = ConfigurationManager.AppSettings["ProductionImportBatchPath"] ?? string.Empty;
            var completionFile = ConfigurationManager.AppSettings["ProductionImportCompletionFile"] ?? string.Empty;
            var sourceEventsDirectory = ConfigurationManager.AppSettings["ProductionSourceEventsDirectory"] ?? string.Empty;
            var sourceDatDirectory = ConfigurationManager.AppSettings["ProductionSourceDatDirectory"] ?? string.Empty;
            var timeoutSecondsText = ConfigurationManager.AppSettings["ProductionImportTimeoutSeconds"] ?? "180";

            if (string.IsNullOrWhiteSpace(eventsDirectory))
            {
                throw new InvalidOperationException("ProductionEventsDirectory nao esta configurado no app.config.");
            }

            Directory.CreateDirectory(eventsDirectory);

            if (!int.TryParse(timeoutSecondsText, out var timeoutSeconds) || timeoutSeconds <= 0)
            {
                timeoutSeconds = 180;
            }

            return new ProductionImportSettings(
                eventsDirectory,
                batchPath,
                completionFile,
                sourceEventsDirectory,
                sourceDatDirectory,
                timeoutSeconds);
        }

        private static void ExecuteBatchIfConfigured(ProductionImportSettings settings, ProductionImportResult result)
        {
            if (string.IsNullOrWhiteSpace(settings.BatchPath))
            {
                return;
            }

            if (!File.Exists(settings.BatchPath))
            {
                throw new FileNotFoundException($"Arquivo BAT nao encontrado: {settings.BatchPath}");
            }

            if (!string.IsNullOrWhiteSpace(settings.CompletionFile))
            {
                var completionDir = Path.GetDirectoryName(settings.CompletionFile);
                if (!string.IsNullOrWhiteSpace(completionDir))
                {
                    Directory.CreateDirectory(completionDir);
                }

                if (File.Exists(settings.CompletionFile))
                {
                    File.Delete(settings.CompletionFile);
                }
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{settings.BatchPath}\"",
                WorkingDirectory = Path.GetDirectoryName(settings.BatchPath) ?? AppContext.BaseDirectory,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.Environment["TEAMOPS_PRODUCTION_EVENTS_DIR"] = settings.EventsDirectory;
            startInfo.Environment["TEAMOPS_PRODUCTION_SOURCE_EVENTS_DIR"] = settings.SourceEventsDirectory;
            startInfo.Environment["TEAMOPS_PRODUCTION_SOURCE_DAT_DIR"] = settings.SourceDatDirectory;
            startInfo.Environment["TEAMOPS_PRODUCTION_COMPLETION_FILE"] = settings.CompletionFile;

            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Nao foi possivel iniciar o BAT de importacao.");

            if (!process.WaitForExit(settings.TimeoutSeconds * 1000))
            {
                try
                {
                    process.Kill(true);
                }
                catch
                {
                    // Ignora falha ao encerrar processo.
                }

                throw new TimeoutException("O BAT de importacao excedeu o tempo limite configurado.");
            }

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"O BAT de importacao retornou codigo {process.ExitCode}.");
            }

            if (!string.IsNullOrWhiteSpace(settings.CompletionFile))
            {
                var deadline = DateTime.UtcNow.AddSeconds(settings.TimeoutSeconds);
                while (DateTime.UtcNow <= deadline)
                {
                    if (File.Exists(settings.CompletionFile))
                    {
                        result.BatchExecuted = true;
                        result.BatchMessage = File.ReadAllText(settings.CompletionFile).Trim();
                        return;
                    }

                    Thread.Sleep(500);
                }

                throw new TimeoutException("O BAT terminou, mas o arquivo de retorno nao foi encontrado.");
            }

            result.BatchExecuted = true;
            result.BatchMessage = "BAT executado com sucesso.";
        }

        private readonly record struct ProductionImportSettings(
            string EventsDirectory,
            string BatchPath,
            string CompletionFile,
            string SourceEventsDirectory,
            string SourceDatDirectory,
            int TimeoutSeconds);

        private struct ParsedLine
        {
            public string LineCode { get; set; }
            public string MachineCode { get; set; }
            public string InternalState { get; set; }
            public int StatusCode { get; set; }
            public string StatusText { get; set; }
            public string RecipeName { get; set; }
            public string LotNo { get; set; }
            public DateTime EventDateTime { get; set; }
        }
    }
}
