const I18N = {
    "pt-BR": {
        title: "Producao / Machine Monitor",
        subtitle: "Monitor de maquinas, producao por turno e linha do tempo operacional.",
        metaUser: "Usuario",
        metaOperator: "Operador",
        metaPeriod: "Periodo",
        badge: "Monitor industrial",
        toolbarTitle: "Filtros do turno",
        toolbarSubtitle: "Importe os arquivos da rede, filtre setor/area/maquina e acompanhe a saude da producao no mesmo painel.",
        date: "Data",
        shift: "Turno",
        sector: "Setor",
        local: "Area",
        machine: "Maquina",
        allSectors: "Todos os setores",
        allLocals: "Todas as areas",
        allMachines: "Todas as maquinas",
        refresh: "Atualizar",
        import: "Importar",
        metricProduction: "% Producao",
        metricRunning: "Rodando",
        metricStopped: "Paradas",
        metricError: "Min. erro",
        metricInactive: "Min. inativo",
        tableTitle: "Resumo por maquina",
        tableSubtitle: "Clique na linha para ver os eventos recentes da maquina selecionada.",
        colMachine: "Maquina",
        colArea: "Area",
        colStatus: "Status",
        colRecipe: "Receita",
        colLot: "Lote",
        colOperators: "Operadores",
        colPercent: "%",
        colUpdated: "Atualizado",
        rankingTitle: "Ranking de paradas",
        rankingSubtitle: "Impacto acumulado de parada e erro no periodo selecionado.",
        detailTitle: "Historico recente da maquina",
        detailSubtitle: "Ultimos eventos importados para a maquina selecionada.",
        detailDate: "Data/Hora",
        detailStatus: "Status",
        detailRecipe: "Receita",
        detailLot: "Lote",
        detailSource: "Arquivo",
        timelineTitle: "Timeline de 5 em 5 minutos",
        timelineSubtitle: "Leitura visual do status das maquinas ao longo do turno.",
        loading: "Importando producao...",
        noRows: "Nenhuma maquina encontrada para o filtro selecionado.",
        noRanking: "Sem paradas registradas no periodo.",
        noTimeline: "Sem dados para montar a timeline.",
        selectMachine: "Selecione uma maquina.",
        noOperators: "Sem operador",
        importingSuccess: "Importacao concluida.",
        running: "Rodando",
        stopped: "Parado",
        inactive: "Inativo",
        error: "Erro"
    },
    "ja-JP": {
        title: "生産 / Machine Monitor",
        subtitle: "設備状態、シフト別生産、時間軸の流れを一つの画面で確認できます。",
        metaUser: "ユーザー",
        metaOperator: "作業者",
        metaPeriod: "期間",
        badge: "生産モニター",
        toolbarTitle: "シフトフィルター",
        toolbarSubtitle: "ネットワークのファイルを取り込み、セクター・エリア・設備別に生産状況を確認します。",
        date: "日付",
        shift: "シフト",
        sector: "セクター",
        local: "エリア",
        machine: "設備",
        allSectors: "全セクター",
        allLocals: "全エリア",
        allMachines: "全設備",
        refresh: "更新",
        import: "取込",
        metricProduction: "生産率",
        metricRunning: "稼動中",
        metricStopped: "停止台数",
        metricError: "異常分",
        metricInactive: "非稼動分",
        tableTitle: "設備別サマリー",
        tableSubtitle: "行をクリックすると、その設備の最新イベント履歴を確認できます。",
        colMachine: "設備",
        colArea: "エリア",
        colStatus: "状態",
        colRecipe: "レシピ",
        colLot: "ロット",
        colOperators: "作業者",
        colPercent: "%",
        colUpdated: "更新",
        rankingTitle: "停止ランキング",
        rankingSubtitle: "選択期間で停止・異常の影響が大きい設備です。",
        detailTitle: "設備の最近履歴",
        detailSubtitle: "選択した設備で最後に取り込まれたイベント一覧です。",
        detailDate: "日時",
        detailStatus: "状態",
        detailRecipe: "レシピ",
        detailLot: "ロット",
        detailSource: "ファイル",
        timelineTitle: "5分単位タイムライン",
        timelineSubtitle: "シフト中の設備状態を時系列で確認できます。",
        loading: "生産データを取り込み中...",
        noRows: "選択した条件に該当する設備がありません。",
        noRanking: "この期間に停止実績がありません。",
        noTimeline: "タイムラインを表示できるデータがありません。",
        selectMachine: "設備を選択してください。",
        noOperators: "作業者なし",
        importingSuccess: "取込が完了しました。",
        running: "稼動中",
        stopped: "停止",
        inactive: "非稼動",
        error: "異常"
    }
};

