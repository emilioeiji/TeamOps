const palette = ["#1f4ea3", "#0f766e", "#b45309", "#6d28d9", "#be123c", "#0f6da8", "#4d7c0f", "#475569"];

document.addEventListener("DOMContentLoaded", () => {
    bind();
    post("load");
});

function bind() {
    document.getElementById("btnAplicar").addEventListener("click", applyFilters);
}

window.chrome?.webview?.addEventListener("message", event => {
    const payload = event.data;
    if (!payload?.type) return;

    if (payload.type === "init") {
        hydrateFilters(payload.data);
        return;
    }

    if (payload.type === "dashboard") {
        renderDashboard(payload.data);
        return;
    }

    if (payload.type === "error") {
        showToast(payload.message || "Erro ao carregar o grafico.");
    }
});

function hydrateFilters(data) {
    fillSelect("cmbShift", data.filters?.shifts || []);
    fillSelect("cmbType", data.filters?.types || []);
    fillSelect("cmbReason", data.filters?.reasons || []);
    fillSelect("cmbSector", data.filters?.sectors || []);

    document.getElementById("dtInicial").value = data.defaults?.dtInicial || "";
    document.getElementById("dtFinal").value = data.defaults?.dtFinal || "";
    document.getElementById("cmbShift").value = String(data.defaults?.shiftId ?? 0);
    document.getElementById("cmbType").value = String(data.defaults?.typeId ?? 0);
    document.getElementById("cmbReason").value = String(data.defaults?.reasonId ?? 0);
    document.getElementById("cmbSector").value = String(data.defaults?.sectorId ?? 0);
}

function renderDashboard(data) {
    setText("periodLabel", data.periodLabel || "-");
    setText("sumTotal", data.summary?.total ?? 0);
    setText("sumErrors", data.summary?.errorCount ?? 0);
    setText("sumGuidance", data.summary?.guidanceCount ?? 0);
    setText("sumSectors", data.summary?.sectorCount ?? 0);

    renderBars("chartType", data.charts?.byType || []);
    renderBars("chartReason", data.charts?.byReason || []);
    renderBars("chartOperator", data.charts?.byOperator || []);
    renderBars("chartSector", data.charts?.bySector || []);
    renderDonut(data.charts?.byShift || []);
}

function applyFilters() {
    post("apply", {
        dtInicial: document.getElementById("dtInicial").value,
        dtFinal: document.getElementById("dtFinal").value,
        shiftId: Number(document.getElementById("cmbShift").value || 0),
        typeId: Number(document.getElementById("cmbType").value || 0),
        reasonId: Number(document.getElementById("cmbReason").value || 0),
        sectorId: Number(document.getElementById("cmbSector").value || 0)
    });
}

function renderBars(id, items) {
    const root = document.getElementById(id);
    root.innerHTML = "";

    if (!items.length) {
        root.innerHTML = `<div class="empty-state">Nenhum dado para os filtros informados.</div>`;
        return;
    }

    const max = Math.max(...items.map(item => item.count), 1);

    items.forEach((item, index) => {
        const percent = Math.max((item.count / max) * 100, 6);
        const row = document.createElement("div");
        row.className = "bar-row";
        row.innerHTML = `
            <div class="bar-copy">
                <strong title="${escapeHtml(item.label)}">${escapeHtml(item.label)}</strong>
                <span>${item.count} registro(s)</span>
            </div>
            <div class="bar-track">
                <div class="bar-fill" style="width:${percent}%; background:${palette[index % palette.length]}"></div>
            </div>
        `;
        root.appendChild(row);
    });
}

function renderDonut(items) {
    const donut = document.getElementById("shiftDonut");
    const legend = document.getElementById("shiftLegend");
    donut.innerHTML = "";
    legend.innerHTML = "";

    if (!items.length) {
        donut.className = "donut-chart donut-chart-empty";
        donut.textContent = "0";
        legend.innerHTML = `<div class="empty-state">Nenhum dado para os filtros informados.</div>`;
        return;
    }

    const total = items.reduce((sum, item) => sum + item.count, 0);
    let start = 0;
    const stops = items.map((item, index) => {
        const portion = (item.count / total) * 100;
        const end = start + portion;
        const segment = `${palette[index % palette.length]} ${start}% ${end}%`;
        start = end;
        return segment;
    });

    donut.className = "donut-chart";
    donut.style.background = `conic-gradient(${stops.join(", ")})`;
    donut.innerHTML = `<div class="donut-core"><strong>${total}</strong><span>total</span></div>`;

    items.forEach((item, index) => {
        const pct = total === 0 ? 0 : Math.round((item.count / total) * 100);
        const row = document.createElement("div");
        row.className = "legend-row";
        row.innerHTML = `
            <span class="legend-dot" style="background:${palette[index % palette.length]}"></span>
            <span class="legend-label">${escapeHtml(item.label)}</span>
            <strong>${item.count}</strong>
            <small>${pct}%</small>
        `;
        legend.appendChild(row);
    });
}

function fillSelect(id, items) {
    const select = document.getElementById(id);
    select.innerHTML = items.map(item => `<option value="${item.id}">${escapeHtml(item.name)}</option>`).join("");
}

function post(action, payload = {}) {
    window.chrome?.webview?.postMessage({
        action,
        ...payload
    });
}

function setText(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = String(value ?? "-");
    }
}

function showToast(message) {
    const toast = document.getElementById("toast");
    toast.textContent = message;
    toast.classList.remove("hidden");
    clearTimeout(showToast._timer);
    showToast._timer = setTimeout(() => toast.classList.add("hidden"), 2800);
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;");
}
