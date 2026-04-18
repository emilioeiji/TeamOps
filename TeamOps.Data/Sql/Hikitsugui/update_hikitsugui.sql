UPDATE Hikitsugui
SET
    CategoryId = @categoryId,
    EquipmentId = @equipmentId,
    LocalId = @localId,
    SectorId = @sectorId,
    Description = @description
WHERE Id = @id;
