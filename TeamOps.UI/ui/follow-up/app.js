const state = {
    locale: "pt-BR",
    operators: [],
    locals: [],
    currentOperatorCodigo: "",
    closeTimer: null
};

const I18N = {
    "pt-BR": {
        documentTitle: "Cadastro de acompanhamento",
        pageTitle: "Cadastro de acompanhamento",
        pageSubtitle: "Registro rapido de erros, orientacoes e observacoes do operador.",
        eyebrow: "TeamOps FollowUp",
        heroBadge: "Formulario HTML",
        heroTitle: "Fluxo leve para cadastro rapido",
        heroText: "Setor e turno filtram o operador no navegador para deixar a tela mais responsiva.",
        metaCreatedBy: "Registrado por",
        metaExecutor: "Executor padrao",
        metaCurrentOperator: "Operador atual",
        sectionKicker: "Contexto",
        sectionTitle: "Dados do acompanhamento",
        filteredCountLabel: "operadores no filtro",
        fieldDate: "Data",
        fieldSector: "Setor",
        fieldShift: "Turno",
        fieldOperator: "Operador",
        fieldExecutor: "Executor",
        fieldWitness: "Testemunha",
        fieldReason: "Motivo",
        fieldType: "Tipo",
        fieldLocal: "Local",
        fieldEquipment: "Equipamento",
        fieldDescription: "Descricao",
        fieldGuidance: "Orientacao",
        descriptionPlaceholder: "Descreva o que aconteceu.",
        guidancePlaceholder: "Registre a orientacao aplicada.",
        cancel: "Cancelar",
        saveFollowUp: "Salvar acompanhamento",
        selectPlaceholder: "Selecione...",
        noWitness: "Sem testemunha",
        toastSuccess: "Sucesso",
        toastError: "Erro",
        toastValidation: "Validacao",
        toastSaved: "Acompanhamento salvo com sucesso.",
        toastErrorFallback: "Nao foi possivel salvar o acompanhamento.",
        validationDate: "Informe a data.",
        validationSector: "Selecione o setor.",
        validationShift: "Selecione o turno.",
        validationOperator: "Selecione o operador.",
        validationExecutor: "Selecione o executor.",
        validationReason: "Selecione o motivo.",
        validationType: "Selecione o tipo.",
        validationLocal: "Selecione o local.",
        validationEquipment: "Selecione o equipamento.",
        validationDescription: "Digite a descricao.",
        validationGuidance: "Digite a orientacao."
    },
    "ja-JP": {
        documentTitle: "\u30d5\u30a9\u30ed\u30fc\u767b\u9332",
        pageTitle: "\u30d5\u30a9\u30ed\u30fc\u767b\u9332",
        pageSubtitle: "\u4f5c\u696d\u8005\u306e\u30a8\u30e9\u30fc\u3001\u6307\u5c0e\u3001\u6c17\u4ed8\u304d\u3092\u7c21\u5358\u306b\u8a18\u9332\u3057\u307e\u3059\u3002",
        eyebrow: "TeamOps FollowUp",
        heroBadge: "HTML \u30d5\u30a9\u30fc\u30e0",
        heroTitle: "\u7d20\u65e9\u304f\u5165\u529b\u3067\u304d\u308b\u30d5\u30a9\u30ed\u30fc\u753b\u9762",
        heroText: "\u30bb\u30af\u30bf\u30fc\u3068\u30b7\u30d5\u30c8\u3067\u4f5c\u696d\u8005\u3092\u7d5e\u308a\u8fbc\u307f\u3001\u753b\u9762\u306e\u53cd\u5fdc\u3092\u8efd\u304f\u4fdd\u3061\u307e\u3059\u3002",
        metaCreatedBy: "\u767b\u9332\u8005",
        metaExecutor: "\u5b9f\u65bd\u8005",
        metaCurrentOperator: "\u73fe\u5728\u306e\u4f5c\u696d\u8005",
        sectionKicker: "\u57fa\u672c\u60c5\u5831",
        sectionTitle: "\u30d5\u30a9\u30ed\u30fc\u5185\u5bb9",
        filteredCountLabel: "\u30d5\u30a3\u30eb\u30bf\u5bfe\u8c61\u306e\u4f5c\u696d\u8005",
        fieldDate: "\u65e5\u4ed8",
        fieldSector: "\u30bb\u30af\u30bf\u30fc",
        fieldShift: "\u30b7\u30d5\u30c8",
        fieldOperator: "\u4f5c\u696d\u8005",
        fieldExecutor: "\u5b9f\u65bd\u8005",
        fieldWitness: "\u7acb\u4f1a\u3044",
        fieldReason: "\u7406\u7531",
        fieldType: "\u7a2e\u5225",
        fieldLocal: "\u5834\u6240",
        fieldEquipment: "\u8a2d\u5099",
        fieldDescription: "\u5185\u5bb9",
        fieldGuidance: "\u6307\u5c0e",
        descriptionPlaceholder: "\u767a\u751f\u3057\u305f\u5185\u5bb9\u3092\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        guidancePlaceholder: "\u5b9f\u65bd\u3057\u305f\u6307\u5c0e\u5185\u5bb9\u3092\u8a18\u9332\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        cancel: "\u30ad\u30e3\u30f3\u30bb\u30eb",
        saveFollowUp: "\u30d5\u30a9\u30ed\u30fc\u3092\u4fdd\u5b58",
        selectPlaceholder: "\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044",
        noWitness: "\u7acb\u4f1a\u3044\u306a\u3057",
        toastSuccess: "\u5b8c\u4e86",
        toastError: "\u30a8\u30e9\u30fc",
        toastValidation: "\u78ba\u8a8d",
        toastSaved: "\u30d5\u30a9\u30ed\u30fc\u3092\u4fdd\u5b58\u3057\u307e\u3057\u305f\u3002",
        toastErrorFallback: "\u30d5\u30a9\u30ed\u30fc\u3092\u4fdd\u5b58\u3067\u304d\u307e\u305b\u3093\u3067\u3057\u305f\u3002",
        validationDate: "\u65e5\u4ed8\u3092\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        validationSector: "\u30bb\u30af\u30bf\u30fc\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        validationShift: "\u30b7\u30d5\u30c8\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        validationOperator: "\u4f5c\u696d\u8005\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        validationExecutor: "\u5b9f\u65bd\u8005\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        validationReason: "\u7406\u7531\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        validationType: "\u7a2e\u5225\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        validationLocal: "\u5834\u6240\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        validationEquipment: "\u8a2d\u5099\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        validationDescription: "\u5185\u5bb9\u3092\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        validationGuidance: "\u6307\u5c0e\u5185\u5bb9\u3092\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002"
    }
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
        setLocale(payload.locale);
        hydrate(payload.data || {});
        return;
    }

    if (payload.type === "saved") {
        showToast(t("toastSuccess"), payload.data?.message || t("toastSaved"));
        clearTimeout(state.closeTimer);
        state.closeTimer = setTimeout(() => post("cancel"), 700);
        return;
    }

    if (payload.type === "error") {
        showToast(t("toastError"), payload.message || t("toastErrorFallback"));
    }
});

