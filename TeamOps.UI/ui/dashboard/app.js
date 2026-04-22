const ACCESS_LABELS = {
    0: "Operador",
    1: "KL",
    2: "GL",
    3: "Admin"
};

const PERMISSION_RULES = {
    btnRelatorios: 2,
    btnPresenca: 2,
    btnPresenca2: 2,
    btnFollowUp: 1,
    btnHikitsugui: 1,
    btnHikitsuguiLeaderRead: 1,
    btnPR: 1,
    btnCL: 1,
    btnSobraDePeca: 1,
    btnAdmin: 3,
    btnAccessControl: 3
};

document.addEventListener("DOMContentLoaded", () => {
    bindActions();
});

function bindActions() {
    document.querySelectorAll("[data-action]").forEach(button => {
        button.addEventListener("click", () => {
            window.chrome.webview.postMessage(button.dataset.action);
        });
    });
}

window.chrome?.webview?.addEventListener("message", event => {
    const payload = event.data;

    if (!payload || payload.type !== "init") {
        return;
    }

    hydrateDashboard(payload);
});

function hydrateDashboard(payload) {
    setText("lblUser", payload.user);
    setText("lblOperator", payload.operatorName);
    setText("lblShift", payload.shiftName);
    setText("lblDate", payload.date);

    const level = Number(payload.accessLevel ?? 0);
    const label = ACCESS_LABELS[level] || `Nivel ${level}`;

    setText("lblAccessLevel", label);
    setText("accessBadge", `Perfil ${label}`);

    applyPermissions(level);
}

function applyPermissions(level) {
    Object.entries(PERMISSION_RULES).forEach(([id, minimumLevel]) => {
        const button = document.getElementById(id);
        if (!button) return;

        button.classList.toggle("hidden", level < minimumLevel);
    });

    document.querySelectorAll("[data-group]").forEach(group => {
        const visibleActions = Array.from(group.querySelectorAll(".action-tile"))
            .filter(button => !button.classList.contains("hidden"));

        group.classList.toggle("hidden", visibleActions.length === 0);
    });
}

function setText(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = value ?? "-";
    }
}
