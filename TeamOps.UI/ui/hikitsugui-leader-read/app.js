console.log("JS CARREGOU");
// ======================================================
// Variáveis globais
// ======================================================
let currentUserAccessLevel = 0;
let editExistingAttachments = [];
let editNewAttachments = [];
let currentEditId = null;

const ACTION_ICONS = {
    view: `
        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false" xmlns="http://www.w3.org/2000/svg">
          <circle cx="10" cy="10" r="5.5" fill="none" stroke="currentColor" stroke-width="2"/>
          <path d="M13.8 13.8L19.5 19.5" fill="none" stroke="currentColor" stroke-width="2.6" stroke-linecap="round"/>
          <circle cx="8.3" cy="8.3" r="1.1" fill="currentColor" opacity="0.18"/>
        </svg>
    `,
    edit: `
        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
            <path d="M3 17.25V21h3.75L18.3 9.45l-3.75-3.75L3 17.25Z"></path>
            <path d="m14.55 5.7 3.75 3.75 1.15-1.15a1.32 1.32 0 0 0 0-1.9l-1.85-1.85a1.32 1.32 0 0 0-1.9 0L14.55 5.7Z"></path>
        </svg>
    `,
    delete: `
        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
            <path d="M9 3.75h6l.75 1.5H20v1.5H4V5.25h4.25L9 3.75Z"></path>
            <path d="M6.75 8.25h10.5l-.8 10.1A1.5 1.5 0 0 1 14.95 19.75h-5.9a1.5 1.5 0 0 1-1.49-1.4l-.81-10.1Z"></path>
        </svg>
    `
};

// ======================================================
// Função padrão para enviar mensagens ao C#
// ======================================================
function send(action, extra = {}) {
    window.chrome.webview.postMessage({
        action,
        ...extra
    });
}

// ======================================================
// Ao carregar a página
// ======================================================
window.addEventListener("DOMContentLoaded", () => {

    // Carrega dados iniciais
    send("load");
    console.log("LOAD DISPARADO");

    initReplyEditor();

    document.querySelectorAll("input[name='publico']").forEach(radio => {
        radio.addEventListener("change", () => {
            document.getElementById("btnBuscar").click();
        });
    });

    document.getElementById("txtSearch").addEventListener("input", () => {
        document.getElementById("btnBuscar").click();
    });

    // ✅ BOTÃO BUSCAR
    document.getElementById("btnBuscar").addEventListener("click", () => {

        const publico = document.querySelector("input[name='publico']:checked").value;

        if (publico === "masv" && currentUserAccessLevel < 3) {
            alert("Seu nível de acesso não permite visualizar MA/SV.");
            return;
        }

        const payload = {
            dtInicial: document.getElementById("dtInicial").value,
            dtFinal: document.getElementById("dtFinal").value,
            publico,
            shiftId: Number(document.getElementById("shiftId").value),
            operatorId: Number(document.getElementById("operatorId").value),
            reasonId: Number(document.getElementById("reasonId").value),
            typeId: 0,
            equipId: Number(document.getElementById("equipId").value),
            sectorId: Number(document.getElementById("sectorId").value),
            search: document.getElementById("txtSearch").value.trim()
        };

        send("filter", payload);
    });

    // -----------------------------
    // OUTROS EVENTOS
    // -----------------------------

    const btnFechar = document.getElementById("btnFecharModal");
    if (btnFechar) {
        btnFechar.addEventListener("click", closeModal);
    }

    const btnReply = document.getElementById("btnEnviarReply");
    if (btnReply) {
        btnReply.addEventListener("click", () => {
            const text = document.getElementById("replyEditor").innerHTML;
            const id = document.getElementById("replyEditor").dataset.hikitsuguiId;

            if (!text.trim()) return;

            send("reply", { id: Number(id), text });
        });
    }

    const upload = document.getElementById("editFileUpload");
    if (upload) {
        upload.addEventListener("change", async ev => {

            if (!ev.target.files || ev.target.files.length === 0) return;

            for (const file of ev.target.files) {
                const base64 = await toBase64(file);
                editNewAttachments.push({
                    fileName: file.name,
                    base64
                });
            }

            renderEditNewFiles();
        }); // ← FECHAMENTO CORRETO DO addEventListener
    } // ← FECHAMENTO CORRETO DO if(upload)

}); // ← FECHAMENTO CORRETO DO DOMContentLoaded


