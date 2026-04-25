const state = {
    locale: "pt-BR",
    entities: {},
    entityOrder: [],
    lookups: {},
    currentEntity: "",
    rows: [],
    filteredRows: [],
    modalMode: "create",
    editingId: 0
};

const I18N = {
    "pt-BR": {
        title: "Administracao",
        headerTitle: "Administracao",
        headerSubtitle: "Cadastros administrativos concentrados em um unico painel, com edicao em modal.",
        metaEntity: "Cadastro ativo",
        metaTotal: "Registros",
        sidebarTitle: "Itens",
        sidebarSubtitle: "Selecione o cadastro que deseja manter.",
        badge: "Painel administrativo",
        searchLabel: "Buscar",
        searchPlaceholder: "Buscar por nome...",
        new: "Novo",
        tableTitle: "Registros",
        tableSubtitle: "Edite ou exclua direto da lista para manter o fluxo rapido.",
        tableReadonlySubtitle: "Consulta simples do historico, sem permitir inclusao, edicao ou exclusao.",
        actions: "Acoes",
        actionEdit: "Editar",
        actionDelete: "Excluir",
        loading: "Carregando...",
        empty: "Nenhum registro encontrado.",
        modalCreateTitle: entity => `Novo ${entity}`,
        modalEditTitle: entity => `Editar ${entity}`,
        modalCreateSubtitle: "Preencha os campos e salve.",
        modalEditSubtitle: "Ajuste os dados e salve as alteracoes.",
        cancel: "Cancelar",
        save: "Salvar",
        readonlyBadge: "Somente leitura",
        selectPlaceholder: "Selecione...",
        confirmDelete: entity => `Deseja excluir este registro de ${entity}?`,
        saved: "Cadastro salvo com sucesso.",
        updated: "Cadastro atualizado com sucesso.",
        deleted: "Cadastro excluido com sucesso."
    },
    "ja-JP": {
        title: "\u7ba1\u7406",
        headerTitle: "\u7ba1\u7406",
        headerSubtitle: "\u8907\u6570\u306e\u7ba1\u7406\u30de\u30b9\u30bf\u3092\u4e00\u3064\u306e\u753b\u9762\u306b\u96c6\u7d04\u3057\u3001\u30e2\u30fc\u30c0\u30eb\u3067\u7de8\u96c6\u3067\u304d\u307e\u3059\u3002",
        metaEntity: "\u9078\u629e\u4e2d",
        metaTotal: "\u4ef6\u6570",
        sidebarTitle: "\u9805\u76ee",
        sidebarSubtitle: "\u7ba1\u7406\u3059\u308b\u30de\u30b9\u30bf\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        badge: "\u7ba1\u7406\u30d1\u30cd\u30eb",
        searchLabel: "\u691c\u7d22",
        searchPlaceholder: "\u540d\u79f0\u3067\u691c\u7d22...",
        new: "\u65b0\u898f",
        tableTitle: "\u30ec\u30b3\u30fc\u30c9",
        tableSubtitle: "\u30ea\u30b9\u30c8\u304b\u3089\u3059\u3070\u3084\u304f\u7de8\u96c6\u30fb\u524a\u9664\u3067\u304d\u307e\u3059\u3002",
        tableReadonlySubtitle: "\u5c65\u6b74\u3092\u53c2\u7167\u3059\u308b\u305f\u3081\u306e\u4e00\u89a7\u3067\u3001\u8ffd\u52a0\u30fb\u7de8\u96c6\u30fb\u524a\u9664\u306f\u3067\u304d\u307e\u305b\u3093\u3002",
        actions: "\u64cd\u4f5c",
        actionEdit: "\u7de8\u96c6",
        actionDelete: "\u524a\u9664",
        loading: "\u8aad\u307f\u8fbc\u307f\u4e2d...",
        empty: "\u30ec\u30b3\u30fc\u30c9\u304c\u3042\u308a\u307e\u305b\u3093\u3002",
        modalCreateTitle: entity => `${entity} \u3092\u65b0\u898f\u767b\u9332`,
        modalEditTitle: entity => `${entity} \u3092\u7de8\u96c6`,
        modalCreateSubtitle: "\u5fc5\u8981\u9805\u76ee\u3092\u5165\u529b\u3057\u3066\u4fdd\u5b58\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        modalEditSubtitle: "\u5185\u5bb9\u3092\u4fee\u6b63\u3057\u3066\u4fdd\u5b58\u3057\u3066\u304f\u3060\u3055\u3044\u3002",
        cancel: "\u30ad\u30e3\u30f3\u30bb\u30eb",
        save: "\u4fdd\u5b58",
        readonlyBadge: "\u95b2\u89a7\u306e\u307f",
        selectPlaceholder: "\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044",
        confirmDelete: entity => `${entity} \u306e\u3053\u306e\u30ec\u30b3\u30fc\u30c9\u3092\u524a\u9664\u3057\u307e\u3059\u304b\u3002`,
        saved: "\u767b\u9332\u3092\u4fdd\u5b58\u3057\u307e\u3057\u305f\u3002",
        updated: "\u767b\u9332\u3092\u66f4\u65b0\u3057\u307e\u3057\u305f\u3002",
        deleted: "\u767b\u9332\u3092\u524a\u9664\u3057\u307e\u3057\u305f\u3002"
    }
};

