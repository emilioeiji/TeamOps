SELECT 
    Id,
    LocalId,
    SectorId,
    X,
    Y
FROM OperatorPositions
WHERE SectorId = @SectorId;
