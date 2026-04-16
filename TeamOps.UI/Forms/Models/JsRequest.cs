namespace TeamOps.UI.Forms.Models
{
    public class JsRequest
    {
        // Ação enviada pelo app.js
        public string action { get; set; }

        // ID genérico usado em preview, reply, delete_reply, mark_read
        public int id { get; set; }

        // ============================
        // HIKITSUGUI
        // ============================
        public string text { get; set; }       // ← ESSENCIAL PARA REPLY
        public int parentId { get; set; }      // delete_reply
        public string dtInicial { get; set; }
        public string dtFinal { get; set; }
        public List<AttachmentItem> attachments { get; set; }

        // Filtros do Hikitsugui
        public string publico { get; set; }
        public int shiftId { get; set; }
        public int operatorId { get; set; }
        public int reasonId { get; set; }
        public int typeId { get; set; }
        public int equipId { get; set; }
        public int sectorId { get; set; }
        public string search { get; set; }
        public int localId { get; set; }

        // ============================
        // PAID LEAVE
        // ============================
        public string opCodigoFJ { get; set; }
        public string reqDate { get; set; }
        public string notes { get; set; }
        public int motivoId { get; set; }
    }
    public class AttachmentItem
    {
        public string fileName { get; set; }
        public string base64 { get; set; }
    }

}
