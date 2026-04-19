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
        public int equipId { get; set; }          // FILTRO
        public int sectorIdFilter { get; set; }   // FILTRO (renomeado)
        public int localIdFilter { get; set; }    // FILTRO
        public string search { get; set; }

        // ============================
        // CAMPOS DO MODAL DE EDIÇÃO
        // ============================
        public int categoryId { get; set; }
        public int equipmentId { get; set; }      // ← AGORA EXISTE
        public int localId { get; set; }          // ← AGORA É DO MODAL
        public int sectorId { get; set; }         // ← AGORA É DO MODAL
        public string description { get; set; }

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
