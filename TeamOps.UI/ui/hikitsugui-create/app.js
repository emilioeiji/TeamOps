const state = {
    locale: "pt-BR",
    creatorNamePt: "",
    creatorNameJp: ""
};

const I18N = {
    "pt-BR": {
        headerTitle: "Registrar Hikitsugui",
        dateLabel: "Data",
        creatorLabel: "Criador",
        shiftLabel: "Turno",
        categoryLabel: "Categoria",
        equipmentLabel: "Equipamento",
        localLabel: "Local",
        sectorLabel: "Setor",
        selectPlaceholder: "Selecionar...",
        publicOperator: "Operador",
        publicLeader: "Líder",
        publicMasv: "MA/SV",
        descriptionLabel: "Descrição",
        listButton: "• Lista",
        clearButton: "Limpar",
        attachmentsLabel: "Anexos",
        saveButton: "Salvar",
        cancelButton: "Cancelar",
        savedMessage: "Hikitsugui registrado com sucesso!",
        deleteAttachment: "Excluir",
        validationCategory: "Selecione uma categoria.",
        validationEquipment: "Selecione um equipamento.",
        validationLocal: "Selecione um local.",
        validationSector: "Selecione um setor.",
        validationDescription: "Informe a descrição."
    },
    "ja-JP": {
        headerTitle: "Hikitsugui を登録",
        dateLabel: "日付",
        creatorLabel: "作成者",
        shiftLabel: "シフト",
        categoryLabel: "カテゴリ",
        equipmentLabel: "設備",
        localLabel: "場所",
        sectorLabel: "セクター",
        selectPlaceholder: "選択してください",
        publicOperator: "オペレーター",
        publicLeader: "リーダー",
        publicMasv: "MA/SV",
        descriptionLabel: "内容",
        listButton: "• リスト",
        clearButton: "クリア",
        attachmentsLabel: "添付",
        saveButton: "保存",
        cancelButton: "キャンセル",
        savedMessage: "Hikitsugui を登録しました。",
        deleteAttachment: "削除",
        validationCategory: "カテゴリを選択してください。",
        validationEquipment: "設備を選択してください。",
        validationLocal: "場所を選択してください。",
        validationSector: "セクターを選択してください。",
        validationDescription: "内容を入力してください。"
    }
};

function t(key) {
    return I18N[state.locale]?.[key] ?? I18N["pt-BR"][key];
}

function getLocalizedField() {
    return state.locale === "ja-JP" ? "NameJp" : "NamePt";
}

function getCreatorName() {
    return state.locale === "ja-JP"
        ? (state.creatorNameJp || state.creatorNamePt || "")
        : (state.creatorNamePt || state.creatorNameJp || "");
}

function send(action, extra = {}) {
    window.chrome.webview.postMessage({
        action,
        ...extra
    });
}

window.addEventListener("DOMContentLoaded", () => {
    send("load");

    document.getElementById("btnSalvar").addEventListener("click", salvar);
    document.getElementById("btnCancelar").addEventListener("click", () => send("cancel"));
    document.getElementById("fileUpload").addEventListener("change", handleFiles);

    initEditorToolbar();
});

window.chrome.webview.addEventListener("message", e => {
    const msg = e.data;

    switch (msg.type) {
        case "filters":
            preencherFiltros(msg);
            break;

        case "saved":
            alert(t("savedMessage"));
            send("cancel");
            break;
    }
});

function preencherFiltros(data) {
    state.locale = data.locale === "ja-JP" ? "ja-JP" : "pt-BR";
    state.creatorNamePt = data.creatorNamePt || "";
    state.creatorNameJp = data.creatorNameJp || data.creatorNamePt || "";

    applyLocale();

    const now = new Date();
    const yyyy = now.getFullYear();
    const mm = String(now.getMonth() + 1).padStart(2, "0");
    const dd = String(now.getDate()).padStart(2, "0");
    document.getElementById("txtDate").value = `${yyyy}-${mm}-${dd}`;
    document.getElementById("txtCreator").value = getCreatorName();

    fillSelect("shiftId", data.shifts, "Id", getLocalizedField());
    document.getElementById("shiftId").value = data.shiftId;

    fillSelect("categoryId", data.categories, "Id", getLocalizedField(), true);
    fillSelect("equipId", data.equipments, "Id", getLocalizedField(), true);
    fillSelect("localId", data.locals, "Id", getLocalizedField(), true);
    fillSelect("sectorId", data.sectors, "Id", getLocalizedField(), true);
}

