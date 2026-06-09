const state = {
    locale: "pt-BR",
    rows: [],
    shifts: [],
    machines: []
};

const I18N = {
    "pt-BR": {
        title: "Relatorio de Sobra de Peca",
        badge: "Sobra de Peca",
        subtitle: "Resumo gerencial, filtros e exportacao dos lancamentos.",
        exportCsv: "Exportar CSV",
        print: "Imprimir",
        metricRows: "Registros",
        metricQuantity: "Quantidade",
        metricWeight: "Peso (g)",
        metricItems: "Itens",
        start: "Data inicial",
        end: "Data final",
        shift: "Turno",
        machine: "Maquina",
        item: "Item",
        search: "Buscar",
        searchPlaceholder: "Lote, operador, shain, lider ou observacao",
        apply: "Buscar",
        clear: "Limpar",
        all: "Todos",
        tableTitle: "Lancamentos",
        tableSubtitle: "Conferencia com quantidade, peso, item e responsavel.",
        colDate: "Data",
        colShift: "Turno",
        colLot: "Lote",
        colOperator: "Operador",
        colItem: "Item",
        colMachine: "Maquina",
        colQuantity: "Qtd",
        colWeight: "Peso",
        colShain: "Shain",
        colLeader: "Lider",
        colCreated: "Criado em",
        empty: "Nenhum lancamento encontrado para o filtro atual."
    },
    "ja-JP": {
        title: "Scrap Parts Report",
        badge: "\u90e8\u54c1\u4f59\u308a",
        subtitle: "\u767b\u9332\u30c7\u30fc\u30bf\u306e\u96c6\u8a08\u3001\u691c\u7d22\u3001CSV \u51fa\u529b\u3092\u884c\u3044\u307e\u3059\u3002",
        exportCsv: "CSV",
        print: "\u5370\u5237",
        metricRows: "\u4ef6\u6570",
        metricQuantity: "\u6570\u91cf",
        metricWeight: "\u91cd\u91cf (g)",
        metricItems: "\u54c1\u76ee",
        start: "\u958b\u59cb\u65e5",
        end: "\u7d42\u4e86\u65e5",
        shift: "\u30b7\u30d5\u30c8",
        machine: "\u8a2d\u5099",
        item: "\u54c1\u76ee",
        search: "\u691c\u7d22",
        searchPlaceholder: "\u30ed\u30c3\u30c8\u3001\u4f5c\u696d\u8005\u3001\u793e\u54e1\u3001\u30ea\u30fc\u30c0\u30fc\u3001\u5099\u8003",
        apply: "\u691c\u7d22",
        clear: "\u30af\u30ea\u30a2",
        all: "\u3059\u3079\u3066",
        tableTitle: "\u767b\u9332\u4e00\u89a7",
        tableSubtitle: "\u6570\u91cf\u3001\u91cd\u91cf\u3001\u54c1\u76ee\u3001\u62c5\u5f53\u8005\u3092\u78ba\u8a8d\u3057\u307e\u3059\u3002",
        colDate: "\u65e5\u4ed8",
        colShift: "\u30b7\u30d5\u30c8",
        colLot: "\u30ed\u30c3\u30c8",
        colOperator: "\u4f5c\u696d\u8005",
        colItem: "\u54c1\u76ee",
        colMachine: "\u8a2d\u5099",
        colQuantity: "\u6570\u91cf",
        colWeight: "\u91cd\u91cf",
        colShain: "\u793e\u54e1",
        colLeader: "\u30ea\u30fc\u30c0\u30fc",
        colCreated: "\u767b\u9332\u65e5\u6642",
        empty: "\u73fe\u5728\u306e\u6761\u4ef6\u3067\u306f\u30c7\u30fc\u30bf\u304c\u3042\u308a\u307e\u305b\u3093\u3002"
    }
};

window.addEventListener("DOMContentLoaded", () => {
    bindEvents();
    send("load");
});

function bindEvents() {
    document.getElementById("btnApply").addEventListener("click", applyFilters);
    document.getElementById("btnClear").addEventListener("click", clearFilters);
    document.getElementById("btnExport").addEventListener("click", exportCsv);
    document.getElementById("btnPrint").addEventListener("click", () => window.print());
    document.getElementById("txtItemInput").addEventListener("input", event => {
        event.target.value = event.target.value.toUpperCase();
    });
}