function initReplyEditor() {
    const toolbar = document.querySelector("#modal .editor-toolbar");
    const editor = document.getElementById("replyEditor");

    if (!toolbar || !editor) return; // 👈 ESSENCIAL

    toolbar.addEventListener("click", (ev) => {
        const btn = ev.target.closest("button");
        if (!btn) return;

        const cmd = btn.getAttribute("data-cmd");
        if (!cmd) return;

        editor.focus();
        document.execCommand(cmd, false, null);
    });

    document.getElementById("btnClearReplyFormat")?.addEventListener("click", () => {
        const text = editor.innerText;
        editor.innerHTML = text ? `<p>${text}</p>` : "";
    });
}

// ======================================================
// Receber mensagens do C#
// ======================================================
window.chrome.webview.addEventListener("message", e => {
    const msg = e.data;

    switch (msg.type) {

        // --------------------------------------------------------
        // RECEBE FILTROS + ACCESS LEVEL
        // --------------------------------------------------------
        case "filters":
            currentUserAccessLevel = msg.accessLevel; // ← ADICIONADO
            fillFilters(msg);
            applyAccessRules();                       // ← ADICIONADO
            document.querySelector("input[value='todos']").checked = true;
            document.getElementById("dtInicial").value = msg.dtInicial;
            document.getElementById("dtFinal").value = msg.dtFinal;
            fillSelect("editCategoria", msg.categories, "Id", "NamePt");
            fillSelect("editEquipamento", msg.equipments, "Id", "NamePt");
            fillSelect("editLocal", msg.locals, "Id", "NamePt");
            fillSelect("editSector", msg.sectors, "Id", "NamePt");
            break;

        case "hikitsugui_for_leader":
            renderTable(msg.data);
            closeEditModal();
            break;

        case "hikitsugui_by_id":
            openModal(msg.data[0]);
            break;

        case "replies":
            renderReplies(msg.data);

            // limpar campo após salvar e atualizar lista
            document.getElementById("replyEditor").innerHTML = "";
            break;

        case "hikitsugui_edit":
            console.log("msg.data:", msg.data);

            setTimeout(() => {
                openEditModal(msg.data[0]);              // Preenche selects e descrição
                renderEditAttachments(msg.attachments);  // Agora o DOM existe
                renderEditNewFiles();                    // Agora o DOM existe
            }, 50);

            break;

        case "attachments":
            renderAttachments(msg.data);
            break;

        case "error":
            console.error("C# ERROR:", msg.message);
            alert("Erro: " + msg.message);
            break;
    }
});

// ======================================================
// Aplicar regras de acesso (camada 1)
// ======================================================
function applyAccessRules() {
    const masvOption = document.querySelector("input[value='masv']")?.parentElement;

    if (currentUserAccessLevel < 3 && masvOption) {
        masvOption.style.display = "none"; // esconder MA/SV
    }
}

// ======================================================
// Preencher filtros
// ======================================================
function fillFilters(msg) {

    fillSelect("shiftId", msg.shifts, "Id", "NamePt");
    fillSelect("operatorId", msg.operators, "CodigoFJ", "NameRomanji");
    fillSelect("reasonId", msg.categories, "Id", "NamePt");
    fillSelect("equipId", msg.equipments, "Id", "NamePt");
    fillSelect("localId", msg.locals, "Id", "NamePt");
    fillSelect("sectorId", msg.sectors, "Id", "NamePt");
}

function fillSelect(id, list, valueField, textField) {
    const sel = document.getElementById(id);
    sel.innerHTML = `<option value="0">Todos</option>`;

    list.forEach(item => {
        sel.innerHTML += `<option value="${item[valueField]}">${item[textField]}</option>`;
    });
}

