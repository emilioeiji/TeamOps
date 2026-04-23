const state = {
    operators: [],
    locals: [],
    currentOperatorCodigo: "",
    closeTimer: null
};

document.addEventListener("DOMContentLoaded", () => {
    bind();
    post("load");
});

function bind() {
    document.getElementById("cmbSector").addEventListener("change", syncFilteredOperators);
    document.getElementById("cmbShift").addEventListener("change", syncFilteredOperators);
    document.getElementById("btnSalvar").addEventListener("click", save);
    document.getElementById("btnCancelar").addEventListener("click", () => post("cancel"));
}

window.chrome?.webview?.addEventListener("message", event => {
    const payload = event.data;
    if (!payload?.type) {
        return;
    }

    if (payload.type === "init") {
        hydrate(payload.data || {});
        return;
    }

    if (payload.type === "saved") {
        showToast("Sucesso", payload.data?.message || "Acompanhamento salvo com sucesso.");
        clearTimeout(state.closeTimer);
        state.closeTimer = setTimeout(() => post("cancel"), 700);
        return;
    }

    if (payload.type === "error") {
        showToast("Erro", payload.message || "Nao foi possivel salvar o acompanhamento.");
    }
});

function hydrate(data) {
    const lookups = data.lookups || {};
    const defaults = data.defaults || {};

    state.operators = lookups.operators || [];
    state.locals = lookups.locals || [];
    state.currentOperatorCodigo = defaults.currentOperatorCodigo || "";

    setText("pageTitle", data.header?.title || "Cadastro de acompanhamento");
    setText("pageSubtitle", data.header?.subtitle || "");
    setText("creatorName", defaults.creatorName || "-");
    setText("executorName", defaults.executorName || "-");
    setText("currentOperatorName", defaults.currentOperatorName || "-");

    fillSelect("cmbSector", lookups.sectors || [], "Selecione...");
    fillSelect("cmbShift", lookups.shifts || [], "Selecione...");
    fillSelect("cmbReason", lookups.reasons || [], "Selecione...");
    fillSelect("cmbType", lookups.types || [], "Selecione...");
    fillSelect("cmbEquipment", lookups.equipments || [], "Selecione...");
    fillOperatorSelect("cmbExecutor", state.operators, "Selecione...");

    document.getElementById("txtDate").value = defaults.date || "";
    document.getElementById("cmbSector").value = String(defaults.sectorId || "");
    document.getElementById("cmbShift").value = String(defaults.shiftId || "");
    document.getElementById("cmbExecutor").value = defaults.executorCodigoFJ || "";

    syncFilteredOperators();
}

function syncFilteredOperators() {
    const sectorId = Number(document.getElementById("cmbSector").value || 0);
    const shiftId = Number(document.getElementById("cmbShift").value || 0);

    const filtered = state.operators.filter(operator => {
        const sectorOk = !sectorId || Number(operator.sectorId) === sectorId;
        const shiftOk = !shiftId || Number(operator.shiftId) === shiftId;
        return sectorOk && shiftOk;
    });
    const filteredLocals = state.locals.filter(local => {
        return !sectorId || Number(local.sectorId) === sectorId;
    });

    fillOperatorSelect("cmbOperator", filtered, "Selecione...");
    fillOperatorSelect("cmbWitness", filtered, "Sem testemunha");
    fillSelect("cmbLocal", filteredLocals, "Selecione...");

    const executor = document.getElementById("cmbExecutor");
    if (!executor.value && state.currentOperatorCodigo) {
        executor.value = state.currentOperatorCodigo;
    }

    setText("filteredCount", filtered.length);
}

function save() {
    const payload = collectPayload();
    const validation = validatePayload(payload);
    if (validation) {
        showToast("Validacao", validation);
        return;
    }

    post("save", payload);
}

function collectPayload() {
    return {
        date: document.getElementById("txtDate").value,
        shiftId: Number(document.getElementById("cmbShift").value || 0),
        sectorId: Number(document.getElementById("cmbSector").value || 0),
        operatorCodigoFJ: document.getElementById("cmbOperator").value,
        executorCodigoFJ: document.getElementById("cmbExecutor").value,
        witnessCodigoFJ: document.getElementById("cmbWitness").value,
        reasonId: Number(document.getElementById("cmbReason").value || 0),
        typeId: Number(document.getElementById("cmbType").value || 0),
        localId: Number(document.getElementById("cmbLocal").value || 0),
        equipmentId: Number(document.getElementById("cmbEquipment").value || 0),
        description: document.getElementById("txtDescription").value.trim(),
        guidance: document.getElementById("txtGuidance").value.trim()
    };
}

function validatePayload(payload) {
    if (!payload.date) return "Informe a data.";
    if (!payload.sectorId) return "Selecione o setor.";
    if (!payload.shiftId) return "Selecione o turno.";
    if (!payload.operatorCodigoFJ) return "Selecione o operador.";
    if (!payload.executorCodigoFJ) return "Selecione o executor.";
    if (!payload.reasonId) return "Selecione o motivo.";
    if (!payload.typeId) return "Selecione o tipo.";
    if (!payload.localId) return "Selecione o local.";
    if (!payload.equipmentId) return "Selecione o equipamento.";
    if (!payload.description) return "Digite a descricao.";
    if (!payload.guidance) return "Digite a orientacao.";
    return "";
}

function fillSelect(id, items, placeholder) {
    const select = document.getElementById(id);
    const options = [`<option value="">${escapeHtml(placeholder)}</option>`];

    for (const item of items) {
        options.push(`<option value="${item.id}">${escapeHtml(item.name)}</option>`);
    }

    select.innerHTML = options.join("");
}

function fillOperatorSelect(id, items, placeholder) {
    const select = document.getElementById(id);
    const previousValue = select.value;
    const options = [`<option value="">${escapeHtml(placeholder)}</option>`];

    for (const item of items) {
        options.push(`<option value="${escapeHtml(item.codigoFJ)}">${escapeHtml(item.nameRomanji)}</option>`);
    }

    select.innerHTML = options.join("");

    if (previousValue && [...select.options].some(option => option.value === previousValue)) {
        select.value = previousValue;
    }
}

function post(action, payload = {}) {
    window.chrome?.webview?.postMessage({
        action,
        ...payload
    });
}

function showToast(title, message) {
    const root = document.getElementById("toast");
    setText("toastTitle", title);
    setText("toastMessage", message);
    root.classList.remove("hidden");
    clearTimeout(showToast.timer);
    showToast.timer = setTimeout(() => root.classList.add("hidden"), 3200);
}

function setText(id, value) {
    const node = document.getElementById(id);
    if (node) {
        node.textContent = String(value ?? "");
    }
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;");
}
