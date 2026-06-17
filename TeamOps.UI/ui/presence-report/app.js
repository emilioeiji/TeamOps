const state = {
    locale: "pt-BR",
    rows: [],
    activeView: "manager"
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
        percentOfficial: "% sem domingo",
        percentControl: "% com domingo",
        overtimeCurrent: "Zangyou Jikan",
        overtimeProjection: "Projecao HE",
        overtimeSunday: "Zangyou + Dom/Shukkin",
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
        avgPresenceOfficial: "Media oficial",
        avgPresenceControl: "Media controle",
        below82ByShift: "Abaixo de 82% por turno",
        daysWithoutSunday: "Dias sem domingo",
        daysWithSunday: "Dias com domingo",
        absencesWithoutSunday: "Faltas sem domingo",
        absencesWithSunday: "Faltas com domingo",
        overtimeOver45: "Acima de 45h",
        overtime35To45: "Risco 35-45h",
        overtimeBelow35: "Normal abaixo 35h",
        overtimeTotal: "HE mes atual",
        workedSundays: "Domingos trab.",
        holidayWork: "Qtd. Shukkin",
        holidayWorkHours: "Horas Shukkin",
        overtimeSundayHint: "Zangyou + horas de domingo/shukkin",
        topOvertime: "Top Zangyou projetado",
        topAbsences: "Top Ausentes",
        currentMonth: "Mes atual",
        previousMonth: "Mes anterior",
        projected: "Projetado",
        realized: "Realizado",
        remaining: "Futuro",
        overtimeLimit: "Limite 45h",
        performance: "Performance",
        pending: "Todoke pendente",
        managerSummary: "Resumo Gerencial",
        details: "Detalhamento",
        byGroup: "Resumo por grupo",
        bySector: "Resumo por area",
        byShift: "Resumo por turno",
        absenceRate: "Taxa ausencia",
        statusPresent: "Presenca",
        statusIssue: "Ocorrencias",
        statusLate: "Atraso",
        statusEarly: "Saida antecipada",
        notifyTitle: "Aviso"
    },
    "ja-JP": {
        title: "出勤レポート - TeamOps",
        badge: "出勤",
        heading: "出勤レポート",
        subtitle: "Haidai の配置、出勤、Todoke、移動記録を作業者別に集計します。",
        window: "期間",
        start: "開始",
        end: "終了",
        shift: "シフト",
        sector: "工程",
        group: "グループ",
        status: "ステータス",
        search: "検索",
        apply: "適用",
        all: "すべて",
        tableTitle: "作業者",
        tableSubtitle: "選択期間内の予定日数を基準に割合を計算します。",
        operator: "作業者",
        scheduled: "予定",
        present: "確認済み",
        percentOfficial: "% 日曜除外",
        percentControl: "% 日曜含む",
        overtimeCurrent: "Zangyou Jikan",
        overtimeProjection: "残業予測",
        overtimeSunday: "Zangyou + 日曜/Shukkin",
        issues: "発生項目",
        last: "最新ステータス",
        empty: "該当する記録はありません。",
        loading: "読み込み中...",
        operators: "作業者",
        scheduledDays: "予定日数",
        presentDays: "確認済み日数",
        yukyu: "Yukyu",
        falta: "欠勤",
        late: "遅刻",
        early: "早退",
        avgPresence: "平均出勤",
        avgPresenceOfficial: "公式平均",
        avgPresenceControl: "管理平均",
        below82ByShift: "シフト別 82% 未満",
        daysWithoutSunday: "日曜除外日数",
        daysWithSunday: "日曜含む日数",
        absencesWithoutSunday: "日曜除外の欠勤",
        absencesWithSunday: "日曜含む欠勤",
        overtimeOver45: "45h 超過",
        overtime35To45: "35-45h リスク",
        overtimeBelow35: "35h 未満正常",
        overtimeTotal: "当月残業",
        workedSundays: "日曜出勤",
        holidayWork: "Shukkin 日数",
        holidayWorkHours: "Shukkin 時間",
        overtimeSundayHint: "Zangyou + 日曜/Shukkin 時間",
        topOvertime: "Zangyou 予測上位",
        topAbsences: "欠勤・遅刻上位",
        currentMonth: "当月",
        previousMonth: "前月",
        projected: "予測",
        realized: "実績",
        remaining: "今後",
        overtimeLimit: "45h 上限",
        performance: "パフォーマンス",
        pending: "Todoke 未処理",
        managerSummary: "管理サマリー",
        details: "詳細",
        byGroup: "グループ別",
        bySector: "工程別",
        byShift: "シフト別",
        absenceRate: "欠勤率",
        statusPresent: "出勤",
        statusIssue: "発生項目",
        statusLate: "遅刻",
        statusEarly: "早退",
        notifyTitle: "通知"
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
    document.querySelectorAll("[data-view]").forEach(button => {
        button.addEventListener("click", () => switchView(button.dataset.view || "manager"));
    });
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
    setText("tabManager", t("managerSummary"));
    setText("tabDetails", t("details"));
    setText("thOperator", t("operator"));
    setText("thGroup", t("group"));
    setText("thScheduled", t("scheduled"));
    setText("thPresent", t("present"));
    setText("thPercentOfficial", t("percentOfficial"));
    setText("thPercentControl", t("percentControl"));
    setText("thOvertimeCurrent", t("overtimeCurrent"));
    setText("thOvertimeProjection", t("overtimeProjection"));
    setText("thOvertimeSunday", t("overtimeSunday"));
    document.getElementById("thOvertimeSunday")?.setAttribute("title", t("overtimeSundayHint"));
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
    switchView(state.activeView);
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
    renderBreakdowns(data || {});
    renderRows(state.rows);
}

