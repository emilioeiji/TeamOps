let currentRows = [];
let selectedRowId = 0;
let selectedOperatorCodigo = "";
let searchTimer = null;

document.addEventListener("DOMContentLoaded", () => {
    bind();
    post("load");
});

function bind() {
    document.getElementById("btnBuscar").addEventListener("click", applyFilters);
    document.getElementById("btnExportar").addEventListener("click", exportRows);
    document.getElementById("btnLimpar").addEventListener("click", resetFilters);
    document.getElementById("btnCloseModal").addEventListener("click", closeModal);
    document.getElementById("btnModalPrint").addEventListener("click", () => {
        if (selectedRowId > 0) {
            post("print", { id: selectedRowId });
        }
    });
    document.getElementById("btnModalOperator").addEventListener("click", () => {
        if (selectedOperatorCodigo) {
            post("operator_report", { codigoFJ: selectedOperatorCodigo });
        }
    });

    document.getElementById("txtSearch").addEventListener("input", () => {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(applyFilters, 220);
    });

    ["dtInicial", "dtFinal", "cmbShift", "cmbType", "cmbReason", "cmbSector"].forEach(id => {
        document.getElementById(id).addEventListener("change", applyFilters);
    });

    document.getElementById("modal").addEventListener("click", event => {
        if (event.target.id === "modal") {
            closeModal();
        }
    });
}

window.chrome?.webview?.addEventListener("message", event => {
    const payload = event.data;
    if (!payload?.type) return;

    if (payload.type === "init") {
        hydrateFilters(payload.data);
        return;
    }

    if (payload.type === "rows") {
        currentRows = payload.data?.rows || [];
        renderRows(currentRows);
        setText("totalCount", payload.data?.total ?? 0);
        return;
    }

    if (payload.type === "notify") {
        showToast(payload.data?.title || "Aviso", payload.data?.message || "");
        return;
    }

    if (payload.type === "error") {
        showToast("Erro", payload.message || "Falha ao carregar relatorio.");
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
    document.getElementById("txtSearch").value = data.defaults?.search || "";
}

function renderRows(rows) {
    const body = document.getElementById("tblBody");

    if (!rows.length) {
        body.innerHTML = `<tr><td colspan="9" class="empty-cell">Nenhum registro encontrado.</td></tr>`;
        return;
    }

    body.innerHTML = rows.map(row => `
        <tr>
            <td>${escapeHtml(row.date)}</td>
            <td>${escapeHtml(row.shiftName)}</td>
            <td>${escapeHtml(row.operatorName)}</td>
            <td>${escapeHtml(row.typeName)}</td>
            <td>${escapeHtml(row.reasonName)}</td>
            <td>${escapeHtml(row.sectorName)}</td>
            <td class="text-cell">${escapeHtml(row.description)}</td>
            <td class="text-cell">${escapeHtml(row.guidance)}</td>
            <td class="actions-cell">
                <button class="icon-btn icon-btn-view" type="button" data-view="${row.id}" title="Visualizar" aria-label="Visualizar">
                    <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M2.25 12s3.75-6 9.75-6 9.75 6 9.75 6-3.75 6-9.75 6S2.25 12 2.25 12Z"/><path d="M12 14.75A2.75 2.75 0 1 0 12 9.25a2.75 2.75 0 0 0 0 5.5Z"/></svg>
                </button>
                <button class="icon-btn icon-btn-print" type="button" data-print="${row.id}" title="Imprimir" aria-label="Imprimir">
                    <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6 9V3.75h12V9"/><path d="M6.75 14.25h10.5V20.25H6.75z"/><path d="M5.25 9h13.5A2.25 2.25 0 0 1 21 11.25v4.5A2.25 2.25 0 0 1 18.75 18H17.25v-3.75H6.75V18H5.25A2.25 2.25 0 0 1 3 15.75v-4.5A2.25 2.25 0 0 1 5.25 9Z"/></svg>
                </button>
            </td>
        </tr>
    `).join("");

    body.querySelectorAll("[data-view]").forEach(button => {
        button.addEventListener("click", () => openModal(Number(button.dataset.view)));
    });

    body.querySelectorAll("[data-print]").forEach(button => {
        button.addEventListener("click", () => post("print", { id: Number(button.dataset.print) }));
    });
}

function openModal(id) {
    const row = currentRows.find(item => item.id === id);
    if (!row) return;

    selectedRowId = id;
    selectedOperatorCodigo = row.operatorCodigoFJ || "";
    document.getElementById("modalBody").innerHTML = `
        <div class="preview-grid">
            <div><span class="preview-label">Data</span><strong>${escapeHtml(row.date)}</strong></div>
            <div><span class="preview-label">Turno</span><strong>${escapeHtml(row.shiftName)}</strong></div>
            <div><span class="preview-label">Operador</span><strong>${escapeHtml(row.operatorName)}</strong></div>
            <div><span class="preview-label">Executor</span><strong>${escapeHtml(row.executorName)}</strong></div>
            <div><span class="preview-label">Testemunha</span><strong>${escapeHtml(row.witnessName)}</strong></div>
            <div><span class="preview-label">Motivo</span><strong>${escapeHtml(row.reasonName)}</strong></div>
            <div><span class="preview-label">Tipo</span><strong>${escapeHtml(row.typeName)}</strong></div>
            <div><span class="preview-label">Setor</span><strong>${escapeHtml(row.sectorName)}</strong></div>
            <div><span class="preview-label">Local</span><strong>${escapeHtml(row.localName)}</strong></div>
            <div><span class="preview-label">Equipamento</span><strong>${escapeHtml(row.equipmentName)}</strong></div>
        </div>
        <div class="text-block">
            <span class="preview-label">Descricao</span>
            <p>${escapeHtml(row.description)}</p>
        </div>
        <div class="text-block">
            <span class="preview-label">Orientacao</span>
            <p>${escapeHtml(row.guidance)}</p>
        </div>
    `;
    document.getElementById("modal").classList.remove("hidden");
}

function closeModal() {
    selectedRowId = 0;
    selectedOperatorCodigo = "";
    document.getElementById("modal").classList.add("hidden");
}

function applyFilters() {
    post("apply", collectFilterPayload());
}

function exportRows() {
    post("export", collectFilterPayload());
}

function resetFilters() {
    document.getElementById("cmbShift").value = "0";
    document.getElementById("cmbType").value = "0";
    document.getElementById("cmbReason").value = "0";
    document.getElementById("cmbSector").value = "0";
    document.getElementById("txtSearch").value = "";
    applyFilters();
}

function collectFilterPayload() {
    return {
        dtInicial: document.getElementById("dtInicial").value,
        dtFinal: document.getElementById("dtFinal").value,
        shiftId: Number(document.getElementById("cmbShift").value || 0),
        typeId: Number(document.getElementById("cmbType").value || 0),
        reasonId: Number(document.getElementById("cmbReason").value || 0),
        sectorId: Number(document.getElementById("cmbSector").value || 0),
        search: document.getElementById("txtSearch").value
    };
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

function showToast(title, message) {
    const root = document.getElementById("toast");
    document.getElementById("toastTitle").textContent = title;
    document.getElementById("toastMessage").textContent = message;
    root.classList.remove("hidden");
    clearTimeout(showToast._timer);
    showToast._timer = setTimeout(() => root.classList.add("hidden"), 3200);
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;");
}
