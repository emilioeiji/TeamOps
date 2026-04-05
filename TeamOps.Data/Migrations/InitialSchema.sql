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
    Nascimento  DATE,

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
    ForMaSv         INTEGER  NOT NULL DEFAULT 0,
    FOREIGN KEY (ShiftId)         REFERENCES Shifts (Id),
    FOREIGN KEY (CreatorCodigoFJ) REFERENCES Operators (CodigoFJ),
    FOREIGN KEY (CategoryId)      REFERENCES Categories (Id),
    FOREIGN KEY (EquipmentId)     REFERENCES Equipments (Id),
    FOREIGN KEY (LocalId)         REFERENCES Locals (Id),
    FOREIGN KEY (SectorId)        REFERENCES Sectors (Id)
);
-- ALTER TABLE Hikitsugui ADD COLUMN ForMaSv INTEGER NOT NULL DEFAULT 0;

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
    MachineId INTEGER NOT NULL,
    ShainId INTEGER NOT NULL,
    Observacao TEXT,
    Lider TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    Item TEXT NOT NULL,

    FOREIGN KEY (TurnoId) REFERENCES Shifts(Id),
    FOREIGN KEY (OperadorId) REFERENCES Operators(CodigoFJ),
    FOREIGN KEY (MachineId) REFERENCES Machines(Id),
    FOREIGN KEY (ShainId) REFERENCES Shain(Id)
);

CREATE TABLE IF NOT EXISTS Shain (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NameRomanji TEXT NOT NULL,
    NameNihongo TEXT,
    Ativo INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS PRCategorias (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NamePt TEXT NOT NULL,
    NameJp TEXT NOT NULL
);
-- 1 Sobre Operacao
-- 2 Sobre Seguranca
-- 3 Sobre Trabalho
-- 4 Outros

CREATE TABLE IF NOT EXISTS PRPrioridades (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NamePt TEXT NOT NULL,
    NameJp TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS PR (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,

    SetorId INTEGER NOT NULL,                 -- FK para Sectors
    CategoriaId INTEGER NOT NULL,             -- FK para PRCategorias
    PrioridadeId INTEGER NOT NULL,            -- FK para PRPrioridades

    Titulo TEXT NOT NULL,
    NomeArquivo TEXT NOT NULL,                -- Gerado automaticamente

    DataEmissao TEXT NOT NULL,                -- Data atual
    DataRetornoHiru TEXT,                     -- Para uso futuro
    DataRetornoYakin TEXT,                    -- Para uso futuro

    AutorCodigoFJ TEXT NOT NULL,              -- Nome do usuário (Operators)

    CreatedAt TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),

    FOREIGN KEY (SetorId) REFERENCES Sectors(Id),
    FOREIGN KEY (CategoriaId) REFERENCES PRCategorias(Id),
    FOREIGN KEY (PrioridadeId) REFERENCES PRPrioridades(Id),
    FOREIGN KEY (AutorCodigoFJ) REFERENCES Operators(CodigoFJ)
);

CREATE TABLE IF NOT EXISTS CLCategorias (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NamePt TEXT NOT NULL,
    NameJp TEXT NOT NULL
);
-- 1 Sobre Operacao
-- 2 Sobre Seguranca
-- 3 Sobre Trabalho
-- 4 Outros

CREATE TABLE IF NOT EXISTS CLPrioridades (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NamePt TEXT NOT NULL,
    NameJp TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS CL (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,

    SetorId INTEGER NOT NULL,                 -- FK para Sectors
    CategoriaId INTEGER NOT NULL,             -- FK para PRCategorias
    PrioridadeId INTEGER NOT NULL,            -- FK para PRPrioridades

    Titulo TEXT NOT NULL,
    NomeArquivo TEXT NOT NULL,                -- Gerado automaticamente

    DataEmissao TEXT NOT NULL,                -- Data atual
    DataRetornoHiru TEXT,                     -- Para uso futuro
    DataRetornoYakin TEXT,                    -- Para uso futuro

    AutorCodigoFJ TEXT NOT NULL,              -- Nome do usuário (Operators)

    CreatedAt TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),

    FOREIGN KEY (SetorId) REFERENCES Sectors(Id),
    FOREIGN KEY (CategoriaId) REFERENCES PRCategorias(Id),
    FOREIGN KEY (PrioridadeId) REFERENCES PRPrioridades(Id),
    FOREIGN KEY (AutorCodigoFJ) REFERENCES Operators(CodigoFJ)
);

CREATE TABLE IF NOT EXISTS Machines (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NamePt TEXT NOT NULL,
    NameJp TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS TodokeMotivo (
    Id      INTEGER PRIMARY KEY AUTOINCREMENT,
    NomePt  TEXT NOT NULL,
    NomeJp  TEXT NOT NULL
);

INSERT INTO TodokeMotivo (Id, NomePt, NomeJp)
SELECT 1, 'Yukyu', '有休'
WHERE NOT EXISTS (SELECT 1 FROM TodokeMotivo WHERE Id = 1);

INSERT INTO TodokeMotivo (Id, NomePt, NomeJp)
SELECT 2, 'Falta', '欠勤'
WHERE NOT EXISTS (SELECT 1 FROM TodokeMotivo WHERE Id = 2);

INSERT INTO TodokeMotivo (Id, NomePt, NomeJp)
SELECT 3, 'Atraso', '遅刻'
WHERE NOT EXISTS (SELECT 1 FROM TodokeMotivo WHERE Id = 3);

INSERT INTO TodokeMotivo (Id, NomePt, NomeJp)
SELECT 4, 'Saida Retorno', '外出'
WHERE NOT EXISTS (SELECT 1 FROM TodokeMotivo WHERE Id = 4);

INSERT INTO TodokeMotivo (Id, NomePt, NomeJp)
SELECT 5, 'Sair Cedo', '早退'
WHERE NOT EXISTS (SELECT 1 FROM TodokeMotivo WHERE Id = 5);

INSERT INTO TodokeMotivo (Id, NomePt, NomeJp)
SELECT 6, 'Shukin', '休日出勤'
WHERE NOT EXISTS (SELECT 1 FROM TodokeMotivo WHERE Id = 6);

INSERT INTO TodokeMotivo (Id, NomePt, NomeJp)
SELECT 7, 'Hr Extra', '残業'
WHERE NOT EXISTS (SELECT 1 FROM TodokeMotivo WHERE Id = 7);

INSERT INTO TodokeMotivo (Id, NomePt, NomeJp)
SELECT 8, 'Domingo', '法定休日'
WHERE NOT EXISTS (SELECT 1 FROM TodokeMotivo WHERE Id = 8);

CREATE TABLE AcompYukyu (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,

    OperatorCodigoFJ TEXT NOT NULL,
    RequestDate TEXT NOT NULL,

    AuthorizedByCodigoFJ TEXT,
    Notes TEXT,

    TodokeMotivoId INTEGER,     -- NOVO

    FOREIGN KEY (OperatorCodigoFJ) REFERENCES Operators (CodigoFJ),
    FOREIGN KEY (AuthorizedByCodigoFJ) REFERENCES Operators (CodigoFJ),
    FOREIGN KEY (TodokeMotivoId) REFERENCES TodokeMotivo (Id)
);

ALTER TABLE AcompYukyu ADD COLUMN TodokeMotivoId INTEGER;
ALTER TABLE AcompYukyu ADD COLUMN Conferencia INTEGER NOT NULL DEFAULT 0;

-- Foreign key para TodokeMotivo
-- (SQLite não permite adicionar FK depois, mas você pode garantir via lógica)

CREATE TABLE YukyuConferencia (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AcompYukyuId INTEGER NOT NULL,
    TakenBy TEXT NOT NULL,
    TakenAt TEXT NOT NULL,

    FOREIGN KEY (AcompYukyuId) REFERENCES AcompYukyu(Id),
    FOREIGN KEY (TakenBy) REFERENCES Operators(CodigoFJ)
);

CREATE TABLE YukyuTodoke (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AcompYukyuId INTEGER NOT NULL,
    TakenBy INTEGER NOT NULL,
    TakenAt TEXT NOT NULL
);

CREATE TABLE YukyuFolhaControle (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AcompYukyuId INTEGER NOT NULL,
    TakenBy INTEGER NOT NULL,
    TakenAt TEXT NOT NULL
);

CREATE TABLE OperatorPresence (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CodigoFJ TEXT NOT NULL,
    SectorId INTEGER NOT NULL,   -- reutilizado do Local
    LocalId INTEGER NOT NULL,
    ShiftId INTEGER NOT NULL,
    Date DATETIME NOT NULL,
    FOREIGN KEY (CodigoFJ) REFERENCES Operators(CodigoFJ),
    FOREIGN KEY (LocalId) REFERENCES Locals(Id),
    FOREIGN KEY (SectorId) REFERENCES Sectors(Id)
);

CREATE TABLE IF NOT EXISTS OperatorPositions (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    SectorId  INTEGER NOT NULL,
    LocalId   INTEGER NOT NULL,
    X         INTEGER NOT NULL,
    Y         INTEGER NOT NULL,

    FOREIGN KEY (SectorId) REFERENCES Sectors(Id),
    FOREIGN KEY (LocalId)  REFERENCES Locals(Id)
);

INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 1, 1600, 940);
INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 3, 1600, 690);
INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 4, 1600, 440);
INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 5, 1600, 190);

INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 6, 1260, 940);
INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 7, 1260, 690);
INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 8, 1260, 440);
INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 9, 1260, 190);

INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 10, 1000, 940);
INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 11, 1000, 690);
INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 12, 1000, 440);
INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 13, 1000, 190);

INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 14, 660, 940);
INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 15, 660, 690);
INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 16, 660, 440);
INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 17, 660, 190);

INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 18, 320, 940);
INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 19, 320, 690);
INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 20, 320, 440);
INSERT INTO Locals (SectorId, Id, X, Y) VALUES (1, 21, 320, 190);

CREATE TABLE IF NOT EXISTS OperatorSchedule (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,

    CodigoFJ TEXT NOT NULL,
    SectorId INTEGER NOT NULL,
    LocalId INTEGER NOT NULL,
    ShiftId INTEGER NOT NULL,
    ScheduleDate DATE NOT NULL,

    ImportedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (CodigoFJ) REFERENCES Operators(CodigoFJ),
    FOREIGN KEY (SectorId) REFERENCES Sectors(Id),
    FOREIGN KEY (LocalId) REFERENCES Locals(Id)
);

CREATE INDEX IF NOT EXISTS IX_Operators_BadgeCode ON Operators(BadgeCode);
CREATE INDEX IF NOT EXISTS IX_GL_Login ON GroupLeaders(Login);
CREATE INDEX IF NOT EXISTS IX_Assignments_GL_Operator ON Assignments(GLId, OperatorId);
