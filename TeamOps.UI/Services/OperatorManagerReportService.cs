using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dapper;
using TeamOps.Data.Db;
using TeamOps.UI.Forms.Models;
using TeamOps.Services;

namespace TeamOps.UI.Services
{
    public sealed class OperatorManagerReportService
    {
        private readonly SqliteConnectionFactory _factory;

        public OperatorManagerReportService(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public OperatorManagerInitPayload GetInitialPayload(int defaultShiftId, int defaultSectorId)
        {
            using var conn = _factory.CreateOpenConnection();

            var shifts = conn.Query<OperatorManagerLookupItem>(
                @"
                    SELECT
                        Id,
                        COALESCE(NULLIF(NamePt, ''), NULLIF(NameJp, ''), 'Turno ' || Id) AS Name
                    FROM Shifts
                    ORDER BY Id;")
                .ToList();

            var sectors = conn.Query<OperatorManagerLookupItem>(
                @"
                    SELECT
                        Id,
                        COALESCE(NULLIF(NamePt, ''), NULLIF(NameJp, ''), 'Setor ' || Id) AS Name
                    FROM Sectors
                    ORDER BY Id;")
                .ToList();

            var groups = conn.Query<OperatorManagerLookupItem>(
                @"
                    SELECT
                        Id,
                        COALESCE(NULLIF(NamePt, ''), NULLIF(NameJp, ''), 'Grupo ' || Id) AS Name
                    FROM Groups
                    ORDER BY Id;")
                .ToList();

            return new OperatorManagerInitPayload(
                shifts.Any(item => item.Id == defaultShiftId) ? defaultShiftId : 0,
                sectors.Any(item => item.Id == defaultSectorId) ? defaultSectorId : 0,
                90,
                shifts,
                sectors,
                groups);
        }

        public OperatorManagerDirectoryPayload GetDirectory(OperatorManagerDirectoryFilter filter)
        {
            using var conn = _factory.CreateOpenConnection();

            var startDate = DateTime.Today.AddDays(-(Math.Max(1, filter.PeriodDays) - 1));
            var endDate = DateTime.Today;
            var search = (filter.Search ?? string.Empty).Trim();

            var rows = conn.Query<OperatorManagerDirectoryRow>(
                @"
                    WITH scheduled AS (
                        SELECT
                            OperatorCodigoFJ,
                            COUNT(DISTINCT date(ScheduleDate)) AS ScheduledDays
                        FROM HaidaiAssignments
                        WHERE date(ScheduleDate) BETWEEN date(@StartDate) AND date(@EndDate)
                          AND COALESCE(AvailabilityStatus, '') <> 'Folga'
                          AND (
                                COALESCE(AssignmentCode, '') <> ''
                             OR COALESCE(LocalId, 0) > 0
                             OR COALESCE(AvailabilityStatus, '') IN ('Yukyu', 'Falta', 'Atraso', 'Saiu cedo')
                          )
                        GROUP BY OperatorCodigoFJ
                    ),
                    presence AS (
                        SELECT
                            CodigoFJ AS OperatorCodigoFJ,
                            COUNT(DISTINCT date(Date)) AS PresentDays
                        FROM OperatorPresence
                        WHERE date(Date) BETWEEN date(@StartDate) AND date(@EndDate)
                        GROUP BY CodigoFJ
                    ),
                    followup_stats AS (
                        SELECT
                            OperatorCodigoFJ,
                            COUNT(1) AS FollowUpCount
                        FROM FollowUps
                        WHERE date(Date) BETWEEN date(@StartDate) AND date(@EndDate)
                        GROUP BY OperatorCodigoFJ
                    ),
                    todoke AS (
                        SELECT
                            a.OperatorCodigoFJ,
                            SUM(CASE WHEN a.TodokeMotivoId = 1 THEN 1 ELSE 0 END) AS YukyuDays,
                            SUM(CASE WHEN a.TodokeMotivoId = 2 THEN 1 ELSE 0 END) AS FaltaDays,
                            SUM(CASE WHEN a.TodokeMotivoId = 3 THEN 1 ELSE 0 END) AS LateDays,
                            SUM(CASE WHEN a.TodokeMotivoId = 5 THEN 1 ELSE 0 END) AS EarlyLeaveDays,
                            COUNT(DISTINCT CASE
                                WHEN a.TodokeMotivoId IN (2, 3, 5) THEN date(a.RequestDate)
                                ELSE NULL
                            END) AS PresenceImpactDays,
                            SUM(CASE
                                    WHEN NOT EXISTS (
                                        SELECT 1
                                        FROM YukyuTodoke y
                                        WHERE y.AcompYukyuId = a.Id
                                    ) THEN 1
                                    ELSE 0
                                END) AS PendingTodokeCount
                        FROM AcompYukyu a
                        WHERE date(a.RequestDate) BETWEEN date(@StartDate) AND date(@EndDate)
                        GROUP BY a.OperatorCodigoFJ
                    )
                    SELECT
                        o.CodigoFJ,
                        COALESCE(NULLIF(o.NameRomanji, ''), o.CodigoFJ) AS Name,
                        COALESCE(NULLIF(o.NameNihongo, ''), COALESCE(NULLIF(o.NameRomanji, ''), o.CodigoFJ)) AS NameJp,
                        o.ShiftId,
                        COALESCE(NULLIF(s.NamePt, ''), NULLIF(s.NameJp, ''), 'Turno ' || o.ShiftId) AS ShiftName,
                        o.SectorId,
                        COALESCE(NULLIF(sec.NamePt, ''), NULLIF(sec.NameJp, ''), 'Setor ' || o.SectorId) AS SectorName,
                        o.GroupId,
                        COALESCE(NULLIF(g.NamePt, ''), NULLIF(g.NameJp, ''), 'Grupo ' || o.GroupId) AS GroupName,
                        COALESCE(o.Trainer, 0) AS Trainer,
                        COALESCE(o.IsLeader, 0) AS IsLeader,
                        COALESCE(scheduled.ScheduledDays, 0) AS ScheduledDays,
                        COALESCE(presence.PresentDays, 0) AS PresentDays,
                        COALESCE(followup_stats.FollowUpCount, 0) AS FollowUpCount,
                        COALESCE(todoke.YukyuDays, 0) AS YukyuDays,
                        COALESCE(todoke.FaltaDays, 0) AS FaltaDays,
                        COALESCE(todoke.LateDays, 0) AS LateDays,
                        COALESCE(todoke.EarlyLeaveDays, 0) AS EarlyLeaveDays,
                        COALESCE(todoke.PresenceImpactDays, 0) AS PresenceImpactDays,
                        COALESCE(todoke.PendingTodokeCount, 0) AS PendingTodokeCount
                    FROM Operators o
                    LEFT JOIN Shifts s ON s.Id = o.ShiftId
                    LEFT JOIN Sectors sec ON sec.Id = o.SectorId
                    LEFT JOIN Groups g ON g.Id = o.GroupId
                    LEFT JOIN scheduled ON scheduled.OperatorCodigoFJ = o.CodigoFJ
                    LEFT JOIN presence ON presence.OperatorCodigoFJ = o.CodigoFJ
                    LEFT JOIN followup_stats ON followup_stats.OperatorCodigoFJ = o.CodigoFJ
                    LEFT JOIN todoke ON todoke.OperatorCodigoFJ = o.CodigoFJ
                    WHERE COALESCE(o.Status, 1) = 1
                      AND (@ShiftId <= 0 OR o.ShiftId = @ShiftId)
                      AND (@SectorId <= 0 OR o.SectorId = @SectorId)
                      AND (@GroupId <= 0 OR o.GroupId = @GroupId)
                      AND (
                            @Search = ''
                         OR o.CodigoFJ LIKE '%' || @Search || '%'
                         OR COALESCE(o.NameRomanji, '') LIKE '%' || @Search || '%'
                         OR COALESCE(o.NameNihongo, '') LIKE '%' || @Search || '%'
                      )
                    ORDER BY o.NameRomanji, o.CodigoFJ;",
                new
                {
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd"),
                    filter.ShiftId,
                    filter.SectorId,
                    filter.GroupId,
                    Search = search
                })
                .ToList();

            var items = rows
                .Select(row =>
                {
                    var effectivePresenceDays = Math.Max(0, row.ScheduledDays - row.PresenceImpactDays);
                    var presencePercent = row.ScheduledDays <= 0
                        ? 0
                        : Math.Round((effectivePresenceDays / (double)row.ScheduledDays) * 100d, 1);

                    var coveragePercent = row.ScheduledDays <= 0
                        ? 0
                        : Math.Round((Math.Min(row.ScheduledDays, row.PresentDays + row.YukyuDays) / (double)row.ScheduledDays) * 100d, 1);

                    return new OperatorManagerDirectoryItem(
                        row.CodigoFJ,
                        row.Name,
                        row.NameJp,
                        row.ShiftId,
                        row.ShiftName,
                        row.SectorId,
                        row.SectorName,
                        row.GroupId,
                        row.GroupName,
                        row.Trainer,
                        row.IsLeader,
                        row.ScheduledDays,
                        row.PresentDays,
                        row.YukyuDays,
                        row.FaltaDays,
                        row.LateDays,
                        row.EarlyLeaveDays,
                        row.FollowUpCount,
                        row.PendingTodokeCount,
                        presencePercent,
                        coveragePercent);
                })
                .ToList();

            return new OperatorManagerDirectoryPayload(
                startDate.ToString("yyyy-MM-dd"),
                endDate.ToString("yyyy-MM-dd"),
                items);
        }

        public OperatorManagerReportPayload GetReport(string codigoFJ, int periodDays)
        {
            if (string.IsNullOrWhiteSpace(codigoFJ))
            {
                throw new InvalidOperationException("Operador invalido para o relatorio gerencial.");
            }

            using var conn = _factory.CreateOpenConnection();

            var operatorInfo = conn.QuerySingleOrDefault<OperatorManagerOperatorInfo>(
                @"
                    SELECT
                        o.CodigoFJ,
                        COALESCE(NULLIF(o.NameRomanji, ''), o.CodigoFJ) AS Name,
                        COALESCE(NULLIF(o.NameNihongo, ''), COALESCE(NULLIF(o.NameRomanji, ''), o.CodigoFJ)) AS NameJp,
                        o.ShiftId,
                        COALESCE(NULLIF(s.NamePt, ''), NULLIF(s.NameJp, ''), 'Turno ' || o.ShiftId) AS ShiftName,
                        o.SectorId,
                        COALESCE(NULLIF(sec.NamePt, ''), NULLIF(sec.NameJp, ''), 'Setor ' || o.SectorId) AS SectorName,
                        o.GroupId,
                        COALESCE(NULLIF(g.NamePt, ''), NULLIF(g.NameJp, ''), 'Grupo ' || o.GroupId) AS GroupName,
                        o.StartDate,
                        COALESCE(o.Trainer, 0) AS Trainer,
                        COALESCE(o.IsLeader, 0) AS IsLeader
                    FROM Operators o
                    LEFT JOIN Shifts s ON s.Id = o.ShiftId
                    LEFT JOIN Sectors sec ON sec.Id = o.SectorId
                    LEFT JOIN Groups g ON g.Id = o.GroupId
                    WHERE o.CodigoFJ = @CodigoFJ;",
                new { CodigoFJ = codigoFJ.Trim() });

            if (operatorInfo == null)
            {
                throw new InvalidOperationException("Operador nao encontrado para o relatorio gerencial.");
            }

            var startDate = DateTime.Today.AddDays(-(Math.Max(1, periodDays) - 1));
            var endDate = DateTime.Today;

            var scheduledRows = conn.Query<OperatorManagerScheduleRow>(
                @"
                    SELECT
                        date(ha.ScheduleDate) AS Day,
                        ha.ShiftId,
                        ha.LocalId,
                        COALESCE(NULLIF(l.NamePt, ''), NULLIF(l.NameJp, ''), '') AS LocalName,
                        COALESCE(ha.AssignmentCode, '') AS AssignmentCode,
                        COALESCE(ha.AvailabilityStatus, '') AS AvailabilityStatus,
                        COALESCE(ha.IsLineupActive, 1) AS IsLineupActive,
                        COALESCE(ha.IsTrainee, 0) AS IsTrainee
                    FROM HaidaiAssignments ha
                    LEFT JOIN Locals l ON l.Id = ha.LocalId
                    WHERE ha.OperatorCodigoFJ = @CodigoFJ
                      AND date(ha.ScheduleDate) BETWEEN date(@StartDate) AND date(@EndDate)
                    ORDER BY date(ha.ScheduleDate) DESC, ha.Id DESC;",
                new
                {
                    CodigoFJ = codigoFJ.Trim(),
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd")
                })
                .ToList();

            var latestScheduleByDay = scheduledRows
                .GroupBy(item => item.Day)
                .ToDictionary(group => group.Key, group => group.First());

            var presentRows = conn.Query<OperatorManagerPresenceRow>(
                @"
                    SELECT
                        date(Date) AS Day,
                        MAX(Date) AS LastPresence
                    FROM OperatorPresence
                    WHERE CodigoFJ = @CodigoFJ
                      AND date(Date) BETWEEN date(@StartDate) AND date(@EndDate)
                    GROUP BY date(Date);",
                new
                {
                    CodigoFJ = codigoFJ.Trim(),
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd")
                })
                .ToDictionary(item => item.Day, item => item);

            var todokeRows = conn.Query<OperatorManagerTodokeRow>(
                @"
                    SELECT
                        date(a.RequestDate) AS Day,
                        a.Id,
                        a.TodokeMotivoId AS MotiveId,
                        COALESCE(m.NomePt, '') AS MotiveName,
                        COALESCE(a.Notes, '') AS Notes,
                        CASE
                            WHEN EXISTS (
                                SELECT 1
                                FROM YukyuTodoke y
                                WHERE y.AcompYukyuId = a.Id
                            ) THEN 1
                            ELSE 0
                        END AS Validated
                    FROM AcompYukyu a
                    LEFT JOIN TodokeMotivo m ON m.Id = a.TodokeMotivoId
                    WHERE a.OperatorCodigoFJ = @CodigoFJ
                      AND date(a.RequestDate) BETWEEN date(@StartDate) AND date(@EndDate)
                    ORDER BY date(a.RequestDate) DESC, a.Id DESC;",
                new
                {
                    CodigoFJ = codigoFJ.Trim(),
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd")
                })
                .ToList();

            var latestTodokeByDay = todokeRows
                .GroupBy(item => item.Day)
                .ToDictionary(group => group.Key, group => group.First());

            var movementRows = conn.Query<OperatorManagerMovementRow>(
                @"
                    SELECT
                        date(ScheduleDate) AS Day,
                        COALESCE(MovementType, '') AS MovementType,
                        COALESCE(EventTime, '') AS EventTime,
                        COALESCE(AssignmentCode, '') AS AssignmentCode,
                        COALESCE(Reason, '') AS Reason
                    FROM HaidaiMovements
                    WHERE OperatorCodigoFJ = @CodigoFJ
                      AND date(ScheduleDate) BETWEEN date(@StartDate) AND date(@EndDate)
                    ORDER BY date(ScheduleDate) DESC, COALESCE(EventTime, '') DESC, Id DESC;",
                new
                {
                    CodigoFJ = codigoFJ.Trim(),
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd")
                })
                .GroupBy(item => item.Day)
                .ToDictionary(group => group.Key, group => group.First());

            var followUpCount = conn.ExecuteScalar<int>(
                @"
                    SELECT COUNT(1)
                    FROM FollowUps
                    WHERE OperatorCodigoFJ = @CodigoFJ
                      AND date(Date) BETWEEN date(@StartDate) AND date(@EndDate);",
                new
                {
                    CodigoFJ = codigoFJ.Trim(),
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd")
                });

            var recentFollowUps = conn.Query<OperatorManagerFollowUpRow>(
                @"
                    SELECT
                        f.Id,
                        f.Date,
                        COALESCE(NULLIF(s.NamePt, ''), NULLIF(s.NameJp, ''), '') AS ShiftName,
                        COALESCE(NULLIF(r.NamePt, ''), '') AS ReasonName,
                        COALESCE(NULLIF(t.NamePt, ''), '') AS TypeName,
                        COALESCE(NULLIF(l.NamePt, ''), NULLIF(l.NameJp, ''), '') AS LocalName,
                        COALESCE(NULLIF(e.NamePt, ''), NULLIF(e.NameJp, ''), '') AS EquipmentName,
                        COALESCE(f.Description, '') AS Description,
                        COALESCE(f.Guidance, '') AS Guidance
                    FROM FollowUps f
                    LEFT JOIN Shifts s ON s.Id = f.ShiftId
                    LEFT JOIN FollowUpReasons r ON r.Id = f.ReasonId
                    LEFT JOIN FollowUpTypes t ON t.Id = f.TypeId
                    LEFT JOIN Locals l ON l.Id = f.LocalId
                    LEFT JOIN Equipments e ON e.Id = f.EquipmentId
                    WHERE f.OperatorCodigoFJ = @CodigoFJ
                      AND date(f.Date) BETWEEN date(@StartDate) AND date(@EndDate)
                    ORDER BY datetime(f.Date) DESC
                    LIMIT 12;",
                new
                {
                    CodigoFJ = codigoFJ.Trim(),
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd")
                })
                .ToList();

            var metrics = BuildPresenceMetrics(latestScheduleByDay.Values, presentRows, todokeRows, followUpCount);
            var production = BuildProductionSummary(operatorInfo, scheduledRows, presentRows.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase), startDate, endDate);

            var dailyHistory = BuildDailyHistory(latestScheduleByDay, presentRows, latestTodokeByDay, movementRows);

            return new OperatorManagerReportPayload(
                operatorInfo.CodigoFJ,
                operatorInfo.Name,
                operatorInfo.NameJp,
                operatorInfo.ShiftName,
                operatorInfo.SectorName,
                operatorInfo.GroupName,
                operatorInfo.StartDate.ToString("yyyy-MM-dd"),
                operatorInfo.Trainer,
                operatorInfo.IsLeader,
                metrics,
                production,
                recentFollowUps
                    .Select(item => new OperatorManagerFollowUpItem(
                        item.Id,
                        item.Date.ToString("yyyy-MM-dd HH:mm"),
                        item.ShiftName,
                        item.ReasonName,
                        item.TypeName,
                        item.LocalName,
                        item.EquipmentName,
                        item.Description,
                        item.Guidance))
                    .ToList(),
                dailyHistory);
        }

        private OperatorManagerPresenceMetrics BuildPresenceMetrics(
            IEnumerable<OperatorManagerScheduleRow> latestSchedules,
            IReadOnlyDictionary<string, OperatorManagerPresenceRow> presences,
            IEnumerable<OperatorManagerTodokeRow> todokes,
            int followUpCount)
        {
            var scheduleList = latestSchedules.ToList();
            var scheduledDays = scheduleList.Count(item =>
                !string.Equals(item.AvailabilityStatus, "Folga", StringComparison.OrdinalIgnoreCase) &&
                (!string.IsNullOrWhiteSpace(item.AssignmentCode) || item.LocalId > 0 || !string.IsNullOrWhiteSpace(item.AvailabilityStatus)));

            var presentDays = presences.Count;
            var todokeList = todokes.ToList();
            var yukyuDays = todokeList.Count(item => item.MotiveId == 1);
            var faltaDays = todokeList.Count(item => item.MotiveId == 2);
            var lateDays = todokeList.Count(item => item.MotiveId == 3);
            var earlyLeaveDays = todokeList.Count(item => item.MotiveId == 5);
            var presenceImpactDays = todokeList
                .Where(item => item.MotiveId == 2 || item.MotiveId == 3 || item.MotiveId == 5)
                .Select(item => item.Day)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
            var pendingTodokeCount = todokeList.Count(item => !item.Validated);
            var effectivePresenceDays = Math.Max(0, scheduledDays - presenceImpactDays);

            var presencePercent = scheduledDays <= 0
                ? 0
                : Math.Round((effectivePresenceDays / (double)scheduledDays) * 100d, 1);

            var coveragePercent = scheduledDays <= 0
                ? 0
                : Math.Round((Math.Min(scheduledDays, presentDays + yukyuDays) / (double)scheduledDays) * 100d, 1);

            return new OperatorManagerPresenceMetrics(
                scheduledDays,
                presentDays,
                yukyuDays,
                faltaDays,
                lateDays,
                earlyLeaveDays,
                pendingTodokeCount,
                followUpCount,
                presencePercent,
                coveragePercent);
        }

        private OperatorManagerProductionSummary BuildProductionSummary(
            OperatorManagerOperatorInfo operatorInfo,
            IReadOnlyList<OperatorManagerScheduleRow> scheduleRows,
            ISet<string> presentDays,
            DateTime startDate,
            DateTime endDate)
        {
            using var conn = _factory.CreateOpenConnection();
            var analytics = new ProductionAnalyticsService(_factory);

            var scheduleDays = scheduleRows
                .Where(item => item.LocalId > 0 && presentDays.Contains(item.Day))
                .GroupBy(item => new { item.Day, item.LocalId, item.ShiftId, item.LocalName, item.AssignmentCode })
                .Select(group => group.First())
                .ToList();

            if (scheduleDays.Count == 0)
            {
                return new OperatorManagerProductionSummary(0, 0, Array.Empty<string>(), Array.Empty<OperatorManagerProductionDay>());
            }

            var machineRows = conn.Query<OperatorManagerMachineRow>(
                @"
                    SELECT
                        Id,
                        LocalId
                    FROM Machines
                    WHERE COALESCE(IsActive, 1) = 1
                      AND LocalId IN @LocalIds;",
                new
                {
                    LocalIds = scheduleDays
                        .Select(item => item.LocalId)
                        .Distinct()
                        .ToArray()
                })
                .ToList();

            if (machineRows.Count == 0)
            {
                return new OperatorManagerProductionSummary(0, 0, Array.Empty<string>(), Array.Empty<OperatorManagerProductionDay>());
            }

            var machineIds = machineRows.Select(item => item.Id).ToArray();
            var eventRows = conn.Query<OperatorManagerEventRow>(
                @"
                    SELECT
                        MachineId,
                        StatusCode,
                        EventDateTime
                    FROM MachineEvents
                    WHERE MachineId IN @MachineIds
                      AND datetime(EventDateTime) >= datetime(@RangeStart)
                      AND datetime(EventDateTime) <= datetime(@RangeEnd)
                    ORDER BY MachineId, datetime(EventDateTime), Id;",
                new
                {
                    MachineIds = machineIds,
                    RangeStart = startDate.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                    RangeEnd = endDate.AddDays(2).ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToList();

            var machineEventsById = eventRows
                .GroupBy(item => item.MachineId)
                .ToDictionary(group => group.Key, group => group.Select(item => new ProductionEventPoint(item.MachineId, item.StatusCode, item.EventDateTime)).ToList());

            var machineIdsByLocal = machineRows
                .GroupBy(item => item.LocalId)
                .ToDictionary(group => group.Key, group => group.Select(item => item.Id).ToList());

            var productionDays = new List<OperatorManagerProductionDay>();
            var totalMinutes = 0d;
            var totalScheduledMinutes = 0d;
            var localNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var dayGroup in scheduleDays.GroupBy(item => item.Day).OrderByDescending(group => group.Key))
            {
                var dayDate = DateTime.ParseExact(dayGroup.Key, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var shiftPeriod = analytics.GetShiftPeriod(dayDate, operatorInfo.ShiftName);
                var periodMinutes = Math.Max(0, (shiftPeriod.End - shiftPeriod.Start).TotalMinutes);
                var dayRunningMinutes = 0d;
                var dayLocalNames = new List<string>();

                foreach (var item in dayGroup)
                {
                    if (!machineIdsByLocal.TryGetValue(item.LocalId, out var localMachineIds))
                    {
                        continue;
                    }

                    var localRunning = 0d;
                    foreach (var machineId in localMachineIds)
                    {
                        if (!machineEventsById.TryGetValue(machineId, out var points))
                        {
                            continue;
                        }

                        localRunning += CalculateRunningMinutes(points, shiftPeriod);
                    }

                    dayRunningMinutes += localRunning;
                    if (!string.IsNullOrWhiteSpace(item.LocalName))
                    {
                        dayLocalNames.Add(item.LocalName);
                        localNames.Add(item.LocalName);
                    }
                }

                totalMinutes += dayRunningMinutes;
                totalScheduledMinutes += periodMinutes;

                productionDays.Add(new OperatorManagerProductionDay(
                    dayDate.ToString("yyyy-MM-dd"),
                    dayRunningMinutes,
                    periodMinutes <= 0 ? 0 : Math.Round((dayRunningMinutes / periodMinutes) * 100d, 1),
                    dayLocalNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList()));
            }

            return new OperatorManagerProductionSummary(
                Math.Round(totalMinutes, 1),
                totalScheduledMinutes <= 0 ? 0 : Math.Round((totalMinutes / totalScheduledMinutes) * 100d, 1),
                localNames.ToList(),
                productionDays.Take(12).ToList());
        }

        private List<OperatorManagerDailyHistoryItem> BuildDailyHistory(
            IReadOnlyDictionary<string, OperatorManagerScheduleRow> latestScheduleByDay,
            IReadOnlyDictionary<string, OperatorManagerPresenceRow> presences,
            IReadOnlyDictionary<string, OperatorManagerTodokeRow> todokes,
            IReadOnlyDictionary<string, OperatorManagerMovementRow> movements)
        {
            var allDays = latestScheduleByDay.Keys
                .Concat(presences.Keys)
                .Concat(todokes.Keys)
                .Concat(movements.Keys)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(day => day, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var list = new List<OperatorManagerDailyHistoryItem>(allDays.Count);
            foreach (var day in allDays)
            {
                latestScheduleByDay.TryGetValue(day, out var schedule);
                presences.TryGetValue(day, out var presence);
                todokes.TryGetValue(day, out var todoke);
                movements.TryGetValue(day, out var movement);

                var status = ResolveDailyStatus(schedule, presence, todoke, movement);
                var area = !string.IsNullOrWhiteSpace(schedule?.AssignmentCode)
                    ? schedule.AssignmentCode
                    : movement?.AssignmentCode ?? string.Empty;

                var notes = new List<string>();
                if (!string.IsNullOrWhiteSpace(todoke?.Notes))
                {
                    notes.Add(todoke.Notes);
                }

                if (!string.IsNullOrWhiteSpace(movement?.Reason))
                {
                    notes.Add(movement.Reason);
                }

                if (presence?.LastPresence != null)
                {
                    notes.Add($"Presenca {presence.LastPresence.Value.ToString("HH:mm", CultureInfo.InvariantCulture)}");
                }

                list.Add(new OperatorManagerDailyHistoryItem(
                    day,
                    status,
                    area,
                    string.Join(" | ", notes.Where(item => !string.IsNullOrWhiteSpace(item))),
                    todoke != null && !todoke.Validated));
            }

            return list.Take(20).ToList();
        }

        private static string ResolveDailyStatus(
            OperatorManagerScheduleRow? schedule,
            OperatorManagerPresenceRow? presence,
            OperatorManagerTodokeRow? todoke,
            OperatorManagerMovementRow? movement)
        {
            if (todoke != null)
            {
                return todoke.MotiveName;
            }

            if (movement != null)
            {
                return movement.MovementType switch
                {
                    "late" => "Atraso",
                    "early_leave" => "Sair cedo",
                    _ => movement.MovementType
                };
            }

            if (presence != null)
            {
                return "Presente";
            }

            if (!string.IsNullOrWhiteSpace(schedule?.AvailabilityStatus))
            {
                return schedule.AvailabilityStatus;
            }

            return !string.IsNullOrWhiteSpace(schedule?.AssignmentCode) || schedule?.LocalId > 0
                ? "Escalado"
                : "Sem registro";
        }

        private static double CalculateRunningMinutes(IReadOnlyList<ProductionEventPoint> events, ProductionShiftPeriod period)
        {
            var running = 0d;
            var seed = events
                .Where(item => item.EventDateTime <= period.Start)
                .OrderByDescending(item => item.EventDateTime)
                .FirstOrDefault();

            var cursor = period.Start;
            var currentStatus = seed?.StatusCode ?? 2;

            foreach (var machineEvent in events.Where(item => item.EventDateTime >= period.Start && item.EventDateTime <= period.End))
            {
                if (machineEvent.EventDateTime > cursor && currentStatus == 0)
                {
                    running += (machineEvent.EventDateTime - cursor).TotalMinutes;
                }

                cursor = machineEvent.EventDateTime < period.Start ? period.Start : machineEvent.EventDateTime;
                currentStatus = machineEvent.StatusCode;
            }

            if (cursor < period.End && currentStatus == 0)
            {
                running += (period.End - cursor).TotalMinutes;
            }

            return running;
        }

        private sealed class OperatorManagerDirectoryRow
        {
            public string CodigoFJ { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string NameJp { get; set; } = string.Empty;
            public int ShiftId { get; set; }
            public string ShiftName { get; set; } = string.Empty;
            public int SectorId { get; set; }
            public string SectorName { get; set; } = string.Empty;
            public int GroupId { get; set; }
            public string GroupName { get; set; } = string.Empty;
            public bool Trainer { get; set; }
            public bool IsLeader { get; set; }
            public int ScheduledDays { get; set; }
            public int PresentDays { get; set; }
            public int FollowUpCount { get; set; }
            public int YukyuDays { get; set; }
            public int FaltaDays { get; set; }
            public int LateDays { get; set; }
            public int EarlyLeaveDays { get; set; }
            public int PresenceImpactDays { get; set; }
            public int PendingTodokeCount { get; set; }
        }

        private sealed class OperatorManagerOperatorInfo
        {
            public string CodigoFJ { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string NameJp { get; set; } = string.Empty;
            public int ShiftId { get; set; }
            public string ShiftName { get; set; } = string.Empty;
            public int SectorId { get; set; }
            public string SectorName { get; set; } = string.Empty;
            public int GroupId { get; set; }
            public string GroupName { get; set; } = string.Empty;
            public DateTime StartDate { get; set; }
            public bool Trainer { get; set; }
            public bool IsLeader { get; set; }
        }

        private sealed class OperatorManagerScheduleRow
        {
            public string Day { get; set; } = string.Empty;
            public int ShiftId { get; set; }
            public int LocalId { get; set; }
            public string LocalName { get; set; } = string.Empty;
            public string AssignmentCode { get; set; } = string.Empty;
            public string AvailabilityStatus { get; set; } = string.Empty;
            public bool IsLineupActive { get; set; }
            public bool IsTrainee { get; set; }
        }

        private sealed class OperatorManagerPresenceRow
        {
            public string Day { get; set; } = string.Empty;
            public DateTime? LastPresence { get; set; }
        }

        private sealed class OperatorManagerTodokeRow
        {
            public string Day { get; set; } = string.Empty;
            public long Id { get; set; }
            public int MotiveId { get; set; }
            public string MotiveName { get; set; } = string.Empty;
            public string Notes { get; set; } = string.Empty;
            public bool Validated { get; set; }
        }

        private sealed class OperatorManagerMovementRow
        {
            public string Day { get; set; } = string.Empty;
            public string MovementType { get; set; } = string.Empty;
            public string EventTime { get; set; } = string.Empty;
            public string AssignmentCode { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
        }

        private sealed class OperatorManagerFollowUpRow
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public string ShiftName { get; set; } = string.Empty;
            public string ReasonName { get; set; } = string.Empty;
            public string TypeName { get; set; } = string.Empty;
            public string LocalName { get; set; } = string.Empty;
            public string EquipmentName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Guidance { get; set; } = string.Empty;
        }

        private sealed class OperatorManagerMachineRow
        {
            public int Id { get; set; }
            public int LocalId { get; set; }
        }

        private sealed class OperatorManagerEventRow
        {
            public int MachineId { get; set; }
            public int StatusCode { get; set; }
            public DateTime EventDateTime { get; set; }
        }

        private sealed record ProductionEventPoint(int MachineId, int StatusCode, DateTime EventDateTime);
    }

    public sealed record OperatorManagerInitPayload(
        int DefaultShiftId,
        int DefaultSectorId,
        int DefaultPeriodDays,
        IReadOnlyList<OperatorManagerLookupItem> Shifts,
        IReadOnlyList<OperatorManagerLookupItem> Sectors,
        IReadOnlyList<OperatorManagerLookupItem> Groups);

    public sealed class OperatorManagerLookupItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed record OperatorManagerDirectoryFilter(
        int ShiftId,
        int SectorId,
        int GroupId,
        int PeriodDays,
        string Search);

    public sealed record OperatorManagerDirectoryPayload(
        string StartDateIso,
        string EndDateIso,
        IReadOnlyList<OperatorManagerDirectoryItem> Items);

    public sealed record OperatorManagerDirectoryItem(
        string CodigoFJ,
        string Name,
        string NameJp,
        int ShiftId,
        string ShiftName,
        int SectorId,
        string SectorName,
        int GroupId,
        string GroupName,
        bool Trainer,
        bool IsLeader,
        int ScheduledDays,
        int PresentDays,
        int YukyuDays,
        int FaltaDays,
        int LateDays,
        int EarlyLeaveDays,
        int FollowUpCount,
        int PendingTodokeCount,
        double PresencePercent,
        double CoveragePercent);

    public sealed record OperatorManagerReportPayload(
        string CodigoFJ,
        string Name,
        string NameJp,
        string ShiftName,
        string SectorName,
        string GroupName,
        string StartDateIso,
        bool Trainer,
        bool IsLeader,
        OperatorManagerPresenceMetrics Presence,
        OperatorManagerProductionSummary Production,
        IReadOnlyList<OperatorManagerFollowUpItem> FollowUps,
        IReadOnlyList<OperatorManagerDailyHistoryItem> DailyHistory);

    public sealed record OperatorManagerPresenceMetrics(
        int ScheduledDays,
        int PresentDays,
        int YukyuDays,
        int FaltaDays,
        int LateDays,
        int EarlyLeaveDays,
        int PendingTodokeCount,
        int FollowUpCount,
        double PresencePercent,
        double CoveragePercent);

    public sealed record OperatorManagerProductionSummary(
        double EstimatedRunningMinutes,
        double EstimatedKadouritsuPercent,
        IReadOnlyList<string> LocalNames,
        IReadOnlyList<OperatorManagerProductionDay> Days);

    public sealed record OperatorManagerProductionDay(
        string DateIso,
        double EstimatedRunningMinutes,
        double EstimatedKadouritsuPercent,
        IReadOnlyList<string> LocalNames);

    public sealed record OperatorManagerFollowUpItem(
        int Id,
        string DateLabel,
        string ShiftName,
        string ReasonName,
        string TypeName,
        string LocalName,
        string EquipmentName,
        string Description,
        string Guidance);

    public sealed record OperatorManagerDailyHistoryItem(
        string DateIso,
        string Status,
        string Area,
        string Notes,
        bool HasPendingTodoke);
}
