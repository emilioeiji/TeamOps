namespace TeamOps.Core.Entities
{
    public class FollowUp
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public int ShiftId { get; set; }
        public string OperatorCodigoFJ { get; set; } = "";
        public string ExecutorCodigoFJ { get; set; } = "";
        public string? WitnessCodigoFJ { get; set; }

        public int ReasonId { get; set; }
        public int TypeId { get; set; }
        public int LocalId { get; set; }
        public int EquipmentId { get; set; }
        public int SectorId { get; set; }

        public string Description { get; set; } = "";
        public string Guidance { get; set; } = "";

        // ---------------------------------------------------------
        // Campos extras para JOINs (opcional, mas MUITO úteis)
        // ---------------------------------------------------------
        public string? OperatorName { get; set; }
        public string? ExecutorName { get; set; }
        public string? WitnessName { get; set; }

        public string? ReasonName { get; set; }
        public string? TypeName { get; set; }
        public string? LocalName { get; set; }
        public string? EquipmentName { get; set; }
        public string? SectorName { get; set; }
        public string? ShiftName { get; set; }
    }
}
