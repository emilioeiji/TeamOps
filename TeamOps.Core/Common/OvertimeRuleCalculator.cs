using System.Globalization;

namespace TeamOps.Core.Common;

public enum OvertimeShiftKind
{
    Hirukin = 1,
    Yakin = 2
}

public sealed record OvertimeShiftWindow(
    OvertimeShiftKind ShiftKind,
    string ShiftLabel,
    DateTime ScheduledStart,
    DateTime TeijiEnd,
    DateTime ZangyouStart,
    DateTime ZangyouEnd,
    int ExpectedWorkMinutes,
    int MaxNormalZangyouMinutes);

public sealed record OvertimeCalculationInput(
    string OperatorId,
    string OperatorName,
    DateTime ScheduleDate,
    string ShiftName,
    bool HasSchedule,
    bool IsHolidayWork,
    string AttendanceStatus,
    DateTime? ActualEnd,
    bool HasLate,
    bool HasEarlyLeave,
    bool IsProjection,
    bool AllowFullShiftFallbackWithoutPresenceEnd);

public sealed record OvertimeCalculationAudit(
    string OperatorId,
    string OperatorName,
    DateTime Date,
    string DayOfWeek,
    string Shift,
    bool IsSunday,
    bool IsHolidayWork,
    string AttendanceStatus,
    DateTime? ScheduledStart,
    DateTime? TeijiEnd,
    DateTime? ZangyouStart,
    DateTime? ZangyouEnd,
    DateTime? ActualEnd,
    bool HasLate,
    bool HasEarlyLeave,
    double NormalZangyouMinutes,
    double HolidayWorkMinutes,
    double TotalOvertimeMinutes,
    string Reason,
    bool HasDiagnostic,
    bool WorkedSunday,
    bool IsFullShift,
    double ExpectedWorkMinutes,
    double ActualWorkMinutes);

public static class OvertimeRuleCalculator
{
    private const int HolidayWorkMinutes = 11 * 60;
    private const int NormalMaxMinutes = 2 * 60;
    private const int ExpectedShiftMinutes = 605;

