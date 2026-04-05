namespace TeamOps.Core.Entities
{
    public class OperatorSchedule
    {
        public int Id { get; set; }
        public string CodigoFJ { get; set; } = string.Empty;
        public int SectorId { get; set; }
        public int LocalId { get; set; }
        public int ShiftId { get; set; }
        public DateTime ScheduleDate { get; set; }
        public DateTime ImportedAt { get; set; }
    }
}
