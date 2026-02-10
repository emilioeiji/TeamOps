namespace TeamOps.Core.Entities
{
    public class PR
    {
        public int Id { get; set; }

        public int SetorId { get; set; }
        public int CategoriaId { get; set; }
        public int PrioridadeId { get; set; }

        public string Titulo { get; set; } = "";
        public string NomeArquivo { get; set; } = "";

        public DateTime DataEmissao { get; set; }
        public DateTime? DataRetornoHiru { get; set; }
        public DateTime? DataRetornoYakin { get; set; }

        public string AutorCodigoFJ { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}
