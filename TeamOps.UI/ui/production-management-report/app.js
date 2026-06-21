const state = {
  locale: "pt-BR",
  init: null,
  report: null,
  activeTab: "operators",
  sort: { key: "Ranking", dir: 1 }
};

const $ = (id) => document.getElementById(id);
const g = (obj, pascal, camel = pascal.charAt(0).toLowerCase() + pascal.slice(1)) =>
  obj ? (obj[pascal] ?? obj[camel]) : undefined;

function post(message) {
  if (window.chrome?.webview) {
    window.chrome.webview.postMessage(message);
  }
}

function setStatus(text, isError = false) {
  const el = $("status");
  el.textContent = text;
  el.style.color = isError ? "#b83b31" : "#65706a";
}

function nameOf(item) {
  if (!item) return "-";
  return state.locale === "ja-JP" ? (item.nameJp || item.NameJp || item.namePt || item.NamePt || "-") : (item.namePt || item.NamePt || item.nameJp || item.NameJp || "-");
}

function formatNumber(value, digits = 1) {
  const n = Number(value || 0);
  return n.toLocaleString(state.locale || "pt-BR", { maximumFractionDigits: digits, minimumFractionDigits: digits });
}

function formatPercent(value) {
  const n = Number(value || 0);
  return `${formatNumber(Math.max(0, Math.min(999.9, n)))}%`;
}

function formatInt(value) {
  return Number(value || 0).toLocaleString(state.locale || "pt-BR", { maximumFractionDigits: 0 });
}

function fillSelect(id, items, options = {}) {
  const select = $(id);
  const current = select.value;
  select.innerHTML = "";
  const all = document.createElement("option");
  all.value = "";
  all.textContent = options.allLabel || "Todos";
  select.appendChild(all);

  for (const item of items || []) {
    const option = document.createElement("option");
    option.value = String(options.value ? options.value(item) : (item.id ?? item.Id ?? ""));
    option.textContent = options.text ? options.text(item) : nameOf(item);
    select.appendChild(option);
  }

  if ([...select.options].some((option) => option.value === current)) {
    select.value = current;
  }
}

function applyInit(data) {
  state.locale = data.locale || "pt-BR";
  state.init = data;

  const defaults = data.defaults || {};
  $("startDate").value = defaults.startDateIso || "";
  $("endDate").value = defaults.endDateIso || "";
  $("onlyActive").checked = defaults.onlyActive !== false;
  $("onlyProduction").checked = defaults.onlyProduction === true;

  fillSelect("sectorId", data.sectors);
  fillSelect("localId", data.locals);
  fillSelect("shiftId", data.shifts);
  fillSelect("groupId", data.groups);
  fillSelect("groupAId", data.groups, { allLabel: "Grupo A" });
  fillSelect("groupBId", data.groups, { allLabel: "Grupo B" });
  fillSelect("operatorCode", data.operators, {
    value: (item) => item.codigoFJ,
    text: (item) => `${item.codigoFJ} - ${nameOf(item)}`
  });
  fillSelect("leaderCode", (data.operators || []).filter((item) => Number(item.isLeader || 0) === 1), {
    value: (item) => item.codigoFJ,
    text: (item) => `${item.codigoFJ} - ${nameOf(item)}`
  });
  fillSelect("machineId", data.machines, {
    value: (item) => item.id,
    text: (item) => `${item.machineCode || "-"} - ${nameOf(item)}`
  });

  $("sectorId").value = String(defaults.sectorId || "");
  $("shiftId").value = String(defaults.shiftId || "");
  renderPartCodes(data.partCodes || []);
  setStatus("Configuracoes carregadas. Clique em Gerar relatorio.");
}

function renderPartCodes(partCodes) {
  const datalist = $("partCodes");
  datalist.innerHTML = "";
  for (const code of partCodes) {
    const option = document.createElement("option");
    option.value = code;
    datalist.appendChild(option);
  }
}