// ======================================================
// Renderizar tabela
// ======================================================
function renderTable(rows) {
    const tbody = document.getElementById("tblHikitsugui");
    tbody.innerHTML = "";

    rows.forEach(r => {

        const icon = r.IsRead
            ? `<span class="maru animate">○</span>`
            : `<span class="batsu animate" onclick="markRead(${r.Id})">×</span>`;

        const dataFormatada = r.Date?.split(" ")[0] ?? "";

        tbody.innerHTML += `
            <tr>
                <td class="border p-2">${icon}</td>
                <td class="border p-2 no-wrap">${r.Id}</td>
                <td class="border p-2 col-data no-wrap">${dataFormatada}</td>
                <td class="border p-2">${r.OperatorName}</td>
                <td class="border p-2">${r.Category ?? ""}</td>
                <td class="border p-2">${r.Equipment ?? ""}</td>
                <td class="border p-2">${r.Local ?? ""}</td>
                <td class="border p-2 col-setor no-wrap">${r.Sector ?? ""}</td>
                <td class="border p-2">
                    ${truncateHtmlPreservingFormat(r.DescriptionHtml, 120)}
                </td>
                <td class="border p-2 actions-cell">
                    <div class="action-buttons">
                        <button
                            type="button"
                            class="icon-btn icon-btn-view"
                            onclick="preview(${r.Id})"
                            title="Visualizar"
                            aria-label="Visualizar hikitsugui ${r.Id}">
                            ${ACTION_ICONS.view}
                        </button>
                        <button
                            type="button"
                            class="icon-btn icon-btn-edit"
                            onclick="openEdit(${r.Id})"
                            title="Editar"
                            aria-label="Editar hikitsugui ${r.Id}">
                            ${ACTION_ICONS.edit}
                        </button>
                        <button
                            type="button"
                            class="icon-btn icon-btn-delete"
                            onclick="deleteHikitsugui(${r.Id})"
                            title="Excluir"
                            aria-label="Excluir hikitsugui ${r.Id}">
                            ${ACTION_ICONS.delete}
                        </button>
                    </div>
                </td>
            </tr>
        `;
    });
}

function truncateHtmlPreservingFormat(html, maxChars = 120) {
    const div = document.createElement("div");
    div.innerHTML = html;

    let count = 0;

    function walk(node) {
        let clone = node.cloneNode(false);

        for (let child of node.childNodes) {
            if (count >= maxChars) break;

            if (child.nodeType === Node.TEXT_NODE) {
                const remaining = maxChars - count;
                const text = child.textContent;

                if (text.length <= remaining) {
                    clone.appendChild(document.createTextNode(text));
                    count += text.length;
                } else {
                    clone.appendChild(document.createTextNode(text.substring(0, remaining) + "..."));
                    count = maxChars;
                }
            } else if (child.nodeType === Node.ELEMENT_NODE) {
                const childClone = walk(child);
                if (childClone) clone.appendChild(childClone);
            }
        }

        return clone;
    }

    const truncated = walk(div);
    return truncated.innerHTML;
}

function renderAttachments(rows) {
    const list = document.getElementById("attachmentList");
    list.innerHTML = "";

    if (rows.length === 0) {
        list.innerHTML = `<i>Nenhum anexo</i>`;
        return;
    }

    rows.forEach(a => {
        list.innerHTML += `
        <div class="attach-item">
            <div class="attach-info">
                <span class="attach-icon">📎</span>
                <span class="attach-name">${a.FileName}</span>
            </div>

            <button class="attach-open"
                onclick="openAttachment('${a.FilePath.replace(/\\/g, "\\\\")}')">
                Abrir
            </button>
        </div>
    `;
    });
}