const state = {
    locale: "pt-BR",
    shifts: [],
    sectors: [],
    locals: [],
    machines: [],
    dashboard: null,
    selectedMachineCode: "",
    pinnedNotice: false
};

document.addEventListener("DOMContentLoaded", () => {
    bindEvents();
    send("production_init");
});

window.chrome?.webview?.addEventListener("message", event => {
    const msg = event.data;

    switch (msg.type) {
        case "init":
            hydrateInit(msg.data || {});
            break;
        case "dashboard":
            hideLoading();
            hydrateDashboard(msg.data || {});
            break;
        case "machine_detail":
            renderDetail(msg.data || {});
            break;
        case "import_result":
            hideLoading();
            state.pinnedNotice = true;
            showNotice(msg.data?.message || t("importingSuccess"), "success");
            break;
        case "error":
            hideLoading();
            state.pinnedNotice = true;
            showNotice(msg.message || "Erro.", "error");
            break;
    }
});

function bindEvents() {
    document.getElementById("btnRefresh").addEventListener("click", refreshDashboard);
    document.getElementById("btnImport").addEventListener("click", importProduction);
    document.getElementById("sectorPicker").addEventListener("change", onSectorChanged);
    document.getElementById("localPicker").addEventListener("change", onLocalChanged);
}

function send(action, extra = {}) {
    window.chrome?.webview?.postMessage({
        action,
        ...extra
    });
}

function hydrateInit(data) {
    state.locale = data.locale === "ja-JP" ? "ja-JP" : "pt-BR";
    state.shifts = data.shifts || [];
    state.sectors = data.sectors || [];
    state.locals = data.locals || [];
    state.machines = data.machines || [];

    document.getElementById("datePicker").value = data.defaults?.dateIso || "";
    fillShiftOptions(data.defaults?.shiftId || "");
    fillSectorOptions();
    fillLocalOptions();
    fillMachineOptions();

    setText("lblUser", data.currentUser || "-");
    setText("lblOperator", resolveOperatorName(data));
    applyLocale();
}

function hydrateDashboard(data) {
    state.dashboard = data;

    setText("lblPeriod", `${formatDateTime(data.periodStart)} - ${formatDateTime(data.periodEnd)}`);
    setText("lblProductionPercent", `${Number(data.productionPercent || 0).toFixed(1)}%`);
    setText("lblMachinesRunning", data.machinesRunning ?? 0);
    setText("lblMachinesStopped", data.machinesStopped ?? 0);
    setText("lblErrorMinutes", Number(data.errorMinutes || 0).toFixed(1));
    setText("lblInactiveMinutes", Number(data.inactiveMinutes || 0).toFixed(1));

    renderMachineTable(data.machines || []);
    renderRanking(data.ranking || []);
    renderTimeline(data.timeline || []);

    if (!state.pinnedNotice) {
        hideNotice();
    }

    if (state.selectedMachineCode) {
        send("production_machine_detail", { machineCode: state.selectedMachineCode });
    }
}

function refreshDashboard() {
    state.pinnedNotice = false;
    send("production_load_dashboard", buildFilterPayload());
}

function importProduction() {
    state.pinnedNotice = false;
    showLoading();
    send("production_import", buildFilterPayload());
}

function buildFilterPayload() {
    return {
        date: document.getElementById("datePicker").value,
        shiftId: Number(document.getElementById("shiftPicker").value || 0),
        sectorId: Number(document.getElementById("sectorPicker").value || 0),
        localId: Number(document.getElementById("localPicker").value || 0),
        machineCode: document.getElementById("machinePicker").value || ""
    };
}