function collectFilters() {
  return {
    action: "apply_filters",
    startDateIso: $("startDate").value,
    endDateIso: $("endDate").value,
    sectorId: Number($("sectorId").value || 0),
    localId: Number($("localId").value || 0),
    shiftId: Number($("shiftId").value || 0),
    groupId: Number($("groupId").value || 0),
    groupAId: Number($("groupAId").value || 0),
    groupBId: Number($("groupBId").value || 0),
    operatorCode: $("operatorCode").value || "",
    machineId: Number($("machineId").value || 0),
    partCode: $("partCode").value || "",
    leaderCode: $("leaderCode").value || "",
    onlyActive: $("onlyActive").checked,
    onlyProduction: $("onlyProduction").checked
  };
}

function applyFilters() {
  setStatus("Gerando relatorio...");
  post(collectFilters());
}

function renderReport(data) {
  state.report = data;
  renderSummary(g(data, "Summary"));
  renderPerformance(g(data, "Performance"));
  renderOperators(g(data, "Operators") || []);
  renderRankings(g(data, "Rankings"));
  renderShiftComparison(g(data, "ShiftComparison"));
  renderGroupComparison(g(data, "GroupComparison"));
  renderDailyTrend(g(data, "DailyTrend") || []);
  renderSectors(g(data, "Sectors") || []);
  renderMachines(g(data, "Machines") || []);
  renderPresence(g(data, "PresenceCrossing") || []);
  renderAlerts(g(data, "Alerts") || []);
  setStatus(`Relatorio gerado: ${g(data, "StartDateIso")} ate ${g(data, "EndDateIso")}.`);
}

function renderSummary(summary) {
  const cards = [
    ["Producao Total", formatNumber(g(summary, "ProductionTotal"))],
    ["Meta Total", formatNumber(g(summary, "MetaTotal"))],
    ["Eficiencia Media", formatPercent(g(summary, "EfficiencyAverage"))],
    ["Kadouritsu Medio", formatPercent(g(summary, "KadouritsuAverage"))],
    ["Total Operadores", formatInt(g(summary, "TotalOperators"))],
    ["Total Maquinas", formatInt(g(summary, "TotalMachines"))],
    ["Horas Trabalhadas", `${formatNumber(g(summary, "TotalWorkedHours"))}h`],
    ["Horas Extras", `${formatNumber(g(summary, "TotalOvertimeHours"))}h`],
    ["Domingos", formatInt(g(summary, "TotalWorkedSundays"))]
  ];
  $("summaryCards").innerHTML = cards.map(([label, value]) => `<article class="card"><span>${label}</span><strong>${value}</strong></article>`).join("");
}

function renderPerformance(perf) {
  const items = [
    ["LoadProductionMs", g(perf, "LoadProductionMs")],
    ["LoadOperatorsMs", g(perf, "LoadOperatorsMs")],
    ["LoadPresenceMs", g(perf, "LoadPresenceMs")],
    ["LoadChartsMs", g(perf, "LoadChartsMs")],
    ["BuildReportMs", g(perf, "BuildReportMs")],
    ["TotalMs", g(perf, "TotalMs")]
  ];
  $("performanceGrid").innerHTML = items.map(([label, value]) => `<div class="perf"><b>${formatInt(value)} ms</b><span>${label}</span></div>`).join("");
}

function table(id, columns, rows) {
  const el = $(id);
  const head = columns.map((col) => `<th data-key="${col.key || ""}">${col.label}</th>`).join("");
  const body = rows.map((row) => `<tr>${columns.map((col) => `<td>${col.render(row)}</td>`).join("")}</tr>`).join("");
  el.innerHTML = `<thead><tr>${head}</tr></thead><tbody>${body || `<tr><td colspan="${columns.length}">Sem dados no periodo.</td></tr>`}</tbody>`;
  for (const th of el.querySelectorAll("th[data-key]")) {
    if (!th.dataset.key) continue;
    th.addEventListener("click", () => sortOperators(th.dataset.key));
  }
}

