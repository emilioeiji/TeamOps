const txtFJ = document.getElementById("txtFJ");
const txtNome = document.getElementById("txtNome");
const cboLocal = document.getElementById("cboLocal");
const dtInicial = document.getElementById("dtInicial");
const dtFinal = document.getElementById("dtFinal");
const btnFiltrar = document.getElementById("btnFiltrar");
const tblBody = document.querySelector("#tblHikitsugui tbody");
const statusBadge = document.getElementById("statusBadge");

const modal = document.getElementById("modal");
const btnCloseModal = document.getElementById("btnCloseModal");
const modalBody = document.getElementById("modalBody");
const attachmentList = document.getElementById("attachmentList");

let fjTimer = null;
let currentFJ = "";
let currentRows = [];

window.addEventListener("DOMContentLoaded", () => {
    applyDefaultDates();
    bindEvents();
});

function bindEvents() {
    txtFJ.addEventListener("input", handleFjInput);
    btnFiltrar.addEventListener("click", runFilter);
    cboLocal.addEventListener("change", handleLocalChange);
    btnCloseModal.addEventListener("click", closeModal);

    document.addEventListener("click", event => {
        if (event.target === modal) {
            closeModal();
        }
    });
}

function applyDefaultDates() {
    const today = new Date();
    const start = new Date(today);
    start.setDate(today.getDate() - 31);

    dtFinal.value = formatDate(today);
    dtInicial.value = formatDate(start);
}

function handleFjInput() {
    txtFJ.value = txtFJ.value.toUpperCase();
    clearTimeout(fjTimer);

    fjTimer = setTimeout(() => {
        const fj = txtFJ.value.trim();

        if (fj.length < 4) {
            resetOperatorState(true);
            setStatus("Informe ao menos 4 caracteres do codigo FJ.");
            return;
        }

        setStatus("Buscando operador...");
        send("load_operator", { fj });
    }, 220);
}

function handleLocalChange() {
    if (!currentFJ) return;

    const localId = Number(cboLocal.value || 0);
    if (localId <= 0) return;

    send("register_presence", {
        fj: currentFJ,
        localId
    });

    setStatus("Presenca confirmada. Carregando hikitsugui...");
    runFilter();
}

function runFilter() {
    if (!currentFJ) {
        setStatus("Informe um operador valido antes de filtrar.");
        return;
    }

    const localId = Number(cboLocal.value || 0);
    if (localId <= 0) {
        setStatus("Selecione a area para continuar.");
        return;
    }

    setStatus("Carregando hikitsugui...");

    send("filter", {
        fj: currentFJ,
        dtInicial: dtInicial.value,
        dtFinal: dtFinal.value,
        localId
    });
}

window.chrome.webview.addEventListener("message", event => {
    const msg = event.data;

    switch (msg.type) {
        case "operator_not_found":
            resetOperatorState(true);
            setStatus("Operador nao encontrado.");
            break;

        case "operator_loaded":
            hydrateOperator(msg.data);
            break;

        case "hikitsugui_list":
            currentRows = msg.data || [];
            renderTable(currentRows);
            setStatus(`${currentRows.length} hikitsugui encontrado(s).`);
            break;

        case "hikitsugui_preview":
            renderPreview(msg.data);
            break;

        case "attachments":
            renderAttachments(msg.data || []);
            break;

        case "read_status":
            updateReadStatus(msg.data.id, msg.data.lido);
            break;
    }
});

function hydrateOperator(data) {
    currentFJ = data.CodigoFJ;
    txtNome.value = data.NameRomanji || "";
    cboLocal.innerHTML = `<option value="">Selecione...</option>`;

    (data.locals || []).forEach(local => {
        const option = document.createElement("option");
        option.value = local.Id;
        option.textContent = local.NamePt;
        cboLocal.appendChild(option);
    });

    if ((data.locals || []).length === 1) {
        cboLocal.value = String(data.locals[0].Id);
        handleLocalChange();
        return;
    }

    setStatus("Operador carregado. Selecione a area para confirmar a presenca.");
}

