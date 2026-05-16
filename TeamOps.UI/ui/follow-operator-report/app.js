const state = {
    locale: "pt-BR"
};

const I18N = {
    "pt-BR": {
        documentTitle: "Historico Follow por Operador",
        print: "Imprimir",
        savePdf: "Salvar PDF",
        eyebrow: "Historico de acompanhamento",
        reportTitle: "FOLLOW HISTORY BY OPERATOR",
        generatedAtPrefix: "Gerado em",
        operatorLabel: "Operador",
        codePrefix: "Codigo FJ:",
        startDate: "Admissao",
        total: "Total",
        lastDate: "Ultimo registro",
        empty: "Nenhum acompanhamento encontrado para este operador.",
        executor: "Executor",
        witness: "Testemunha",
        reason: "Motivo",
        type: "Tipo",
        local: "Local",
        equipment: "Equipamento",
        sector: "Setor",
        description: "Descricao",
        guidance: "Orientacao",
        notice: "Aviso"
    },
    "ja-JP": {
        documentTitle: "\u4f5c\u696d\u8005\u30d5\u30a9\u30ed\u30fc\u5c65\u6b74",
        print: "\u5370\u5237",
        savePdf: "PDF \u4fdd\u5b58",
        eyebrow: "\u30d5\u30a9\u30ed\u30fc\u5c65\u6b74",
        reportTitle: "\u4f5c\u696d\u8005\u5225\u30d5\u30a9\u30ed\u30fc\u5c65\u6b74",
        generatedAtPrefix: "\u51fa\u529b\u65e5\u6642",
        operatorLabel: "\u4f5c\u696d\u8005",
        codePrefix: "FJ \u30b3\u30fc\u30c9:",
        startDate: "\u5165\u793e\u65e5",
        total: "\u5408\u8a08",
        lastDate: "\u6700\u65b0\u767b\u9332",
        empty: "\u3053\u306e\u4f5c\u696d\u8005\u306e\u30d5\u30a9\u30ed\u30fc\u306f\u3042\u308a\u307e\u305b\u3093\u3002",
        executor: "\u5b9f\u65bd\u8005",
        witness: "\u7acb\u4f1a\u3044",
        reason: "\u7406\u7531",
        type: "\u7a2e\u5225",
        local: "\u5834\u6240",
        equipment: "\u8a2d\u5099",
        sector: "\u30bb\u30af\u30bf\u30fc",
        description: "\u5185\u5bb9",
        guidance: "\u6307\u5c0e",
        notice: "\u304a\u77e5\u3089\u305b"
    }
};

document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("btnPrint").addEventListener("click", () => post("print"));
    document.getElementById("btnPdf").addEventListener("click", () => post("save_pdf"));
    post("load");
});

window.chrome?.webview?.addEventListener("message", event => {
    const payload = event.data;
    if (!payload?.type) return;

    if (payload.type === "init") {
        setLocale(payload.data?.locale);
        hydrate(payload.data || {});
        return;
    }

    if (payload.type === "notify") {
        showToast(payload.data?.title || t("notice"), payload.data?.message || "");
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

function hydrate(data) {
    setText("generatedAt", data.generatedAt);
    setText("operatorName", `${data.operatorInfo?.nameRomanji || "-"} / ${data.operatorInfo?.nameNihongo || "-"}`);
    setText("operatorCode", `${t("codePrefix")} ${data.operatorInfo?.codigoFJ || "-"}`);
    setText("startDate", data.operatorInfo?.startDate);
    setText("totalCount", data.operatorInfo?.total ?? 0);
    setText("lastDate", data.operatorInfo?.lastDate);
    document.getElementById("logo").src = data.logoUrl || "";

    const timeline = document.getElementById("timeline");
    const records = data.records || [];

    if (!records.length) {
        timeline.innerHTML = `<article class="record-card"><p class="empty-state">${escapeHtml(t("empty"))}</p></article>`;
        return;
    }

    timeline.innerHTML = records.map(record => `
        <article class="record-card">
            <div class="record-head">
                <strong>${escapeHtml(record.date)}</strong>
                <span>${escapeHtml(record.shift)}</span>
            </div>
            <div class="record-grid">
                <div><span class="section-label">${escapeHtml(t("executor"))}</span><strong>${escapeHtml(record.executor)}</strong></div>
                <div><span class="section-label">${escapeHtml(t("witness"))}</span><strong>${escapeHtml(record.witness)}</strong></div>
                <div><span class="section-label">${escapeHtml(t("reason"))}</span><strong>${escapeHtml(record.reason)}</strong></div>
                <div><span class="section-label">${escapeHtml(t("type"))}</span><strong>${escapeHtml(record.type)}</strong></div>
                <div><span class="section-label">${escapeHtml(t("local"))}</span><strong>${escapeHtml(record.local)}</strong></div>
                <div><span class="section-label">${escapeHtml(t("equipment"))}</span><strong>${escapeHtml(record.equipment)}</strong></div>
                <div><span class="section-label">${escapeHtml(t("sector"))}</span><strong>${escapeHtml(record.sector)}</strong></div>
            </div>
            <div class="content-card">
                <span class="section-label">${escapeHtml(t("description"))}</span>
                <div class="content-box">${escapeHtml(record.description)}</div>
            </div>
            <div class="content-card">
                <span class="section-label">${escapeHtml(t("guidance"))}</span>
                <div class="content-box">${escapeHtml(record.guidance)}</div>
            </div>
        </article>
    `).join("");
}

function post(action) {
    window.chrome?.webview?.postMessage({ action });
}

function setText(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = value ?? "-";
    }
}

function showToast(title, message) {
    const root = document.getElementById("toast");
    document.getElementById("toastTitle").textContent = title;
    document.getElementById("toastMessage").textContent = message;
    root.classList.remove("hidden");
    clearTimeout(showToast._timer);
    showToast._timer = setTimeout(() => root.classList.add("hidden"), 2800);
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
