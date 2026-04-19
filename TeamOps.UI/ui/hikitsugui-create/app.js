// =========================================================
// Função padrão para enviar mensagens ao C#
// =========================================================
function send(action, extra = {}) {
    window.chrome.webview.postMessage({
        action,
        ...extra
    });
}

// =========================================================
// Ao carregar a página
// =========================================================
window.addEventListener("DOMContentLoaded", () => {
    send("load");

    document.getElementById("btnSalvar").addEventListener("click", salvar);
    document.getElementById("btnCancelar").addEventListener("click", () => send("cancel"));

    document.getElementById("fileUpload").addEventListener("change", handleFiles);

    initEditorToolbar();
});

// =========================================================
// Receber mensagens do C#
// =========================================================
window.chrome.webview.addEventListener("message", e => {
    const msg = e.data;

    switch (msg.type) {

        case "filters":
            preencherFiltros(msg);
            break;

        case "saved":
            alert("Hikitsugui registrado com sucesso!");
            send("cancel");
            break;
    }
});

// =========================================================
// Preencher selects e campos fixos
// =========================================================
function preencherFiltros(data) {

    // -----------------------------
    // DATA: mostrar só a data
    // -----------------------------
    const now = new Date();
    const yyyy = now.getFullYear();
    const mm = String(now.getMonth() + 1).padStart(2, '0');
    const dd = String(now.getDate()).padStart(2, '0');
    document.getElementById("txtDate").value = `${yyyy}-${mm}-${dd}`;

    // -----------------------------
    // CRIADOR: mostrar o nome
    // -----------------------------
    document.getElementById("txtCreator").value = data.creatorName;

    // -----------------------------
    // SHIFT: carregar e selecionar automaticamente
    // -----------------------------
    fillSelect("shiftId", data.shifts, "Id", "NamePt");
    document.getElementById("shiftId").value = data.shiftId;

    // -----------------------------
    // DEMAIS SELECTS
    // -----------------------------
    fillSelect("categoryId", data.categories, "Id", "NamePt", true);
    fillSelect("equipId", data.equipments, "Id", "NamePt", true);
    fillSelect("localId", data.locals, "Id", "NamePt", true);
    fillSelect("sectorId", data.sectors, "Id", "NamePt", true);
}

function fillSelect(id, list, valueField, textField, allowZero = false) {
    const sel = document.getElementById(id);
    sel.innerHTML = "";

    if (allowZero)
        sel.innerHTML += `<option value="0">Selecione...</option>`;

    list.forEach(item => {
        sel.innerHTML += `<option value="${item[valueField]}">${item[textField]}</option>`;
    });
}

// =========================================================
// Upload de anexos (base64)
// =========================================================
let attachments = [];

async function handleFiles(ev) {
    const list = document.getElementById("fileList");

    for (const file of ev.target.files) {
        const base64 = await toBase64(file);

        attachments.push({
            fileName: file.name,
            base64
        });
    }

    renderAttachmentList();
    ev.target.value = "";
}

function renderAttachmentList() {
    const list = document.getElementById("fileList");
    list.innerHTML = "";

    attachments.forEach((file, index) => {
        const li = document.createElement("li");
        li.innerHTML = `<span>${file.fileName}</span>`;

        const btn = document.createElement("button");
        btn.textContent = "Excluir";

        btn.onclick = () => {
            attachments.splice(index, 1);
            renderAttachmentList();
        };

        li.appendChild(btn);
        list.appendChild(li);
    });
}

function toBase64(file) {
    return new Promise(resolve => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result.split(",")[1]);
        reader.readAsDataURL(file);
    });
}

// =========================================================
// Tool Bar
// =========================================================
function initEditorToolbar() {
    const toolbar = document.querySelector(".editor-toolbar");
    const editor = document.getElementById("txtDescricao");

    toolbar.addEventListener("click", (ev) => {
        const btn = ev.target.closest("button");
        if (!btn) return;

        const cmd = btn.getAttribute("data-cmd");
        if (!cmd) return;

        editor.focus();
        document.execCommand(cmd, false, null);
    });

    document.getElementById("btnClearFormat").addEventListener("click", () => {
        const text = editor.innerText; // só texto puro
        editor.innerHTML = text ? `<p>${text}</p>` : "";
    });
}

// =========================================================
// Gerar data + hora atual para salvar
// =========================================================
function getFullDateTime() {
    const dateOnly = document.getElementById("txtDate").value; // yyyy-mm-dd
    const now = new Date();

    const hh = String(now.getHours()).padStart(2, '0');
    const mi = String(now.getMinutes()).padStart(2, '0');
    const ss = String(now.getSeconds()).padStart(2, '0');

    return `${dateOnly} ${hh}:${mi}:${ss}`;
}

// =========================================================
// Validação
// =========================================================
function validar() {

    if (Number(document.getElementById("categoryId").value) === 0) {
        alert("Selecione uma categoria.");
        return false;
    }

    if (Number(document.getElementById("equipId").value) === 0) {
        alert("Selecione um equipamento.");
        return false;
    }

    if (Number(document.getElementById("localId").value) === 0) {
        alert("Selecione um local.");
        return false;
    }

    if (Number(document.getElementById("sectorId").value) === 0) {
        alert("Selecione um setor.");
        return false;
    }

    const editor = document.getElementById("txtDescricao");
    const plain = editor.innerText.trim();
    if (!plain) {
        alert("Informe a descrição.");
        return false;
    }

    return true;
}

// =========================================================
// Salvar
// =========================================================
function salvar() {

    if (!validar()) return;

    const publico = document.querySelector("input[name='publico']:checked").value;

    const payload = {
        action: "save",
        date: getFullDateTime(),
        shiftId: Number(document.getElementById("shiftId").value),
        reasonId: Number(document.getElementById("categoryId").value),
        equipId: Number(document.getElementById("equipId").value),
        localId: Number(document.getElementById("localId").value),
        sectorId: Number(document.getElementById("sectorId").value),
        publico,
        text: document.getElementById("txtDescricao").innerHTML,
        attachments
    };

    send("save", payload);
}
