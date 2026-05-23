const I18N = {
    "pt-BR": {
        title: "Kadouritsu / Machine Monitor",
        subtitle: "Painel de tempo de maquina rodando, com visao por area, maquinas, operadores previstos e historico comparativo.",
        metaUser: "Usuario",
        metaOperator: "Operador",
        metaPeriod: "Periodo",
        badge: "Monitor industrial",
        toolbarTitle: "Filtros do turno",
        toolbarSubtitle: "Importe os arquivos, escolha o recorte e acompanhe o kadouritsu primeiro por area, depois por maquina.",
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
        metricProduction: "% Kadouritsu",
        metricRunning: "Rodando",
        metricStopped: "Paradas",
        metricError: "Min. erro",
        metricInactive: "Min. inativo",
        areaBoardTitle: "Painel por area",
        areaBoardSubtitle: "A lista principal fica resumida por area para nao crescer demais com todas as maquinas.",
        areaDetailTitle: "Detalhe da area",
        areaDetailSubtitle: "Clique em uma area para listar as maquinas e os operadores previstos naquele local.",
        areaFocusLabel: "Area em foco",
        areaSummaryTemplate: "{machines} maquinas, {running} rodando, {stopped} paradas, {operators} operadores previstos.",
        areaMachineCount: "Maquinas",
        areaKadouritsu: "Kadouritsu",
        areaRunningMinutes: "Min. rodando",
        areaLastUpdate: "Ultima atualizacao",
        areaOperatorsTitle: "Operadores previstos",
        tableTitle: "Resumo por maquina",
        tableSubtitle: "Lista de maquinas da area selecionada. Clique na linha para ver o historico.",
        colMachine: "Maquina",
        colStatus: "Status",
        colRecipe: "Receita",
        colLot: "Lote",
        colOperators: "Operadores",
        colPercent: "%",
        colUpdated: "Atualizado",
        rankingTitle: "Ranking de paradas",
        rankingSubtitle: "Impacto acumulado de parada e erro dentro da area selecionada.",
        shiftCompareTitle: "Hirukin vs Yakin",
        shiftCompareSubtitle: "Comparativo de tempo rodando por turno para o mesmo dia e filtro.",
        operatorRankingTitle: "Ranking estimado por operador",
        operatorRankingSubtitle: "Historico estimado por area/turno. Em areas com dupla, os dois operadores recebem o mesmo resultado da area no dia.",
        dailyTrendTitle: "Historico dia a dia",
        dailyTrendSubtitle: "Evolucao do kadouritsu no turno selecionado ao longo dos ultimos dias.",
        areaHistoryTitle: "Historico por area",
        areaHistorySubtitle: "Comparativo rapido das areas nos ultimos dias para o mesmo turno.",
        detailTitle: "Historico recente da maquina",
        detailSubtitle: "Ultimos eventos importados para a maquina selecionada na area.",
        detailDate: "Data/Hora",
        detailStatus: "Status",
        detailRecipe: "Receita",
        detailLot: "Lote",
        detailSource: "Arquivo",
        timelineTitle: "Timeline da area",
        timelineSubtitle: "Leitura visual de 5 em 5 minutos das maquinas da area selecionada.",
        loading: "Importando producao...",
        noRows: "Nenhuma maquina encontrada para a area selecionada.",
        noAreas: "Nenhuma area encontrada para o filtro selecionado.",
        noRanking: "Sem paradas registradas para esta area.",
        noTimeline: "Sem dados para montar a timeline da area.",
        noTrend: "Sem dados para o historico.",
        noOperators: "Sem operador previsto",
        noOperatorRanking: "Sem schedule suficiente para estimar producao por operador.",
        operatorHistoryTitle: "Historico do operador",
        operatorHistorySubtitle: "Leitura gerencial por dia e area do operador selecionado.",
        operatorHistoryDate: "Data",
        operatorHistoryArea: "Area",
        operatorHistoryPercent: "% Kadouritsu",
        operatorHistoryRun: "Min. rodando",
        operatorHistoryStop: "Parado",
        operatorHistoryError: "Erro",
        operatorHistoryCoverage: "Cobertura",
        operatorHistoryAvg: "Media",
        operatorHistoryAreas: "Areas",
        operatorHistoryEmpty: "Sem historico para este operador no recorte atual.",
        operatorCoverageFull: "Turno completo",
        operatorCoverageLate: "Parcial por atraso",
        operatorCoverageEarlyLeave: "Parcial por saida antecipada",
        operatorCoverageReplacementLate: "Cobriu atraso",
        operatorCoverageReplacementEarlyLeave: "Cobriu saida",
        operatorCoveragePartialDays: "dias parciais",
        operatorCoverageFullDays: "dias completos",
        operatorCoverageWindow: "Janela",
        selectMachine: "Selecione uma maquina.",
        importingSuccess: "Importacao concluida.",
        running: "Rodando",
        stopped: "Parado",
        inactive: "Inativo",
        error: "Erro",
        machinesSuffix: "maquinas",
        operatorsSuffix: "operadores",
        avgLabel: "Media",
        minutesLabel: "min",
        compareRun: "Rodando",
        compareStop: "Parado",
        compareError: "Erro",
        compareInactive: "Inativo",
        compareMachines: "Maquinas",
        estimatedLabel: "Estimado",
        currentAreaFallback: "Todas as areas",
        machineMinutesRunning: "rodando",
        machineMinutesStopped: "parado",
        machineMinutesError: "erro",
        impactLabel: "Impacto",
        rankingMinutesSuffix: "min de impacto"
    },
    "ja-JP": {}
};

