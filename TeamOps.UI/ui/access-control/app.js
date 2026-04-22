let rows = [];
let accessLevels = [];
let currentModalMode = "create";
let editingLogin = "";
let passwordTargetLogin = "";

window.addEventListener("DOMContentLoaded", () => {
    bindEvents();
    send("load");
});

function bindEvents() {
    document.getElementById("searchInput").addEventListener("input", applyFilters);
    document.getElementById("btnHeaderNovo").addEventListener("click", openCreateModal);

    document.getElementById("btnCloseUserModal").addEventListener("click", closeUserModal);
    document.getElementById("btnCancelUserModal").addEventListener("click", closeUserModal);
    document.getElementById("userModalBackdrop").addEventListener("click", closeUserModal);
    document.getElementById("btnSaveUserModal").addEventListener("click", submitUserModal);

    document.getElementById("btnClosePasswordModal").addEventListener("click", closePasswordModal);
    document.getElementById("btnCancelPasswordModal").addEventListener("click", closePasswordModal);
    document.getElementById("passwordModalBackdrop").addEventListener("click", closePasswordModal);
    document.getElementById("btnSavePasswordModal").addEventListener("click", submitPasswordReset);
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
            rows = normalizeRows(msg.data || []);
            renderMetrics(rows);
            applyFilters();
            syncEditingUser();
            break;

        case "created":
            alert(msg.message || "Usuario cadastrado com sucesso.");
            closeUserModal();
            break;

        case "updated":
            alert(msg.message || "Usuario atualizado com sucesso.");
            closeUserModal();
            break;

        case "password_reset":
            alert(msg.message || "Senha redefinida com sucesso.");
            closePasswordModal();
            break;

        case "error":
            alert(msg.message || "Ocorreu um erro ao processar a solicitacao.");
            break;
    }
});

function hydrateScreen(payload) {
    accessLevels = payload.accessLevels || [];
    rows = normalizeRows(payload.rows || []);

    fillSelect("cmbAccessLevel", accessLevels, "value", "label");
    renderMetrics(rows);
    resetUserForm();
    applyFilters();
}

function normalizeRows(items) {
    return items.map(item => ({
        ...item,
        accessLevel: Number(item.accessLevel || 0),
        accessLabel: item.accessLabel || getAccessLabel(item.accessLevel),
        codigoFJ: item.codigoFJ || "",
        name: item.name || ""
    }));
}

function fillSelect(id, items, valueField, textField) {
    const select = document.getElementById(id);
    select.innerHTML = `<option value="">Selecione...</option>`;

    items.forEach(item => {
        select.innerHTML += `<option value="${item[valueField]}">${escapeHtml(item[textField])}</option>`;
    });
}

function renderMetrics(items) {
    document.getElementById("metricTotal").textContent = String(items.length);
    document.getElementById("metricAdmins").textContent = String(items.filter(item => item.accessLevel === 5).length);
    document.getElementById("metricManagers").textContent = String(items.filter(item => item.accessLevel >= 4).length);
}