function renderSummary(summary) {
    document.getElementById("summary").innerHTML = [
        summaryCard(t("operators"), summary.operatorCount ?? 0),
        summaryCard(t("daysWithoutSunday"), summary.scheduledDaysWithoutSunday ?? summary.scheduledDays ?? 0),
        summaryCard(t("daysWithSunday"), summary.scheduledDaysWithSunday ?? summary.scheduledDays ?? 0),
        summaryCard(t("presentDays"), summary.presentDays ?? 0),
        summaryCard(t("avgPresenceOfficial"), formatPercent(summary.presencePercentWithoutSunday ?? summary.presencePercent), "accent"),
        summaryCard(t("avgPresenceControl"), formatPercent(summary.presencePercentWithSunday ?? summary.presencePercent), "accent"),
        summaryCard(t("absenceRate"), formatPercent(calculateAbsenceRate(summary)), "danger"),
        summaryCard(t("overtimeOver45"), summary.overtimeOver45Count ?? 0, "danger"),
        summaryCard(t("overtime35To45"), summary.overtimeBetween35And45Count ?? 0, "warn"),
        summaryCard(t("overtimeBelow35"), summary.overtimeBelow35Count ?? 0, "accent"),
        summaryCard(t("overtimeTotal"), formatHours(summary.totalCurrentMonthOvertimeHours), "accent"),
        summaryCard(t("workedSundays"), summary.totalWorkedSundays ?? 0, "warn"),
        summaryCard(t("holidayWork"), summary.totalHolidayWorkDays ?? 0, "warn"),
        summaryCard(t("absencesWithoutSunday"), summary.absencesWithoutSunday ?? summary.faltaDays ?? 0, "danger"),
        summaryCard(t("absencesWithSunday"), summary.absencesWithSunday ?? summary.faltaDays ?? 0, "danger"),
        summaryCard(t("yukyu"), summary.yukyuDays ?? 0),
        summaryCard(t("late"), summary.lateDays ?? 0, "warn"),
        summaryCard(t("early"), summary.earlyLeaveDays ?? 0, "warn")
    ].join("");
}

function renderBreakdowns(data) {
    const container = document.getElementById("managerBreakdowns");
    const rows = data.rows || [];
    container.innerHTML = [
        breakdownCard(t("byGroup"), buildBreakdown(rows, "groupName")),
        breakdownCard(t("bySector"), buildBreakdown(rows, "sectorName")),
        breakdownCard(t("byShift"), buildBreakdown(rows, "shiftName")),
        below82Card(t("below82ByShift"), rows),
        rankingCard(t("topOvertime"), data.topOvertime || [], "overtime"),
        rankingCard(t("topAbsences"), data.topAbsences || [], "absence"),
        performanceCard(data.performance || {})
    ].join("");
}

function buildBreakdown(rows, field) {
    const map = new Map();
    rows.forEach(row => {
        const key = row[field] || "-";
        const current = map.get(key) || { label: key, scheduled: 0, present: 0, issues: 0 };
        current.scheduled += Number(row.scheduledDaysWithoutSunday ?? row.scheduledDays ?? 0);
        current.present += Number(row.presentDays || 0);
        current.issues += Number(row.yukyuDays || 0) + Number(row.faltaDays || 0)
            + Number(row.lateDays || 0) + Number(row.earlyLeaveDays || 0);
        map.set(key, current);
    });

    return Array.from(map.values())
        .sort((a, b) => b.scheduled - a.scheduled || a.label.localeCompare(b.label))
        .slice(0, 8);
}

