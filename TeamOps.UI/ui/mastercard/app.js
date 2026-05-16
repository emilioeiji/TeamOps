const state = {
    locale: "pt-BR",
    currentOperatorNamePt: "",
    currentOperatorNameJp: "",
    currentUser: "",
    rows: [],
    operators: [],
    trainers: [],
    sectors: [],
    equipments: [],
    statuses: []
};

const I18N = {
    "pt-BR": {
        title: "MasterCard",
        headerTitle: "Controle de MasterCard",
        eyebrow: "Treinamento Operacional",
        heroTitle: "Controle do que foi ensinado, quem treinou e quando deve acontecer o follow",
        heroText: "Use este fluxo para registrar o treinamento, concluir o MasterCard e acompanhar o follow de 30 dias ate a finalizacao.",
        metricInProgress: "Andamento",
        metricFollow: "Follow",
        metricCompleted: "Finalizados",
        tableTitle: "MasterCards cadastrados",
        subtitlePrefix: "Responsavel logado",
        subtitleUser: "Usuario",
        showCompleted: "Mostrar finalizados",
        statusLabel: "Status",
        searchLabel: "Buscar",
        searchPlaceholder: "Operador, treinador, setor ou equipamento...",
        newCard: "Novo MasterCard",
        empty: "Nenhum MasterCard encontrado.",
        colOperator: "Operador",
        colTrainer: "Treinador",
        colStartDate: "Inicio",
        colFollowDate: "Follow",
        colSector: "Setor",
        colEquipment: "Equipamento",
        colStatus: "Status",
        colUpdated: "Atualizado",
        colActions: "Acoes",
        createTitle: "Novo MasterCard",
        createSubtitle: "Registre o operador treinado, treinador e os dados base do acompanhamento.",
        editTitle: id => `Editar MasterCard #${id}`,
        editSubtitle: id => `Atualize o MasterCard #${id} e mantenha o fluxo de treinamento e follow sob controle.`,
        operatorLabel: "Operador",
        trainerLabel: "Treinador",
        startDateLabel: "Data de inicio",
        sectorLabel: "Setor",
        equipmentLabel: "Equipamento",
        statusValueLabel: "Status",
        notesLabel: "Observacoes",
        notesPlaceholder: "Anote pontos importantes do treinamento, ressalvas ou combinados.",
        metaCreatedBy: "Criado por",
        metaCreatedAt: "Criado em",
        metaUpdatedAt: "Atualizado em",
        metaConcludedAt: "Concluido em",
        metaFollowDate: "Data do Follow",
        metaFinalizedAt: "Finalizado em",
        historyTitle: "Historico de Status",
        historySubtitle: "Ultimas alteracoes registradas para este MasterCard.",
        historyEmpty: "Nenhuma mudanca registrada ainda.",
        historyFromTo: (fromLabel, toLabel) => `De ${fromLabel} para ${toLabel}`,
        historyInitial: toLabel => `Status inicial: ${toLabel}`,
        historyBy: "Por",
        cancel: "Cancelar",
        save: "Salvar",
        saveChanges: "Salvar Alteracoes",
        createCard: "Cadastrar MasterCard",
        conclude: "Concluir MasterCard",
        finalize: "Finalizar Follow",
        confirmConclude: id => `Deseja concluir o MasterCard #${id} e agendar o follow em 30 dias?`,
        confirmFinalize: id => `Deseja finalizar o MasterCard #${id} apos realizar o follow?`,
        quickConclude: "Concluir",
        quickFinalize: "Finalizar",
        saveSuccess: "MasterCard cadastrado com sucesso.",
        updateSuccess: "MasterCard atualizado com sucesso.",
        statusSuccess: "Status do MasterCard atualizado com sucesso.",
        errorFallback: "Ocorreu um erro ao processar o MasterCard.",
        selectAll: "Todos",
        selectPlaceholder: "Selecione...",
        viewTitle: id => `Visualizar MasterCard #${id}`,
        viewSubtitle: "Leitura completa do treinamento, follow e historico.",
        viewStatus: "Status",
        viewOperator: "Operador",
        viewTrainer: "Treinador",
        viewStartDate: "Inicio",
        viewSector: "Setor",
        viewEquipment: "Equipamento",
        viewFollowDate: "Data do Follow",
        viewFinalizedAt: "Finalizado em",
        viewDescriptionTitle: "Resumo do treinamento",
        viewDescriptionSubtitle: "Referencia do equipamento e observacoes registradas.",
        viewHistoryTitle: "Historico de Status",
        viewHistorySubtitle: "Acompanhe a evolucao do MasterCard ate o follow final.",
        close: "Fechar",
        editCard: "Editar MasterCard",
        noDescription: "Sem descricao.",
        noNotes: "Sem observacoes.",
        closeModalAria: "Fechar modal"
    },
    "ja-JP": {
        title: "MasterCard",
        headerTitle: "MasterCard Control",
        eyebrow: "Training Flow",
        heroTitle: "何を教えたか、誰が教えたか、30日後の Follow をいつ実施するかを管理",
        heroText: "教育内容を登録し、MasterCard を完了し、30日後の Follow から最終完了まで追跡します。",
        metricInProgress: "進行中",
        metricFollow: "Follow",
        metricCompleted: "完了",
        tableTitle: "登録済み MasterCard",
        subtitlePrefix: "ログイン担当",
        subtitleUser: "ユーザー",
        showCompleted: "完了済みも表示",
        statusLabel: "ステータス",
        searchLabel: "検索",
        searchPlaceholder: "作業者、指導者、セクター、設備で検索...",
        newCard: "新規 MasterCard",
        empty: "MasterCard が見つかりません。",
        colOperator: "作業者",
        colTrainer: "指導者",
        colProcess: "工程",
        colStartDate: "開始",
        colFollowDate: "Follow",
        colSector: "セクター",
        colEquipment: "設備",
        colStatus: "ステータス",
        colUpdated: "更新",
        colActions: "操作",
        createTitle: "新規 MasterCard",
        createSubtitle: "教育対象者、指導者と基本情報を登録してください。",
        editTitle: id => `MasterCard #${id} を編集`,
        editSubtitle: id => `MasterCard #${id} を更新し、教育から Follow までの流れを維持します。`,
        operatorLabel: "作業者",
        trainerLabel: "指導者",
        startDateLabel: "開始日",
        sectorLabel: "セクター",
        equipmentLabel: "設備",
        statusValueLabel: "ステータス",
        notesLabel: "備考",
        notesPlaceholder: "教育時の注意点や引継ぎ事項を記録してください。",
        metaCreatedBy: "作成者",
        metaCreatedAt: "作成日時",
        metaUpdatedAt: "更新日時",
        metaConcludedAt: "完了日時",
        metaFollowDate: "Follow 日",
        metaFinalizedAt: "最終完了日時",
        historyTitle: "ステータス履歴",
        historySubtitle: "この MasterCard の最新履歴です。",
        historyEmpty: "まだ履歴はありません。",
        historyFromTo: (fromLabel, toLabel) => `${fromLabel} から ${toLabel} へ変更`,
        historyInitial: toLabel => `初期ステータス: ${toLabel}`,
        historyBy: "担当",
        cancel: "キャンセル",
        save: "保存",
        saveChanges: "変更を保存",
        createCard: "MasterCard を登録",
        conclude: "MasterCard を完了",
        finalize: "Follow を完了",
        confirmConclude: id => `MasterCard #${id} を完了し、30日後の Follow を登録しますか。`,
        confirmFinalize: id => `MasterCard #${id} の Follow を完了して最終完了にしますか。`,
        quickConclude: "完了",
        quickFinalize: "最終",
        saveSuccess: "MasterCard を登録しました。",
        updateSuccess: "MasterCard を更新しました。",
        statusSuccess: "MasterCard のステータスを更新しました。",
        errorFallback: "MasterCard の処理中にエラーが発生しました。",
        selectAll: "すべて",
        selectPlaceholder: "選択してください",
        viewTitle: id => `MasterCard #${id} を表示`,
        viewSubtitle: "教育、Follow、履歴をまとめて確認します。",
        viewStatus: "ステータス",
        viewOperator: "作業者",
        viewTrainer: "指導者",
        viewStartDate: "開始",
        viewSector: "セクター",
        viewEquipment: "設備",
        viewFollowDate: "Follow 日",
        viewFinalizedAt: "最終完了日時",
        viewDescriptionTitle: "教育概要",
        viewDescriptionSubtitle: "設備の参照情報と登録された備考を確認します。",
        viewHistoryTitle: "ステータス履歴",
        viewHistorySubtitle: "MasterCard の進捗を最終完了まで追跡します。",
        close: "閉じる",
        editCard: "MasterCard を編集",
        noDescription: "説明はありません。",
        noNotes: "備考はありません。",
        closeModalAria: "モーダルを閉じる"
    }
};

