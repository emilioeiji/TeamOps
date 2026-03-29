// -------------------------------------------------------------
// ENVIO DE MENSAGENS PARA O BACKEND (C#)
// -------------------------------------------------------------
function openModule(module) {
    window.chrome.webview.postMessage(`open:${module}`);
}

// -------------------------------------------------------------
// RECEBIMENTO DE MENSAGENS DO BACKEND (C# → JS)
// -------------------------------------------------------------
window.chrome?.webview?.addEventListener("message", (event) => {
    const data = event.data;

    // Preenche nome do usuário no header
    if (data.user) {
        const lblUser = document.getElementById("lblUser");
        if (lblUser) lblUser.textContent = data.user;
    }

    // Preenche data no footer (caso venha do C#)
    if (data.date) {
        const lblDate = document.getElementById("lblDate");
        if (lblDate) lblDate.textContent = data.date;
    }

    // Controle de permissões (Admin, GL, KL etc.)
    if (data.accessLevel !== undefined) {
        applyPermissions(data.accessLevel);
    }

    if (data.operatorName)
    document.getElementById("lblOperator").innerText = " | Operador: " + data.operatorName;
});

// -------------------------------------------------------------
// PERMISSÕES (oculta botões conforme AccessLevel)
// -------------------------------------------------------------
function applyPermissions(level) {
    // AccessLevel enum no C#:
    // 0 = Operator
    // 1 = KL
    // 2 = GL
    // 3 = Admin

    // Admin-only
    if (level < 3) {
        hideButton("btnAdmin");
        hideButton("btnAccessControl");
        hideButton("btnAtribuir");
    }

    // GL-only
    if (level < 2) {
        hideButton("btnRelatorios");
    }

    // KL-only
    if (level < 1) {
        hideButton("btnFollowUp");
        hideButton("btnHikitsugui");
        hideButton("btnHikitsuguiLeaderRead");
        hideButton("btnPR");
        hideButton("btnCL");
        hideButton("btnSobraDePeca");
    }
}

function hideButton(id) {
    const el = document.getElementById(id);
    if (el) el.style.display = "none";
}

// -------------------------------------------------------------
// INICIALIZAÇÃO DO FRONTEND
// -------------------------------------------------------------
document.addEventListener("DOMContentLoaded", () => {
    // Preenche data local caso o C# não envie
    const lblDate = document.getElementById("lblDate");
    if (lblDate) {
        lblDate.textContent = new Date().toLocaleString("pt-BR");
    }
});
