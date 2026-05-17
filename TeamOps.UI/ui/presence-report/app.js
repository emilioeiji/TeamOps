const state = {
    locale: "pt-BR",
    rows: []
};

const toast = {
    root: document.getElementById("toast"),
    title: document.getElementById("toastTitle"),
    message: document.getElementById("toastMessage"),
    timer: null
};

const I18N = {
    "pt-BR": {
        title: "Relatorio de Presenca - TeamOps",
        badge: "Presenca",
        heading: "Relatorio de Presenca",
        subtitle: "Consolidado por operador usando escala do Haidai, presenca, Todoke e movimentos.",
        window: "Periodo",
        start: "Inicio",
        end: "Fim",
        shift: "Turno",
        sector: "Setor",
        group: "Grupo",
        status: "Status",
        search: "Buscar",
        apply: "Aplicar",
        all: "Todos",
        tableTitle: "Operadores",
        tableSubtitle: "Percentual calculado sobre dias escalados no periodo.",
        operator: "Operador",
        scheduled: "Escalado",
        present: "Conforme",
        percent: "%",
        issues: "Ocorrencias",
        last: "Ultimo status",
        empty: "Nenhum registro encontrado.",
        loading: "Carregando...",
        operators: "Operadores",
        scheduledDays: "Dias escalados",
        presentDays: "Dias conformes",
        yukyu: "Yukyu",
        falta: "Faltas",
        late: "Atrasos",
        early: "Saida antecipada",
        avgPresence: "Media presenca",
        pending: "Todoke pendente",
        statusPresent: "Presenca",
        statusIssue: "Ocorrencias",
        statusLate: "Atraso",
        statusEarly: "Saida antecipada",
        notifyTitle: "Aviso"
    },
    "ja-JP": {
        title: "Attendance Report - TeamOps",
        badge: "Attendance",
        heading: "Attendance Report",
        subtitle: "Operator attendance using Haidai lineup, attendance, Todoke, and movement records.",
        window: "Period",
        start: "Start",
        end: "End",
        shift: "Shift",
        sector: "Sector",
        group: "Group",
        status: "Status",
        search: "Search",
        apply: "Apply",
        all: "All",
        tableTitle: "Operators",
        tableSubtitle: "Percent is calculated from scheduled days in the selected period.",
        operator: "Operator",
        scheduled: "Scheduled",
        present: "Compliant",
        percent: "%",
        issues: "Issues",
        last: "Latest status",
        empty: "No records found.",
        loading: "Loading...",
        operators: "Operators",
        scheduledDays: "Scheduled days",
        presentDays: "Compliant days",
        yukyu: "Yukyu",
        falta: "Absences",
        late: "Late",
        early: "Early leave",
        avgPresence: "Avg attendance",
        pending: "Pending Todoke",
        statusPresent: "Attendance",
        statusIssue: "Issues",
        statusLate: "Late",
        statusEarly: "Early leave",
        notifyTitle: "Notice"
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

    if (payload.type === "report") {
        renderReport(payload.data || {});
        return;
    }

    if (payload.type === "error") {
        showToast(t("notifyTitle"), payload.message || "");
    }
});

function bindEvents() {
    document.getElementById("btnApply").addEventListener("click", requestReport);
    ["startDate", "endDate", "shiftId", "sectorId", "groupId", "status"].forEach(id => {
        document.getElementById(id).addEventListener("change", requestReport);
    });
    document.getElementById("search").addEventListener("input", debounce(requestReport, 250));
}

function hydrate(data) {
    state.locale = data.locale === "ja-JP" ? "ja-JP" : "pt-BR";
    applyLocale();
    fillLookup("shiftId", data.shifts || []);
    fillLookup("sectorId", data.sectors || []);
    fillLookup("groupId", data.groups || []);

    document.getElementById("startDate").value = data.defaults?.startDateIso || "";
    document.getElementById("endDate").value = data.defaults?.endDateIso || "";
    document.getElementById("shiftId").value = String(data.defaults?.shiftId || 0);
    document.getElementById("sectorId").value = String(data.defaults?.sectorId || 0);
    document.getElementById("groupId").value = String(data.defaults?.groupId || 0);
    document.getElementById("status").value = data.defaults?.status || "all";

    requestReport();
}

function applyLocale() {
    document.title = t("title");
    setText("txtBadge", t("badge"));
    setText("txtTitle", t("heading"));
    setText("txtSubtitle", t("subtitle"));
    setText("txtWindow", t("window"));
    setText("txtStart", t("start"));
    setText("txtEnd", t("end"));
    setText("txtShift", t("shift"));
    setText("txtSector", t("sector"));
    setText("txtGroup", t("group"));
    setText("txtStatus", t("status"));
    setText("txtSearch", t("search"));
    setText("btnApply", t("apply"));
    setText("txtTableTitle", t("tableTitle"));
    setText("txtTableSubtitle", t("tableSubtitle"));
    setText("thOperator", t("operator"));
    setText("thGroup", t("group"));
    setText("thScheduled", t("scheduled"));
    setText("thPresent", t("present"));
    setText("thPercent", t("percent"));
    setText("thIssues", t("issues"));
    setText("thLast", t("last"));
    setText("optStatusAll", t("all"));
    setText("optStatusPresent", t("statusPresent"));
    setText("optStatusIssue", t("statusIssue"));
    setText("optStatusYukyu", t("yukyu"));
    setText("optStatusFalta", t("falta"));
    setText("optStatusLate", t("statusLate"));
    setText("optStatusEarly", t("statusEarly"));
    setText("optStatusPending", t("pending"));
}

