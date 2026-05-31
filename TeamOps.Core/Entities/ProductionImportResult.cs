using System.Collections.Generic;

namespace TeamOps.Core.Entities
{
    public sealed class ProductionImportResult
    {
        public int FilesRead { get; set; }
        public int LinesRead { get; set; }
        public int Imported { get; set; }
        public int Ignored { get; set; }
        public int MachinesCreated { get; set; }
        public int PlanFilesRead { get; set; }
        public int PlanRowsImported { get; set; }
        public int PlanRowsIgnored { get; set; }
        public bool Ec2ImportAttempted { get; set; }
        public bool Ec2ImportSkipped { get; set; }
        public string Ec2ImportMessage { get; set; } = string.Empty;
        public int Ec2RowsRead { get; set; }
        public int Ec2RowsImported { get; set; }
        public int Ec2RowsIgnored { get; set; }
        public int Ec2AreaCount { get; set; }
        public int Ec2RunningCount { get; set; }
        public int Ec2StoppedCount { get; set; }
        public int Ec2IgnoredCount { get; set; }
        public List<string> Errors { get; } = new();
        public Dictionary<string, long> PerformanceMs { get; } = new();
        public bool BatchExecuted { get; set; }
        public string BatchMessage { get; set; } = string.Empty;
    }
}
