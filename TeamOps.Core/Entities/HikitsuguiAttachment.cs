// Project: TeamOps.Core
// File: Entities/HikitsuguiAttachment.cs
namespace TeamOps.Core.Entities
{
    public class HikitsuguiAttachment
    {
        public int Id { get; set; }
        public int HikitsuguiId { get; set; }
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}