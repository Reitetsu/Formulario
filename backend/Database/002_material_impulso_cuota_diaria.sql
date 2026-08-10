SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.MaterialImpulsoTienda', N'CuotaDiaria') IS NULL
BEGIN
    ALTER TABLE dbo.MaterialImpulsoTienda
        ADD CuotaDiaria INT NOT NULL
            CONSTRAINT DF_MaterialImpulsoTienda_CuotaDiaria DEFAULT (0) WITH VALUES;
END;

IF OBJECT_ID(N'dbo.CK_MaterialImpulsoTienda_CuotaDiaria', N'C') IS NULL
BEGIN
    EXEC(N'ALTER TABLE dbo.MaterialImpulsoTienda
        ADD CONSTRAINT CK_MaterialImpulsoTienda_CuotaDiaria
        CHECK (CuotaDiaria >= 0);');
END;

COMMIT TRANSACTION;
