SELECT 
    o.NameRomanji AS TakenByName,
    t.TakenAt
FROM YukyuTodoke t
JOIN Operators o ON o.CodigoFJ = t.TakenBy
WHERE t.AcompYukyuId = @AcompYukyuId
ORDER BY t.TakenAt DESC
LIMIT 1;
