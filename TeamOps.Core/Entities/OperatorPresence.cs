using System;

namespace TeamOps.Core.Entities
{
    public class OperatorPresence
    {
        public int Id { get; set; }

        public string CodigoFJ { get; set; } = "";

        public int SectorId { get; set; }

        public int LocalId { get; set; }

        public int ShiftId { get; set; }

        public DateTime Date { get; set; }

        // 🔥 Campos necessários para o mapa
        public string NameRomanji { get; set; } = "";

        public string NameNihongo { get; set; } = "";
    }
}