function send(action, extra = {}) {
    window.chrome.webview.postMessage({ action, ...extra });
}

window.chrome.webview.addEventListener("message", event => {
    const msg = event.data;
    switch (msg.type) {
        case "init":
            hydrateInit(msg.data || {});
            break;
        case "rows":
            hydrateRows(msg.data || {});
            break;
        case "error":
            alert(msg.message || "Erro");
            break;
    }
});

function hydrateInit(data) {
    state.locale = data.locale === "ja-JP" ? "ja-JP" : "pt-BR";
    state.shifts = data.filters?.shifts || [];
    state.machines = data.filters?.machines || [];
    applyLocale();
    fillSelect("cmbShift", state.shifts, "id", true);
    fillSelect("cmbMachine", state.machines, "id", true);
    document.getElementById("dtInicial").value = data.defaults?.dtInicial || "";
    document.getElementById("dtFinal").value = data.defaults?.dtFinal || "";
    document.getElementById("cmbShift").value = String(data.defaults?.shiftId || 0);
    document.getElementById("cmbMachine").value = String(data.defaults?.machineId || 0);
    document.getElementById("txtItemInput").value = data.defaults?.item || "";
    document.getElementById("txtSearchInput").value = data.defaults?.search || "";
}

function hydrateRows(data) {
    state.rows = data.rows || [];
    renderMetrics(data.totals || {});
    renderRows(state.rows);
}

function applyLocale() {
    document.documentElement.lang = state.locale;
    document.title = t("title");
    setText("txtBadge", t("badge"));
    setText("txtTitle", t("title"));
    setText("txtSubtitle", t("subtitle"));
    setText("btnExport", t("exportCsv"));
    setText("btnPrint", t("print"));
    setText("txtMetricRows", t("metricRows"));
    setText("txtMetricQuantity", t("metricQuantity"));
    setText("txtMetricWeight", t("metricWeight"));
    setText("txtMetricItems", t("metricItems"));
    setText("txtStart", t("start"));
    setText("txtEnd", t("end"));
    setText("txtShift", t("shift"));
    setText("txtMachine", t("machine"));
    setText("txtItem", t("item"));
    setText("txtSearch", t("search"));
    document.getElementById("txtSearchInput").placeholder = t("searchPlaceholder");
    setText("btnApply", t("apply"));
    setText("btnClear", t("clear"));
    setText("txtTableTitle", t("tableTitle"));
    setText("txtTableSubtitle", t("tableSubtitle"));
    setText("txtColDate", t("colDate"));
    setText("txtColShift", t("colShift"));
    setText("txtColLot", t("colLot"));
    setText("txtColOperator", t("colOperator"));
    setText("txtColItem", t("colItem"));
    setText("txtColMachine", t("colMachine"));
    setText("txtColQuantity", t("colQuantity"));
    setText("txtColWeight", t("colWeight"));
    setText("txtColShain", t("colShain"));
    setText("txtColLeader", t("colLeader"));
    setText("txtColCreated", t("colCreated"));
}

function applyFilters() {
    send("apply", currentFilter());
}

function clearFilters() {
    document.getElementById("cmbShift").value = "0";
    document.getElementById("cmbMachine").value = "0";
    document.getElementById("txtItemInput").value = "";
    document.getElementById("txtSearchInput").value = "";
    applyFilters();
}

function currentFilter() {
    return {
        dtInicial: document.getElementById("dtInicial").value,
        dtFinal: document.getElementById("dtFinal").value,
        shiftId: Number(document.getElementById("cmbShift").value || 0),
        machineId: Number(document.getElementById("cmbMachine").value || 0),
        item: document.getElementById("txtItemInput").value.trim().toUpperCase(),
        search: document.getElementById("txtSearchInput").value.trim()
    };
}

function renderMetrics(totals) {
    setText("metricRows", totals.total || 0);
    setText("metricQuantity", formatNumber(totals.quantidade));
    setText("metricWeight", formatNumber(totals.pesoGramas));
    setText("metricItems", totals.itens || 0);
}

