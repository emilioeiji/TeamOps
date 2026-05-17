const state = {
    locale: "pt-BR",
    kind: "PR",
    nextId: 1,
    sectors: [],
    categories: [],
    priorities: []
};

const I18N = {
    "pt-BR": {
        title: kind => `${kind} - TeamOps`,
        heading: kind => `Novo ${kind}`,
        subtitle: "Preencha os dados principais para gerar o arquivo Excel pelo template configurado.",
        author: "Autor",
        sector: "Setor",
        category: "Categoria",
        priority: "Prioridade",
        docTitle: "Titulo",
        fileName: "Nome do arquivo",
        clear: "Limpar",
        save: "Salvar e gerar Excel",
        loading: "Gerando arquivo...",
        select: "Selecione",
        saved: "Documento salvo.",
        required: "Preencha setor, categoria, prioridade e titulo."
    },
    "ja-JP": {
        title: kind => `${kind} - TeamOps`,
        heading: kind => `${kind} \u65b0\u898f\u4f5c\u6210`,
        subtitle: "\u8a2d\u5b9a\u6e08\u307f\u30c6\u30f3\u30d7\u30ec\u30fc\u30c8\u304b\u3089 Excel \u30d5\u30a1\u30a4\u30eb\u3092\u4f5c\u6210\u3057\u307e\u3059\u3002",
        author: "\u4f5c\u6210\u8005",
        sector: "\u30bb\u30af\u30bf\u30fc",
        category: "\u30ab\u30c6\u30b4\u30ea",
        priority: "\u512a\u5148\u5ea6",
        docTitle: "\u30bf\u30a4\u30c8\u30eb",
        fileName: "\u30d5\u30a1\u30a4\u30eb\u540d",
        clear: "\u30af\u30ea\u30a2",
        save: "\u4fdd\u5b58\u3057\u3066 Excel \u4f5c\u6210",
        loading: "\u30d5\u30a1\u30a4\u30eb\u4f5c\u6210\u4e2d...",
        select: "\u9078\u629e",
        saved: "\u4fdd\u5b58\u3057\u307e\u3057\u305f\u3002",
        required: "\u30bb\u30af\u30bf\u30fc\u3001\u30ab\u30c6\u30b4\u30ea\u3001\u512a\u5148\u5ea6\u3001\u30bf\u30a4\u30c8\u30eb\u3092\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002"
    }
};

document.addEventListener("DOMContentLoaded", () => {
    bindEvents();
    post({ action: "load" });
});

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
        hideLoading();
        state.nextId = payload.data?.nextId || state.nextId + 1;
        clearForm();
        showNotice(payload.data?.message || t("saved"), "success");
        return;
    }

    if (payload.type === "error") {
        hideLoading();
        showNotice(payload.message || "", "error");
    }
});

function bindEvents() {
    document.getElementById("docTitle").addEventListener("input", updateFileName);
    document.getElementById("btnClear").addEventListener("click", clearForm);
    document.getElementById("btnSave").addEventListener("click", saveDocument);
}

function hydrate(data) {
    state.locale = data.locale === "ja-JP" ? "ja-JP" : "pt-BR";
    state.kind = data.kind || "PR";
    state.nextId = Number(data.nextId || 1);
    state.sectors = data.sectors || [];
    state.categories = data.categories || [];
    state.priorities = data.priorities || [];

    document.documentElement.lang = state.locale;
    document.title = t("title")(state.kind);
    setText("txtBadge", state.kind);
    setText("txtTitle", t("heading")(state.kind));
    setText("txtSubtitle", t("subtitle"));
    setText("txtAuthorLabel", t("author"));
    setText("txtSector", t("sector"));
    setText("txtCategory", t("category"));
    setText("txtPriority", t("priority"));
    setText("txtDocTitle", t("docTitle"));
    setText("txtFileName", t("fileName"));
    setText("btnClear", t("clear"));
    setText("btnSave", t("save"));
    setText("txtLoading", t("loading"));
    setText("lblAuthor", data.author || "-");
    setText("lblDate", data.emissionDate || "-");

    fillSelect("sectorId", state.sectors);
    fillSelect("categoryId", state.categories);
    fillSelect("priorityId", state.priorities);
    updateFileName();
}

function fillSelect(id, items) {
    const select = document.getElementById(id);
    const nameKey = state.locale === "ja-JP" ? "nameJp" : "namePt";
    select.innerHTML = [`<option value="">${escapeHtml(t("select"))}</option>`]
        .concat(items.map(item => `<option value="${item.id}">${escapeHtml(item[nameKey] || item.namePt || item.nameJp || "")}</option>`))
        .join("");
}

function updateFileName() {
    const title = document.getElementById("docTitle").value.trim();
    document.getElementById("fileName").value = title
        ? `${state.kind}_${state.nextId}_${sanitizeFileName(title)}.xlsx`
        : "";
}

function saveDocument() {
    const payload = {
        action: "save",
        sectorId: Number(document.getElementById("sectorId").value || 0),
        categoryId: Number(document.getElementById("categoryId").value || 0),
        priorityId: Number(document.getElementById("priorityId").value || 0),
        title: document.getElementById("docTitle").value.trim(),
        fileName: document.getElementById("fileName").value.trim()
    };

    if (!payload.sectorId || !payload.categoryId || !payload.priorityId || !payload.title) {
        showNotice(t("required"), "warning");
        return;
    }

    showLoading();
    window.setTimeout(() => post(payload), 40);
}

function clearForm() {
    document.getElementById("sectorId").value = "";
    document.getElementById("categoryId").value = "";
    document.getElementById("priorityId").value = "";
    document.getElementById("docTitle").value = "";
    updateFileName();
}

function sanitizeFileName(value) {
    return String(value || "")
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .replace(/[<>:"/\\|?*]+/g, "")
        .replace(/\s+/g, "_")
        .slice(0, 80);
}

function showLoading() {
    document.getElementById("loadingOverlay").classList.remove("hidden");
    document.getElementById("btnSave").disabled = true;
}

function hideLoading() {
    document.getElementById("loadingOverlay").classList.add("hidden");
    document.getElementById("btnSave").disabled = false;
}

function showNotice(message, kind) {
    const notice = document.getElementById("notice");
    notice.textContent = message;
    notice.className = `notice notice-${kind || "warning"}`;
}

function post(payload) {
    window.chrome?.webview?.postMessage(payload);
}

function setText(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = value ?? "";
    }
}

function t(key) {
    return I18N[state.locale]?.[key] || I18N["pt-BR"][key] || key;
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;");
}