function applyLocale() {
    document.documentElement.lang = state.locale;
    document.title = t("headerTitle");

    document.getElementById("txtHeaderTitle").textContent = t("headerTitle");
    document.getElementById("txtDateLabel").textContent = t("dateLabel");
    document.getElementById("txtCreatorLabel").textContent = t("creatorLabel");
    document.getElementById("txtShiftLabel").textContent = t("shiftLabel");
    document.getElementById("txtCategoryLabel").textContent = t("categoryLabel");
    document.getElementById("txtEquipmentLabel").textContent = t("equipmentLabel");
    document.getElementById("txtLocalLabel").textContent = t("localLabel");
    document.getElementById("txtSectorLabel").textContent = t("sectorLabel");
    document.getElementById("txtPublicOperator").textContent = t("publicOperator");
    document.getElementById("txtPublicLeader").textContent = t("publicLeader");
    document.getElementById("txtPublicMasv").textContent = t("publicMasv");
    document.getElementById("txtDescriptionLabel").textContent = t("descriptionLabel");
    document.getElementById("btnBulletList").textContent = t("listButton");
    document.getElementById("btnClearFormat").textContent = t("clearButton");
    document.getElementById("txtAttachmentsLabel").textContent = t("attachmentsLabel");
    document.getElementById("btnSalvar").textContent = t("saveButton");
    document.getElementById("btnCancelar").textContent = t("cancelButton");
}

function fillSelect(id, list, valueField, textField, allowZero = false) {
    const sel = document.getElementById(id);
    sel.innerHTML = "";

    if (allowZero) {
        sel.innerHTML += `<option value="0">${escapeHtml(t("selectPlaceholder"))}</option>`;
    }

    list.forEach(item => {
        sel.innerHTML += `<option value="${item[valueField]}">${escapeHtml(item[textField])}</option>`;
    });
}

let attachments = [];

async function handleFiles(ev) {
    for (const file of ev.target.files) {
        const base64 = await toBase64(file);

        attachments.push({
            fileName: file.name,
            base64
        });
    }

    renderAttachmentList();
    ev.target.value = "";
}

function renderAttachmentList() {
    const list = document.getElementById("fileList");
    list.innerHTML = "";

    attachments.forEach((file, index) => {
        const li = document.createElement("li");
        li.innerHTML = `<span>${escapeHtml(file.fileName)}</span>`;

        const btn = document.createElement("button");
        btn.textContent = t("deleteAttachment");

        btn.onclick = () => {
            attachments.splice(index, 1);
            renderAttachmentList();
        };

        li.appendChild(btn);
        list.appendChild(li);
    });
}

function toBase64(file) {
    return new Promise(resolve => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result.split(",")[1]);
        reader.readAsDataURL(file);
    });
}

function initEditorToolbar() {
    const toolbar = document.querySelector(".editor-toolbar");
    const editor = document.getElementById("txtDescricao");

    toolbar.addEventListener("click", ev => {
        const btn = ev.target.closest("button");
        if (!btn) return;

        const cmd = btn.getAttribute("data-cmd");
        if (!cmd) return;

        editor.focus();
        document.execCommand(cmd, false, null);
    });

    document.getElementById("btnClearFormat").addEventListener("click", () => {
        const text = editor.innerText;
        editor.innerHTML = text ? `<p>${text}</p>` : "";
    });
}

function getFullDateTime() {
    const dateOnly = document.getElementById("txtDate").value;
    const now = new Date();

    const hh = String(now.getHours()).padStart(2, "0");
    const mi = String(now.getMinutes()).padStart(2, "0");
    const ss = String(now.getSeconds()).padStart(2, "0");

    return `${dateOnly} ${hh}:${mi}:${ss}`;
}

function validar() {
    if (Number(document.getElementById("categoryId").value) === 0) {
        alert(t("validationCategory"));
        return false;
    }

    if (Number(document.getElementById("equipId").value) === 0) {
        alert(t("validationEquipment"));
        return false;
    }

    if (Number(document.getElementById("localId").value) === 0) {
        alert(t("validationLocal"));
        return false;
    }

    if (Number(document.getElementById("sectorId").value) === 0) {
        alert(t("validationSector"));
        return false;
    }

    const editor = document.getElementById("txtDescricao");
    const plain = editor.innerText.trim();
    if (!plain) {
        alert(t("validationDescription"));
        return false;
    }

    return true;
}

function salvar() {
    if (!validar()) return;

    const publico = document.querySelector("input[name='publico']:checked").value;

    const payload = {
        action: "save",
        date: getFullDateTime(),
        shiftId: Number(document.getElementById("shiftId").value),
        reasonId: Number(document.getElementById("categoryId").value),
        equipId: Number(document.getElementById("equipId").value),
        localId: Number(document.getElementById("localId").value),
        sectorId: Number(document.getElementById("sectorId").value),
        publico,
        text: document.getElementById("txtDescricao").innerHTML,
        attachments
    };

    send("save", payload);
}

function escapeHtml(value) {
    return String(value ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
}
