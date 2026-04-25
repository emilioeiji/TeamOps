const I18N = {
    "pt-BR": {
        title: "Presenca por setor",
        subtitle: "Previsto do haidai, confirmacao do operador e leitura visual dos dois setores em uma unica tela.",
        metaDate: "Data",
        metaShift: "Turno",
        metaImport: "Schedule",
        badge: "Layout combinado",
        toolbarTitle: "Filtro unico para os dois setores",
        toolbarSubtitle: "Importe o haidai uma vez e acompanhe G-Bareru e DAD lado a lado com o mesmo periodo e turno.",
        date: "Data",
        shift: "Turno",
        refresh: "Atualizar",
        import: "Importar haidai",
        planned: "Previstos",
        confirmed: "Confirmados",
        missing: "Faltantes",
        extra: "Extras",
        imported: "Importado",
        pending: "Pendente",
        sectorPlanned: "Previsto",
        sectorConfirmed: "Confirmado",
        sectorMissing: "Faltando",
        sectorExtra: "Extra",
        noSchedule: "Sem schedule importado para esta data e turno.",
        importLoading: "Importando schedule...",
        local: "Local",
        emptyLocal: "Sem operadores neste local.",
        corridor: "Corredor",
        mixed: "Parcial",
        confirmedState: "Confirmado",
        plannedState: "Previsto",
        extraState: "Extra"
    },
    "ja-JP": {
        title: "セクター出勤レイアウト",
        subtitle: "廃台スケジュールと作業者の確認状況を、2つのセクターで同時に見られます。",
        metaDate: "日付",
        metaShift: "シフト",
        metaImport: "スケジュール",
        badge: "統合レイアウト",
        toolbarTitle: "2セクター共通フィルター",
        toolbarSubtitle: "廃台を一度取り込むだけで、G-Bareru と DAD を同じ日付・シフトで同時に確認できます。",
        date: "日付",
        shift: "シフト",
        refresh: "更新",
        import: "廃台取込",
        planned: "予定",
        confirmed: "確認済み",
        missing: "未確認",
        extra: "予定外",
        imported: "取込済み",
        pending: "未取込",
        sectorPlanned: "予定",
        sectorConfirmed: "確認済み",
        sectorMissing: "未確認",
        sectorExtra: "予定外",
        noSchedule: "この日付・シフトにはスケジュールがありません。",
        importLoading: "スケジュール取込中...",
        local: "工程",
        emptyLocal: "この工程に作業者はいません。",
        corridor: "通路",
        mixed: "一部確認",
        confirmedState: "確認済み",
        plannedState: "予定",
        extraState: "予定外"
    }
};

const state = {
    locale: "pt-BR",
    shifts: [],
    layouts: {},
    localsCatalog: [],
    board: null,
    pinnedNotice: false
};

document.addEventListener("DOMContentLoaded", async () => {
    bindEvents();
    await bootstrap();
});

window.chrome?.webview?.addEventListener("message", event => {
    const payload = event.data;

    if (!payload?.type) {
        return;
    }

    switch (payload.type) {
        case "init":
            hydrateInit(payload.data);
            break;
        case "board":
            hydrateBoard(payload.data);
            break;
        case "import_result":
            hideLoading();
            state.pinnedNotice = true;
            showNotice(payload.data.message, payload.data.success ? "success" : "error");
            break;
        case "error":
            hideLoading();
            state.pinnedNotice = true;
            showNotice(payload.message || "Erro inesperado.", "error");
            break;
    }
});

function bindEvents() {
    document.getElementById("btnRefresh").addEventListener("click", refreshBoard);
    document.getElementById("btnImportSchedule").addEventListener("click", importSchedule);
    document.getElementById("datePicker").addEventListener("change", refreshBoard);
    document.getElementById("shiftPicker").addEventListener("change", refreshBoard);
}

async function bootstrap() {
    state.layouts = await loadLayouts();
    window.chrome?.webview?.postMessage({ action: "load" });
}

async function loadLayouts() {
    try {
        const response = await fetch("layouts.json", { cache: "no-store" });
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error("Nao foi possivel carregar layouts.json", error);
        return {};
    }
}

