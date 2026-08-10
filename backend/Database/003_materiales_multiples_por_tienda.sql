SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.MaterialImpulsoTienda')
      AND name = N'UX_MaterialImpulsoTienda_Tienda_Activo'
)
BEGIN
    DROP INDEX UX_MaterialImpulsoTienda_Tienda_Activo
        ON dbo.MaterialImpulsoTienda;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.MaterialImpulsoTienda')
      AND name = N'UX_MaterialImpulsoTienda_Tienda_Material_Activo'
)
BEGIN
    CREATE UNIQUE INDEX UX_MaterialImpulsoTienda_Tienda_Material_Activo
        ON dbo.MaterialImpulsoTienda (TiendaCadenaKey, NombreMaterial)
        WHERE Activo = 1;
END;

COMMIT TRANSACTION;
