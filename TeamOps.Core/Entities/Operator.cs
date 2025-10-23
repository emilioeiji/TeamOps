namespace TeamOps.Core.Entities
{
    public class Operator
    {
        public string CodigoFJ { get; set; } = string.Empty;
        public string NameRomanji { get; set; } = string.Empty;
        public string NameNihongo { get; set; } = string.Empty;
        public int ShiftId { get; set; }
        public int GroupId { get; set; }
        public int SectorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool Trainer { get; set; }
        public bool Status { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
