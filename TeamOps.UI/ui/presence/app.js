console.log("🔥 app.js carregou!");
console.log("🔥 script executando…");

let _presences = [];
let _positions = [];
let _sectorName = "";

console.log("🔥 registrando listener de mensagens…");

window.chrome?.webview?.addEventListener("message", (event) => {
    const msg = event.data;
    console.log("RECEBIDO DO C#:", msg);

    switch (msg.type) {
        console.log("🔥 filtersChanged disparou!", date, shift);
        
        case "select_presence":
    _presences = msg.data;
    _sectorName = msg.sectorName;
    updateHeader();
    renderOperators();
    break;

        case "select_positions":
    _positions = msg.data;
    renderOperators();
    break;
}
});

function updateHeader() {
    const lbl = document.getElementById("sectorName");
    if (lbl) lbl.textContent = _sectorName;
}

function renderOperators() {
    const layer = document.getElementById("operatorsLayer");
    const map = document.getElementById("mapImage");

    if (!layer || !map) return;

    layer.innerHTML = "";

    if (_presences.length === 0 || _positions.length === 0)
        return;

    const rect = map.getBoundingClientRect();
    const scaleX = rect.width / map.naturalWidth;
    const scaleY = rect.height / map.naturalHeight;

    _presences.forEach(op => {
        const pos = _positions.find(p => p.LocalId === op.LocalId);
        if (!pos) return;

        const div = document.createElement("div");
        div.className = "operatorCard";

        div.style.left = (pos.X * scaleX) + "px";
        div.style.top = (pos.Y * scaleY) + "px";

        div.innerHTML = `
            <img src="https://app/assets/operators/${op.CodigoFJ}.png"
                onerror="this.onerror=null; this.src='https://app/assets/default-operator.png';">
            <div class="nameRomanji">${op.NameRomanji ?? op.CodigoFJ}</div>
            <div class="nameNihongo">${op.NameNihongo ?? ""}</div>
        `;

        layer.appendChild(div);
    });
}

document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("datePicker").addEventListener("change", filtersChanged);
    document.getElementById("shiftPicker").addEventListener("change", filtersChanged);
});

function filtersChanged() {
    console.log("🔥 filtersChanged disparou!", date, shift);

    const date = document.getElementById("datePicker").value;
    const shift = document.getElementById("shiftPicker").value;

    window.chrome.webview.postMessage({
        type: "filtersChanged",
        date,
        shift
    });
}

window.addEventListener("load", () => {
    // Preenche a data automaticamente
    const today = new Date().toISOString().split("T")[0];
    document.getElementById("datePicker").value = today;

    // Seleciona turno padrão (1 = dia)
    document.getElementById("shiftPicker").value = "1";

    // Dispara o carregamento inicial
    filtersChanged();
});
