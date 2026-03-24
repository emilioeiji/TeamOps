SELECT 
    a.Id AS id,
    o.NameRomanji AS operatorName,
    a.RequestDate AS requestDate,
    COALESCE(l.NameRomanji, '') AS authorizedBy,

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

WHERE (@ShiftId = 0 OR o.ShiftId = @ShiftId)

ORDER BY a.RequestDate DESC;
