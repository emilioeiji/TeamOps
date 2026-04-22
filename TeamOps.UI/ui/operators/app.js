let rows = [];
let shifts = [];
let groups = [];
let sectors = [];
let editingCodigoFJ = "";
let currentModalMode = "create";

window.addEventListener("DOMContentLoaded", () => {
    bindEvents();
    send("load");
});

function bindEvents() {
    document.getElementById("searchInput").addEventListener("input", applyFilters);
    document.getElementById("toggleShowInactive").addEventListener("change", applyFilters);
    document.getElementById("btnHeaderNovo").addEventListener("click", openCreateModal);
    document.getElementById("btnSalvarModal").addEventListener("click", submitModal);
    document.getElementById("btnCancelarModal").addEventListener("click", closeModal);
    document.getElementById("btnCloseModal").addEventListener("click", closeModal);
    document.getElementById("modalBackdrop").addEventListener("click", closeModal);
    document.getElementById("chkHasEnd").addEventListener("change", toggleEndDate);
    document.getElementById("txtCodigoFJ").addEventListener("input", event => {
        event.target.value = event.target.value.toUpperCase();
    });
}

function send(action, extra = {}) {
    window.chrome.webview.postMessage({
        action,
        ...extra
    });
}

window.chrome.webview.addEventListener("message", event => {
    const msg = event.data;

    switch (msg.type) {
        case "init":
            hydrateScreen(msg);
            break;

        case "rows":
            rows = normalizeRows(msg.data || []);
            applyFilters();
            syncEditingOperator();
            break;

        case "saved":
            alert(msg.message || "Operador cadastrado com sucesso.");
            closeModal();
            break;

        case "updated":
            alert(msg.message || "Operador atualizado com sucesso.");
            closeModal();
            break;

        case "deleted":
            alert(msg.message || "Operador excluido com sucesso.");
            applyFilters();
            break;

        case "error":
            alert(msg.message || "Ocorreu um erro ao processar a solicitacao.");
            break;
    }
});

function hydrateScreen(payload) {
    shifts = payload.shifts || [];
    groups = payload.groups || [];
    sectors = payload.sectors || [];
    rows = normalizeRows(payload.rows || []);

    fillSelect("cmbShift", shifts, "Id", "NamePt");
    fillSelect("cmbGroup", groups, "Id", "NamePt");
    fillSelect("cmbSector", sectors, "Id", "NamePt");

    resetModalForm();
    applyFilters();
}

function normalizeRows(items) {
    return items.map(item => ({
        ...item,
        trainer: toBool(item.trainer),
        status: toBool(item.status),
        isLeader: toBool(item.isLeader)
    }));
}

function toBool(value) {
    return value === true || value === 1 || value === "1";
}

function fillSelect(id, items, valueField, textField) {
    const select = document.getElementById(id);
    select.innerHTML = `<option value="">Selecione...</option>`;

    items.forEach(item => {
        select.innerHTML += `<option value="${item[valueField]}">${escapeHtml(item[textField])}</option>`;
    });
}

function applyFilters() {
    const term = document.getElementById("searchInput").value.trim().toLowerCase();
    const showInactive = document.getElementById("toggleShowInactive").checked;

    const filtered = rows.filter(item => {
        if (!showInactive && !item.status) return false;
        if (!term) return true;

        const haystack = [
            item.codigoFJ,
            item.nameRomanji,
            item.nameNihongo,
            item.shiftName,
            item.groupName,
            item.sectorName,
            item.phone,
            item.address
        ]
            .filter(Boolean)
            .join(" ")
            .toLowerCase();

        return haystack.includes(term);
    });

    renderTable(filtered);
}