function sortOperators(key) {
  if (!state.report) return;
  const rows = [...(g(state.report, "Operators") || [])];
  state.sort.dir = state.sort.key === key ? state.sort.dir * -1 : 1;
  state.sort.key = key;
  rows.sort((a, b) => {
    const av = g(a, key);
    const bv = g(b, key);
    if (typeof av === "number" || typeof bv === "number") return (Number(av || 0) - Number(bv || 0)) * state.sort.dir;
    return String(av || "").localeCompare(String(bv || ""), undefined, { numeric: true }) * state.sort.dir;
  });
  renderOperators(rows);
}

function percentBadge(value) {
  const n = Number(value || 0);
  const cls = n < 70 ? "bad" : n < 80 ? "warn" : "good";
  return `<span class="badge ${cls}">${formatPercent(n)}</span>`;
}

function renderOperators(rows) {
  table("operatorsTable", [
    { label: "Ranking", key: "Ranking", render: (r) => formatInt(g(r, "Ranking")) },
    { label: "Operador", key: "OperatorNamePt", render: (r) => `${g(r, "OperatorCode") || ""} ${g(r, "OperatorNamePt") || "-"}` },
    { label: "Grupo", key: "GroupNamePt", render: (r) => g(r, "GroupNamePt") || "-" },
    { label: "Turno", key: "ShiftNamePt", render: (r) => g(r, "ShiftNamePt") || "-" },
    { label: "Producao", key: "Production", render: (r) => formatNumber(g(r, "Production")) },
    { label: "Meta", key: "Meta", render: (r) => formatNumber(g(r, "Meta")) },
    { label: "%", key: "ProductionPercent", render: (r) => percentBadge(g(r, "ProductionPercent")) },
    { label: "Kadouritsu", key: "Kadouritsu", render: (r) => formatPercent(g(r, "Kadouritsu")) },
    { label: "Horas", key: "WorkedHours", render: (r) => `${formatNumber(g(r, "WorkedHours"))}h` },
    { label: "Extras", key: "OvertimeHours", render: (r) => `${formatNumber(g(r, "OvertimeHours"))}h` },
    { label: "Domingos", key: "WorkedSundays", render: (r) => formatInt(g(r, "WorkedSundays")) }
  ], rows);
}

function renderRankings(rankings) {
  const groups = [
    ["Top 20 Operadores", g(rankings, "Operators") || []],
    ["Top 20 Setores", g(rankings, "Sectors") || []],
    ["Top 20 Grupos", g(rankings, "Groups") || []],
    ["Top 20 Maquinas", g(rankings, "Machines") || []]
  ];
  $("rankingsGrid").innerHTML = groups.map(([title, rows]) => `
    <article class="ranking-card">
      <h3>${title}</h3>
      <table class="data-table">
        <thead><tr><th>#</th><th>Nome</th><th>Producao</th><th>Meta</th><th>%</th><th>Dif.</th></tr></thead>
        <tbody>${rows.map((r) => `<tr><td>${g(r, "Rank")}</td><td>${g(r, "Name")}</td><td>${formatNumber(g(r, "Production"))}</td><td>${formatNumber(g(r, "Meta"))}</td><td>${formatPercent(g(r, "Percent"))}</td><td>${formatNumber(g(r, "Difference"))}</td></tr>`).join("")}</tbody>
      </table>
    </article>`).join("");
}

function metricBars(containerId, rows) {
  const max = Math.max(1, ...rows.map((row) => Number(row.value || 0)));
  $(containerId).innerHTML = rows.map((row) => `
    <div class="bar-row">
      <strong>${row.label}</strong>
      <div class="bar"><i style="width:${Math.min(100, (Number(row.value || 0) / max) * 100)}%"></i></div>
      <span>${formatNumber(row.value)}</span>
    </div>`).join("");
}