function renderRows(rows) {
    const tbody = document.getElementById("tblBody");
    if (!rows.length) {
        tbody.innerHTML = `<tr><td colspan="12" class="empty-cell">${escapeHtml(t("empty"))}</td></tr>`;
        return;
    }

    tbody.innerHTML = rows.map(row => `
        <tr>
            <td>${row.Id ?? row.id}</td>
            <td>${escapeHtml(row.Data ?? row.data)}</td>
            <td>${escapeHtml(localize(row.ShiftNamePt ?? row.shiftNamePt, row.ShiftNameJp ?? row.shiftNameJp))}</td>
            <td>${escapeHtml(row.Lote ?? row.lote)}</td>
            <td>${escapeHtml(localize(row.OperatorNamePt ?? row.operatorNamePt, row.OperatorNameJp ?? row.operatorNameJp))}</td>
            <td><strong>${escapeHtml(row.Item ?? row.item)}</strong></td>
            <td>${escapeHtml(localize(row.MachineNamePt ?? row.machineNamePt, row.MachineNameJp ?? row.machineNameJp))}</td>
            <td class="number">${formatNumber(row.Quantidade ?? row.quantidade)}</td>
            <td class="number">${formatNumber(row.PesoGramas ?? row.pesoGramas)}</td>
            <td>${escapeHtml(localize(row.ShainNamePt ?? row.shainNamePt, row.ShainNameJp ?? row.shainNameJp))}</td>
            <td>${escapeHtml(row.Lider ?? row.lider)}</td>
            <td>${escapeHtml(row.CreatedAt ?? row.createdAt)}</td>
        </tr>
    `).join("");
}

function exportCsv() {
    const headers = ["ID", t("colDate"), t("colShift"), t("colLot"), t("colOperator"), t("colItem"), t("colMachine"), t("colQuantity"), t("colWeight"), t("colShain"), t("colLeader"), t("colCreated")];
    const lines = [headers, ...state.rows.map(row => [
        row.Id ?? row.id,
        row.Data ?? row.data,
        localize(row.ShiftNamePt ?? row.shiftNamePt, row.ShiftNameJp ?? row.shiftNameJp),
        row.Lote ?? row.lote,
        localize(row.OperatorNamePt ?? row.operatorNamePt, row.OperatorNameJp ?? row.operatorNameJp),
        row.Item ?? row.item,
        localize(row.MachineNamePt ?? row.machineNamePt, row.MachineNameJp ?? row.machineNameJp),
        row.Quantidade ?? row.quantidade,
        row.PesoGramas ?? row.pesoGramas,
        localize(row.ShainNamePt ?? row.shainNamePt, row.ShainNameJp ?? row.shainNameJp),
        row.Lider ?? row.lider,
        row.CreatedAt ?? row.createdAt
    ])];
    const csv = lines.map(cols => cols.map(csvEscape).join(";")).join("\r\n");
    const blob = new Blob(["\uFEFF" + csv], { type: "text/csv;charset=utf-8" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = `sobra-de-peca-${new Date().toISOString().slice(0, 10)}.csv`;
    link.click();
    URL.revokeObjectURL(link.href);
}

function fillSelect(id, items, valueField, withAll) {
    const select = document.getElementById(id);
    const options = withAll ? [`<option value="0">${escapeHtml(t("all"))}</option>`] : [];
    items.forEach(item => {
        options.push(`<option value="${escapeHtmlAttr(item[valueField])}">${escapeHtml(localize(item.namePt, item.nameJp))}</option>`);
    });
    select.innerHTML = options.join("");
}

function localize(pt, jp) {
    return state.locale === "ja-JP" ? (jp || pt || "") : (pt || jp || "");
}

function formatNumber(value) {
    const numeric = Number(value || 0);
    return Number.isFinite(numeric)
        ? numeric.toLocaleString(state.locale, { maximumFractionDigits: 2 })
        : "";
}

function csvEscape(value) {
    const text = String(value ?? "");
    return /[;"\r\n]/.test(text) ? `"${text.replace(/"/g, '""')}"` : text;
}

function setText(id, value) {
    const element = document.getElementById(id);
    if (element) element.textContent = value ?? "";
}

function t(key) {
    return I18N[state.locale]?.[key] ?? I18N["pt-BR"][key] ?? key;
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
    return escapeHtml(value);
}