let currentModalMode = "create";
let editingId = 0;
let viewingId = 0;

window.addEventListener("DOMContentLoaded", () => {
    bindEvents();
    send("load");
});

function bindEvents() {
    document.getElementById("searchInput").addEventListener("input", applyFilters);
    document.getElementById("statusFilter").addEventListener("change", applyFilters);
    document.getElementById("toggleShowCompleted").addEventListener("change", applyFilters);
    document.getElementById("btnHeaderNovo").addEventListener("click", openCreateModal);
    document.getElementById("btnSaveModal").addEventListener("click", submitModal);
    document.getElementById("btnCancelModal").addEventListener("click", closeModal);
    document.getElementById("btnCloseModal").addEventListener("click", closeModal);
    document.getElementById("modalBackdrop").addEventListener("click", closeModal);
    document.getElementById("btnAdvanceStatus").addEventListener("click", handleAdvanceStatus);

    document.getElementById("btnCloseViewModal").addEventListener("click", closeViewModal);
    document.getElementById("btnCloseView").addEventListener("click", closeViewModal);
    document.getElementById("viewModalBackdrop").addEventListener("click", closeViewModal);
    document.getElementById("btnEditFromView").addEventListener("click", () => {
        const id = viewingId;
        closeViewModal();
        if (id) {
            openEditModal(id);
        }
    });
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
            state.rows = normalizeRows(msg.data || []);
            renderMetrics(state.rows);
            applyFilters();
            syncModals();
            break;
        case "mastercard_details":
            handleDetails(msg);
            break;
        case "saved":
            alert(msg.message || t("saveSuccess"));
            closeModal();
            break;
        case "updated":
            alert(msg.message || t("updateSuccess"));
            closeModal();
            break;
        case "status_changed":
            alert(msg.message || t("statusSuccess"));
            break;
        case "error":
            alert(msg.message || t("errorFallback"));
            break;
    }
});

