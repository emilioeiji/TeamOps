// Garantir que o modal comece escondido
window.addEventListener("load", () => {
    const modal = document.getElementById("modalAdd");
    modal.classList.add("hidden");
});

let operators = [];

// Tooltip
const tooltip = document.getElementById("tooltip");
const tooltipContent = document.getElementById("tooltipContent");

document.addEventListener("mousemove", (e) => {
    tooltip.style.left = (e.pageX + 12) + "px";
    tooltip.style.top = (e.pageY + 12) + "px";
});

// Filtro por turno
document.querySelectorAll("input[name='shiftFilter']").forEach(r => {
    r.addEventListener("change", () => {
        const shiftId = parseInt(r.value);

        window.chrome.webview.postMessage({
            action: "filter_shift",
            shiftId
        });
    });
});

document.addEventListener("mouseover", (e) => {
    const cell = e.target.closest(".motivo-cell");
    if (!cell) return;

    const text = cell.dataset.notes || "";
    showNotesTooltip(text);
});

document.addEventListener("mouseout", (e) => {
    if (!e.target.closest(".motivo-cell")) return;
    if (e.relatedTarget && e.relatedTarget.closest(".motivo-cell")) return;
    hideTooltip();
});

// Carregar dados ao abrir
window.onload = () => {
    window.chrome.webview.postMessage({
        action: "paidleave_load"
    });
};

// Receber mensagens do backend
window.chrome.webview.addEventListener('message', event => {
    const msg = event.data;

    if (msg.type === "select_all") {
        renderTable(msg.data);
        return;
    }

    if (msg.type === "select_all_by_shift") {
        renderTable(msg.data);
        return;
    }

    if (msg.type === "get_todoke_info") {
        showTooltip(msg.data);
        return;
    }

    if (msg.type === "get_folha_info") {
        showTooltip(msg.data);
        return;
    }

    if (msg.type === "get_conferencia_info") {
        showTooltip(msg.data);
        return;
    }

    if (msg.type === "select_operators") {
        operators = msg.data;
        return;
    }

    if (msg.type === "select_motivos") {
        const sel = document.getElementById("motivoId");
        sel.innerHTML = '<option value="">Selecione...</option>';
    
        msg.data.forEach(m => {
            sel.innerHTML += `<option value="${m.Id}">${m.NomePt}</option>`;
        });
    }
});

// Renderizar tabela
function renderTable(items) {
    if (!items || items.length === 0) {
        document.getElementById("tableContainer").innerHTML =
            "<p>Nenhum registro encontrado.</p>";
        return;
    }

    let html = `
        <table class="min-w-full text-sm border-collapse">
            <thead class="bg-gray-200">
                <tr>
                    <th class="p-2 border">Operador</th>
                    <th class="p-2 border">Data Solicitação</th>
                    <th class="p-2 border">Lancado por</th>
                    <th class="p-2 border">Motivo<br>届出理由</th>
                    <th class="p-2 border">Todoke</th>
                    <th class="p-2 border">Folha Yukyu</th>
                    <th class="p-2 border text-center">Conferência<br>確認</th>
                </tr>
            </thead>
            <tbody>
    `;

    for (const item of items) {
        html += `
            <tr class="border-b hover:bg-gray-50">
                <td class="p-2 border">${item.operatorName}</td>
                <td class="p-2 border text-center">${item.requestDate}</td>
                <td class="p-2 border text-center">${item.authorizedBy}</td>

                <!-- MOTIVO -->
                <!-- MOTIVO -->
                <td class="p-2 border text-center motivo-cell"
                    data-notes="${(item.notes || '').replace(/"/g, '&quot;')}">
                    ${item.todokeMotivoName ?? ""}
                </td>

                <!-- TODOKE -->
                <td class="p-2 border text-center">
                    <span class="cursor-pointer"
                          onclick="toggleTodoke(${item.id})"
                          onmouseenter="requestTodokeInfo(${item.id})"
                          onmouseleave="hideTooltip()">
                        ${item.hasTodoke
                            ? "<span class='maru'>○</span>"
                            : "<span class='batsu'>×</span>"}
                    </span>
                </td>

                <!-- FOLHA CONTROLE -->
                <td class="p-2 border text-center">
                    ${
                        item.todokeMotivoName === "Yukyu"
                        ? `
                            <span class="cursor-pointer"
                                  onclick="toggleFolha(${item.id})"
                                  onmouseenter="requestFolhaInfo(${item.id})"
                                  onmouseleave="hideTooltip()">
                                ${item.hasFolha
                                    ? "<span class='maru'>○</span>"
                                    : "<span class='batsu'>×</span>"}
                            </span>
                        `
                        : `
                            <span class="text-gray-300 cursor-not-allowed">—</span>
                        `
                    }
                </td>

                <!-- CONFERÊNCIA -->
                <td class="p-2 border text-center">
                    <span class="cursor-pointer"
                          onclick="toggleConferencia(${item.id})"
                          onmouseenter="requestConferenciaInfo(${item.id})"
                          onmouseleave="hideTooltip()">
                        ${item.hasConferencia
                            ? "<span class='maru'>○</span>"
                            : "<span class='batsu'>×</span>"}
                    </span>
                </td>
            </tr>
        `;
    }

    html += `
            </tbody>
        </table>
    `;

    document.getElementById("tableContainer").innerHTML = html;
}