function renderTable(items) {
    const container = document.getElementById("tableContainer");

    if (!items || items.length === 0) {
        container.innerHTML = `<div class="empty-state">Nenhum operador encontrado.</div>`;
        return;
    }

    let html = `
        <table class="operators-table">
            <thead>
                <tr>
                    <th>Codigo FJ</th>
                    <th>Romanji</th>
                    <th>Nihongo</th>
                    <th>Turno</th>
                    <th>Grupo</th>
                    <th>Setor</th>
                    <th>Inicio</th>
                    <th>Termino</th>
                    <th>Status</th>
                    <th>Lider</th>
                    <th>Telefone</th>
                    <th>Acoes</th>
                </tr>
            </thead>
            <tbody>
    `;

    items.forEach(item => {
        const selectedClass = item.codigoFJ === editingCodigoFJ ? "is-selected" : "";
        const statusLabel = item.status ? "Ativo" : "Inativo";
        const leaderLabel = item.isLeader ? "Sim" : "Nao";

        html += `
            <tr class="${selectedClass}">
                <td>${escapeHtml(item.codigoFJ)}</td>
                <td>${escapeHtml(item.nameRomanji)}</td>
                <td>${escapeHtml(item.nameNihongo)}</td>
                <td>${escapeHtml(item.shiftName || "")}</td>
                <td>${escapeHtml(item.groupName || "")}</td>
                <td>${escapeHtml(item.sectorName || "")}</td>
                <td>${escapeHtml(item.startDate || "")}</td>
                <td>${escapeHtml(item.endDate || "-")}</td>
                <td><span class="status-pill ${item.status ? "status-active" : "status-inactive"}">${statusLabel}</span></td>
                <td>${leaderLabel}</td>
                <td>${escapeHtml(item.phone || "-")}</td>
                <td class="actions-cell">
                    <button class="row-btn row-btn-edit" type="button" data-edit="${escapeHtmlAttr(item.codigoFJ)}">Editar</button>
                    <button class="row-btn row-btn-delete" type="button" data-delete="${escapeHtmlAttr(item.codigoFJ)}">Excluir</button>
                </td>
            </tr>
        `;
    });

    html += `
            </tbody>
        </table>
    `;

    container.innerHTML = html;

    container.querySelectorAll("[data-edit]").forEach(button => {
        button.addEventListener("click", () => {
            openEditModal(button.dataset.edit);
        });
    });

    container.querySelectorAll("[data-delete]").forEach(button => {
        button.addEventListener("click", () => {
            deleteOperator(button.dataset.delete);
        });
    });
}

function openCreateModal() {
    currentModalMode = "create";
    resetModalForm();
    syncModeState();
    document.getElementById("operatorModal").classList.remove("hidden");
}

function openEditModal(codigoFJ) {
    const operator = rows.find(item => item.codigoFJ === codigoFJ);
    if (!operator) return;

    currentModalMode = "edit";
    editingCodigoFJ = operator.codigoFJ;

    document.getElementById("txtCodigoFJ").value = operator.codigoFJ || "";
    document.getElementById("txtCodigoFJ").readOnly = true;
    document.getElementById("txtRomanji").value = operator.nameRomanji || "";
    document.getElementById("txtNihongo").value = operator.nameNihongo || "";
    document.getElementById("cmbShift").value = String(operator.shiftId || "");
    document.getElementById("cmbGroup").value = String(operator.groupId || "");
    document.getElementById("cmbSector").value = String(operator.sectorId || "");
    document.getElementById("dtStart").value = operator.startDate || "";
    document.getElementById("chkHasEnd").checked = !!operator.endDate;
    document.getElementById("dtEnd").value = operator.endDate || "";
    document.getElementById("chkTrainer").checked = operator.trainer;
    document.getElementById("chkStatus").checked = operator.status;
    document.getElementById("chkIsLeader").checked = operator.isLeader;
    document.getElementById("txtTelefone").value = operator.phone || "";
    document.getElementById("txtEndereco").value = operator.address || "";
    document.getElementById("dtNascimento").value = operator.birthDate || "";

    toggleEndDate();
    syncModeState();
    applyFilters();
    document.getElementById("operatorModal").classList.remove("hidden");
}

