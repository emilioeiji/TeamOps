const state = {
    locale: "pt-BR"
};

let rows = [];
let currentShiftId = 0;

const I18N = {
    "pt-BR": {
        documentTitle: "Sobra de Peca",
        headerBar: "Sobra de Peca",
        newRecordTitle: "Novo Registro",
        newRecordSubtitle: "Preencha os dados abaixo para registrar a sobra de peca.",
        labelDate: "Data",
        labelShift: "Turno",
        labelLot: "Lote",
        labelOperator: "Operador",
        labelTanjuu: "Tanjuu",
        labelWeight: "Peso (g)",
        labelQuantity: "Quantidade",
        labelMachine: "Maquina",
        labelShain: "Shain",
        labelItem: "Item",
        labelLeader: "Lider",
        labelObservation: "Observacao",
        observationPlaceholder: "Detalhes complementares...",
        cancel: "Cancelar",
        save: "Salvar",
        listTitle: "Sobra de Peca",
        listSubtitle: "Cadastro rapido com historico dos ultimos 100 registros",
        searchLabel: "Buscar",
        searchPlaceholder: "Buscar lote, operador, maquina, item ou observacao...",
        selectPlaceholder: "Selecione...",
        savedFallback: "Registro salvo com sucesso.",
        errorFallback: "Ocorreu um erro ao processar a solicitacao.",
        empty: "Nenhum registro encontrado.",
        loading: "Carregando...",
        tableId: "ID",
        tableDate: "Data",
        tableShift: "Turno",
        tableLot: "Lote",
        tableOperator: "Operador",
        tableTanjuu: "Tanjuu",
        tableWeight: "Peso (g)",
        tableQuantity: "Qtd",
        tableMachine: "Maquina",
        tableShain: "Shain",
        tableItem: "Item",
        tableLeader: "Lider",
        tableCreatedAt: "Criado em",
        tableObservation: "Observacao",
        validationLot: "Informe o lote.",
        validationOperator: "Selecione um operador.",
        validationTanjuu: "Informe um tanjuu valido.",
        validationWeight: "Informe um peso valido.",
        validationQuantity: "Quantidade invalida.",
        validationMachine: "Selecione uma maquina.",
        validationShain: "Selecione um shain.",
        validationItem: "Informe o item."
    },
    "ja-JP": {
        documentTitle: "\u90e8\u54c1\u4f59\u308a",
        headerBar: "\u90e8\u54c1\u4f59\u308a",
        newRecordTitle: "\u65b0\u898f\u767b\u9332",
        newRecordSubtitle: "\u90e8\u54c1\u4f59\u308a\u3092\u767b\u9332\u3059\u308b\u60c5\u5831\u3092\u5165\u529b\u3057\u307e\u3059\u3002",
        labelDate: "\u65e5\u4ed8",
        labelShift: "\u30b7\u30d5\u30c8",
        labelLot: "\u30ed\u30c3\u30c8",
        labelOperator: "\u4f5c\u696d\u8005",
        labelTanjuu: "\u5358\u91cd",
        labelWeight: "\u91cd\u91cf (g)",
        labelQuantity: "\u6570\u91cf",
        labelMachine: "\u8a2d\u5099",
        labelShain: "\u793e\u54e1",
        labelItem: "\u54c1\u76ee",
        labelLeader: "\u30ea\u30fc\u30c0\u30fc",
        labelObservation: "\u5099\u8003",
        observationPlaceholder: "\u88dc\u8db3\u60c5\u5831\u3092\u8a18\u5165...",
        cancel: "\u30ad\u30e3\u30f3\u30bb\u30eb",
        save: "\u4fdd\u5b58",
        listTitle: "\u90e8\u54c1\u4f59\u308a",
        listSubtitle: "\u76f4\u8fd1 100 \u4ef6\u306e\u5c65\u6b74\u3092\u78ba\u8a8d\u3067\u304d\u308b\u7c21\u6613\u767b\u9332\u753b\u9762\u3067\u3059",
        searchLabel: "\u691c\u7d22",
        searchPlaceholder: "\u30ed\u30c3\u30c8\u3001\u4f5c\u696d\u8005\u3001\u8a2d\u5099\u3001\u54c1\u76ee\u3001\u5099\u8003\u3067\u691c\u7d22...",
        selectPlaceholder: "\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044",
        savedFallback: "\u767b\u9332\u3057\u307e\u3057\u305f\u3002",
        errorFallback: "\u51e6\u7406\u4e2d\u306b\u30a8\u30e9\u30fc\u304c\u767a\u751f\u3057\u307e\u3057\u305f\u3002",
        empty: "\u8a72\u5f53\u30c7\u30fc\u30bf\u304c\u3042\u308a\u307e\u305b\u3093\u3002",
        loading: "\u8aad\u307f\u8fbc\u307f\u4e2d...",
        tableId: "ID",
        tableDate: "\u65e5\u4ed8",
        tableShift: "\u30b7\u30d5\u30c8",
        tableLot: "\u30ed\u30c3\u30c8",
        tableOperator: "\u4f5c\u696d\u8005",
        tableTanjuu: "\u5358\u91cd",
        tableWeight: "\u91cd\u91cf (g)",
        tableQuantity: "\u6570\u91cf",
        tableMachine: "\u8a2d\u5099",
        tableShain: "\u793e\u54e1",
        tableItem: "\u54c1\u76ee",
        tableLeader: "\u30ea\u30fc\u30c0\u30fc",
        tableCreatedAt: "\u767b\u9332\u65e5\u6642",
        tableObservation: "\u5099\u8003",
        validationLot: "\u30ed\u30c3\u30c8\u3092\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        validationOperator: "\u4f5c\u696d\u8005\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        validationTanjuu: "\u6709\u52b9\u306a\u5358\u91cd\u3092\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        validationWeight: "\u6709\u52b9\u306a\u91cd\u91cf\u3092\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        validationQuantity: "\u6570\u91cf\u304c\u7121\u52b9\u3067\u3059\u3002",
        validationMachine: "\u8a2d\u5099\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        validationShain: "\u793e\u54e1\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        validationItem: "\u54c1\u76ee\u3092\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002"
    }
};

