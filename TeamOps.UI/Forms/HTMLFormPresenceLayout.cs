using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;
using TeamOps.Services;

namespace TeamOps.UI.Forms
{
    public partial class HTMLFormPresenceLayout : Form
    {
        private static readonly SectorDefinition[] SectorDefinitions =
        {
            new(1, "G-Bareru"),
            new(2, "DAD")
        };

        private readonly SqliteConnectionFactory _factory;
        private readonly OperatorPresenceRepository _presenceRepo;
        private readonly OperatorScheduleRepository _scheduleRepo;
        private readonly OperatorRepository _operatorRepo;
        private readonly ShiftRepository _shiftRepo;
        private readonly LocalRepository _localRepo;
        private readonly int _defaultShiftId;

        public HTMLFormPresenceLayout(SqliteConnectionFactory factory, int defaultShiftId)
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            Text = L("Presenca por Setor", "セクター出勤レイアウト");

            _factory = factory;
            _defaultShiftId = defaultShiftId;
            _presenceRepo = new OperatorPresenceRepository(factory);
            _scheduleRepo = new OperatorScheduleRepository(factory);
            _operatorRepo = new OperatorRepository(factory);
            _shiftRepo = new ShiftRepository(factory);
            _localRepo = new LocalRepository(factory);

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewPresenceLayout.EnsureCoreWebView2Async(null);

            var core = webViewPresenceLayout.CoreWebView2;
            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreDevToolsEnabled = false;

            core.WebMessageReceived += WebMessageReceived;

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "presence-layout"),
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
                    case "load":
                        LoadInitialData();
                        break;

                    case "refresh":
                        SendBoard(ReadDate(root, "date"), ReadInt(root, "shiftId"));
                        break;

                    case "import_schedule":
                        ImportSchedule(ReadDate(root, "date"), ReadInt(root, "shiftId"));
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

        private void LoadInitialData()
        {
            var shifts = _shiftRepo.GetAll();
            var locals = _localRepo.GetAll();
            var today = DateTime.Today;
            var shiftId = shifts.Any(s => s.Id == _defaultShiftId)
                ? _defaultShiftId
                : shifts.FirstOrDefault()?.Id ?? 1;

            PostJson(new
            {
                type = "init",
                data = new
                {
                    locale = Program.CurrentLocale,
                    defaults = new
                    {
                        dateIso = today.ToString("yyyy-MM-dd"),
                        shiftId
                    },
                    shifts = shifts.Select(shift => new
                    {
                        id = shift.Id,
                        namePt = shift.NamePt,
                        nameJp = string.IsNullOrWhiteSpace(shift.NameJp) ? shift.NamePt : shift.NameJp
                    }),
                    locals = locals.Select(local => new
                    {
                        id = local.Id,
                        sectorId = local.SectorId,
                        namePt = local.NamePt,
                        nameJp = string.IsNullOrWhiteSpace(local.NameJp) ? local.NamePt : local.NameJp
                    }),
                    sectors = SectorDefinitions.Select(sector => new
                    {
                        id = sector.Id,
                        name = sector.Name
                    })
                }
            });

            SendBoard(today, shiftId);
        }

