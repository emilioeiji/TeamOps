SELECT 
    Id,
    Date,
    ShiftId,
    Creator,
    CategoryId,
    EquipmentId,
    LocalId,
    SectorId,
    ForLeaders,
    ForOperators,
    ForMaSv,
    Description,
    DescriptionHtml
FROM Hikitsugui
WHERE Id = @id;
