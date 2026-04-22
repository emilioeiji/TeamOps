let rows = [];
let currentShiftId = 0;

window.addEventListener("DOMContentLoaded", () => {
    bindEvents();
    send("load");
});

function bindEvents() {
    document.getElementById("btnSalvar").addEventListener("click", saveRecord);
    document.getElementById("btnCancelar").addEventListener("click", clearForm);
    document.getElementById("searchInput").addEventListener("input", applyFilters);
    document.getElementById("txtPeso").addEventListener("input", calculateQuantidade);
    document.getElementById("txtTanjuu").addEventListener("input", calculateQuantidade);
    document.getElementById("txtItem").addEventListener("input", event => {
        event.target.value = event.target.value.toUpperCase();
    });
}

function send(action, extra = {}) {
    window.chrome.webview.postMessage({
        action,
        ...extra
    });
}

window.chrome.webview.addEventListener("message", event => {
    const msg = event.data;

    switch (msg.type) {
        case "init":
            hydrateScreen(msg);
            break;

        case "rows":
            rows = msg.data || [];
            applyFilters();
            break;

        case "saved":
            alert(msg.message || "Registro salvo com sucesso.");
            clearForm();
            break;

        case "error":
            alert(msg.message || "Ocorreu um erro ao processar a solicitacao.");
            break;
    }
});

function hydrateScreen(payload) {
    currentShiftId = Number(payload.shiftId || 0);

    document.getElementById("txtDate").value = payload.today || "";
    document.getElementById("txtTurno").value = payload.shiftName || "";
    document.getElementById("txtLider").value = payload.leaderName || "";

    fillSelect("cmbOperador", payload.operators || [], "CodigoFJ", "NameRomanji");
    fillSelect("cmbMaquina", payload.machines || [], "Id", "NamePt");
    fillSelect("cmbShain", payload.shains || [], "Id", "NameRomanji");

    rows = payload.rows || [];
    applyFilters();
}

function fillSelect(id, items, valueField, textField) {
    const select = document.getElementById(id);
    select.innerHTML = `<option value="">Selecione...</option>`;

    items.forEach(item => {
        select.innerHTML += `<option value="${item[valueField]}">${escapeHtml(item[textField])}</option>`;
    });
}

function calculateQuantidade() {
    const peso = parseDecimal(document.getElementById("txtPeso").value);
    const tanjuu = parseDecimal(document.getElementById("txtTanjuu").value);
    const quantidadeInput = document.getElementById("txtQuantidade");

    if (peso > 0 && tanjuu > 0) {
        quantidadeInput.value = String(Math.round(peso / tanjuu));
        return;
    }

    quantidadeInput.value = "";
}

function saveRecord() {
    const lote = document.getElementById("txtLote").value.trim();
    const opCodigoFJ = document.getElementById("cmbOperador").value;
    const tanjuu = parseDecimal(document.getElementById("txtTanjuu").value);
    const pesoGramas = parseDecimal(document.getElementById("txtPeso").value);
    const quantidade = parseDecimal(document.getElementById("txtQuantidade").value);
    const machineId = Number(document.getElementById("cmbMaquina").value || 0);
    const shainId = Number(document.getElementById("cmbShain").value || 0);
    const item = document.getElementById("txtItem").value.trim().toUpperCase();

    if (!lote) {
        alert("Informe o lote.");
        return;
    }

    if (!opCodigoFJ) {
        alert("Selecione um operador.");
        return;
    }

    if (tanjuu <= 0) {
        alert("Informe um tanjuu valido.");
        return;
    }

    if (pesoGramas <= 0) {
        alert("Informe um peso valido.");
        return;
    }

    if (quantidade <= 0) {
        alert("Quantidade invalida.");
        return;
    }

    if (machineId <= 0) {
        alert("Selecione uma maquina.");
        return;
    }

    if (shainId <= 0) {
        alert("Selecione um shain.");
        return;
    }

    if (!item) {
        alert("Informe o item.");
        return;
    }

    send("save", {
        date: document.getElementById("txtDate").value,
        shiftId: currentShiftId,
        lote,
        opCodigoFJ,
        tanjuu,
        pesoGramas,
        quantidade,
        machineId,
        shainId,
        observacao: document.getElementById("txtObservacao").value.trim(),
        item
    });
}

