SELECT 
    a.Id AS id,

    -- Nome do operador
    o.NameRomanji AS operatorName,

    -- Data da solicitação
    a.RequestDate AS requestDate,

    -- Nome de quem autorizou
    COALESCE(l.NameRomanji, '') AS authorizedBy,

    -- Flags booleanas
    CASE 
        WHEN EXISTS (SELECT 1 FROM YukyuTodoke t WHERE t.AcompYukyuId = a.Id)
        THEN 1 ELSE 0
    END AS hasTodoke,

    CASE 
        WHEN EXISTS (SELECT 1 FROM YukyuFolhaControle f WHERE f.AcompYukyuId = a.Id)
        THEN 1 ELSE 0
    END AS hasFolha

FROM AcompYukyu a
JOIN Operators o ON o.CodigoFJ = a.OperatorCodigoFJ
LEFT JOIN Operators l ON l.CodigoFJ = a.AuthorizedByCodigoFJ

ORDER BY a.RequestDate DESC;
