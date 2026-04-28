namespace TeamOps.Core.Entities
{
    public sealed class MachineCurrentStatus
    {
        public int MachineId { get; set; }
        public string MachineCode { get; set; } = string.Empty;
        public string LineCode { get; set; } = string.Empty;
        public int? LocalId { get; set; }
        public int? SectorId { get; set; }
        public string RecipeName { get; set; } = string.Empty;
        public string LotNo { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string InternalState { get; set; } = string.Empty;
        public DateTime EventDateTime { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