I18N["ja-JP"] = { ...I18N["pt-BR"] };

const state = {
    locale: "pt-BR",
    shifts: [],
    sectors: [],
    locals: [],
    machines: [],
    statuses: {},
    dashboard: null,
    selectedAreaId: 0,
    selectedMachineId: 0,
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
        case "operator_detail":
            hideLoading();
            renderOperatorDetail(msg.data || {});
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
    document.getElementById("btnCloseOperatorModal").addEventListener("click", closeOperatorModal);
    document.getElementById("operatorModal").addEventListener("click", event => {
        if (event.target.id === "operatorModal") {
            closeOperatorModal();
        }
    });
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
    state.statuses = normalizeStatuses(data.statuses || []);

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
    if (data.statuses) {
        state.statuses = normalizeStatuses(data.statuses);
    }
    state.selectedAreaId = resolveSelectedAreaId(data);

    setText("lblPeriod", `${formatDateTime(data.periodStart)} - ${formatDateTime(data.periodEnd)}`);
    setText("lblProductionPercent", `${toFixed(data.productionPercent)}%`);
    setText("lblMachinesRunning", data.machinesRunning ?? 0);
    setText("lblMachinesStopped", data.machinesStopped ?? 0);
    setText("lblErrorMinutes", toFixed(data.errorMinutes));
    setText("lblInactiveMinutes", toFixed(data.inactiveMinutes));

    renderAreaBoard(data.areas || []);
    renderSelectedArea();
    renderShiftComparisons(data.shiftComparisons || []);
    renderDailyTrend(data.dailyTrend || []);
    renderAreaHistory(data.areaHistory || []);
    renderOperatorRanking(data.operatorRanking || []);

    if (!state.pinnedNotice) {
        hideNotice();
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
    const machineId = Number(document.getElementById("machinePicker").value || 0);
    const selectedMachine = state.machines.find(machine => Number(machine.id || 0) === machineId);
    return {
        date: document.getElementById("datePicker").value,
        shiftId: Number(document.getElementById("shiftPicker").value || 0),
        sectorId: Number(document.getElementById("sectorPicker").value || 0),
        localId: Number(document.getElementById("localPicker").value || 0),
        machineId,
        machineCode: selectedMachine?.machineCode || ""
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

    const options = [`<option value="0">${escapeHtml(t("allMachines"))}</option>`]
        .concat(machines.map(machine => `<option value="${machine.id}">${escapeHtml(getMachinePickerLabel(machine))}</option>`));

    picker.innerHTML = options.join("");
    picker.value = currentValue;
    if (picker.value !== currentValue) {
        picker.value = "0";
    }
}

function resolveSelectedAreaId(data) {
    const localFilterId = Number(data.localId || document.getElementById("localPicker")?.value || 0);
    const areas = data.areas || [];

    if (localFilterId > 0 && areas.some(area => Number(area.localId || 0) === localFilterId)) {
        return localFilterId;
    }

    if (state.selectedAreaId > 0 && areas.some(area => Number(area.localId || 0) === state.selectedAreaId)) {
        return state.selectedAreaId;
    }

    return Number(areas[0]?.localId || 0);
}

function renderAreaBoard(areas) {
    const wrap = document.getElementById("areaBoard");

    if (!areas.length) {
        wrap.innerHTML = `<div class="empty-card">${escapeHtml(t("noAreas"))}</div>`;
        return;
    }

    const grouped = groupAreasBySector(areas);
    wrap.innerHTML = grouped.map(group => `
        <section class="area-group">
            <header class="area-group-head">
                <strong>${escapeHtml(group.label)}</strong>
                <span>${group.areas.length}</span>
            </header>
            <div class="area-group-grid">
                ${group.areas.map(area => `
                    <article class="area-card ${Number(area.localId || 0) === Number(state.selectedAreaId) ? "is-active" : ""}" data-area="${area.localId || 0}">
                        <div class="area-card-top">
                            <div>
                                <strong>${escapeHtml(getAreaLabel(area))}</strong>
                                <small>${escapeHtml(getAreaSectorLabel(area))}</small>
                            </div>
                            <span class="area-percent">${toFixed(area.productionPercent)}%</span>
                        </div>
                        <div class="area-stats">
                            <div class="stat-chip">
                                <span>${escapeHtml(t("areaMachineCount"))}</span>
                                <strong>${area.machineCount || 0}</strong>
                            </div>
                            <div class="stat-chip">
                                <span>${escapeHtml(t("metricRunning"))}</span>
                                <strong>${area.machinesRunning || 0}</strong>
                            </div>
                            <div class="stat-chip">
                                <span>${escapeHtml(t("metricStopped"))}</span>
                                <strong>${area.machinesStopped || 0}</strong>
                            </div>
                            <div class="stat-chip">
                                <span>${escapeHtml(t("metricError"))}</span>
                                <strong>${toFixed(area.errorMinutes)}</strong>
                            </div>
                        </div>
                        <div class="area-operators-line">${escapeHtml(operatorCountLabel(area))}</div>
                    </article>
                `).join("")}
            </div>
        </section>
    `).join("");

    wrap.querySelectorAll("[data-area]").forEach(card => {
        card.addEventListener("click", () => {
            state.selectedAreaId = Number(card.dataset.area || 0);
            renderSelectedArea();
            renderAreaBoard(areas);
        });
    });
}

function renderSelectedArea() {
    const area = getSelectedArea();
    const areaMachines = getSelectedAreaMachines();

    setText("lblSelectedArea", area ? getAreaLabel(area) : t("currentAreaFallback"));
    setText("lblSelectedAreaMeta", area ? getAreaSectorLabel(area) : "-");
    setText("lblAreaPill", t("areaFocusLabel"));
    setText("lblAreaHeadline", area ? getAreaLabel(area) : t("currentAreaFallback"));
    setText("lblAreaSummaryLead", buildAreaSummaryLine(area));
    setText("lblAreaHeroPercent", `${toFixed(area?.productionPercent)}%`);
    setText("lblAreaMachineCount", area?.machineCount ?? 0);
    setText("lblAreaKadouritsu", `${toFixed(area?.productionPercent)}%`);
    setText("lblAreaRunningMinutes", toFixed(area?.runningMinutes));
    setText("lblAreaLastUpdate", formatDateTime(area?.lastUpdate));

    const operators = getAreaOperators(area);
    setText("lblAreaOperatorCount", operators.length);
    renderOperatorPills(operators);
    renderMachineTable(areaMachines);
    renderRanking(getSelectedAreaRanking());
    renderTimeline(getSelectedAreaTimeline());

    if (!areaMachines.length) {
        state.selectedMachineId = 0;
        renderDetail({ machineId: 0, machineCode: "", lineCode: "", events: [] });
        return;
    }

    if (!areaMachines.some(machine => Number(machine.machineId || 0) === Number(state.selectedMachineId))) {
        state.selectedMachineId = Number(areaMachines[0].machineId || 0);
        send("production_machine_detail", { machineId: state.selectedMachineId });
    }
}

function renderOperatorPills(operators) {
    const wrap = document.getElementById("areaOperators");

    if (!operators.length) {
        wrap.innerHTML = `<div class="empty-card">${escapeHtml(t("noOperators"))}</div>`;
        return;
    }

    wrap.innerHTML = operators
        .map(name => `<span class="operator-pill">${escapeHtml(name)}</span>`)
        .join("");
}

function renderMachineTable(rows) {
    const body = document.getElementById("machineTableBody");

    if (!rows.length) {
        body.innerHTML = `<tr><td colspan="7" class="empty-cell">${escapeHtml(t("noRows"))}</td></tr>`;
        return;
    }

    body.innerHTML = rows.map(row => `
        <tr class="machine-row ${Number(row.machineId || 0) === Number(state.selectedMachineId) ? "is-selected" : ""}" data-machine-id="${row.machineId || 0}">
            <td>
                <div class="machine-main">
                    <strong>${escapeHtml(getMachineLabel(row))}</strong>
                    <small>${escapeHtml(getMachineCodeLineLabel(row))}</small>
                </div>
            </td>
            <td>
                <div class="machine-status-stack">
                    <span class="status-badge" style="${escapeHtmlAttr(getStatusBadgeStyle(row.statusCode, row.displayCode, row.sectorId))}">${escapeHtml(getStatusLabel(row))}</span>
                    <small>${escapeHtml(formatMachineMinutesSummary(row))}</small>
                </div>
            </td>
            <td>${escapeHtml(row.recipeName || "-")}</td>
            <td>${escapeHtml(row.lotNo || "-")}</td>
            <td>${escapeHtml(getOperatorsLabel(row))}</td>
            <td>
                <div class="machine-kadouritsu-cell">
                    <strong>${toFixed(row.productionPercent)}%</strong>
                    <div class="machine-progress-track">
                        <div class="machine-progress-fill" style="width:${clampPercent(row.productionPercent)}%"></div>
                    </div>
                </div>
            </td>
            <td>${escapeHtml(formatDateTime(row.lastUpdate))}</td>
        </tr>
    `).join("");

    body.querySelectorAll(".machine-row").forEach(row => {
        row.addEventListener("click", () => {
            state.selectedMachineId = Number(row.dataset.machineId || 0);
            renderMachineTable(rows);
            send("production_machine_detail", { machineId: state.selectedMachineId });
        });
    });
}

function renderRanking(items) {
    const list = document.getElementById("rankingList");

    if (!items.length) {
        list.innerHTML = `<div class="empty-card">${escapeHtml(t("noRanking"))}</div>`;
        return;
    }

    const maxImpact = Math.max(...items.map(item => Number(item.totalImpactMinutes || 0)), 1);
    list.innerHTML = items.map((item, index) => `
        <article class="ranking-item">
            <span class="ranking-index">${index + 1}</span>
            <div class="ranking-copy">
                <strong>${escapeHtml(getLocalizedMachineName(item))}</strong>
                <small>${escapeHtml(item.machineCode || "-")}</small>
                <div class="ranking-progress-track">
                    <div class="ranking-progress-fill" style="width:${Math.max(6, (Number(item.totalImpactMinutes || 0) / maxImpact) * 100)}%"></div>
                </div>
            </div>
            <div class="ranking-metrics">
                <span>${toFixed(item.totalImpactMinutes)}</span>
                <small>${toFixed(item.stopMinutes)} stop / ${toFixed(item.errorMinutes)} err</small>
            </div>
        </article>
    `).join("");
}

function renderShiftComparisons(items) {
    const wrap = document.getElementById("shiftCompareGrid");

    if (!items.length) {
        wrap.innerHTML = `<div class="empty-card">${escapeHtml(t("noTrend"))}</div>`;
        return;
    }

    wrap.innerHTML = items.map(item => `
        <article class="compare-item">
            <span class="access-badge">${escapeHtml(getShiftCompareName(item))}</span>
            <strong>${toFixed(item.productionPercent)}%</strong>
            <small>${escapeHtml(formatDateTime(item.start))} - ${escapeHtml(formatDateTime(item.end))}</small>
            <div class="compare-meta">
                <span>${escapeHtml(t("compareRun"))}: <strong>${toFixed(item.runningMinutes)}</strong></span>
                <span>${escapeHtml(t("compareStop"))}: <strong>${toFixed(item.stoppedMinutes)}</strong></span>
                <span>${escapeHtml(t("compareError"))}: <strong>${toFixed(item.errorMinutes)}</strong></span>
                <span>${escapeHtml(t("compareInactive"))}: <strong>${toFixed(item.inactiveMinutes)}</strong></span>
            </div>
        </article>
    `).join("");
}

function renderDailyTrend(items) {
    const wrap = document.getElementById("dailyTrend");

    if (!items.length) {
        wrap.innerHTML = `<div class="empty-card">${escapeHtml(t("noTrend"))}</div>`;
        return;
    }

    wrap.innerHTML = buildDailyTrendLineChart(items);
}

function renderAreaHistory(items) {
    const wrap = document.getElementById("areaHistory");

    if (!items.length) {
        wrap.innerHTML = `<div class="empty-card">${escapeHtml(t("noTrend"))}</div>`;
        return;
    }

    wrap.innerHTML = items.map(item => `
        <article class="area-history-item">
            <strong>${escapeHtml(getAreaLabel(item))}</strong>
            ${buildAreaHistoryLineChart(item.days || [], getAreaLabel(item))}
            <div class="spark-avg">${toFixed(averagePercent(item.days || []))}%</div>
        </article>
    `).join("");
}

function buildAreaHistoryLineChart(days, areaLabel) {
    if (!days.length) {
        return `<div class="spark-empty">${escapeHtml(t("noTrend"))}</div>`;
    }

    const width = 420;
    const height = 88;
    const padding = { top: 10, right: 8, bottom: 22, left: 8 };
    const innerWidth = width - padding.left - padding.right;
    const innerHeight = height - padding.top - padding.bottom;
    const maxValue = 100;

    const points = days.map((day, index) => {
        const x = padding.left + (days.length <= 1 ? innerWidth / 2 : (innerWidth * index) / (days.length - 1));
        const y = padding.top + innerHeight - ((Math.max(0, Math.min(maxValue, Number(day.productionPercent || 0))) / maxValue) * innerHeight);
        return {
            x,
            y,
            label: day.label || "-",
            value: Number(day.productionPercent || 0)
        };
    });

    const path = points.map((point, index) => `${index === 0 ? "M" : "L"} ${point.x} ${point.y}`).join(" ");
    const gridLines = [0, 50, 100].map(percent => {
        const y = padding.top + innerHeight - ((percent / maxValue) * innerHeight);
        return `<line x1="${padding.left}" x2="${width - padding.right}" y1="${y}" y2="${y}"></line>`;
    }).join("");
    const labels = points.map(point => `<text x="${point.x}" y="${height - 6}" text-anchor="middle">${escapeHtml(point.label)}</text>`).join("");

    return `
        <div class="spark-line-shell">
            <svg class="spark-line-chart" viewBox="0 0 ${width} ${height}" preserveAspectRatio="none" role="img" aria-label="Historico por area de ${escapeHtmlAttr(areaLabel)}">
                <g class="spark-line-grid">${gridLines}</g>
                ${points.length >= 2 ? `<path class="spark-line-path" d="${path}" fill="none"></path>` : ""}
                ${points.map(point => `<circle class="spark-line-point" cx="${point.x}" cy="${point.y}" r="3.5"><title>${escapeHtml(point.label)} - ${escapeHtml(toFixed(point.value))}%</title></circle>`).join("")}
                <g class="spark-line-labels">${labels}</g>
            </svg>
        </div>
    `;
}

function renderOperatorRanking(items) {
    const wrap = document.getElementById("operatorRankingList");

    if (!items.length) {
        wrap.innerHTML = `<div class="empty-card">${escapeHtml(t("noOperatorRanking"))}</div>`;
        return;
    }

    wrap.innerHTML = items.map((item, index) => `
        <article class="operator-ranking-item" data-operator="${escapeHtmlAttr(item.operatorCodigoFJ || "")}">
            <span class="operator-ranking-index">${index + 1}</span>
            <div class="operator-ranking-copy">
                <strong>${escapeHtml(getOperatorName(item))}</strong>
                <small>${escapeHtml(item.operatorCodigoFJ || "-")}</small>
                <div class="operator-ranking-locals">
                    ${getOperatorLocalNames(item).map(name => `<span class="operator-ranking-local">${escapeHtml(name)}</span>`).join("")}
                </div>
                <div class="operator-ranking-coverage">
                    <span class="coverage-chip coverage-chip-${escapeHtmlAttr(Number(item.partialCoverageDays || 0) > 0 ? "partial" : "full")}">${escapeHtml(formatOperatorCoverageSummary(item))}</span>
                </div>
                <div class="operator-ranking-progress-track">
                    <div class="operator-ranking-progress-fill" style="width:${clampPercent(item.estimatedKadouritsuPercent)}%"></div>
                </div>
            </div>
            <div class="operator-ranking-metrics">
                <span>${toFixed(item.estimatedKadouritsuPercent)}%</span>
                <small>${toFixed(item.estimatedRunningMinutes)} ${escapeHtml(t("minutesLabel"))}</small>
            </div>
        </article>
    `).join("");

    wrap.querySelectorAll(".operator-ranking-item").forEach(card => {
        card.addEventListener("click", () => {
            const operatorCodigoFJ = card.dataset.operator || "";
            if (!operatorCodigoFJ) {
                return;
            }

            showLoading();
            send("production_operator_detail", {
                ...buildFilterPayload(),
                operatorCodigoFJ
            });
        });
    });
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
            <td class="timeline-machine-cell">${escapeHtml(getMachineCodeLineLabel(row))}</td>
            ${row.cells.map(cell => `<td class="timeline-status-cell" style="${escapeHtmlAttr(getTimelineCellStyle(cell.statusCode, cell.displayCode, row.sectorId))}" title="${escapeHtml(cell.timeLabel)}"></td>`).join("")}
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
    setText("lblDetailMachine", buildMachineDetailLabel(data));

    const body = document.getElementById("detailTableBody");
    const rows = data.events || [];

    if (!rows.length) {
        body.innerHTML = `<tr><td colspan="5" class="empty-cell">${escapeHtml(t("selectMachine"))}</td></tr>`;
        return;
    }

    body.innerHTML = rows.map(row => `
        <tr>
            <td>${escapeHtml(row.eventDateTime || "-")}</td>
            <td><span class="status-badge" style="${escapeHtmlAttr(getStatusBadgeStyle(row.statusCode, null, row.sectorId))}">${escapeHtml(getStatusLabel({ statusCode: row.statusCode, sectorId: row.sectorId }))}</span></td>
            <td>${escapeHtml(row.recipeName || "-")}</td>
            <td>${escapeHtml(row.lotNo || "-")}</td>
            <td>${escapeHtml(row.sourceFile || "-")}</td>
        </tr>
    `).join("");
}

function openOperatorDetail(operatorCodigoFJ) {
    if (!operatorCodigoFJ) {
        return;
    }

    showLoading();
    send("production_operator_detail", {
        ...buildFilterPayload(),
        operatorCodigoFJ
    });
}

function renderOperatorDetail(data) {
    setText("lblOperatorModalName", getOperatorModalName(data));
    setText("lblOperatorModalCode", data.operatorCodigoFJ || "-");
    setText("lblOperatorModalAvg", `${toFixed(data.averageKadouritsuPercent)}%`);
    setText("lblOperatorModalRunning", toFixed(data.totalRunningMinutes));
    setText("lblOperatorModalAreas", data.assignedAreaCount ?? 0);

    const areaWrap = document.getElementById("operatorModalAreas");
    const localNames = state.locale === "ja-JP"
        ? (data.localNamesJp || [])
        : (data.localNamesPt || []);
    areaWrap.innerHTML = localNames.length
        ? localNames.map(name => `<span class="operator-ranking-local">${escapeHtml(name)}</span>`).join("")
        : `<span class="operator-ranking-local">${escapeHtml(t("currentAreaFallback"))}</span>`;

    const body = document.getElementById("operatorModalTableBody");
    const rows = data.entries || [];

    if (!rows.length) {
        body.innerHTML = `<tr><td colspan="7" class="empty-cell">${escapeHtml(t("operatorHistoryEmpty"))}</td></tr>`;
    } else {
        body.innerHTML = rows.map(row => `
            <tr>
                <td>${escapeHtml(formatDateOnly(row.date) || row.label || "-")}</td>
                <td>${escapeHtml(getOperatorEntryAreaName(row))}</td>
                <td>${toFixed(row.kadouritsuPercent)}%</td>
                <td>
                    <div class="coverage-cell">
                        <span class="coverage-chip coverage-chip-${escapeHtmlAttr(row.isPartialCoverage ? "partial" : "full")}">${escapeHtml(coverageLabel(row.coverageMode))}</span>
                        <small>${escapeHtml(t("operatorCoverageWindow"))}: ${toFixed(row.effectiveMinutes)} / ${toFixed(row.plannedMinutes)} min</small>
                    </div>
                </td>
                <td>${toFixed(row.runningMinutes)}</td>
                <td>${toFixed(row.stoppedMinutes)}</td>
                <td>${toFixed(row.errorMinutes)}</td>
            </tr>
        `).join("");
    }

    document.getElementById("operatorModal").classList.remove("hidden");
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
    setText("txtAreaBoardTitle", t("areaBoardTitle"));
    setText("txtAreaBoardSubtitle", t("areaBoardSubtitle"));
    setText("txtAreaDetailTitle", t("areaDetailTitle"));
    setText("txtAreaDetailSubtitle", t("areaDetailSubtitle"));
    setText("txtAreaKadouritsuHero", t("areaKadouritsu"));
    setText("txtAreaMachineCount", t("areaMachineCount"));
    setText("txtAreaKadouritsu", t("areaKadouritsu"));
    setText("txtAreaRunningMinutes", t("areaRunningMinutes"));
    setText("txtAreaLastUpdate", t("areaLastUpdate"));
    setText("txtAreaOperatorsTitle", t("areaOperatorsTitle"));
    setText("txtColMachine", t("colMachine"));
    setText("txtColStatus", t("colStatus"));
    setText("txtColRecipe", t("colRecipe"));
    setText("txtColLot", t("colLot"));
    setText("txtColOperators", t("colOperators"));
    setText("txtColPercent", t("colPercent"));
    setText("txtColUpdated", t("colUpdated"));
    setText("txtRankingTitle", t("rankingTitle"));
    setText("txtRankingSubtitle", t("rankingSubtitle"));
    setText("txtShiftCompareTitle", t("shiftCompareTitle"));
    setText("txtShiftCompareSubtitle", t("shiftCompareSubtitle"));
    setText("txtOperatorRankingTitle", t("operatorRankingTitle"));
    setText("txtOperatorRankingSubtitle", t("operatorRankingSubtitle"));
    setText("txtOperatorModalTitle", t("operatorHistoryTitle"));
    setText("txtOperatorModalSubtitle", t("operatorHistorySubtitle"));
    setText("txtOperatorModalDate", t("operatorHistoryDate"));
    setText("txtOperatorModalArea", t("operatorHistoryArea"));
    setText("txtOperatorModalPercent", t("operatorHistoryPercent"));
    setText("txtOperatorModalCoverage", t("operatorHistoryCoverage"));
    setText("txtOperatorModalRunMinutes", t("operatorHistoryRun"));
    setText("txtOperatorModalStopMinutes", t("operatorHistoryStop"));
    setText("txtOperatorModalErrorMinutes", t("operatorHistoryError"));
    setText("txtOperatorModalAvg", t("operatorHistoryAvg"));
    setText("txtOperatorModalRunning", t("areaRunningMinutes"));
    setText("txtOperatorModalAreas", t("operatorHistoryAreas"));
    setText("txtDailyTrendTitle", t("dailyTrendTitle"));
    setText("txtDailyTrendSubtitle", t("dailyTrendSubtitle"));
    setText("txtAreaHistoryTitle", t("areaHistoryTitle"));
    setText("txtAreaHistorySubtitle", t("areaHistorySubtitle"));
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

function getSelectedArea() {
    const areas = state.dashboard?.areas || [];
    return areas.find(area => Number(area.localId || 0) === Number(state.selectedAreaId)) || areas[0] || null;
}

function getSelectedAreaMachines() {
    const machines = state.dashboard?.machines || [];
    const area = getSelectedArea();
    if (!area) return machines;
    return machines.filter(machine => Number(machine.localId || 0) === Number(area.localId || 0));
}

function getSelectedAreaRanking() {
    const area = getSelectedArea();
    const items = state.dashboard?.ranking || [];
    if (!area) return items;
    return items.filter(item => Number(item.localId || 0) === Number(area.localId || 0));
}

function getSelectedAreaTimeline() {
    const area = getSelectedArea();
    const rows = state.dashboard?.timeline || [];
    if (!area) return rows;
    return rows.filter(row => Number(row.localId || 0) === Number(area.localId || 0));
}

function getAreaOperators(area) {
    if (!area) return [];
    const values = state.locale === "ja-JP"
        ? (area.scheduledOperatorsJp || [])
        : (area.scheduledOperatorsPt || []);
    return values.filter(Boolean);
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

function getMachineCodeLineLabel(row) {
    const lineCode = row.lineCode || "";
    const machineCode = row.machineCode || "-";
    return lineCode ? `${lineCode} / ${machineCode}` : machineCode;
}

function getMachinePickerLabel(machine) {
    const name = getLocalizedName(machine);
    const codeLabel = getMachineCodeLineLabel(machine);
    return name && name !== machine.machineCode
        ? `${codeLabel} - ${name}`
        : codeLabel;
}

function getLocalizedMachineName(row) {
    return getMachineLabel(row);
}

function getAreaLabel(area) {
    return state.locale === "ja-JP"
        ? (area.localNameJp || area.localNamePt || t("currentAreaFallback"))
        : (area.localNamePt || area.localNameJp || t("currentAreaFallback"));
}

function getAreaSectorLabel(area) {
    return state.locale === "ja-JP"
        ? (area.sectorNameJp || area.sectorNamePt || "-")
        : (area.sectorNamePt || area.sectorNameJp || "-");
}

function getShiftCompareName(item) {
    return state.locale === "ja-JP"
        ? (item.shiftNameJp || item.shiftNamePt || "-")
        : (item.shiftNamePt || item.shiftNameJp || "-");
}

function getOperatorsLabel(row) {
    const values = state.locale === "ja-JP"
        ? (row.scheduledOperatorsJp || [])
        : (row.scheduledOperatorsPt || []);

    return values.length ? values.join(", ") : t("noOperators");
}

function getOperatorName(item) {
    return state.locale === "ja-JP"
        ? (item.operatorNameJp || item.operatorNamePt || item.operatorCodigoFJ || "-")
        : (item.operatorNamePt || item.operatorNameJp || item.operatorCodigoFJ || "-");
}

function getOperatorModalName(item) {
    return state.locale === "ja-JP"
        ? (item.operatorNameJp || item.operatorNamePt || item.operatorCodigoFJ || "-")
        : (item.operatorNamePt || item.operatorNameJp || item.operatorCodigoFJ || "-");
}

function getOperatorLocalNames(item) {
    const values = state.locale === "ja-JP"
        ? (item.localNamesJp || [])
        : (item.localNamesPt || []);

    return values.length ? values : [t("currentAreaFallback")];
}

function operatorCountLabel(area) {
    const count = getAreaOperators(area).length;
    return `${count} ${t("operatorsSuffix")}`;
}

function buildAreaSummaryLine(area) {
    if (!area) {
        return t("currentAreaFallback");
    }

    return t("areaSummaryTemplate")
        .replace("{machines}", String(area.machineCount || 0))
        .replace("{running}", String(area.machinesRunning || 0))
        .replace("{stopped}", String(area.machinesStopped || 0))
        .replace("{operators}", String(getAreaOperators(area).length));
}

function formatMachineMinutesSummary(row) {
    return `${toFixed(row.runningMinutes)} ${t("machineMinutesRunning")} · ${toFixed(row.stoppedMinutes)} ${t("machineMinutesStopped")} · ${toFixed(row.errorMinutes)} ${t("machineMinutesError")}`;
}

function formatOperatorCoverageSummary(item) {
    const partial = Number(item.partialCoverageDays || 0);
    const full = Number(item.fullCoverageDays || 0);
    if (partial > 0) {
        return `${partial} ${t("operatorCoveragePartialDays")}`;
    }

    return `${full} ${t("operatorCoverageFullDays")}`;
}

function coverageLabel(mode) {
    switch ((mode || "full").toLowerCase()) {
        case "late":
            return t("operatorCoverageLate");
        case "early_leave":
            return t("operatorCoverageEarlyLeave");
        case "replacement_late":
            return t("operatorCoverageReplacementLate");
        case "replacement_early_leave":
            return t("operatorCoverageReplacementEarlyLeave");
        default:
            return t("operatorCoverageFull");
    }
}

function averagePercent(days) {
    if (!days.length) return 0;
    return days.reduce((sum, item) => sum + Number(item.productionPercent || 0), 0) / days.length;
}

function groupAreasBySector(areas) {
    const groups = new Map();
    areas.forEach(area => {
        const key = String(area.sectorId || 0);
        if (!groups.has(key)) {
            groups.set(key, {
                key,
                label: getAreaSectorLabel(area),
                areas: []
            });
        }
        groups.get(key).areas.push(area);
    });

    return [...groups.values()];
}

function getStatusLabel(row) {
    const statusCode = Number(row.statusCode || 0);
    const displayCode = Number(row.displayCode || statusCode || 0);
    const configured = findStatusDefinition(row.sectorId, statusCode, displayCode);
    if (configured) {
        return state.locale === "ja-JP"
            ? (configured.nameJp || configured.namePt || String(statusCode))
            : (configured.namePt || configured.nameJp || String(statusCode));
    }

    switch (statusCode) {
        case 0:
            return t("running");
        case 1:
            return t("inactive");
        case 3:
            return t("stopped");
        case 4:
            return t("error");
        default:
            return t("inactive");
    }
}

function getStatusClass(statusCode) {
    const code = Number(statusCode || 0);
    switch (code) {
        case 0:
            return "status-running";
        case 1:
            return "status-inactive";
        case 3:
            return "status-stopped";
        case 4:
            return "status-error";
        default:
            return "status-inactive";
    }
}

function getStatusBadgeStyle(statusCode, fallbackCode = null, sectorId = null) {
    const configured = findStatusDefinition(sectorId, statusCode, fallbackCode);
    if (!configured) {
        return "";
    }

    const background = configured.colorHex || "#5B88E8";
    const color = configured.textColorHex || "#FFFFFF";
    const border = background.toUpperCase() === "#FFFFFF"
        ? "border:1px solid #D8E0EA;"
        : "";

    return `background:${background};color:${color};${border}`;
}

function getTimelineCellStyle(statusCode, fallbackCode = null, sectorId = null) {
    const configured = findStatusDefinition(sectorId, statusCode, fallbackCode);
    if (!configured) {
        return "";
    }

    const background = configured.colorHex || "#5B88E8";
    const border = background.toUpperCase() === "#FFFFFF"
        ? "box-shadow:inset 0 0 0 1px #D8E0EA;"
        : "";

    return `background:${background};${border}`;
}

function normalizeStatuses(statuses) {
    const map = {};
    (statuses || []).forEach(item => {
        const code = Number(item.statusCode ?? item.code ?? -999);
        if (!Number.isFinite(code)) return;
        const sectorId = Number(item.sectorId || 0);

        const status = {
            sectorId,
            statusCode: code,
            displayCode: Number(item.displayCode || 0),
            classification: item.classification || "",
            namePt: item.namePt || "",
            nameJp: item.nameJp || item.namePt || "",
            colorHex: item.colorHex || "#5B88E8",
            textColorHex: item.textColorHex || "#FFFFFF"
        };

        if (sectorId > 0) {
            map[`${sectorId}:${code}`] = status;
            return;
        }

        map[String(code)] = status;
    });
    return map;
}

function findStatusDefinition(sectorId, statusCode, fallbackCode = null) {
    const normalizedSectorId = Number(sectorId || 0);
    const code = Number(statusCode || 0);
    const fallback = Number(fallbackCode || 0);

    if (normalizedSectorId > 0) {
        return state.statuses[`${normalizedSectorId}:${code}`]
            || state.statuses[String(code)]
            || state.statuses[`${normalizedSectorId}:${fallback}`]
            || state.statuses[String(fallback)];
    }

    return state.statuses[String(code)] || state.statuses[String(fallback)];
}

function buildMachineDetailLabel(data) {
    const machineCode = data.machineCode || "-";
    const lineCode = data.lineCode || "";
    return lineCode ? `${lineCode} / ${machineCode}` : machineCode;
}

function getOperatorEntryAreaName(item) {
    return state.locale === "ja-JP"
        ? (item.localNameJp || item.localNamePt || "-")
        : (item.localNamePt || item.localNameJp || "-");
}

function buildDailyTrendLineChart(items) {
    const orderedDays = [...items]
        .sort((left, right) => String(left.date || "").localeCompare(String(right.date || "")))
        .reduce((acc, item) => {
            if (!acc.includes(item.label)) {
                acc.push(item.label);
            }
            return acc;
        }, []);

    const hirukinItems = items.filter(item => getShiftSeriesKey(item) === "hirukin");
    const yakinItems = items.filter(item => getShiftSeriesKey(item) === "yakin");

    const width = 760;
    const height = 240;
    const padding = { top: 24, right: 20, bottom: 42, left: 44 };
    const innerWidth = width - padding.left - padding.right;
    const innerHeight = height - padding.top - padding.bottom;
    const maxValue = Math.max(
        100,
        ...hirukinItems.map(item => Number(item.productionPercent || 0)),
        ...yakinItems.map(item => Number(item.productionPercent || 0)),
        1
    );

    const toPoint = (value, index, count) => {
        const x = padding.left + (count <= 1 ? innerWidth / 2 : (innerWidth * index) / (count - 1));
        const y = padding.top + innerHeight - ((value / maxValue) * innerHeight);
        return `${x},${y}`;
    };

    const toSeriesPoints = seriesItems => seriesItems
        .map(item => {
            const index = orderedDays.indexOf(item.label);
            if (index < 0) {
                return null;
            }

            const [x, y] = toPoint(Number(item.productionPercent || 0), index, orderedDays.length).split(",");
            return {
                x: Number(x),
                y: Number(y)
            };
        })
        .filter(Boolean);

    const hirukinPoints = toSeriesPoints(hirukinItems);
    const yakinPoints = toSeriesPoints(yakinItems);
    const buildPath = points => points
        .map((point, index) => `${index === 0 ? "M" : "L"} ${point.x} ${point.y}`)
        .join(" ");
    const dayLabels = orderedDays.map((label, index) => {
        const x = padding.left + (orderedDays.length <= 1 ? innerWidth / 2 : (innerWidth * index) / (orderedDays.length - 1));
        return `<text x="${x}" y="${height - 12}" text-anchor="middle">${escapeHtml(label)}</text>`;
    }).join("");
    const gridLines = [0, 25, 50, 75, 100].map(percent => {
        const y = padding.top + innerHeight - ((percent / maxValue) * innerHeight);
        return `
            <line x1="${padding.left}" x2="${width - padding.right}" y1="${y}" y2="${y}"></line>
            <text x="${padding.left - 10}" y="${y + 4}" text-anchor="end">${percent}%</text>
        `;
    }).join("");

    return `
        <div class="trend-legend">
            <span class="trend-legend-item"><i class="trend-dot trend-dot-hirukin"></i>Hirukin</span>
            <span class="trend-legend-item"><i class="trend-dot trend-dot-yakin"></i>Yakin</span>
        </div>
        <div class="trend-chart-shell">
            <svg class="trend-chart" viewBox="0 0 ${width} ${height}" preserveAspectRatio="none" role="img" aria-label="Historico diario de kadouritsu por turno">
                <g class="trend-grid">${gridLines}</g>
                ${hirukinPoints.length >= 2 ? `<path class="trend-line trend-line-hirukin" d="${buildPath(hirukinPoints)}" fill="none"></path>` : ""}
                ${yakinPoints.length >= 2 ? `<path class="trend-line trend-line-yakin" d="${buildPath(yakinPoints)}" fill="none"></path>` : ""}
                ${hirukinPoints.map(point => {
                    return `<circle class="trend-point trend-point-hirukin" cx="${point.x}" cy="${point.y}" r="4"></circle>`;
                }).join("")}
                ${yakinPoints.map(point => {
                    return `<circle class="trend-point trend-point-yakin" cx="${point.x}" cy="${point.y}" r="4"></circle>`;
                }).join("")}
                <g class="trend-axis-labels">${dayLabels}</g>
            </svg>
        </div>
    `;
}

function getShiftSeriesKey(item) {
    const name = `${item.shiftNamePt || ""} ${item.shiftNameJp || ""}`.toLowerCase();
    if (name.includes("yakin") || name.includes("夜")) {
        return "yakin";
    }

    return "hirukin";
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

function formatDateOnly(value) {
    if (!value) {
        return "-";
    }

    const date = new Date(String(value).replace(" ", "T"));
    if (Number.isNaN(date.getTime())) {
        return value;
    }

    return new Intl.DateTimeFormat(state.locale, {
        month: "2-digit",
        day: "2-digit"
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

function closeOperatorModal() {
    document.getElementById("operatorModal").classList.add("hidden");
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

function toFixed(value) {
    return Number(value || 0).toFixed(1);
}

function clampPercent(value) {
    return Math.max(0, Math.min(100, Number(value || 0)));
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