function renderShiftComparison(comparison) {
  const points = g(comparison, "Points") || [];
  renderLineChart("shiftChart", points);
}

function renderLineChart(containerId, points) {
  const el = $(containerId);
  if (!points.length) {
    el.innerHTML = "<p>Sem dados para comparar os turnos.</p>";
    return;
  }

  const width = 900;
  const height = 280;
  const pad = 34;
  const max = Math.max(100, ...points.flatMap((p) => [Number(g(p, "DayKadouritsu") || 0), Number(g(p, "NightKadouritsu") || 0)]));
  const x = (index) => points.length === 1 ? width / 2 : pad + (index * (width - pad * 2)) / (points.length - 1);
  const y = (value) => height - pad - (Number(value || 0) / max) * (height - pad * 2);
  const path = (key) => points.map((p, i) => `${i === 0 ? "M" : "L"} ${x(i).toFixed(1)} ${y(g(p, key)).toFixed(1)}`).join(" ");
  const dots = (key, cls) => points.map((p, i) => `<circle class="${cls}" cx="${x(i).toFixed(1)}" cy="${y(g(p, key)).toFixed(1)}" r="4"><title>${g(p, "Day")}: ${formatPercent(g(p, key))}</title></circle>`).join("");

  el.innerHTML = `
    <div class="legend"><span class="line-dot day"></span>Dia Kadouritsu medio <span class="line-dot night"></span>Noite Kadouritsu medio</div>
    <svg viewBox="0 0 ${width} ${height}" role="img" aria-label="Kadouritsu medio por dia e turno">
      <line class="axis" x1="${pad}" y1="${height - pad}" x2="${width - pad}" y2="${height - pad}"></line>
      <line class="axis" x1="${pad}" y1="${pad}" x2="${pad}" y2="${height - pad}"></line>
      <path class="line day-line" d="${path("DayKadouritsu")}"></path>
      <path class="line night-line" d="${path("NightKadouritsu")}"></path>
      ${dots("DayKadouritsu", "day-point")}
      ${dots("NightKadouritsu", "night-point")}
    </svg>
    <div class="line-labels">${points.map((p) => `<span>${String(g(p, "Day")).slice(5)}</span>`).join("")}</div>`;
}

function renderGroupComparison(comparison) {
  const a = g(comparison, "GroupA") || {};
  const b = g(comparison, "GroupB") || {};
  metricBars("groupChart", [
    { label: `${g(a, "Name") || "Grupo A"} - Producao`, value: g(a, "Production") },
    { label: `${g(a, "Name") || "Grupo A"} - Meta`, value: g(a, "Meta") },
    { label: `${g(a, "Name") || "Grupo A"} - Extras`, value: g(a, "OvertimeHours") },
    { label: `${g(b, "Name") || "Grupo B"} - Producao`, value: g(b, "Production") },
    { label: `${g(b, "Name") || "Grupo B"} - Meta`, value: g(b, "Meta") },
    { label: `${g(b, "Name") || "Grupo B"} - Extras`, value: g(b, "OvertimeHours") }
  ]);
}

function renderDailyTrend(rows) {
  metricBars("dailyChart", rows.map((row) => ({ label: g(row, "Day"), value: g(row, "Production") })));
}

function renderSectors(rows) {
  table("sectorsTable", [
    { label: "Setor", render: (r) => g(r, "SectorNamePt") || "-" },
    { label: "Producao", render: (r) => formatNumber(g(r, "Production")) },
    { label: "Meta", render: (r) => formatNumber(g(r, "Meta")) },
    { label: "%", render: (r) => percentBadge(g(r, "Percent")) },
    { label: "Operadores", render: (r) => formatInt(g(r, "Operators")) },
    { label: "Maquinas", render: (r) => formatInt(g(r, "Machines")) },
    { label: "Kadouritsu", render: (r) => formatPercent(g(r, "Kadouritsu")) }
  ], rows);
}

