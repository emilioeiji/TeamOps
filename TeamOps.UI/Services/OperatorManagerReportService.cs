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
            MasterCardModuleService.EnsureSchema(conn);

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
            MasterCardModuleService.EnsureSchema(conn);

            var startDate = DateTime.Today.AddDays(-(Math.Max(1, filter.PeriodDays) - 1));
            var endDate = DateTime.Today;
            var search = (filter.Search ?? string.Empty).Trim();

            var rows = conn.Query<OperatorManagerDirectoryRow>(
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
                        COALESCE(o.Trainer, 0) AS Trainer,
                        COALESCE(o.IsLeader, 0) AS IsLeader
                    FROM Operators o
                    LEFT JOIN Shifts s ON s.Id = o.ShiftId
                    LEFT JOIN Sectors sec ON sec.Id = o.SectorId
                    LEFT JOIN Groups g ON g.Id = o.GroupId
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

            if (rows.Count == 0)
            {
                return new OperatorManagerDirectoryPayload(
                    startDate.ToString("yyyy-MM-dd"),
                    endDate.ToString("yyyy-MM-dd"),
                    Array.Empty<OperatorManagerDirectoryItem>());
            }

            var operatorCodes = rows
                .Select(item => item.CodigoFJ)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var scheduleRows = conn.Query<OperatorManagerScheduleRow>(
                @"
                    SELECT
                        ha.OperatorCodigoFJ,
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
                    WHERE ha.OperatorCodigoFJ IN @OperatorCodes
                      AND date(ha.ScheduleDate) BETWEEN date(@StartDate) AND date(@EndDate)
                    ORDER BY date(ha.ScheduleDate) DESC, ha.Id DESC;",
                new
                {
                    OperatorCodes = operatorCodes,
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd")
                })
                .ToList();

            var latestSchedulesByOperator = scheduleRows
                .GroupBy(item => item.OperatorCodigoFJ, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyDictionary<string, OperatorManagerScheduleRow>)group
                        .GroupBy(item => item.Day, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(dayGroup => dayGroup.Key, dayGroup => dayGroup.First(), StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase);

            var todokeRows = conn.Query<OperatorManagerTodokeRow>(
                @"
                    SELECT
                        a.OperatorCodigoFJ,
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
                    WHERE a.OperatorCodigoFJ IN @OperatorCodes
                      AND date(a.RequestDate) BETWEEN date(@StartDate) AND date(@EndDate)
                    ORDER BY date(a.RequestDate) DESC, a.Id DESC;",
                new
                {
                    OperatorCodes = operatorCodes,
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd")
                })
                .ToList();

            var latestTodokesByOperator = todokeRows
                .GroupBy(item => item.OperatorCodigoFJ, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyDictionary<string, OperatorManagerTodokeRow>)group
                        .GroupBy(item => item.Day, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(dayGroup => dayGroup.Key, dayGroup => dayGroup.First(), StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase);

            var movementRows = conn.Query<OperatorManagerMovementRow>(
                @"
                    SELECT
                        OperatorCodigoFJ,
                        date(ScheduleDate) AS Day,
                        COALESCE(MovementType, '') AS MovementType,
                        COALESCE(EventTime, '') AS EventTime,
                        COALESCE(AssignmentCode, '') AS AssignmentCode,
                        COALESCE(Reason, '') AS Reason
                    FROM HaidaiMovements
                    WHERE OperatorCodigoFJ IN @OperatorCodes
                      AND date(ScheduleDate) BETWEEN date(@StartDate) AND date(@EndDate)
                    ORDER BY date(ScheduleDate) DESC, COALESCE(EventTime, '') DESC, Id DESC;",
                new
                {
                    OperatorCodes = operatorCodes,
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd")
                })
                .ToList();

            var latestMovementsByOperator = movementRows
                .GroupBy(item => item.OperatorCodigoFJ, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyDictionary<string, OperatorManagerMovementRow>)group
                        .GroupBy(item => item.Day, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(dayGroup => dayGroup.Key, dayGroup => dayGroup.First(), StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase);

            var followUpCountByOperator = conn.Query<OperatorManagerFollowUpCountRow>(
                @"
                    SELECT
                        OperatorCodigoFJ,
                        COUNT(1) AS Count
                    FROM FollowUps
                    WHERE OperatorCodigoFJ IN @OperatorCodes
                      AND date(Date) BETWEEN date(@StartDate) AND date(@EndDate)
                    GROUP BY OperatorCodigoFJ;",
                new
                {
                    OperatorCodes = operatorCodes,
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd")
                })
                .ToDictionary(item => item.OperatorCodigoFJ, item => item.Count, StringComparer.OrdinalIgnoreCase);

            var emptyPresenceMap = new Dictionary<string, OperatorManagerPresenceRow>(StringComparer.OrdinalIgnoreCase);
            var emptyScheduleMap = new Dictionary<string, OperatorManagerScheduleRow>(StringComparer.OrdinalIgnoreCase);
            var emptyTodokeMap = new Dictionary<string, OperatorManagerTodokeRow>(StringComparer.OrdinalIgnoreCase);
            var emptyMovementMap = new Dictionary<string, OperatorManagerMovementRow>(StringComparer.OrdinalIgnoreCase);
            var items = rows
                .Select(row =>
                {
                    latestSchedulesByOperator.TryGetValue(row.CodigoFJ, out var schedulesByDay);
                    latestTodokesByOperator.TryGetValue(row.CodigoFJ, out var todokesByDay);
                    latestMovementsByOperator.TryGetValue(row.CodigoFJ, out var movementsByDay);
                    followUpCountByOperator.TryGetValue(row.CodigoFJ, out var followUpCount);

                    schedulesByDay ??= emptyScheduleMap;
                    todokesByDay ??= emptyTodokeMap;
                    movementsByDay ??= emptyMovementMap;

                    var attendanceByDay = BuildAttendanceDaySummaries(
                        schedulesByDay,
                        emptyPresenceMap,
                        todokesByDay,
                        movementsByDay);

                    var metrics = BuildPresenceMetrics(
                        attendanceByDay.Values,
                        emptyPresenceMap,
                        todokesByDay.Values,
                        followUpCount);

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
                        metrics.ScheduledDays,
                        metrics.PresentDays,
                        metrics.YukyuDays,
                        metrics.FaltaDays,
                        metrics.LateDays,
                        metrics.EarlyLeaveDays,
                        metrics.FollowUpCount,
                        metrics.PendingTodokeCount,
                        metrics.PresencePercent,
                        metrics.CoveragePercent);
                })
                .ToList();

            return new OperatorManagerDirectoryPayload(
                startDate.ToString("yyyy-MM-dd"),
                endDate.ToString("yyyy-MM-dd"),
                items);
        }

        public OperatorPresenceReportPayload GetPresenceReport(OperatorPresenceReportFilter filter)
        {
            using var conn = _factory.CreateOpenConnection();

            var startDate = ParseDateOrDefault(filter.StartDateIso, DateTime.Today.AddDays(-29));
            var endDate = ParseDateOrDefault(filter.EndDateIso, DateTime.Today);
            if (endDate < startDate)
            {
                (startDate, endDate) = (endDate, startDate);
            }

            var search = (filter.Search ?? string.Empty).Trim();
            var statusFilter = (filter.Status ?? string.Empty).Trim().ToLowerInvariant();

            var operators = conn.Query<OperatorManagerDirectoryRow>(
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
                        COALESCE(o.Trainer, 0) AS Trainer,
                        COALESCE(o.IsLeader, 0) AS IsLeader
                    FROM Operators o
                    LEFT JOIN Shifts s ON s.Id = o.ShiftId
                    LEFT JOIN Sectors sec ON sec.Id = o.SectorId
                    LEFT JOIN Groups g ON g.Id = o.GroupId
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
                    filter.ShiftId,
                    filter.SectorId,
                    filter.GroupId,
                    Search = search
                })
                .ToList();

            if (operators.Count == 0)
            {
                return new OperatorPresenceReportPayload(
                    startDate.ToString("yyyy-MM-dd"),
                    endDate.ToString("yyyy-MM-dd"),
                    new OperatorPresenceReportSummary(0, 0, 0, 0, 0, 0, 0, 0),
                    Array.Empty<OperatorPresenceReportRow>());
            }

            var operatorCodes = operators
                .Select(item => item.CodigoFJ)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var scheduleRows = conn.Query<OperatorManagerScheduleRow>(
                @"
                    SELECT
                        ha.OperatorCodigoFJ,
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
                    WHERE ha.OperatorCodigoFJ IN @OperatorCodes
                      AND date(ha.ScheduleDate) BETWEEN date(@StartDate) AND date(@EndDate)
                    ORDER BY date(ha.ScheduleDate) DESC, ha.Id DESC;",
                new
                {
                    OperatorCodes = operatorCodes,
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd")
                })
                .ToList();

            var presenceRows = conn.Query<OperatorPresenceReportPresenceRow>(
                @"
                    SELECT
                        CodigoFJ AS OperatorCodigoFJ,
                        date(Date) AS Day,
                        MAX(Date) AS LastPresence
                    FROM OperatorPresence
                    WHERE CodigoFJ IN @OperatorCodes
                      AND date(Date) BETWEEN date(@StartDate) AND date(@EndDate)
                    GROUP BY CodigoFJ, date(Date);",
                new
                {
                    OperatorCodes = operatorCodes,
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd")
                })
                .ToList();

            var todokeRows = conn.Query<OperatorManagerTodokeRow>(
                @"
                    SELECT
                        a.OperatorCodigoFJ,
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
                    WHERE a.OperatorCodigoFJ IN @OperatorCodes
                      AND date(a.RequestDate) BETWEEN date(@StartDate) AND date(@EndDate)
                    ORDER BY date(a.RequestDate) DESC, a.Id DESC;",
                new
                {
                    OperatorCodes = operatorCodes,
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd")
                })
                .ToList();

            var movementRows = conn.Query<OperatorManagerMovementRow>(
                @"
                    SELECT
                        OperatorCodigoFJ,
                        date(ScheduleDate) AS Day,
                        COALESCE(MovementType, '') AS MovementType,
                        COALESCE(EventTime, '') AS EventTime,
                        COALESCE(AssignmentCode, '') AS AssignmentCode,
                        COALESCE(Reason, '') AS Reason
                    FROM HaidaiMovements
                    WHERE OperatorCodigoFJ IN @OperatorCodes
                      AND date(ScheduleDate) BETWEEN date(@StartDate) AND date(@EndDate)
                    ORDER BY date(ScheduleDate) DESC, COALESCE(EventTime, '') DESC, Id DESC;",
                new
                {
                    OperatorCodes = operatorCodes,
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd")
                })
                .ToList();

            var schedulesByOperator = scheduleRows
                .GroupBy(item => item.OperatorCodigoFJ, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => (IReadOnlyDictionary<string, OperatorManagerScheduleRow>)group
                    .GroupBy(item => item.Day, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(dayGroup => dayGroup.Key, dayGroup => dayGroup.First(), StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

            var presencesByOperator = presenceRows
                .GroupBy(item => item.OperatorCodigoFJ, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => (IReadOnlyDictionary<string, OperatorManagerPresenceRow>)group
                    .ToDictionary(item => item.Day, item => new OperatorManagerPresenceRow { Day = item.Day, LastPresence = item.LastPresence }, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

            var todokesByOperator = todokeRows
                .GroupBy(item => item.OperatorCodigoFJ, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => (IReadOnlyDictionary<string, OperatorManagerTodokeRow>)group
                    .GroupBy(item => item.Day, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(dayGroup => dayGroup.Key, dayGroup => dayGroup.First(), StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

            var movementsByOperator = movementRows
                .GroupBy(item => item.OperatorCodigoFJ, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => (IReadOnlyDictionary<string, OperatorManagerMovementRow>)group
                    .GroupBy(item => item.Day, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(dayGroup => dayGroup.Key, dayGroup => dayGroup.First(), StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

            var emptyScheduleMap = new Dictionary<string, OperatorManagerScheduleRow>(StringComparer.OrdinalIgnoreCase);
            var emptyPresenceMap = new Dictionary<string, OperatorManagerPresenceRow>(StringComparer.OrdinalIgnoreCase);
            var emptyTodokeMap = new Dictionary<string, OperatorManagerTodokeRow>(StringComparer.OrdinalIgnoreCase);
            var emptyMovementMap = new Dictionary<string, OperatorManagerMovementRow>(StringComparer.OrdinalIgnoreCase);

            var rows = new List<OperatorPresenceReportRow>();
            foreach (var op in operators)
            {
                schedulesByOperator.TryGetValue(op.CodigoFJ, out var scheduleMap);
                presencesByOperator.TryGetValue(op.CodigoFJ, out var presenceMap);
                todokesByOperator.TryGetValue(op.CodigoFJ, out var todokeMap);
                movementsByOperator.TryGetValue(op.CodigoFJ, out var movementMap);

                scheduleMap ??= emptyScheduleMap;
                presenceMap ??= emptyPresenceMap;
                todokeMap ??= emptyTodokeMap;
                movementMap ??= emptyMovementMap;

                var attendanceDays = BuildAttendanceDaySummaries(scheduleMap, presenceMap, todokeMap, movementMap);
                var metrics = BuildPresenceMetrics(attendanceDays.Values, presenceMap, todokeMap.Values, 0);
                var latest = attendanceDays.Values
                    .OrderByDescending(item => item.Day, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();

                var row = new OperatorPresenceReportRow(
                    op.CodigoFJ,
                    op.Name,
                    op.NameJp,
                    op.ShiftName,
                    op.SectorName,
                    op.GroupName,
                    metrics.ScheduledDays,
                    metrics.PresentDays,
                    metrics.YukyuDays,
                    metrics.FaltaDays,
                    metrics.LateDays,
                    metrics.EarlyLeaveDays,
                    metrics.PendingTodokeCount,
                    metrics.PresencePercent,
                    latest?.DisplayStatus ?? "Sem registro",
                    latest?.Day ?? string.Empty,
                    latest?.Area ?? string.Empty);

                if (MatchesPresenceStatusFilter(row, statusFilter))
                {
                    rows.Add(row);
                }
            }

            var summary = new OperatorPresenceReportSummary(
                rows.Count,
                rows.Sum(item => item.ScheduledDays),
                rows.Sum(item => item.PresentDays),
                rows.Sum(item => item.YukyuDays),
                rows.Sum(item => item.FaltaDays),
                rows.Sum(item => item.LateDays),
                rows.Sum(item => item.EarlyLeaveDays),
                rows.Count == 0 ? 0 : Math.Round(rows.Average(item => item.PresencePercent), 1));

            return new OperatorPresenceReportPayload(
                startDate.ToString("yyyy-MM-dd"),
                endDate.ToString("yyyy-MM-dd"),
                summary,
                rows
                    .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList());
        }

        public OperatorManagerReportPayload GetReport(string codigoFJ, int periodDays)
        {
            if (string.IsNullOrWhiteSpace(codigoFJ))
            {
                throw new InvalidOperationException("Operador invalido para o relatorio gerencial.");
            }

            using var conn = _factory.CreateOpenConnection();
            MasterCardModuleService.EnsureSchema(conn);

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

            var masterCardRows = conn.Query<OperatorManagerMasterCardRow>(
                @"
                    SELECT
                        m.Id,
                        COALESCE(NULLIF(eq.NamePt, ''), NULLIF(eq.NameJp, ''), '') AS EquipmentName,
                        COALESCE(NULLIF(sec.NamePt, ''), NULLIF(sec.NameJp, ''), '') AS SectorName,
                        m.Status,
                        substr(m.StartDate, 1, 10) AS StartDate,
                        CASE WHEN m.ConcludedAt IS NULL THEN '' ELSE substr(m.ConcludedAt, 1, 16) END AS ConcludedAt,
                        CASE WHEN m.FollowDate IS NULL THEN '' ELSE substr(m.FollowDate, 1, 10) END AS FollowDate,
                        CASE WHEN m.FinalizedAt IS NULL THEN '' ELSE substr(m.FinalizedAt, 1, 16) END AS FinalizedAt,
                        COALESCE(m.Notes, '') AS Notes
                    FROM MasterCards m
                    LEFT JOIN Equipments eq ON eq.Id = m.EquipmentId
                    LEFT JOIN Sectors sec ON sec.Id = m.SectorId
                    WHERE m.OperatorCodigoFJ = @CodigoFJ
                      AND date(m.StartDate) BETWEEN date(@StartDate) AND date(@EndDate)
                    ORDER BY
                        CASE m.Status
                            WHEN 'follow' THEN 0
                            WHEN 'in_progress' THEN 1
                            ELSE 2
                        END,
                        date(COALESCE(m.FollowDate, m.StartDate)) ASC,
                        m.Id DESC;",
                new
                {
                    CodigoFJ = codigoFJ.Trim(),
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd")
                })
                .ToList();

            var attendanceByDay = BuildAttendanceDaySummaries(latestScheduleByDay, presentRows, latestTodokeByDay, movementRows);
            var metrics = BuildPresenceMetrics(attendanceByDay.Values, presentRows, latestTodokeByDay.Values, followUpCount);
            var production = BuildProductionSummary(operatorInfo, scheduledRows, presentRows.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase), startDate, endDate);
            var masterCards = BuildMasterCardSummary(masterCardRows);

            var dailyHistory = BuildDailyHistory(attendanceByDay.Values, presentRows, latestTodokeByDay, movementRows);

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
                masterCards,
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

        private OperatorManagerMasterCardSummary BuildMasterCardSummary(IReadOnlyList<OperatorManagerMasterCardRow> rows)
        {
            var today = DateTime.Today;
            var inProgressCount = rows.Count(item => item.Status == "in_progress");
            var followCount = rows.Count(item => item.Status == "follow");
            var completedCount = rows.Count(item => item.Status == "completed");
            var overdueFollowCount = rows.Count(item =>
                item.Status == "follow"
                && DateTime.TryParse(item.FollowDate, out var followDate)
                && followDate.Date < today);
            var dueSoonFollowCount = rows.Count(item =>
                item.Status == "follow"
                && DateTime.TryParse(item.FollowDate, out var followDate)
                && followDate.Date >= today
                && followDate.Date <= today.AddDays(7));

            return new OperatorManagerMasterCardSummary(
                rows.Count,
                inProgressCount,
                followCount,
                completedCount,
                overdueFollowCount,
                dueSoonFollowCount,
                rows
                    .Take(12)
                    .Select(item => new OperatorManagerMasterCardItem(
                        item.Id,
                        item.EquipmentName,
                        item.SectorName,
                        item.Status,
                        item.StartDate,
                        item.ConcludedAt,
                        item.FollowDate,
                        item.FinalizedAt,
                        item.Notes,
                        ResolveMasterCardFollowState(item)))
                    .ToList());
        }

        private static string ResolveMasterCardFollowState(OperatorManagerMasterCardRow row)
        {
            if (row.Status != "follow")
            {
                return row.Status switch
                {
                    "in_progress" => "in_progress",
                    "completed" => "completed",
                    _ => "none"
                };
            }

            if (!DateTime.TryParse(row.FollowDate, out var followDate))
            {
                return "follow";
            }

            var today = DateTime.Today;
            if (followDate.Date < today)
            {
                return "overdue";
            }

            if (followDate.Date <= today.AddDays(7))
            {
                return "due_soon";
            }

            return "scheduled";
        }

        private OperatorManagerPresenceMetrics BuildPresenceMetrics(
            IEnumerable<OperatorManagerAttendanceDaySummary> attendanceDays,
            IReadOnlyDictionary<string, OperatorManagerPresenceRow> presences,
            IEnumerable<OperatorManagerTodokeRow> todokes,
            int followUpCount)
        {
            var dayList = attendanceDays
                .Where(item => item.CountsAsScheduled)
                .ToList();

            var scheduledDays = dayList.Count;
            var effectivePresenceDays = dayList.Count(item => item.CountsTowardPresence);
            var coveredDays = dayList.Count(item => item.CountsTowardCoverage);
            var yukyuDays = dayList.Count(item => item.StatusKey == AttendanceStatusKeys.Yukyu);
            var faltaDays = dayList.Count(item => item.StatusKey == AttendanceStatusKeys.Falta);
            var lateDays = dayList.Count(item => item.StatusKey == AttendanceStatusKeys.Late);
            var earlyLeaveDays = dayList.Count(item => item.StatusKey == AttendanceStatusKeys.EarlyLeave);
            var pendingTodokeCount = todokes.Count(item => !item.Validated);

            var presencePercent = scheduledDays <= 0
                ? 0
                : Math.Round((effectivePresenceDays / (double)scheduledDays) * 100d, 1);

            var coveragePercent = scheduledDays <= 0
                ? 0
                : Math.Round((coveredDays / (double)scheduledDays) * 100d, 1);

            return new OperatorManagerPresenceMetrics(
                scheduledDays,
                effectivePresenceDays,
                yukyuDays,
                faltaDays,
                lateDays,
                earlyLeaveDays,
                pendingTodokeCount,
                followUpCount,
                presencePercent,
                coveragePercent);
        }

        private static bool MatchesPresenceStatusFilter(OperatorPresenceReportRow row, string statusFilter)
        {
            return statusFilter switch
            {
                "" or "all" => true,
                "present" => row.PresentDays > 0,
                "yukyu" => row.YukyuDays > 0,
                "falta" => row.FaltaDays > 0,
                "late" => row.LateDays > 0,
                "early_leave" => row.EarlyLeaveDays > 0,
                "pending" => row.PendingTodokeCount > 0,
                "issue" => row.FaltaDays > 0 || row.LateDays > 0 || row.EarlyLeaveDays > 0 || row.PendingTodokeCount > 0,
                _ => true
            };
        }

        private static DateTime ParseDateOrDefault(string value, DateTime fallback)
        {
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed)
                ? parsed.Date
                : fallback.Date;
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
                var dayScheduledMinutes = 0d;
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
                    dayScheduledMinutes += periodMinutes * localMachineIds.Count;
                    if (!string.IsNullOrWhiteSpace(item.LocalName))
                    {
                        dayLocalNames.Add(item.LocalName);
                        localNames.Add(item.LocalName);
                    }
                }

                totalMinutes += dayRunningMinutes;
                totalScheduledMinutes += dayScheduledMinutes;

                productionDays.Add(new OperatorManagerProductionDay(
                    dayDate.ToString("yyyy-MM-dd"),
                    dayRunningMinutes,
                    dayScheduledMinutes <= 0 ? 0 : Math.Round((dayRunningMinutes / dayScheduledMinutes) * 100d, 1),
                    dayLocalNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList()));
            }

            return new OperatorManagerProductionSummary(
                Math.Round(totalMinutes, 1),
                totalScheduledMinutes <= 0 ? 0 : Math.Round((totalMinutes / totalScheduledMinutes) * 100d, 1),
                localNames.ToList(),
                productionDays.Take(12).ToList());
        }

        private List<OperatorManagerDailyHistoryItem> BuildDailyHistory(
            IEnumerable<OperatorManagerAttendanceDaySummary> attendanceDays,
            IReadOnlyDictionary<string, OperatorManagerPresenceRow> presences,
            IReadOnlyDictionary<string, OperatorManagerTodokeRow> todokes,
            IReadOnlyDictionary<string, OperatorManagerMovementRow> movements)
        {
            var orderedDays = attendanceDays
                .OrderByDescending(item => item.Day, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var list = new List<OperatorManagerDailyHistoryItem>(orderedDays.Count);
            foreach (var daySummary in orderedDays)
            {
                presences.TryGetValue(daySummary.Day, out var presence);
                todokes.TryGetValue(daySummary.Day, out var todoke);
                movements.TryGetValue(daySummary.Day, out var movement);

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
                    daySummary.Day,
                    daySummary.DisplayStatus,
                    daySummary.Area,
                    string.Join(" | ", notes.Where(item => !string.IsNullOrWhiteSpace(item))),
                    todoke != null && !todoke.Validated));
            }

            return list.Take(20).ToList();
        }

        private static IReadOnlyDictionary<string, OperatorManagerAttendanceDaySummary> BuildAttendanceDaySummaries(
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
                .ToList();

            var map = new Dictionary<string, OperatorManagerAttendanceDaySummary>(StringComparer.OrdinalIgnoreCase);
            foreach (var day in allDays)
            {
                latestScheduleByDay.TryGetValue(day, out var schedule);
                presences.TryGetValue(day, out var presence);
                todokes.TryGetValue(day, out var todoke);
                movements.TryGetValue(day, out var movement);

                var statusKey = ResolveAttendanceStatusKey(schedule, presence, todoke, movement);
                var area = !string.IsNullOrWhiteSpace(schedule?.AssignmentCode)
                    ? schedule.AssignmentCode
                    : movement?.AssignmentCode ?? string.Empty;
                var countsAsScheduled = IsScheduledDay(schedule);
                var countsTowardPresence = countsAsScheduled && statusKey != AttendanceStatusKeys.Falta
                    && statusKey != AttendanceStatusKeys.Late
                    && statusKey != AttendanceStatusKeys.EarlyLeave;
                var countsTowardCoverage = countsAsScheduled && statusKey != AttendanceStatusKeys.Falta;

                map[day] = new OperatorManagerAttendanceDaySummary(
                    day,
                    statusKey,
                    ResolveDisplayStatus(statusKey),
                    area,
                    countsAsScheduled,
                    countsTowardPresence,
                    countsTowardCoverage);
            }

            return map;
        }

        private static string ResolveAttendanceStatusKey(
            OperatorManagerScheduleRow? schedule,
            OperatorManagerPresenceRow? presence,
            OperatorManagerTodokeRow? todoke,
            OperatorManagerMovementRow? movement)
        {
            if (todoke?.MotiveId == 2)
            {
                return AttendanceStatusKeys.Falta;
            }

            if (todoke?.MotiveId == 3
                || string.Equals(movement?.MovementType, "late", StringComparison.OrdinalIgnoreCase)
                || string.Equals(schedule?.AvailabilityStatus, "Atraso", StringComparison.OrdinalIgnoreCase))
            {
                return AttendanceStatusKeys.Late;
            }

            if (todoke?.MotiveId == 5
                || string.Equals(movement?.MovementType, "early_leave", StringComparison.OrdinalIgnoreCase)
                || string.Equals(schedule?.AvailabilityStatus, "Saiu cedo", StringComparison.OrdinalIgnoreCase)
                || string.Equals(schedule?.AvailabilityStatus, "Saida antecipada", StringComparison.OrdinalIgnoreCase))
            {
                return AttendanceStatusKeys.EarlyLeave;
            }

            if (todoke?.MotiveId == 1
                || string.Equals(schedule?.AvailabilityStatus, "Yukyu", StringComparison.OrdinalIgnoreCase))
            {
                return AttendanceStatusKeys.Yukyu;
            }

            if (string.Equals(schedule?.AvailabilityStatus, "Folga", StringComparison.OrdinalIgnoreCase))
            {
                return AttendanceStatusKeys.Folga;
            }

            if (presence != null)
            {
                return AttendanceStatusKeys.Present;
            }

            if (IsScheduledDay(schedule))
            {
                return AttendanceStatusKeys.Scheduled;
            }

            return AttendanceStatusKeys.NoRecord;
        }

        private static bool IsScheduledDay(OperatorManagerScheduleRow? schedule)
        {
            return schedule != null
                && !string.Equals(schedule.AvailabilityStatus, "Folga", StringComparison.OrdinalIgnoreCase)
                && (!string.IsNullOrWhiteSpace(schedule.AssignmentCode)
                    || schedule.LocalId > 0
                    || !string.IsNullOrWhiteSpace(schedule.AvailabilityStatus));
        }

        private static string ResolveDisplayStatus(string statusKey)
        {
            return statusKey switch
            {
                AttendanceStatusKeys.Falta => "Falta",
                AttendanceStatusKeys.Late => "Atraso",
                AttendanceStatusKeys.EarlyLeave => "Sair cedo",
                AttendanceStatusKeys.Yukyu => "Yukyu",
                AttendanceStatusKeys.Folga => "Folga",
                AttendanceStatusKeys.Present => "Presente",
                AttendanceStatusKeys.Scheduled => "Escalado",
                _ => "Sem registro"
            };
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
            public string OperatorCodigoFJ { get; set; } = string.Empty;
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

        private sealed class OperatorPresenceReportPresenceRow
        {
            public string OperatorCodigoFJ { get; set; } = string.Empty;
            public string Day { get; set; } = string.Empty;
            public DateTime? LastPresence { get; set; }
        }

        private sealed class OperatorManagerTodokeRow
        {
            public string OperatorCodigoFJ { get; set; } = string.Empty;
            public string Day { get; set; } = string.Empty;
            public long Id { get; set; }
            public int MotiveId { get; set; }
            public string MotiveName { get; set; } = string.Empty;
            public string Notes { get; set; } = string.Empty;
            public bool Validated { get; set; }
        }

        private sealed class OperatorManagerMovementRow
        {
            public string OperatorCodigoFJ { get; set; } = string.Empty;
            public string Day { get; set; } = string.Empty;
            public string MovementType { get; set; } = string.Empty;
            public string EventTime { get; set; } = string.Empty;
            public string AssignmentCode { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
        }

        private sealed class OperatorManagerFollowUpCountRow
        {
            public string OperatorCodigoFJ { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        private sealed record OperatorManagerAttendanceDaySummary(
            string Day,
            string StatusKey,
            string DisplayStatus,
            string Area,
            bool CountsAsScheduled,
            bool CountsTowardPresence,
            bool CountsTowardCoverage);

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

        private sealed class OperatorManagerMasterCardRow
        {
            public int Id { get; set; }
            public string EquipmentName { get; set; } = string.Empty;
            public string SectorName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string StartDate { get; set; } = string.Empty;
            public string ConcludedAt { get; set; } = string.Empty;
            public string FollowDate { get; set; } = string.Empty;
            public string FinalizedAt { get; set; } = string.Empty;
            public string Notes { get; set; } = string.Empty;
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

        private static class AttendanceStatusKeys
        {
            public const string Scheduled = "scheduled";
            public const string Present = "present";
            public const string Yukyu = "yukyu";
            public const string Falta = "falta";
            public const string Late = "late";
            public const string EarlyLeave = "early_leave";
            public const string Folga = "folga";
            public const string NoRecord = "no_record";
        }
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

    public sealed record OperatorPresenceReportFilter(
        string StartDateIso,
        string EndDateIso,
        int ShiftId,
        int SectorId,
        int GroupId,
        string Status,
        string Search);

    public sealed record OperatorPresenceReportPayload(
        string StartDateIso,
        string EndDateIso,
        OperatorPresenceReportSummary Summary,
        IReadOnlyList<OperatorPresenceReportRow> Rows);

    public sealed record OperatorPresenceReportSummary(
        int OperatorCount,
        int ScheduledDays,
        int PresentDays,
        int YukyuDays,
        int FaltaDays,
        int LateDays,
        int EarlyLeaveDays,
        double PresencePercent);

    public sealed record OperatorPresenceReportRow(
        string CodigoFJ,
        string Name,
        string NameJp,
        string ShiftName,
        string SectorName,
        string GroupName,
        int ScheduledDays,
        int PresentDays,
        int YukyuDays,
        int FaltaDays,
        int LateDays,
        int EarlyLeaveDays,
        int PendingTodokeCount,
        double PresencePercent,
        string LastStatus,
        string LastDateIso,
        string LastArea);

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
        OperatorManagerMasterCardSummary MasterCards,
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

    public sealed record OperatorManagerMasterCardSummary(
        int TotalCount,
        int InProgressCount,
        int FollowCount,
        int CompletedCount,
        int OverdueFollowCount,
        int DueSoonFollowCount,
        IReadOnlyList<OperatorManagerMasterCardItem> Items);

    public sealed record OperatorManagerMasterCardItem(
        int Id,
        string EquipmentName,
        string SectorName,
        string Status,
        string StartDateIso,
        string ConcludedAt,
        string FollowDateIso,
        string FinalizedAt,
        string Notes,
        string FollowState);

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
