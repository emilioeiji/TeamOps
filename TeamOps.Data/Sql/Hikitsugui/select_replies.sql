SELECT
    r.Id,
    r.HikitsuguiId,
    r.Message,
    r.Date,
    o.NameRomanji AS ResponderName
FROM HikitsuguiResponses r
JOIN Operators o ON o.CodigoFJ = r.ResponderCodigoFJ
WHERE r.HikitsuguiId = @id
ORDER BY r.Date ASC;