window.addEventListener("DOMContentLoaded", () => {
    bindEvents();
    send("load");
});

function bindEvents() {
    document.getElementById("searchInput").addEventListener("input", applySearch);
    document.getElementById("btnNew").addEventListener("click", () => openModal("create"));
    document.getElementById("btnCancelModal").addEventListener("click", closeModal);
    document.getElementById("btnCloseModalX").addEventListener("click", closeModal);
    document.getElementById("modalBackdrop").addEventListener("click", closeModal);
    document.getElementById("btnSaveModal").addEventListener("click", saveModal);
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
            hydrateInit(msg.data || {});
            break;
        case "entity_rows":
            hydrateRows(msg.data || {});
            break;
        case "saved":
        case "updated":
        case "deleted":
            alert(msg.message || "");
            closeModal();
            break;
        case "error":
            alert(msg.message || "Erro");
            break;
    }
});

function hydrateInit(data) {
    state.locale = data.locale === "ja-JP" ? "ja-JP" : "pt-BR";
    state.lookups = normalizeLookups(data.lookups || {});
    state.entities = {};
    state.entityOrder = [];

    (data.entities || []).forEach(entity => {
        state.entities[entity.key] = normalizeEntity(entity);
        state.entityOrder.push(entity.key);
    });

    state.currentEntity = data.activeEntity || state.entityOrder[0] || "";

    applyLocale();
    renderEntityList();

    if (state.currentEntity) {
        send("load_entity", { entity: state.currentEntity });
    }
}

function hydrateRows(data) {
    if (data.lookups) {
        state.lookups = normalizeLookups(data.lookups);
    }

    state.currentEntity = data.entity || state.currentEntity;
    state.rows = Array.isArray(data.rows) ? data.rows : [];
    applyLocale();
    renderEntityList();
    applySearch();
}

function normalizeEntity(entity) {
    return {
        key: entity.key,
        group: entity.group || "misc",
        readOnly: !!entity.readOnly,
        titlePt: entity.titlePt || entity.key,
        titleJp: entity.titleJp || entity.titlePt || entity.key,
        descriptionPt: entity.descriptionPt || "",
        descriptionJp: entity.descriptionJp || entity.descriptionPt || "",
        fields: entity.fields || [],
        columns: entity.columns || []
    };
}

function normalizeLookups(lookups) {
    const normalized = {};

    Object.keys(lookups || {}).forEach(key => {
        normalized[key] = (lookups[key] || []).map(item => ({
            id: Number(item.id || 0),
            namePt: item.namePt || "",
            nameJp: item.nameJp || item.namePt || ""
        }));
    });

    return normalized;
}

function applyLocale() {
    document.documentElement.lang = state.locale;
    document.title = t("title");

    setText("txtHeaderTitle", t("headerTitle"));
    setText("txtHeaderSubtitle", t("headerSubtitle"));
    setText("txtMetaEntity", t("metaEntity"));
    setText("txtMetaTotal", t("metaTotal"));
    setText("txtSidebarTitle", t("sidebarTitle"));
    setText("txtSidebarSubtitle", t("sidebarSubtitle"));
    setText("txtBadge", t("badge"));
    setText("txtSearchLabel", t("searchLabel"));
    document.getElementById("searchInput").placeholder = t("searchPlaceholder");
    setText("btnNew", t("new"));
    setText("txtTableTitle", t("tableTitle"));
    setText("btnCancelModal", t("cancel"));
    setText("btnSaveModal", t("save"));

    const entity = currentEntity();
    setText("entityTitle", entity ? entityTitle(entity) : "-");
    setText("entityDescription", entity ? entityDescription(entity) : "-");
    setText("lblActiveEntity", entity ? entityTitle(entity) : "-");
    setText("lblTotalRows", state.rows.length);

    const tableSubtitle = entity?.readOnly ? t("tableReadonlySubtitle") : t("tableSubtitle");
    setText("txtTableSubtitle", tableSubtitle);

    const btnNew = document.getElementById("btnNew");
    btnNew.classList.toggle("hidden", !!entity?.readOnly);
    btnNew.disabled = !!entity?.readOnly;
}

