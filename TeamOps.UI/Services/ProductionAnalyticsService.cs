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
        private readonly SqliteConnectionFactory _factory;

        public ProductionAnalyticsService(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public ProductionShiftPeriod GetShiftPeriod(DateTime date, string shiftName)
        {
            var normalized = (shiftName ?? string.Empty).Trim().ToLowerInvariant();
            var isNightShift = normalized.Contains("yakin", StringComparison.Ordinal)
                               || normalized.Contains("夜勤", StringComparison.Ordinal);

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

            var shift = conn.QueryFirstOrDefault<ShiftLookupRow>(
                @"
                    SELECT
                        Id,
                        COALESCE(NamePt, '') AS NamePt,
                        COALESCE(NULLIF(NameJp, ''), NamePt, '') AS NameJp
                    FROM Shifts
                    WHERE Id = @id
                    LIMIT 1;",
                new
                {
                    id = filter.ShiftId
                }
            ) ?? throw new InvalidOperationException("Turno nao encontrado para o monitor de producao.");

            var period = GetShiftPeriod(filter.Date, shift.NamePt);

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

            var dashboard = new ProductionDashboardDto
            {
                Period = period
            };

            if (machines.Count == 0)
            {
                return dashboard;
            }

            var machineIds = machines.Select(machine => machine.Id).ToArray();
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
                    ORDER BY datetime(EventDateTime), Id;",
                new
                {
                    machineIds,
                    rangeStart = period.Start.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                    rangeEnd = period.End.ToString("yyyy-MM-dd HH:mm:ss")
                }
            ).ToList();

            var scheduleRows = conn.Query<ScheduleRow>(
                @"
                    SELECT
                        sc.LocalId,
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

            var operatorsByLocal = scheduleRows
                .GroupBy(row => row.LocalId)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList());

            foreach (var machine in machines)
            {
                var machineEvents = events
                    .Where(row => row.MachineId == machine.Id)
                    .OrderBy(row => row.EventDateTime)
                    .ToList();

                var metrics = CalculateMetrics(machineEvents, period);
                var currentStatus = statusRows.TryGetValue(machine.Id, out var status)
                    ? status
                    : machineEvents.LastOrDefault();

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
                    summary.ScheduledOperatorsPt.AddRange(assignedOperators.Select(item => item.OperatorNamePt));
                    summary.ScheduledOperatorsJp.AddRange(assignedOperators.Select(item => item.OperatorNameJp));
                }

                dashboard.Machines.Add(summary);
            }

            var totalMinutes = dashboard.Machines.Sum(machine => machine.TotalMinutes);
            var runningMinutes = dashboard.Machines.Sum(machine => machine.RunningMinutes);

            dashboard.ProductionPercent = totalMinutes <= 0
                ? 0
                : Math.Round((runningMinutes / totalMinutes) * 100d, 1);
            dashboard.MachinesRunning = dashboard.Machines.Count(machine => machine.StatusCode == 0);
            dashboard.MachinesStopped = dashboard.Machines.Count(machine => machine.StatusCode == 1);
            dashboard.ErrorMinutes = Math.Round(dashboard.Machines.Sum(machine => machine.ErrorMinutes), 1);
            dashboard.InactiveMinutes = Math.Round(dashboard.Machines.Sum(machine => machine.InactiveMinutes), 1);

            dashboard.Ranking.AddRange(
                dashboard.Machines
                    .Select(machine => new ProductionRankingItemDto
                    {
                        MachineCode = machine.MachineCode,
                        MachineNamePt = machine.MachineNamePt,
                        MachineNameJp = machine.MachineNameJp,
                        StopMinutes = Math.Round(machine.StoppedMinutes, 1),
                        ErrorMinutes = Math.Round(machine.ErrorMinutes, 1),
                        TotalImpactMinutes = Math.Round(machine.StoppedMinutes + machine.ErrorMinutes, 1)
                    })
                    .OrderByDescending(item => item.TotalImpactMinutes)
                    .ThenBy(item => item.MachineCode, StringComparer.OrdinalIgnoreCase)
                    .Take(10)
            );

            var cellMoments = BuildTimelineMoments(period);
            foreach (var machine in dashboard.Machines)
            {
                var machineEvents = events
                    .Where(row => row.MachineId == machine.MachineId)
                    .OrderBy(row => row.EventDateTime)
                    .ToList();

                var timelineRow = new ProductionTimelineRowDto
                {
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

            return dashboard;
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
            return (statusCode, locale) switch
            {
                (0, "ja-JP") => "稼動中",
                (1, "ja-JP") => "停止",
                (2, "ja-JP") => "非稼動",
                (3, "ja-JP") => "異常",
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
            public int LocalId { get; set; }
            public string OperatorNamePt { get; set; } = string.Empty;
            public string OperatorNameJp { get; set; } = string.Empty;
        }

        private readonly record struct ProductionMetrics(
            double RunningMinutes,
            double StoppedMinutes,
            double InactiveMinutes,
            double ErrorMinutes,
            double TotalMinutes);
    }
}
