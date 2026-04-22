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

        // ============================
        // SOBRA DE PECA
        // ============================
        public string date { get; set; }
        public string lote { get; set; }
        public decimal tanjuu { get; set; }
        public decimal pesoGramas { get; set; }
        public decimal quantidade { get; set; }
        public int machineId { get; set; }
        public int shainId { get; set; }
        public string observacao { get; set; }
        public string item { get; set; }

        // ============================
        // OPERATORS
        // ============================
        public string codigoFJ { get; set; }
        public string nameRomanji { get; set; }
        public string nameNihongo { get; set; }
        public int groupId { get; set; }
        public string startDate { get; set; }
        public bool hasEndDate { get; set; }
        public string endDate { get; set; }
        public bool trainer { get; set; }
        public bool status { get; set; }
        public bool isLeader { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string birthDate { get; set; }

        // ============================
        // ACCESS CONTROL
        // ============================
        public string login { get; set; }
        public string name { get; set; }
        public int accessLevel { get; set; }
        public string password { get; set; }
        public string confirmPassword { get; set; }

        // ============================
        // TASKS
        // ============================
        public string dueDate { get; set; }
        public string leaderCodigoFJ { get; set; }
        public string taskStatus { get; set; }

        // ============================
        // ANEXOS DO EDITAR
        // ============================
        public List<AttachmentExistingDto> existingAttachments { get; set; }
        public List<AttachmentNewDto> newAttachments { get; set; }
    }

    public class AttachmentItem
    {
        public string fileName { get; set; }
        public string base64 { get; set; }
    }

    public class AttachmentExistingDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }

    public class AttachmentNewDto
    {
        public string fileName { get; set; }
        public string base64 { get; set; }
    }

}