window.addEventListener("DOMContentLoaded", () => {
    bindEvents();
    send("load");
});

function bindEvents() {
    document.getElementById("btnSalvar").addEventListener("click", saveRecord);
    document.getElementById("btnCancelar").addEventListener("click", clearForm);
    document.getElementById("searchInput").addEventListener("input", applyFilters);
    document.getElementById("txtPeso").addEventListener("input", calculateQuantidade);
    document.getElementById("txtTanjuu").addEventListener("input", calculateQuantidade);
    document.getElementById("txtItem").addEventListener("input", event => {
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
            setLocale(msg.locale);
            hydrateScreen(msg);
            break;
        case "rows":
            rows = msg.data || [];
            applyFilters();
            break;
        case "saved":
            alert(t("savedFallback"));
            clearForm();
            break;
        case "error":
            alert(msg.message || t("errorFallback"));
            break;
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

function hydrateScreen(payload) {
    currentShiftId = Number(payload.shiftId || 0);

    document.getElementById("txtDate").value = payload.today || "";
    document.getElementById("txtTurno").value = payload.shiftName || "";
    document.getElementById("txtLider").value = localize(payload.leaderNamePt, payload.leaderNameJp);

    fillSelect("cmbOperador", payload.operators || [], "CodigoFJ");
    fillSelect("cmbMaquina", payload.machines || [], "Id");
    fillSelect("cmbShain", payload.shains || [], "Id");

    rows = payload.rows || [];
    applyFilters();
}

function fillSelect(id, items, valueField) {
    const select = document.getElementById(id);
    select.innerHTML = `<option value="">${escapeHtml(t("selectPlaceholder"))}</option>`;

    items.forEach(item => {
        select.innerHTML += `<option value="${item[valueField]}">${escapeHtml(localize(item.NamePt, item.NameJp))}</option>`;
    });
}

function calculateQuantidade() {
    const peso = parseDecimal(document.getElementById("txtPeso").value);
    const tanjuu = parseDecimal(document.getElementById("txtTanjuu").value);
    const quantidadeInput = document.getElementById("txtQuantidade");

    if (peso > 0 && tanjuu > 0) {
        quantidadeInput.value = String(Math.round(peso / tanjuu));
        return;
    }

    quantidadeInput.value = "";
}

function saveRecord() {
    const lote = document.getElementById("txtLote").value.trim();
    const opCodigoFJ = document.getElementById("cmbOperador").value;
    const tanjuu = parseDecimal(document.getElementById("txtTanjuu").value);
    const pesoGramas = parseDecimal(document.getElementById("txtPeso").value);
    const quantidade = parseDecimal(document.getElementById("txtQuantidade").value);
    const machineId = Number(document.getElementById("cmbMaquina").value || 0);
    const shainId = Number(document.getElementById("cmbShain").value || 0);
    const item = document.getElementById("txtItem").value.trim().toUpperCase();

    if (!lote) {
        alert(t("validationLot"));
        return;
    }

    if (!opCodigoFJ) {
        alert(t("validationOperator"));
        return;
    }

    if (tanjuu <= 0) {
        alert(t("validationTanjuu"));
        return;
    }

    if (pesoGramas <= 0) {
        alert(t("validationWeight"));
        return;
    }

    if (quantidade <= 0) {
        alert(t("validationQuantity"));
        return;
    }

    if (machineId <= 0) {
        alert(t("validationMachine"));
        return;
    }

    if (shainId <= 0) {
        alert(t("validationShain"));
        return;
    }

    if (!item) {
        alert(t("validationItem"));
        return;
    }

    send("save", {
        date: document.getElementById("txtDate").value,
        shiftId: currentShiftId,
        lote,
        opCodigoFJ,
        tanjuu,
        pesoGramas,
        quantidade,
        machineId,
        shainId,
        observacao: document.getElementById("txtObservacao").value.trim(),
        item
    });
}

function clearForm() {
    document.getElementById("txtDate").value = formatDate(new Date());
    document.getElementById("txtLote").value = "";
    document.getElementById("cmbOperador").value = "";
    document.getElementById("txtTanjuu").value = "";
    document.getElementById("txtPeso").value = "";
    document.getElementById("txtQuantidade").value = "";
    document.getElementById("cmbMaquina").value = "";
    document.getElementById("cmbShain").value = "";
    document.getElementById("txtObservacao").value = "";
    document.getElementById("txtItem").value = "";
}

function applyFilters() {
    const term = document.getElementById("searchInput").value.trim().toLowerCase();

    const filtered = rows
        .filter(item => {
            if (!term) return true;

            const haystack = [
                item.data,
                item.shiftNamePt,
                item.shiftNameJp,
                item.lote,
                item.operatorNamePt,
                item.operatorNameJp,
                item.machineNamePt,
                item.machineNameJp,
                item.shainNamePt,
                item.shainNameJp,
                item.item,
                item.lider,
                item.observacao,
                item.createdAt
            ]
                .filter(Boolean)
                .join(" ")
                .toLowerCase();

            return haystack.includes(term);
        })
        .sort((a, b) => Number(b.id || 0) - Number(a.id || 0));

    renderTable(filtered);
}

function renderTable(items) {
    const container = document.getElementById("tableContainer");

    if (!items || items.length === 0) {
        container.innerHTML = `<div class="empty-state">${escapeHtml(t("empty"))}</div>`;
        return;
    }

    let html = `
        <table class="min-w-full sobra-table">
            <thead>
                <tr>
                    <th>${escapeHtml(t("tableId"))}</th>
                    <th>${escapeHtml(t("tableDate"))}</th>
                    <th>${escapeHtml(t("tableShift"))}</th>
                    <th>${escapeHtml(t("tableLot"))}</th>
                    <th>${escapeHtml(t("tableOperator"))}</th>
                    <th>${escapeHtml(t("tableTanjuu"))}</th>
                    <th>${escapeHtml(t("tableWeight"))}</th>
                    <th>${escapeHtml(t("tableQuantity"))}</th>
                    <th>${escapeHtml(t("tableMachine"))}</th>
                    <th>${escapeHtml(t("tableShain"))}</th>
                    <th>${escapeHtml(t("tableItem"))}</th>
                    <th>${escapeHtml(t("tableLeader"))}</th>
                    <th>${escapeHtml(t("tableCreatedAt"))}</th>
                    <th>${escapeHtml(t("tableObservation"))}</th>
                </tr>
            </thead>
            <tbody>
    `;

    items.forEach(item => {
        html += `
            <tr>
                <td>${escapeHtml(item.id)}</td>
                <td>${escapeHtml(item.data)}</td>
                <td>${escapeHtml(localize(item.shiftNamePt, item.shiftNameJp))}</td>
                <td>${escapeHtml(item.lote)}</td>
                <td>${escapeHtml(localize(item.operatorNamePt, item.operatorNameJp))}</td>
                <td class="text-right">${formatNumber(item.tanjuu)}</td>
                <td class="text-right">${formatNumber(item.pesoGramas)}</td>
                <td class="text-right">${formatNumber(item.quantidade)}</td>
                <td>${escapeHtml(localize(item.machineNamePt, item.machineNameJp))}</td>
                <td>${escapeHtml(localize(item.shainNamePt, item.shainNameJp))}</td>
                <td>${escapeHtml(item.item)}</td>
                <td>${escapeHtml(item.lider)}</td>
                <td>${escapeHtml(item.createdAt)}</td>
                <td class="notes-cell" title="${escapeHtmlAttr(item.observacao || "")}">
                    ${escapeHtml(item.observacao || "")}
                </td>
            </tr>
        `;
    });

    html += `
            </tbody>
        </table>
    `;

    container.innerHTML = html;
}

function localize(pt, jp) {
    return state.locale === "ja-JP"
        ? (jp || pt || "")
        : (pt || jp || "");
}

function parseDecimal(value) {
    if (!value) return 0;

    const raw = String(value).trim().replace(/\s+/g, "");
    const commaCount = (raw.match(/,/g) || []).length;
    const dotCount = (raw.match(/\./g) || []).length;

    let normalized = raw;

    if (commaCount > 0 && dotCount > 0) {
        normalized = raw.replace(/\./g, "").replace(",", ".");
    } else if (commaCount > 0) {
        normalized = raw.replace(",", ".");
    }

    const parsed = Number(normalized);
    return Number.isFinite(parsed) ? parsed : 0;
}

function formatDate(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
}

function formatNumber(value) {
    const numeric = Number(value || 0);

    if (!Number.isFinite(numeric)) {
        return "";
    }

    return numeric.toLocaleString(state.locale, {
        maximumFractionDigits: 2
    });
}

function t(key) {
    return I18N[state.locale]?.[key] ?? I18N["pt-BR"]?.[key] ?? key;
}

function escapeHtml(value) {
    return String(value ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
}

function escapeHtmlAttr(value) {
    return escapeHtml(value).replace(/`/g, "&#96;");
}
