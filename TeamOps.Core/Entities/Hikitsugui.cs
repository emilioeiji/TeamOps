// Project: TeamOps.Core
// File: Entities/Hikitsugui.cs
namespace TeamOps.Core.Entities
{
    public class Hikitsugui
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int ShiftId { get; set; }
        public string CreatorCodigoFJ { get; set; } = "";
        public int CategoryId { get; set; }
        public int? EquipmentId { get; set; }
        public int? LocalId { get; set; }
        public bool ForLeaders { get; set; }
        public bool ForOperators { get; set; }
        public string Description { get; set; } = "";
        public string? AttachmentPath { get; set; }
    }
}