        private void SendBoard(DateTime date, int shiftId)
        {
            if (shiftId <= 0)
                throw new InvalidOperationException(L("Selecione um turno valido.", "有効なシフトを選択してください。"));

            var operators = _operatorRepo.GetAll()
                .GroupBy(op => op.CodigoFJ, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
            var locals = _localRepo.GetAll()
                .ToDictionary(local => local.Id);

            var schedules = _scheduleRepo.GetByDateShift(date, shiftId);

            var sectorStates = SectorDefinitions
                .Select(sector =>
                {
                    var sectorSchedule = schedules
                        .Where(item => item.SectorId == sector.Id)
                        .ToList();

                    var sectorPresence = _presenceRepo.GetLatestByDateSectorShift(date, sector.Id, shiftId);

                    return BuildSectorState(sector, sectorSchedule, sectorPresence, operators, locals);
                })
                .ToList();

            PostJson(new
            {
                type = "board",
                data = new
                {
                    dateIso = date.ToString("yyyy-MM-dd"),
                    shiftId,
                    summary = BuildSummary(sectorStates),
                    sectors = sectorStates.Select(state => new
                    {
                        id = state.Definition.Id,
                        name = state.Definition.Name,
                        summary = new
                        {
                            plannedCount = state.PlannedKeys.Count,
                            confirmedCount = state.ConfirmedKeys.Count,
                            missingCount = state.MissingKeys.Count,
                            extraCount = state.ExtraKeys.Count
                        },
                        locals = state.Locals
                    })
                }
            });
        }

        private void ImportSchedule(DateTime date, int shiftId)
        {
            if (shiftId <= 0)
                throw new InvalidOperationException(L("Selecione um turno valido antes de importar.", "インポート前に有効なシフトを選択してください。"));

            var repo = new OperatorScheduleRepository(_factory);
            var service = new OperatorScheduleImportService(repo);
            var successes = new List<string>();
            var errors = new List<string>();

            foreach (var sector in SectorDefinitions)
            {
                try
                {
                    service.Import(sector.Id, shiftId, date);
                    successes.Add(sector.Name);
                }
                catch (Exception ex)
                {
                    errors.Add($"{sector.Name}: {ex.Message}");
                }
            }

            var message = BuildImportMessage(successes, errors);

            SendBoard(date, shiftId);

            PostJson(new
            {
                type = "import_result",
                data = new
                {
                    success = errors.Count == 0,
                    message
                }
            });
        }

        private static string BuildImportMessage(IReadOnlyCollection<string> successes, IReadOnlyCollection<string> errors)
        {
            if (successes.Count > 0 && errors.Count == 0)
            {
                return $"{L("Schedule importado com sucesso para", "スケジュールを正常に取り込みました")}: {string.Join(", ", successes)}.";
            }

            if (successes.Count == 0 && errors.Count > 0)
            {
                return L("Falha ao importar schedule", "スケジュールの取込に失敗しました")
                    + Environment.NewLine
                    + string.Join(Environment.NewLine, errors);
            }

            return L("Importacao parcial concluida.", "一部の取込が完了しました。")
                + Environment.NewLine
                + string.Join(Environment.NewLine, successes.Select(name => $"+ {name}"))
                + Environment.NewLine
                + string.Join(Environment.NewLine, errors.Select(error => $"- {error}"));
        }

        private static object BuildSummary(IEnumerable<SectorRuntimeState> sectorStates)
        {
            var planned = sectorStates.Sum(state => state.PlannedKeys.Count);
            var confirmed = sectorStates.Sum(state => state.ConfirmedKeys.Count);
            var missing = sectorStates.Sum(state => state.MissingKeys.Count);
            var extra = sectorStates.Sum(state => state.ExtraKeys.Count);

            return new
            {
                plannedCount = planned,
                confirmedCount = confirmed,
                missingCount = missing,
                extraCount = extra,
                imported = planned > 0
            };
        }

        private static SectorRuntimeState BuildSectorState(
            SectorDefinition definition,
            IReadOnlyCollection<OperatorSchedule> schedules,
            IReadOnlyCollection<OperatorPresence> presences,
            IReadOnlyDictionary<string, Operator> operators,
            IReadOnlyDictionary<int, Local> localsById)
        {
            var plannedByLocal = schedules
                .GroupBy(item => item.LocalId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .GroupBy(item => item.CodigoFJ, StringComparer.OrdinalIgnoreCase)
                        .Select(codeGroup => CreatePerson(codeGroup.Key, operators))
                        .OrderBy(item => item.Display, StringComparer.OrdinalIgnoreCase)
                        .ToList());

            var presentByLocal = presences
                .GroupBy(item => item.LocalId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .GroupBy(item => item.CodigoFJ, StringComparer.OrdinalIgnoreCase)
                        .Select(codeGroup => CreatePerson(codeGroup.Key, operators, codeGroup.First()))
                        .OrderBy(item => item.Display, StringComparer.OrdinalIgnoreCase)
                        .ToList());

            var localIds = plannedByLocal.Keys
                .Concat(presentByLocal.Keys)
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            var localStates = localIds.Select(localId =>
            {
                var localName = ResolveLocalName(localId, localsById);
                var planned = plannedByLocal.TryGetValue(localId, out var scheduledPeople)
                    ? scheduledPeople
                    : new List<PresencePerson>();

                var present = presentByLocal.TryGetValue(localId, out var presentPeople)
                    ? presentPeople
                    : new List<PresencePerson>();

                var plannedCodes = planned
                    .Select(item => item.CodigoFJ)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var presentCodes = present
                    .Select(item => item.CodigoFJ)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var combined = new List<object>();

                combined.AddRange(planned.Select(item => new
                {
                    codigoFJ = item.CodigoFJ,
                    display = item.Display,
                    shortName = item.ShortName,
                    status = presentCodes.Contains(item.CodigoFJ) ? "confirmed" : "planned"
                }));

                combined.AddRange(present
                    .Where(item => !plannedCodes.Contains(item.CodigoFJ))
                    .Select(item => new
                    {
                        codigoFJ = item.CodigoFJ,
                        display = item.Display,
                        shortName = item.ShortName,
                        status = "extra"
                    }));

                var confirmedCount = plannedCodes.Intersect(presentCodes, StringComparer.OrdinalIgnoreCase).Count();
                var missingCount = plannedCodes.Except(presentCodes, StringComparer.OrdinalIgnoreCase).Count();
                var extraCount = presentCodes.Except(plannedCodes, StringComparer.OrdinalIgnoreCase).Count();

                return new
                {
                    localId,
                    localName,
                    plannedCount = planned.Count,
                    confirmedCount,
                    missingCount,
                    extraCount,
                    people = combined,
                    tooltip = BuildLocalTooltip(localId, localName, planned, present)
                };
            }).ToList();

            var plannedKeys = schedules
                .Select(item => item.CodigoFJ)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var presentKeys = presences
                .Select(item => item.CodigoFJ)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return new SectorRuntimeState(
                definition,
                plannedKeys,
                presentKeys.Intersect(plannedKeys, StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase),
                plannedKeys.Except(presentKeys, StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase),
                presentKeys.Except(plannedKeys, StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase),
                localStates.Cast<object>().ToList());
        }

        private static string BuildLocalTooltip(int localId, string localName, IEnumerable<PresencePerson> planned, IEnumerable<PresencePerson> present)
        {
            var plannedList = planned.Select(item => item.Display).ToList();
            var presentList = present.Select(item => item.Display).ToList();

            var lines = new List<string>
            {
                string.IsNullOrWhiteSpace(localName) ? $"Local {localId}" : localName
            };

            lines.Add(plannedList.Count > 0
                ? $"{L("Previsto", "予定")}: {string.Join(", ", plannedList)}"
                : $"{L("Previsto", "予定")}: -");

            lines.Add(presentList.Count > 0
                ? $"{L("Confirmado", "確認済み")}: {string.Join(", ", presentList)}"
                : $"{L("Confirmado", "確認済み")}: -");

            return string.Join(Environment.NewLine, lines);
        }

        private static string ResolveLocalName(int localId, IReadOnlyDictionary<int, Local> locals)
        {
            if (!locals.TryGetValue(localId, out var local))
                return $"Local {localId}";

            var preferred = string.Equals(Program.CurrentLocale, "ja-JP", StringComparison.OrdinalIgnoreCase)
                ? local.NameJp
                : local.NamePt;

            if (!string.IsNullOrWhiteSpace(preferred))
                return preferred.Trim();

            if (!string.IsNullOrWhiteSpace(local.NamePt))
                return local.NamePt.Trim();

            if (!string.IsNullOrWhiteSpace(local.NameJp))
                return local.NameJp.Trim();

            return $"Local {localId}";
        }

        private static PresencePerson CreatePerson(
            string codigoFJ,
            IReadOnlyDictionary<string, Operator> operators,
            OperatorPresence? presence = null)
        {
            if (operators.TryGetValue(codigoFJ, out var op))
            {
                var display = string.IsNullOrWhiteSpace(op.NameRomanji) ? codigoFJ : op.NameRomanji.Trim();
                return new PresencePerson(codigoFJ, display, CreateShortName(display, codigoFJ));
            }

            if (presence != null)
            {
                var display = string.IsNullOrWhiteSpace(presence.NameRomanji)
                    ? codigoFJ
                    : presence.NameRomanji.Trim();

                return new PresencePerson(codigoFJ, display, CreateShortName(display, codigoFJ));
            }

            return new PresencePerson(codigoFJ, codigoFJ, codigoFJ);
        }

        private static string CreateShortName(string displayName, string fallback)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return fallback;

            var token = displayName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault() ?? fallback;

            return token.Length <= 8 ? token : token[..8];
        }