function renderTable(list) {
    tblBody.innerHTML = "";

    if (!list || list.length === 0) {
        tblBody.innerHTML = `
            <tr>
                <td colspan="7" class="empty-cell">Nenhum hikitsugui encontrado para o filtro atual.</td>
            </tr>
        `;
        return;
    }

    const fragment = document.createDocumentFragment();

    list.forEach(item => {
        const row = document.createElement("tr");
        const readSymbol = item.IsRead ? "○" : "×";
        const readClass = item.IsRead ? "status-read" : "status-unread";

        row.innerHTML = `
            <td id="read-${item.Id}" class="${readClass}">${readSymbol}</td>
            <td>${item.Id}</td>
            <td>${formatServerDate(item.Date)}</td>
            <td>${escapeHtml(item.CategoryName)}</td>
            <td>${escapeHtml(item.CreatorCodigoFJ)}</td>
            <td class="description-cell">${escapeHtml(truncate(stripHtml(item.Description), 90))}</td>
            <td class="actions-cell">
                <button class="icon-btn icon-btn-view" type="button" data-preview-id="${item.Id}" title="Abrir" aria-label="Abrir hikitsugui ${item.Id}">
                    <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                        <path d="M2.25 12s3.75-6 9.75-6 9.75 6 9.75 6-3.75 6-9.75 6S2.25 12 2.25 12Z"></path>
                        <path d="M12 14.75A2.75 2.75 0 1 0 12 9.25a2.75 2.75 0 0 0 0 5.5Z"></path>
                    </svg>
                </button>
            </td>
        `;

        row.querySelector("[data-preview-id]").addEventListener("click", () => openPreview(item.Id));
        fragment.appendChild(row);
    });

    tblBody.appendChild(fragment);
}

function openPreview(id) {
    send("preview", { id, fj: currentFJ });
}

function renderPreview(item) {
    modalBody.innerHTML = `
        <div class="preview-grid">
            <div><span class="preview-label">ID</span><strong>${item.Id}</strong></div>
            <div><span class="preview-label">Data</span><strong>${formatServerDate(item.Date)}</strong></div>
            <div><span class="preview-label">Criador</span><strong>${escapeHtml(item.CreatorCodigoFJ || "")}</strong></div>
            <div><span class="preview-label">Categoria</span><strong>${escapeHtml(item.CategoryName || "")}</strong></div>
        </div>
        <div class="preview-description">${item.Description || ""}</div>
    `;

    attachmentList.innerHTML = `<div class="attachment-empty">Carregando anexos...</div>`;
    modal.classList.remove("hidden");
}

function renderAttachments(list) {
    attachmentList.innerHTML = "";

    if (!list || list.length === 0) {
        attachmentList.innerHTML = `<div class="attachment-empty">Nenhum anexo.</div>`;
        return;
    }

    const fragment = document.createDocumentFragment();

    list.forEach(item => {
        const row = document.createElement("div");
        row.className = "attach-item";
        row.innerHTML = `
            <div class="attach-name">${escapeHtml(item.FileName)}</div>
            <button class="btn-attach" type="button">Abrir</button>
        `;

        row.querySelector("button").addEventListener("click", () => {
            send("open_attachment", { path: item.FilePath });
        });

        fragment.appendChild(row);
    });

    attachmentList.appendChild(fragment);
}

function updateReadStatus(id, isRead) {
    const cell = document.getElementById(`read-${id}`);
    if (!cell) return;

    cell.textContent = isRead ? "○" : "×";
    cell.className = isRead ? "status-read" : "status-unread";

    const row = currentRows.find(item => Number(item.Id) === Number(id));
    if (row) {
        row.IsRead = isRead;
    }
}

function closeModal() {
    modal.classList.add("hidden");
}

function resetOperatorState(clearFj) {
    if (clearFj) {
        currentFJ = "";
    }

    txtNome.value = "";
    cboLocal.innerHTML = `<option value="">Selecione...</option>`;
    currentRows = [];
    renderTable([]);
}

function setStatus(text) {
    statusBadge.textContent = text;
}

function send(action, data = {}) {
    window.chrome.webview.postMessage({ action, ...data });
}

function truncate(text, max) {
    if (!text) return "";
    return text.length > max ? `${text.substring(0, max)}...` : text;
}

function stripHtml(html) {
    const div = document.createElement("div");
    div.innerHTML = html || "";
    return div.textContent || div.innerText || "";
}

function escapeHtml(value) {
    return String(value ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
}

function formatDate(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
}

function formatServerDate(value) {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
        return value ?? "";
    }

    return date.toLocaleDateString("pt-BR");
}
