-- Retrocompatible upgrade for sector-aware production machine statuses.
-- Keeps existing rows as global fallback (SectorId NULL) and adds DAD-specific rows.

ALTER TABLE MachineStatuses ADD COLUMN SectorId INTEGER;
ALTER TABLE MachineStatuses ADD COLUMN Classification TEXT NOT NULL DEFAULT 'StopCounts';

DROP INDEX IF EXISTS IX_MachineStatuses_StatusCode;

CREATE TABLE IF NOT EXISTS MachineStatuses_SectorMigration (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SectorId INTEGER,
    StatusCode INTEGER NOT NULL,
    DisplayCode INTEGER NOT NULL,
    Classification TEXT NOT NULL DEFAULT 'StopCounts',
    NamePt TEXT NOT NULL,
    NameJp TEXT NOT NULL,
    ColorHex TEXT NOT NULL DEFAULT '#5B88E8',
    TextColorHex TEXT NOT NULL DEFAULT '#FFFFFF',
    SortOrder INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (SectorId) REFERENCES Sectors(Id)
);

INSERT OR IGNORE INTO MachineStatuses_SectorMigration
(
    Id,
    SectorId,
    StatusCode,
    DisplayCode,
    Classification,
    NamePt,
    NameJp,
    ColorHex,
    TextColorHex,
    SortOrder,
    IsActive
)
SELECT
    Id,
    SectorId,
    StatusCode,
    DisplayCode,
    CASE
        WHEN trim(COALESCE(Classification, '')) = '' THEN
            CASE DisplayCode
                WHEN 0 THEN 'Running'
                WHEN 4 THEN 'Error'
                ELSE 'StopCounts'
            END
        ELSE Classification
    END,
    NamePt,
    NameJp,
    ColorHex,
    TextColorHex,
    SortOrder,
    IsActive
FROM MachineStatuses;

DROP TABLE MachineStatuses;

ALTER TABLE MachineStatuses_SectorMigration
RENAME TO MachineStatuses;

CREATE UNIQUE INDEX IF NOT EXISTS IX_MachineStatuses_Sector_StatusCode
ON MachineStatuses(COALESCE(SectorId, 0), StatusCode);

UPDATE MachineStatuses
SET Classification = CASE DisplayCode
    WHEN 0 THEN 'Running'
    WHEN 4 THEN 'Error'
    ELSE 'StopCounts'
END
WHERE trim(COALESCE(Classification, '')) = '';

INSERT OR IGNORE INTO MachineStatuses
(SectorId, StatusCode, DisplayCode, Classification, NamePt, NameJp, ColorHex, TextColorHex, SortOrder, IsActive)
VALUES
(2, 0, 0, 'Running', 'Rodando', '運転', '#5B88E8', '#FFFFFF', 0, 1),
(2, 1, 3, 'StopCounts', 'Parado DAD', '停止中', '#F2CB58', '#4A3200', 1, 1),
(2, 3, 3, 'StopNoCount', 'Limpeza programada', '清掃', '#8EC5A8', '#123524', 3, 1),
(2, 4, 4, 'Error', 'Erro', '異常', '#FFFFFF', '#516174', 4, 1),
(2, 17, 1, 'StopNoCount', 'Intervalo', 'レス処理', '#8EC5A8', '#123524', 17, 1),
(2, 18, 1, 'StopNoCount', 'Limpeza programada', '吸引時間', '#8EC5A8', '#123524', 18, 1),
(2, 19, 1, 'StopNoCount', 'Amostra', 'サンプル', '#8EC5A8', '#123524', 19, 1);
