const state = {
    locale: "pt-BR"
};

const palette = ["#1f4ea3", "#0f766e", "#b45309", "#6d28d9", "#be123c", "#0f6da8", "#4d7c0f", "#475569"];

const I18N = {
    "pt-BR": {
        documentTitle: "Grafico Follow",
        headerTitle: "Grafico Follow",
        headerSubtitle: "Painel visual dos acompanhamentos por tipo, motivo, turno e operador.",
        labelStart: "Data Inicial",
        labelEnd: "Data Final",
        labelShift: "Turno",
        labelType: "Tipo",
        labelReason: "Motivo",
        labelSector: "Setor",
        btnApply: "Atualizar",
        sumTotal: "Total no periodo",
        sumErrors: "Erros",
        sumGuidance: "Orientacoes",
        sumSectors: "Setores ativos",
        chartTypeTitle: "Distribuicao por Tipo",
        chartTypeSubtitle: "Volume de acompanhamentos por classificacao.",
        chartReasonTitle: "Distribuicao por Motivo",
        chartReasonSubtitle: "Principais razoes cadastradas no periodo.",
        chartOperatorTitle: "Top Operadores",
        chartOperatorSubtitle: "Operadores com mais acompanhamentos registrados.",
        chartShiftTitle: "Distribuicao por Turno",
        chartShiftSubtitle: "Leitura rapida para comparar concentracao do periodo.",
        chartSectorTitle: "Participacao por Setor",
        chartSectorSubtitle: "Setores com maior incidencia dentro dos filtros atuais.",
        empty: "Nenhum dado para os filtros informados.",
        total: "total",
        records: " registro(s)",
        errorFallback: "Erro ao carregar o grafico."
    },
    "ja-JP": {
        documentTitle: "\u30d5\u30a9\u30ed\u30fc\u30b0\u30e9\u30d5",
        headerTitle: "\u30d5\u30a9\u30ed\u30fc\u30b0\u30e9\u30d5",
        headerSubtitle: "\u7a2e\u5225\u3001\u7406\u7531\u3001\u30b7\u30d5\u30c8\u3001\u4f5c\u696d\u8005\u5225\u3067\u30d5\u30a9\u30ed\u30fc\u3092\u53ef\u8996\u5316\u3057\u307e\u3059\u3002",
        labelStart: "\u958b\u59cb\u65e5",
        labelEnd: "\u7d42\u4e86\u65e5",
        labelShift: "\u30b7\u30d5\u30c8",
        labelType: "\u7a2e\u5225",
        labelReason: "\u7406\u7531",
        labelSector: "\u30bb\u30af\u30bf\u30fc",
        btnApply: "\u66f4\u65b0",
        sumTotal: "\u671f\u9593\u5408\u8a08",
        sumErrors: "\u30a8\u30e9\u30fc",
        sumGuidance: "\u6307\u5c0e",
        sumSectors: "\u7a3c\u50cd\u30bb\u30af\u30bf\u30fc",
        chartTypeTitle: "\u7a2e\u5225\u5206\u5e03",
        chartTypeSubtitle: "\u5206\u985e\u5225\u306e\u30d5\u30a9\u30ed\u30fc\u4ef6\u6570\u3067\u3059\u3002",
        chartReasonTitle: "\u7406\u7531\u5206\u5e03",
        chartReasonSubtitle: "\u671f\u9593\u5185\u306b\u767b\u9332\u3055\u308c\u305f\u4e3b\u306a\u7406\u7531\u3067\u3059\u3002",
        chartOperatorTitle: "\u4e0a\u4f4d\u4f5c\u696d\u8005",
        chartOperatorSubtitle: "\u30d5\u30a9\u30ed\u30fc\u767b\u9332\u304c\u591a\u3044\u4f5c\u696d\u8005\u3067\u3059\u3002",
        chartShiftTitle: "\u30b7\u30d5\u30c8\u5206\u5e03",
        chartShiftSubtitle: "\u671f\u9593\u5185\u306e\u504f\u308a\u3092\u7d20\u65e9\u304f\u6bd4\u8f03\u3067\u304d\u307e\u3059\u3002",
        chartSectorTitle: "\u30bb\u30af\u30bf\u30fc\u5225\u69cb\u6210",
        chartSectorSubtitle: "\u73fe\u5728\u306e\u30d5\u30a3\u30eb\u30bf\u3067\u767a\u751f\u304c\u591a\u3044\u30bb\u30af\u30bf\u30fc\u3067\u3059\u3002",
        empty: "\u9078\u629e\u4e2d\u306e\u30d5\u30a3\u30eb\u30bf\u306b\u30c7\u30fc\u30bf\u304c\u3042\u308a\u307e\u305b\u3093\u3002",
        total: "\u5408\u8a08",
        records: " \u4ef6",
        errorFallback: "\u30b0\u30e9\u30d5\u306e\u8aad\u307f\u8fbc\u307f\u306b\u5931\u6557\u3057\u307e\u3057\u305f\u3002"
    }
};

