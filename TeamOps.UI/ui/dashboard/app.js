const ACCESS_LABELS = {
    "pt-BR": {
        1: "Basic",
        2: "KL",
        3: "GL",
        4: "Manager",
        5: "Admin"
    },
    "ja-JP": {
        1: "Basic",
        2: "KL",
        3: "GL",
        4: "Manager",
        5: "Admin"
    }
};

const PERMISSION_RULES = {
    btnRelatorios: 3,
    btnPresenca: 3,
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

const I18N = {
    "pt-BR": {
        htmlLang: "pt-BR",
        pageTitle: "TeamOps - Dashboard",
        headerTitle: "TeamOps Dashboard",
        headerSubtitle: "Central de operacoes e acompanhamento da equipe",
        metaUser: "Usuario",
        metaOperator: "Operador",
        metaShift: "Turno",
        heroTitle: "Atalhos principais do time",
        heroSubtitle: "Mesmo fluxo do dashboard atual, agora em HTML com layout mais leve e organizado.",
        statDate: "Data e Hora",
        statProfile: "Perfil Atual",
        statTasks: "Tasks em Aberto",
        groupOpsTitle: "Operacao",
        groupOpsSubtitle: "Cadastros, acompanhamento e consulta de registros.",
        groupDocsTitle: "Hikitsugui e Documentos",
        groupDocsSubtitle: "Fluxos de comunicacao e documentos de processo.",
        groupPresenceTitle: "Presenca",
        groupPresenceSubtitle: "Acesso rapido aos paineis por setor.",
        groupAdminTitle: "Administracao",
        groupAdminSubtitle: "Configuracoes e controle de acesso.",
        tileOperatorsTitle: "Operadores",
        tileOperatorsSubtitle: "Cadastro e consulta",
        tileFollowTitle: "Acompanhamento",
        tileFollowSubtitle: "Registro e orientacao",
        tileTasksTitle: "Tasks",
        tileTasksSubtitle: "Planejamento do turno",
        tileScrapTitle: "Sobra de Peca",
        tileScrapSubtitle: "Lancamento de perdas",
        tileReportsTitle: "Relatorios",
        tileReportsSubtitle: "Consultas e impressao",
        tileHikiCreateTitle: "Hikitsugui",
        tileHikiCreateSubtitle: "Cadastro rapido",
        tileHikiReadTitle: "Leitura Hikitsugui",
        tileHikiReadSubtitle: "Consulta e resposta",
        tilePrTitle: "PR",
        tilePrSubtitle: "Documento de processo",
        tileClTitle: "CL",
        tileClSubtitle: "Controle de linha",
        tileTodokeTitle: "Todoke",
        tileTodokeSubtitle: "Solicitacoes e folhas",
        tilePresenceTitle: "Presenca",
        tilePresenceSubtitle: "Painel dos setores",
        tileAdminTitle: "Admin",
        tileAdminSubtitle: "Configuracoes do sistema",
        tileAccessTitle: "Acesso",
        tileAccessSubtitle: "Permissoes e usuarios",
        profilePrefix: "Perfil",
        toggleAria: "Alternar idioma para japones"
    },
    "ja-JP": {
        htmlLang: "ja-JP",
        pageTitle: "\u30c1\u30fc\u30e0\u30aa\u30d7\u30b9 - \u30c0\u30c3\u30b7\u30e5\u30dc\u30fc\u30c9",
        headerTitle: "\u30c1\u30fc\u30e0\u904b\u7528\u30c0\u30c3\u30b7\u30e5\u30dc\u30fc\u30c9",
        headerSubtitle: "\u30c1\u30fc\u30e0\u306e\u904b\u7528\u3068\u30d5\u30a9\u30ed\u30fc\u3092\u307e\u3068\u3081\u3066\u78ba\u8a8d\u3067\u304d\u307e\u3059\u3002",
        metaUser: "\u30e6\u30fc\u30b6\u30fc",
        metaOperator: "\u4f5c\u696d\u8005",
        metaShift: "\u30b7\u30d5\u30c8",
        heroTitle: "\u4e3b\u8981\u30b7\u30e7\u30fc\u30c8\u30ab\u30c3\u30c8",
        heroSubtitle: "\u73fe\u5728\u306e\u30c0\u30c3\u30b7\u30e5\u30dc\u30fc\u30c9\u306e\u6d41\u308c\u306f\u305d\u306e\u307e\u307e\u306b\u3001HTML\u3067\u3088\u308a\u8efd\u304f\u6574\u7406\u3057\u305f\u69cb\u6210\u3067\u3059\u3002",
        statDate: "\u65e5\u6642",
        statProfile: "\u73fe\u5728\u306e\u6a29\u9650",
        statTasks: "\u672a\u5b8c\u4e86\u30bf\u30b9\u30af",
        groupOpsTitle: "\u904b\u7528",
        groupOpsSubtitle: "\u767b\u9332\u3001\u30d5\u30a9\u30ed\u30fc\u3001\u8a18\u9332\u78ba\u8a8d\u3002",
        groupDocsTitle: "\u5f15\u7d99\u304e\u3068\u6587\u66f8",
        groupDocsSubtitle: "\u9023\u7d61\u30d5\u30ed\u30fc\u3068\u5de5\u7a0b\u95a2\u4fc2\u306e\u66f8\u985e\u3002",
        groupPresenceTitle: "\u51fa\u52e4",
        groupPresenceSubtitle: "\u30bb\u30af\u30bf\u30fc\u5225\u30d1\u30cd\u30eb\u3078\u7d20\u65e9\u304f\u79fb\u52d5\u3002",
        groupAdminTitle: "\u7ba1\u7406",
        groupAdminSubtitle: "\u8a2d\u5b9a\u3068\u30a2\u30af\u30bb\u30b9\u7ba1\u7406\u3002",
        tileOperatorsTitle: "\u4f5c\u696d\u8005",
        tileOperatorsSubtitle: "\u767b\u9332\u3068\u691c\u7d22",
        tileFollowTitle: "\u30d5\u30a9\u30ed\u30fc",
        tileFollowSubtitle: "\u8a18\u9332\u3068\u6307\u5c0e",
        tileTasksTitle: "\u30bf\u30b9\u30af",
        tileTasksSubtitle: "\u30b7\u30d5\u30c8\u8a08\u753b",
        tileScrapTitle: "\u88fd\u54c1\u6b8b\u308a",
        tileScrapSubtitle: "\u30ed\u30b9\u5165\u529b",
        tileReportsTitle: "\u30ec\u30dd\u30fc\u30c8",
        tileReportsSubtitle: "\u691c\u7d22\u3068\u5370\u5237",
        tileHikiCreateTitle: "\u5f15\u7d99\u304e",
        tileHikiCreateSubtitle: "\u65b0\u898f\u767b\u9332",
        tileHikiReadTitle: "\u5f15\u7d99\u304e\u95b2\u89a7",
        tileHikiReadSubtitle: "\u78ba\u8a8d\u3068\u8fd4\u4fe1",
        tilePrTitle: "PR",
        tilePrSubtitle: "\u5de5\u7a0b\u66f8\u985e",
        tileClTitle: "CL",
        tileClSubtitle: "\u30e9\u30a4\u30f3\u7ba1\u7406",
        tileTodokeTitle: "\u5c4a\u51fa",
        tileTodokeSubtitle: "\u7533\u8acb\u3068\u66f8\u985e",
        tilePresenceTitle: "\u51fa\u52e4",
        tilePresenceSubtitle: "\u30bb\u30af\u30bf\u30fc\u30d1\u30cd\u30eb",
        tileAdminTitle: "\u7ba1\u7406",
        tileAdminSubtitle: "\u30b7\u30b9\u30c6\u30e0\u8a2d\u5b9a",
        tileAccessTitle: "\u30a2\u30af\u30bb\u30b9",
        tileAccessSubtitle: "\u6a29\u9650\u3068\u30e6\u30fc\u30b6\u30fc",
        profilePrefix: "\u6a29\u9650",
        toggleAria: "\u8a00\u8a9e\u3092\u30dd\u30eb\u30c8\u30ac\u30eb\u8a9e\u306b\u5207\u308a\u66ff\u3048"
    }
};

const state = {
    locale: "pt-BR",
    payload: null
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

    document.getElementById("btnLocaleToggle").addEventListener("click", () => {
        const nextLocale = state.locale === "ja-JP" ? "pt-BR" : "ja-JP";

        setLocale(nextLocale);

        window.chrome.webview.postMessage({
            action: "set_locale",
            locale: nextLocale
        });
    });
}

window.chrome?.webview?.addEventListener("message", event => {
    const payload = event.data;

    if (!payload?.type) {
        return;
    }

    if (payload.type === "init") {
        hydrateDashboard(payload);
        return;
    }

    if (payload.type === "locale_changed" && payload.locale) {
        setLocale(payload.locale);
    }
});

function hydrateDashboard(payload) {
    state.payload = payload;

    setLocale(payload.locale || "pt-BR");
    setText("lblUser", payload.user);
    refreshNames();
    setText("lblDate", formatDashboardDate(payload.dateIso, state.locale));
    setText("lblOpenTasks", payload.openTasksForShift ?? 0);

    const level = Number(payload.accessLevel ?? 0);
    const label = ACCESS_LABELS[state.locale]?.[level] || `Nivel ${level}`;
    const profilePrefix = I18N[state.locale]?.profilePrefix || "Perfil";

    setText("lblAccessLevel", label);
    setText("accessBadge", `${profilePrefix} ${label}`);

    applyPermissions(level);
}

function refreshNames() {
    if (!state.payload) {
        return;
    }

    const operatorName = state.locale === "ja-JP"
        ? (state.payload.operatorNameJp || state.payload.operatorNamePt)
        : (state.payload.operatorNamePt || state.payload.operatorNameJp);

    const shiftName = state.locale === "ja-JP"
        ? (state.payload.shiftNameJp || state.payload.shiftNamePt)
        : (state.payload.shiftNamePt || state.payload.shiftNameJp);

    setText("lblOperator", operatorName);
    setText("lblShift", shiftName);
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

function setLocale(locale) {
    state.locale = locale === "ja-JP" ? "ja-JP" : "pt-BR";

    const strings = I18N[state.locale];
    document.documentElement.lang = strings.htmlLang;
    document.title = strings.pageTitle;

    const toggle = document.getElementById("btnLocaleToggle");
    toggle.classList.toggle("locale-ja", state.locale === "ja-JP");
    toggle.classList.toggle("locale-pt", state.locale !== "ja-JP");
    toggle.setAttribute("aria-label", strings.toggleAria);

    setText("txtHeaderTitle", strings.headerTitle);
    setText("txtHeaderSubtitle", strings.headerSubtitle);
    setText("txtMetaUser", strings.metaUser);
    setText("txtMetaOperator", strings.metaOperator);
    setText("txtMetaShift", strings.metaShift);
    setText("txtHeroTitle", strings.heroTitle);
    setText("txtHeroSubtitle", strings.heroSubtitle);
    setText("txtStatDate", strings.statDate);
    setText("txtStatProfile", strings.statProfile);
    setText("txtStatTasks", strings.statTasks);
    setText("txtGroupOpsTitle", strings.groupOpsTitle);
    setText("txtGroupOpsSubtitle", strings.groupOpsSubtitle);
    setText("txtGroupDocsTitle", strings.groupDocsTitle);
    setText("txtGroupDocsSubtitle", strings.groupDocsSubtitle);
    setText("txtGroupPresenceTitle", strings.groupPresenceTitle);
    setText("txtGroupPresenceSubtitle", strings.groupPresenceSubtitle);
    setText("txtGroupAdminTitle", strings.groupAdminTitle);
    setText("txtGroupAdminSubtitle", strings.groupAdminSubtitle);
    setText("txtTileOperatorsTitle", strings.tileOperatorsTitle);
    setText("txtTileOperatorsSubtitle", strings.tileOperatorsSubtitle);
    setText("txtTileFollowTitle", strings.tileFollowTitle);
    setText("txtTileFollowSubtitle", strings.tileFollowSubtitle);
    setText("txtTileTasksTitle", strings.tileTasksTitle);
    setText("txtTileTasksSubtitle", strings.tileTasksSubtitle);
    setText("txtTileScrapTitle", strings.tileScrapTitle);
    setText("txtTileScrapSubtitle", strings.tileScrapSubtitle);
    setText("txtTileReportsTitle", strings.tileReportsTitle);
    setText("txtTileReportsSubtitle", strings.tileReportsSubtitle);
    setText("txtTileHikiCreateTitle", strings.tileHikiCreateTitle);
    setText("txtTileHikiCreateSubtitle", strings.tileHikiCreateSubtitle);
    setText("txtTileHikiReadTitle", strings.tileHikiReadTitle);
    setText("txtTileHikiReadSubtitle", strings.tileHikiReadSubtitle);
    setText("txtTilePrTitle", strings.tilePrTitle);
    setText("txtTilePrSubtitle", strings.tilePrSubtitle);
    setText("txtTileClTitle", strings.tileClTitle);
    setText("txtTileClSubtitle", strings.tileClSubtitle);
    setText("txtTileTodokeTitle", strings.tileTodokeTitle);
    setText("txtTileTodokeSubtitle", strings.tileTodokeSubtitle);
    setText("txtTilePresenceTitle", strings.tilePresenceTitle);
    setText("txtTilePresenceSubtitle", strings.tilePresenceSubtitle);
    setText("txtTileAdminTitle", strings.tileAdminTitle);
    setText("txtTileAdminSubtitle", strings.tileAdminSubtitle);
    setText("txtTileAccessTitle", strings.tileAccessTitle);
    setText("txtTileAccessSubtitle", strings.tileAccessSubtitle);

    if (state.payload) {
        refreshNames();
        setText("lblDate", formatDashboardDate(state.payload.dateIso, state.locale));

        const level = Number(state.payload.accessLevel ?? 0);
        const label = ACCESS_LABELS[state.locale]?.[level] || `Nivel ${level}`;
        setText("lblAccessLevel", label);
        setText("accessBadge", `${strings.profilePrefix} ${label}`);
    }
}

function formatDashboardDate(dateIso, locale) {
    if (!dateIso) {
        return "-";
    }

    const date = new Date(dateIso);
    if (Number.isNaN(date.getTime())) {
        return "-";
    }

    return new Intl.DateTimeFormat(locale, {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit"
    }).format(date);
}

function setText(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = value ?? "-";
    }
}
