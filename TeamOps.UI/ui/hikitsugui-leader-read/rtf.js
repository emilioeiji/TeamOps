window.RTFJS = {
    parse: function (rtf) {
        if (!rtf || typeof rtf !== "string") return "";

        let txt = rtf;

        // Remover header até o \pard
        let idx = txt.indexOf("\\pard");
        if (idx > -1) {
            txt = txt.substring(idx);
        }

        // Remover \pard completamente
        txt = txt.split("\\pard").join("");

        // Bold
        txt = txt.split("\\b ").join("<strong>");
        txt = txt.split("\\b0").join("</strong>");

        // Italic
        txt = txt.split("\\i ").join("<em>");
        txt = txt.split("\\i0").join("</em>");

        // Underline
        txt = txt.split("\\ul ").join("<u>");
        txt = txt.split("\\ulnone").join("</u>");

        // Bullet
        txt = txt.split("\\bullet").join("• ");

        // Parágrafos
        txt = txt.split("\\par").join("<br>");

        // Unicode (\u1234)
        txt = txt.replace(/\\u(-?\d+)\??/g, (_, code) =>
            String.fromCharCode(parseInt(code, 10))
        );

        // Remover comandos restantes começando com "\"
        while (txt.includes("\\")) {
            let pos = txt.indexOf("\\");
            let end = pos + 1;

            while (end < txt.length && /[a-zA-Z0-9]/.test(txt[end])) {
                end++;
            }

            txt = txt.slice(0, pos) + txt.slice(end);
        }

        // Remover { }
        txt = txt.split("{").join("");
        txt = txt.split("}").join("");

        // Remover espaços sobrando no início
        txt = txt.trimStart();

        // Decodificar sequências 'xx' em bytes
        txt = txt.replace(/'([0-9A-Fa-f]{2})/g, function (_, hex) {
            return String.fromCharCode(parseInt(hex, 16));
        });

        // Converter bytes Shift-JIS para Unicode real
        txt = decodeShiftJIS(txt);

        return txt.trim();
    }
};

// Função fora do objeto
function decodeShiftJIS(str) {
    const bytes = [];
    for (let i = 0; i < str.length; i++) {
        const code = str.charCodeAt(i);
        if (code <= 0xFF) {
            bytes.push(code);
        }
    }

    try {
        const decoder = new TextDecoder("shift-jis");
        return decoder.decode(new Uint8Array(bytes));
    } catch {
        return str;
    }
}
