INSERT INTO Hikitsugui
(
    Date,
    ShiftId,
    CreatorCodigoFJ,
    CategoryId,
    EquipmentId,
    LocalId,
    SectorId,
    ForLeaders,
    ForOperators,
    ForMaSv,
    Description
)
VALUES
(
    @date,
    @shiftId,
    @creator,
    @categoryId,
    @equipmentId,
    @localId,
    @sectorId,
    @forLeaders,
    @forOperators,
    @forMaSv,
    @description
);

SELECT last_insert_rowid();
