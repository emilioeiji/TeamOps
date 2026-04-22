const toast = {
    root: document.getElementById("toast"),
    title: document.getElementById("toastTitle"),
    message: document.getElementById("toastMessage"),
    timer: null
};

document.addEventListener("DOMContentLoaded", () => {
    bindActions();
});

function bindActions() {
    document.querySelectorAll("[data-action]").forEach(button => {
        button.addEventListener("click", () => {
            window.chrome.webview.postMessage({
                action: button.dataset.action
            });
        });
    });
}

window.chrome?.webview?.addEventListener("message", event => {
    const payload = event.data;
    if (!payload?.type) {
        return;
    }

    if (payload.type === "init") {
        hydrate(payload.data || {});
        return;
    }

    if (payload.type === "notify") {
        showToast(payload.data?.title || "Aviso", payload.data?.message || "");
    }
});

function hydrate(data) {
    setText("lblOperator", data.operatorName);
    setText("lblShift", data.shiftName);
    setText("lblDate", data.date);
    setText("lblAvailable", data.availableCount);
    setText("lblTotal", data.totalCount);
}

function setText(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = value ?? "-";
    }
}

function showToast(title, message) {
    toast.title.textContent = title;
    toast.message.textContent = message;
    toast.root.classList.remove("hidden");

    if (toast.timer) {
        clearTimeout(toast.timer);
    }

    toast.timer = setTimeout(() => {
        toast.root.classList.add("hidden");
    }, 3200);
}
