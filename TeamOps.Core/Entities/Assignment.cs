// Project: TeamOps.Core
// File: Entities/Assignment.cs
namespace TeamOps.Core.Entities
{
    public class Assignment
    {
        public int Id { get; set; }
        public int GLId { get; set; }
        public int OperatorId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.Now;
    }
}
