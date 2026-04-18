namespace TeamOps.UI.Forms.Models
{
    public class JsRequest
    {
        // Ação enviada pelo app.js
        public string action { get; set; }

        // ID genérico usado em preview, reply, delete_reply, mark_read, edit, delete
        public int id { get; set; }

        // ============================
        // HIKITSUGUI - CAMPOS GERAIS
        // ============================
        public string text { get; set; }       // reply
        public int parentId { get; set; }      // delete_reply
        public string dtInicial { get; set; }
        public string dtFinal { get; set; }
        public List<AttachmentItem> attachments { get; set; }
        public string path { get; set; }

        // ============================
        // FILTROS DO HIKITSUGUI
        // ============================
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
        // CAMPOS NOVOS PARA EDIÇÃO
        // ============================
        public int categoryId { get; set; }     // categoria editada
        public string description { get; set; } // descrição editada

        // ============================
        // PAID LEAVE (já existente)
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