function fillLookup(id, items) {
    const select = document.getElementById(id);
    select.innerHTML = [`<option value="0">${escapeHtml(t("all"))}</option>`]
        .concat((items || []).map(item => `<option value="${item.id}">${escapeHtml(item.name)}</option>`))
        .join("");
}

function requestReport() {
    setRowsLoading();
    post({
        action: "apply_filters",
        startDateIso: document.getElementById("startDate").value,
        endDateIso: document.getElementById("endDate").value,
        shiftId: Number(document.getElementById("shiftId").value || 0),
        sectorId: Number(document.getElementById("sectorId").value || 0),
        groupId: Number(document.getElementById("groupId").value || 0),
        status: document.getElementById("status").value,
        search: document.getElementById("search").value.trim()
    });
}

function renderReport(data) {
    state.rows = data.rows || [];
    setText("lblWindow", `${formatDate(data.startDateIso)} - ${formatDate(data.endDateIso)}`);
    setText("lblCount", state.rows.length);
    renderSummary(data.summary || {});
    renderRows(state.rows);
}

function renderSummary(summary) {
    document.getElementById("summary").innerHTML = [
        summaryCard(t("operators"), summary.operatorCount ?? 0),
        summaryCard(t("scheduledDays"), summary.scheduledDays ?? 0),
        summaryCard(t("presentDays"), summary.presentDays ?? 0),
        summaryCard(t("avgPresence"), formatPercent(summary.presencePercent), "accent"),
        summaryCard(t("yukyu"), summary.yukyuDays ?? 0),
        summaryCard(t("falta"), summary.faltaDays ?? 0, "danger"),
        summaryCard(t("late"), summary.lateDays ?? 0, "warn"),
        summaryCard(t("early"), summary.earlyLeaveDays ?? 0, "warn")
    ].join("");
}

function renderRows(rows) {
    const body = document.getElementById("rows");
    if (!rows.length) {
        body.innerHTML = `<tr><td colspan="7" class="empty-cell">${escapeHtml(t("empty"))}</td></tr>`;
        return;
    }

    body.innerHTML = rows.map(row => {
        const issueText = [
            row.yukyuDays ? `${t("yukyu")} ${row.yukyuDays}` : "",
            row.faltaDays ? `${t("falta")} ${row.faltaDays}` : "",
            row.lateDays ? `${t("late")} ${row.lateDays}` : "",
            row.earlyLeaveDays ? `${t("early")} ${row.earlyLeaveDays}` : "",
            row.pendingTodokeCount ? `${t("pending")} ${row.pendingTodokeCount}` : ""
        ].filter(Boolean).join(" | ");

        return `
            <tr>
                <td>
                    <div class="operator-cell">
                        <strong>${escapeHtml(localizedName(row.name, row.nameJp))}</strong>
                        <span>${escapeHtml(row.codigoFJ)} | ${escapeHtml(row.shiftName || "-")} | ${escapeHtml(row.sectorName || "-")}</span>
                    </div>
                </td>
                <td>${escapeHtml(row.groupName || "-")}</td>
                <td>${row.scheduledDays ?? 0}</td>
                <td>${row.presentDays ?? 0}</td>
                <td><span class="percent-pill">${formatPercent(row.presencePercent)}</span></td>
                <td>${escapeHtml(issueText || "-")}</td>
                <td>${escapeHtml(row.lastStatus || "-")}<small>${escapeHtml(row.lastDateIso ? ` | ${formatDate(row.lastDateIso)}` : "")}${escapeHtml(row.lastArea ? ` | ${row.lastArea}` : "")}</small></td>
            </tr>
        `;
    }).join("");
}

function setRowsLoading() {
    document.getElementById("rows").innerHTML = `<tr><td colspan="7" class="empty-cell">${escapeHtml(t("loading"))}</td></tr>`;
}

function summaryCard(label, value, tone = "") {
    return `
        <article class="summary-card ${tone ? `summary-card-${tone}` : ""}">
            <span>${escapeHtml(label)}</span>
            <strong>${escapeHtml(String(value))}</strong>
        </article>
    `;
}

function localizedName(name, nameJp) {
    return state.locale === "ja-JP" ? (nameJp || name || "-") : (name || nameJp || "-");
}

function formatPercent(value) {
    return `${Number(value || 0).toFixed(1)}%`;
}

function formatDate(value) {
    if (!value) {
        return "-";
    }
    const [year, month, day] = value.split("-").map(Number);
    if (year && month && day) {
        return state.locale === "ja-JP"
            ? new Intl.DateTimeFormat("ja-JP", { dateStyle: "medium" }).format(new Date(year, month - 1, day))
            : new Intl.DateTimeFormat("pt-BR", { dateStyle: "short" }).format(new Date(year, month - 1, day));
    }
    return value;
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
    return I18N[state.locale]?.[key] ?? I18N["pt-BR"][key] ?? key;
}

function showToast(title, message) {
    toast.title.textContent = title;
    toast.message.textContent = message;
    toast.root.classList.remove("hidden");
    clearTimeout(toast.timer);
    toast.timer = setTimeout(() => toast.root.classList.add("hidden"), 3200);
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;");
}

function debounce(callback, delay) {
    let timer = null;
    return (...args) => {
        clearTimeout(timer);
        timer = setTimeout(() => callback(...args), delay);
    };
}
