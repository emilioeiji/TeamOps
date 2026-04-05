let _presences = [];
let _positions = [];
let _schedule = [];
let _allOperators = [];
let _sectorName = "";

window.chrome?.webview?.addEventListener("message", (event) => {
    const msg = event.data;

    switch (msg.type) {
        case "select_presence":
            _presences = msg.data;
            _sectorName = msg.sectorName;
            _sectorId = msg.sectorId;
            updateHeader();
            updateMapImage();
            updateSummary();
            renderOperators();
            break;

        case "select_positions":
            _positions = msg.data;
            renderOperators();
            break;

        case "select_schedule":
            _schedule = msg.data;
            updateSummary();
            renderOperators();
            break;
        
        case "select_operators":
           _allOperators = msg.data;
           renderOperators();
           break;

        case "import_result":
            alert(msg.message);
            break;
    }
});

function updateHeader() {
    const lbl = document.getElementById("sectorName");
    if (lbl) lbl.textContent = _sectorName;
}

function updateMapImage() {
    const map = document.getElementById("mapImage");
    map.src = `https://local/assets/${_sectorId}.png`;
}

function renderOperators() {
    console.log("ALL OPERATORS:", _allOperators);
    console.log("SCHEDULE:", _schedule);

    const layer = document.getElementById("operatorsLayer");
    const map = document.getElementById("mapImage");
    const container = document.getElementById("mapContainer");

    if (!layer || !map || !container) return;
    if (_positions.length === 0) return;

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

    // Agrupar presença por LocalId
    const presenceGroups = {};
    _presences.forEach(op => {
        if (!presenceGroups[op.LocalId]) presenceGroups[op.LocalId] = [];
        presenceGroups[op.LocalId].push(op);
    });

    // Agrupar schedule por LocalId
    const scheduleGroups = {};
    _schedule.forEach(op => {
        if (!scheduleGroups[op.LocalId]) scheduleGroups[op.LocalId] = [];
        scheduleGroups[op.LocalId].push(op);
    });

    // Para cada posição do mapa
    _positions.forEach(pos => {
        const localId = pos.LocalId;

        const pres = presenceGroups[localId] || [];
        const sch = scheduleGroups[localId] || [];

        // Se não tem nada, não renderiza
        if (pres.length === 0 && sch.length === 0) return;

        const groupDiv = document.createElement("div");
        groupDiv.className = "operatorGroup";

        groupDiv.style.left = (pos.X * scale + offsetX) + "px";
        groupDiv.style.top  = (pos.Y * scale + offsetY) + "px";

        // Renderizar presença (preto)
        pres.forEach(op => {
            const card = document.createElement("div");
            card.className = "operatorCardSmall";

            card.innerHTML = `
                <img src="https://local/assets/operators/${op.CodigoFJ}.png"
                     onerror="this.onerror=null; this.src='https://local/assets/default-operator.png';">
                <div class="nameRomanjiSmall">${op.NameRomanji || op.CodigoFJ}</div>
            `;

            groupDiv.appendChild(card);
        });

        // Renderizar previsão (vermelho)
        sch.forEach(op => {
            // Se já está presente, não renderiza previsão duplicada
            if (pres.some(p => p.CodigoFJ === op.CodigoFJ)) return;

            // Buscar dados completos do operador
            const full = _allOperators.find(o => o.CodigoFJ === op.CodigoFJ);

            const name = full?.NameRomanji || op.CodigoFJ;
            const foto = full
                ? `https://local/assets/operators/${full.CodigoFJ}.png`
                : `https://local/assets/operators/${op.CodigoFJ}.png`;

            const card = document.createElement("div");
            card.className = "operatorCardSmall schedule";

            card.innerHTML = `
                <img src="${foto}"
                     onerror="this.onerror=null; this.src='https://local/assets/default-operator.png';">
                <div class="nameRomanjiSmall">${name}</div>
            `;

            groupDiv.appendChild(card);
        });

        layer.appendChild(groupDiv);
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
    // cria o tooltip invisível
    window.summaryTooltip = document.createElement("div");
    summaryTooltip.id = "summaryTooltip";
    document.body.appendChild(summaryTooltip);

    // eventos dos filtros
    document.getElementById("datePicker").addEventListener("change", filtersChanged);
    document.getElementById("shiftPicker").addEventListener("change", filtersChanged);
    document.getElementById("btnRefresh").addEventListener("click", filtersChanged);
    document.getElementById("btnImportSchedule").addEventListener("click", importSchedule);

    // ativa o tooltip
    setupSummaryTooltip();
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

    const presentes = _presences.length;
    const previstos = _schedule.length;

    const faltantes = _schedule.filter(sch =>
        !_presences.some(pre => pre.CodigoFJ === sch.CodigoFJ)
    ).length;

    box.innerHTML = `
        Presentes: ${presentes} &nbsp;|&nbsp;
        Previsto: ${previstos} &nbsp;|&nbsp;
        <span style="color:#ff8080;">Faltantes: ${faltantes}</span>
    `;
}

function importSchedule() {
    const date = document.getElementById("datePicker").value;
    const shift = document.getElementById("shiftPicker").value;

    if (!date) {
        alert("Selecione uma data antes de importar o schedule.");
        return;
    }

    window.chrome.webview.postMessage({
        type: "import_schedule",
        date,
        shift
    });
}

function setupSummaryTooltip() {
    const box = document.getElementById("summaryBox");

    box.addEventListener("mouseenter", () => {
        const faltantes = _schedule.filter(sch =>
            !_presences.some(pre => pre.CodigoFJ === sch.CodigoFJ)
        );

        if (faltantes.length === 0) {
            summaryTooltip.style.display = "none";
            return;
        }

        summaryTooltip.innerHTML = faltantes
            .map(f => {
                const full = _allOperators.find(o => o.CodigoFJ === f.CodigoFJ);
                const name = full?.NameRomanji || f.CodigoFJ;
                return `<div>${name}</div>`;
            })
            .join("");

        summaryTooltip.style.display = "block";
    });

    box.addEventListener("mousemove", (e) => {
        summaryTooltip.style.left = (e.pageX + 12) + "px";
        summaryTooltip.style.top  = (e.pageY + 12) + "px";
    });

    box.addEventListener("mouseleave", () => {
        summaryTooltip.style.display = "none";
    });
}