function hydrateScreen(payload) {
    state.locale = payload.locale === "ja-JP" ? "ja-JP" : "pt-BR";
    state.currentOperatorNamePt = payload.currentOperatorNamePt || "";
    state.currentOperatorNameJp = payload.currentOperatorNameJp || payload.currentOperatorNamePt || "";
    state.currentUser = payload.currentUser || "";
    state.rows = normalizeRows(payload.rows || []);
    state.operators = normalizePeople(payload.operators || []);
    state.trainers = normalizePeople(payload.trainers || []);
    state.sectors = normalizeLookup(payload.sectors || []);
    state.equipments = normalizeLookup(payload.equipments || []);
    state.statuses = normalizeStatuses(payload.statuses || []);

    setLocale(state.locale);
    refillLookupControls();
    renderMetrics(state.rows);
    applyFilters();
}

function normalizePeople(items) {
    return items.map(item => ({
        codigoFJ: item.codigoFJ || "",
        namePt: item.namePt || "",
        nameJp: item.nameJp || item.namePt || "",
        shiftId: Number(item.shiftId || 0),
        sectorId: Number(item.sectorId || 0)
    }));
}

function normalizeLookup(items) {
    return items.map(item => ({
        id: Number(item.id || 0),
        namePt: item.namePt || "",
        nameJp: item.nameJp || item.namePt || ""
    }));
}

function normalizeStatuses(items) {
    return items.map(item => ({
        value: item.value || "",
        labelPt: item.labelPt || "",
        labelJp: item.labelJp || item.labelPt || ""
    }));
}

