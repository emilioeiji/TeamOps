// Project: TeamOps.Core
// File: Entities/HikitsuguiCorrection.cs
namespace TeamOps.Core.Entities
{
    public class HikitsuguiCorrection
    {
        public int Id { get; set; }
        public int HikitsuguiId { get; set; }
        public DateTime Date { get; set; }
        public string CorrectorCodigoFJ { get; set; } = "";
        public string Correction { get; set; } = "";
    }
}