const ACCESS_LABELS = {
    1: "Basic",
    2: "KL",
    3: "GL",
    4: "Manager",
    5: "Admin"
};

const PERMISSION_RULES = {
    btnRelatorios: 3,
    btnPresenca: 3,
    btnPresenca2: 3,
    btnFollowUp: 2,
    btnTasks: 3,
    btnHikitsugui: 2,
    btnHikitsuguiLeaderRead: 2,
    btnPR: 2,
    btnCL: 2,
    btnSobraDePeca: 2,
    btnAdmin: 5,
    btnAccessControl: 5
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
    setText("lblOpenTasks", payload.openTasksForShift ?? 0);

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