function renderEntityList() {
    const container = document.getElementById("entityList");
    const groups = {};

    state.entityOrder.forEach(key => {
        const entity = state.entities[key];
        if (!entity) return;
        if (!groups[entity.group]) groups[entity.group] = [];
        groups[entity.group].push(entity);
    });

    const order = ["core", "production", "followup", "people", "misc"];

    container.innerHTML = order
        .filter(group => groups[group]?.length)
        .map(group => `
            <section class="entity-group">
                <h3>${escapeHtml(groupLabel(group))}</h3>
                <div class="entity-group-list">
                    ${groups[group].map(entity => `
                        <button
                            type="button"
                            class="entity-card ${entity.key === state.currentEntity ? "is-active" : ""}"
                            data-entity="${entity.key}">
                            <strong>${escapeHtml(entityTitle(entity))}</strong>
                            <small>${escapeHtml(entityDescription(entity))}</small>
                        </button>
                    `).join("")}
                </div>
            </section>
        `).join("");

    container.querySelectorAll("[data-entity]").forEach(button => {
        button.addEventListener("click", () => {
            const key = button.dataset.entity;
            if (!key || key === state.currentEntity) return;
            state.currentEntity = key;
            state.rows = [];
            applyLocale();
            renderEntityList();
            renderTable();
            send("load_entity", { entity: key });
        });
    });
}

function renderTable() {
    const entity = currentEntity();
    const head = document.getElementById("tableHeadRow");
    const body = document.getElementById("tableBody");

    if (!entity) {
        head.innerHTML = "";
        body.innerHTML = `<tr><td class="empty-cell" colspan="4">${escapeHtml(t("empty"))}</td></tr>`;
        return;
    }

    head.innerHTML = `
        ${entity.columns.map(column => `<th>${escapeHtml(columnLabel(column))}</th>`).join("")}
        ${entity.readOnly ? "" : `<th class="actions-col">${escapeHtml(t("actions"))}</th>`}
    `;

    if (!state.filteredRows.length) {
        body.innerHTML = `<tr><td class="empty-cell" colspan="${entity.columns.length + (entity.readOnly ? 0 : 1)}">${escapeHtml(state.rows.length ? t("empty") : t("loading"))}</td></tr>`;
        return;
    }

    body.innerHTML = state.filteredRows.map(row => `
        <tr>
            ${entity.columns.map(column => `<td>${escapeHtml(displayValue(row, column))}</td>`).join("")}
            ${entity.readOnly ? "" : `
                <td class="actions-col">
                    <div class="action-buttons">
                        <button class="icon-btn icon-btn-edit" type="button" data-edit="${row.id}" title="${escapeHtmlAttr(t("actionEdit"))}" aria-label="${escapeHtmlAttr(t("actionEdit"))}">
                            ${iconEdit()}
                        </button>
                        <button class="icon-btn icon-btn-delete" type="button" data-delete="${row.id}" title="${escapeHtmlAttr(t("actionDelete"))}" aria-label="${escapeHtmlAttr(t("actionDelete"))}">
                            ${iconDelete()}
                        </button>
                    </div>
                </td>
            `}
        </tr>
    `).join("");

    if (entity.readOnly) {
        return;
    }

    body.querySelectorAll("[data-edit]").forEach(button => {
        button.addEventListener("click", () => openModal("edit", Number(button.dataset.edit)));
    });

    body.querySelectorAll("[data-delete]").forEach(button => {
        button.addEventListener("click", () => deleteRow(Number(button.dataset.delete)));
    });
}

function applySearch() {
    const term = document.getElementById("searchInput").value.trim().toLowerCase();
    const entity = currentEntity();

    if (!entity) {
        state.filteredRows = [];
        renderTable();
        return;
    }

    if (!term) {
        state.filteredRows = [...state.rows];
        renderTable();
        return;
    }

    state.filteredRows = state.rows.filter(row => {
        return entity.columns.some(column => {
            const value = String(displayValue(row, column) || "").toLowerCase();
            return value.includes(term);
        });
    });

    renderTable();
}

function openModal(mode, id = 0) {
    const entity = currentEntity();
    if (!entity || entity.readOnly) return;

    state.modalMode = mode;
    state.editingId = id;

    const row = mode === "edit"
        ? state.rows.find(item => Number(item.id) === Number(id))
        : null;

    setText("modalTitle", mode === "edit" ? t("modalEditTitle")(entityTitle(entity)) : t("modalCreateTitle")(entityTitle(entity)));
    setText("modalSubtitle", mode === "edit" ? t("modalEditSubtitle") : t("modalCreateSubtitle"));

    document.getElementById("modalFields").innerHTML = entity.fields.map(field => renderField(entity, field, row)).join("");
    document.getElementById("modal").classList.remove("hidden");
}

