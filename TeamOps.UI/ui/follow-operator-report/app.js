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
    setText("generatedAt", data.generatedAt);
    setText("operatorName", `${data.operatorInfo?.nameRomanji || "-"} / ${data.operatorInfo?.nameNihongo || "-"}`);
    setText("operatorCode", `Codigo FJ: ${data.operatorInfo?.codigoFJ || "-"}`);
    setText("startDate", data.operatorInfo?.startDate);
    setText("totalCount", data.operatorInfo?.total ?? 0);
    setText("lastDate", data.operatorInfo?.lastDate);
    document.getElementById("logo").src = data.logoUrl || "";

    const timeline = document.getElementById("timeline");
    const records = data.records || [];

    if (!records.length) {
        timeline.innerHTML = `<article class="record-card"><p class="empty-state">Nenhum acompanhamento encontrado para este operador.</p></article>`;
        return;
    }

    timeline.innerHTML = records.map(record => `
        <article class="record-card">
            <div class="record-head">
                <strong>${escapeHtml(record.date)}</strong>
                <span>${escapeHtml(record.shift)}</span>
            </div>
            <div class="record-grid">
                <div><span class="section-label">Executor</span><strong>${escapeHtml(record.executor)}</strong></div>
                <div><span class="section-label">Testemunha</span><strong>${escapeHtml(record.witness)}</strong></div>
                <div><span class="section-label">Motivo</span><strong>${escapeHtml(record.reason)}</strong></div>
                <div><span class="section-label">Tipo</span><strong>${escapeHtml(record.type)}</strong></div>
                <div><span class="section-label">Local</span><strong>${escapeHtml(record.local)}</strong></div>
                <div><span class="section-label">Equipamento</span><strong>${escapeHtml(record.equipment)}</strong></div>
                <div><span class="section-label">Setor</span><strong>${escapeHtml(record.sector)}</strong></div>
            </div>
            <div class="content-card">
                <span class="section-label">Descricao</span>
                <div class="content-box">${escapeHtml(record.description)}</div>
            </div>
            <div class="content-card">
                <span class="section-label">Orientacao</span>
                <div class="content-box">${escapeHtml(record.guidance)}</div>
            </div>
        </article>
    `).join("");
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

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;");
}