function below82Card(title, rows) {
    const byShift = new Map();
    rows
        .filter(row => row.below82WithoutSunday || row.below82WithSunday)
        .forEach(row => {
            const shift = row.shiftName || "-";
            const items = byShift.get(shift) || [];
            items.push(row);
            byShift.set(shift, items);
        });

    const content = Array.from(byShift.entries())
        .sort((a, b) => a[0].localeCompare(b[0]))
        .map(([shift, items]) => `
            <div class="below-shift">
                <strong>${escapeHtml(shift)}</strong>
                ${items
                    .sort((a, b) => Number(a.presencePercentWithoutSunday || 0) - Number(b.presencePercentWithoutSunday || 0))
                    .map(item => `
                        <span>${escapeHtml(localizedName(item.name, item.nameJp))}: ${escapeHtml(formatPercent(item.presencePercentWithoutSunday))} / ${escapeHtml(formatPercent(item.presencePercentWithSunday))}</span>
                    `).join("")}
            </div>
        `).join("");

    return `
        <article class="breakdown-card breakdown-card-alert">
            <h2>${escapeHtml(title)}</h2>
            ${content || `<p class="breakdown-empty">${escapeHtml(t("empty"))}</p>`}
        </article>
    `;
}

function breakdownCard(title, items) {
    const rows = items.length
        ? items.map(item => {
            const percent = item.scheduled > 0 ? (item.present / item.scheduled) * 100 : 0;
            return `
                <div class="breakdown-row">
                    <span>${escapeHtml(item.label)}</span>
                    <strong>${escapeHtml(formatPercent(percent))}</strong>
                    <small>${item.present}/${item.scheduled} | ${escapeHtml(t("issues"))}: ${item.issues}</small>
                </div>
            `;
        }).join("")
        : `<p class="breakdown-empty">${escapeHtml(t("empty"))}</p>`;

    return `
        <article class="breakdown-card">
            <h2>${escapeHtml(title)}</h2>
            ${rows}
        </article>
    `;
}

function rankingCard(title, items, mode) {
    const heading = mode === "absence"
        ? `${escapeHtml(title)} <span class="absence-legend"><span title="Falta">F</span> | <span title="Atraso">A</span> | <span title="Saída Antecipada">S</span> | <span title="Yuukyu">Y</span></span>`
        : escapeHtml(title);
    const rows = items.length
        ? items.map(item => `
            <div class="breakdown-row">
                <span>${item.rank}. ${escapeHtml(localizedName(item.name, item.nameJp))}</span>
                <strong>${mode === "absence"
                    ? `${item.faltaDays ?? 0} | ${item.lateDays ?? 0} | ${item.earlyLeaveDays ?? 0} | ${item.yukyuDays ?? 0}`
                    : formatHours(item.overtimeHours)}</strong>
                <small>${mode === "absence"
                    ? `${escapeHtml(item.codigoFJ)} | ${escapeHtml(item.shiftName || "-")}`
                    : `${escapeHtml(item.codigoFJ)} | ${escapeHtml(item.shiftName || "-")} | ${escapeHtml(t("holidayWork"))}: ${item.holidayWorkDays}`}</small>
            </div>
        `).join("")
        : `<p class="breakdown-empty">${escapeHtml(t("empty"))}</p>`;

    return `
        <article class="breakdown-card breakdown-card-overtime">
            <h2>${heading}</h2>
            ${rows}
        </article>
    `;
}

function performanceCard(performance) {
    const items = [
        ["LoadPresenceMs", performance.loadPresenceMs ?? 0],
        ["LoadHaidaiMs", performance.loadHaidaiMs ?? 0],
        ["BuildOvertimeMs", performance.buildOvertimeMs ?? 0],
        ["BuildProjectionMs", performance.buildProjectionMs ?? 0],
        ["BuildRankingMs", performance.buildRankingMs ?? 0]
    ];

    return `
        <article class="breakdown-card breakdown-card-performance">
            <h2>${escapeHtml(t("performance"))}</h2>
            ${items.map(([label, value]) => `
                <div class="breakdown-row">
                    <span>${escapeHtml(label)}</span>
                    <strong>${escapeHtml(String(value))}ms</strong>
                </div>
            `).join("")}
        </article>
    `;
}

function calculateAbsenceRate(summary) {
    const scheduled = Number(summary.scheduledDays || 0);
    if (scheduled <= 0) {
        return 0;
    }

    const present = Number(summary.presentDays || 0);
    return Math.max(0, ((scheduled - present) / scheduled) * 100);
}

function switchView(view) {
    state.activeView = view === "details" ? "details" : "manager";
    document.getElementById("managerPanel")?.classList.toggle("hidden", state.activeView !== "manager");
    document.getElementById("detailsPanel")?.classList.toggle("hidden", state.activeView !== "details");
    document.getElementById("tabManager")?.classList.toggle("active", state.activeView === "manager");
    document.getElementById("tabDetails")?.classList.toggle("active", state.activeView === "details");
}

