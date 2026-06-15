using System;
using System.Collections.Generic;

namespace TeamOps.UI.Forms.Models
{
    public sealed class ProductionDashboardFilter
    {
        public DateTime Date { get; set; }
        public int ShiftId { get; set; }
        public int SectorId { get; set; }
        public int LocalId { get; set; }
        public int MachineId { get; set; }
        public string MachineCode { get; set; } = string.Empty;
    }

    public sealed class ProductionShiftPeriod
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public sealed class ProductionDashboardDto
    {
        public ProductionShiftPeriod Period { get; set; } = new();
        public double ProductionPercent { get; set; }
        public int MachinesRunning { get; set; }
        public int MachinesStopped { get; set; }
        public int MachinesIgnored { get; set; }
        public int MachinesTotal { get; set; }
        public double AverageOperatingProcessMinutes { get; set; }
        public double ErrorMinutes { get; set; }
        public double InactiveMinutes { get; set; }
        public List<ProductionMachineSummaryDto> Machines { get; } = new();
        public List<ProductionAreaSummaryDto> Areas { get; } = new();
        public List<ProductionRankingItemDto> Ranking { get; } = new();
        public List<ProductionTimelineRowDto> Timeline { get; } = new();
        public List<ProductionShiftComparisonDto> ShiftComparisons { get; } = new();
        public List<ProductionDailyTrendDto> DailyTrend { get; } = new();
        public List<ProductionAreaHistoryDto> AreaHistory { get; } = new();
        public List<ProductionOperatorRankingDto> OperatorRanking { get; } = new();
        public GBareruCapacityForecastDto GBareruCapacityForecast { get; set; } = new();
    }

    public sealed class GBareruCapacityForecastDto
    {
        public bool IsAvailable { get; set; }
        public string Message { get; set; } = string.Empty;
        public double EciiMinutes { get; set; }
        public double BunkatsuMinutes { get; set; }
        public double DcsMinutes { get; set; }
        public int PeopleCount { get; set; }
        public string CycleMode { get; set; } = string.Empty;
        public double CycleMinutes { get; set; }
        public double Block1Minutes { get; set; }
        public double Block2Minutes { get; set; }
        public string Bottleneck { get; set; } = string.Empty;
        public double AvailableMinutes { get; set; }
        public double ForecastCapacity { get; set; }
        public double ForecastKadouritsuPercent { get; set; }
        public double RealKadouritsuPercent { get; set; }
        public double DifferencePercent { get; set; }
        public long CalculationMs { get; set; }
        public List<GBareruCapacityAreaForecastDto> Areas { get; } = new();
    }

