let rows = [];
let shifts = [];
let leaders = [];
let statuses = [];
let currentModalMode = "create";
let editingTaskId = 0;

window.addEventListener("DOMContentLoaded", () => {
    bindEvents();
    send("load");
});

function bindEvents() {
    document.getElementById("searchInput").addEventListener("input", applyFilters);
    document.getElementById("statusFilter").addEventListener("change", applyFilters);
    document.getElementById("toggleShowClosed").addEventListener("change", applyFilters);
    document.getElementById("btnHeaderNovo").addEventListener("click", openCreateModal);
    document.getElementById("btnSaveModal").addEventListener("click", submitModal);
    document.getElementById("btnCancelModal").addEventListener("click", closeModal);
    document.getElementById("btnCloseModal").addEventListener("click", closeModal);
    document.getElementById("modalBackdrop").addEventListener("click", closeModal);
    document.getElementById("cmbShift").addEventListener("change", event => {
        updateLeaderOptions(event.target.value, "");
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
            renderMetrics(rows);
            applyFilters();
            syncEditingTask();
            break;

        case "task_details":
            if (Number(msg.id) === Number(editingTaskId)) {
                renderHistory(msg.history || []);
            }
            break;

        case "saved":
            alert(msg.message || "Task cadastrada com sucesso.");
            closeModal();
            break;

        case "updated":
            alert(msg.message || "Task atualizada com sucesso.");
            closeModal();
            break;

        case "status_changed":
            alert(msg.message || "Status da task atualizado com sucesso.");
            break;

        case "error":
            alert(msg.message || "Ocorreu um erro ao processar a task.");
            break;
    }
});

function hydrateScreen(payload) {
    rows = normalizeRows(payload.rows || []);
    shifts = payload.shifts || [];
    leaders = payload.leaders || [];
    statuses = payload.statuses || [];

    fillSelect("cmbShift", shifts, "Id", "NamePt");
    fillSelect("cmbTaskStatus", statuses, "value", "label");
    fillSelect("statusFilter", statuses, "value", "label", "Todos");

    document.getElementById("screenSubtitle").textContent =
        `Responsavel logado: ${payload.currentOperatorName || "-"} | Usuario: ${payload.currentUser || "-"}`;

    resetModalForm();
    renderMetrics(rows);
    applyFilters();
}

function normalizeRows(items) {
    return items.map(item => ({
        ...item,
        id: Number(item.id || 0),
        shiftId: Number(item.shiftId || 0),
        description: item.description || "",
        dueDate: item.dueDate || "",
        leaderCodigoFJ: item.leaderCodigoFJ || "",
        leaderName: item.leaderName || "",
        taskStatus: item.taskStatus || "pending",
        statusLabel: item.statusLabel || getStatusLabel(item.taskStatus),
        createdByName: item.createdByName || "",
        createdAt: item.createdAt || "",
        updatedAt: item.updatedAt || "",
        startedAt: item.startedAt || "",
        completedAt: item.completedAt || "",
        cancelledAt: item.cancelledAt || ""
    }));
}

function fillSelect(id, items, valueField, textField, firstLabel = "Selecione...") {
    const select = document.getElementById(id);
    select.innerHTML = `<option value="">${escapeHtml(firstLabel)}</option>`;

    items.forEach(item => {
        select.innerHTML += `<option value="${item[valueField]}">${escapeHtml(item[textField])}</option>`;
    });
}

function renderMetrics(items) {
    const openCount = items.filter(item => !isClosedStatus(item.taskStatus)).length;
    const progressCount = items.filter(item => item.taskStatus === "in_progress").length;
    const overdueCount = items.filter(item => isOverdue(item)).length;

    document.getElementById("metricOpen").textContent = String(openCount);
    document.getElementById("metricProgress").textContent = String(progressCount);
    document.getElementById("metricOverdue").textContent = String(overdueCount);
}

