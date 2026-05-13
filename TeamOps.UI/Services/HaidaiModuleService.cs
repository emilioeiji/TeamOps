using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Dapper;
using TeamOps.Data.Db;

namespace TeamOps.UI.Services
{
    public sealed class HaidaiModuleService
    {
        private readonly SqliteConnectionFactory _factory;

        public HaidaiModuleService(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public void EnsureSchema()
        {
            using var conn = _factory.CreateOpenConnection();

            conn.Execute(
                @"
                    CREATE TABLE IF NOT EXISTS HaidaiAssignments (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ScheduleDate TEXT NOT NULL,
                        ShiftId INTEGER NOT NULL,
                        SectorId INTEGER NOT NULL,
                        OperatorCodigoFJ TEXT NOT NULL,
                        LocalId INTEGER,
                        AssignmentCode TEXT,
                        PairKey TEXT,
                        IsTrainee INTEGER NOT NULL DEFAULT 0,
                        TrainerCodigoFJ TEXT,
                        CountsTowardKousu INTEGER NOT NULL DEFAULT 1,
                        IsLineupActive INTEGER NOT NULL DEFAULT 1,
                        AvailabilityStatus TEXT,
                        Notes TEXT,
                        UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (ShiftId) REFERENCES Shifts(Id),
                        FOREIGN KEY (SectorId) REFERENCES Sectors(Id),
                        FOREIGN KEY (OperatorCodigoFJ) REFERENCES Operators(CodigoFJ),
                        FOREIGN KEY (LocalId) REFERENCES Locals(Id),
                        FOREIGN KEY (TrainerCodigoFJ) REFERENCES Operators(CodigoFJ)
                    );

                    CREATE UNIQUE INDEX IF NOT EXISTS IX_HaidaiAssignments_DateShiftSectorOperator
                    ON HaidaiAssignments(ScheduleDate, ShiftId, SectorId, OperatorCodigoFJ);

                    CREATE INDEX IF NOT EXISTS IX_HaidaiAssignments_DateShiftSector
                    ON HaidaiAssignments(ScheduleDate, ShiftId, SectorId);");

            EnsureColumn(conn, "Locals", "ShortCode", "TEXT");
            EnsureColumn(conn, "HaidaiAssignments", "IsLineupActive", "INTEGER NOT NULL DEFAULT 1");
            EnsureColumn(conn, "HaidaiAssignments", "AvailabilityStatus", "TEXT");

            conn.Execute(
                @"
                    CREATE TABLE IF NOT EXISTS HaidaiMovements (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ScheduleDate TEXT NOT NULL,
                        ShiftId INTEGER NOT NULL,
                        SectorId INTEGER NOT NULL,
                        OperatorCodigoFJ TEXT NOT NULL,
                        MovementType TEXT NOT NULL,
                        EventTime TEXT,
                        LocalId INTEGER,
                        AssignmentCode TEXT,
                        PairKey TEXT,
                        ReplacementOperatorCodigoFJ TEXT,
                        Reason TEXT,
                        CreatedByCodigoFJ TEXT NOT NULL,
                        CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (ShiftId) REFERENCES Shifts(Id),
                        FOREIGN KEY (SectorId) REFERENCES Sectors(Id),
                        FOREIGN KEY (OperatorCodigoFJ) REFERENCES Operators(CodigoFJ),
                        FOREIGN KEY (LocalId) REFERENCES Locals(Id),
                        FOREIGN KEY (ReplacementOperatorCodigoFJ) REFERENCES Operators(CodigoFJ),
                        FOREIGN KEY (CreatedByCodigoFJ) REFERENCES Operators(CodigoFJ)
                    );

                    CREATE INDEX IF NOT EXISTS IX_HaidaiMovements_DateShiftSectorOperator
                    ON HaidaiMovements(ScheduleDate, ShiftId, SectorId, OperatorCodigoFJ);");
        }

        public HaidaiInitPayload GetInitialPayload(int defaultShiftId, int defaultSectorId)
        {
            EnsureSchema();

            using var conn = _factory.CreateOpenConnection();

            var shifts = conn.Query<HaidaiLookupItem>(
                @"
                    SELECT
                        Id,
                        COALESCE(NULLIF(NamePt, ''), NULLIF(NameJp, ''), 'Turno ' || Id) AS Name
                    FROM Shifts
                    ORDER BY Id;")
                .ToList();

            var sectors = conn.Query<HaidaiLookupItem>(
                @"
                    SELECT
                        Id,
                        COALESCE(NULLIF(NamePt, ''), NULLIF(NameJp, ''), 'Setor ' || Id) AS Name
                    FROM Sectors
                    ORDER BY Id;")
                .ToList();

            var locals = conn.Query<HaidaiLocalLookup>(
                @"
                    SELECT
                        Id,
                        SectorId,
                        COALESCE(NULLIF(NamePt, ''), NULLIF(NameJp, ''), 'Local ' || Id) AS Name,
                        COALESCE(NULLIF(ShortCode, ''), NULLIF(NamePt, ''), NULLIF(NameJp, ''), 'L' || Id) AS ShortCode
                    FROM Locals
                    ORDER BY SectorId, Id;")
                .ToList();

            var operators = conn.Query<HaidaiOperatorLookup>(
                @"
                    SELECT
                        o.CodigoFJ,
                        COALESCE(NULLIF(o.NameRomanji, ''), o.CodigoFJ) AS Name,
                        o.SectorId,
                        o.ShiftId,
                        o.GroupId,
                        COALESCE(NULLIF(g.NamePt, ''), NULLIF(g.NameJp, ''), 'Grupo ' || o.GroupId) AS GroupName,
                        COALESCE(o.Trainer, 0) AS Trainer,
                        COALESCE(o.IsLeader, 0) AS IsLeader
                    FROM Operators o
                    LEFT JOIN Groups g ON g.Id = o.GroupId
                    WHERE COALESCE(o.Status, 1) = 1
                    ORDER BY o.GroupId, o.NameRomanji, o.CodigoFJ;")
                .ToList();

            var resolvedShiftId = shifts.Any(item => item.Id == defaultShiftId)
                ? defaultShiftId
                : shifts.FirstOrDefault()?.Id ?? 1;

            var resolvedSectorId = sectors.Any(item => item.Id == defaultSectorId)
                ? defaultSectorId
                : sectors.FirstOrDefault()?.Id ?? 1;

            return new HaidaiInitPayload(
                DateTime.Today.ToString("yyyy-MM-dd"),
                resolvedShiftId,
                resolvedSectorId,
                shifts,
                sectors,
                locals,
                operators);
        }

        public HaidaiMonthlyPlanPayload GetMonthlyPlan(int year, int month, int shiftId, int sectorId)
        {
            EnsureSchema();

            using var conn = _factory.CreateOpenConnection();

            var firstDay = new DateTime(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            var dayNumbers = Enumerable.Range(1, lastDay.Day).ToList();

            var operators = conn.Query<HaidaiOperatorRow>(
                @"
                    SELECT
                        o.CodigoFJ,
                        COALESCE(NULLIF(o.NameRomanji, ''), o.CodigoFJ) AS Name,
                        COALESCE(NULLIF(o.NameNihongo, ''), COALESCE(NULLIF(o.NameRomanji, ''), o.CodigoFJ)) AS NameJp,
                        o.GroupId,
                        COALESCE(NULLIF(g.NamePt, ''), NULLIF(g.NameJp, ''), 'Grupo ' || o.GroupId) AS GroupName,
                        COALESCE(o.Trainer, 0) AS Trainer,
                        COALESCE(o.IsLeader, 0) AS IsLeader
                    FROM Operators o
                    LEFT JOIN Groups g ON g.Id = o.GroupId
                    WHERE COALESCE(o.Status, 1) = 1
                      AND o.SectorId = @SectorId
                      AND o.ShiftId = @ShiftId
                    ORDER BY o.GroupId,
                             CASE WHEN COALESCE(o.IsLeader, 0) = 1 THEN 1 ELSE 0 END,
                             o.NameRomanji,
                             o.CodigoFJ;",
                new
                {
                    SectorId = sectorId,
                    ShiftId = shiftId
                })
                .ToList();

            var assignments = conn.Query<HaidaiAssignmentRecord>(
                @"
                    SELECT
                        Id,
                        ScheduleDate,
                        ShiftId,
                        SectorId,
                        OperatorCodigoFJ,
                        LocalId,
                        COALESCE(AssignmentCode, '') AS AssignmentCode,
                        COALESCE(PairKey, '') AS PairKey,
                        COALESCE(IsTrainee, 0) AS IsTrainee,
                        COALESCE(TrainerCodigoFJ, '') AS TrainerCodigoFJ,
                        COALESCE(CountsTowardKousu, 1) AS CountsTowardKousu,
                        COALESCE(IsLineupActive, 1) AS IsLineupActive,
                        COALESCE(AvailabilityStatus, '') AS AvailabilityStatus,
                        COALESCE(Notes, '') AS Notes
                    FROM HaidaiAssignments
                    WHERE ShiftId = @ShiftId
                      AND SectorId = @SectorId
                      AND date(ScheduleDate) BETWEEN date(@StartDate) AND date(@EndDate);",
                new
                {
                    ShiftId = shiftId,
                    SectorId = sectorId,
                    StartDate = firstDay.ToString("yyyy-MM-dd"),
                    EndDate = lastDay.ToString("yyyy-MM-dd")
                })
                .GroupBy(item => item.OperatorCodigoFJ, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToDictionary(
                        item => DateTime.Parse(item.ScheduleDate, CultureInfo.InvariantCulture).Day,
                        item => item,
                        EqualityComparer<int>.Default),
                    StringComparer.OrdinalIgnoreCase);

            var exceptions = conn.Query<HaidaiMonthlyExceptionRecord>(
                @"
                    SELECT
                        a.OperatorCodigoFJ,
                        date(a.RequestDate) AS RequestDate,
                        a.TodokeMotivoId AS MotiveId
                    FROM AcompYukyu a
                    INNER JOIN (
                        SELECT
                            OperatorCodigoFJ,
                            date(RequestDate) AS RequestDay,
                            MAX(Id) AS MaxId
                        FROM AcompYukyu
                        WHERE date(RequestDate) BETWEEN date(@StartDate) AND date(@EndDate)
                          AND TodokeMotivoId IN (1, 2)
                        GROUP BY OperatorCodigoFJ, date(RequestDate)
                    ) latest ON latest.MaxId = a.Id;",
                new
                {
                    StartDate = firstDay.ToString("yyyy-MM-dd"),
                    EndDate = lastDay.ToString("yyyy-MM-dd")
                })
                .GroupBy(item => item.OperatorCodigoFJ, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToDictionary(
                        item => DateTime.Parse(item.RequestDate, CultureInfo.InvariantCulture).Day,
                        item => item,
                        EqualityComparer<int>.Default),
                    StringComparer.OrdinalIgnoreCase);

            var groups = operators
                .GroupBy(item => new { item.GroupId, item.GroupName })
                .OrderBy(group => group.Key.GroupId)
                .Select(group => new HaidaiMonthlyGroupPlan(
                    group.Key.GroupId,
                    group.Key.GroupName,
                    group.Select(op =>
                    {
                        assignments.TryGetValue(op.CodigoFJ, out var days);
                        var cells = dayNumbers
                            .Select(day =>
                            {
                                var record = days != null && days.TryGetValue(day, out var found)
                                    ? found
                                    : null;
                                var exception = exceptions.TryGetValue(op.CodigoFJ, out var exceptionDays)
                                    && exceptionDays.TryGetValue(day, out var foundException)
                                        ? foundException
                                        : null;
                                var plannerCode = ResolveMonthlyPlannerCode(record);
                                var status = ResolveMonthlyStatus(exception, record, plannerCode);

                                return new HaidaiMonthlyCell(
                                    day,
                                    ResolveDisplayAssignmentCode(exception?.MotiveId ?? 0, plannerCode),
                                    record?.LocalId,
                                    record?.IsTrainee ?? false,
                                    record?.IsLineupActive ?? false,
                                    status);
                            })
                            .ToList();

                        return new HaidaiMonthlyOperatorPlan(
                            op.CodigoFJ,
                            op.Name,
                            op.NameJp,
                            op.GroupId,
                            op.GroupName,
                            cells);
                    }).ToList()))
                .ToList();

            return new HaidaiMonthlyPlanPayload(
                year,
                month,
                shiftId,
                sectorId,
                dayNumbers,
                groups);
        }

        public void SaveMonthlyPlan(HaidaiMonthlySaveRequest request)
        {
            EnsureSchema();

            using var conn = _factory.CreateOpenConnection();
            using var transaction = conn.BeginTransaction();

            var firstDay = new DateTime(request.Year, request.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var locals = conn.Query<HaidaiLocalLookup>(
                @"
                    SELECT
                        Id,
                        SectorId,
                        COALESCE(NULLIF(NamePt, ''), NULLIF(NameJp, ''), 'Local ' || Id) AS Name,
                        COALESCE(NULLIF(ShortCode, ''), NULLIF(NamePt, ''), NULLIF(NameJp, ''), 'L' || Id) AS ShortCode
                    FROM Locals
                    WHERE SectorId = @SectorId;",
                new { request.SectorId },
                transaction)
                .ToList();

            var localsByCode = locals
                .GroupBy(item => item.ShortCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key.Trim(), group => group.First(), StringComparer.OrdinalIgnoreCase);

            var existingAssignments = conn.Query<HaidaiAssignmentRecord>(
                @"
                    SELECT
                        Id,
                        ScheduleDate,
                        ShiftId,
                        SectorId,
                        OperatorCodigoFJ,
                        LocalId,
                        COALESCE(AssignmentCode, '') AS AssignmentCode,
                        COALESCE(PairKey, '') AS PairKey,
                        COALESCE(IsTrainee, 0) AS IsTrainee,
                        COALESCE(TrainerCodigoFJ, '') AS TrainerCodigoFJ,
                        COALESCE(CountsTowardKousu, 1) AS CountsTowardKousu,
                        COALESCE(IsLineupActive, 1) AS IsLineupActive,
                        COALESCE(AvailabilityStatus, '') AS AvailabilityStatus,
                        COALESCE(Notes, '') AS Notes
                    FROM HaidaiAssignments
                    WHERE ShiftId = @ShiftId
                      AND SectorId = @SectorId
                      AND date(ScheduleDate) BETWEEN date(@StartDate) AND date(@EndDate);",
                new
                {
                    request.ShiftId,
                    request.SectorId,
                    StartDate = firstDay.ToString("yyyy-MM-dd"),
                    EndDate = lastDay.ToString("yyyy-MM-dd")
                },
                transaction)
                .ToList();

            var existingMap = existingAssignments.ToDictionary(
                item => $"{item.OperatorCodigoFJ}|{item.ScheduleDate}",
                item => item,
                StringComparer.OrdinalIgnoreCase);

            var exceptionMap = conn.Query<HaidaiMonthlyExceptionRecord>(
                @"
                    SELECT
                        a.OperatorCodigoFJ,
                        date(a.RequestDate) AS RequestDate,
                        a.TodokeMotivoId AS MotiveId
                    FROM AcompYukyu a
                    INNER JOIN (
                        SELECT
                            OperatorCodigoFJ,
                            date(RequestDate) AS RequestDay,
                            MAX(Id) AS MaxId
                        FROM AcompYukyu
                        WHERE date(RequestDate) BETWEEN date(@StartDate) AND date(@EndDate)
                          AND TodokeMotivoId IN (1, 2)
                        GROUP BY OperatorCodigoFJ, date(RequestDate)
                    ) latest ON latest.MaxId = a.Id;",
                new
                {
                    StartDate = firstDay.ToString("yyyy-MM-dd"),
                    EndDate = lastDay.ToString("yyyy-MM-dd")
                },
                transaction)
                .ToDictionary(
                    item => $"{item.OperatorCodigoFJ}|{item.RequestDate}",
                    item => item,
                    StringComparer.OrdinalIgnoreCase);

            foreach (var cell in request.Cells)
            {
                if (cell.Day < 1 || cell.Day > lastDay.Day)
                {
                    continue;
                }

                var date = new DateTime(request.Year, request.Month, cell.Day);
                var dateIso = date.ToString("yyyy-MM-dd");
                var key = $"{cell.OperatorCodigoFJ}|{dateIso}";
                existingMap.TryGetValue(key, out var existing);
                exceptionMap.TryGetValue(key, out var exception);

                var resolved = ResolveMonthlyCell(cell.AssignmentCode, localsByCode, existing, exception);

                UpsertMonthlyAssignmentInternal(
                    conn,
                    transaction,
                    date,
                    request.ShiftId,
                    request.SectorId,
                    cell.OperatorCodigoFJ,
                    resolved,
                    existing);
            }

            transaction.Commit();
        }

        private static HaidaiResolvedMonthlyCell ResolveMonthlyCell(
            string? rawAssignmentCode,
            IReadOnlyDictionary<string, HaidaiLocalLookup> localsByCode,
            HaidaiAssignmentRecord? existing,
            HaidaiMonthlyExceptionRecord? exception)
        {
            var normalized = (rawAssignmentCode ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return new HaidaiResolvedMonthlyCell(
                    null,
                    string.Empty,
                    false,
                    false,
                    exception == null ? string.Empty : ResolveExceptionStatus(exception.MotiveId));
            }

            if (IsOffDayCode(normalized))
            {
                return new HaidaiResolvedMonthlyCell(null, "休", false, false, "Folga");
            }

            if (IsDisplayExceptionCode(normalized) && existing != null)
            {
                return new HaidaiResolvedMonthlyCell(
                    existing.LocalId,
                    existing.AssignmentCode,
                    existing.IsTrainee,
                    exception == null ? existing.IsLineupActive : false,
                    exception == null ? existing.AvailabilityStatus : ResolveExceptionStatus(exception.MotiveId));
            }

            var isTrainee = normalized.EndsWith("#", StringComparison.Ordinal);
            var localCode = isTrainee
                ? normalized[..^1].Trim()
                : normalized;

            int? localId = null;
            if (!string.IsNullOrWhiteSpace(localCode) &&
                localsByCode.TryGetValue(localCode, out var local))
            {
                localId = local.Id;
            }
            else
            {
                localId = existing?.LocalId;
            }

            return new HaidaiResolvedMonthlyCell(
                localId,
                normalized,
                isTrainee,
                exception == null,
                exception == null ? "Escalado" : ResolveExceptionStatus(exception.MotiveId));
        }

        private static string ResolveMonthlyPlannerCode(HaidaiAssignmentRecord? record)
        {
            if (record == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(record.AssignmentCode))
            {
                return record.AssignmentCode.Trim();
            }

            return string.Equals(record.AvailabilityStatus, "Folga", StringComparison.OrdinalIgnoreCase)
                ? "休"
                : string.Empty;
        }

        private static bool IsDisplayExceptionCode(string value)
        {
            return string.Equals(value, "欠", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "有", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsOffDayCode(string value)
        {
            return string.Equals(value, "休", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "folga", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "off", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "-", StringComparison.OrdinalIgnoreCase);
        }

        public HaidaiBoardPayload GetBoard(DateTime date, int shiftId, int sectorId)
        {
            EnsureSchema();

            using var conn = _factory.CreateOpenConnection();

            var locals = conn.Query<HaidaiLocalLookup>(
                @"
                    SELECT
                        Id,
                        SectorId,
                        COALESCE(NULLIF(NamePt, ''), NULLIF(NameJp, ''), 'Local ' || Id) AS Name,
                        COALESCE(NULLIF(ShortCode, ''), NULLIF(NamePt, ''), NULLIF(NameJp, ''), 'L' || Id) AS ShortCode
                    FROM Locals
                    WHERE SectorId = @SectorId
                    ORDER BY Id;",
                new { SectorId = sectorId })
                .ToList();

            var localsById = locals.ToDictionary(item => item.Id);

            var operators = conn.Query<HaidaiOperatorRow>(
                @"
                    SELECT
                        o.CodigoFJ,
                        COALESCE(NULLIF(o.NameRomanji, ''), o.CodigoFJ) AS Name,
                        COALESCE(NULLIF(o.NameNihongo, ''), COALESCE(NULLIF(o.NameRomanji, ''), o.CodigoFJ)) AS NameJp,
                        o.GroupId,
                        COALESCE(NULLIF(g.NamePt, ''), NULLIF(g.NameJp, ''), 'Grupo ' || o.GroupId) AS GroupName,
                        COALESCE(o.Trainer, 0) AS Trainer,
                        COALESCE(o.IsLeader, 0) AS IsLeader
                    FROM Operators o
                    LEFT JOIN Groups g ON g.Id = o.GroupId
                    WHERE COALESCE(o.Status, 1) = 1
                      AND o.SectorId = @SectorId
                      AND o.ShiftId = @ShiftId
                    ORDER BY o.GroupId,
                             CASE WHEN COALESCE(o.IsLeader, 0) = 1 THEN 1 ELSE 0 END,
                             o.NameRomanji,
                             o.CodigoFJ;",
                new
                {
                    SectorId = sectorId,
                    ShiftId = shiftId
                })
                .ToList();

            var assignments = conn.Query<HaidaiAssignmentRecord>(
                @"
                    SELECT
                        Id,
                        ScheduleDate,
                        ShiftId,
                        SectorId,
                        OperatorCodigoFJ,
                        LocalId,
                        COALESCE(AssignmentCode, '') AS AssignmentCode,
                        COALESCE(PairKey, '') AS PairKey,
                        COALESCE(IsTrainee, 0) AS IsTrainee,
                        COALESCE(TrainerCodigoFJ, '') AS TrainerCodigoFJ,
                        COALESCE(CountsTowardKousu, 1) AS CountsTowardKousu,
                        COALESCE(IsLineupActive, 1) AS IsLineupActive,
                        COALESCE(AvailabilityStatus, '') AS AvailabilityStatus,
                        COALESCE(Notes, '') AS Notes
                    FROM HaidaiAssignments
                    WHERE date(ScheduleDate) = date(@ScheduleDate)
                      AND ShiftId = @ShiftId
                      AND SectorId = @SectorId;",
                new
                {
                    ScheduleDate = date.ToString("yyyy-MM-dd"),
                    ShiftId = shiftId,
                    SectorId = sectorId
                })
                .ToDictionary(item => item.OperatorCodigoFJ, StringComparer.OrdinalIgnoreCase);

            var movementMap = conn.Query<HaidaiMovementRecord>(
                @"
                    SELECT
                        Id,
                        OperatorCodigoFJ,
                        COALESCE(MovementType, '') AS MovementType,
                        COALESCE(EventTime, '') AS EventTime,
                        COALESCE(AssignmentCode, '') AS AssignmentCode,
                        COALESCE(PairKey, '') AS PairKey,
                        COALESCE(ReplacementOperatorCodigoFJ, '') AS ReplacementOperatorCodigoFJ,
                        COALESCE(Reason, '') AS Reason,
                        COALESCE(CreatedAt, '') AS CreatedAt
                    FROM HaidaiMovements
                    WHERE date(ScheduleDate) = date(@ScheduleDate)
                      AND ShiftId = @ShiftId
                      AND SectorId = @SectorId
                    ORDER BY COALESCE(EventTime, '') DESC, Id DESC;",
                new
                {
                    ScheduleDate = date.ToString("yyyy-MM-dd"),
                    ShiftId = shiftId,
                    SectorId = sectorId
                })
                .GroupBy(item => item.OperatorCodigoFJ, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

            var exceptions = conn.Query<HaidaiExceptionRecord>(
                @"
                    SELECT
                        a.OperatorCodigoFJ,
                        a.TodokeMotivoId AS MotiveId,
                        COALESCE(m.NomePt, '') AS MotiveName,
                        COALESCE(a.Notes, '') AS Notes
                    FROM AcompYukyu a
                    INNER JOIN (
                        SELECT OperatorCodigoFJ, MAX(Id) AS MaxId
                        FROM AcompYukyu
                        WHERE date(RequestDate) = date(@ScheduleDate)
                          AND TodokeMotivoId IN (1, 2)
                        GROUP BY OperatorCodigoFJ
                    ) latest ON latest.MaxId = a.Id
                    LEFT JOIN TodokeMotivo m ON m.Id = a.TodokeMotivoId;",
                new
                {
                    ScheduleDate = date.ToString("yyyy-MM-dd")
                })
                .ToDictionary(item => item.OperatorCodigoFJ, StringComparer.OrdinalIgnoreCase);

            var rows = operators
                .Select(op =>
                {
                    assignments.TryGetValue(op.CodigoFJ, out var assignment);
                    exceptions.TryGetValue(op.CodigoFJ, out var exception);
                    movementMap.TryGetValue(op.CodigoFJ, out var movements);
                    movements ??= new List<HaidaiMovementRecord>();

                    var localId = assignment?.LocalId;
                    HaidaiLocalLookup? local = null;
                    if (localId.HasValue && localsById.TryGetValue(localId.Value, out var foundLocal))
                    {
                        local = foundLocal;
                    }

                    var baseAssignmentCode = ResolveAssignmentCode(assignment, local);
                    var status = ResolveStatus(exception, assignment, baseAssignmentCode);
                    var displayAssignmentCode = ResolveDisplayAssignmentCode(exception?.MotiveId ?? 0, baseAssignmentCode);
                    var latestMovement = movements.FirstOrDefault();

                    return new HaidaiBoardRow(
                        op.CodigoFJ,
                        op.Name,
                        op.NameJp,
                        op.GroupId,
                        op.GroupName,
                        op.Trainer,
                        op.IsLeader,
                        local?.Id,
                        local?.Name ?? string.Empty,
                        local?.ShortCode ?? string.Empty,
                        displayAssignmentCode,
                        baseAssignmentCode,
                        assignment?.PairKey ?? string.Empty,
                        assignment?.IsTrainee ?? false,
                        assignment?.TrainerCodigoFJ ?? string.Empty,
                        assignment?.CountsTowardKousu ?? true,
                        assignment?.IsLineupActive ?? false,
                        assignment?.Notes ?? string.Empty,
                        exception?.MotiveId ?? 0,
                        exception?.MotiveName ?? string.Empty,
                        exception?.Notes ?? string.Empty,
                        status,
                        latestMovement == null ? string.Empty : BuildMovementSummary(latestMovement),
                        movements.Count);
                })
                .ToList();

            var grouped = rows
                .GroupBy(item => new { item.GroupId, item.GroupName })
                .OrderBy(group => group.Key.GroupId)
                .Select(group => new HaidaiGroupBlock(
                    group.Key.GroupId,
                    group.Key.GroupName,
                    group.Count(),
                    group.ToList()))
                .ToList();

            var summary = new HaidaiSummary(
                rows.Count,
                rows.Count(item => item.IsLineupActive && !string.IsNullOrWhiteSpace(item.AssignmentCode)),
                rows.Count(item => item.ExceptionMotiveId == 1),
                rows.Count(item => item.ExceptionMotiveId == 2),
                rows.Count(item => string.Equals(item.Status, "Atraso", StringComparison.OrdinalIgnoreCase)),
                rows.Count(item => string.Equals(item.Status, "Saiu cedo", StringComparison.OrdinalIgnoreCase)),
                rows.Count(item => item.IsTrainee),
                rows.Select(item => item.PairKey)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count());

            return new HaidaiBoardPayload(
                date.ToString("yyyy-MM-dd"),
                shiftId,
                sectorId,
                summary,
                grouped);
        }

        public void SaveAssignment(HaidaiSaveAssignmentRequest request)
        {
            EnsureSchema();

            using var conn = _factory.CreateOpenConnection();
            using var transaction = conn.BeginTransaction();

            string assignmentCode = request.AssignmentCode;
            if (request.LocalId.HasValue && request.LocalId.Value > 0 && string.IsNullOrWhiteSpace(assignmentCode))
            {
                assignmentCode = conn.ExecuteScalar<string?>(
                    @"
                        SELECT COALESCE(NULLIF(ShortCode, ''), NULLIF(NamePt, ''), NULLIF(NameJp, ''), '')
                        FROM Locals
                        WHERE Id = @LocalId;",
                    new { LocalId = request.LocalId.Value },
                    transaction) ?? string.Empty;
            }

            UpsertAssignmentInternal(
                conn,
                transaction,
                request with
                {
                    AssignmentCode = assignmentCode ?? string.Empty
                });

            transaction.Commit();
        }

        public void UpsertException(DateTime date, string operatorCodigoFJ, int motiveId, string notes, string authorizedByCodigoFJ)
        {
            EnsureSchema();

            using var conn = _factory.CreateOpenConnection();
            using var transaction = conn.BeginTransaction();

            var normalizedDate = date.ToString("yyyy-MM-dd");
            UpsertPendingTodokeRequest(
                conn,
                transaction,
                operatorCodigoFJ,
                normalizedDate,
                motiveId,
                notes,
                authorizedByCodigoFJ,
                replaceExceptionMotives: true);

            conn.Execute(
                @"
                    UPDATE HaidaiAssignments
                    SET IsLineupActive = 0,
                        AvailabilityStatus = @AvailabilityStatus,
                        UpdatedAt = CURRENT_TIMESTAMP
                    WHERE OperatorCodigoFJ = @OperatorCodigoFJ
                      AND ShiftId = @ShiftId
                      AND SectorId = @SectorId
                      AND date(ScheduleDate) = date(@ScheduleDate);",
                new
                {
                    AvailabilityStatus = motiveId == 1 ? "Yukyu" : "Falta",
                    OperatorCodigoFJ = operatorCodigoFJ,
                    ShiftId = conn.ExecuteScalar<int?>(
                        @"SELECT ShiftId
                          FROM HaidaiAssignments
                          WHERE OperatorCodigoFJ = @OperatorCodigoFJ
                            AND date(ScheduleDate) = date(@ScheduleDate)
                          ORDER BY Id DESC
                          LIMIT 1;",
                        new
                        {
                            OperatorCodigoFJ = operatorCodigoFJ,
                            ScheduleDate = normalizedDate
                        },
                        transaction) ?? 0,
                    SectorId = conn.ExecuteScalar<int?>(
                        @"SELECT SectorId
                          FROM HaidaiAssignments
                          WHERE OperatorCodigoFJ = @OperatorCodigoFJ
                            AND date(ScheduleDate) = date(@ScheduleDate)
                          ORDER BY Id DESC
                          LIMIT 1;",
                        new
                        {
                            OperatorCodigoFJ = operatorCodigoFJ,
                            ScheduleDate = normalizedDate
                        },
                        transaction) ?? 0,
                    ScheduleDate = normalizedDate
                },
                transaction);

            transaction.Commit();
        }

        public void ClearException(DateTime date, string operatorCodigoFJ)
        {
            EnsureSchema();

            using var conn = _factory.CreateOpenConnection();
            using var transaction = conn.BeginTransaction();

            var ids = conn.Query<long>(
                @"
                    SELECT Id
                    FROM AcompYukyu
                    WHERE OperatorCodigoFJ = @OperatorCodigoFJ
                      AND date(RequestDate) = date(@RequestDate)
                      AND TodokeMotivoId IN (1, 2);",
                new
                {
                    OperatorCodigoFJ = operatorCodigoFJ,
                    RequestDate = date.ToString("yyyy-MM-dd")
                },
                transaction)
                .ToList();

            if (ids.Count == 0)
            {
                transaction.Commit();
                return;
            }

            DeleteAcompYukyuCascade(conn, transaction, ids);
            conn.Execute(
                @"
                    UPDATE HaidaiAssignments
                    SET IsLineupActive = CASE
                            WHEN COALESCE(AssignmentCode, '') = '' AND COALESCE(LocalId, 0) = 0 THEN 0
                            ELSE 1
                        END,
                        AvailabilityStatus = CASE
                            WHEN COALESCE(AssignmentCode, '') = '' AND COALESCE(LocalId, 0) = 0 THEN NULL
                            ELSE 'Escalado'
                        END,
                        UpdatedAt = CURRENT_TIMESTAMP
                    WHERE OperatorCodigoFJ = @OperatorCodigoFJ
                      AND date(ScheduleDate) = date(@RequestDate);",
                new
                {
                    OperatorCodigoFJ = operatorCodigoFJ,
                    RequestDate = date.ToString("yyyy-MM-dd")
                },
                transaction);

            transaction.Commit();
        }

        public void RegisterMovement(HaidaiMovementRequest request)
        {
            EnsureSchema();

            using var conn = _factory.CreateOpenConnection();
            using var transaction = conn.BeginTransaction();

            var normalizedDate = request.Date.ToString("yyyy-MM-dd");
            var assignment = conn.QueryFirstOrDefault<HaidaiAssignmentRecord>(
                @"
                    SELECT
                        Id,
                        ScheduleDate,
                        ShiftId,
                        SectorId,
                        OperatorCodigoFJ,
                        LocalId,
                        COALESCE(AssignmentCode, '') AS AssignmentCode,
                        COALESCE(PairKey, '') AS PairKey,
                        COALESCE(IsTrainee, 0) AS IsTrainee,
                        COALESCE(TrainerCodigoFJ, '') AS TrainerCodigoFJ,
                        COALESCE(CountsTowardKousu, 1) AS CountsTowardKousu,
                        COALESCE(IsLineupActive, 1) AS IsLineupActive,
                        COALESCE(AvailabilityStatus, '') AS AvailabilityStatus,
                        COALESCE(Notes, '') AS Notes
                    FROM HaidaiAssignments
                    WHERE OperatorCodigoFJ = @OperatorCodigoFJ
                      AND ShiftId = @ShiftId
                      AND SectorId = @SectorId
                      AND date(ScheduleDate) = date(@ScheduleDate)
                    LIMIT 1;",
                new
                {
                    request.OperatorCodigoFJ,
                    request.ShiftId,
                    request.SectorId,
                    ScheduleDate = normalizedDate
                },
                transaction);

            var localId = assignment?.LocalId ?? request.LocalId;
            var assignmentCode = string.IsNullOrWhiteSpace(assignment?.AssignmentCode)
                ? request.AssignmentCode
                : assignment.AssignmentCode;
            var pairKey = string.IsNullOrWhiteSpace(assignment?.PairKey)
                ? request.PairKey
                : assignment.PairKey;

            conn.Execute(
                @"
                    INSERT INTO HaidaiMovements (
                        ScheduleDate,
                        ShiftId,
                        SectorId,
                        OperatorCodigoFJ,
                        MovementType,
                        EventTime,
                        LocalId,
                        AssignmentCode,
                        PairKey,
                        ReplacementOperatorCodigoFJ,
                        Reason,
                        CreatedByCodigoFJ
                    )
                    VALUES (
                        @ScheduleDate,
                        @ShiftId,
                        @SectorId,
                        @OperatorCodigoFJ,
                        @MovementType,
                        @EventTime,
                        @LocalId,
                        @AssignmentCode,
                        @PairKey,
                        @ReplacementOperatorCodigoFJ,
                        @Reason,
                        @CreatedByCodigoFJ
                    );",
                new
                {
                    ScheduleDate = normalizedDate,
                    request.ShiftId,
                    request.SectorId,
                    request.OperatorCodigoFJ,
                    request.MovementType,
                    EventTime = NullIfWhiteSpace(request.EventTime),
                    LocalId = localId,
                    AssignmentCode = NullIfWhiteSpace(assignmentCode),
                    PairKey = NullIfWhiteSpace(pairKey),
                    ReplacementOperatorCodigoFJ = NullIfWhiteSpace(request.ReplacementOperatorCodigoFJ),
                    Reason = NullIfWhiteSpace(request.Reason),
                    request.CreatedByCodigoFJ
                },
                transaction);

            var movementTodokeNotes = BuildMovementTodokeNotes(
                request.MovementType,
                request.EventTime,
                request.Reason,
                assignmentCode,
                request.ReplacementOperatorCodigoFJ);

            UpsertPendingTodokeRequest(
                conn,
                transaction,
                request.OperatorCodigoFJ,
                normalizedDate,
                ResolveMovementTodokeMotiveId(request.MovementType),
                movementTodokeNotes,
                request.CreatedByCodigoFJ);

            if (assignment != null)
            {
                conn.Execute(
                    @"
                        UPDATE HaidaiAssignments
                        SET IsLineupActive = 0,
                            AvailabilityStatus = @AvailabilityStatus,
                            UpdatedAt = CURRENT_TIMESTAMP
                        WHERE Id = @Id;",
                    new
                    {
                        Id = assignment.Id,
                        AvailabilityStatus = request.MovementType == "late" ? "Atraso" : "Saiu cedo"
                    },
                    transaction);
            }

            if (!string.IsNullOrWhiteSpace(request.ReplacementOperatorCodigoFJ))
            {
                UpsertAssignmentInternal(
                    conn,
                    transaction,
                    new HaidaiSaveAssignmentRequest(
                        request.Date,
                        request.ShiftId,
                        request.SectorId,
                        request.ReplacementOperatorCodigoFJ.Trim(),
                        localId,
                        assignmentCode ?? string.Empty,
                        pairKey ?? string.Empty,
                        false,
                        string.Empty,
                        true,
                        BuildReplacementNote(request.OperatorCodigoFJ, request.MovementType, request.EventTime, request.Reason)));
            }

            transaction.Commit();
        }

        public void RestoreLineup(DateTime date, int shiftId, int sectorId, string operatorCodigoFJ)
        {
            EnsureSchema();

            using var conn = _factory.CreateOpenConnection();
            conn.Execute(
                @"
                    UPDATE HaidaiAssignments
                    SET IsLineupActive = 1,
                        AvailabilityStatus = 'Escalado',
                        UpdatedAt = CURRENT_TIMESTAMP
                    WHERE OperatorCodigoFJ = @OperatorCodigoFJ
                      AND ShiftId = @ShiftId
                      AND SectorId = @SectorId
                      AND date(ScheduleDate) = date(@ScheduleDate);",
                new
                {
                    OperatorCodigoFJ = operatorCodigoFJ,
                    ShiftId = shiftId,
                    SectorId = sectorId,
                    ScheduleDate = date.ToString("yyyy-MM-dd")
                });
        }

        public IReadOnlyList<HaidaiPlannedAssignment> GetActiveAssignments(DateTime date, int shiftId)
        {
            EnsureSchema();

            using var conn = _factory.CreateOpenConnection();
            return conn.Query<HaidaiPlannedAssignment>(
                @"
                    SELECT
                        OperatorCodigoFJ,
                        ShiftId,
                        SectorId,
                        LocalId,
                        COALESCE(AssignmentCode, '') AS AssignmentCode,
                        COALESCE(PairKey, '') AS PairKey,
                        COALESCE(IsTrainee, 0) AS IsTrainee,
                        COALESCE(TrainerCodigoFJ, '') AS TrainerCodigoFJ,
                        COALESCE(CountsTowardKousu, 1) AS CountsTowardKousu
                    FROM HaidaiAssignments
                    WHERE date(ScheduleDate) = date(@ScheduleDate)
                      AND ShiftId = @ShiftId
                      AND COALESCE(IsLineupActive, 1) = 1
                      AND COALESCE(AssignmentCode, '') <> '';",
                new
                {
                    ScheduleDate = date.ToString("yyyy-MM-dd"),
                    ShiftId = shiftId
                })
                .ToList();
        }

        public HaidaiExportResult ExportSector(DateTime date, int sectorId)
        {
            EnsureSchema();

            var exportDirectory = ConfigurationManager.AppSettings["HaidaiExportDirectory"];
            if (string.IsNullOrWhiteSpace(exportDirectory))
            {
                throw new InvalidOperationException("HaidaiExportDirectory nao esta configurado no app.config.");
            }

            Directory.CreateDirectory(exportDirectory);

            using var conn = _factory.CreateOpenConnection();

            var sector = conn.QuerySingleOrDefault<HaidaiLookupItem>(
                @"
                    SELECT
                        Id,
                        COALESCE(NULLIF(NamePt, ''), NULLIF(NameJp, ''), 'Setor ' || Id) AS Name
                    FROM Sectors
                    WHERE Id = @Id;",
                new { Id = sectorId });

            if (sector == null)
            {
                throw new InvalidOperationException("Setor nao encontrado para exportacao.");
            }

            var shifts = conn.Query<HaidaiLookupItem>(
                @"
                    SELECT
                        Id,
                        COALESCE(NULLIF(NamePt, ''), NULLIF(NameJp, ''), 'Turno ' || Id) AS Name
                    FROM Shifts
                    ORDER BY Id;")
                .ToList();

            var generatedFiles = new List<string>();

            for (var i = 0; i < shifts.Count; i++)
            {
                var shift = shifts[i];
                var board = GetBoard(date, shift.Id, sectorId);
                var currentFile = BuildExportFileName(sector.Name, shift.Name);
                var otherFile = shifts.Count > 1
                    ? BuildExportFileName(sector.Name, shifts[(i + 1) % shifts.Count].Name)
                    : currentFile;

                var html = BuildExportHtml(
                    sectorId,
                    sector.Name,
                    shift.Name,
                    otherFile,
                    board);

                var fullPath = Path.Combine(exportDirectory, currentFile);
                File.WriteAllText(fullPath, html, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                generatedFiles.Add(fullPath);
            }

            return new HaidaiExportResult(exportDirectory, generatedFiles);
        }

        private static string ResolveAssignmentCode(HaidaiAssignmentRecord? assignment, HaidaiLocalLookup? local)
        {
            var code = string.IsNullOrWhiteSpace(assignment?.AssignmentCode)
                ? local?.ShortCode ?? string.Empty
                : assignment!.AssignmentCode.Trim();

            if (assignment?.IsTrainee == true && !string.IsNullOrWhiteSpace(code) && !code.EndsWith("#", StringComparison.Ordinal))
            {
                code += "#";
            }

            return code;
        }

        private static string ResolveDisplayAssignmentCode(int motiveId, string assignmentCode)
        {
            return motiveId switch
            {
                1 => "有",
                2 => "欠",
                _ => assignmentCode
            };
        }

        private static string ResolveStatus(HaidaiExceptionRecord? exception, HaidaiAssignmentRecord? assignment, string assignmentCode)
        {
            if (exception?.MotiveId == 1)
            {
                return "Yukyu";
            }

            if (exception?.MotiveId == 2)
            {
                return "Falta";
            }

            if (!string.IsNullOrWhiteSpace(assignment?.AvailabilityStatus))
            {
                return assignment.AvailabilityStatus.Trim();
            }

            return string.IsNullOrWhiteSpace(assignmentCode) ? "Pendente" : "Escalado";
        }

        private static string ResolveMonthlyStatus(HaidaiMonthlyExceptionRecord? exception, HaidaiAssignmentRecord? assignment, string assignmentCode)
        {
            if (exception != null)
            {
                return ResolveExceptionStatus(exception.MotiveId);
            }

            if (!string.IsNullOrWhiteSpace(assignment?.AvailabilityStatus))
            {
                return assignment.AvailabilityStatus.Trim();
            }

            return string.IsNullOrWhiteSpace(assignmentCode) ? "Pendente" : "Escalado";
        }

        private static string ResolveExceptionStatus(int motiveId)
        {
            return motiveId switch
            {
                1 => "Yukyu",
                2 => "Falta",
                _ => string.Empty
            };
        }

        private static string BuildMovementSummary(HaidaiMovementRecord movement)
        {
            var typeLabel = movement.MovementType switch
            {
                "late" => "Atraso",
                "early_leave" => "Saiu cedo",
                _ => movement.MovementType
            };

            var parts = new List<string> { typeLabel };
            if (!string.IsNullOrWhiteSpace(movement.EventTime))
            {
                parts.Add(movement.EventTime);
            }

            if (!string.IsNullOrWhiteSpace(movement.AssignmentCode))
            {
                parts.Add($"Origem {movement.AssignmentCode}");
            }

            if (!string.IsNullOrWhiteSpace(movement.ReplacementOperatorCodigoFJ))
            {
                parts.Add($"Subst. {movement.ReplacementOperatorCodigoFJ}");
            }

            if (!string.IsNullOrWhiteSpace(movement.Reason))
            {
                parts.Add(movement.Reason);
            }

            return string.Join(" | ", parts);
        }

        private static string BuildReplacementNote(string originalOperatorCodigoFJ, string movementType, string? eventTime, string? reason)
        {
            var movementLabel = movementType == "late" ? "Atraso" : "Saida antecipada";
            var parts = new List<string> { $"Substituindo {originalOperatorCodigoFJ}", movementLabel };

            if (!string.IsNullOrWhiteSpace(eventTime))
            {
                parts.Add(eventTime.Trim());
            }

            if (!string.IsNullOrWhiteSpace(reason))
            {
                parts.Add(reason.Trim());
            }

            return string.Join(" | ", parts);
        }

        private static void UpsertPendingTodokeRequest(
            System.Data.IDbConnection conn,
            System.Data.IDbTransaction transaction,
            string operatorCodigoFJ,
            string requestDate,
            int motiveId,
            string? notes,
            string authorizedByCodigoFJ,
            bool replaceExceptionMotives = false)
        {
            const string exactSql = @"
                SELECT Id
                FROM AcompYukyu
                WHERE OperatorCodigoFJ = @OperatorCodigoFJ
                  AND date(RequestDate) = date(@RequestDate)
                  AND TodokeMotivoId = @TodokeMotivoId
                ORDER BY Id DESC
                LIMIT 1;";

            const string exceptionSql = @"
                SELECT Id
                FROM AcompYukyu
                WHERE OperatorCodigoFJ = @OperatorCodigoFJ
                  AND date(RequestDate) = date(@RequestDate)
                  AND TodokeMotivoId IN (1, 2)
                ORDER BY Id DESC
                LIMIT 1;";

            var existingId = conn.ExecuteScalar<long?>(
                replaceExceptionMotives ? exceptionSql : exactSql,
                new
                {
                    OperatorCodigoFJ = operatorCodigoFJ,
                    RequestDate = requestDate,
                    TodokeMotivoId = motiveId
                },
                transaction);

            long currentId;
            if (existingId.HasValue)
            {
                conn.Execute(
                    @"
                        UPDATE AcompYukyu
                        SET TodokeMotivoId = @TodokeMotivoId,
                            Notes = @Notes,
                            AuthorizedByCodigoFJ = @AuthorizedByCodigoFJ,
                            RequestDate = @RequestDate
                        WHERE Id = @Id;",
                    new
                    {
                        Id = existingId.Value,
                        TodokeMotivoId = motiveId,
                        Notes = NullIfWhiteSpace(notes),
                        AuthorizedByCodigoFJ = authorizedByCodigoFJ,
                        RequestDate = requestDate
                    },
                    transaction);

                currentId = existingId.Value;
            }
            else
            {
                currentId = conn.ExecuteScalar<long>(
                    @"
                        INSERT INTO AcompYukyu (
                            OperatorCodigoFJ,
                            RequestDate,
                            AuthorizedByCodigoFJ,
                            Notes,
                            TodokeMotivoId
                        )
                        VALUES (
                            @OperatorCodigoFJ,
                            @RequestDate,
                            @AuthorizedByCodigoFJ,
                            @Notes,
                            @TodokeMotivoId
                        );
                        SELECT last_insert_rowid();",
                    new
                    {
                        OperatorCodigoFJ = operatorCodigoFJ,
                        RequestDate = requestDate,
                        AuthorizedByCodigoFJ = authorizedByCodigoFJ,
                        Notes = NullIfWhiteSpace(notes),
                        TodokeMotivoId = motiveId
                    },
                    transaction);
            }

            var duplicateIds = conn.Query<long>(
                @"
                    SELECT Id
                    FROM AcompYukyu
                    WHERE OperatorCodigoFJ = @OperatorCodigoFJ
                      AND date(RequestDate) = date(@RequestDate)
                      AND Id <> @CurrentId
                      AND (
                            (@ReplaceExceptionMotives = 1 AND TodokeMotivoId IN (1, 2))
                         OR (@ReplaceExceptionMotives = 0 AND TodokeMotivoId = @TodokeMotivoId)
                      );",
                new
                {
                    OperatorCodigoFJ = operatorCodigoFJ,
                    RequestDate = requestDate,
                    CurrentId = currentId,
                    ReplaceExceptionMotives = replaceExceptionMotives ? 1 : 0,
                    TodokeMotivoId = motiveId
                },
                transaction);

            DeleteAcompYukyuCascade(conn, transaction, duplicateIds);
        }

        private static void DeleteAcompYukyuCascade(
            System.Data.IDbConnection conn,
            System.Data.IDbTransaction transaction,
            IEnumerable<long> ids)
        {
            var idList = ids
                .Distinct()
                .ToList();

            if (idList.Count == 0)
            {
                return;
            }

            conn.Execute("DELETE FROM YukyuTodoke WHERE AcompYukyuId IN @Ids;", new { Ids = idList }, transaction);
            conn.Execute("DELETE FROM YukyuFolhaControle WHERE AcompYukyuId IN @Ids;", new { Ids = idList }, transaction);
            conn.Execute("DELETE FROM YukyuConferencia WHERE AcompYukyuId IN @Ids;", new { Ids = idList }, transaction);
            conn.Execute("DELETE FROM AcompYukyu WHERE Id IN @Ids;", new { Ids = idList }, transaction);
        }

        private static int ResolveMovementTodokeMotiveId(string movementType)
        {
            return string.Equals(movementType, "late", StringComparison.OrdinalIgnoreCase) ? 3 : 5;
        }

        private static string BuildMovementTodokeNotes(
            string movementType,
            string? eventTime,
            string? reason,
            string? assignmentCode,
            string? replacementOperatorCodigoFJ)
        {
            var label = string.Equals(movementType, "late", StringComparison.OrdinalIgnoreCase)
                ? "Atraso"
                : "Saida antecipada";

            var parts = new List<string> { label };
            if (!string.IsNullOrWhiteSpace(eventTime))
            {
                parts.Add($"Horario {eventTime.Trim()}");
            }

            if (!string.IsNullOrWhiteSpace(assignmentCode))
            {
                parts.Add($"Area {assignmentCode.Trim()}");
            }

            if (!string.IsNullOrWhiteSpace(replacementOperatorCodigoFJ))
            {
                parts.Add($"Substituto {replacementOperatorCodigoFJ.Trim()}");
            }

            if (!string.IsNullOrWhiteSpace(reason))
            {
                parts.Add(reason.Trim());
            }

            return string.Join(" | ", parts);
        }

        private static void UpsertMonthlyAssignmentInternal(
            System.Data.IDbConnection conn,
            System.Data.IDbTransaction transaction,
            DateTime date,
            int shiftId,
            int sectorId,
            string operatorCodigoFJ,
            HaidaiResolvedMonthlyCell resolved,
            HaidaiAssignmentRecord? existing)
        {
            var countsTowardKousu = resolved.IsTrainee
                ? false
                : existing?.CountsTowardKousu ?? true;

            var payload = new
            {
                ScheduleDate = date.ToString("yyyy-MM-dd"),
                ShiftId = shiftId,
                SectorId = sectorId,
                OperatorCodigoFJ = operatorCodigoFJ,
                resolved.LocalId,
                resolved.AssignmentCode,
                PairKey = NullIfWhiteSpace(existing?.PairKey),
                IsTrainee = resolved.IsTrainee ? 1 : 0,
                TrainerCodigoFJ = NullIfWhiteSpace(existing?.TrainerCodigoFJ),
                CountsTowardKousu = countsTowardKousu ? 1 : 0,
                IsLineupActive = resolved.IsLineupActive ? 1 : 0,
                AvailabilityStatus = NullIfWhiteSpace(resolved.AvailabilityStatus),
                Notes = NullIfWhiteSpace(existing?.Notes)
            };

            if (existing != null)
            {
                conn.Execute(
                    @"
                        UPDATE HaidaiAssignments
                        SET LocalId = @LocalId,
                            AssignmentCode = @AssignmentCode,
                            PairKey = @PairKey,
                            IsTrainee = @IsTrainee,
                            TrainerCodigoFJ = @TrainerCodigoFJ,
                            CountsTowardKousu = @CountsTowardKousu,
                            IsLineupActive = @IsLineupActive,
                            AvailabilityStatus = @AvailabilityStatus,
                            Notes = @Notes,
                            UpdatedAt = CURRENT_TIMESTAMP
                        WHERE Id = @Id;",
                    new
                    {
                        existing.Id,
                        payload.LocalId,
                        payload.AssignmentCode,
                        payload.PairKey,
                        payload.IsTrainee,
                        payload.TrainerCodigoFJ,
                        payload.CountsTowardKousu,
                        payload.IsLineupActive,
                        payload.AvailabilityStatus,
                        payload.Notes
                    },
                    transaction);

                return;
            }

            conn.Execute(
                @"
                    INSERT INTO HaidaiAssignments (
                        ScheduleDate,
                        ShiftId,
                        SectorId,
                        OperatorCodigoFJ,
                        LocalId,
                        AssignmentCode,
                        PairKey,
                        IsTrainee,
                        TrainerCodigoFJ,
                        CountsTowardKousu,
                        IsLineupActive,
                        AvailabilityStatus,
                        Notes
                    )
                    VALUES (
                        @ScheduleDate,
                        @ShiftId,
                        @SectorId,
                        @OperatorCodigoFJ,
                        @LocalId,
                        @AssignmentCode,
                        @PairKey,
                        @IsTrainee,
                        @TrainerCodigoFJ,
                        @CountsTowardKousu,
                        @IsLineupActive,
                        @AvailabilityStatus,
                        @Notes
                    );",
                payload,
                transaction);
        }

        private static void UpsertAssignmentInternal(System.Data.IDbConnection conn, System.Data.IDbTransaction transaction, HaidaiSaveAssignmentRequest request)
        {
            var normalizedDate = request.Date.ToString("yyyy-MM-dd");
            var assignmentCode = (request.AssignmentCode ?? string.Empty).Trim();

            var existingId = conn.ExecuteScalar<long?>(
                @"
                    SELECT Id
                    FROM HaidaiAssignments
                    WHERE date(ScheduleDate) = date(@ScheduleDate)
                      AND ShiftId = @ShiftId
                      AND SectorId = @SectorId
                      AND OperatorCodigoFJ = @OperatorCodigoFJ
                    LIMIT 1;",
                new
                {
                    ScheduleDate = normalizedDate,
                    request.ShiftId,
                    request.SectorId,
                    request.OperatorCodigoFJ
                },
                transaction);

            if (existingId.HasValue)
            {
                conn.Execute(
                    @"
                        UPDATE HaidaiAssignments
                        SET LocalId = @LocalId,
                            AssignmentCode = @AssignmentCode,
                            PairKey = @PairKey,
                            IsTrainee = @IsTrainee,
                            TrainerCodigoFJ = @TrainerCodigoFJ,
                            CountsTowardKousu = @CountsTowardKousu,
                            IsLineupActive = 1,
                            AvailabilityStatus = 'Escalado',
                            Notes = @Notes,
                            UpdatedAt = CURRENT_TIMESTAMP
                        WHERE Id = @Id;",
                    new
                    {
                        Id = existingId.Value,
                        request.LocalId,
                        AssignmentCode = assignmentCode,
                        PairKey = NullIfWhiteSpace(request.PairKey),
                        IsTrainee = request.IsTrainee ? 1 : 0,
                        TrainerCodigoFJ = NullIfWhiteSpace(request.TrainerCodigoFJ),
                        CountsTowardKousu = request.CountsTowardKousu ? 1 : 0,
                        Notes = NullIfWhiteSpace(request.Notes)
                    },
                    transaction);
            }
            else
            {
                conn.Execute(
                    @"
                        INSERT INTO HaidaiAssignments (
                            ScheduleDate,
                            ShiftId,
                            SectorId,
                            OperatorCodigoFJ,
                            LocalId,
                            AssignmentCode,
                            PairKey,
                            IsTrainee,
                            TrainerCodigoFJ,
                            CountsTowardKousu,
                            IsLineupActive,
                            AvailabilityStatus,
                            Notes
                        )
                        VALUES (
                            @ScheduleDate,
                            @ShiftId,
                            @SectorId,
                            @OperatorCodigoFJ,
                            @LocalId,
                            @AssignmentCode,
                            @PairKey,
                            @IsTrainee,
                            @TrainerCodigoFJ,
                            @CountsTowardKousu,
                            1,
                            'Escalado',
                            @Notes
                        );",
                    new
                    {
                        ScheduleDate = normalizedDate,
                        request.ShiftId,
                        request.SectorId,
                        request.OperatorCodigoFJ,
                        request.LocalId,
                        AssignmentCode = assignmentCode,
                        PairKey = NullIfWhiteSpace(request.PairKey),
                        IsTrainee = request.IsTrainee ? 1 : 0,
                        TrainerCodigoFJ = NullIfWhiteSpace(request.TrainerCodigoFJ),
                        CountsTowardKousu = request.CountsTowardKousu ? 1 : 0,
                        Notes = NullIfWhiteSpace(request.Notes)
                    },
                    transaction);
            }
        }

        private static object? NullIfWhiteSpace(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static void EnsureColumn(System.Data.IDbConnection conn, string tableName, string columnName, string definition)
        {
            var exists = conn.ExecuteScalar<int>(
                $@"
                    SELECT COUNT(1)
                    FROM pragma_table_info('{tableName}')
                    WHERE name = @ColumnName;",
                new { ColumnName = columnName }) > 0;

            if (!exists)
            {
                conn.Execute($"ALTER TABLE {tableName} ADD COLUMN {columnName} {definition};");
            }
        }

        private static string BuildExportFileName(string sectorName, string shiftName)
        {
            return $"{Slugify(sectorName)}-{SlugifyShift(shiftName)}.html";
        }

        private static string SlugifyShift(string value)
        {
            var normalized = Slugify(value);

            if (normalized.Contains("dia", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("hiru", StringComparison.OrdinalIgnoreCase))
            {
                return "dia";
            }

            if (normalized.Contains("noite", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("yakin", StringComparison.OrdinalIgnoreCase))
            {
                return "noite";
            }

            return normalized;
        }

        private static string Slugify(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "haidai";
            }

            var normalized = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var ch in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(ch))
                {
                    builder.Append(char.ToLowerInvariant(ch));
                }
                else if (builder.Length == 0 || builder[^1] != '-')
                {
                    builder.Append('-');
                }
            }

            var result = builder.ToString().Trim('-');
            return string.IsNullOrWhiteSpace(result) ? "haidai" : result;
        }

        private static string BuildExportHtml(int sectorId, string sectorName, string shiftName, string otherFileName, HaidaiBoardPayload board)
        {
            var rows = board.Groups
                .SelectMany(group => group.Rows)
                .ToList();

            var activeRows = rows
                .Where(row => row.IsLineupActive && !string.IsNullOrWhiteSpace(row.AssignmentCode))
                .ToList();

            var layout = LoadExportLayout(sectorId);
            var layoutMarkup = layout == null
                ? string.Empty
                : BuildMappedLayoutMarkup(layout, activeRows);

            var unmappedRows = activeRows
                .Where(row => row.LocalId == null || layout == null || !layout.Locals.Any(item => item.LocalId == row.LocalId.Value))
                .ToList();

            var summaryBadges = new List<(string Label, int Value)>
            {
                ("Operadores", board.Summary.OperatorCount),
                ("Escalados", board.Summary.AssignedCount),
                ("Aprendizes", board.Summary.TraineeCount),
                ("Duplas", board.Summary.PairCount)
            };

            if (unmappedRows.Count > 0)
            {
                summaryBadges.Add(("Sem posicao", unmappedRows.Count));
            }

            var summaryMarkup = string.Join(
                Environment.NewLine,
                summaryBadges.Select(item => $@"
                    <article class=""summary-card"">
                        {EscapeHtml(item.Label)}
                        <strong>{item.Value}</strong>
                    </article>"));

            var unmappedMarkup = unmappedRows.Count == 0
                ? "<p class=\"empty-copy\">Todas as areas ativas estao posicionadas no mapa.</p>"
                : string.Join(
                    Environment.NewLine,
                    unmappedRows.Select(row => $@"
                        <div class=""unmapped-item"">
                            <strong>{EscapeHtml(row.AssignmentCode)}</strong>
                            <span>{EscapeHtml(row.Name)}</span>
                        </div>"));

            var fallbackMarkup = string.Join(
                Environment.NewLine,
                activeRows
                    .GroupBy(row => string.IsNullOrWhiteSpace(row.AssignmentCode) ? "Sem area" : row.AssignmentCode)
                    .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(group => $@"
                        <div class=""fallback-item"">
                            <strong>{EscapeHtml(group.Key)}</strong>
                            <span>{EscapeHtml(string.Join(" / ", group.Select(item => item.Name)))}</span>
                        </div>"));

            return $@"
<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
    <meta charset=""UTF-8"">
    <meta http-equiv=""refresh"" content=""120"">
    <title>Haidai {EscapeHtml(sectorName)} - {EscapeHtml(shiftName)}</title>
    <style>
        :root {{
            color-scheme: light;
            --bg: #eef4f8;
            --surface: #ffffff;
            --accent: #144c8a;
            --accent-soft: #dbeafe;
            --text: #142133;
            --muted: #63748a;
            --border: #d7e1ea;
        }}
        * {{
            box-sizing: border-box;
        }}
        body {{
            margin: 0;
            font-family: ""Segoe UI"", Tahoma, sans-serif;
            background:
                radial-gradient(circle at top right, rgba(20, 76, 138, 0.18), transparent 24%),
                linear-gradient(180deg, #f6fbff 0%, var(--bg) 100%);
            color: var(--text);
        }}
        .page {{
            padding: 24px;
            display: grid;
            gap: 18px;
        }}
        .hero {{
            display: flex;
            justify-content: space-between;
            gap: 18px;
            padding: 22px;
            border-radius: 22px;
            background: linear-gradient(135deg, #0f4a86 0%, #2663b8 100%);
            color: #fff;
        }}
        .hero h1 {{
            margin: 0 0 8px;
            font-size: 36px;
        }}
        .hero p {{
            margin: 0;
            color: rgba(255,255,255,0.86);
        }}
        .hero-meta {{
            display: grid;
            grid-template-columns: repeat(2, minmax(180px, 1fr));
            gap: 12px;
            min-width: 360px;
        }}
        .meta-card {{
            padding: 14px 16px;
            border-radius: 16px;
            border: 1px solid rgba(255,255,255,0.18);
            background: rgba(255,255,255,0.12);
        }}
        .meta-card strong {{
            display: block;
            margin-top: 6px;
            font-size: 20px;
        }}
        .toolbar {{
            display: flex;
            justify-content: space-between;
            align-items: center;
            gap: 18px;
        }}
        .toolbar a {{
            display: inline-flex;
            align-items: center;
            justify-content: center;
            min-height: 44px;
            padding: 0 18px;
            border-radius: 999px;
            background: var(--accent);
            color: #fff;
            text-decoration: none;
            font-weight: 700;
        }}
        .summary {{
            display: grid;
            grid-template-columns: repeat(5, minmax(120px, 1fr));
            gap: 12px;
        }}
        .summary-card {{
            padding: 16px;
            border-radius: 18px;
            background: var(--surface);
            border: 1px solid var(--border);
            box-shadow: 0 10px 25px rgba(20, 33, 51, 0.06);
        }}
        .summary-card strong {{
            display: block;
            margin-top: 6px;
            font-size: 24px;
        }}
        .group-head h2,
        .section-head h3 {{
            margin: 0;
            font-size: 24px;
        }}
        .visual-grid {{
            display: grid;
            grid-template-columns: minmax(0, 2.1fr) minmax(280px, 0.9fr);
            gap: 18px;
            align-items: start;
        }}
        .layout-card, .side-card, .fallback-card {{
            padding: 18px;
            border-radius: 20px;
            background: var(--surface);
            border: 1px solid var(--border);
            box-shadow: 0 10px 25px rgba(20, 33, 51, 0.06);
        }}
        .section-head {{
            display: flex;
            justify-content: space-between;
            gap: 12px;
            align-items: baseline;
            margin-bottom: 12px;
        }}
        .layout-stage {{
            position: relative;
            overflow: hidden;
            border-radius: 18px;
            border: 1px solid var(--border);
            background:
                linear-gradient(180deg, #ffffff 0%, #f7fbff 100%);
        }}
        .layout-canvas {{
            position: absolute;
            inset: 0;
        }}
        .corridor {{
            position: absolute;
            border-radius: 999px;
            background: linear-gradient(180deg, #e2edf7 0%, #cddced 100%);
        }}
        .local-box {{
            position: absolute;
            padding: 10px;
            border-radius: 18px;
            border: 2px solid #c9d8e7;
            background: linear-gradient(180deg, #ffffff 0%, #edf5ff 100%);
            box-shadow: 0 12px 24px rgba(20, 33, 51, 0.08);
            overflow: hidden;
        }}
        .local-box strong {{
            display: block;
            font-size: 18px;
            color: var(--accent);
        }}
        .local-box small {{
            display: block;
            margin-top: 6px;
            font-size: 12px;
        }}
        .operator-stack {{
            display: grid;
            gap: 6px;
            margin-top: 8px;
        }}
        .operator-pill {{
            display: block;
            padding: 7px 8px;
            border-radius: 12px;
            background: #f5f9ff;
            border: 1px solid #d8e5f2;
            font-size: 14px;
            font-weight: 700;
            line-height: 1.25;
        }}
        .operator-pill.trainee {{
            background: #fff3d6;
            border-color: #f4d18c;
        }}
        .operator-pill .subcode {{
            display: block;
            margin-top: 3px;
            font-size: 11px;
            color: var(--muted);
            font-weight: 600;
        }}
        .side-stack,
        .unmapped-list,
        .fallback-list {{
            display: grid;
            gap: 10px;
        }}
        .unmapped-item,
        .fallback-item {{
            display: grid;
            gap: 4px;
            padding: 12px 14px;
            border-radius: 14px;
            border: 1px solid var(--border);
            background: #f9fbfe;
        }}
        .empty-copy {{
            margin: 0;
            color: var(--muted);
        }}
        small {{
            color: var(--muted);
        }}
        @media (max-width: 1180px) {{
            .hero {{
                flex-direction: column;
            }}
            .hero-meta {{
                min-width: 0;
            }}
            .summary {{
                grid-template-columns: repeat(2, minmax(120px, 1fr));
            }}
            .visual-grid {{
                grid-template-columns: 1fr;
            }}
        }}
    </style>
</head>
<body>
    <main class=""page"">
        <section class=""hero"">
            <div>
                <h1>{EscapeHtml(sectorName)} - {EscapeHtml(shiftName)}</h1>
                <p>Haidai exportado em {EscapeHtml(board.DateIso)}.</p>
            </div>
            <div class=""hero-meta"">
                <div class=""meta-card"">
                    Data
                    <strong>{EscapeHtml(board.DateIso)}</strong>
                </div>
                <div class=""meta-card"">
                    Turno
                    <strong>{EscapeHtml(shiftName)}</strong>
                </div>
                <div class=""meta-card"">
                    Operadores
                    <strong>{board.Summary.OperatorCount}</strong>
                </div>
                <div class=""meta-card"">
                    Escalados
                    <strong>{board.Summary.AssignedCount}</strong>
                </div>
            </div>
        </section>

        <section class=""toolbar"">
            <div>TV mode - atualizacao automatica a cada 2 minutos.</div>
            <a href=""{EscapeHtml(otherFileName)}"">Abrir outro turno</a>
        </section>

        <section class=""summary"">
            {summaryMarkup}
        </section>

        {(string.IsNullOrWhiteSpace(layoutMarkup)
            ? $@"<article class=""fallback-card"">
                    <div class=""section-head"">
                        <h3>Mapa resumido</h3>
                        <span>Visual alternativo</span>
                    </div>
                    <div class=""fallback-list"">{fallbackMarkup}</div>
                </article>"
            : $@"<section class=""visual-grid"">
                    <article class=""layout-card"">
                        <div class=""section-head"">
                            <h3>Mapa do setor</h3>
                            <span>Visual usado na TV do operador</span>
                        </div>
                        {layoutMarkup}
                    </article>
                    <div class=""side-stack"">
                        <article class=""side-card"">
                            <div class=""section-head"">
                                <h3>Resumo do turno</h3>
                                <span>Informacao publica</span>
                            </div>
                            <p class=""empty-copy"">Use este quadro para consultar rapidamente a area designada de cada operador.</p>
                        </article>
                        <article class=""side-card"">
                            <div class=""section-head"">
                                <h3>Sem posicao no mapa</h3>
                                <span>{unmappedRows.Count} item(ns)</span>
                            </div>
                            <div class=""unmapped-list"">{unmappedMarkup}</div>
                        </article>
                    </div>
                </section>")}
    </main>
</body>
</html>";
        }

        private static HaidaiExportLayoutDefinition? LoadExportLayout(int sectorId)
        {
            var layoutPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "ui",
                "presence-layout",
                "layouts.json");

            if (!File.Exists(layoutPath))
            {
                return null;
            }

            var json = File.ReadAllText(layoutPath);
            var layouts = JsonSerializer.Deserialize<Dictionary<string, HaidaiExportLayoutDefinition>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return layouts != null && layouts.TryGetValue(sectorId.ToString(CultureInfo.InvariantCulture), out var layout)
                ? layout
                : null;
        }

        private static string BuildMappedLayoutMarkup(HaidaiExportLayoutDefinition layout, IReadOnlyList<HaidaiBoardRow> activeRows)
        {
            var localsById = activeRows
                .Where(row => row.LocalId.HasValue)
                .GroupBy(row => row.LocalId!.Value)
                .ToDictionary(group => group.Key, group => group.ToList());

            var corridors = string.Join(
                Environment.NewLine,
                layout.Corridors.Select(corridor => $@"
                    <div class=""corridor"" style=""left:{corridor.X}px;top:{corridor.Y}px;width:{corridor.W}px;height:{corridor.H}px;""></div>"));

            var slots = string.Join(
                Environment.NewLine,
                layout.Locals.Select(slot =>
                {
                    localsById.TryGetValue(slot.LocalId, out var assigned);
                    assigned ??= new List<HaidaiBoardRow>();

                    var headerCode = assigned.FirstOrDefault()?.LocalShortCode;
                    if (string.IsNullOrWhiteSpace(headerCode))
                    {
                        headerCode = assigned.FirstOrDefault()?.AssignmentCode;
                    }

                    headerCode ??= $"Area {slot.LocalId}";

                    var operators = assigned.Count == 0
                        ? "<span class=\"operator-pill\"><span class=\"subcode\">Sem operador</span></span>"
                        : string.Join(
                            Environment.NewLine,
                            assigned.Select(row =>
                            {
                                var detailCode = string.Equals(row.AssignmentCode, row.LocalShortCode, StringComparison.OrdinalIgnoreCase)
                                    ? string.Empty
                                    : row.AssignmentCode;

                                var tags = new List<string>();
                                if (row.IsTrainee)
                                {
                                    tags.Add("Aprendiz");
                                }

                                if (!row.CountsTowardKousu)
                                {
                                    tags.Add("Nao conta kousu");
                                }

                                if (!string.IsNullOrWhiteSpace(detailCode))
                                {
                                    tags.Insert(0, detailCode);
                                }

                                var suffix = tags.Count == 0
                                    ? string.Empty
                                    : $"<span class=\"subcode\">{EscapeHtml(string.Join(" | ", tags))}</span>";

                                return $@"
                                    <span class=""operator-pill {(row.IsTrainee ? "trainee" : string.Empty)}"">
                                        {EscapeHtml(row.Name)}
                                        {suffix}
                                    </span>";
                            }));

                    return $@"
                        <article class=""local-box"" style=""left:{slot.X}px;top:{slot.Y}px;width:{slot.W}px;height:{slot.H}px;"">
                            <strong>{EscapeHtml(headerCode)}</strong>
                            <small>{EscapeHtml(slot.Label ?? string.Empty)}</small>
                            <div class=""operator-stack"">
                                {operators}
                            </div>
                        </article>";
                }));

            return $@"
                <div class=""layout-stage"" style=""width:{layout.Width}px;height:{layout.Height}px;max-width:100%;"">
                    <div class=""layout-canvas"">
                        {corridors}
                        {slots}
                    </div>
                </div>";
        }

        private static string EscapeHtml(string? value)
        {
            return (value ?? string.Empty)
                .Replace("&", "&amp;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal)
                .Replace("\"", "&quot;", StringComparison.Ordinal);
        }
    }

    public sealed record HaidaiInitPayload(
        string DateIso,
        int ShiftId,
        int SectorId,
        IReadOnlyList<HaidaiLookupItem> Shifts,
        IReadOnlyList<HaidaiLookupItem> Sectors,
        IReadOnlyList<HaidaiLocalLookup> Locals,
        IReadOnlyList<HaidaiOperatorLookup> Operators);

    public sealed class HaidaiLookupItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class HaidaiLocalLookup
    {
        public int Id { get; set; }
        public int SectorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortCode { get; set; } = string.Empty;
    }

    public sealed class HaidaiOperatorLookup
    {
        public string CodigoFJ { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int SectorId { get; set; }
        public int ShiftId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public bool Trainer { get; set; }
        public bool IsLeader { get; set; }
    }

    public sealed record HaidaiMonthlyPlanPayload(
        int Year,
        int Month,
        int ShiftId,
        int SectorId,
        IReadOnlyList<int> Days,
        IReadOnlyList<HaidaiMonthlyGroupPlan> Groups);

    public sealed record HaidaiMonthlyGroupPlan(
        int GroupId,
        string GroupName,
        IReadOnlyList<HaidaiMonthlyOperatorPlan> Operators);

    public sealed record HaidaiMonthlyOperatorPlan(
        string CodigoFJ,
        string Name,
        string NameJp,
        int GroupId,
        string GroupName,
        IReadOnlyList<HaidaiMonthlyCell> Cells);

    public sealed record HaidaiMonthlyCell(
        int Day,
        string AssignmentCode,
        int? LocalId,
        bool IsTrainee,
        bool IsLineupActive,
        string Status);

    public sealed record HaidaiMonthlySaveRequest(
        int Year,
        int Month,
        int ShiftId,
        int SectorId,
        IReadOnlyList<HaidaiMonthlySaveCell> Cells);

    public sealed record HaidaiMonthlySaveCell(
        string OperatorCodigoFJ,
        int Day,
        string AssignmentCode);

    public sealed record HaidaiBoardPayload(
        string DateIso,
        int ShiftId,
        int SectorId,
        HaidaiSummary Summary,
        IReadOnlyList<HaidaiGroupBlock> Groups);

    public sealed record HaidaiSummary(
        int OperatorCount,
        int AssignedCount,
        int YukyuCount,
        int FaltaCount,
        int LateCount,
        int EarlyLeaveCount,
        int TraineeCount,
        int PairCount);

    public sealed record HaidaiGroupBlock(
        int GroupId,
        string GroupName,
        int OperatorCount,
        IReadOnlyList<HaidaiBoardRow> Rows);

    public sealed record HaidaiBoardRow(
        string CodigoFJ,
        string Name,
        string NameJp,
        int GroupId,
        string GroupName,
        bool Trainer,
        bool IsLeader,
        int? LocalId,
        string LocalName,
        string LocalShortCode,
        string AssignmentCode,
        string StoredAssignmentCode,
        string PairKey,
        bool IsTrainee,
        string TrainerCodigoFJ,
        bool CountsTowardKousu,
        bool IsLineupActive,
        string Notes,
        int ExceptionMotiveId,
        string ExceptionMotiveName,
        string ExceptionNotes,
        string Status,
        string MovementSummary,
        int MovementCount);

    public sealed record HaidaiSaveAssignmentRequest(
        DateTime Date,
        int ShiftId,
        int SectorId,
        string OperatorCodigoFJ,
        int? LocalId,
        string AssignmentCode,
        string PairKey,
        bool IsTrainee,
        string TrainerCodigoFJ,
        bool CountsTowardKousu,
        string Notes);

    public sealed record HaidaiMovementRequest(
        DateTime Date,
        int ShiftId,
        int SectorId,
        string OperatorCodigoFJ,
        string MovementType,
        string EventTime,
        string ReplacementOperatorCodigoFJ,
        int? LocalId,
        string AssignmentCode,
        string PairKey,
        string Reason,
        string CreatedByCodigoFJ);

    public sealed record HaidaiExportResult(string Directory, IReadOnlyList<string> Files);
    public sealed record HaidaiPlannedAssignment(
        string OperatorCodigoFJ,
        int ShiftId,
        int SectorId,
        int? LocalId,
        string AssignmentCode,
        string PairKey,
        bool IsTrainee,
        string TrainerCodigoFJ,
        bool CountsTowardKousu);

    internal sealed class HaidaiOperatorRow
    {
        public string CodigoFJ { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NameJp { get; set; } = string.Empty;
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public bool Trainer { get; set; }
        public bool IsLeader { get; set; }
    }

    internal sealed class HaidaiAssignmentRecord
    {
        public int Id { get; set; }
        public string ScheduleDate { get; set; } = string.Empty;
        public int ShiftId { get; set; }
        public int SectorId { get; set; }
        public string OperatorCodigoFJ { get; set; } = string.Empty;
        public int? LocalId { get; set; }
        public string AssignmentCode { get; set; } = string.Empty;
        public string PairKey { get; set; } = string.Empty;
        public bool IsTrainee { get; set; }
        public string TrainerCodigoFJ { get; set; } = string.Empty;
        public bool CountsTowardKousu { get; set; }
        public bool IsLineupActive { get; set; }
        public string AvailabilityStatus { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    internal sealed class HaidaiMovementRecord
    {
        public int Id { get; set; }
        public string OperatorCodigoFJ { get; set; } = string.Empty;
        public string MovementType { get; set; } = string.Empty;
        public string EventTime { get; set; } = string.Empty;
        public string AssignmentCode { get; set; } = string.Empty;
        public string PairKey { get; set; } = string.Empty;
        public string ReplacementOperatorCodigoFJ { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }

    internal sealed class HaidaiExceptionRecord
    {
        public string OperatorCodigoFJ { get; set; } = string.Empty;
        public int MotiveId { get; set; }
        public string MotiveName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    internal sealed class HaidaiMonthlyExceptionRecord
    {
        public string OperatorCodigoFJ { get; set; } = string.Empty;
        public string RequestDate { get; set; } = string.Empty;
        public int MotiveId { get; set; }
    }

    internal sealed record HaidaiResolvedMonthlyCell(
        int? LocalId,
        string AssignmentCode,
        bool IsTrainee,
        bool IsLineupActive,
        string AvailabilityStatus);

    internal sealed class HaidaiExportLayoutDefinition
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<HaidaiExportLocalSlot> Locals { get; set; } = new();
        public List<HaidaiExportCorridorSlot> Corridors { get; set; } = new();
    }

    internal sealed class HaidaiExportLocalSlot
    {
        public int LocalId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public string? Label { get; set; }
    }

    internal sealed class HaidaiExportCorridorSlot
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public bool Rotate { get; set; }
    }
}
