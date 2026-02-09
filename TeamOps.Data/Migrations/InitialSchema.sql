-- File: TeamOps.Data/Migrations/InitialSchema.sql

CREATE TABLE IF NOT EXISTS Operators (
    CodigoFJ    TEXT     PRIMARY KEY,              -- Código único do operador
    NameRomanji TEXT     NOT NULL,                 -- Nome em Romanji
    NameNihongo TEXT     NOT NULL,                 -- Nome em Kanji/Katakana
    ShiftId     INTEGER  NOT NULL,                 -- FK para Shifts
    GroupId     INTEGER  NOT NULL,                 -- FK para Groups
    SectorId    INTEGER  NOT NULL,                 -- FK para Sectors
    StartDate   DATE     NOT NULL,
    EndDate     DATE,
    Trainer     BOOLEAN  NOT NULL DEFAULT 0,       -- Se é treinador
    Status      BOOLEAN  NOT NULL DEFAULT 1,       -- Ativo/Inativo
    CreatedAt   DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsLeader    INTEGER  NOT NULL DEFAULT 0,
    Telefone    TEXT,
    Endereco    TEXT,

    FOREIGN KEY (ShiftId) REFERENCES Shifts(Id),
    FOREIGN KEY (GroupId) REFERENCES Groups(Id),
    FOREIGN KEY (SectorId) REFERENCES Sectors(Id)
);
-- ALTER TABLE Operators ADD COLUMN Telefone TEXT;
-- ALTER TABLE Operators ADD COLUMN Endereco TEXT;

 
CREATE TABLE IF NOT EXISTS Shifts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NamePt TEXT NOT NULL,
    NameJp TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Groups (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NamePt TEXT NOT NULL,
    NameJp TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Sectors (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NamePt TEXT NOT NULL,
    NameJp TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS GroupLeaders (
    Id           INTEGER  PRIMARY KEY AUTOINCREMENT,
    Name         TEXT     NOT NULL,
    Login        TEXT     UNIQUE NOT NULL,
    PasswordHash TEXT     NOT NULL,
    AccessLevel  INTEGER  NOT NULL DEFAULT 1,
    CreatedAt    DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS Assignments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    GLId INTEGER NOT NULL,
    OperatorCodigoFJ TEXT NOT NULL,
    AssignedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (GLId) REFERENCES GroupLeaders(Id),
    FOREIGN KEY (OperatorCodigoFJ) REFERENCES Operators(CodigoFJ)
);

CREATE TABLE IF NOT EXISTS Users (
    Id           INTEGER  PRIMARY KEY AUTOINCREMENT,
    Login        TEXT     UNIQUE,
    CodigoFJ     TEXT,
    Name         TEXT,
    PasswordHash TEXT     NOT NULL,
    AccessLevel  INTEGER  NOT NULL,
    CreatedAt    DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (
        CodigoFJ
    )
    REFERENCES Operators (CodigoFJ) 
);

CREATE TABLE IF NOT EXISTS Locals (
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    NamePt   TEXT NOT NULL,
    NameJp   TEXT NOT NULL,
    SectorId INTEGER NOT NULL,
    FOREIGN KEY (SectorId) REFERENCES Sectors(Id)
);

CREATE TABLE IF NOT EXISTS FollowUpReasons (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NamePt TEXT NOT NULL,
    NameJp TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS FollowUpTypes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NamePt TEXT NOT NULL,
    NameJp TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Equipments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NamePt TEXT NOT NULL,
    NameJp TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS FollowUps (
    Id               INTEGER  PRIMARY KEY AUTOINCREMENT,
    Date             DATETIME NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ShiftId          INTEGER  NOT NULL,
    OperatorCodigoFJ TEXT     NOT NULL,
    ExecutorCodigoFJ TEXT     NOT NULL,
    WitnessCodigoFJ  TEXT,
    ReasonId         INTEGER  NOT NULL,
    TypeId           INTEGER  NOT NULL,
    LocalId          INTEGER  NOT NULL,
    EquipmentId      INTEGER  NOT NULL,
    SectorId         INTEGER  NOT NULL,
    Description      TEXT     NOT NULL,
    Guidance         TEXT     NOT NULL,
    FOREIGN KEY (ShiftId)          REFERENCES Shifts (Id),
    FOREIGN KEY (OperatorCodigoFJ) REFERENCES Operators (CodigoFJ),
    FOREIGN KEY (ExecutorCodigoFJ) REFERENCES Operators (CodigoFJ),
    FOREIGN KEY (WitnessCodigoFJ)  REFERENCES Operators (CodigoFJ),
    FOREIGN KEY (ReasonId)         REFERENCES FollowUpReasons (Id),
    FOREIGN KEY (TypeId)           REFERENCES FollowUpTypes (Id),
    FOREIGN KEY (LocalId)          REFERENCES Locals (Id),
    FOREIGN KEY (EquipmentId)      REFERENCES Equipments (Id),
    FOREIGN KEY (SectorId)         REFERENCES Sectors (Id)
);

CREATE TABLE IF NOT EXISTS Categories (
    Id     INTEGER PRIMARY KEY AUTOINCREMENT,
    NamePt TEXT    NOT NULL,
    NameJp TEXT    NOT NULL
);

CREATE TABLE IF NOT EXISTS Hikitsugui (
    Id              INTEGER  PRIMARY KEY AUTOINCREMENT,
    Date            DATETIME NOT NULL,
    ShiftId         INTEGER  NOT NULL,
    CreatorCodigoFJ TEXT     NOT NULL,
    CategoryId      INTEGER  NOT NULL,
    EquipmentId     INTEGER,
    LocalId         INTEGER,
    SectorId        INTEGER,   -- NOVO
    ForLeaders      INTEGER  NOT NULL,
    ForOperators    INTEGER  NOT NULL,
    Description     TEXT     NOT NULL,
    AttachmentPath  TEXT,
    FOREIGN KEY (ShiftId)         REFERENCES Shifts (Id),
    FOREIGN KEY (CreatorCodigoFJ) REFERENCES Operators (CodigoFJ),
    FOREIGN KEY (CategoryId)      REFERENCES Categories (Id),
    FOREIGN KEY (EquipmentId)     REFERENCES Equipments (Id),
    FOREIGN KEY (LocalId)         REFERENCES Locals (Id),
    FOREIGN KEY (SectorId)        REFERENCES Sectors (Id)
);

CREATE TABLE IF NOT EXISTS HikitsuguiResponses (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    HikitsuguiId    INTEGER NOT NULL,
    Date            DATETIME NOT NULL,
    ResponderCodigoFJ TEXT NOT NULL,
    Message         TEXT NOT NULL,
    FOREIGN KEY (HikitsuguiId) REFERENCES Hikitsugui(Id),
    FOREIGN KEY (ResponderCodigoFJ) REFERENCES Operators(CodigoFJ)
);

CREATE TABLE IF NOT EXISTS HikitsuguiCorrections (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    HikitsuguiId    INTEGER NOT NULL,
    Date            DATETIME NOT NULL,
    CorrectorCodigoFJ TEXT NOT NULL,
    Correction      TEXT NOT NULL,
    FOREIGN KEY (HikitsuguiId) REFERENCES Hikitsugui(Id),
    FOREIGN KEY (CorrectorCodigoFJ) REFERENCES Operators(CodigoFJ)
);

CREATE TABLE IF NOT EXISTS HikitsuguiReads (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    HikitsuguiId    INTEGER NOT NULL,
    ReaderCodigoFJ  TEXT NOT NULL,
    ReadAt          DATETIME NOT NULL,
    FOREIGN KEY (HikitsuguiId) REFERENCES Hikitsugui(Id),
    FOREIGN KEY (ReaderCodigoFJ) REFERENCES Operators(CodigoFJ)
);

CREATE TABLE IF NOT EXISTS HikitsuguiAttachments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    HikitsuguiId INTEGER NOT NULL,
    FileName TEXT NOT NULL,
    FilePath TEXT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (HikitsuguiId) REFERENCES Hikitsugui(Id)
);

CREATE TABLE IF NOT EXISTS SobraDePeca (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Data TEXT NOT NULL,
    TurnoId INTEGER NOT NULL,
    Lote TEXT NOT NULL,
    OperadorId TEXT NOT NULL,
    Tanjuu REAL NOT NULL,
    PesoGramas REAL NOT NULL,
    Quantidade REAL NOT NULL,
    EquipmentId INTEGER NOT NULL,
    ShainId INTEGER NOT NULL,
    Observacao TEXT,
    Lider TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,

    FOREIGN KEY (TurnoId) REFERENCES Shifts(Id),
    FOREIGN KEY (OperadorId) REFERENCES Operators(CodigoFJ),
    FOREIGN KEY (EquipmentId) REFERENCES Equipments(Id),
    FOREIGN KEY (ShainId) REFERENCES Shain(Id)
);

CREATE TABLE IF NOT EXISTS Shain (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NomeRomanji TEXT NOT NULL,
    NomeKanji TEXT,
    Ativo INTEGER NOT NULL DEFAULT 1
);

CREATE INDEX IF NOT EXISTS IX_Operators_BadgeCode ON Operators(BadgeCode);
CREATE INDEX IF NOT EXISTS IX_GL_Login ON GroupLeaders(Login);
CREATE INDEX IF NOT EXISTS IX_Assignments_GL_Operator ON Assignments(GLId, OperatorId);