    public sealed class GBareruCapacityAreaForecastDto
    {
        public int? LocalId { get; set; }
        public string LocalNamePt { get; set; } = string.Empty;
        public string LocalNameJp { get; set; } = string.Empty;
        public int PeopleCount { get; set; }
        public string CycleMode { get; set; } = string.Empty;
        public double CycleMinutes { get; set; }
        public double ForecastCapacity { get; set; }
        public double ForecastKadouritsuPercent { get; set; }
        public double RealKadouritsuPercent { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public sealed class ProductionOperatorDetailDto
    {
        public string OperatorCodigoFJ { get; set; } = string.Empty;
        public string OperatorNamePt { get; set; } = string.Empty;
        public string OperatorNameJp { get; set; } = string.Empty;
        public string ShiftNamePt { get; set; } = string.Empty;
        public string ShiftNameJp { get; set; } = string.Empty;
        public double AverageKadouritsuPercent { get; set; }
        public double TotalRunningMinutes { get; set; }
        public int AssignedAreaCount { get; set; }
        public int FullCoverageDays { get; set; }
        public int PartialCoverageDays { get; set; }
        public List<string> LocalNamesPt { get; set; } = new();
        public List<string> LocalNamesJp { get; set; } = new();
        public List<ProductionOperatorHistoryEntryDto> Entries { get; } = new();
    }

    public sealed class ProductionMachineSummaryDto
    {
        public int MachineId { get; set; }
        public string MachineCode { get; set; } = string.Empty;
        public string Machine { get; set; } = string.Empty;
        public string LineCode { get; set; } = string.Empty;
        public string MachineNamePt { get; set; } = string.Empty;
        public string MachineNameJp { get; set; } = string.Empty;
        public int? SectorId { get; set; }
        public string SectorNamePt { get; set; } = string.Empty;
        public string SectorNameJp { get; set; } = string.Empty;
        public int? LocalId { get; set; }
        public string Area { get; set; } = string.Empty;
        public string LocalNamePt { get; set; } = string.Empty;
        public string LocalNameJp { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public int DisplayCode { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string RecipeName { get; set; } = string.Empty;
        public string LotNo { get; set; } = string.Empty;
        public string Ec2StatusText { get; set; } = string.Empty;
        public string Ec2Status { get; set; } = string.Empty;
        public string Ec2PartCode { get; set; } = string.Empty;
        public string PartCode { get; set; } = string.Empty;
        public string Ec2PartColorHex { get; set; } = string.Empty;
        public string PartCodeColorHex { get; set; } = string.Empty;
        public string Ec2PartTextColorHex { get; set; } = string.Empty;
        public string PartCodeTextColorHex { get; set; } = string.Empty;
        public string PartCodeDescription { get; set; } = string.Empty;
        public string Ec2IgnoreReason { get; set; } = string.Empty;
        public bool IsEc2Running { get; set; }
        public bool IsEc2Ignored { get; set; }
        public double? Ec2ProcessMinutes { get; set; }
        public double? Ec2SettingRate { get; set; }
        public double? Ec2OperationRate { get; set; }
        public DateTime? Ec2SnapshotAt { get; set; }
        public DateTime? LastUpdate { get; set; }
        public bool EnteredAreaAverage { get; set; }
        public string AreaAverageReason { get; set; } = string.Empty;
        public double RunningMinutes { get; set; }
        public double StoppedMinutes { get; set; }
        public double InactiveMinutes { get; set; }
        public double ErrorMinutes { get; set; }
        public double TotalMinutes { get; set; }
        public double ProductionPercent { get; set; }
        public List<string> ScheduledOperatorsPt { get; } = new();
        public List<string> ScheduledOperatorsJp { get; } = new();
    }

    public sealed class ProductionRankingItemDto
    {
        public int? LocalId { get; set; }
        public string LocalNamePt { get; set; } = string.Empty;
        public string LocalNameJp { get; set; } = string.Empty;
        public string MachineCode { get; set; } = string.Empty;
        public string MachineNamePt { get; set; } = string.Empty;
        public string MachineNameJp { get; set; } = string.Empty;
        public double StopMinutes { get; set; }
        public double ErrorMinutes { get; set; }
        public double TotalImpactMinutes { get; set; }
    }

    public sealed class ProductionTimelineRowDto
    {
        public int? SectorId { get; set; }
        public int? LocalId { get; set; }
        public string LocalNamePt { get; set; } = string.Empty;
        public string LocalNameJp { get; set; } = string.Empty;
        public string MachineCode { get; set; } = string.Empty;
        public string LineCode { get; set; } = string.Empty;
        public string MachineNamePt { get; set; } = string.Empty;
        public string MachineNameJp { get; set; } = string.Empty;
        public List<ProductionTimelineCellDto> Cells { get; } = new();
    }

    public sealed class ProductionTimelineCellDto
    {
        public string TimeLabel { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public int StatusCode { get; set; }
        public int DisplayCode { get; set; }
        public bool IsIgnored { get; set; }
        public string IgnoreReason { get; set; } = string.Empty;
        public string CssClass { get; set; } = string.Empty;
    }

    public sealed class ProductionAreaSummaryDto
    {
        public int? LocalId { get; set; }
        public int? SectorId { get; set; }
        public string LocalNamePt { get; set; } = string.Empty;
        public string LocalNameJp { get; set; } = string.Empty;
        public string SectorNamePt { get; set; } = string.Empty;
        public string SectorNameJp { get; set; } = string.Empty;
        public int MachineCount { get; set; }
        public int MachinesRunning { get; set; }
        public int MachinesStopped { get; set; }
        public int MachinesIgnored { get; set; }
        public double AverageOperatingProcessMinutes { get; set; }
        public double RunningMinutes { get; set; }
        public double StoppedMinutes { get; set; }
        public double InactiveMinutes { get; set; }
        public double ErrorMinutes { get; set; }
        public double TotalMinutes { get; set; }
        public double ProductionPercent { get; set; }
        public DateTime? LastUpdate { get; set; }
        public int ScheduledOperatorCount { get; set; }
        public List<string> ScheduledOperatorsPt { get; } = new();
        public List<string> ScheduledOperatorsJp { get; } = new();
    }

    public sealed class ProductionShiftComparisonDto
    {
        public int ShiftId { get; set; }
        public string ShiftNamePt { get; set; } = string.Empty;
        public string ShiftNameJp { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public double ProductionPercent { get; set; }
        public double RunningMinutes { get; set; }
        public double StoppedMinutes { get; set; }
        public double InactiveMinutes { get; set; }
        public double ErrorMinutes { get; set; }
        public int MachineCount { get; set; }
    }

    public sealed class ProductionDailyTrendDto
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = string.Empty;
        public int ShiftId { get; set; }
        public string ShiftNamePt { get; set; } = string.Empty;
        public string ShiftNameJp { get; set; } = string.Empty;
        public double ProductionPercent { get; set; }
        public double RunningMinutes { get; set; }
        public double StoppedMinutes { get; set; }
        public double InactiveMinutes { get; set; }
        public double ErrorMinutes { get; set; }
    }

    public sealed class ProductionAreaHistoryDto
    {
        public int? LocalId { get; set; }
        public string LocalNamePt { get; set; } = string.Empty;
        public string LocalNameJp { get; set; } = string.Empty;
        public List<ProductionDailyTrendDto> Days { get; } = new();
    }

    public sealed class ProductionOperatorRankingDto
    {
        public string OperatorCodigoFJ { get; set; } = string.Empty;
        public string OperatorNamePt { get; set; } = string.Empty;
        public string OperatorNameJp { get; set; } = string.Empty;
        public double EstimatedRunningMinutes { get; set; }
        public double EstimatedKadouritsuPercent { get; set; }
        public int FullCoverageDays { get; set; }
        public int PartialCoverageDays { get; set; }
        public List<string> LocalNamesPt { get; set; } = new();
        public List<string> LocalNamesJp { get; set; } = new();
    }

    public sealed class ProductionOperatorHistoryEntryDto
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = string.Empty;
        public int? LocalId { get; set; }
        public string LocalNamePt { get; set; } = string.Empty;
        public string LocalNameJp { get; set; } = string.Empty;
        public double RunningMinutes { get; set; }
        public double StoppedMinutes { get; set; }
        public double InactiveMinutes { get; set; }
        public double ErrorMinutes { get; set; }
        public double EligibleMinutes { get; set; }
        public double KadouritsuPercent { get; set; }
        public string CoverageMode { get; set; } = "full";
        public bool IsPartialCoverage { get; set; }
        public double EffectiveMinutes { get; set; }
        public double PlannedMinutes { get; set; }
    }
}