function hydrate(data) {
    const lookups = data.lookups || {};
    const defaults = data.defaults || {};

    state.operators = lookups.operators || [];
    state.locals = lookups.locals || [];
    state.currentOperatorCodigo = defaults.currentOperatorCodigo || "";

    setText("pageTitle", data.header?.title || t("pageTitle"));
    setText("pageSubtitle", data.header?.subtitle || t("pageSubtitle"));
    setText("creatorName", localize(defaults.creatorNamePt, defaults.creatorNameJp));
    setText("executorName", localize(defaults.executorNamePt, defaults.executorNameJp));
    setText("currentOperatorName", localize(defaults.currentOperatorNamePt, defaults.currentOperatorNameJp));

    fillSelect("cmbSector", lookups.sectors || [], t("selectPlaceholder"));
    fillSelect("cmbShift", lookups.shifts || [], t("selectPlaceholder"));
    fillSelect("cmbReason", lookups.reasons || [], t("selectPlaceholder"));
    fillSelect("cmbType", lookups.types || [], t("selectPlaceholder"));
    fillSelect("cmbEquipment", lookups.equipments || [], t("selectPlaceholder"));
    fillOperatorSelect("cmbExecutor", state.operators, t("selectPlaceholder"));

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
    const witnesses = state.operators.filter(operator => {
        const shiftOk = !shiftId || Number(operator.shiftId) === shiftId;
        const sectorOk = !sectorId || Number(operator.sectorId) === sectorId;
        const leaderOk = Boolean(operator.isLeader);
        return shiftOk && sectorOk && leaderOk;
    });
    const filteredLocals = state.locals.filter(local => {
        return !sectorId || Number(local.sectorId) === sectorId;
    });

    fillOperatorSelect("cmbOperator", filtered, t("selectPlaceholder"));
    fillOperatorSelect("cmbWitness", witnesses, t("noWitness"));
    fillSelect("cmbLocal", filteredLocals, t("selectPlaceholder"));

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
        showToast(t("toastValidation"), validation);
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
    if (!payload.date) return t("validationDate");
    if (!payload.sectorId) return t("validationSector");
    if (!payload.shiftId) return t("validationShift");
    if (!payload.operatorCodigoFJ) return t("validationOperator");
    if (!payload.executorCodigoFJ) return t("validationExecutor");
    if (!payload.reasonId) return t("validationReason");
    if (!payload.typeId) return t("validationType");
    if (!payload.localId) return t("validationLocal");
    if (!payload.equipmentId) return t("validationEquipment");
    if (!payload.description) return t("validationDescription");
    if (!payload.guidance) return t("validationGuidance");
    return "";
}