function hydrateInit(data) {
    state.locale = data.locale === "ja-JP" ? "ja-JP" : "pt-BR";
    state.shifts = data.shifts || [];
    state.localsCatalog = data.locals || [];

    const datePicker = document.getElementById("datePicker");
    const shiftPicker = document.getElementById("shiftPicker");

    datePicker.value = data.defaults?.dateIso || "";

    shiftPicker.innerHTML = state.shifts
        .map(shift => `<option value="${shift.id}">${escapeHtml(getShiftName(shift))}</option>`)
        .join("");

    shiftPicker.value = String(data.defaults?.shiftId || "");

    applyLocale();
}

function hydrateBoard(data) {
    state.board = data;

    setText("lblCurrentDate", formatDateLabel(data.dateIso));
    setText("lblCurrentShift", resolveShiftName(data.shiftId));
    setText("lblImportState", data.summary?.imported ? t("imported") : t("pending"));
    setText("summaryPlanned", data.summary?.plannedCount ?? 0);
    setText("summaryConfirmed", data.summary?.confirmedCount ?? 0);
    setText("summaryMissing", data.summary?.missingCount ?? 0);
    setText("summaryExtra", data.summary?.extraCount ?? 0);

    renderSectors(data.sectors || []);

    if (state.pinnedNotice) {
        return;
    }

    if ((data.summary?.plannedCount ?? 0) === 0) {
        showNotice(t("noSchedule"), "warning");
    } else {
        hideNotice();
    }
}

function refreshBoard() {
    state.pinnedNotice = false;
    window.chrome?.webview?.postMessage({
        action: "refresh",
        date: document.getElementById("datePicker").value,
        shiftId: Number(document.getElementById("shiftPicker").value || 0)
    });
}

function importSchedule() {
    showLoading();

    window.chrome?.webview?.postMessage({
        action: "import_schedule",
        date: document.getElementById("datePicker").value,
        shiftId: Number(document.getElementById("shiftPicker").value || 0)
    });
}

function renderSectors(sectors) {
    const grid = document.getElementById("sectorsGrid");

    grid.innerHTML = sectors.map(sector => renderSectorCard(sector)).join("");
}

function renderSectorCard(sector) {
    const layout = state.layouts[String(sector.id)] || state.layouts[sector.id];
    if (!layout) {
        return renderFallbackSectorCard(sector);
    }

    const localsMap = new Map((sector.locals || []).map(local => [Number(local.localId), local]));

    const blueprint = (layout.locals || []).map(slot => {
        const localId = Number(slot.localId || 0);
        const localState = localsMap.get(localId) || createEmptyLocal(localId, resolveLocalName(localId, slot.label));
        const statusClass = getLocalStatusClass(localState);
        const style = toBlueprintStyle(slot, layout);
        const localTitle = localState.localName || resolveLocalName(localId, slot.label);

        return `
            <div class="local-slot ${statusClass}" style="${style}" title="${escapeHtml(localState.tooltip || localTitle)}">
                <div class="local-slot-head">
                    <span class="local-slot-name">${escapeHtml(localTitle)}</span>
                    <span class="local-slot-state">${escapeHtml(getLocalStateLabel(localState))}</span>
                </div>
                <div class="local-slot-metrics">
                    <span>P ${localState.plannedCount ?? 0}</span>
                    <span>C ${localState.confirmedCount ?? 0}</span>
                </div>
                <div class="local-people">
                    ${renderPeople(localState)}
                </div>
            </div>
        `;
    }).join("");

    const corridors = (layout.corridors || []).map(corridor => `
        <div class="corridor-label ${corridor.rotate ? "corridor-vertical" : ""}" style="${toBlueprintStyle(corridor, layout)}">
            ${escapeHtml(t("corridor"))}
        </div>
    `).join("");

    return `
        <article class="sector-card">
            <div class="sector-head">
                <div>
                    <h3>${escapeHtml(sector.name)}</h3>
                    <p>${escapeHtml(buildSectorSubtitle(sector.summary || {}))}</p>
                </div>
                <div class="sector-stat-grid">
                    <span class="sector-stat"><strong>${sector.summary?.plannedCount ?? 0}</strong><small>${escapeHtml(t("sectorPlanned"))}</small></span>
                    <span class="sector-stat sector-stat-confirmed"><strong>${sector.summary?.confirmedCount ?? 0}</strong><small>${escapeHtml(t("sectorConfirmed"))}</small></span>
                    <span class="sector-stat sector-stat-missing"><strong>${sector.summary?.missingCount ?? 0}</strong><small>${escapeHtml(t("sectorMissing"))}</small></span>
                    <span class="sector-stat sector-stat-extra"><strong>${sector.summary?.extraCount ?? 0}</strong><small>${escapeHtml(t("sectorExtra"))}</small></span>
                </div>
            </div>

            <div class="sector-blueprint-stage">
                <div class="sector-blueprint" style="aspect-ratio:${layout.width} / ${layout.height}">
                    ${corridors}
                    ${blueprint}
                </div>
            </div>
        </article>
    `;
}