function normalizeRows(items) {
    return items.map(item => ({
        id: Number(item.id || 0),
        operatorCodigoFJ: item.operatorCodigoFJ || "",
        operatorNamePt: item.operatorNamePt || "",
        operatorNameJp: item.operatorNameJp || item.operatorNamePt || "",
        trainerCodigoFJ: item.trainerCodigoFJ || "",
        trainerNamePt: item.trainerNamePt || "",
        trainerNameJp: item.trainerNameJp || item.trainerNamePt || "",
        sectorId: Number(item.sectorId || 0),
        sectorNamePt: item.sectorNamePt || "",
        sectorNameJp: item.sectorNameJp || item.sectorNamePt || "",
        equipmentId: Number(item.equipmentId || 0),
        equipmentNamePt: item.equipmentNamePt || "",
        equipmentNameJp: item.equipmentNameJp || item.equipmentNamePt || "",
        description: item.description || "",
        notes: item.notes || "",
        startDate: item.startDate || "",
        masterCardStatus: item.masterCardStatus || "in_progress",
        concludedAt: item.concludedAt || "",
        followDate: item.followDate || "",
        finalizedAt: item.finalizedAt || "",
        createdAt: item.createdAt || "",
        updatedAt: item.updatedAt || "",
        createdByNamePt: item.createdByNamePt || "",
        createdByNameJp: item.createdByNameJp || item.createdByNamePt || "",
        historyCount: Number(item.historyCount || 0),
        history: item.history || []
    }));
}

function renderMetrics(rows) {
    setText("metricInProgress", rows.filter(row => row.masterCardStatus === "in_progress").length);
    setText("metricFollow", rows.filter(row => row.masterCardStatus === "follow").length);
    setText("metricCompleted", rows.filter(row => row.masterCardStatus === "completed").length);
}

function applyFilters() {
    const term = document.getElementById("searchInput").value.trim().toLowerCase();
    const statusFilter = document.getElementById("statusFilter").value;
    const showCompleted = document.getElementById("toggleShowCompleted").checked;

    const filtered = state.rows.filter(row => {
        if (!showCompleted && row.masterCardStatus === "completed") return false;
        if (statusFilter && row.masterCardStatus !== statusFilter) return false;
        if (!term) return true;

        const haystack = [
            row.operatorCodigoFJ,
            row.operatorNamePt,
            row.operatorNameJp,
            row.trainerCodigoFJ,
            row.trainerNamePt,
            row.trainerNameJp,
            row.notes,
            row.sectorNamePt,
            row.sectorNameJp,
            row.equipmentNamePt,
            row.equipmentNameJp,
            getStatusLabel(row.masterCardStatus, "pt-BR"),
            getStatusLabel(row.masterCardStatus, "ja-JP")
        ]
            .filter(Boolean)
            .join(" ")
            .toLowerCase();

        return haystack.includes(term);
    });

    renderTable(filtered);
}

