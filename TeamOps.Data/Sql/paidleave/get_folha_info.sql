SELECT 
    o.NameRomanji AS TakenByName,
    f.TakenAt
FROM YukyuFolhaControle f
JOIN Operators o ON o.CodigoFJ = f.TakenBy
WHERE f.AcompYukyuId = @AcompYukyuId
ORDER BY f.TakenAt DESC
LIMIT 1;