function renderFallbackSectorCard(sector) {
    const items = (sector.locals || []).map(local => `
        <div class="fallback-local-card ${getLocalStatusClass(local)}">
            <div class="local-slot-head">
                <span class="local-slot-name">${escapeHtml(local.localName || resolveLocalName(local.localId))}</span>
                <span class="local-slot-state">${escapeHtml(getLocalStateLabel(local))}</span>
            </div>
            <div class="local-slot-metrics">
                <span>P ${local.plannedCount ?? 0}</span>
                <span>C ${local.confirmedCount ?? 0}</span>
            </div>
            <div class="local-people">
                ${renderPeople(local)}
            </div>
        </div>
    `).join("");

    return `
        <article class="sector-card">
            <div class="sector-head">
                <div>
                    <h3>${escapeHtml(sector.name)}</h3>
                    <p>${escapeHtml(buildSectorSubtitle(sector.summary || {}))}</p>
                </div>
                <div class="sector-stat-grid">
                    <span class="sector-stat"><strong>${sector.summary?.plannedCount ?? 0}</strong><small>${escapeHtml(t("sectorPlanned"))}</small></span>
                    <span class="sector-stat sector-stat-confirmed"><strong>${sector.summary?.confirmedCount ?? 0}</strong><small>${escapeHtml(t("sectorConfirmed"))}</small></span>
                    <span class="sector-stat sector-stat-missing"><strong>${sector.summary?.missingCount ?? 0}</strong><small>${escapeHtml(t("sectorMissing"))}</small></span>
                    <span class="sector-stat sector-stat-extra"><strong>${sector.summary?.extraCount ?? 0}</strong><small>${escapeHtml(t("sectorExtra"))}</small></span>
                </div>
            </div>
            <div class="fallback-sector-grid">
                ${items || `<div class="chip chip-empty">${escapeHtml(t("emptyLocal"))}</div>`}
            </div>
        </article>
    `;
}

function buildSectorSubtitle(summary) {
    return `${t("planned")}: ${summary.plannedCount ?? 0} | ${t("confirmed")}: ${summary.confirmedCount ?? 0} | ${t("missing")}: ${summary.missingCount ?? 0}`;
}

function renderPeople(localState) {
    const people = localState.people || [];

    if (people.length === 0) {
        return `<span class="chip chip-empty">${escapeHtml(t("emptyLocal"))}</span>`;
    }

    return people.map(person => `
        <span class="chip chip-${person.status}" title="${escapeHtml(person.display)}">
            ${escapeHtml(person.shortName || person.display)}
        </span>
    `).join("");
}

function getLocalStatusClass(localState) {
    const planned = Number(localState.plannedCount || 0);
    const confirmed = Number(localState.confirmedCount || 0);
    const extra = Number(localState.extraCount || 0);

    if (planned === 0 && confirmed === 0 && extra === 0) return "local-slot-idle";
    if (planned > 0 && confirmed === planned && extra === 0) return "local-slot-confirmed";
    if (planned > 0 && confirmed === 0 && extra === 0) return "local-slot-planned";
    if (planned === 0 && extra > 0) return "local-slot-extra";
    return "local-slot-mixed";
}