function applyFilters() {
    const term = document.getElementById("searchInput").value.trim().toLowerCase();

    const filtered = rows.filter(item => {
        if (!term) return true;

        const haystack = [
            item.login,
            item.name,
            item.codigoFJ,
            item.accessLabel
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

    if (!items.length) {
        container.innerHTML = `<div class="empty-state">Nenhum usuario encontrado.</div>`;
        return;
    }

    let html = `
        <table class="users-table">
            <thead>
                <tr>
                    <th>Login</th>
                    <th>Nome</th>
                    <th>Codigo FJ</th>
                    <th>Nivel</th>
                    <th>Criado em</th>
                    <th>Acoes</th>
                </tr>
            </thead>
            <tbody>
    `;

    items.forEach(item => {
        const selectedClass = item.login === editingLogin ? "is-selected" : "";
        const displayName = item.name || "-";
        const displayCodigoFJ = item.codigoFJ || "-";
        const accessClass = `access-${String(item.accessLabel || "").toLowerCase()}`;

        html += `
            <tr class="${selectedClass}">
                <td>${escapeHtml(item.login)}</td>
                <td>${escapeHtml(displayName)}</td>
                <td>${escapeHtml(displayCodigoFJ)}</td>
                <td><span class="access-pill ${accessClass}">${escapeHtml(item.accessLabel)}</span></td>
                <td>${escapeHtml(item.createdAt || "-")}</td>
                <td class="actions-cell">
                    <button class="row-btn row-btn-edit" type="button" data-edit="${escapeHtmlAttr(item.login)}">Editar</button>
                    <button class="row-btn row-btn-password" type="button" data-password="${escapeHtmlAttr(item.login)}">Senha</button>
                </td>
            </tr>
        `;
    });

    html += `
            </tbody>
        </table>
    `;

    container.innerHTML = html;

    container.querySelectorAll("[data-edit]").forEach(button => {
        button.addEventListener("click", () => {
            openEditModal(button.dataset.edit);
        });
    });

    container.querySelectorAll("[data-password]").forEach(button => {
        button.addEventListener("click", () => {
            openPasswordModal(button.dataset.password);
        });
    });
}

function openCreateModal() {
    currentModalMode = "create";
    resetUserForm();
    syncUserModeState();
    document.getElementById("userModal").classList.remove("hidden");
}

function openEditModal(login) {
    const user = rows.find(item => item.login === login);
    if (!user) return;

    currentModalMode = "edit";
    editingLogin = user.login;

    document.getElementById("txtLogin").value = user.login || "";
    document.getElementById("txtLogin").readOnly = true;
    document.getElementById("txtName").value = user.name || "";
    document.getElementById("cmbAccessLevel").value = String(user.accessLevel || "");
    document.getElementById("txtPassword").value = "";
    document.getElementById("txtConfirmPassword").value = "";

    syncUserModeState();
    applyFilters();
    document.getElementById("userModal").classList.remove("hidden");
}

function syncEditingUser() {
    if (!editingLogin || currentModalMode !== "edit") return;

    const refreshed = rows.find(item => item.login === editingLogin);
    if (!refreshed) {
        closeUserModal();
        return;
    }

    if (!document.getElementById("userModal").classList.contains("hidden")) {
        openEditModal(editingLogin);
    }
}

function resetUserForm() {
    editingLogin = "";
    document.getElementById("txtLogin").value = "";
    document.getElementById("txtLogin").readOnly = false;
    document.getElementById("txtName").value = "";
    document.getElementById("cmbAccessLevel").value = "";
    document.getElementById("txtPassword").value = "";
    document.getElementById("txtConfirmPassword").value = "";
    syncUserModeState();
    applyFilters();
}

function closeUserModal() {
    document.getElementById("userModal").classList.add("hidden");
    currentModalMode = "create";
    resetUserForm();
}

function syncUserModeState() {
    const isEdit = currentModalMode === "edit" && !!editingLogin;
    document.getElementById("userFormTitle").textContent = isEdit ? "Editar Usuario" : "Novo Usuario";
    document.getElementById("userFormSubtitle").textContent = isEdit
        ? `Ajuste os dados e o nivel de acesso de ${editingLogin}.`
        : "Cadastre um novo usuario com o nivel de acesso correto.";
    document.getElementById("btnSaveUserModal").textContent = isEdit ? "Salvar Alteracoes" : "Cadastrar";
    document.getElementById("createPasswordBlock").classList.toggle("hidden", isEdit);
}

function submitUserModal() {
    if (currentModalMode === "edit") {
        send("update", buildUserPayload(false));
        return;
    }

    send("create", buildUserPayload(true));
}

function buildUserPayload(includePassword) {
    return {
        login: document.getElementById("txtLogin").value.trim(),
        name: document.getElementById("txtName").value.trim(),
        accessLevel: Number(document.getElementById("cmbAccessLevel").value || 0),
        password: includePassword ? document.getElementById("txtPassword").value : "",
        confirmPassword: includePassword ? document.getElementById("txtConfirmPassword").value : ""
    };
}

function openPasswordModal(login) {
    const user = rows.find(item => item.login === login);
    if (!user) return;

    passwordTargetLogin = user.login;
    document.getElementById("txtPasswordLogin").value = user.login;
    document.getElementById("txtNewPassword").value = "";
    document.getElementById("txtConfirmNewPassword").value = "";
    document.getElementById("passwordSubtitle").textContent = `Defina uma nova senha para ${user.login}.`;
    document.getElementById("passwordModal").classList.remove("hidden");
}

function closePasswordModal() {
    passwordTargetLogin = "";
    document.getElementById("txtPasswordLogin").value = "";
    document.getElementById("txtNewPassword").value = "";
    document.getElementById("txtConfirmNewPassword").value = "";
    document.getElementById("passwordModal").classList.add("hidden");
}

function submitPasswordReset() {
    if (!passwordTargetLogin) {
        alert("Selecione um usuario para redefinir a senha.");
        return;
    }

    send("reset_password", {
        login: passwordTargetLogin,
        password: document.getElementById("txtNewPassword").value,
        confirmPassword: document.getElementById("txtConfirmNewPassword").value
    });
}

function getAccessLabel(accessLevel) {
    const match = accessLevels.find(item => Number(item.value) === Number(accessLevel));
    return match ? match.label : "-";
}

function escapeHtml(value) {
    return String(value ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/\"/g, "&quot;")
        .replace(/'/g, "&#39;");
}

function escapeHtmlAttr(value) {
    return escapeHtml(value);
}