function renderTable(rows) {
    const container = document.getElementById("tableContainer");

    if (!rows.length) {
        container.innerHTML = `<div class="empty-state">${escapeHtml(t("empty"))}</div>`;
        return;
    }

    let html = `
        <table class="tasks-table">
            <thead>
                <tr>
                    <th>#</th>
                    <th>${escapeHtml(t("colOperator"))}</th>
                    <th>${escapeHtml(t("colTrainer"))}</th>
                    <th>${escapeHtml(t("colStartDate"))}</th>
                    <th>${escapeHtml(t("colFollowDate"))}</th>
                    <th>${escapeHtml(t("colSector"))}</th>
                    <th>${escapeHtml(t("colEquipment"))}</th>
                    <th>${escapeHtml(t("colStatus"))}</th>
                    <th>${escapeHtml(t("colUpdated"))}</th>
                    <th>${escapeHtml(t("colActions"))}</th>
                </tr>
            </thead>
            <tbody>
    `;

    rows.forEach(row => {
        const operatorName = getLocalizedValue(row.operatorNamePt, row.operatorNameJp) || row.operatorCodigoFJ;
        const trainerName = getLocalizedValue(row.trainerNamePt, row.trainerNameJp) || row.trainerCodigoFJ;
        const sectorName = getLocalizedValue(row.sectorNamePt, row.sectorNameJp) || "-";
        const equipmentName = getLocalizedValue(row.equipmentNamePt, row.equipmentNameJp) || "-";

        html += `
            <tr>
                <td>${row.id}</td>
                <td><div class="task-main"><strong>${escapeHtml(operatorName)}</strong><small>${escapeHtml(row.operatorCodigoFJ)}</small></div></td>
                <td>${escapeHtml(trainerName)}</td>
                <td>${escapeHtml(row.startDate || "-")}</td>
                <td>${escapeHtml(row.followDate || "-")}</td>
                <td>${escapeHtml(sectorName)}</td>
                <td>${escapeHtml(equipmentName)}</td>
                <td><span class="status-pill ${statusClass(row.masterCardStatus)}">${escapeHtml(getStatusLabel(row.masterCardStatus))}</span></td>
                <td>${escapeHtml(row.updatedAt || "-")}</td>
                <td class="actions-cell">
                    <div class="actions-stack">
                        <button class="icon-btn icon-btn-view" type="button" data-view="${row.id}" title="${escapeHtmlAttr(t("viewTitle")(row.id))}" aria-label="${escapeHtmlAttr(t("viewTitle")(row.id))}">
                            ${iconEye()}
                        </button>
                        <button class="icon-btn icon-btn-edit" type="button" data-edit="${row.id}" title="${escapeHtmlAttr(t("editCard"))}" aria-label="${escapeHtmlAttr(t("editCard"))}">
                            ${iconEdit()}
                        </button>
                        ${row.masterCardStatus === "completed"
                            ? ""
                            : `<button class="quick-action-btn" type="button" data-advance="${row.id}" title="${escapeHtmlAttr(row.masterCardStatus === "in_progress" ? t("quickConclude") : t("quickFinalize"))}" aria-label="${escapeHtmlAttr(row.masterCardStatus === "in_progress" ? t("quickConclude") : t("quickFinalize"))}">
                                ${escapeHtml(row.masterCardStatus === "in_progress" ? t("quickConclude") : t("quickFinalize"))}
                            </button>`}
                    </div>
                </td>
            </tr>
        `;
    });

    html += "</tbody></table>";
    container.innerHTML = html;

    container.querySelectorAll("[data-view]").forEach(button => {
        button.addEventListener("click", () => openViewModal(Number(button.dataset.view)));
    });

    container.querySelectorAll("[data-edit]").forEach(button => {
        button.addEventListener("click", () => openEditModal(Number(button.dataset.edit)));
    });

    container.querySelectorAll("[data-advance]").forEach(button => {
        button.addEventListener("click", () => quickAdvanceStatus(Number(button.dataset.advance)));
    });
}

function openCreateModal() {
    currentModalMode = "create";
    editingId = 0;
    resetModalForm();
    syncModeState();
    document.getElementById("masterCardModal").classList.remove("hidden");
}

function openEditModal(id) {
    const row = state.rows.find(item => item.id === id);
    if (!row) return;

    currentModalMode = "edit";
    editingId = row.id;
    fillEditModal(row);
    syncModeState();
    document.getElementById("masterCardModal").classList.remove("hidden");
    send("load_details", { id: row.id });
}

function openViewModal(id) {
    const row = state.rows.find(item => item.id === id);
    if (!row) return;

    viewingId = row.id;
    fillViewModal(row);
    document.getElementById("viewModal").classList.remove("hidden");
    send("load_details", { id: row.id });
}

function fillEditModal(row) {
    document.getElementById("cmbOperator").value = row.operatorCodigoFJ || "";
    document.getElementById("cmbTrainer").value = row.trainerCodigoFJ || "";
    document.getElementById("dtStartDate").value = row.startDate || "";
    document.getElementById("cmbSector").value = String(row.sectorId || "");
    document.getElementById("cmbEquipment").value = String(row.equipmentId || "");
    document.getElementById("txtNotes").value = row.notes || "";
    document.getElementById("txtStatusValue").value = getStatusLabel(row.masterCardStatus);

    setText("metaCreatedBy", getLocalizedValue(row.createdByNamePt, row.createdByNameJp) || "-");
    setText("metaCreatedAt", row.createdAt || "-");
    setText("metaUpdatedAt", row.updatedAt || "-");
    setText("metaConcludedAt", row.concludedAt || "-");
    setText("metaFollowDate", row.followDate || "-");
    setText("metaFinalizedAt", row.finalizedAt || "-");
}