function syncEditingOperator() {
    if (!editingCodigoFJ || currentModalMode !== "edit") return;

    const refreshed = rows.find(item => item.codigoFJ === editingCodigoFJ);
    if (!refreshed) {
        closeModal();
        return;
    }

    if (!document.getElementById("operatorModal").classList.contains("hidden")) {
        openEditModal(editingCodigoFJ);
    }
}

function resetModalForm() {
    editingCodigoFJ = "";
    document.getElementById("txtCodigoFJ").value = "";
    document.getElementById("txtCodigoFJ").readOnly = false;
    document.getElementById("txtRomanji").value = "";
    document.getElementById("txtNihongo").value = "";
    document.getElementById("cmbShift").value = "";
    document.getElementById("cmbGroup").value = "";
    document.getElementById("cmbSector").value = "";
    document.getElementById("dtStart").value = "";
    document.getElementById("chkHasEnd").checked = false;
    document.getElementById("dtEnd").value = "";
    document.getElementById("chkTrainer").checked = false;
    document.getElementById("chkStatus").checked = true;
    document.getElementById("chkIsLeader").checked = false;
    document.getElementById("txtTelefone").value = "";
    document.getElementById("txtEndereco").value = "";
    document.getElementById("dtNascimento").value = "";

    toggleEndDate();
    syncModeState();
    applyFilters();
}

function closeModal() {
    document.getElementById("operatorModal").classList.add("hidden");
    currentModalMode = "create";
    resetModalForm();
}

function toggleEndDate() {
    const hasEnd = document.getElementById("chkHasEnd").checked;
    const endInput = document.getElementById("dtEnd");
    endInput.disabled = !hasEnd;

    if (!hasEnd) {
        endInput.value = "";
    }
}

function submitModal() {
    if (currentModalMode === "edit") {
        send("update", buildPayload());
        return;
    }

    send("save", buildPayload());
}

function deleteOperator(codigoFJ) {
    const targetCodigoFJ = String(codigoFJ || "").trim();
    if (!targetCodigoFJ) {
        alert("Selecione um operador para excluir.");
        return;
    }

    if (!confirm(`Deseja realmente excluir o operador ${targetCodigoFJ}?`)) {
        return;
    }

    send("delete", { codigoFJ: targetCodigoFJ });
}

function buildPayload() {
    return {
        codigoFJ: document.getElementById("txtCodigoFJ").value.trim().toUpperCase(),
        nameRomanji: document.getElementById("txtRomanji").value.trim(),
        nameNihongo: document.getElementById("txtNihongo").value.trim(),
        shiftId: Number(document.getElementById("cmbShift").value || 0),
        groupId: Number(document.getElementById("cmbGroup").value || 0),
        sectorId: Number(document.getElementById("cmbSector").value || 0),
        startDate: document.getElementById("dtStart").value,
        hasEndDate: document.getElementById("chkHasEnd").checked,
        endDate: document.getElementById("dtEnd").value,
        trainer: document.getElementById("chkTrainer").checked,
        status: document.getElementById("chkStatus").checked,
        isLeader: document.getElementById("chkIsLeader").checked,
        phone: document.getElementById("txtTelefone").value.trim(),
        address: document.getElementById("txtEndereco").value.trim(),
        birthDate: document.getElementById("dtNascimento").value
    };
}

function syncModeState() {
    const isEdit = currentModalMode === "edit" && !!editingCodigoFJ;
    document.getElementById("formTitle").textContent = isEdit ? "Editar Operador" : "Novo Operador";
    document.getElementById("formSubtitle").textContent = isEdit
        ? `Ajuste os dados do operador ${editingCodigoFJ}.`
        : "Preencha os dados para cadastrar um novo operador.";
    document.getElementById("btnSalvarModal").textContent = isEdit ? "Salvar Alteracoes" : "Cadastrar";
}

function escapeHtml(value) {
    return String(value ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/\"/g, "&quot;")
        .replace(/'/g, "&#39;");
}

function escapeHtmlAttr(value) {
    return escapeHtml(value);
}