function applyFilters() {
    const term = document.getElementById("searchInput").value.trim().toLowerCase();
    const statusFilter = document.getElementById("statusFilter").value;
    const showClosed = document.getElementById("toggleShowClosed").checked;

    const filtered = rows.filter(item => {
        if (!showClosed && isClosedStatus(item.taskStatus)) return false;
        if (statusFilter && item.taskStatus !== statusFilter) return false;
        if (!term) return true;

        const haystack = [
            item.description,
            item.shiftName,
            item.leaderName,
            item.statusLabel,
            item.createdByName
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

    if (!items.length) {
        container.innerHTML = `<div class="empty-state">Nenhuma task encontrada.</div>`;
        return;
    }

    let html = `
        <table class="tasks-table">
            <thead>
                <tr>
                    <th>#</th>
                    <th>O que</th>
                    <th>Quando</th>
                    <th>Turno</th>
                    <th>Quem</th>
                    <th>Status</th>
                    <th>Atualizada</th>
                    <th>Acoes</th>
                </tr>
            </thead>
            <tbody>
    `;

    items.forEach(item => {
        const selectedClass = item.id === editingTaskId ? "is-selected" : "";
        const overdueClass = isOverdue(item) ? "due-overdue" : "";
        const dueLabel = isOverdue(item) ? "Atrasada" : "No prazo";
        const leaderName = item.leaderName || "Sem lider";

        html += `
            <tr class="${selectedClass}">
                <td>${item.id}</td>
                <td>
                    <div class="task-main">
                        <strong>${escapeHtml(truncate(item.description, 110))}</strong>
                        <small>Criada por ${escapeHtml(item.createdByName || "-")}</small>
                    </div>
                </td>
                <td>
                    <div class="due-stack">
                        <strong class="${overdueClass}">${escapeHtml(item.dueDate || "-")}</strong>
                        <small class="${overdueClass}">${dueLabel}</small>
                    </div>
                </td>
                <td>${escapeHtml(item.shiftName || "-")}</td>
                <td>${escapeHtml(leaderName)}</td>
                <td><span class="status-pill ${statusClass(item.taskStatus)}">${escapeHtml(item.statusLabel)}</span></td>
                <td>${escapeHtml(item.updatedAt || "-")}</td>
                <td class="actions-cell">
                    <button class="row-btn row-btn-edit" type="button" data-edit="${item.id}">Editar</button>
                    ${isClosedStatus(item.taskStatus)
                        ? ""
                        : `<button class="row-btn row-btn-finish" type="button" data-finish="${item.id}">Finalizar</button>`}
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
            openEditModal(Number(button.dataset.edit));
        });
    });

    container.querySelectorAll("[data-finish]").forEach(button => {
        button.addEventListener("click", () => {
            quickFinishTask(Number(button.dataset.finish));
        });
    });
}

function openCreateModal() {
    currentModalMode = "create";
    resetModalForm();
    syncModeState();
    document.getElementById("taskModal").classList.remove("hidden");
}

function openEditModal(taskId) {
    const task = rows.find(item => item.id === taskId);
    if (!task) return;

    currentModalMode = "edit";
    editingTaskId = task.id;

    document.getElementById("txtDescription").value = task.description || "";
    document.getElementById("dtDueDate").value = task.dueDate || "";
    document.getElementById("cmbShift").value = String(task.shiftId || "");
    updateLeaderOptions(task.shiftId, task.leaderCodigoFJ || "");
    document.getElementById("cmbTaskStatus").value = task.taskStatus || "pending";

    document.getElementById("metaCreatedBy").textContent = task.createdByName || "-";
    document.getElementById("metaCreatedAt").textContent = task.createdAt || "-";
    document.getElementById("metaUpdatedAt").textContent = task.updatedAt || "-";
    document.getElementById("metaCompletedAt").textContent = task.completedAt || task.cancelledAt || "-";

    renderHistory([]);
    syncModeState();
    applyFilters();
    document.getElementById("taskModal").classList.remove("hidden");
    send("load_details", { id: task.id });
}

function syncEditingTask() {
    if (!editingTaskId || currentModalMode !== "edit") return;

    const refreshed = rows.find(item => item.id === editingTaskId);
    if (!refreshed) {
        closeModal();
        return;
    }

    if (!document.getElementById("taskModal").classList.contains("hidden")) {
        openEditModal(editingTaskId);
    }
}

function quickFinishTask(taskId) {
    const task = rows.find(item => item.id === taskId);
    if (!task) return;

    if (!confirm(`Deseja finalizar a task #${task.id}?`)) {
        return;
    }

    send("change_status", {
        id: task.id,
        taskStatus: "completed"
    });
}

function resetModalForm() {
    editingTaskId = 0;
    document.getElementById("txtDescription").value = "";
    document.getElementById("dtDueDate").value = "";
    document.getElementById("cmbShift").value = "";
    document.getElementById("cmbTaskStatus").value = "pending";
    document.getElementById("metaCreatedBy").textContent = "-";
    document.getElementById("metaCreatedAt").textContent = "-";
    document.getElementById("metaUpdatedAt").textContent = "-";
    document.getElementById("metaCompletedAt").textContent = "-";
    updateLeaderOptions("", "");
    renderHistory([]);
    syncModeState();
    applyFilters();
}

function closeModal() {
    document.getElementById("taskModal").classList.add("hidden");
    currentModalMode = "create";
    resetModalForm();
}

function updateLeaderOptions(shiftId, selectedLeaderCodigoFJ) {
    const select = document.getElementById("cmbLeader");
    const normalizedShiftId = Number(shiftId || 0);
    const availableLeaders = leaders.filter(item => Number(item.ShiftId) === normalizedShiftId);

    select.innerHTML = `<option value="">Sem lider definido</option>`;

    availableLeaders.forEach(item => {
        select.innerHTML += `<option value="${escapeHtmlAttr(item.CodigoFJ)}">${escapeHtml(item.NameRomanji)}</option>`;
    });

    select.value = selectedLeaderCodigoFJ || "";
}

function submitModal() {
    const payload = buildPayload();

    if (currentModalMode === "edit") {
        send("update", {
            id: editingTaskId,
            ...payload
        });
        return;
    }

    send("save", payload);
}

function buildPayload() {
    return {
        description: document.getElementById("txtDescription").value.trim(),
        dueDate: document.getElementById("dtDueDate").value,
        shiftId: Number(document.getElementById("cmbShift").value || 0),
        leaderCodigoFJ: document.getElementById("cmbLeader").value,
        taskStatus: document.getElementById("cmbTaskStatus").value || "pending"
    };
}

function renderHistory(items) {
    const section = document.getElementById("historySection");
    const list = document.getElementById("historyList");

    if (currentModalMode !== "edit" || !editingTaskId) {
        section.classList.add("hidden");
        list.innerHTML = "";
        return;
    }

    section.classList.remove("hidden");

    if (!items.length) {
        list.innerHTML = `<div class="history-empty">Nenhuma mudanca de status registrada ainda.</div>`;
        return;
    }

    list.innerHTML = items.map(item => `
        <article class="history-item">
            <div class="history-head">
                <strong>${escapeHtml(item.newStatusLabel || "-")}</strong>
                <span>${escapeHtml(item.changedAt || "-")}</span>
            </div>
            <p>${item.previousStatusLabel
                ? `De ${escapeHtml(item.previousStatusLabel)} para ${escapeHtml(item.newStatusLabel)}`
                : `Status inicial: ${escapeHtml(item.newStatusLabel)}`}</p>
            <small>Por ${escapeHtml(item.changedByName || "-")} ${item.note ? `| ${escapeHtml(item.note)}` : ""}</small>
        </article>
    `).join("");
}

function syncModeState() {
    const isEdit = currentModalMode === "edit" && !!editingTaskId;
    document.getElementById("formTitle").textContent = isEdit ? "Editar Task" : "Nova Task";
    document.getElementById("formSubtitle").textContent = isEdit
        ? `Acompanhe a task #${editingTaskId}, ajuste o status e mantenha o controle do andamento.`
        : "Registre o que deve ser feito, para quando e qual lider do turno vai acompanhar.";
    document.getElementById("btnSaveModal").textContent = isEdit ? "Salvar Alteracoes" : "Cadastrar Task";
    document.getElementById("metaSummary").classList.toggle("hidden", !isEdit);
    document.getElementById("historySection").classList.toggle("hidden", !isEdit);
}

function isClosedStatus(taskStatus) {
    return taskStatus === "completed" || taskStatus === "cancelled";
}

function isOverdue(item) {
    if (isClosedStatus(item.taskStatus) || !item.dueDate) return false;
    return item.dueDate < todayIso();
}

function statusClass(taskStatus) {
    return {
        pending: "status-pending",
        in_progress: "status-progress",
        blocked: "status-blocked",
        completed: "status-completed",
        cancelled: "status-cancelled"
    }[taskStatus] || "status-pending";
}

function getStatusLabel(taskStatus) {
    const match = statuses.find(item => item.value === taskStatus);
    return match ? match.label : taskStatus;
}

function todayIso() {
    return new Date().toISOString().slice(0, 10);
}

function truncate(value, size) {
    const text = String(value || "");
    return text.length > size ? `${text.slice(0, size - 1)}...` : text;
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
