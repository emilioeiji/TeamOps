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
        public int DadLinesRead { get; set; }
        public int DadRowsParsed { get; set; }
        public int DadRowsImported { get; set; }
        public int DadRowsIgnored { get; set; }
        public int DadMachinesFound { get; set; }
        public int DadMachinesImported { get; set; }
        public int DadMachinesWithRunningEvents { get; set; }
        public int DadMachinesWithZeroRunningEvents { get; set; }
        public int DadLinkErrors { get; set; }
        public int PlanFilesRead { get; set; }
        public int PlanRowsImported { get; set; }
        public int PlanRowsIgnored { get; set; }
        public bool CleanupPerformed { get; set; }
        public int CleanupEventsDeleted { get; set; }
        public int CleanupCurrentStatusesDeleted { get; set; }
        public string CleanupMessage { get; set; } = string.Empty;
        public bool Ec2ImportAttempted { get; set; }
        public bool Ec2ImportSkipped { get; set; }
        public string Ec2ImportMessage { get; set; } = string.Empty;
        public string Ec2FilePath { get; set; } = string.Empty;
        public string Ec2ResolvedFullPath { get; set; } = string.Empty;
        public bool Ec2FileExists { get; set; }
        public long Ec2FileSize { get; set; }
        public string Ec2FileLastWriteTime { get; set; } = string.Empty;
        public string Ec2EncodingDetected { get; set; } = string.Empty;
        public string Ec2DelimiterDetected { get; set; } = string.Empty;
        public string Ec2RawLinePreview { get; set; } = string.Empty;
        public string Ec2DecodedLinePreview { get; set; } = string.Empty;
        public bool Ec2ContainsReplacementChar { get; set; }
        public string Ec2FirstLinePreview { get; set; } = string.Empty;
        public string Ec2HeaderLinePreview { get; set; } = string.Empty;
        public string Ec2FirstDataLinePreview { get; set; } = string.Empty;
        public int Ec2RowsRead { get; set; }
        public int Ec2RowsCandidate { get; set; }
        public int Ec2RowsImported { get; set; }
        public int Ec2RowsIgnored { get; set; }
        public int Ec2IgnoredByEmptyLine { get; set; }
        public int Ec2IgnoredByNotAreaBlock { get; set; }
        public int Ec2IgnoredByTooFewColumns { get; set; }
        public int Ec2IgnoredByMissingMachine { get; set; }
        public int Ec2IgnoredByInvalidMachineCode { get; set; }
        public int Ec2IgnoredByHeaderOrSummaryLine { get; set; }
        public int Ec2IgnoredByInvalidStatus { get; set; }
        public int Ec2IgnoredByInvalidTime { get; set; }
        public int Ec2IgnoredByUnknownFormat { get; set; }
        public int Ec2AreaCount { get; set; }
        public int Ec2RunningCount { get; set; }
        public int Ec2StoppedCount { get; set; }
        public int Ec2IgnoredCount { get; set; }
        public List<string> Ec2DiscardSamples { get; } = new();
        public List<string> Ec2ImportedSamples { get; } = new();
        public Dictionary<string, int> DadEventsByMachine { get; } = new(System.StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> DadRunningEventsByMachine { get; } = new(System.StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> DadIgnoredByMachine { get; } = new(System.StringComparer.OrdinalIgnoreCase);
        public List<string> DadDiagnostics { get; } = new();
        public List<string> Errors { get; } = new();
        public Dictionary<string, long> PerformanceMs { get; } = new();
        public bool BatchExecuted { get; set; }
        public string BatchMessage { get; set; } = string.Empty;
    }
}
