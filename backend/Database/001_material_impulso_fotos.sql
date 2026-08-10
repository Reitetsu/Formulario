SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.MaterialImpulsoTienda', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MaterialImpulsoTienda
    (
        MaterialImpulsoTiendaId BIGINT IDENTITY(1, 1) NOT NULL
            CONSTRAINT PK_MaterialImpulsoTienda PRIMARY KEY,
        TiendaCadenaKey NVARCHAR(450) NOT NULL,
        NombreMaterial NVARCHAR(200) NOT NULL,
        Descripcion NVARCHAR(500) NULL,
        CuotaDiaria INT NOT NULL
            CONSTRAINT DF_MaterialImpulsoTienda_CuotaDiaria DEFAULT (0),
        Activo BIT NOT NULL
            CONSTRAINT DF_MaterialImpulsoTienda_Activo DEFAULT (1),
        FechaCreacion DATETIME2 NOT NULL
            CONSTRAINT DF_MaterialImpulsoTienda_FechaCreacion DEFAULT (SYSUTCDATETIME())
    );

    CREATE UNIQUE INDEX UX_MaterialImpulsoTienda_Tienda_Material_Activo
        ON dbo.MaterialImpulsoTienda (TiendaCadenaKey, NombreMaterial)
        WHERE Activo = 1;

    ALTER TABLE dbo.MaterialImpulsoTienda
        ADD CONSTRAINT CK_MaterialImpulsoTienda_CuotaDiaria
        CHECK (CuotaDiaria >= 0);
END;

IF OBJECT_ID(N'dbo.FotoMaterialImpulso', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FotoMaterialImpulso
    (
        FotoMaterialImpulsoId BIGINT IDENTITY(1, 1) NOT NULL
            CONSTRAINT PK_FotoMaterialImpulso PRIMARY KEY,
        MaterialImpulsoTiendaId BIGINT NOT NULL,
        TiendaCadenaKey NVARCHAR(450) NOT NULL,
        NombreArchivo NVARCHAR(260) NOT NULL,
        TipoContenido NVARCHAR(100) NOT NULL,
        TamanoBytes BIGINT NOT NULL,
        Contenido VARBINARY(MAX) NOT NULL,
        FechaCaptura DATETIME2 NOT NULL,
        CONSTRAINT FK_FotoMaterialImpulso_MaterialImpulsoTienda
            FOREIGN KEY (MaterialImpulsoTiendaId)
            REFERENCES dbo.MaterialImpulsoTienda (MaterialImpulsoTiendaId)
    );

    CREATE INDEX IX_FotoMaterialImpulso_Material_Fecha
        ON dbo.FotoMaterialImpulso (MaterialImpulsoTiendaId, FechaCaptura DESC);
END;

COMMIT TRANSACTION;

/*
Ejemplo para asignar el material real de una tienda:

INSERT INTO dbo.MaterialImpulsoTienda
    (TiendaCadenaKey, NombreMaterial, Descripcion, CuotaDiaria)
VALUES
    (N'CLAVE_REAL_DE_TIENDA', N'Nombre del material', N'Indicaciones para el impulsador', 20);
*/