function renderRows(rows) {
    const body = document.getElementById("rows");
    if (!rows.length) {
        body.innerHTML = `<tr><td colspan="11" class="empty-cell">${escapeHtml(t("empty"))}</td></tr>`;
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
            <tr class="${row.below82WithoutSunday || row.below82WithSunday ? "attendance-alert-row" : ""}">
                <td>
                    <div class="operator-cell">
                        <button class="operator-link" type="button" data-open-operator="${escapeHtmlAttr(row.codigoFJ)}">
                            ${escapeHtml(localizedName(row.name, row.nameJp))}
                        </button>
                        <span>${escapeHtml(row.codigoFJ)} | ${escapeHtml(row.shiftName || "-")} | ${escapeHtml(row.sectorName || "-")}</span>
                    </div>
                </td>
                <td>${escapeHtml(row.groupName || "-")}</td>
                <td>${row.scheduledDays ?? 0}</td>
                <td>${row.presentDays ?? 0}</td>
                <td>
                    <span class="percent-pill ${row.below82WithoutSunday ? "percent-pill-alert" : ""}">${formatPercent(row.presencePercentWithoutSunday ?? row.presencePercent)}</span>
                    <small>${escapeHtml(t("absencesWithoutSunday"))}: ${row.absencesWithoutSunday ?? 0}</small>
                </td>
                <td>
                    <span class="percent-pill ${row.below82WithSunday ? "percent-pill-alert" : ""}">${formatPercent(row.presencePercentWithSunday ?? row.presencePercent)}</span>
                    <small>${escapeHtml(t("absencesWithSunday"))}: ${row.absencesWithSunday ?? 0}</small>
                </td>
                <td>
                    <span class="percent-pill overtime-risk-${escapeHtmlAttr(row.overtimeRiskLevel || "normal")}">${formatHours(row.currentMonthOvertimeHours)}</span>
                    <small>${escapeHtml(t("previousMonth"))}: ${formatHours(row.previousMonthOvertimeHours)}</small>
                </td>
                <td>
                    <span class="percent-pill overtime-risk-${escapeHtmlAttr(row.overtimeRiskLevel || "normal")}">${formatHours(row.projectedFinalOvertimeHours)}</span>
                    <small>${escapeHtml(t("realized"))}: ${formatHours(row.realizedOvertimeHours)} | ${escapeHtml(t("remaining"))}: ${formatHours(row.projectedRemainingOvertimeHours)}</small>
                    <small>${escapeHtml(t("overtimeLimit"))}: ${formatSignedHours(row.overtimeLimitDifferenceHours)}</small>
                </td>
                <td>
                    <span class="percent-pill overtime-total-risk-${escapeHtmlAttr(row.totalOvertimeRiskLevel || "normal")}" title="${escapeHtmlAttr(t("overtimeSundayHint"))}">${escapeHtml(row.overtimePlusSundaysLabel || formatHours(row.domingoShukkinTotalHours))}</span>
                    <small>${escapeHtml(t("holidayWork"))}: ${row.holidayWorkDays ?? 0} | ${escapeHtml(t("holidayWorkHours"))}: ${formatHours(row.holidayWorkHours)}</small>
                </td>
                <td>${escapeHtml(issueText || "-")}</td>
                <td>${escapeHtml(row.lastStatus || "-")}<small>${escapeHtml(row.lastDateIso ? ` | ${formatDate(row.lastDateIso)}` : "")}${escapeHtml(row.lastArea ? ` | ${row.lastArea}` : "")}</small></td>
            </tr>
        `;
    }).join("");

    body.querySelectorAll("[data-open-operator]").forEach(button => {
        button.addEventListener("click", () => {
            post({
                action: "open_operator_report",
                codigoFJ: button.dataset.openOperator || "",
                startDateIso: document.getElementById("startDate").value,
                endDateIso: document.getElementById("endDate").value
            });
        });
    });
}

function setRowsLoading() {
    document.getElementById("rows").innerHTML = `<tr><td colspan="11" class="empty-cell">${escapeHtml(t("loading"))}</td></tr>`;
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

function formatHours(value) {
    return `${Number(value || 0).toFixed(1)}h`;
}

function formatSignedHours(value) {
    const numeric = Number(value || 0);
    return `${numeric >= 0 ? "+" : ""}${numeric.toFixed(1)}h`;
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

function escapeHtmlAttr(value) {
    return escapeHtml(value);
}

function debounce(callback, delay) {
    let timer = null;
    return (...args) => {
        clearTimeout(timer);
        timer = setTimeout(() => callback(...args), delay);
    };
}