// Requests de tooltip
function requestTodokeInfo(id) {
    window.chrome.webview.postMessage({
        action: "get_todoke_info",
        id
    });
}

function requestFolhaInfo(id) {
    window.chrome.webview.postMessage({
        action: "get_folha_info",
        id
    });
}

function requestConferenciaInfo(id) {
    window.chrome.webview.postMessage({
        action: "get_conferencia_info",
        id
    });
}

// Toggles
function toggleTodoke(id) {
    if (!confirm("Registrar Todoke para este operador?")) return;

    window.chrome.webview.postMessage({
        action: "toggle_todoke",
        id
    });
}

function toggleFolha(id) {
    if (!confirm("Registrar Folha de Controle para este operador?")) return;

    window.chrome.webview.postMessage({
        action: "toggle_folha",
        id
    });
}

function toggleConferencia(id) {
    if (!confirm("Registrar Conferência para este operador?")) return;

    window.chrome.webview.postMessage({
        action: "toggle_conferencia",
        id
    });
}

// Tooltip
function showTooltip(info) {
    if (!info || info.length === 0) {
        tooltipContent.innerHTML = "Nenhum registro";
    } else {
        const row = info[0];
        tooltipContent.innerHTML = `
            <div><strong>${row.TakenByName}</strong></div>
            <div>${row.TakenAt}</div>
        `;
    }

    tooltip.classList.remove("hidden");
    tooltip.classList.add("show");
}

function showNotesTooltip(text) {
    tooltipContent.innerHTML = text && text.trim() !== "" 
        ? text 
        : "Sem notas";

    tooltip.classList.remove("hidden");
    tooltip.classList.add("show");
}

function hideTooltip() {
    tooltip.classList.remove("show");
    setTimeout(() => tooltip.classList.add("hidden"), 150);
}

// Modal
document.getElementById("btnAdd").onclick = () => {
    document.getElementById("modalAdd").classList.remove("hidden");
};

function closeModal() {
    document.getElementById("modalAdd").classList.add("hidden");
}

function clearForm() {
    document.getElementById("opSearch").value = "";
    document.getElementById("opCodigoFJ").value = "";
    document.getElementById("reqDate").value = "";
    document.getElementById("notes").value = "";

    const sug = document.getElementById("opSuggestions");
    if (sug) sug.classList.add("hidden");
}

// Salvar request
function saveRequest() {
    const opCodigoFJ = document.getElementById("opCodigoFJ").value;
    const reqDate = document.getElementById("reqDate").value;
    const notes = document.getElementById("notes").value;
    const motivoIdStr = document.getElementById("motivoId").value;

    if (!opCodigoFJ || !reqDate) {
        alert("Operator and Request Date are required.");
        return;
    }

    if (!motivoIdStr) {
        alert("Selecione um motivo.");
        return;
    }

    const motivoId = parseInt(motivoIdStr, 10);

    window.chrome.webview.postMessage({
        action: "add_request",
        opCodigoFJ,
        reqDate,
        notes,
        motivoId
    });

    closeModal();
    clearForm();
}

// Autocomplete
const opSearch = document.getElementById("opSearch");
const opCodigoFJ = document.getElementById("opCodigoFJ");
const opSuggestions = document.getElementById("opSuggestions");

opSearch.addEventListener("input", () => {
    const text = opSearch.value.toLowerCase();

    if (!text) {
        opSuggestions.classList.add("hidden");
        return;
    }

    const filtered = operators.filter(op =>
        op.NameRomanji.toLowerCase().includes(text) ||
        op.NameNihongo.includes(text) ||
        op.CodigoFJ.toLowerCase().includes(text)
    );

    if (filtered.length === 0) {
        opSuggestions.classList.add("hidden");
        return;
    }

    let html = "";
    for (const op of filtered) {
        html += `
            <div class="suggestion-item"
                 onclick="selectOperator('${op.CodigoFJ}', '${op.NameRomanji}')">
                ${op.NameRomanji} (${op.CodigoFJ})
            </div>
        `;
    }

    opSuggestions.innerHTML = html;
    opSuggestions.classList.remove("hidden");
});

function selectOperator(codigoFJ, name) {
    opSearch.value = name + " (" + codigoFJ + ")";
    opCodigoFJ.value = codigoFJ;
    opSuggestions.classList.add("hidden");
}