function onSectorChanged() {
    fillLocalOptions();
    fillMachineOptions();
}

function onLocalChanged() {
    fillMachineOptions();
}

function fillShiftOptions(selectedValue) {
    const picker = document.getElementById("shiftPicker");
    picker.innerHTML = state.shifts
        .map(shift => `<option value="${shift.id}">${escapeHtml(getShiftName(shift))}</option>`)
        .join("");

    if (selectedValue) {
        picker.value = String(selectedValue);
    }
}

function fillSectorOptions() {
    const picker = document.getElementById("sectorPicker");
    const options = [`<option value="0">${escapeHtml(t("allSectors"))}</option>`]
        .concat(state.sectors.map(sector => `<option value="${sector.id}">${escapeHtml(getLocalizedName(sector))}</option>`));

    picker.innerHTML = options.join("");
}

function fillLocalOptions() {
    const sectorId = Number(document.getElementById("sectorPicker").value || 0);
    const picker = document.getElementById("localPicker");
    const currentValue = picker.value || "0";

    const locals = state.locals.filter(local => sectorId <= 0 || Number(local.sectorId) === sectorId);
    const options = [`<option value="0">${escapeHtml(t("allLocals"))}</option>`]
        .concat(locals.map(local => `<option value="${local.id}">${escapeHtml(getLocalizedName(local))}</option>`));

    picker.innerHTML = options.join("");
    picker.value = currentValue;
    if (picker.value !== currentValue) {
        picker.value = "0";
    }
}

function fillMachineOptions() {
    const sectorId = Number(document.getElementById("sectorPicker").value || 0);
    const localId = Number(document.getElementById("localPicker").value || 0);
    const picker = document.getElementById("machinePicker");
    const currentValue = picker.value || "";

    const machines = state.machines.filter(machine => {
        if (sectorId > 0 && Number(machine.sectorId || 0) !== sectorId) return false;
        if (localId > 0 && Number(machine.localId || 0) !== localId) return false;
        return true;
    });

    const options = [`<option value="">${escapeHtml(t("allMachines"))}</option>`]
        .concat(machines.map(machine => `<option value="${escapeHtmlAttr(machine.machineCode)}">${escapeHtml(machine.machineCode || getLocalizedName(machine))}</option>`));

    picker.innerHTML = options.join("");
    picker.value = currentValue;
    if (picker.value !== currentValue) {
        picker.value = "";
    }
}

function renderMachineTable(rows) {
    const body = document.getElementById("machineTableBody");

    if (!rows.length) {
        body.innerHTML = `<tr><td colspan="8" class="empty-cell">${escapeHtml(t("noRows"))}</td></tr>`;
        return;
    }

    body.innerHTML = rows.map(row => `
        <tr class="machine-row" data-machine="${escapeHtmlAttr(row.machineCode)}">
            <td>
                <strong>${escapeHtml(getMachineLabel(row))}</strong>
                <small>${escapeHtml(row.machineCode || "-")}</small>
            </td>
            <td>${escapeHtml(getLocalLabel(row))}</td>
            <td><span class="status-badge ${getStatusClass(row.statusCode)}">${escapeHtml(getStatusLabel(row))}</span></td>
            <td>${escapeHtml(row.recipeName || "-")}</td>
            <td>${escapeHtml(row.lotNo || "-")}</td>
            <td>${escapeHtml(getOperatorsLabel(row))}</td>
            <td>${Number(row.productionPercent || 0).toFixed(1)}%</td>
            <td>${escapeHtml(formatDateTime(row.lastUpdate))}</td>
        </tr>
    `).join("");

    body.querySelectorAll(".machine-row").forEach(row => {
        row.addEventListener("click", () => {
            state.selectedMachineCode = row.dataset.machine || "";
            send("production_machine_detail", { machineCode: state.selectedMachineCode });
        });
    });
}

