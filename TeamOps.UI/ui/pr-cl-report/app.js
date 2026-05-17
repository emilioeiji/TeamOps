const state = {
    locale: "pt-BR",
    kind: "PR",
    rows: []
};

const I18N = {
    "pt-BR": {
        title: kind => `Relatorio ${kind} - TeamOps`,
        heading: kind => `Relatorio ${kind}`,
        subtitle: "Consulta dos documentos gerados por periodo, setor, categoria e prioridade.",
        report: "Relatorio",
        start: "Inicio",
        end: "Fim",
        sector: "Setor",
        category: "Categoria",
        priority: "Prioridade",
        search: "Buscar",
        apply: "Aplicar",
        all: "Todos",
        documents: "Documentos",
        sectors: "Setores",
        authors: "Autores",
        date: "Data",
        docTitle: "Titulo",
        author: "Autor",
        file: "Arquivo",
        open: "Abrir",
        empty: "Nenhum documento encontrado."
    },
    "ja-JP": {
        title: kind => `${kind} \u30ec\u30dd\u30fc\u30c8 - TeamOps`,
        heading: kind => `${kind} \u30ec\u30dd\u30fc\u30c8`,
        subtitle: "\u671f\u9593\u3001\u30bb\u30af\u30bf\u30fc\u3001\u30ab\u30c6\u30b4\u30ea\u3001\u512a\u5148\u5ea6\u3067\u4f5c\u6210\u6e08\u307f\u6587\u66f8\u3092\u691c\u7d22\u3057\u307e\u3059\u3002",
        report: "\u30ec\u30dd\u30fc\u30c8",
        start: "\u958b\u59cb",
        end: "\u7d42\u4e86",
        sector: "\u30bb\u30af\u30bf\u30fc",
        category: "\u30ab\u30c6\u30b4\u30ea",
        priority: "\u512a\u5148\u5ea6",
        search: "\u691c\u7d22",
        apply: "\u9069\u7528",
        all: "\u3059\u3079\u3066",
        documents: "\u6587\u66f8",
        sectors: "\u30bb\u30af\u30bf\u30fc",
        authors: "\u4f5c\u6210\u8005",
        date: "\u65e5\u4ed8",
        docTitle: "\u30bf\u30a4\u30c8\u30eb",
        author: "\u4f5c\u6210\u8005",
        file: "\u30d5\u30a1\u30a4\u30eb",
        open: "\u958b\u304f",
        empty: "\u6587\u66f8\u304c\u898b\u3064\u304b\u308a\u307e\u305b\u3093\u3002"
    }
};

document.addEventListener("DOMContentLoaded", () => {
    bindEvents();
    post({ action: "load" });
});

window.chrome?.webview?.addEventListener("message", event => {
    const payload = event.data;
    if (!payload?.type) return;

    if (payload.type === "init") {
        hydrate(payload.data || {});
        return;
    }

    if (payload.type === "rows") {
        renderRows(payload.data || {});
        return;
    }

    if (payload.type === "error") {
        showNotice(payload.message || "", "error");
    }
});

function bindEvents() {
    document.getElementById("btnApply").addEventListener("click", requestRows);
    ["start", "end", "sectorId", "categoryId", "priorityId"].forEach(id => {
        document.getElementById(id).addEventListener("change", requestRows);
    });
    document.getElementById("search").addEventListener("input", debounce(requestRows, 250));
    document.getElementById("rows").addEventListener("click", event => {
        const button = event.target.closest("[data-file]");
        if (!button) return;
        post({ action: "open_file", fileName: button.dataset.file || "" });
    });
}

function hydrate(data) {
    state.locale = data.locale === "ja-JP" ? "ja-JP" : "pt-BR";
    state.kind = data.kind || "PR";
    applyLocale();
    document.getElementById("start").value = data.defaults?.start || "";
    document.getElementById("end").value = data.defaults?.end || "";
    fillSelect("sectorId", data.sectors || []);
    fillSelect("categoryId", data.categories || []);
    fillSelect("priorityId", data.priorities || []);
    requestRows();
}

