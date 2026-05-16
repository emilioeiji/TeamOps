const state = {
    locale: "pt-BR"
};

let currentRows = [];
let selectedRowId = 0;
let selectedOperatorCodigo = "";
let searchTimer = null;

const I18N = {
    "pt-BR": {
        documentTitle: "Relatorio Follow",
        headerTitle: "Relatorio Follow",
        headerSubtitle: "Consulta dos acompanhamentos com busca rapida, exportacao e impressao.",
        recordsSuffix: " registros",
        labelStart: "Data Inicial",
        labelEnd: "Data Final",
        labelShift: "Turno",
        labelType: "Tipo",
        labelReason: "Motivo",
        labelSector: "Setor",
        labelSearch: "Buscar",
        searchPlaceholder: "Operador, descricao, orientacao, setor...",
        btnSearch: "Buscar",
        btnClear: "Limpar",
        btnExport: "Exportar",
        tableTitle: "Registros",
        tableSubtitle: "Filtros reduzidos para manter a busca mais rapida no dia a dia.",
        colDate: "Data",
        colShift: "Turno",
        colOperator: "Operador",
        colType: "Tipo",
        colReason: "Motivo",
        colSector: "Setor",
        colDescription: "Descricao",
        colGuidance: "Orientacao",
        colActions: "Acoes",
        loading: "Carregando...",
        empty: "Nenhum registro encontrado.",
        view: "Visualizar",
        print: "Imprimir",
        modalTitle: "Detalhes do Acompanhamento",
        btnOperatorHistory: "Historico do operador",
        btnPrintSheet: "Ficha para impressao",
        previewDate: "Data",
        previewShift: "Turno",
        previewOperator: "Operador",
        previewExecutor: "Executor",
        previewWitness: "Testemunha",
        previewReason: "Motivo",
        previewType: "Tipo",
        previewSector: "Setor",
        previewLocal: "Local",
        previewEquipment: "Equipamento",
        previewDescription: "Descricao",
        previewGuidance: "Orientacao",
        toastNotice: "Aviso",
        toastError: "Erro",
        errorFallback: "Falha ao carregar relatorio."
    },
    "ja-JP": {
        documentTitle: "\u30d5\u30a9\u30ed\u30fc\u30ec\u30dd\u30fc\u30c8",
        headerTitle: "\u30d5\u30a9\u30ed\u30fc\u30ec\u30dd\u30fc\u30c8",
        headerSubtitle: "\u30d5\u30a9\u30ed\u30fc\u306e\u691c\u7d22\u3001\u5370\u5237\u3001\u30a8\u30af\u30b9\u30dd\u30fc\u30c8\u3092\u7d20\u65e9\u304f\u884c\u3048\u307e\u3059\u3002",
        recordsSuffix: " \u4ef6",
        labelStart: "\u958b\u59cb\u65e5",
        labelEnd: "\u7d42\u4e86\u65e5",
        labelShift: "\u30b7\u30d5\u30c8",
        labelType: "\u7a2e\u5225",
        labelReason: "\u7406\u7531",
        labelSector: "\u30bb\u30af\u30bf\u30fc",
        labelSearch: "\u691c\u7d22",
        searchPlaceholder: "\u4f5c\u696d\u8005\u3001\u5185\u5bb9\u3001\u6307\u5c0e\u3001\u30bb\u30af\u30bf\u30fc...",
        btnSearch: "\u691c\u7d22",
        btnClear: "\u30af\u30ea\u30a2",
        btnExport: "\u30a8\u30af\u30b9\u30dd\u30fc\u30c8",
        tableTitle: "\u30ec\u30b3\u30fc\u30c9",
        tableSubtitle: "\u65e5\u5e38\u3067\u7d20\u65e9\u304f\u691c\u7d22\u3067\u304d\u308b\u3088\u3046\u3001\u30d5\u30a3\u30eb\u30bf\u3092\u7c21\u7d20\u306b\u3057\u3066\u3044\u307e\u3059\u3002",
        colDate: "\u65e5\u4ed8",
        colShift: "\u30b7\u30d5\u30c8",
        colOperator: "\u4f5c\u696d\u8005",
        colType: "\u7a2e\u5225",
        colReason: "\u7406\u7531",
        colSector: "\u30bb\u30af\u30bf\u30fc",
        colDescription: "\u5185\u5bb9",
        colGuidance: "\u6307\u5c0e",
        colActions: "\u64cd\u4f5c",
        loading: "\u8aad\u307f\u8fbc\u307f\u4e2d...",
        empty: "\u30ec\u30b3\u30fc\u30c9\u304c\u3042\u308a\u307e\u305b\u3093\u3002",
        view: "\u8868\u793a",
        print: "\u5370\u5237",
        modalTitle: "\u30d5\u30a9\u30ed\u30fc\u8a73\u7d30",
        btnOperatorHistory: "\u4f5c\u696d\u8005\u5c65\u6b74",
        btnPrintSheet: "\u5370\u5237\u7528\u5e33\u7968",
        previewDate: "\u65e5\u4ed8",
        previewShift: "\u30b7\u30d5\u30c8",
        previewOperator: "\u4f5c\u696d\u8005",
        previewExecutor: "\u5b9f\u65bd\u8005",
        previewWitness: "\u7acb\u4f1a\u3044",
        previewReason: "\u7406\u7531",
        previewType: "\u7a2e\u5225",
        previewSector: "\u30bb\u30af\u30bf\u30fc",
        previewLocal: "\u5834\u6240",
        previewEquipment: "\u8a2d\u5099",
        previewDescription: "\u5185\u5bb9",
        previewGuidance: "\u6307\u5c0e",
        toastNotice: "\u304a\u77e5\u3089\u305b",
        toastError: "\u30a8\u30e9\u30fc",
        errorFallback: "\u30ec\u30dd\u30fc\u30c8\u306e\u8aad\u307f\u8fbc\u307f\u306b\u5931\u6557\u3057\u307e\u3057\u305f\u3002"
    }
};

