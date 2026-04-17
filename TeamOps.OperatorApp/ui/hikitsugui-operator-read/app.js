const txtFJ = document.getElementById("txtFJ");
const txtNome = document.getElementById("txtNome");
const cboLocal = document.getElementById("cboLocal");
const dtInicial = document.getElementById("dtInicial");
const dtFinal = document.getElementById("dtFinal");
const btnFiltrar = document.getElementById("btnFiltrar");
const tblBody = document.querySelector("#tblHikitsugui tbody");

const modal = document.getElementById("modal");
const btnCloseModal = document.getElementById("btnCloseModal");
const modalBody = document.getElementById("modalBody");
const attachmentList = document.getElementById("attachmentList");

let fjTimer = null;
let currentFJ = null;

// ===============================
// Inicializar datas automaticamente
// ===============================
window.addEventListener("DOMContentLoaded", () => {
    const hoje = new Date();
    const mesAtras = new Date();
    mesAtras.setDate(hoje.getDate() - 31);

    dtFinal.value = hoje.toISOString().split("T")[0];
    dtInicial.value = mesAtras.toISOString().split("T")[0];
});

// ===============================
// Debounce FJ
// ===============================
txtFJ.addEventListener("input", () => {
    txtFJ.value = txtFJ.value.toUpperCase();
    clearTimeout(fjTimer);

    fjTimer = setTimeout(() => {
        const fj = txtFJ.value.trim();
        if (fj.length >= 4) {
            send("load_operator", { fj });
        }
    }, 300);
});

// ===============================
// Filtrar
// ===============================
btnFiltrar.addEventListener("click", () => {
    if (!currentFJ) return;

    const localId = cboLocal.value === "" ? -1 : parseInt(cboLocal.value);

    if (localId === -1) return;

    send("filter", {
        fj: currentFJ,
        dtInicial: dtInicial.value,
        dtFinal: dtFinal.value,
        localId
    });
});

cboLocal.addEventListener("change", () => {
    if (!currentFJ) return;

    const localId = cboLocal.value === "" ? -1 : parseInt(cboLocal.value);
    if (localId === -1) return;

    send("register_presence", {
        fj: currentFJ,
        localId
    });

    send("filter", {
        fj: currentFJ,
        dtInicial: dtInicial.value,
        dtFinal: dtFinal.value,
        localId
    });
});

// ===============================
// Modal
// ===============================
btnCloseModal.addEventListener("click", () => {
    modal.classList.add("hidden");
});

// ===============================
// Receber mensagens do C#
// ===============================
window.chrome.webview.addEventListener("message", e => {
    const msg = e.data;

    switch (msg.type) {

        case "operator_not_found":
            txtNome.value = "";
            cboLocal.innerHTML = "";
            currentFJ = null;
            break;

        case "operator_loaded":
            currentFJ = msg.data.CodigoFJ;
            txtNome.value = msg.data.NameRomanji;

            cboLocal.innerHTML = "";

            const optDefault = document.createElement("option");
            optDefault.value = "";
            optDefault.textContent = "Selecione...";
            cboLocal.appendChild(optDefault);
            
            msg.data.locals.forEach(l => {
                const opt = document.createElement("option");
                opt.value = l.Id;
                opt.textContent = l.NamePt;
                cboLocal.appendChild(opt);
            });
            
            // garante que não dispara change automático
            cboLocal.selectedIndex = 0;

            break;

        case "hikitsugui_list":
            renderTable(msg.data);
            break;

        case "hikitsugui_preview":
            renderPreview(msg.data);
            break;

        case "attachments":
            renderAttachments(msg.data);
            break;

        case "read_status":
            updateReadStatus(msg.data.id, msg.data.lido);
            break;

    }
});

// ===============================
// Renderizar tabela
// ===============================
function renderTable(lista) {
    tblBody.innerHTML = "";

    lista.forEach(h => {
        const tr = document.createElement("tr");

        const lido = h.IsRead ? "〇" : "×";

        tr.innerHTML = `
            <td id="read-${h.Id}">…</td>
            <td>${h.Id}</td>
            <td>${h.Date}</td>
            <td>${h.CategoryName}</td>
            <td>${h.CreatorCodigoFJ}</td>
            <td>${truncate(h.Description, 80)}</td>
            <td><button class="btn-view" onclick="openPreview(${h.Id})">Ver</button></td>
        `;

        tblBody.appendChild(tr);
    });

    lista.forEach(h => {
        send("has_read", { id: h.Id, fj: currentFJ });
    });

}

function truncate(text, max) {
    if (!text) return "";
    return text.length > max ? text.substring(0, max) + "..." : text;
}

// ===============================
// Preview
// ===============================
function openPreview(id) {
    send("preview", { id, fj: currentFJ });
    send("mark_read", { id, fj: currentFJ });
}

function renderPreview(h) {
    modalBody.innerHTML = `
        <p><b>ID:</b> ${h.Id}</p>
        <p><b>Data:</b> ${h.Date}</p>
        <p><b>Criador:</b> ${h.CreatorCodigoFJ}</p>
        <p><b>Categoria:</b> ${h.CategoryName}</p>
        <hr>
        <div>${h.Description}</div>
    `;

    modal.classList.remove("hidden");
}

// ===============================
// Anexos
// ===============================
function renderAttachments(list) {
    attachmentList.innerHTML = "";

    list.forEach(a => {
        const btn = document.createElement("button");
        btn.textContent = a.FileName;
        btn.onclick = () => send("open_attachment", { path: a.FilePath });
        attachmentList.appendChild(btn);
    });
}

// ===============================
// Enviar mensagem ao C#
// ===============================
function send(action, data) {
    chrome.webview.postMessage({ action, ...data });
}

function updateReadStatus(id, lido) {
    const cell = document.getElementById(`read-${id}`);
    if (!cell) return;

    if (lido) {
        cell.textContent = "〇";
        cell.style.color = "green";
    } else {
        cell.textContent = "×";
        cell.style.color = "red";
    }
}
