document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("btnPrint").addEventListener("click", () => post("print"));
    document.getElementById("btnPdf").addEventListener("click", () => post("save_pdf"));
    post("load");
});

window.chrome?.webview?.addEventListener("message", event => {
    const payload = event.data;
    if (!payload?.type) return;

    if (payload.type === "init") {
        hydrate(payload.data || {});
        return;
    }

    if (payload.type === "notify") {
        showToast(payload.data?.title || "Aviso", payload.data?.message || "");
    }
});

function hydrate(data) {
    setText("reportTitle", data.header?.title);
    setText("reportSubtitle", data.header?.subtitle);
    setText("generatedAt", data.generatedAt);
    setText("operatorName", `${data.operatorInfo?.nameRomanji || "-"} / ${data.operatorInfo?.nameNihongo || "-"}`);
    setText("operatorCode", `Codigo FJ: ${data.operatorInfo?.codigoFJ || "-"}`);
    setText("startDate", data.operatorInfo?.startDate);
    setText("followDate", data.follow?.date);
    setText("shift", data.follow?.shift);
    setText("executor", data.follow?.executor);
    setText("witness", data.follow?.witness);
    setText("reason", data.follow?.reason);
    setText("type", data.follow?.type);
    setText("local", data.follow?.local);
    setText("equipment", data.follow?.equipment);
    setText("sector", data.follow?.sector);
    setText("description", data.follow?.description);
    setText("guidance", data.follow?.guidance);

    const logo = document.getElementById("logo");
    logo.src = data.logoUrl || "";
}

function post(action) {
    window.chrome?.webview?.postMessage({ action });
}

function setText(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = value ?? "-";
    }
}

function showToast(title, message) {
    const root = document.getElementById("toast");
    document.getElementById("toastTitle").textContent = title;
    document.getElementById("toastMessage").textContent = message;
    root.classList.remove("hidden");
    clearTimeout(showToast._timer);
    showToast._timer = setTimeout(() => root.classList.add("hidden"), 2800);
}
