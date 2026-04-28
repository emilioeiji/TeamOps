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
        public double ErrorMinutes { get; set; }
        public double InactiveMinutes { get; set; }
        public List<ProductionMachineSummaryDto> Machines { get; } = new();
        public List<ProductionRankingItemDto> Ranking { get; } = new();
        public List<ProductionTimelineRowDto> Timeline { get; } = new();
    }

    public sealed class ProductionMachineSummaryDto
    {
        public int MachineId { get; set; }
        public string MachineCode { get; set; } = string.Empty;
        public string MachineNamePt { get; set; } = string.Empty;
        public string MachineNameJp { get; set; } = string.Empty;
        public int? SectorId { get; set; }
        public string SectorNamePt { get; set; } = string.Empty;
        public string SectorNameJp { get; set; } = string.Empty;
        public int? LocalId { get; set; }
        public string LocalNamePt { get; set; } = string.Empty;
        public string LocalNameJp { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string RecipeName { get; set; } = string.Empty;
        public string LotNo { get; set; } = string.Empty;
        public DateTime? LastUpdate { get; set; }
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
        public string MachineCode { get; set; } = string.Empty;
        public string MachineNamePt { get; set; } = string.Empty;
        public string MachineNameJp { get; set; } = string.Empty;
        public double StopMinutes { get; set; }
        public double ErrorMinutes { get; set; }
        public double TotalImpactMinutes { get; set; }
    }

    public sealed class ProductionTimelineRowDto
    {
        public string MachineCode { get; set; } = string.Empty;
        public string MachineNamePt { get; set; } = string.Empty;
        public string MachineNameJp { get; set; } = string.Empty;
        public List<ProductionTimelineCellDto> Cells { get; } = new();
    }

    public sealed class ProductionTimelineCellDto
    {
        public string TimeLabel { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public int StatusCode { get; set; }
        public string CssClass { get; set; } = string.Empty;
    }
}
