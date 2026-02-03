// Project: TeamOps.Core
// File: Entities/HikitsuguiResponse.cs
namespace TeamOps.Core.Entities
{

    public class HikitsuguiResponse
    {
        public int Id { get; set; }
        public int HikitsuguiId { get; set; }
        public DateTime Date { get; set; }
        public string ResponderCodigoFJ { get; set; } = "";
        public string Message { get; set; } = "";
    }
}