function renderField(entity, field, row) {
    const value = row?.[field.key] ?? "";
    const label = state.locale === "ja-JP" ? field.labelJp : field.labelPt;

    if (field.type === "select") {
        const options = (state.lookups[field.lookupKey] || [])
            .map(item => {
                const text = state.locale === "ja-JP" ? item.nameJp : item.namePt;
                const selected = Number(value) === Number(item.id) ? "selected" : "";
                return `<option value="${item.id}" ${selected}>${escapeHtml(text)}</option>`;
            })
            .join("");

        return `
            <label class="form-field">
                <span>${escapeHtml(label)}</span>
                <select class="input" data-field="${field.key}">
                    <option value="0">${escapeHtml(t("selectPlaceholder"))}</option>
                    ${options}
                </select>
            </label>
        `;
    }

    return `
        <label class="form-field">
            <span>${escapeHtml(label)}</span>
            <input class="input" type="text" data-field="${field.key}" value="${escapeHtmlAttr(value)}">
        </label>
    `;
}

function saveModal() {
    const entity = currentEntity();
    if (!entity || entity.readOnly) return;

    const values = {};
    document.querySelectorAll("[data-field]").forEach(input => {
        values[input.dataset.field] = input.value;
    });

    if (state.modalMode === "edit" && state.editingId > 0) {
        send("update", {
            entity: entity.key,
            id: state.editingId,
            values
        });
        return;
    }

    send("save", {
        entity: entity.key,
        values
    });
}

function deleteRow(id) {
    const entity = currentEntity();
    if (!entity || entity.readOnly || id <= 0) return;

    if (!confirm(t("confirmDelete")(entityTitle(entity)))) {
        return;
    }

    send("delete", {
        entity: entity.key,
        id
    });
}

function closeModal() {
    document.getElementById("modal").classList.add("hidden");
    state.modalMode = "create";
    state.editingId = 0;
}

function currentEntity() {
    return state.entities[state.currentEntity] || null;
}

function entityTitle(entity) {
    return state.locale === "ja-JP" ? entity.titleJp : entity.titlePt;
}

function entityDescription(entity) {
    return state.locale === "ja-JP" ? entity.descriptionJp : entity.descriptionPt;
}

function columnLabel(column) {
    return state.locale === "ja-JP" ? column.labelJp : column.labelPt;
}

function displayValue(row, column) {
    if (state.locale === "ja-JP" && column.jpKey && row[column.jpKey] !== undefined && row[column.jpKey] !== "") {
        return row[column.jpKey];
    }
    return row[column.key] ?? "";
}

function groupLabel(group) {
    const labels = {
        core: state.locale === "ja-JP" ? "\u57fa\u790e\u30de\u30b9\u30bf" : "Base",
        production: state.locale === "ja-JP" ? "\u751f\u7523\u30de\u30b9\u30bf" : "Producao",
        followup: state.locale === "ja-JP" ? "\u30d5\u30a9\u30ed\u30fc\u30de\u30b9\u30bf" : "Follow",
        people: state.locale === "ja-JP" ? "\u4eba\u54e1" : "Pessoas",
        misc: state.locale === "ja-JP" ? "\u305d\u306e\u4ed6" : "Outros"
    };
    return labels[group] || group;
}

function setText(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = value ?? "-";
    }
}

function t(key) {
    return I18N[state.locale]?.[key] ?? I18N["pt-BR"][key];
}

function iconEdit() {
    return `
        <svg viewBox="0 0 24 24" aria-hidden="true">
            <path d="M4 16.7V20h3.3L18.7 8.6l-3.3-3.3L4 16.7Z"></path>
            <path d="M13.9 4.7l3.3 3.3"></path>
        </svg>
    `;
}

function iconDelete() {
    return `
        <svg viewBox="0 0 24 24" aria-hidden="true">
            <path d="M9 3.75h6l.75 1.5H20v1.5H4V5.25h4.25L9 3.75Z"></path>
            <path d="M6.75 8.25h10.5l-.8 10.1A1.5 1.5 0 0 1 14.95 19.75h-5.9a1.5 1.5 0 0 1-1.49-1.4l-.81-10.1Z"></path>
        </svg>
    `;
}

function escapeHtml(value) {
    return String(value ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
}

function escapeHtmlAttr(value) {
    return escapeHtml(value);
}