function renderMachines(rows) {
  table("machinesTable", [
    { label: "Maquina", render: (r) => `${g(r, "MachineCode") || "-"} ${g(r, "MachineNamePt") || ""}` },
    { label: "Setor", render: (r) => g(r, "SectorNamePt") || "-" },
    { label: "Local", render: (r) => g(r, "LocalNamePt") || "-" },
    { label: "Producao", render: (r) => formatNumber(g(r, "Production")) },
    { label: "Rodando", render: (r) => `${formatNumber(g(r, "RunningHours"))}h` },
    { label: "Parado", render: (r) => `${formatNumber(g(r, "StoppedHours"))}h` },
    { label: "Kadouritsu", render: (r) => formatPercent(g(r, "Kadouritsu")) },
    { label: "PartCode", render: (r) => g(r, "PartCode") || "-" }
  ], rows);
}

function renderPresence(rows) {
  table("presenceTable", [
    { label: "Operador", render: (r) => `${g(r, "OperatorCode") || ""} ${g(r, "OperatorNamePt") || "-"}` },
    { label: "Presenca", render: (r) => percentBadge(g(r, "PresencePercent")) },
    { label: "Producao", render: (r) => formatNumber(g(r, "Production")) },
    { label: "Extras", render: (r) => `${formatNumber(g(r, "OvertimeHours"))}h` },
    { label: "Domingos", render: (r) => formatInt(g(r, "WorkedSundays")) },
    { label: "Absenteismo", render: (r) => formatInt(g(r, "Absenteeism")) },
    { label: "Diagnostico", render: (r) => g(r, "Insight") || "-" }
  ], rows);
}

function renderAlerts(rows) {
  $("alertsList").innerHTML = rows.length === 0
    ? "<p>Sem alertas no periodo.</p>"
    : rows.map((row) => `<div class="alert ${g(row, "Level")}"><strong>${g(row, "Title")}</strong><br>${g(row, "Target")} - ${formatNumber(g(row, "Value"))}</div>`).join("");
}

function exportRows(format) {
  if (!state.report) return;
  const rows = g(state.report, "Operators") || [];
  const headers = ["Ranking", "OperatorCode", "OperatorNamePt", "GroupNamePt", "ShiftNamePt", "Production", "Meta", "ProductionPercent", "Kadouritsu", "WorkedHours", "OvertimeHours", "WorkedSundays"];
  const csv = [headers.join(";"), ...rows.map((row) => headers.map((key) => String(g(row, key) ?? "").replaceAll(";", ",")).join(";"))].join("\r\n");
  const blob = format === "excel"
    ? new Blob([csv], { type: "application/vnd.ms-excel;charset=utf-8" })
    : new Blob([csv], { type: "text/csv;charset=utf-8" });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = `relatorio-gerencial-producao.${format === "excel" ? "xls" : "csv"}`;
  a.click();
  URL.revokeObjectURL(url);
}

function bind() {
  $("btnApply").addEventListener("click", applyFilters);
  $("btnExportCsv").addEventListener("click", () => exportRows("csv"));
  $("btnExportExcel").addEventListener("click", () => exportRows("excel"));
  $("btnPrint").addEventListener("click", () => window.print());

  for (const tab of document.querySelectorAll(".tab")) {
    tab.addEventListener("click", () => {
      state.activeTab = tab.dataset.tab;
      document.querySelectorAll(".tab").forEach((item) => item.classList.toggle("active", item === tab));
      document.querySelectorAll(".tab-panel").forEach((panel) => panel.classList.toggle("active", panel.id === `tab-${state.activeTab}`));
    });
  }
}

window.chrome?.webview?.addEventListener("message", (event) => {
  const message = event.data || {};
  if (message.type === "init") {
    applyInit(message.data);
  } else if (message.type === "report") {
    renderReport(message.data);
  } else if (message.type === "error") {
    setStatus(message.message || "Erro ao carregar relatorio.", true);
  }
});

bind();
post({ action: "load" });