function applyLocale() {
    document.documentElement.lang = state.locale;
    document.title = t("title")(state.kind);
    setText("txtBadge", `${t("report")} ${state.kind}`);
    setText("txtTitle", t("heading")(state.kind));
    setText("txtSubtitle", t("subtitle"));
    setText("txtStart", t("start"));
    setText("txtEnd", t("end"));
    setText("txtSector", t("sector"));
    setText("txtCategory", t("category"));
    setText("txtPriority", t("priority"));
    setText("txtSearch", t("search"));
    setText("btnApply", t("apply"));
    setText("txtCount", t("documents"));
    setText("txtSectors", t("sectors"));
    setText("txtAuthors", t("authors"));
    setText("txtTableTitle", t("documents"));
    setText("thDate", t("date"));
    setText("thTitle", t("docTitle"));
    setText("thSector", t("sector"));
    setText("thCategory", t("category"));
    setText("thPriority", t("priority"));
    setText("thAuthor", t("author"));
    setText("thFile", t("file"));
}

function fillSelect(id, items) {
    const key = state.locale === "ja-JP" ? "nameJp" : "namePt";
    document.getElementById(id).innerHTML = [`<option value="0">${escapeHtml(t("all"))}</option>`]
        .concat(items.map(item => `<option value="${item.id}">${escapeHtml(item[key] || item.namePt || item.nameJp || "")}</option>`))
        .join("");
}

function requestRows() {
    post({
        action: "filter",
        start: document.getElementById("start").value,
        end: document.getElementById("end").value,
        sectorId: Number(document.getElementById("sectorId").value || 0),
        categoryId: Number(document.getElementById("categoryId").value || 0),
        priorityId: Number(document.getElementById("priorityId").value || 0),
        search: document.getElementById("search").value.trim()
    });
}

function renderRows(data) {
    hideNotice();
    state.rows = data.rows || [];
    setText("sumCount", data.totals?.count ?? 0);
    setText("sumSectors", data.totals?.sectors ?? 0);
    setText("sumAuthors", data.totals?.authors ?? 0);
    setText("rowCount", state.rows.length);

    const body = document.getElementById("rows");
    if (!state.rows.length) {
        body.innerHTML = `<tr><td colspan="8" class="empty">${escapeHtml(t("empty"))}</td></tr>`;
        return;
    }

    body.innerHTML = state.rows.map(row => `
        <tr>
            <td>${row.id}</td>
            <td>${escapeHtml(formatDate(row.emissionDate))}</td>
            <td><strong>${escapeHtml(row.title)}</strong></td>
            <td>${escapeHtml(localized(row.sectorNamePt, row.sectorNameJp))}</td>
            <td>${escapeHtml(localized(row.categoryNamePt, row.categoryNameJp))}</td>
            <td>${escapeHtml(localized(row.priorityNamePt, row.priorityNameJp))}</td>
            <td>${escapeHtml(localized(row.authorNamePt, row.authorNameJp))}</td>
            <td><button class="open-btn" type="button" data-file="${escapeAttr(row.fileName)}">${escapeHtml(t("open"))}</button><small>${escapeHtml(row.fileName)}</small></td>
        </tr>
    `).join("");
}

function localized(pt, jp) {
    return state.locale === "ja-JP" ? (jp || pt || "-") : (pt || jp || "-");
}

function formatDate(value) {
    if (!value) return "-";
    const [year, month, day] = value.split("-").map(Number);
    if (!year || !month || !day) return value;
    return new Intl.DateTimeFormat(state.locale, { dateStyle: "short" }).format(new Date(year, month - 1, day));
}

function showNotice(message, kind) {
    const notice = document.getElementById("notice");
    notice.textContent = message;
    notice.className = `notice notice-${kind || "warning"}`;
}

function hideNotice() {
    const notice = document.getElementById("notice");
    notice.className = "notice hidden";
    notice.textContent = "";
}

function post(payload) {
    window.chrome?.webview?.postMessage(payload);
}

function setText(id, value) {
    const element = document.getElementById(id);
    if (element) element.textContent = value ?? "";
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

function escapeAttr(value) {
    return escapeHtml(value).replaceAll("'", "&#39;");
}

function debounce(callback, delay) {
    let timer = null;
    return (...args) => {
        clearTimeout(timer);
        timer = setTimeout(() => callback(...args), delay);
    };
}
