const state = {
    locale: "pt-BR"
};

const I18N = {
    "pt-BR": {
        documentTitle: "Follow-up Individual",
        print: "Imprimir",
        savePdf: "Salvar PDF",
        generatedAtPrefix: "Gerado em",
        operatorLabel: "Operador",
        codePrefix: "Codigo FJ:",
        startDate: "Admissao",
        followDate: "Data",
        shift: "Turno",
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
        documentTitle: "\u500b\u5225\u30d5\u30a9\u30ed\u30fc",
        print: "\u5370\u5237",
        savePdf: "PDF \u4fdd\u5b58",
        generatedAtPrefix: "\u51fa\u529b\u65e5\u6642",
        operatorLabel: "\u4f5c\u696d\u8005",
        codePrefix: "FJ \u30b3\u30fc\u30c9:",
        startDate: "\u5165\u793e\u65e5",
        followDate: "\u65e5\u4ed8",
        shift: "\u30b7\u30d5\u30c8",
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
    setText("reportTitle", data.header?.title);
    setText("reportSubtitle", data.header?.subtitle);
    setText("generatedAt", data.generatedAt);
    setText("operatorName", `${data.operatorInfo?.nameRomanji || "-"} / ${data.operatorInfo?.nameNihongo || "-"}`);
    setText("operatorCode", `${t("codePrefix")} ${data.operatorInfo?.codigoFJ || "-"}`);
    setText("startDate", data.operatorInfo?.startDate);
    setText("followDate", data.follow?.date);
    setText("shift", data.follow?.shift);
    setText("executor", data.follow?.executor);
    setText("witness", data.follow?.witness);
    setText("reason", data.follow?.reason);
    setText("type", data.follow?.type);
    setText("local", data.follow?.local);
    setText("equipment", data.follow?.equipment);
    setText("sector", data.follow?.sector);
    setText("description", data.follow?.description);
    setText("guidance", data.follow?.guidance);

    const logo = document.getElementById("logo");
    logo.src = data.logoUrl || "";
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