function fillSelect(id, items, placeholder) {
    const select = document.getElementById(id);
    const previousValue = select.value;
    const options = [`<option value="">${escapeHtml(placeholder)}</option>`];

    for (const item of items) {
        options.push(`<option value="${item.id}">${escapeHtml(localize(item.namePt, item.nameJp))}</option>`);
    }

    select.innerHTML = options.join("");

    if (previousValue && [...select.options].some(option => option.value === previousValue)) {
        select.value = previousValue;
    }
}

function fillOperatorSelect(id, items, placeholder) {
    const select = document.getElementById(id);
    const previousValue = select.value;
    const options = [`<option value="">${escapeHtml(placeholder)}</option>`];

    for (const item of items) {
        options.push(`<option value="${escapeHtml(item.codigoFJ)}">${escapeHtml(localize(item.namePt, item.nameJp))}</option>`);
    }

    select.innerHTML = options.join("");

    if (previousValue && [...select.options].some(option => option.value === previousValue)) {
        select.value = previousValue;
    }
}

function setLocale(locale) {
    state.locale = locale === "ja-JP" ? "ja-JP" : "pt-BR";
    document.documentElement.lang = state.locale;
    document.title = t("documentTitle");

    document.querySelectorAll("[data-i18n]").forEach(element => {
        const key = element.dataset.i18n;
        element.textContent = t(key);
    });

    document.querySelectorAll("[data-i18n-placeholder]").forEach(element => {
        const key = element.dataset.i18nPlaceholder;
        element.setAttribute("placeholder", t(key));
    });
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

function localize(pt, jp) {
    return state.locale === "ja-JP"
        ? (jp || pt || "-")
        : (pt || jp || "-");
}

function t(key) {
    return I18N[state.locale]?.[key] ?? I18N["pt-BR"]?.[key] ?? key;
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;");
}