function fillViewModal(row) {
    setText("viewTitle", t("viewTitle")(row.id));
    setText("viewSubtitle", t("viewSubtitle"));
    setText("viewStatus", getStatusLabel(row.masterCardStatus));
    setText("viewOperator", getLocalizedValue(row.operatorNamePt, row.operatorNameJp) || row.operatorCodigoFJ);
    setText("viewTrainer", getLocalizedValue(row.trainerNamePt, row.trainerNameJp) || row.trainerCodigoFJ);
    setText("viewStartDate", row.startDate || "-");
    setText("viewSector", getLocalizedValue(row.sectorNamePt, row.sectorNameJp) || "-");
    setText("viewEquipment", getLocalizedValue(row.equipmentNamePt, row.equipmentNameJp) || "-");
    setText("viewFollowDate", row.followDate || "-");
    setText("viewFinalizedAt", row.finalizedAt || "-");
    setText("viewDescription", getLocalizedValue(row.equipmentNamePt, row.equipmentNameJp) || "-");
    setText("viewNotes", row.notes || t("noNotes"));
}

function syncModals() {
    if (editingId && !document.getElementById("masterCardModal").classList.contains("hidden")) {
        const row = state.rows.find(item => item.id === editingId);
        if (!row) {
            closeModal();
        } else {
            fillEditModal(row);
            syncModeState();
        }
    }

    if (viewingId && !document.getElementById("viewModal").classList.contains("hidden")) {
        const row = state.rows.find(item => item.id === viewingId);
        if (!row) {
            closeViewModal();
        } else {
            fillViewModal(row);
        }
    }
}

function handleDetails(msg) {
    const detail = normalizeRows([msg.detail || {}])[0];
    if (!detail) return;

    detail.history = msg.history || [];
    const index = state.rows.findIndex(item => item.id === detail.id);
    if (index >= 0) {
        state.rows[index] = { ...state.rows[index], ...detail, history: msg.history || [] };
    }

    if (editingId === detail.id) {
        fillEditModal(detail);
        renderHistoryList("historyList", msg.history || []);
        syncModeState();
    }

    if (viewingId === detail.id) {
        fillViewModal(detail);
        renderHistoryList("viewHistoryList", msg.history || []);
    }
}

function handleAdvanceStatus() {
    if (!editingId) return;

    const row = state.rows.find(item => item.id === editingId);
    if (!row) return;

    const confirmMessage = row.masterCardStatus === "in_progress"
        ? t("confirmConclude")(editingId)
        : t("confirmFinalize")(editingId);

    if (!confirm(confirmMessage)) {
        return;
    }

    send("advance_status", { id: editingId });
}

function quickAdvanceStatus(id) {
    const row = state.rows.find(item => item.id === id);
    if (!row || row.masterCardStatus === "completed") return;

    const confirmMessage = row.masterCardStatus === "in_progress"
        ? t("confirmConclude")(id)
        : t("confirmFinalize")(id);

    if (!confirm(confirmMessage)) {
        return;
    }

    send("advance_status", { id });
}

function submitModal() {
    const payload = buildPayload();

    if (currentModalMode === "edit") {
        send("update", { id: editingId, ...payload });
        return;
    }

    send("save", payload);
}

function buildPayload() {
    return {
        operatorCodigoFJ: document.getElementById("cmbOperator").value,
        trainerCodigoFJ: document.getElementById("cmbTrainer").value,
        startDate: document.getElementById("dtStartDate").value,
        sectorId: Number(document.getElementById("cmbSector").value || 0),
        equipmentId: Number(document.getElementById("cmbEquipment").value || 0),
        notes: document.getElementById("txtNotes").value.trim()
    };
}

function renderHistoryList(containerId, items) {
    const container = document.getElementById(containerId);

    if (!items.length) {
        container.innerHTML = `<div class="history-empty">${escapeHtml(t("historyEmpty"))}</div>`;
        return;
    }

    container.innerHTML = items.map(item => {
        const previousLabel = state.locale === "ja-JP"
            ? (item.previousStatusLabelJp || "")
            : (item.previousStatusLabelPt || "");
        const newLabel = state.locale === "ja-JP"
            ? (item.newStatusLabelJp || "")
            : (item.newStatusLabelPt || "");
        const changedBy = getLocalizedValue(item.changedByNamePt, item.changedByNameJp) || "-";
        const description = previousLabel
            ? t("historyFromTo")(previousLabel, newLabel)
            : t("historyInitial")(newLabel);

        return `
            <article class="history-item">
                <div class="history-head">
                    <strong>${escapeHtml(newLabel || "-")}</strong>
                    <span>${escapeHtml(item.changedAt || "-")}</span>
                </div>
                <p>${escapeHtml(description)}</p>
                <small>${escapeHtml(t("historyBy"))} ${escapeHtml(changedBy)}${item.note ? ` | ${escapeHtml(item.note)}` : ""}</small>
            </article>
        `;
    }).join("");
}