        private void PostJson(object payload)
        {
            var json = JsonSerializer.Serialize(payload);

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    if (webViewPresenceLayout.CoreWebView2 != null)
                        webViewPresenceLayout.CoreWebView2.PostWebMessageAsJson(json);
                }));
                return;
            }

            if (webViewPresenceLayout.CoreWebView2 != null)
                webViewPresenceLayout.CoreWebView2.PostWebMessageAsJson(json);
        }

        private static string ReadString(JsonElement root, string propertyName)
        {
            return root.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String
                ? prop.GetString() ?? string.Empty
                : string.Empty;
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

        private static DateTime ReadDate(JsonElement root, string propertyName)
        {
            var raw = ReadString(root, propertyName);

            if (DateTime.TryParseExact(raw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return parsed;

            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                return parsed.Date;

            return DateTime.Today;
        }

        private static string L(string pt, string jp)
        {
            return string.Equals(Program.CurrentLocale, "ja-JP", StringComparison.OrdinalIgnoreCase)
                ? jp
                : pt;
        }

        private sealed record SectorDefinition(int Id, string Name);

        private sealed record PresencePerson(string CodigoFJ, string Display, string ShortName);

        private sealed record SectorRuntimeState(
            SectorDefinition Definition,
            HashSet<string> PlannedKeys,
            HashSet<string> ConfirmedKeys,
            HashSet<string> MissingKeys,
            HashSet<string> ExtraKeys,
            List<object> Locals);
    }
}
