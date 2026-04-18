using System;

namespace TeamOps.Core.Entities
{
    public class SystemLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserFJ { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int? TargetId { get; set; }
        public string? Details { get; set; }
    }
}
