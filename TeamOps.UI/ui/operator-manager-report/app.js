const state = {
    locale: "pt-BR",
    filters: {
        shiftId: 0,
        sectorId: 0,
        groupId: 0,
        periodDays: 90,
        search: ""
    },
    directory: [],
    selectedCodigoFJ: "",
    report: null,
    window: {
        startDateIso: "",
        endDateIso: ""
    }
};

const toast = {
    root: document.getElementById("toast"),
    title: document.getElementById("toastTitle"),
    message: document.getElementById("toastMessage"),
    timer: null
};

const I18N = {
    "pt-BR": {
        title: "Operadores - TeamOps",
        badge: "Visao gerencial",
        heading: "Operadores",
        subtitle: "Consolidado de presenca, producao, follow-up, MasterCard e ocorrencias do Haidai.",
        windowLabel: "Janela analisada",
        period: "Periodo",
        shift: "Turno",
        sector: "Setor",
        group: "Grupo",
        search: "Buscar operador",
        searchPlaceholder: "FJ, nome romanji ou nihongo",
        refresh: "Atualizar",
        all: "Todos",
        directoryTitle: "Quadro de operadores",
        directorySubtitle: "Clique em um operador para abrir a visao gerencial detalhada.",
        directoryCountLabel: "operadores",
        emptyTitle: "Selecione um operador",
        emptySubtitle: "A lista lateral mostra o desempenho consolidado no periodo filtrado.",
        noOperators: "Nenhum operador encontrado para os filtros atuais.",
        noHistory: "Nenhum historico encontrado no periodo.",
        noFollowUps: "Nenhum follow-up registrado no periodo.",
        btnFollowHistory: "Abrir historico detalhado",
        masterCards: "MasterCards",
        presence: "Presenca",
        production: "Producao",
        dailyHistory: "Historico diario",
        recentFollowUps: "Follow-ups recentes",
        noMasterCards: "Nenhum MasterCard registrado no periodo.",
        started: "Admissao",
        roleTrainer: "Treinador",
        roleLeader: "Lider",
        metricPresence: "% presenca",
        metricCoverage: "% cobertura escala",
        metricScheduled: "Dias escalados",
        metricPresent: "Dias conformes",
        metricYukyu: "Yukyu",
        metricFalta: "Faltas",
        metricLate: "Atrasos",
        metricEarly: "Saida antecipada",
        metricPending: "Todoke pendente",
        metricFollow: "Follow-ups",
        metricMasterTotal: "MasterCards",
        metricMasterFollow: "MC em follow",
        metricMasterOverdue: "Follow vencido",
        metricMasterSoon: "Follow 7 dias",
        productionMinutes: "Minutos estimados rodando",
        productionKadouritsu: "Kadouritsu estimado",
        productionAreas: "Areas recentes",
        masterEquipment: "Equipamento",
        masterSector: "Setor",
        masterStart: "Inicio",
        masterConcluded: "Concluido",
        masterFollowDate: "Follow",
        masterFinalized: "Finalizado",
        masterNotes: "Observacoes",
        masterStatusInProgress: "Andamento",
        masterStatusFollow: "Follow",
        masterStatusCompleted: "Finalizado",
        masterStateOverdue: "Vencido",
        masterStateDueSoon: "Proximo",
        masterStateScheduled: "Agendado",
        masterStateInProgress: "Em andamento",
        masterStateCompleted: "Fechado",
        historyDate: "Data",
        historyStatus: "Status",
        historyArea: "Area",
        historyNotes: "Observacoes",
        historyPending: "Todoke",
        pending: "Pendente",
        validated: "Ok",
        na: "-",
        notifyTitle: "Aviso"
    },
    "ja-JP": {
        title: "Operators - TeamOps",
        badge: "Management view",
        heading: "Operators",
        subtitle: "Attendance, production, follow-up, MasterCard, and Haidai events in one view.",
        windowLabel: "Analysis window",
        period: "Period",
        shift: "Shift",
        sector: "Sector",
        group: "Group",
        search: "Search operator",
        searchPlaceholder: "FJ, romanji or nihongo",
        refresh: "Refresh",
        all: "All",
        directoryTitle: "Operator board",
        directorySubtitle: "Select an operator to open the detailed management view.",
        directoryCountLabel: "operators",
        emptyTitle: "Select an operator",
        emptySubtitle: "The left list shows consolidated results for the filtered period.",
        noOperators: "No operators found for the current filters.",
        noHistory: "No history found in this period.",
        noFollowUps: "No follow-up records in this period.",
        btnFollowHistory: "Open detailed history",
        masterCards: "MasterCards",
        presence: "Attendance",
        production: "Production",
        dailyHistory: "Daily history",
        recentFollowUps: "Recent follow-ups",
        noMasterCards: "No MasterCards found in this period.",
        started: "Start date",
        roleTrainer: "Trainer",
        roleLeader: "Leader",
        metricPresence: "% attendance",
        metricCoverage: "% lineup coverage",
        metricScheduled: "Scheduled days",
        metricPresent: "Compliant days",
        metricYukyu: "Yukyu",
        metricFalta: "Absences",
        metricLate: "Late",
        metricEarly: "Early leave",
        metricPending: "Pending todoke",
        metricFollow: "Follow-ups",
        metricMasterTotal: "MasterCards",
        metricMasterFollow: "MC follow",
        metricMasterOverdue: "Follow overdue",
        metricMasterSoon: "Follow 7 days",
        productionMinutes: "Estimated running minutes",
        productionKadouritsu: "Estimated kadouritsu",
        productionAreas: "Recent areas",
        masterEquipment: "Equipment",
        masterSector: "Sector",
        masterStart: "Start",
        masterConcluded: "Concluded",
        masterFollowDate: "Follow",
        masterFinalized: "Completed",
        masterNotes: "Notes",
        masterStatusInProgress: "In progress",
        masterStatusFollow: "Follow",
        masterStatusCompleted: "Completed",
        masterStateOverdue: "Overdue",
        masterStateDueSoon: "Due soon",
        masterStateScheduled: "Scheduled",
        masterStateInProgress: "In progress",
        masterStateCompleted: "Closed",
        historyDate: "Date",
        historyStatus: "Status",
        historyArea: "Area",
        historyNotes: "Notes",
        historyPending: "Todoke",
        pending: "Pending",
        validated: "Ok",
        na: "-",
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
        hydrateInit(payload.data || {});
        return;
    }

    if (payload.type === "directory") {
        renderDirectory(payload.data || {});
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
    document.getElementById("btnRefresh").addEventListener("click", requestDirectory);
    document.getElementById("periodDays").addEventListener("change", syncFiltersAndRefresh);
    document.getElementById("shiftId").addEventListener("change", syncFiltersAndRefresh);
    document.getElementById("sectorId").addEventListener("change", syncFiltersAndRefresh);
    document.getElementById("groupId").addEventListener("change", syncFiltersAndRefresh);
    document.getElementById("searchInput").addEventListener("input", debounce(syncFiltersAndRefresh, 220));
}

function hydrateInit(data) {
    state.locale = data.locale === "ja-JP" ? "ja-JP" : "pt-BR";
    applyLocale();

    fillLookup("shiftId", data.shifts || []);
    fillLookup("sectorId", data.sectors || []);
    fillLookup("groupId", data.groups || []);

    state.filters.shiftId = Number(data.defaults?.shiftId || 0);
    state.filters.sectorId = Number(data.defaults?.sectorId || 0);
    state.filters.groupId = Number(data.defaults?.groupId || 0);
    state.filters.periodDays = Number(data.defaults?.periodDays || 90);

    document.getElementById("shiftId").value = String(state.filters.shiftId);
    document.getElementById("sectorId").value = String(state.filters.sectorId);
    document.getElementById("groupId").value = String(state.filters.groupId);
    document.getElementById("periodDays").value = String(state.filters.periodDays);

    requestDirectory();
}

function applyLocale() {
    document.documentElement.lang = state.locale;
    document.title = t("title");
    setText("txtBadge", t("badge"));
    setText("txtTitle", t("heading"));
    setText("txtSubtitle", t("subtitle"));
    setText("txtWindowLabel", t("windowLabel"));
    setText("txtPeriod", t("period"));
    setText("txtShift", t("shift"));
    setText("txtSector", t("sector"));
    setText("txtGroup", t("group"));
    setText("txtSearch", t("search"));
    setText("btnRefresh", t("refresh"));
    setText("txtDirectoryTitle", t("directoryTitle"));
    setText("txtDirectorySubtitle", t("directorySubtitle"));
    setText("txtDirectoryCountLabel", t("directoryCountLabel"));
    setText("txtEmptyTitle", t("emptyTitle"));
    setText("txtEmptySubtitle", t("emptySubtitle"));
    document.getElementById("searchInput").placeholder = t("searchPlaceholder");
}

function fillLookup(id, items) {
    const select = document.getElementById(id);
    select.innerHTML = [`<option value="0">${escapeHtml(t("all"))}</option>`]
        .concat((items || []).map(item => `<option value="${item.id}">${escapeHtml(item.name)}</option>`))
        .join("");
}

function syncFiltersAndRefresh() {
    state.filters.periodDays = Number(document.getElementById("periodDays").value || 90);
    state.filters.shiftId = Number(document.getElementById("shiftId").value || 0);
    state.filters.sectorId = Number(document.getElementById("sectorId").value || 0);
    state.filters.groupId = Number(document.getElementById("groupId").value || 0);
    state.filters.search = document.getElementById("searchInput").value.trim();
    requestDirectory();
}

function requestDirectory() {
    syncFilters();
    post({
        action: "apply_filters",
        shiftId: state.filters.shiftId,
        sectorId: state.filters.sectorId,
        groupId: state.filters.groupId,
        periodDays: state.filters.periodDays,
        search: state.filters.search
    });
}

function syncFilters() {
    state.filters.periodDays = Number(document.getElementById("periodDays").value || 90);
    state.filters.shiftId = Number(document.getElementById("shiftId").value || 0);
    state.filters.sectorId = Number(document.getElementById("sectorId").value || 0);
    state.filters.groupId = Number(document.getElementById("groupId").value || 0);
    state.filters.search = document.getElementById("searchInput").value.trim();
}

function renderDirectory(data) {
    state.directory = data.items || [];
    state.window.startDateIso = data.startDateIso || "";
    state.window.endDateIso = data.endDateIso || "";

    setText("lblWindow", `${formatDateOnly(state.window.startDateIso)} - ${formatDateOnly(state.window.endDateIso)}`);
    setText("lblDirectoryCount", state.directory.length);

    const host = document.getElementById("directoryList");
    if (!state.directory.length) {
        host.innerHTML = `<div class="directory-empty">${escapeHtml(t("noOperators"))}</div>`;
        state.selectedCodigoFJ = "";
        state.report = null;
        renderEmptyReport();
        return;
    }

    if (!state.directory.some(item => item.codigoFJ === state.selectedCodigoFJ)) {
        state.selectedCodigoFJ = state.directory[0].codigoFJ;
    }

    host.innerHTML = state.directory.map(item => {
        const selectedClass = item.codigoFJ === state.selectedCodigoFJ ? " directory-item-selected" : "";
        return `
            <button class="directory-item${selectedClass}" type="button" data-codigo="${escapeHtml(item.codigoFJ)}">
                <div class="directory-main">
                    <strong>${escapeHtml(localizedName(item.name, item.nameJp))}</strong>
                    <span>${escapeHtml(item.codigoFJ)} · ${escapeHtml(item.groupName || t("na"))}</span>
                </div>
                <div class="directory-metrics">
                    <span class="mini-pill mini-pill-blue">${formatPercent(item.presencePercent)}</span>
                    <span class="mini-pill mini-pill-neutral">${item.pendingTodokeCount} todoke</span>
                </div>
            </button>
        `;
    }).join("");

    host.querySelectorAll("[data-codigo]").forEach(button => {
        button.addEventListener("click", () => {
            state.selectedCodigoFJ = button.dataset.codigo || "";
            renderDirectory(data);
        });
    });

    requestReport();
}

function requestReport() {
    if (!state.selectedCodigoFJ) {
        renderEmptyReport();
        return;
    }

    post({
        action: "select_operator",
        codigoFJ: state.selectedCodigoFJ,
        periodDays: state.filters.periodDays
    });
}

function renderReport(data) {
    state.report = data;
    state.selectedCodigoFJ = data.codigoFJ || "";

    const presence = data.presence || {};
    const masterCards = data.masterCards || {};
    const production = data.production || {};
    const followUps = data.followUps || [];
    const dailyHistory = data.dailyHistory || [];
    const host = document.getElementById("reportHost");

    host.innerHTML = `
        <section class="operator-hero">
            <div>
                <div class="operator-line">
                    <h2>${escapeHtml(localizedName(data.name, data.nameJp))}</h2>
                    ${data.trainer ? `<span class="role-pill role-pill-blue">${escapeHtml(t("roleTrainer"))}</span>` : ""}
                    ${data.isLeader ? `<span class="role-pill role-pill-amber">${escapeHtml(t("roleLeader"))}</span>` : ""}
                </div>
                <p>${escapeHtml(data.codigoFJ || t("na"))} · ${escapeHtml(data.shiftName || t("na"))} · ${escapeHtml(data.sectorName || t("na"))} · ${escapeHtml(data.groupName || t("na"))}</p>
                <small>${escapeHtml(t("started"))}: ${escapeHtml(formatDateOnly(data.startDateIso))}</small>
            </div>

            <button id="btnFollowHistory" class="ghost-button" type="button">${escapeHtml(t("btnFollowHistory"))}</button>
        </section>

        <section class="metric-grid">
            ${metricCard(t("metricPresence"), formatPercent(presence.presencePercent), "accent")}
            ${metricCard(t("metricCoverage"), formatPercent(presence.coveragePercent), "accent-soft")}
            ${metricCard(t("metricScheduled"), presence.scheduledDays ?? 0)}
            ${metricCard(t("metricPresent"), presence.presentDays ?? 0)}
            ${metricCard(t("metricYukyu"), presence.yukyuDays ?? 0)}
            ${metricCard(t("metricFalta"), presence.faltaDays ?? 0)}
            ${metricCard(t("metricLate"), presence.lateDays ?? 0)}
            ${metricCard(t("metricEarly"), presence.earlyLeaveDays ?? 0)}
            ${metricCard(t("metricPending"), presence.pendingTodokeCount ?? 0, "warn")}
            ${metricCard(t("metricFollow"), presence.followUpCount ?? 0)}
            ${renderMasterCardMetricCards(masterCards)}
        </section>

        <section class="content-grid">
            <article class="panel-card">
                <div class="panel-head">
                    <h3>${escapeHtml(t("production"))}</h3>
                </div>
                <div class="panel-stack">
                    <div class="summary-row">
                        <span>${escapeHtml(t("productionMinutes"))}</span>
                        <strong>${formatNumber(production.estimatedRunningMinutes)} min</strong>
                    </div>
                    <div class="summary-row">
                        <span>${escapeHtml(t("productionKadouritsu"))}</span>
                        <strong>${formatPercent(production.estimatedKadouritsuPercent)}</strong>
                    </div>
                    <div class="summary-row summary-row-top">
                        <span>${escapeHtml(t("productionAreas"))}</span>
                        <strong>${escapeHtml((production.localNames || []).join(", ") || t("na"))}</strong>
                    </div>
                    <div class="table-wrap">
                        <table>
                            <thead>
                                <tr>
                                    <th>${escapeHtml(t("historyDate"))}</th>
                                    <th>${escapeHtml(t("productionMinutes"))}</th>
                                    <th>${escapeHtml(t("productionKadouritsu"))}</th>
                                    <th>${escapeHtml(t("productionAreas"))}</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${renderProductionRows(production.days || [])}
                            </tbody>
                        </table>
                    </div>
                </div>
            </article>

            <article class="panel-card">
                <div class="panel-head">
                    <h3>${escapeHtml(t("masterCards"))}</h3>
                </div>
                <div class="follow-list">
                    ${renderMasterCards(masterCards.items || [])}
                </div>
            </article>
        </section>

        <article class="panel-card panel-card-full">
            <div class="panel-head">
                <h3>${escapeHtml(t("recentFollowUps"))}</h3>
            </div>
            <div class="follow-list">
                ${renderFollowUps(followUps)}
            </div>
        </article>

        <article class="panel-card panel-card-full">
            <div class="panel-head">
                <h3>${escapeHtml(t("dailyHistory"))}</h3>
            </div>
            <div class="table-wrap">
                <table>
                    <thead>
                        <tr>
                            <th>${escapeHtml(t("historyDate"))}</th>
                            <th>${escapeHtml(t("historyStatus"))}</th>
                            <th>${escapeHtml(t("historyArea"))}</th>
                            <th>${escapeHtml(t("historyNotes"))}</th>
                            <th>${escapeHtml(t("historyPending"))}</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${renderDailyHistory(dailyHistory)}
                    </tbody>
                </table>
            </div>
        </article>
    `;

    document.getElementById("btnFollowHistory").addEventListener("click", () => {
        post({
            action: "open_follow_history",
            codigoFJ: state.selectedCodigoFJ
        });
    });
}

function renderProductionRows(days) {
    if (!days.length) {
        return `<tr><td colspan="4" class="table-empty">${escapeHtml(t("noHistory"))}</td></tr>`;
    }

    return days.map(day => `
        <tr>
            <td>${escapeHtml(formatDateOnly(day.dateIso))}</td>
            <td>${formatNumber(day.estimatedRunningMinutes)} min</td>
            <td>${formatPercent(day.estimatedKadouritsuPercent)}</td>
            <td>${escapeHtml((day.localNames || []).join(", ") || t("na"))}</td>
        </tr>
    `).join("");
}

function renderFollowUps(items) {
    if (!items.length) {
        return `<div class="panel-empty">${escapeHtml(t("noFollowUps"))}</div>`;
    }

    return items.map(item => `
        <article class="follow-card">
            <div class="follow-head">
                <strong>${escapeHtml(item.dateLabel || t("na"))}</strong>
                <span>${escapeHtml(item.shiftName || t("na"))}</span>
            </div>
            <div class="follow-badges">
                <span>${escapeHtml(item.reasonName || t("na"))}</span>
                <span>${escapeHtml(item.typeName || t("na"))}</span>
                <span>${escapeHtml(item.localName || t("na"))}</span>
                <span>${escapeHtml(item.equipmentName || t("na"))}</span>
            </div>
            <p>${escapeHtml(item.description || t("na"))}</p>
            <small>${escapeHtml(item.guidance || t("na"))}</small>
        </article>
    `).join("");
}

function renderMasterCards(items) {
    if (!items.length) {
        return `<div class="panel-empty">${escapeHtml(t("noMasterCards"))}</div>`;
    }

    return items.map(item => `
        <article class="follow-card master-card">
            <div class="follow-head">
                <strong>${escapeHtml(item.equipmentName || t("na"))}</strong>
                <span class="master-state master-state-${escapeHtmlAttr(item.followState || "none")}">${escapeHtml(masterStateLabel(item.followState, item.status))}</span>
            </div>
            <div class="follow-badges">
                <span>${escapeHtml(statusLabel(item.status))}</span>
                <span>${escapeHtml(item.sectorName || t("na"))}</span>
            </div>
            <p>${escapeHtml(t("masterStart"))}: ${escapeHtml(formatDateOnly(item.startDateIso))}</p>
            <p>${escapeHtml(t("masterFollowDate"))}: ${escapeHtml(formatDateOnly(item.followDateIso))}</p>
            <small>${escapeHtml(t("masterNotes"))}: ${escapeHtml(item.notes || t("na"))}</small>
        </article>
    `).join("");
}

function renderDailyHistory(items) {
    if (!items.length) {
        return `<tr><td colspan="5" class="table-empty">${escapeHtml(t("noHistory"))}</td></tr>`;
    }

    return items.map(item => `
        <tr>
            <td>${escapeHtml(formatDateOnly(item.dateIso))}</td>
            <td><span class="status-pill">${escapeHtml(item.status || t("na"))}</span></td>
            <td>${escapeHtml(item.area || t("na"))}</td>
            <td>${escapeHtml(item.notes || t("na"))}</td>
            <td>${item.hasPendingTodoke ? `<span class="pending-pill">${escapeHtml(t("pending"))}</span>` : escapeHtml(t("validated"))}</td>
        </tr>
    `).join("");
}

function renderEmptyReport() {
    document.getElementById("reportHost").innerHTML = `
        <div class="empty-state">
            <h3>${escapeHtml(t("emptyTitle"))}</h3>
            <p>${escapeHtml(t("emptySubtitle"))}</p>
        </div>
    `;
}

function metricCard(label, value, tone = "") {
    return `
        <div class="metric-card ${tone ? `metric-card-${tone}` : ""}">
            <span>${escapeHtml(label)}</span>
            <strong>${escapeHtml(String(value))}</strong>
        </div>
    `;
}

function renderMasterCardMetricCards(masterCards) {
    const cards = [
        metricCard(t("metricMasterTotal"), masterCards.totalCount ?? 0)
    ];

    if ((masterCards.followCount ?? 0) > 0) {
        cards.push(metricCard(t("metricMasterFollow"), masterCards.followCount, "accent-soft"));
    }

    if ((masterCards.overdueFollowCount ?? 0) > 0) {
        cards.push(metricCard(t("metricMasterOverdue"), masterCards.overdueFollowCount, "danger"));
    }

    if ((masterCards.dueSoonFollowCount ?? 0) > 0) {
        cards.push(metricCard(t("metricMasterSoon"), masterCards.dueSoonFollowCount, "warn"));
    }

    return cards.join("");
}

function statusLabel(status) {
    switch (status) {
        case "in_progress":
            return t("masterStatusInProgress");
        case "follow":
            return t("masterStatusFollow");
        case "completed":
            return t("masterStatusCompleted");
        default:
            return status || t("na");
    }
}

function masterStateLabel(followState, status) {
    switch (followState) {
        case "overdue":
            return t("masterStateOverdue");
        case "due_soon":
            return t("masterStateDueSoon");
        case "scheduled":
            return t("masterStateScheduled");
        case "in_progress":
            return t("masterStateInProgress");
        case "completed":
            return t("masterStateCompleted");
        default:
            return statusLabel(status);
    }
}

function localizedName(name, nameJp) {
    return state.locale === "ja-JP"
        ? (nameJp || name || t("na"))
        : (name || nameJp || t("na"));
}

function formatPercent(value) {
    const number = Number(value || 0);
    return `${number.toFixed(1)}%`;
}

function formatNumber(value) {
    const number = Number(value || 0);
    return number.toFixed(1);
}

function formatDateOnly(value) {
    if (!value) {
        return t("na");
    }

    if (/^\d{4}-\d{2}-\d{2}$/.test(value)) {
        const [year, month, day] = value.split("-").map(Number);
        const safeDate = new Date(year, (month || 1) - 1, day || 1);
        return state.locale === "ja-JP"
            ? new Intl.DateTimeFormat("ja-JP", { dateStyle: "medium" }).format(safeDate)
            : new Intl.DateTimeFormat("pt-BR", { dateStyle: "short" }).format(safeDate);
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
        return value;
    }

    return state.locale === "ja-JP"
        ? new Intl.DateTimeFormat("ja-JP", { dateStyle: "medium" }).format(date)
        : new Intl.DateTimeFormat("pt-BR", { dateStyle: "short" }).format(date);
}

function setText(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = value ?? "";
    }
}

function post(payload) {
    window.chrome?.webview?.postMessage(payload);
}

function t(key) {
    return I18N[state.locale]?.[key] ?? I18N["pt-BR"][key] ?? key;
}

function showToast(title, message) {
    toast.title.textContent = title;
    toast.message.textContent = message;
    toast.root.classList.remove("hidden");

    if (toast.timer) {
        clearTimeout(toast.timer);
    }

    toast.timer = setTimeout(() => {
        toast.root.classList.add("hidden");
    }, 3200);
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