document.addEventListener("DOMContentLoaded", () => {
    bind();
    post("load");
});

function bind() {
    document.getElementById("btnBuscar").addEventListener("click", applyFilters);
    document.getElementById("btnExportar").addEventListener("click", exportRows);
    document.getElementById("btnLimpar").addEventListener("click", resetFilters);
    document.getElementById("btnCloseModal").addEventListener("click", closeModal);
    document.getElementById("btnModalPrint").addEventListener("click", () => {
        if (selectedRowId > 0) {
            post("print", { id: selectedRowId });
        }
    });
    document.getElementById("btnModalOperator").addEventListener("click", () => {
        if (selectedOperatorCodigo) {
            post("operator_report", { codigoFJ: selectedOperatorCodigo });
        }
    });

    document.getElementById("txtSearch").addEventListener("input", () => {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(applyFilters, 220);
    });

    ["dtInicial", "dtFinal", "cmbShift", "cmbType", "cmbReason", "cmbSector"].forEach(id => {
        document.getElementById(id).addEventListener("change", applyFilters);
    });

    document.getElementById("modal").addEventListener("click", event => {
        if (event.target.id === "modal") {
            closeModal();
        }
    });
}

window.chrome?.webview?.addEventListener("message", event => {
    const payload = event.data;
    if (!payload?.type) return;

    if (payload.type === "init") {
        setLocale(payload.locale);
        hydrateFilters(payload.data);
        return;
    }

    if (payload.type === "rows") {
        currentRows = payload.data?.rows || [];
        renderRows(currentRows);
        setTotal(payload.data?.total ?? 0);
        return;
    }

    if (payload.type === "notify") {
        showToast(payload.data?.title || t("toastNotice"), payload.data?.message || "");
        return;
    }

    if (payload.type === "error") {
        showToast(t("toastError"), payload.message || t("errorFallback"));
    }
});

