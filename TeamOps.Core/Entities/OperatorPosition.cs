namespace TeamOps.Core.Entities
{
    public sealed class OperatorPosition
    {
        public int Id { get; set; }
        public int SectorId { get; set; }
        public int LocalId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
