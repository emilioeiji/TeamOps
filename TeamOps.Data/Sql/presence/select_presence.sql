SELECT 
    p.Id,
    p.CodigoFJ,
    p.LocalId,
    p.SectorId,
    p.ShiftId,
    p.Timestamp,
    o.NameRomanji,
    o.NameNihongo
FROM OperatorPresence p
JOIN Operators o ON o.CodigoFJ = p.CodigoFJ
WHERE p.Date = @Date
  AND p.SectorId = @SectorId
  AND p.ShiftId = @ShiftId
ORDER BY p.Timestamp DESC;
