namespace TeamOps.Core.Entities
{
    public class SobraDePeca
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public int TurnoId { get; set; }
        public string OperadorId { get; set; }
        public decimal Tanjuu { get; set; }
        public decimal PesoGramas { get; set; }
        public decimal Quantidade { get; set; }
        public int MachineId { get; set; }
        public int ShainId { get; set; }
        public string Lote { get; set; }
        public string Observacao { get; set; }
        public string Lider { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}