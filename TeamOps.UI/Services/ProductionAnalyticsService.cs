using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dapper;
using TeamOps.Data.Db;
using TeamOps.UI.Forms.Models;

namespace TeamOps.Services
{
    public sealed class ProductionAnalyticsService
    {
        private const int HistoryDays = 7;

        private readonly SqliteConnectionFactory _factory;

        public ProductionAnalyticsService(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public ProductionShiftPeriod GetShiftPeriod(DateTime date, string shiftName)
        {
            var normalized = (shiftName ?? string.Empty).Trim().ToLowerInvariant();
            var isNightShift = normalized.Contains("yakin", StringComparison.Ordinal)
                               || normalized.Contains("å¤œå‹¤", StringComparison.Ordinal);

            return isNightShift
                ? new ProductionShiftPeriod
                {
                    Start = date.Date.AddHours(20).AddMinutes(35),
                    End = date.Date.AddDays(1).AddHours(8).AddMinutes(35)
                }
                : new ProductionShiftPeriod
                {
                    Start = date.Date.AddHours(8).AddMinutes(35),
                    End = date.Date.AddHours(20).AddMinutes(35)
                };
        }

        public ProductionDashboardDto BuildDashboard(ProductionDashboardFilter filter)
        {
            using var conn = _factory.CreateOpenConnection();
            ProductionSchemaMigrator.Ensure(conn);

            var shifts = conn.Query<ShiftLookupRow>(
                @"
                    SELECT
                        Id,
                        COALESCE(NamePt, '') AS NamePt,
                        COALESCE(NULLIF(NameJp, ''), NamePt, '') AS NameJp
                    FROM Shifts
                    ORDER BY Id;"
            ).ToList();

            var shift = shifts.FirstOrDefault(item => item.Id == filter.ShiftId)
                ?? throw new InvalidOperationException("Turno nao encontrado para o monitor de producao.");

            var period = GetShiftPeriod(filter.Date, BuildShiftPeriodHint(shift.NamePt, shift.NameJp));
            var dashboard = new ProductionDashboardDto
            {
                Period = period
            };

            var machines = conn.Query<MachineRow>(
                @"
                    SELECT
                        m.Id,
                        COALESCE(m.MachineCode, '') AS MachineCode,
                        COALESCE(m.NamePt, '') AS MachineNamePt,
                        COALESCE(NULLIF(m.NameJp, ''), m.NamePt, '') AS MachineNameJp,
                        m.LocalId,
                        m.SectorId,
                        COALESCE(l.NamePt, '') AS LocalNamePt,
                        COALESCE(NULLIF(l.NameJp, ''), l.NamePt, '') AS LocalNameJp,
                        COALESCE(s.NamePt, '') AS SectorNamePt,
                        COALESCE(NULLIF(s.NameJp, ''), s.NamePt, '') AS SectorNameJp
                    FROM Machines m
                    LEFT JOIN Locals l ON l.Id = m.LocalId
                    LEFT JOIN Sectors s ON s.Id = m.SectorId
                    WHERE COALESCE(m.IsActive, 1) = 1
                      AND (@sectorId <= 0 OR m.SectorId = @sectorId)
                      AND (@localId <= 0 OR m.LocalId = @localId)
                      AND (@machineCode = '' OR m.MachineCode = @machineCode)
                    ORDER BY
                        COALESCE(s.NamePt, ''),
                        COALESCE(l.NamePt, ''),
                        COALESCE(m.MachineCode, ''),
                        m.Id;",
                new
                {
                    sectorId = filter.SectorId,
                    localId = filter.LocalId,
                    machineCode = (filter.MachineCode ?? string.Empty).Trim()
                }
            ).ToList();

            if (machines.Count == 0)
            {
                return dashboard;
            }

            var machineIds = machines.Select(machine => machine.Id).ToArray();
            var historyStartDate = filter.Date.Date.AddDays(-(HistoryDays - 1));
            var rangeStart = historyStartDate.AddDays(-1);
            var rangeEnd = filter.Date.Date.AddDays(2);

            var statusRows = conn.Query<StatusRow>(
                @"
                    SELECT
                        MachineId,
                        StatusCode,
                        COALESCE(StatusText, '') AS StatusText,
                        COALESCE(RecipeName, '') AS RecipeName,
                        COALESCE(LotNo, '') AS LotNo,
                        EventDateTime
                    FROM MachineCurrentStatus
                    WHERE MachineId IN @machineIds;",
                new
                {
                    machineIds
                }
            ).ToDictionary(row => row.MachineId);

            var events = conn.Query<EventRow>(
                @"
                    SELECT
                        MachineId,
                        StatusCode,
                        COALESCE(StatusText, '') AS StatusText,
                        COALESCE(RecipeName, '') AS RecipeName,
                        COALESCE(LotNo, '') AS LotNo,
                        EventDateTime
                    FROM MachineEvents
                    WHERE MachineId IN @machineIds
                      AND datetime(EventDateTime) <= datetime(@rangeEnd)
                      AND datetime(EventDateTime) >= datetime(@rangeStart)
                    ORDER BY MachineId, datetime(EventDateTime), Id;",
                new
                {
                    machineIds,
                    rangeStart = rangeStart.ToString("yyyy-MM-dd HH:mm:ss"),
                    rangeEnd = rangeEnd.ToString("yyyy-MM-dd HH:mm:ss")
                }
            ).ToList();

            var eventsByMachine = events
                .GroupBy(row => row.MachineId)
                .ToDictionary(group => group.Key, group => group.OrderBy(item => item.EventDateTime).ToList());

            var scheduleCurrentRows = conn.Query<ScheduleRow>(
                @"
                    SELECT
                        sc.ScheduleDate,
                        sc.LocalId,
                        COALESCE(sc.CodigoFJ, '') AS OperatorCodigoFJ,
                        COALESCE(op.NameRomanji, sc.CodigoFJ) AS OperatorNamePt,
                        COALESCE(NULLIF(op.NameNihongo, ''), op.NameRomanji, sc.CodigoFJ) AS OperatorNameJp
                    FROM OperatorSchedule sc
                    LEFT JOIN Operators op ON op.CodigoFJ = sc.CodigoFJ
                    WHERE date(sc.ScheduleDate) = date(@scheduleDate)
                      AND sc.ShiftId = @shiftId;",
                new
                {
                    scheduleDate = filter.Date.ToString("yyyy-MM-dd"),
                    shiftId = filter.ShiftId
                }
            ).ToList();

            var scheduleHistoryRows = conn.Query<ScheduleRow>(
                @"
                    SELECT
                        sc.ScheduleDate,
                        sc.LocalId,
                        COALESCE(sc.CodigoFJ, '') AS OperatorCodigoFJ,
                        COALESCE(op.NameRomanji, sc.CodigoFJ) AS OperatorNamePt,
                        COALESCE(NULLIF(op.NameNihongo, ''), op.NameRomanji, sc.CodigoFJ) AS OperatorNameJp
                    FROM OperatorSchedule sc
                    LEFT JOIN Operators op ON op.CodigoFJ = sc.CodigoFJ
                    WHERE date(sc.ScheduleDate) BETWEEN date(@startDate) AND date(@endDate)
                      AND sc.ShiftId = @shiftId;",
                new
                {
                    startDate = historyStartDate.ToString("yyyy-MM-dd"),
                    endDate = filter.Date.ToString("yyyy-MM-dd"),
                    shiftId = filter.ShiftId
                }
            ).ToList();

            var operatorsByLocal = scheduleCurrentRows
                .GroupBy(row => row.LocalId)
                .ToDictionary(group => group.Key, group => group.ToList());

            var machineSummaries = BuildMachineSummaries(
                machines,
                statusRows,
                eventsByMachine,
                operatorsByLocal,
                period);

            dashboard.Machines.AddRange(machineSummaries);

            var aggregate = Summarize(machineSummaries);
            dashboard.ProductionPercent = aggregate.ProductionPercent;
            dashboard.MachinesRunning = aggregate.MachinesRunning;
            dashboard.MachinesStopped = aggregate.MachinesStopped;
            dashboard.ErrorMinutes = aggregate.ErrorMinutes;
            dashboard.InactiveMinutes = aggregate.InactiveMinutes;

            dashboard.Areas.AddRange(BuildAreaSummaries(machineSummaries, operatorsByLocal));

            dashboard.Ranking.AddRange(
                machineSummaries
                    .Select(machine => new ProductionRankingItemDto
                    {
                        LocalId = machine.LocalId,
                        LocalNamePt = machine.LocalNamePt,
                        LocalNameJp = machine.LocalNameJp,
                        MachineCode = machine.MachineCode,
                        MachineNamePt = machine.MachineNamePt,
                        MachineNameJp = machine.MachineNameJp,
                        StopMinutes = Math.Round(machine.StoppedMinutes, 1),
                        ErrorMinutes = Math.Round(machine.ErrorMinutes, 1),
                        TotalImpactMinutes = Math.Round(machine.StoppedMinutes + machine.ErrorMinutes, 1)
                    })
                    .OrderByDescending(item => item.TotalImpactMinutes)
                    .ThenBy(item => item.MachineCode, StringComparer.OrdinalIgnoreCase)
                    .Take(12)
            );

            var cellMoments = BuildTimelineMoments(period);
            foreach (var machine in machineSummaries)
            {
                eventsByMachine.TryGetValue(machine.MachineId, out var machineEvents);
                machineEvents ??= new List<EventRow>();

                var timelineRow = new ProductionTimelineRowDto
                {
                    LocalId = machine.LocalId,
                    LocalNamePt = machine.LocalNamePt,
                    LocalNameJp = machine.LocalNameJp,
                    MachineCode = machine.MachineCode,
                    MachineNamePt = machine.MachineNamePt,
                    MachineNameJp = machine.MachineNameJp
                };

                foreach (var moment in cellMoments)
                {
                    var statusCode = ResolveStatusAt(machineEvents, moment);
                    timelineRow.Cells.Add(new ProductionTimelineCellDto
                    {
                        TimeLabel = moment.ToString("HH:mm", CultureInfo.InvariantCulture),
                        DateTime = moment,
                        StatusCode = statusCode,
                        CssClass = GetTimelineClass(statusCode)
                    });
                }

                dashboard.Timeline.Add(timelineRow);
            }

            foreach (var shiftItem in shifts)
            {
                var comparisonPeriod = GetShiftPeriod(filter.Date, BuildShiftPeriodHint(shiftItem.NamePt, shiftItem.NameJp));
                var comparisonSummaries = BuildMachineSummaries(
                    machines,
                    statusRows,
                    eventsByMachine,
                    new Dictionary<int, List<ScheduleRow>>(),
                    comparisonPeriod);

                var comparisonAggregate = Summarize(comparisonSummaries);
                dashboard.ShiftComparisons.Add(new ProductionShiftComparisonDto
                {
                    ShiftId = shiftItem.Id,
                    ShiftNamePt = shiftItem.NamePt,
                    ShiftNameJp = shiftItem.NameJp,
                    Start = comparisonPeriod.Start,
                    End = comparisonPeriod.End,
                    ProductionPercent = comparisonAggregate.ProductionPercent,
                    RunningMinutes = comparisonAggregate.RunningMinutes,
                    StoppedMinutes = comparisonAggregate.StoppedMinutes,
                    InactiveMinutes = comparisonAggregate.InactiveMinutes,
                    ErrorMinutes = comparisonAggregate.ErrorMinutes,
                    MachineCount = comparisonSummaries.Count
                });
            }

            var selectedShiftName = BuildShiftPeriodHint(shift.NamePt, shift.NameJp);
            for (var offset = HistoryDays - 1; offset >= 0; offset--)
            {
                var day = filter.Date.Date.AddDays(-offset);
                var dayPeriod = GetShiftPeriod(day, selectedShiftName);
                var daySummaries = BuildMachineSummaries(
                    machines,
                    statusRows,
                    eventsByMachine,
                    new Dictionary<int, List<ScheduleRow>>(),
                    dayPeriod);

                var dayAggregate = Summarize(daySummaries);
                dashboard.DailyTrend.Add(new ProductionDailyTrendDto
                {
                    Date = day,
                    Label = day.ToString("MM/dd", CultureInfo.InvariantCulture),
                    ProductionPercent = dayAggregate.ProductionPercent,
                    RunningMinutes = dayAggregate.RunningMinutes,
                    StoppedMinutes = dayAggregate.StoppedMinutes,
                    InactiveMinutes = dayAggregate.InactiveMinutes,
                    ErrorMinutes = dayAggregate.ErrorMinutes
                });
            }

            foreach (var area in dashboard.Areas.OrderBy(item => item.LocalNamePt, StringComparer.OrdinalIgnoreCase))
            {
                var machineRows = machines
                    .Where(machine => machine.LocalId == area.LocalId)
                    .ToList();

                var areaHistory = new ProductionAreaHistoryDto
                {
                    LocalId = area.LocalId,
                    LocalNamePt = area.LocalNamePt,
                    LocalNameJp = area.LocalNameJp
                };

                for (var offset = HistoryDays - 1; offset >= 0; offset--)
                {
                    var day = filter.Date.Date.AddDays(-offset);
                    var dayPeriod = GetShiftPeriod(day, selectedShiftName);
                    var areaSummaries = BuildMachineSummaries(
                        machineRows,
                        statusRows,
                        eventsByMachine,
                        new Dictionary<int, List<ScheduleRow>>(),
                        dayPeriod);

                    var areaAggregate = Summarize(areaSummaries);
                    areaHistory.Days.Add(new ProductionDailyTrendDto
                    {
                        Date = day,
                        Label = day.ToString("MM/dd", CultureInfo.InvariantCulture),
                        ProductionPercent = areaAggregate.ProductionPercent,
                        RunningMinutes = areaAggregate.RunningMinutes,
                        StoppedMinutes = areaAggregate.StoppedMinutes,
                        InactiveMinutes = areaAggregate.InactiveMinutes,
                        ErrorMinutes = areaAggregate.ErrorMinutes
                    });
                }

                dashboard.AreaHistory.Add(areaHistory);
            }

            var areaDayMap = dashboard.AreaHistory
                .SelectMany(area => area.Days.Select(day => new
                {
                    area.LocalId,
                    day.Date,
                    day.RunningMinutes
                }))
                .GroupBy(item => new { item.LocalId, Day = item.Date.Date })
                .ToDictionary(
                    group => (group.Key.LocalId ?? 0, group.Key.Day),
                    group => group.First().RunningMinutes);

            var operatorScheduleGroups = scheduleHistoryRows
                .Where(row => row.LocalId > 0 && !string.IsNullOrWhiteSpace(row.OperatorCodigoFJ))
                .GroupBy(row => new { Day = row.ScheduleDate.Date, row.LocalId });

            var operatorAccumulator = new Dictionary<string, ProductionOperatorAccumulator>(StringComparer.OrdinalIgnoreCase);
            var selectedPeriodMinutes = Math.Max(0, (period.End - period.Start).TotalMinutes);

            foreach (var scheduleGroup in operatorScheduleGroups)
            {
                var key = (scheduleGroup.Key.LocalId, scheduleGroup.Key.Day);
                if (!areaDayMap.TryGetValue(key, out var areaRunningMinutes) || areaRunningMinutes <= 0)
                {
                    continue;
                }

                var operators = scheduleGroup
                    .GroupBy(item => item.OperatorCodigoFJ, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First())
                    .ToList();

                if (operators.Count == 0)
                {
                    continue;
                }

                foreach (var item in operators)
                {
                    if (!operatorAccumulator.TryGetValue(item.OperatorCodigoFJ, out var accumulator))
                    {
                        accumulator = new ProductionOperatorAccumulator
                        {
                            OperatorCodigoFJ = item.OperatorCodigoFJ,
                            OperatorNamePt = item.OperatorNamePt,
                            OperatorNameJp = item.OperatorNameJp
                        };
                        operatorAccumulator[item.OperatorCodigoFJ] = accumulator;
                    }

                    // When an area is worked by a pair, both operators carry the same
                    // production history for that area/day because the final result depends
                    // on the full flow executed by the duo, not on splitting machine time.
                    accumulator.EstimatedRunningMinutes += areaRunningMinutes;

                    var localMachine = machines.FirstOrDefault(machine => machine.LocalId == item.LocalId);
                    if (localMachine != null)
                    {
                        accumulator.LocalNamesPt.Add(localMachine.LocalNamePt);
                        accumulator.LocalNamesJp.Add(localMachine.LocalNameJp);
                    }
                }
            }

            dashboard.OperatorRanking.AddRange(
                operatorAccumulator.Values
                    .Select(item => new ProductionOperatorRankingDto
                    {
                        OperatorCodigoFJ = item.OperatorCodigoFJ,
                        OperatorNamePt = item.OperatorNamePt,
                        OperatorNameJp = item.OperatorNameJp,
                        EstimatedRunningMinutes = Math.Round(item.EstimatedRunningMinutes, 1),
                        EstimatedKadouritsuPercent = selectedPeriodMinutes <= 0
                            ? 0
                            : Math.Round((item.EstimatedRunningMinutes / selectedPeriodMinutes) * 100d, 1),
                        LocalNamesPt = item.LocalNamesPt
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList(),
                        LocalNamesJp = item.LocalNamesJp
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList()
                    })
                    .OrderByDescending(item => item.EstimatedRunningMinutes)
                    .ThenBy(item => item.OperatorNamePt, StringComparer.OrdinalIgnoreCase)
                    .Take(12)
            );

            return dashboard;
        }

        private static string BuildShiftPeriodHint(string namePt, string nameJp)
        {
            var normalizedPt = (namePt ?? string.Empty).Trim();
            var normalizedJp = (nameJp ?? string.Empty).Trim();
            var combined = $"{normalizedPt} {normalizedJp}".Trim();

            if (normalizedPt.Contains("noite", StringComparison.OrdinalIgnoreCase)
                || normalizedPt.Contains("night", StringComparison.OrdinalIgnoreCase)
                || normalizedPt.Contains("yakin", StringComparison.OrdinalIgnoreCase)
                || normalizedJp.Contains("夜", StringComparison.Ordinal)
                || normalizedJp.Contains("夜勤", StringComparison.Ordinal))
            {
                return "yakin";
            }

            return string.IsNullOrWhiteSpace(combined) ? "hirukin" : combined;
        }

        private static List<ProductionMachineSummaryDto> BuildMachineSummaries(
            IReadOnlyCollection<MachineRow> machines,
            IReadOnlyDictionary<int, StatusRow> statusRows,
            IReadOnlyDictionary<int, List<EventRow>> eventsByMachine,
            IReadOnlyDictionary<int, List<ScheduleRow>> operatorsByLocal,
            ProductionShiftPeriod period)
        {
            var summaries = new List<ProductionMachineSummaryDto>(machines.Count);

            foreach (var machine in machines)
            {
                eventsByMachine.TryGetValue(machine.Id, out var machineEvents);
                machineEvents ??= new List<EventRow>();

                var metrics = CalculateMetrics(machineEvents, period);
                StatusRow? currentStatus = machineEvents
                    .Where(item => item.EventDateTime <= period.End)
                    .OrderByDescending(item => item.EventDateTime)
                    .FirstOrDefault();

                if (currentStatus == null && statusRows.TryGetValue(machine.Id, out var statusFallback))
                {
                    currentStatus = statusFallback;
                }

                var summary = new ProductionMachineSummaryDto
                {
                    MachineId = machine.Id,
                    MachineCode = machine.MachineCode,
                    MachineNamePt = machine.MachineNamePt,
                    MachineNameJp = machine.MachineNameJp,
                    SectorId = machine.SectorId,
                    SectorNamePt = machine.SectorNamePt,
                    SectorNameJp = machine.SectorNameJp,
                    LocalId = machine.LocalId,
                    LocalNamePt = machine.LocalNamePt,
                    LocalNameJp = machine.LocalNameJp,
                    StatusCode = currentStatus?.StatusCode ?? 2,
                    StatusText = string.IsNullOrWhiteSpace(currentStatus?.StatusText)
                        ? GetStatusLabel(currentStatus?.StatusCode ?? 2, "pt-BR")
                        : currentStatus!.StatusText,
                    RecipeName = currentStatus?.RecipeName ?? string.Empty,
                    LotNo = currentStatus?.LotNo ?? string.Empty,
                    LastUpdate = currentStatus?.EventDateTime,
                    RunningMinutes = metrics.RunningMinutes,
                    StoppedMinutes = metrics.StoppedMinutes,
                    InactiveMinutes = metrics.InactiveMinutes,
                    ErrorMinutes = metrics.ErrorMinutes,
                    TotalMinutes = metrics.TotalMinutes,
                    ProductionPercent = metrics.TotalMinutes <= 0
                        ? 0
                        : Math.Round((metrics.RunningMinutes / metrics.TotalMinutes) * 100d, 1)
                };

                if (machine.LocalId.HasValue && operatorsByLocal.TryGetValue(machine.LocalId.Value, out var assignedOperators))
                {
                    summary.ScheduledOperatorsPt.AddRange(
                        assignedOperators
                            .Select(item => item.OperatorNamePt)
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .Distinct(StringComparer.OrdinalIgnoreCase));

                    summary.ScheduledOperatorsJp.AddRange(
                        assignedOperators
                            .Select(item => item.OperatorNameJp)
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .Distinct(StringComparer.OrdinalIgnoreCase));
                }

                summaries.Add(summary);
            }

            return summaries;
        }

        private static List<ProductionAreaSummaryDto> BuildAreaSummaries(
            IReadOnlyCollection<ProductionMachineSummaryDto> machines,
            IReadOnlyDictionary<int, List<ScheduleRow>> operatorsByLocal)
        {
            return machines
                .GroupBy(machine => machine.LocalId)
                .Select(group =>
                {
                    var machineList = group.ToList();
                    var first = machineList.First();
                    var aggregate = Summarize(machineList);

                    var area = new ProductionAreaSummaryDto
                    {
                        LocalId = first.LocalId,
                        SectorId = first.SectorId,
                        LocalNamePt = first.LocalNamePt,
                        LocalNameJp = first.LocalNameJp,
                        SectorNamePt = first.SectorNamePt,
                        SectorNameJp = first.SectorNameJp,
                        MachineCount = machineList.Count,
                        MachinesRunning = aggregate.MachinesRunning,
                        MachinesStopped = aggregate.MachinesStopped,
                        RunningMinutes = aggregate.RunningMinutes,
                        StoppedMinutes = aggregate.StoppedMinutes,
                        InactiveMinutes = aggregate.InactiveMinutes,
                        ErrorMinutes = aggregate.ErrorMinutes,
                        TotalMinutes = aggregate.TotalMinutes,
                        ProductionPercent = aggregate.ProductionPercent,
                        LastUpdate = machineList
                            .Where(item => item.LastUpdate.HasValue)
                            .Select(item => item.LastUpdate)
                            .OrderByDescending(item => item)
                            .FirstOrDefault()
                    };

                    if (first.LocalId.HasValue && operatorsByLocal.TryGetValue(first.LocalId.Value, out var localOperators))
                    {
                        area.ScheduledOperatorsPt.AddRange(
                            localOperators
                                .Select(item => item.OperatorNamePt)
                                .Where(name => !string.IsNullOrWhiteSpace(name))
                                .Distinct(StringComparer.OrdinalIgnoreCase));

                        area.ScheduledOperatorsJp.AddRange(
                            localOperators
                                .Select(item => item.OperatorNameJp)
                                .Where(name => !string.IsNullOrWhiteSpace(name))
                                .Distinct(StringComparer.OrdinalIgnoreCase));
                    }

                    return area;
                })
                .OrderBy(item => item.LocalNamePt, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static SummaryAggregate Summarize(IEnumerable<ProductionMachineSummaryDto> machines)
        {
            var machineList = machines.ToList();
            var totalMinutes = machineList.Sum(machine => machine.TotalMinutes);
            var runningMinutes = machineList.Sum(machine => machine.RunningMinutes);

            return new SummaryAggregate(
                RunningMinutes: Math.Round(runningMinutes, 1),
                StoppedMinutes: Math.Round(machineList.Sum(machine => machine.StoppedMinutes), 1),
                InactiveMinutes: Math.Round(machineList.Sum(machine => machine.InactiveMinutes), 1),
                ErrorMinutes: Math.Round(machineList.Sum(machine => machine.ErrorMinutes), 1),
                TotalMinutes: Math.Round(totalMinutes, 1),
                ProductionPercent: totalMinutes <= 0
                    ? 0
                    : Math.Round((runningMinutes / totalMinutes) * 100d, 1),
                MachinesRunning: machineList.Count(machine => machine.StatusCode == 0),
                MachinesStopped: machineList.Count(machine => machine.StatusCode == 1));
        }

        private static List<DateTime> BuildTimelineMoments(ProductionShiftPeriod period)
        {
            var cells = new List<DateTime>();
            for (var cursor = period.Start; cursor < period.End; cursor = cursor.AddMinutes(5))
            {
                cells.Add(cursor);
            }

            return cells;
        }

        private static ProductionMetrics CalculateMetrics(IReadOnlyList<EventRow> machineEvents, ProductionShiftPeriod period)
        {
            var running = 0d;
            var stopped = 0d;
            var inactive = 0d;
            var error = 0d;
            var total = Math.Max(0, (period.End - period.Start).TotalMinutes);

            var seed = machineEvents
                .Where(item => item.EventDateTime <= period.Start)
                .OrderByDescending(item => item.EventDateTime)
                .FirstOrDefault();

            var cursor = period.Start;
            var currentStatus = seed?.StatusCode;

            foreach (var machineEvent in machineEvents.Where(item => item.EventDateTime >= period.Start && item.EventDateTime <= period.End))
            {
                if (machineEvent.EventDateTime > cursor)
                {
                    AddMinutes(currentStatus ?? 2, (machineEvent.EventDateTime - cursor).TotalMinutes, ref running, ref stopped, ref inactive, ref error);
                }

                cursor = machineEvent.EventDateTime < period.Start
                    ? period.Start
                    : machineEvent.EventDateTime;
                currentStatus = machineEvent.StatusCode;
            }

            if (cursor < period.End)
            {
                AddMinutes(currentStatus ?? 2, (period.End - cursor).TotalMinutes, ref running, ref stopped, ref inactive, ref error);
            }

            return new ProductionMetrics(running, stopped, inactive, error, total);
        }

        private static int ResolveStatusAt(IReadOnlyList<EventRow> machineEvents, DateTime moment)
        {
            var eventAtMoment = machineEvents
                .Where(item => item.EventDateTime <= moment)
                .OrderByDescending(item => item.EventDateTime)
                .FirstOrDefault();

            return eventAtMoment?.StatusCode ?? 2;
        }

        private static void AddMinutes(int statusCode, double minutes, ref double running, ref double stopped, ref double inactive, ref double error)
        {
            if (minutes <= 0)
            {
                return;
            }

            switch (statusCode)
            {
                case 0:
                    running += minutes;
                    break;
                case 1:
                    stopped += minutes;
                    break;
                case 2:
                    inactive += minutes;
                    break;
                case 3:
                    error += minutes;
                    break;
                default:
                    inactive += minutes;
                    break;
            }
        }

        public static string GetStatusLabel(int statusCode, string locale)
        {
            if (string.Equals(locale, "ja-JP", StringComparison.OrdinalIgnoreCase))
            {
                return statusCode switch
                {
                    0 => "稼動中",
                    1 => "停止",
                    2 => "非稼動",
                    3 => "異常",
                    _ => "-"
                };
            }

            return (statusCode, locale) switch
            {
                (0, "ja-JP") => "ç¨¼å‹•ä¸­",
                (1, "ja-JP") => "åœæ­¢",
                (2, "ja-JP") => "éžç¨¼å‹•",
                (3, "ja-JP") => "ç•°å¸¸",
                (0, _) => "Rodando",
                (1, _) => "Parado",
                (2, _) => "Inativo",
                (3, _) => "Erro",
                _ => "-"
            };
        }

        public static string GetTimelineClass(int statusCode)
        {
            return statusCode switch
            {
                0 => "status-0",
                1 => "status-1",
                2 => "status-2",
                3 => "status-3",
                _ => "status-2"
            };
        }

        private sealed class ShiftLookupRow
        {
            public int Id { get; set; }
            public string NamePt { get; set; } = string.Empty;
            public string NameJp { get; set; } = string.Empty;
        }

        private sealed class MachineRow
        {
            public int Id { get; set; }
            public string MachineCode { get; set; } = string.Empty;
            public string MachineNamePt { get; set; } = string.Empty;
            public string MachineNameJp { get; set; } = string.Empty;
            public int? LocalId { get; set; }
            public int? SectorId { get; set; }
            public string LocalNamePt { get; set; } = string.Empty;
            public string LocalNameJp { get; set; } = string.Empty;
            public string SectorNamePt { get; set; } = string.Empty;
            public string SectorNameJp { get; set; } = string.Empty;
        }

        private class StatusRow
        {
            public int MachineId { get; set; }
            public int StatusCode { get; set; }
            public string StatusText { get; set; } = string.Empty;
            public string RecipeName { get; set; } = string.Empty;
            public string LotNo { get; set; } = string.Empty;
            public DateTime EventDateTime { get; set; }
        }

        private sealed class EventRow : StatusRow
        {
        }

        private sealed class ScheduleRow
        {
            public DateTime ScheduleDate { get; set; }
            public int LocalId { get; set; }
            public string OperatorCodigoFJ { get; set; } = string.Empty;
            public string OperatorNamePt { get; set; } = string.Empty;
            public string OperatorNameJp { get; set; } = string.Empty;
        }

        private sealed class ProductionOperatorAccumulator
        {
            public string OperatorCodigoFJ { get; set; } = string.Empty;
            public string OperatorNamePt { get; set; } = string.Empty;
            public string OperatorNameJp { get; set; } = string.Empty;
            public double EstimatedRunningMinutes { get; set; }
            public HashSet<string> LocalNamesPt { get; } = new(StringComparer.OrdinalIgnoreCase);
            public HashSet<string> LocalNamesJp { get; } = new(StringComparer.OrdinalIgnoreCase);
        }

        private readonly record struct SummaryAggregate(
            double RunningMinutes,
            double StoppedMinutes,
            double InactiveMinutes,
            double ErrorMinutes,
            double TotalMinutes,
            double ProductionPercent,
            int MachinesRunning,
            int MachinesStopped);

        private readonly record struct ProductionMetrics(
            double RunningMinutes,
            double StoppedMinutes,
            double InactiveMinutes,
            double ErrorMinutes,
            double TotalMinutes);
    }
}