function clearForm() {
    document.getElementById("txtDate").value = formatDate(new Date());
    document.getElementById("txtLote").value = "";
    document.getElementById("cmbOperador").value = "";
    document.getElementById("txtTanjuu").value = "";
    document.getElementById("txtPeso").value = "";
    document.getElementById("txtQuantidade").value = "";
    document.getElementById("cmbMaquina").value = "";
    document.getElementById("cmbShain").value = "";
    document.getElementById("txtObservacao").value = "";
    document.getElementById("txtItem").value = "";
}

function applyFilters() {
    const term = document.getElementById("searchInput").value.trim().toLowerCase();

    const filtered = rows.filter(item => {
        if (!term) return true;

        const haystack = [
            item.data,
            item.shiftName,
            item.lote,
            item.operatorName,
            item.machineName,
            item.shainName,
            item.item,
            item.lider,
            item.observacao,
            item.createdAt
        ]
            .filter(Boolean)
            .join(" ")
            .toLowerCase();

        return haystack.includes(term);
    });

    renderTable(filtered);
}

function renderTable(items) {
    const container = document.getElementById("tableContainer");

    if (!items || items.length === 0) {
        container.innerHTML = `<div class="empty-state">Nenhum registro encontrado.</div>`;
        return;
    }

    let html = `
        <table class="min-w-full sobra-table">
            <thead>
                <tr>
                    <th>ID</th>
                    <th>Data</th>
                    <th>Turno</th>
                    <th>Lote</th>
                    <th>Operador</th>
                    <th>Tanjuu</th>
                    <th>Peso (g)</th>
                    <th>Qtd</th>
                    <th>Maquina</th>
                    <th>Shain</th>
                    <th>Item</th>
                    <th>Lider</th>
                    <th>Criado em</th>
                    <th>Observacao</th>
                </tr>
            </thead>
            <tbody>
    `;

    items.forEach(item => {
        html += `
            <tr>
                <td>${escapeHtml(item.id)}</td>
                <td>${escapeHtml(item.data)}</td>
                <td>${escapeHtml(item.shiftName)}</td>
                <td>${escapeHtml(item.lote)}</td>
                <td>${escapeHtml(item.operatorName)}</td>
                <td class="text-right">${formatNumber(item.tanjuu)}</td>
                <td class="text-right">${formatNumber(item.pesoGramas)}</td>
                <td class="text-right">${formatNumber(item.quantidade)}</td>
                <td>${escapeHtml(item.machineName)}</td>
                <td>${escapeHtml(item.shainName)}</td>
                <td>${escapeHtml(item.item)}</td>
                <td>${escapeHtml(item.lider)}</td>
                <td>${escapeHtml(item.createdAt)}</td>
                <td class="notes-cell" title="${escapeHtmlAttr(item.observacao || "")}">
                    ${escapeHtml(item.observacao || "")}
                </td>
            </tr>
        `;
    });

    html += `
            </tbody>
        </table>
    `;

    container.innerHTML = html;
}

function parseDecimal(value) {
    if (!value) return 0;

    const raw = String(value).trim().replace(/\s+/g, "");
    const commaCount = (raw.match(/,/g) || []).length;
    const dotCount = (raw.match(/\./g) || []).length;

    let normalized = raw;

    if (commaCount > 0 && dotCount > 0) {
        normalized = raw.replace(/\./g, "").replace(",", ".");
    } else if (commaCount > 0) {
        normalized = raw.replace(",", ".");
    }

    const parsed = Number(normalized);
    return Number.isFinite(parsed) ? parsed : 0;
}

function formatDate(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
}

function formatNumber(value) {
    const numeric = Number(value || 0);

    if (!Number.isFinite(numeric)) {
        return "";
    }

    return numeric.toLocaleString("pt-BR", {
        maximumFractionDigits: 2
    });
}

function escapeHtml(value) {
    return String(value ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
}

function escapeHtmlAttr(value) {
    return escapeHtml(value).replace(/`/g, "&#96;");
}
