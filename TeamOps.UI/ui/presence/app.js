let _presences = [];
let _positions = [];
let _sectorName = "";

window.chrome?.webview?.addEventListener("message", (event) => {
    const msg = event.data;

    switch (msg.type) {
        case "select_presence":
            _presences = msg.data;
            _sectorName = msg.sectorName;
            updateHeader();
            updateSummary();
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
    const container = document.getElementById("mapContainer");

    if (!layer || !map || !container) return;
    if (_presences.length === 0 || _positions.length === 0) return;

    layer.innerHTML = "";

    const naturalW = map.naturalWidth;
    const naturalH = map.naturalHeight;

    const mapRect = map.getBoundingClientRect();
    const containerRect = container.getBoundingClientRect();

    const scale = Math.min(
        mapRect.width / naturalW,
        mapRect.height / naturalH
    );

    const realW = naturalW * scale;
    const realH = naturalH * scale;

    const leftoverX = (mapRect.width  - realW) / 2;
    const leftoverY = (mapRect.height - realH) / 2;

    const offsetX = mapRect.left - containerRect.left + leftoverX;
    const offsetY = mapRect.top  - containerRect.top  + leftoverY;

    _presences.forEach(op => {
        const pos = _positions.find(p => p.LocalId === op.LocalId);
        if (!pos) return;

        const div = document.createElement("div");
        div.className = "operatorCard";

        div.style.left = (pos.X * scale + offsetX) + "px";
        div.style.top  = (pos.Y * scale + offsetY) + "px";

        div.innerHTML = `
            <img src="https://local/assets/operators/${op.CodigoFJ}.png"
                 onerror="this.onerror=null; this.src='https://local/assets/default-operator.png';">
            <div class="nameRomanji">${op.NameRomanji || pos.NameRomanji || op.CodigoFJ}</div>
        `;

        layer.appendChild(div);
    });
}

function filtersChanged() {
    const date = document.getElementById("datePicker").value;
    const shift = document.getElementById("shiftPicker").value;

    window.chrome.webview.postMessage({
        type: "filtersChanged",
        date,
        shift
    });
}

document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("datePicker").addEventListener("change", filtersChanged);
    document.getElementById("shiftPicker").addEventListener("change", filtersChanged);
    document.getElementById("btnRefresh").addEventListener("click", filtersChanged);
});

window.addEventListener("load", () => {
    const today = new Date().toISOString().split("T")[0];
    document.getElementById("datePicker").value = today;
    document.getElementById("shiftPicker").value = "1";
    filtersChanged();
});

function updateSummary() {
    const box = document.getElementById("summaryBox");
    if (!box) return;

    box.textContent = "Total: " + _presences.length;
}
