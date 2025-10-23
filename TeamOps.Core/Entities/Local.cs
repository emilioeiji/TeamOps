namespace TeamOps.Core.Entities
{
    public class Local
    {
        public int Id { get; set; }
        public string NamePt { get; set; } = "";
        public string NameJp { get; set; } = "";

        // FK para Sector
        public int SectorId { get; set; }
    }
}
