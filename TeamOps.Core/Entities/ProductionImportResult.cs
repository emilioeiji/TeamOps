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
        public List<string> Errors { get; } = new();
        public bool BatchExecuted { get; set; }
        public string BatchMessage { get; set; } = string.Empty;
    }
}
