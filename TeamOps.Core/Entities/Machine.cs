namespace TeamOps.Core.Entities
{
    public class Machine
    {
        public int Id { get; set; }
        public string NamePt { get; set; } = "";
        public string NameJp { get; set; } = "";
        public string? MachineCode { get; set; }
        public string? LineCode { get; set; }
        public int? LocalId { get; set; }
        public int? SectorId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