function renderRanking(items) {
    const list = document.getElementById("rankingList");

    if (!items.length) {
        list.innerHTML = `<div class="empty-card">${escapeHtml(t("noRanking"))}</div>`;
        return;
    }

    list.innerHTML = items.map((item, index) => `
        <article class="ranking-item">
            <span class="ranking-index">${index + 1}</span>
            <div class="ranking-copy">
                <strong>${escapeHtml(getLocalizedMachineName(item))}</strong>
                <small>${escapeHtml(item.machineCode || "-")}</small>
            </div>
            <div class="ranking-metrics">
                <span>${Number(item.stopMinutes || 0).toFixed(1)}</span>
                <small>${Number(item.errorMinutes || 0).toFixed(1)} err</small>
            </div>
        </article>
    `).join("");
}

function renderTimeline(rows) {
    const wrap = document.getElementById("timelineWrap");

    if (!rows.length) {
        wrap.innerHTML = `<div class="empty-card">${escapeHtml(t("noTimeline"))}</div>`;
        return;
    }

    const headerCells = rows[0].cells.map(cell => `<th>${escapeHtml(cell.timeLabel)}</th>`).join("");
    const bodyRows = rows.map(row => `
        <tr>
            <td class="timeline-machine-cell">${escapeHtml(getLocalizedMachineName(row))}</td>
            ${row.cells.map(cell => `<td class="timeline-status-cell ${escapeHtmlAttr(cell.cssClass)}" title="${escapeHtml(cell.timeLabel)}"></td>`).join("")}
        </tr>
    `).join("");

    wrap.innerHTML = `
        <table class="timeline-table">
            <thead>
                <tr>
                    <th>${escapeHtml(t("machine"))}</th>
                    ${headerCells}
                </tr>
            </thead>
            <tbody>
                ${bodyRows}
            </tbody>
        </table>
    `;
}

function renderDetail(data) {
    setText("lblDetailMachine", data.machineCode || "-");

    const body = document.getElementById("detailTableBody");
    const rows = data.events || [];

    if (!rows.length) {
        body.innerHTML = `<tr><td colspan="5" class="empty-cell">${escapeHtml(t("selectMachine"))}</td></tr>`;
        return;
    }

    body.innerHTML = rows.map(row => `
        <tr>
            <td>${escapeHtml(row.eventDateTime || "-")}</td>
            <td><span class="status-badge ${getStatusClass(row.statusCode)}">${escapeHtml(row.statusText || getStatusLabel({ statusCode: row.statusCode }))}</span></td>
            <td>${escapeHtml(row.recipeName || "-")}</td>
            <td>${escapeHtml(row.lotNo || "-")}</td>
            <td>${escapeHtml(row.sourceFile || "-")}</td>
        </tr>
    `).join("");
}

function applyLocale() {
    document.documentElement.lang = state.locale;
    document.title = t("title");

    setText("txtHeaderTitle", t("title"));
    setText("txtHeaderSubtitle", t("subtitle"));
    setText("txtMetaUser", t("metaUser"));
    setText("txtMetaOperator", t("metaOperator"));
    setText("txtMetaPeriod", t("metaPeriod"));
    setText("txtBadge", t("badge"));
    setText("txtToolbarTitle", t("toolbarTitle"));
    setText("txtToolbarSubtitle", t("toolbarSubtitle"));
    setText("txtDateLabel", t("date"));
    setText("txtShiftLabel", t("shift"));
    setText("txtSectorLabel", t("sector"));
    setText("txtLocalLabel", t("local"));
    setText("txtMachineLabel", t("machine"));
    setText("btnRefresh", t("refresh"));
    setText("btnImport", t("import"));
    setText("txtMetricProduction", t("metricProduction"));
    setText("txtMetricRunning", t("metricRunning"));
    setText("txtMetricStopped", t("metricStopped"));
    setText("txtMetricError", t("metricError"));
    setText("txtMetricInactive", t("metricInactive"));
    setText("txtTableTitle", t("tableTitle"));
    setText("txtTableSubtitle", t("tableSubtitle"));
    setText("txtColMachine", t("colMachine"));
    setText("txtColArea", t("colArea"));
    setText("txtColStatus", t("colStatus"));
    setText("txtColRecipe", t("colRecipe"));
    setText("txtColLot", t("colLot"));
    setText("txtColOperators", t("colOperators"));
    setText("txtColPercent", t("colPercent"));
    setText("txtColUpdated", t("colUpdated"));
    setText("txtRankingTitle", t("rankingTitle"));
    setText("txtRankingSubtitle", t("rankingSubtitle"));
    setText("txtDetailTitle", t("detailTitle"));
    setText("txtDetailSubtitle", t("detailSubtitle"));
    setText("txtDetailColDate", t("detailDate"));
    setText("txtDetailColStatus", t("detailStatus"));
    setText("txtDetailColRecipe", t("detailRecipe"));
    setText("txtDetailColLot", t("detailLot"));
    setText("txtDetailColSource", t("detailSource"));
    setText("txtTimelineTitle", t("timelineTitle"));
    setText("txtTimelineSubtitle", t("timelineSubtitle"));
    setText("txtLoading", t("loading"));

    fillShiftOptions(document.getElementById("shiftPicker").value || "");
    fillSectorOptions();
    fillLocalOptions();
    fillMachineOptions();

    if (state.dashboard) {
        hydrateDashboard(state.dashboard);
    }
}