function renderEditAttachments(rows) {
    editExistingAttachments = rows || [];

    const list = document.getElementById("editExistingFiles");
    list.innerHTML = "";

    if (!rows || rows.length === 0) {
        list.innerHTML = `<i>Nenhum anexo existente</i>`;
        return;
    }

    rows.forEach((a, index) => {
        list.innerHTML += `
            <li>
                <span>${a.FileName}</span>
                <button class="btn-delete-file" onclick="removeExistingAttachment(${index})">Excluir</button>
            </li>
        `;
    });
}

function removeExistingAttachment(index) {
    editExistingAttachments.splice(index, 1);
    renderEditAttachments(editExistingAttachments);
}

function openAttachment(path) {
    send("open_attachment", { path });
}

function renderEditNewFiles() {
    const list = document.getElementById("editNewFiles");
    list.innerHTML = "";

    editNewAttachments.forEach((file, index) => {
        list.innerHTML += `
            <li>
                <span>${file.fileName}</span>
                <button class="btn-delete-file" onclick="removeNewEditAttachment(${index})">Excluir</button>
            </li>
        `;
    });
}

function removeNewEditAttachment(index) {
    editNewAttachments.splice(index, 1);
    renderEditNewFiles();
}

// ======================================================
// Marcar como lido (camada 2)
// ======================================================
function markRead(id) {

    const publico = document.querySelector("input[name='publico']:checked").value;

    if (publico === "masv" && currentUserAccessLevel < 3) {
        alert("Seu nível de acesso não permite visualizar MA/SV.");
        return;
    }

    send("mark_read", {
        id,
        dtInicial: document.getElementById("dtInicial").value,
        dtFinal: document.getElementById("dtFinal").value,
        publico,
        shiftId: Number(document.getElementById("shiftId").value),
        operatorId: Number(document.getElementById("operatorId").value),
        reasonId: Number(document.getElementById("reasonId").value),
        equipId: Number(document.getElementById("equipId").value),
        sectorId: Number(document.getElementById("sectorId").value),
        search: document.getElementById("txtSearch").value.trim()
    });
}

// ======================================================
// Abrir modal
// ======================================================
function preview(id) {
    send("preview", { id });
}

function openModal(row) {

    const modal = document.getElementById("modal");
    modal.classList.remove("hidden");

    const body = document.getElementById("modal-body");

    body.innerHTML = `
        <b>ID:</b> ${row.Id}<br>
        <b>Data:</b> ${row.Date.split(" ")[0]}<br>
        <b>Operador:</b> ${row.OperatorName}<br>
        <b>Categoria:</b> ${row.Category ?? ""}<br>
        <b>Equipamento:</b> ${row.Equipment ?? ""}<br>
        <b>Local:</b> ${row.Local ?? ""}<br>
        <b>Setor:</b> ${row.Sector ?? ""}<br><br>

        <b>Descrição:</b><br>
        <div id="descHtml"></div>
        <br><br>

        <h3>Respostas</h3>
        <div id="replyList"></div>

        <h3 class="mt-4">Anexos</h3>
        <div id="attachmentList" class="mt-2"></div>
    `;
    
    document.getElementById("descHtml").innerHTML = row.DescriptionHtml ?? "";

    // GARANTIR QUE O ELEMENTO EXISTE ANTES DE SETAR O DATASET
    const replyEditor = document.getElementById("replyEditor");

    if (replyEditor) {
        replyEditor.dataset.hikitsuguiId = row.Id;
        replyEditor.innerHTML = "";
    }

    // Aguarda DOM atualizar
    setTimeout(() => {
        send("select_replies", { id: row.Id });
        send("select_attachments", { id: row.Id });
    }, 50);
}

function openEditModal(row) {
    console.log("openEditModal row:", row);

    editNewAttachments = []; // só limpa novos

    currentEditId = row.Id;

    document.getElementById("editCategoria").value = row.CategoryId;
    document.getElementById("editEquipamento").value = row.EquipmentId ?? 0;
    document.getElementById("editLocal").value = row.LocalId ?? 0;
    document.getElementById("editSector").value = row.SectorId ?? 0;

    const editor = document.getElementById("editDescricao");
    editor.innerHTML = row.Description ?? "";

    document.getElementById("modalEdit").classList.remove("hidden");

    initEditEditor();
}