    public static bool TryResolveShiftWindow(string shiftName, DateTime scheduleDate, out OvertimeShiftWindow window)
    {
        var normalized = (shiftName ?? string.Empty).Trim();
        if (normalized.Contains("noite", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("night", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("yakin", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("夜", StringComparison.OrdinalIgnoreCase))
        {
            var start = scheduleDate.Date.AddHours(20).AddMinutes(30);
            var teijiEnd = scheduleDate.Date.AddDays(1).AddHours(6).AddMinutes(35);
            var zangyouEnd = scheduleDate.Date.AddDays(1).AddHours(8).AddMinutes(35);
            window = new OvertimeShiftWindow(
                OvertimeShiftKind.Yakin,
                "Yakin",
                start,
                teijiEnd,
                teijiEnd,
                zangyouEnd,
                ExpectedShiftMinutes,
                NormalMaxMinutes);
            return true;
        }

        if (normalized.Contains("dia", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("day", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("hiru", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("hirukin", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(normalized))
        {
            var start = scheduleDate.Date.AddHours(8).AddMinutes(30);
            var teijiEnd = scheduleDate.Date.AddHours(18).AddMinutes(35);
            var zangyouEnd = scheduleDate.Date.AddHours(20).AddMinutes(35);
            window = new OvertimeShiftWindow(
                OvertimeShiftKind.Hirukin,
                "Hirukin",
                start,
                teijiEnd,
                teijiEnd,
                zangyouEnd,
                ExpectedShiftMinutes,
                NormalMaxMinutes);
            return true;
        }

        window = default!;
        return false;
    }

    public static OvertimeCalculationAudit Calculate(OvertimeCalculationInput input)
    {
        var isSunday = input.ScheduleDate.DayOfWeek == DayOfWeek.Sunday;
        var normalizedStatus = NormalizeAttendanceStatus(input.AttendanceStatus);
        var hasDiagnostic = input.HasSchedule
            || input.ActualEnd.HasValue
            || input.IsHolidayWork
            || input.HasLate
            || input.HasEarlyLeave
            || !string.IsNullOrWhiteSpace(normalizedStatus);

        if (!TryResolveShiftWindow(input.ShiftName, input.ScheduleDate, out var window))
        {
            return new OvertimeCalculationAudit(
                input.OperatorId,
                input.OperatorName,
                input.ScheduleDate.Date,
                input.ScheduleDate.DayOfWeek.ToString(),
                input.ShiftName,
                isSunday,
                input.IsHolidayWork,
                normalizedStatus,
                null,
                null,
                null,
                null,
                input.ActualEnd,
                input.HasLate,
                input.HasEarlyLeave,
                0,
                0,
                0,
                "unsupported_shift",
                hasDiagnostic,
                false,
                false,
                0,
                0);
        }

        var actualEnd = NormalizeActualEnd(input.ActualEnd, window, input.IsProjection);
        var expectedWorkMinutes = window.ExpectedWorkMinutes;
        var actualWorkMinutes = actualEnd.HasValue && actualEnd.Value > window.ScheduledStart
            ? Math.Max(0, Math.Min((actualEnd.Value - window.ScheduledStart).TotalMinutes, window.ExpectedWorkMinutes + window.MaxNormalZangyouMinutes))
            : 0d;

        if (normalizedStatus is "falta" or "yukyu")
        {
            return BuildAudit(input, window, actualEnd, isSunday, normalizedStatus, 0, 0, 0, normalizedStatus, false, expectedWorkMinutes, 0, hasDiagnostic);
        }

        if (!input.HasSchedule)
        {
            return BuildAudit(input, window, actualEnd, isSunday, normalizedStatus, 0, 0, 0, "not_scheduled", false, expectedWorkMinutes, actualWorkMinutes, hasDiagnostic);
        }

        if (input.IsHolidayWork)
        {
            return BuildAudit(input, window, actualEnd, isSunday, normalizedStatus, 0, HolidayWorkMinutes, HolidayWorkMinutes, "holiday_work_11h", isSunday, expectedWorkMinutes, actualWorkMinutes, hasDiagnostic);
        }

        if (isSunday)
        {
            return BuildAudit(input, window, actualEnd, isSunday, normalizedStatus, 0, 0, 0, "sunday_no_zangyou", actualEnd.HasValue || input.IsProjection, expectedWorkMinutes, actualWorkMinutes, hasDiagnostic);
        }

        if (input.HasEarlyLeave)
        {
            return BuildAudit(input, window, actualEnd, isSunday, normalizedStatus, 0, 0, 0, "early_leave_zero", false, expectedWorkMinutes, actualWorkMinutes, hasDiagnostic);
        }

        if (!actualEnd.HasValue)
        {
            if (input.AllowFullShiftFallbackWithoutPresenceEnd
                && !input.HasLate
                && !input.HasEarlyLeave
                && normalizedStatus is "present" or "scheduled")
            {
                return BuildAudit(
                    input,
                    window,
                    window.ZangyouEnd,
                    isSunday,
                    normalizedStatus,
                    window.MaxNormalZangyouMinutes,
                    0,
                    window.MaxNormalZangyouMinutes,
                    "full_shift_from_haidai_without_presence_end",
                    false,
                    expectedWorkMinutes,
                    expectedWorkMinutes + window.MaxNormalZangyouMinutes,
                    hasDiagnostic,
                    true);
            }

            return BuildAudit(input, window, actualEnd, isSunday, normalizedStatus, 0, 0, 0, input.IsProjection ? "projected_without_end" : "no_actual_end", false, expectedWorkMinutes, actualWorkMinutes, hasDiagnostic);
        }

        if (actualEnd.Value <= window.TeijiEnd)
        {
            if (input.AllowFullShiftFallbackWithoutPresenceEnd
                && !input.HasLate
                && !input.HasEarlyLeave
                && normalizedStatus is "present" or "scheduled")
            {
                return BuildAudit(
                    input,
                    window,
                    window.ZangyouEnd,
                    isSunday,
                    normalizedStatus,
                    window.MaxNormalZangyouMinutes,
                    0,
                    window.MaxNormalZangyouMinutes,
                    "full_shift_from_haidai_without_presence_end",
                    false,
                    expectedWorkMinutes,
                    expectedWorkMinutes + window.MaxNormalZangyouMinutes,
                    hasDiagnostic,
                    true);
            }

            return BuildAudit(input, window, actualEnd, isSunday, normalizedStatus, 0, 0, 0, "left_at_or_before_teiji", false, expectedWorkMinutes, actualWorkMinutes, hasDiagnostic);
        }

        var normalMinutes = Math.Min(window.MaxNormalZangyouMinutes, Math.Max(0, (actualEnd.Value - window.TeijiEnd).TotalMinutes));
        var reason = actualEnd.Value >= window.ZangyouEnd
            ? input.IsProjection
                ? "projected_full_zangyou"
                : input.HasLate
                    ? "late_with_full_zangyou"
                    : "full_zangyou"
            : input.HasLate
                ? "late_partial_zangyou"
                : "partial_zangyou";

        var isFullShift = normalMinutes >= window.MaxNormalZangyouMinutes;
        return BuildAudit(input, window, actualEnd, isSunday, normalizedStatus, normalMinutes, 0, normalMinutes, reason, false, expectedWorkMinutes, actualWorkMinutes, hasDiagnostic, isFullShift);
    }

    public static DateTime? NormalizeMovementMoment(DateTime scheduleDate, string shiftName, string? eventTime, string? eventDateTime)
    {
        if (!string.IsNullOrWhiteSpace(eventDateTime)
            && DateTime.TryParseExact(eventDateTime.Trim(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var explicitDateTime))
        {
            return explicitDateTime;
        }

        if (string.IsNullOrWhiteSpace(eventTime)
            || !TryResolveShiftWindow(shiftName, scheduleDate, out var window))
        {
            return null;
        }

        var normalizedTime = eventTime.Trim();
        if (normalizedTime.Length == 5)
        {
            normalizedTime += ":00";
        }

        if (!TimeSpan.TryParse(normalizedTime, CultureInfo.InvariantCulture, out var timePart))
        {
            return null;
        }

        var candidate = scheduleDate.Date.Add(timePart);
        if (window.ShiftKind == OvertimeShiftKind.Yakin && candidate < window.ScheduledStart)
        {
            candidate = candidate.AddDays(1);
        }

        return candidate;
    }

    private static OvertimeCalculationAudit BuildAudit(
        OvertimeCalculationInput input,
        OvertimeShiftWindow window,
        DateTime? actualEnd,
        bool isSunday,
        string normalizedStatus,
        double normalMinutes,
        double holidayMinutes,
        double totalMinutes,
        string reason,
        bool workedSunday,
        double expectedWorkMinutes,
        double actualWorkMinutes,
        bool hasDiagnostic,
        bool isFullShift = false)
    {
        return new OvertimeCalculationAudit(
            input.OperatorId,
            input.OperatorName,
            input.ScheduleDate.Date,
            input.ScheduleDate.DayOfWeek.ToString(),
            window.ShiftLabel,
            isSunday,
            input.IsHolidayWork,
            normalizedStatus,
            window.ScheduledStart,
            window.TeijiEnd,
            window.ZangyouStart,
            window.ZangyouEnd,
            actualEnd,
            input.HasLate,
            input.HasEarlyLeave,
            normalMinutes,
            holidayMinutes,
            totalMinutes,
            reason,
            hasDiagnostic,
            workedSunday,
            isFullShift,
            expectedWorkMinutes,
            actualWorkMinutes);
    }

    private static DateTime? NormalizeActualEnd(DateTime? actualEnd, OvertimeShiftWindow window, bool isProjection)
    {
        if (actualEnd.HasValue)
        {
            if (actualEnd.Value < window.ScheduledStart)
            {
                return window.ScheduledStart;
            }

            if (actualEnd.Value > window.ZangyouEnd)
            {
                return window.ZangyouEnd;
            }

            return actualEnd.Value;
        }

        return isProjection ? window.ZangyouEnd : null;
    }

    private static string NormalizeAttendanceStatus(string? status)
    {
        var normalized = (status ?? string.Empty).Trim();
        return normalized.ToLowerInvariant() switch
        {
            "present" => "present",
            "presente" => "present",
            "scheduled" => "scheduled",
            "escalado" => "scheduled",
            "falta" => "falta",
            "yukyu" => "yukyu",
            "yuukyu" => "yukyu",
            "late" => "late",
            "atraso" => "late",
            "early_leave" => "early_leave",
            "saida antecipada" => "early_leave",
            "saiu cedo" => "early_leave",
            "folga" => "folga",
            _ => normalized.ToLowerInvariant()
        };
    }
}
