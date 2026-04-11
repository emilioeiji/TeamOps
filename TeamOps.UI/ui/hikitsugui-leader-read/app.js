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
        const text = document.getElementById("replyText").value;
        const id = document.getElementById("replyText").dataset.hikitsuguiId;

        if (!text.trim()) return;

        send("reply", { id: Number(id), text });
    });
});

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
            const replyBox = document.getElementById("replyText");
            replyBox.value = "";
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
            ? `<span class="maru">○</span>`
            : `<span class="batsu" onclick="markRead(${r.Id})">×</span>`;

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
                <td class="border p-2">${r.Description}</td>
                <td class="border p-2 text-center">
                    <button class="btn-primary" onclick="preview(${r.Id})">Ver</button>
                </td>
            </tr>
        `;
    });
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
        sectorId: Number(document.getElementById("sectorId").value)
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
        ${row.Description}<br><br>

        <h3>Respostas</h3>
        <div id="replyList"></div>
    `;

    // GARANTIR QUE O ELEMENTO EXISTE ANTES DE SETAR O DATASET
    const replyBox = document.getElementById("replyText");
    replyBox.dataset.hikitsuguiId = row.Id;

    console.log("ID SALVO NO DATASET:", replyBox.dataset.hikitsuguiId);

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
    document.getElementById("replyText").value = "";
}