function getLocalizedName(item) {
    return state.locale === "ja-JP"
        ? (item.nameJp || item.namePt || item.machineCode || "-")
        : (item.namePt || item.nameJp || item.machineCode || "-");
}

function getShiftName(shift) {
    return state.locale === "ja-JP"
        ? (shift.nameJp || shift.namePt || `#${shift.id}`)
        : (shift.namePt || shift.nameJp || `#${shift.id}`);
}

function getMachineLabel(row) {
    return state.locale === "ja-JP"
        ? (row.machineNameJp || row.machineNamePt || row.machineCode || "-")
        : (row.machineNamePt || row.machineNameJp || row.machineCode || "-");
}

function getLocalizedMachineName(row) {
    return getMachineLabel(row);
}

function getLocalLabel(row) {
    return state.locale === "ja-JP"
        ? (row.localNameJp || row.localNamePt || "-")
        : (row.localNamePt || row.localNameJp || "-");
}

function getOperatorsLabel(row) {
    const values = state.locale === "ja-JP"
        ? (row.scheduledOperatorsJp || [])
        : (row.scheduledOperatorsPt || []);

    return values.length ? values.join(", ") : t("noOperators");
}

function getStatusLabel(row) {
    const statusCode = Number(row.statusCode || 0);
    switch (statusCode) {
        case 0:
            return t("running");
        case 1:
            return t("stopped");
        case 2:
            return t("inactive");
        case 3:
            return t("error");
        default:
            return "-";
    }
}

function getStatusClass(statusCode) {
    const code = Number(statusCode || 0);
    switch (code) {
        case 0:
            return "status-running";
        case 1:
            return "status-stopped";
        case 2:
            return "status-inactive";
        case 3:
            return "status-error";
        default:
            return "status-inactive";
    }
}

function resolveOperatorName(data) {
    return state.locale === "ja-JP"
        ? (data.currentOperatorNameJp || data.currentOperatorNamePt || "-")
        : (data.currentOperatorNamePt || data.currentOperatorNameJp || "-");
}

function formatDateTime(value) {
    if (!value) {
        return "-";
    }

    const date = new Date(String(value).replace(" ", "T"));
    if (Number.isNaN(date.getTime())) {
        return value;
    }

    return new Intl.DateTimeFormat(state.locale, {
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit"
    }).format(date);
}

function showLoading() {
    document.getElementById("loadingOverlay").classList.remove("hidden");
    document.getElementById("btnImport").disabled = true;
}

function hideLoading() {
    document.getElementById("loadingOverlay").classList.add("hidden");
    document.getElementById("btnImport").disabled = false;
}

function showNotice(message, kind) {
    const notice = document.getElementById("importNotice");
    notice.textContent = message;
    notice.className = `notice-banner notice-${kind}`;
}

function hideNotice() {
    const notice = document.getElementById("importNotice");
    notice.className = "notice-banner hidden";
    notice.textContent = "";
}

function t(key) {
    return I18N[state.locale]?.[key] || I18N["pt-BR"][key] || key;
}

function setText(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = value ?? "-";
    }
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;")
        .replaceAll("'", "&#39;");
}

function escapeHtmlAttr(value) {
    return escapeHtml(value);
}