function getLocalStateLabel(localState) {
    const planned = Number(localState.plannedCount || 0);
    const confirmed = Number(localState.confirmedCount || 0);
    const extra = Number(localState.extraCount || 0);

    if (planned > 0 && confirmed === planned && extra === 0) return t("confirmedState");
    if (planned > 0 && confirmed === 0 && extra === 0) return t("plannedState");
    if (planned === 0 && extra > 0) return t("extraState");
    if (planned > 0 || confirmed > 0 || extra > 0) return t("mixed");
    return "-";
}

function createEmptyLocal(localId, localName) {
    return {
        localId,
        localName: localName || resolveLocalName(localId),
        plannedCount: 0,
        confirmedCount: 0,
        missingCount: 0,
        extraCount: 0,
        people: [],
        tooltip: localName || resolveLocalName(localId)
    };
}

function resolveLocalName(localId, fallbackLabel = "") {
    const local = state.localsCatalog.find(item => Number(item.id) === Number(localId));
    if (local) {
        if (state.locale === "ja-JP" && local.nameJp) {
            return local.nameJp;
        }

        return local.namePt || local.nameJp || fallbackLabel || `${t("local")} ${localId}`;
    }

    return fallbackLabel || `${t("local")} ${localId}`;
}

function toBlueprintStyle(item, layout) {
    return [
        `left:${(item.x / layout.width) * 100}%`,
        `top:${(item.y / layout.height) * 100}%`,
        `width:${(item.w / layout.width) * 100}%`,
        `height:${(item.h / layout.height) * 100}%`
    ].join(";");
}

function applyLocale() {
    document.documentElement.lang = state.locale;
    document.title = t("title");

    setText("txtHeaderTitle", t("title"));
    setText("txtHeaderSubtitle", t("subtitle"));
    setText("txtMetaDate", t("metaDate"));
    setText("txtMetaShift", t("metaShift"));
    setText("txtMetaImport", t("metaImport"));
    setText("toolbarBadge", t("badge"));
    setText("txtToolbarTitle", t("toolbarTitle"));
    setText("txtToolbarSubtitle", t("toolbarSubtitle"));
    setText("txtDateLabel", t("date"));
    setText("txtShiftLabel", t("shift"));
    setText("btnRefresh", t("refresh"));
    setText("btnImportSchedule", t("import"));
    setText("txtSummaryPlanned", t("planned"));
    setText("txtSummaryConfirmed", t("confirmed"));
    setText("txtSummaryMissing", t("missing"));
    setText("txtSummaryExtra", t("extra"));
    setText("txtLoading", t("importLoading"));

    const shiftPicker = document.getElementById("shiftPicker");
    const selected = shiftPicker.value;

    shiftPicker.innerHTML = state.shifts
        .map(shift => `<option value="${shift.id}">${escapeHtml(getShiftName(shift))}</option>`)
        .join("");

    if (selected) {
        shiftPicker.value = selected;
    }

    if (state.board) {
        hydrateBoard(state.board);
    }
}

function getShiftName(shift) {
    return state.locale === "ja-JP"
        ? (shift.nameJp || shift.namePt || `#${shift.id}`)
        : (shift.namePt || shift.nameJp || `#${shift.id}`);
}

function resolveShiftName(shiftId) {
    const shift = state.shifts.find(item => Number(item.id) === Number(shiftId));
    return shift ? getShiftName(shift) : "-";
}

function formatDateLabel(dateIso) {
    if (!dateIso) return "-";
    const date = new Date(`${dateIso}T00:00:00`);
    if (Number.isNaN(date.getTime())) return dateIso;

    return new Intl.DateTimeFormat(state.locale, {
        year: "numeric",
        month: "2-digit",
        day: "2-digit"
    }).format(date);
}

function showLoading() {
    document.getElementById("loadingOverlay").classList.remove("hidden");
    document.getElementById("btnImportSchedule").disabled = true;
}

function hideLoading() {
    document.getElementById("loadingOverlay").classList.add("hidden");
    document.getElementById("btnImportSchedule").disabled = false;
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
    if (element) element.textContent = value ?? "-";
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;")
        .replaceAll("'", "&#39;");
}
