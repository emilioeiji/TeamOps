namespace TeamOps.Core.Entities
{
    public class HikitsuguiListItem
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string CategoryName { get; set; } = "";
        public string CreatorCodigoFJ { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsRead { get; set; }

    }
}