function setLocale(locale) {
    state.locale = locale === "ja-JP" ? "ja-JP" : "pt-BR";
    document.documentElement.lang = state.locale;
    document.title = t("documentTitle");

    document.querySelectorAll("[data-i18n]").forEach(element => {
        element.textContent = t(element.dataset.i18n);
    });

    document.querySelectorAll("[data-i18n-placeholder]").forEach(element => {
        element.setAttribute("placeholder", t(element.dataset.i18nPlaceholder));
    });
}

function setTotal(total) {
    document.getElementById("totalCount").textContent = total;
    document.getElementById("totalSuffix").textContent = t("recordsSuffix");
}

function hydrateFilters(data) {
    fillSelect("cmbShift", data.filters?.shifts || []);
    fillSelect("cmbType", data.filters?.types || []);
    fillSelect("cmbReason", data.filters?.reasons || []);
    fillSelect("cmbSector", data.filters?.sectors || []);

    document.getElementById("dtInicial").value = data.defaults?.dtInicial || "";
    document.getElementById("dtFinal").value = data.defaults?.dtFinal || "";
    document.getElementById("cmbShift").value = String(data.defaults?.shiftId ?? 0);
    document.getElementById("cmbType").value = String(data.defaults?.typeId ?? 0);
    document.getElementById("cmbReason").value = String(data.defaults?.reasonId ?? 0);
    document.getElementById("cmbSector").value = String(data.defaults?.sectorId ?? 0);
    document.getElementById("txtSearch").value = data.defaults?.search || "";
}

function renderRows(rows) {
    const body = document.getElementById("tblBody");

    if (!rows.length) {
        body.innerHTML = `<tr><td colspan="9" class="empty-cell">${escapeHtml(t("empty"))}</td></tr>`;
        return;
    }

    body.innerHTML = rows.map(row => `
        <tr>
            <td>${escapeHtml(row.date)}</td>
            <td>${escapeHtml(row.shiftName)}</td>
            <td>${escapeHtml(row.operatorName)}</td>
            <td>${escapeHtml(row.typeName)}</td>
            <td>${escapeHtml(row.reasonName)}</td>
            <td>${escapeHtml(row.sectorName)}</td>
            <td class="text-cell">${escapeHtml(row.description)}</td>
            <td class="text-cell">${escapeHtml(row.guidance)}</td>
            <td class="actions-cell">
                <button class="icon-btn icon-btn-view" type="button" data-view="${row.id}" title="${escapeHtml(t("view"))}" aria-label="${escapeHtml(t("view"))}">
                    <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M2.25 12s3.75-6 9.75-6 9.75 6 9.75 6-3.75 6-9.75 6S2.25 12 2.25 12Z"/><path d="M12 14.75A2.75 2.75 0 1 0 12 9.25a2.75 2.75 0 0 0 0 5.5Z"/></svg>
                </button>
                <button class="icon-btn icon-btn-print" type="button" data-print="${row.id}" title="${escapeHtml(t("print"))}" aria-label="${escapeHtml(t("print"))}">
                    <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6 9V3.75h12V9"/><path d="M6.75 14.25h10.5V20.25H6.75z"/><path d="M5.25 9h13.5A2.25 2.25 0 0 1 21 11.25v4.5A2.25 2.25 0 0 1 18.75 18H17.25v-3.75H6.75V18H5.25A2.25 2.25 0 0 1 3 15.75v-4.5A2.25 2.25 0 0 1 5.25 9Z"/></svg>
                </button>
            </td>
        </tr>
    `).join("");

    body.querySelectorAll("[data-view]").forEach(button => {
        button.addEventListener("click", () => openModal(Number(button.dataset.view)));
    });

    body.querySelectorAll("[data-print]").forEach(button => {
        button.addEventListener("click", () => post("print", { id: Number(button.dataset.print) }));
    });
}