function resetModalForm() {
    document.getElementById("cmbOperator").value = "";
    document.getElementById("cmbTrainer").value = "";
    document.getElementById("dtStartDate").value = "";
    document.getElementById("cmbSector").value = "";
    document.getElementById("cmbEquipment").value = "";
    document.getElementById("txtNotes").value = "";
    document.getElementById("txtStatusValue").value = getStatusLabel("in_progress");
    setText("metaCreatedBy", "-");
    setText("metaCreatedAt", "-");
    setText("metaUpdatedAt", "-");
    setText("metaConcludedAt", "-");
    setText("metaFollowDate", "-");
    setText("metaFinalizedAt", "-");
    renderHistoryList("historyList", []);
}

function closeModal() {
    document.getElementById("masterCardModal").classList.add("hidden");
    currentModalMode = "create";
    editingId = 0;
    resetModalForm();
    syncModeState();
}

function closeViewModal() {
    viewingId = 0;
    document.getElementById("viewModal").classList.add("hidden");
    setText("viewDescription", "-");
    setText("viewNotes", "-");
    renderHistoryList("viewHistoryList", []);
}

function refillLookupControls() {
    fillSelect("statusFilter", state.statuses, "value", statusFieldName(), t("selectAll"));
    fillSelect("cmbOperator", state.operators, "codigoFJ", localizedFieldName(), t("selectPlaceholder"));
    fillSelect("cmbTrainer", state.trainers, "codigoFJ", localizedFieldName(), t("selectPlaceholder"));
    fillSelect("cmbSector", state.sectors, "id", localizedFieldName(), t("selectPlaceholder"));
    fillSelect("cmbEquipment", state.equipments, "id", localizedFieldName(), t("selectPlaceholder"));
}

function fillSelect(id, items, valueField, textField, firstLabel) {
    const select = document.getElementById(id);
    const current = select.value;
    const options = [`<option value="">${escapeHtml(firstLabel)}</option>`];

    items.forEach(item => {
        options.push(`<option value="${escapeHtmlAttr(item[valueField])}">${escapeHtml(item[textField])}</option>`);
    });

    select.innerHTML = options.join("");
    if (current) {
        select.value = current;
    }
}

function syncModeState() {
    const isEdit = currentModalMode === "edit" && !!editingId;
    const row = isEdit ? state.rows.find(item => item.id === editingId) : null;
    const advanceButton = document.getElementById("btnAdvanceStatus");

    setText("formTitle", isEdit ? t("editTitle")(editingId) : t("createTitle"));
    setText("formSubtitle", isEdit ? t("editSubtitle")(editingId) : t("createSubtitle"));
    setText("btnSaveModal", isEdit ? t("saveChanges") : t("createCard"));
    document.getElementById("metaSummary").classList.toggle("hidden", !isEdit);
    document.getElementById("historySection").classList.toggle("hidden", !isEdit);

    if (!row || row.masterCardStatus === "completed") {
        advanceButton.classList.add("hidden");
        advanceButton.textContent = t("conclude");
    } else {
        advanceButton.classList.remove("hidden");
        advanceButton.textContent = row.masterCardStatus === "in_progress"
            ? t("conclude")
            : t("finalize");
    }
}

