// Project: TeamOps.Core
// File: Entities/HikitsuguiRead.cs
namespace TeamOps.Core.Entities
{
    public class HikitsuguiRead
    {
        public int Id { get; set; }
        public int HikitsuguiId { get; set; }
        public string ReaderCodigoFJ { get; set; } = "";
        public DateTime ReadAt { get; set; }
    }
}