// ======================================================
// Renderizar replies
// ======================================================
function renderReplies(rows) {
    const list = document.getElementById("replyList");
    list.innerHTML = "";

    rows.forEach(r => {
        list.innerHTML += `
            <div class="border rounded p-2 mb-4">
                <b>${r.ResponderName}</b> (${r.Date})<br>
                ${r.Message}<br><br>
            </div>
        `;
    });
}

// ======================================================
// Fechar modal
// ======================================================
function closeModal() {
    document.getElementById("modal").classList.add("hidden");
    document.getElementById("replyEditor").innerHTML = "";
}

function closeEditModal() {
    document.getElementById("modalEdit").classList.add("hidden");
}

function openEdit(id) {
    send("load_for_edit", { id });
}

function saveEdit() {
    console.log("existingAttachments:", editExistingAttachments);
    console.log("newAttachments:", editNewAttachments);

    const payload = {
        id: currentEditId,

        categoryId: Number(document.getElementById("editCategoria").value),
        equipmentId: Number(document.getElementById("editEquipamento").value),
        localId: Number(document.getElementById("editLocal").value),
        sectorId: Number(document.getElementById("editSector").value),

        description: document.getElementById("editDescricao").innerHTML,

        // 🔥 CORREÇÃO AQUI
        existingAttachments: editExistingAttachments.map(x => ({
            FileName: x.FileName ?? x.fileName,
            FilePath: x.FilePath ?? x.filePath
        })),

        // Os novos anexos seguem o contrato esperado pelo C# (camelCase)
        newAttachments: editNewAttachments.map(x => ({
            fileName: x.fileName,
            base64: x.base64
        })),

        dtInicial: document.getElementById("dtInicial").value,
        dtFinal: document.getElementById("dtFinal").value,
        publico: document.querySelector("input[name='publico']:checked").value,
        shiftId: Number(document.getElementById("shiftId").value),
        operatorId: Number(document.getElementById("operatorId").value),
        reasonId: Number(document.getElementById("reasonId").value),
        equipId: Number(document.getElementById("equipId").value),
        sectorIdFilter: Number(document.getElementById("sectorId").value),
        search: document.getElementById("txtSearch").value.trim()
    };

    document.activeElement.blur();

    send("save_edit", payload);
}
function toBase64(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result.split(",")[1]);
        reader.onerror = reject;
        reader.readAsDataURL(file);
    });
}

function deleteHikitsugui(id) {
    if (!confirm("Tem certeza que deseja excluir?")) return;

    send("delete_hikitsugui", {
        id,
        dtInicial: document.getElementById("dtInicial").value,
        dtFinal: document.getElementById("dtFinal").value,
        publico: document.querySelector("input[name='publico']:checked").value,
        shiftId: Number(document.getElementById("shiftId").value),
        operatorId: Number(document.getElementById("operatorId").value),
        reasonId: Number(document.getElementById("reasonId").value),
        equipId: Number(document.getElementById("equipId").value),
        sectorId: Number(document.getElementById("sectorId").value),
        search: document.getElementById("txtSearch").value.trim()
    });
}

function initEditEditor() {
    const editor = document.getElementById("editDescricao");
    const toolbar = document.querySelector("#modalEdit .edit-toolbar");

    if (toolbar.dataset.initialized) return;
    toolbar.dataset.initialized = "true";

    toolbar.addEventListener("click", (ev) => {
        const btn = ev.target.closest("button");
        if (!btn) return;

        const cmd = btn.getAttribute("data-cmd");
        if (!cmd) return;

        editor.focus();
        document.execCommand(cmd, false, null);
    });

    document.getElementById("btnClearEditFormat").addEventListener("click", () => {
        const text = editor.innerText;
        editor.innerHTML = text ? `<p>${text}</p>` : "";
    });
}
