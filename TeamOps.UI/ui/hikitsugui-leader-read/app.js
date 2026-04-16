// ======================================================
// Variáveis globais
// ======================================================
let currentUserAccessLevel = 0;

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

    initReplyEditor();

    document.querySelectorAll("input[name='publico']").forEach(radio => {
        radio.addEventListener("change", () => {
            document.getElementById("btnBuscar").click();
        });
    });

    document.getElementById("txtSearch").addEventListener("input", () => {
        document.getElementById("btnBuscar").click();
    });

    // Botão Buscar
    document.getElementById("btnBuscar").addEventListener("click", () => {

        const publico = document.querySelector("input[name='publico']:checked").value;

        // Bloqueio (camada 2)
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

    // Botão fechar modal
    document.getElementById("btnFecharModal").addEventListener("click", closeModal);

    // Botão enviar reply
    document.getElementById("btnEnviarReply").addEventListener("click", () => {
        const text = document.getElementById("replyEditor").innerHTML;
        const id = document.getElementById("replyEditor").dataset.hikitsuguiId;

        if (!text.trim()) return;

        send("reply", { id: Number(id), text });
    });
});

function initReplyEditor() {
    const toolbar = document.querySelector(".editor-toolbar");
    const editor = document.getElementById("replyEditor");

    toolbar.addEventListener("click", (ev) => {
        const btn = ev.target.closest("button");
        if (!btn) return;

        const cmd = btn.getAttribute("data-cmd");
        if (!cmd) return;

        editor.focus();
        document.execCommand(cmd, false, null);
    });

    document.getElementById("btnClearReplyFormat").addEventListener("click", () => {
        const text = editor.innerText;
        editor.innerHTML = text ? `<p>${text}</p>` : "";
    });
}

// ======================================================
// Receber mensagens do C#
// ======================================================
window.chrome.webview.addEventListener("message", e => {
    const msg = e.data;
    console.log("Mensagem do C#:", msg);

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
            break;

        case "hikitsugui_for_leader":
            renderTable(msg.data);
            break;

        case "hikitsugui_by_id":
            openModal(msg.data[0]);
            break;

        case "replies":
            renderReplies(msg.data);

            // limpar campo após salvar e atualizar lista
            document.getElementById("replyEditor").innerHTML = "";
            break;

        case "hikitsugui_by_id":
            console.log("PREVIEW ROW:", msg.data[0]);
            openModal(msg.data[0]);
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
                <td class="border p-2 text-center">
                    <button class="btn-primary" onclick="preview(${r.Id})">Ver</button>
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
    `;
    
    document.getElementById("descHtml").innerHTML = row.DescriptionHtml ?? "";

    // GARANTIR QUE O ELEMENTO EXISTE ANTES DE SETAR O DATASET
    const replyEditor = document.getElementById("replyEditor");
    replyEditor.dataset.hikitsuguiId = row.Id;
    replyEditor.innerHTML = ""; // limpa ao abrir

    // Carregar replies
    send("select_replies", { id: row.Id });
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