document.addEventListener("DOMContentLoaded", () => {
    bind();
    post("load");
});

function bind() {
    document.getElementById("btnAplicar").addEventListener("click", applyFilters);
}

window.chrome?.webview?.addEventListener("message", event => {
    const payload = event.data;
    if (!payload?.type) return;

    if (payload.type === "init") {
        setLocale(payload.locale);
        hydrateFilters(payload.data);
        return;
    }

    if (payload.type === "dashboard") {
        renderDashboard(payload.data);
        return;
    }

    if (payload.type === "error") {
        showToast(payload.message || t("errorFallback"));
    }
});

function setLocale(locale) {
    state.locale = locale === "ja-JP" ? "ja-JP" : "pt-BR";
    document.documentElement.lang = state.locale;
    document.title = t("documentTitle");

    document.querySelectorAll("[data-i18n]").forEach(element => {
        element.textContent = t(element.dataset.i18n);
    });
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
}

function renderDashboard(data) {
    setText("periodLabel", data.periodLabel || "-");
    setText("sumTotal", data.summary?.total ?? 0);
    setText("sumErrors", data.summary?.errorCount ?? 0);
    setText("sumGuidance", data.summary?.guidanceCount ?? 0);
    setText("sumSectors", data.summary?.sectorCount ?? 0);

    renderBars("chartType", data.charts?.byType || []);
    renderBars("chartReason", data.charts?.byReason || []);
    renderBars("chartOperator", data.charts?.byOperator || []);
    renderBars("chartSector", data.charts?.bySector || []);
    renderDonut(data.charts?.byShift || []);
}

function applyFilters() {
    post("apply", {
        dtInicial: document.getElementById("dtInicial").value,
        dtFinal: document.getElementById("dtFinal").value,
        shiftId: Number(document.getElementById("cmbShift").value || 0),
        typeId: Number(document.getElementById("cmbType").value || 0),
        reasonId: Number(document.getElementById("cmbReason").value || 0),
        sectorId: Number(document.getElementById("cmbSector").value || 0)
    });
}

function renderBars(id, items) {
    const root = document.getElementById(id);
    root.innerHTML = "";

    if (!items.length) {
        root.innerHTML = `<div class="empty-state">${escapeHtml(t("empty"))}</div>`;
        return;
    }

    const max = Math.max(...items.map(item => item.count), 1);

    items.forEach((item, index) => {
        const percent = Math.max((item.count / max) * 100, 6);
        const row = document.createElement("div");
        row.className = "bar-row";
        row.innerHTML = `
            <div class="bar-copy">
                <strong title="${escapeHtml(item.label)}">${escapeHtml(item.label)}</strong>
                <span>${item.count}${escapeHtml(t("records"))}</span>
            </div>
            <div class="bar-track">
                <div class="bar-fill" style="width:${percent}%; background:${palette[index % palette.length]}"></div>
            </div>
        `;
        root.appendChild(row);
    });
}

function renderDonut(items) {
    const donut = document.getElementById("shiftDonut");
    const legend = document.getElementById("shiftLegend");
    donut.innerHTML = "";
    legend.innerHTML = "";

    if (!items.length) {
        donut.className = "donut-chart donut-chart-empty";
        donut.textContent = "0";
        legend.innerHTML = `<div class="empty-state">${escapeHtml(t("empty"))}</div>`;
        return;
    }

    const total = items.reduce((sum, item) => sum + item.count, 0);
    let start = 0;
    const stops = items.map((item, index) => {
        const portion = (item.count / total) * 100;
        const end = start + portion;
        const segment = `${palette[index % palette.length]} ${start}% ${end}%`;
        start = end;
        return segment;
    });

    donut.className = "donut-chart";
    donut.style.background = `conic-gradient(${stops.join(", ")})`;
    donut.innerHTML = `<div class="donut-core"><strong>${total}</strong><span>${escapeHtml(t("total"))}</span></div>`;

    items.forEach((item, index) => {
        const pct = total === 0 ? 0 : Math.round((item.count / total) * 100);
        const row = document.createElement("div");
        row.className = "legend-row";
        row.innerHTML = `
            <span class="legend-dot" style="background:${palette[index % palette.length]}"></span>
            <span class="legend-label">${escapeHtml(item.label)}</span>
            <strong>${item.count}</strong>
            <small>${pct}%</small>
        `;
        legend.appendChild(row);
    });
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

function setText(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = String(value ?? "-");
    }
}

function showToast(message) {
    const toast = document.getElementById("toast");
    toast.textContent = message;
    toast.classList.remove("hidden");
    clearTimeout(showToast._timer);
    showToast._timer = setTimeout(() => toast.classList.add("hidden"), 2800);
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