function setLocale(locale) {
    state.locale = locale === "ja-JP" ? "ja-JP" : "pt-BR";
    document.documentElement.lang = state.locale;
    document.title = t("title");

    setText("txtHeaderTitle", t("headerTitle"));
    setText("txtEyebrow", t("eyebrow"));
    setText("txtHeroTitle", t("heroTitle"));
    setText("txtHeroText", t("heroText"));
    setText("txtMetricInProgress", t("metricInProgress"));
    setText("txtMetricFollow", t("metricFollow"));
    setText("txtMetricCompleted", t("metricCompleted"));
    setText("txtTableTitle", t("tableTitle"));
    setText("txtShowCompleted", t("showCompleted"));
    setText("txtStatusLabel", t("statusLabel"));
    setText("txtSearchLabel", t("searchLabel"));
    document.getElementById("searchInput").placeholder = t("searchPlaceholder");
    setText("btnHeaderNovo", t("newCard"));
    setText("txtOperatorLabel", t("operatorLabel"));
    setText("txtTrainerLabel", t("trainerLabel"));
    setText("txtStartDateLabel", t("startDateLabel"));
    setText("txtSectorLabel", t("sectorLabel"));
    setText("txtEquipmentLabel", t("equipmentLabel"));
    setText("txtStatusValueLabel", t("statusValueLabel"));
    setText("txtNotesLabel", t("notesLabel"));
    document.getElementById("txtNotes").placeholder = t("notesPlaceholder");
    setText("txtMetaCreatedBy", t("metaCreatedBy"));
    setText("txtMetaCreatedAt", t("metaCreatedAt"));
    setText("txtMetaUpdatedAt", t("metaUpdatedAt"));
    setText("txtMetaConcludedAt", t("metaConcludedAt"));
    setText("txtMetaFollowDate", t("metaFollowDate"));
    setText("txtMetaFinalizedAt", t("metaFinalizedAt"));
    setText("txtHistoryTitle", t("historyTitle"));
    setText("txtHistorySubtitle", t("historySubtitle"));
    setText("btnCancelModal", t("cancel"));
    document.getElementById("btnCloseModal").setAttribute("aria-label", t("closeModalAria"));

    setText("txtViewStatus", t("viewStatus"));
    setText("txtViewOperator", t("viewOperator"));
    setText("txtViewTrainer", t("viewTrainer"));
    setText("txtViewStartDate", t("viewStartDate"));
    setText("txtViewSector", t("viewSector"));
    setText("txtViewEquipment", t("viewEquipment"));
    setText("txtViewFollowDate", t("viewFollowDate"));
    setText("txtViewFinalizedAt", t("viewFinalizedAt"));
    setText("txtViewDescriptionTitle", t("viewDescriptionTitle"));
    setText("txtViewDescriptionSubtitle", t("viewDescriptionSubtitle"));
    setText("txtViewHistoryTitle", t("viewHistoryTitle"));
    setText("txtViewHistorySubtitle", t("viewHistorySubtitle"));
    setText("btnCloseView", t("close"));
    setText("btnEditFromView", t("editCard"));
    document.getElementById("btnCloseViewModal").setAttribute("aria-label", t("closeModalAria"));

    const currentOperatorName = getLocalizedValue(state.currentOperatorNamePt, state.currentOperatorNameJp) || "-";
    setText("screenSubtitle", `${t("subtitlePrefix")}: ${currentOperatorName} | ${t("subtitleUser")}: ${state.currentUser || "-"}`);

    refillLookupControls();
    syncModeState();
    renderMetrics(state.rows);
    applyFilters();
}

function getLocalizedValue(pt, jp) {
    return state.locale === "ja-JP" ? (jp || pt || "") : (pt || jp || "");
}

function localizedFieldName() {
    return state.locale === "ja-JP" ? "nameJp" : "namePt";
}

function statusFieldName() {
    return state.locale === "ja-JP" ? "labelJp" : "labelPt";
}

function getStatusLabel(status, locale = state.locale) {
    const match = state.statuses.find(item => item.value === status);
    if (!match) return status || "";
    return locale === "ja-JP" ? match.labelJp : match.labelPt;
}

function statusClass(status) {
    return {
        in_progress: "status-progress",
        follow: "status-blocked",
        completed: "status-completed"
    }[status] || "status-pending";
}

function t(key) {
    return I18N[state.locale]?.[key] ?? I18N["pt-BR"][key];
}

function setText(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = value ?? "-";
    }
}

function truncate(value, size) {
    const text = String(value || "");
    return text.length > size ? `${text.slice(0, size - 1)}...` : text;
}

function iconEye() {
    return `
        <svg viewBox="0 0 24 24" aria-hidden="true">
            <path d="M1.5 12s3.8-6 10.5-6 10.5 6 10.5 6-3.8 6-10.5 6S1.5 12 1.5 12Z"></path>
            <circle cx="12" cy="12" r="3.2"></circle>
        </svg>
    `;
}

function iconEdit() {
    return `
        <svg viewBox="0 0 24 24" aria-hidden="true">
            <path d="M4 16.7V20h3.3L18.7 8.6l-3.3-3.3L4 16.7Z"></path>
            <path d="M13.9 4.7l3.3 3.3"></path>
        </svg>
    `;
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