function openModal(id) {
    const row = currentRows.find(item => item.id === id);
    if (!row) return;

    selectedRowId = id;
    selectedOperatorCodigo = row.operatorCodigoFJ || "";
    document.getElementById("modalBody").innerHTML = `
        <div class="preview-grid">
            <div><span class="preview-label">${escapeHtml(t("previewDate"))}</span><strong>${escapeHtml(row.date)}</strong></div>
            <div><span class="preview-label">${escapeHtml(t("previewShift"))}</span><strong>${escapeHtml(row.shiftName)}</strong></div>
            <div><span class="preview-label">${escapeHtml(t("previewOperator"))}</span><strong>${escapeHtml(row.operatorName)}</strong></div>
            <div><span class="preview-label">${escapeHtml(t("previewExecutor"))}</span><strong>${escapeHtml(row.executorName)}</strong></div>
            <div><span class="preview-label">${escapeHtml(t("previewWitness"))}</span><strong>${escapeHtml(row.witnessName)}</strong></div>
            <div><span class="preview-label">${escapeHtml(t("previewReason"))}</span><strong>${escapeHtml(row.reasonName)}</strong></div>
            <div><span class="preview-label">${escapeHtml(t("previewType"))}</span><strong>${escapeHtml(row.typeName)}</strong></div>
            <div><span class="preview-label">${escapeHtml(t("previewSector"))}</span><strong>${escapeHtml(row.sectorName)}</strong></div>
            <div><span class="preview-label">${escapeHtml(t("previewLocal"))}</span><strong>${escapeHtml(row.localName)}</strong></div>
            <div><span class="preview-label">${escapeHtml(t("previewEquipment"))}</span><strong>${escapeHtml(row.equipmentName)}</strong></div>
        </div>
        <div class="text-block">
            <span class="preview-label">${escapeHtml(t("previewDescription"))}</span>
            <p>${escapeHtml(row.description)}</p>
        </div>
        <div class="text-block">
            <span class="preview-label">${escapeHtml(t("previewGuidance"))}</span>
            <p>${escapeHtml(row.guidance)}</p>
        </div>
    `;
    document.getElementById("modal").classList.remove("hidden");
}

function closeModal() {
    selectedRowId = 0;
    selectedOperatorCodigo = "";
    document.getElementById("modal").classList.add("hidden");
}

function applyFilters() {
    post("apply", collectFilterPayload());
}

function exportRows() {
    post("export", collectFilterPayload());
}

function resetFilters() {
    document.getElementById("cmbShift").value = "0";
    document.getElementById("cmbType").value = "0";
    document.getElementById("cmbReason").value = "0";
    document.getElementById("cmbSector").value = "0";
    document.getElementById("txtSearch").value = "";
    applyFilters();
}

function collectFilterPayload() {
    return {
        dtInicial: document.getElementById("dtInicial").value,
        dtFinal: document.getElementById("dtFinal").value,
        shiftId: Number(document.getElementById("cmbShift").value || 0),
        typeId: Number(document.getElementById("cmbType").value || 0),
        reasonId: Number(document.getElementById("cmbReason").value || 0),
        sectorId: Number(document.getElementById("cmbSector").value || 0),
        search: document.getElementById("txtSearch").value
    };
}

function fillSelect(id, items) {
    const select = document.getElementById(id);
    select.innerHTML = items.map(item => `<option value="${item.id}">${escapeHtml(item.name)}</option>`).join("");
}

function post(action, payload = {}) {
    window.chrome?.webview?.postMessage({
        action,
        ...payload
    });
}

function showToast(title, message) {
    const root = document.getElementById("toast");
    document.getElementById("toastTitle").textContent = title;
    document.getElementById("toastMessage").textContent = message;
    root.classList.remove("hidden");
    clearTimeout(showToast._timer);
    showToast._timer = setTimeout(() => root.classList.add("hidden"), 3200);
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
