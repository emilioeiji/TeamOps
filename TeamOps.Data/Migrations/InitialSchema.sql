-- File: TeamOps.Data/Migrations/InitialSchema.sql

CREATE TABLE IF NOT EXISTS Operators (
    CodigoFJ TEXT PRIMARY KEY,              -- Código único do operador
    NameRomanji TEXT NOT NULL,              -- Nome em Romanji
    NameNihongo TEXT NOT NULL,              -- Nome em Kanji/Katakana
    ShiftId INTEGER NOT NULL,               -- FK para Shifts
    GroupId INTEGER NOT NULL,               -- FK para Groups
    SectorId INTEGER NOT NULL,              -- FK para Sectors
    StartDate DATE NOT NULL,
    EndDate DATE,
    Trainer BOOLEAN NOT NULL DEFAULT 0,     -- Se é treinador
    Status BOOLEAN NOT NULL DEFAULT 1,      -- Ativo/Inativo
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ShiftId) REFERENCES Shifts(Id),
    FOREIGN KEY (GroupId) REFERENCES Groups(Id),
    FOREIGN KEY (SectorId) REFERENCES Sectors(Id)
);
 
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

CREATE INDEX IF NOT EXISTS IX_Operators_BadgeCode ON Operators(BadgeCode);
CREATE INDEX IF NOT EXISTS IX_GL_Login ON GroupLeaders(Login);
CREATE INDEX IF NOT EXISTS IX_Assignments_GL_Operator ON Assignments(GLId, OperatorId);
