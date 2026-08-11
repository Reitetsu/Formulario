/*
    SysBimbo - Instalacion completa del formulario de materiales de impulso

    Este script instala o actualiza exclusivamente los objetos usados por:
      - CRUD de materiales por tienda.
      - Cuota diaria por material.
      - Seleccion de materiales activos en el formulario publico.
      - Registro, consulta y exportacion de evidencias fotograficas.

    Requisito:
      La tabla dbo.DimTiendaMaestra_Export debe existir porque contiene las
      tiendas y el Formato/Marca que utiliza el formulario.

    Caracteristicas:
      - Puede ejecutarse mas de una vez.
      - No elimina datos.
      - Incluye las mejoras de 001, 002 y 003.
      - Las fotografias se almacenan actualmente en SQL Server (VARBINARY(MAX)).

    Ejecucion:
      1. Seleccionar la base de datos de SysBimbo en SQL Server Management Studio.
      2. Ejecutar el script completo con una cuenta autorizada para crear y
         modificar tablas, indices y restricciones.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    BEGIN TRANSACTION;

    ---------------------------------------------------------------------------
    -- 1. Validar la tabla maestra requerida por el formulario
    ---------------------------------------------------------------------------
    IF OBJECT_ID(N'dbo.DimTiendaMaestra_Export', N'U') IS NULL
    BEGIN
        THROW 51000,
            N'No existe dbo.DimTiendaMaestra_Export. Debes cargar primero la tabla maestra de tiendas.',
            1;
    END;

    IF COL_LENGTH(N'dbo.DimTiendaMaestra_Export', N'TiendaCadenaKey') IS NULL
       OR COL_LENGTH(N'dbo.DimTiendaMaestra_Export', N'Formato') IS NULL
       OR COL_LENGTH(N'dbo.DimTiendaMaestra_Export', N'Nombre Tienda') IS NULL
       OR COL_LENGTH(N'dbo.DimTiendaMaestra_Export', N'Nombre Tienda Bimbo') IS NULL
    BEGIN
        THROW 51001,
            N'dbo.DimTiendaMaestra_Export no contiene todas las columnas requeridas: TiendaCadenaKey, Formato, Nombre Tienda y Nombre Tienda Bimbo.',
            1;
    END;

    ---------------------------------------------------------------------------
    -- 2. Asignacion de uno o mas materiales a una tienda
    ---------------------------------------------------------------------------
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
                CONSTRAINT DF_MaterialImpulsoTienda_FechaCreacion
                DEFAULT (SYSUTCDATETIME()),
            CONSTRAINT CK_MaterialImpulsoTienda_CuotaDiaria
                CHECK (CuotaDiaria >= 0)
        );
    END;
    ELSE
    BEGIN
        -- Las columnas de identidad definen el registro. Si faltan, la tabla
        -- existente no corresponde al modulo y no debe alterarse implicitamente.
        IF COL_LENGTH(N'dbo.MaterialImpulsoTienda', N'MaterialImpulsoTiendaId') IS NULL
           OR COL_LENGTH(N'dbo.MaterialImpulsoTienda', N'TiendaCadenaKey') IS NULL
           OR COL_LENGTH(N'dbo.MaterialImpulsoTienda', N'NombreMaterial') IS NULL
        BEGIN
            THROW 51002,
                N'La tabla dbo.MaterialImpulsoTienda existe, pero no tiene las columnas base esperadas.',
                1;
        END;

        IF COL_LENGTH(N'dbo.MaterialImpulsoTienda', N'Descripcion') IS NULL
        BEGIN
            ALTER TABLE dbo.MaterialImpulsoTienda
                ADD Descripcion NVARCHAR(500) NULL;
        END;

        IF COL_LENGTH(N'dbo.MaterialImpulsoTienda', N'CuotaDiaria') IS NULL
        BEGIN
            ALTER TABLE dbo.MaterialImpulsoTienda
                ADD CuotaDiaria INT NOT NULL
                    CONSTRAINT DF_MaterialImpulsoTienda_CuotaDiaria
                    DEFAULT (0) WITH VALUES;
        END;

        IF COL_LENGTH(N'dbo.MaterialImpulsoTienda', N'Activo') IS NULL
        BEGIN
            ALTER TABLE dbo.MaterialImpulsoTienda
                ADD Activo BIT NOT NULL
                    CONSTRAINT DF_MaterialImpulsoTienda_Activo
                    DEFAULT (1) WITH VALUES;
        END;

        IF COL_LENGTH(N'dbo.MaterialImpulsoTienda', N'FechaCreacion') IS NULL
        BEGIN
            ALTER TABLE dbo.MaterialImpulsoTienda
                ADD FechaCreacion DATETIME2 NOT NULL
                    CONSTRAINT DF_MaterialImpulsoTienda_FechaCreacion
                    DEFAULT (SYSUTCDATETIME()) WITH VALUES;
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.check_constraints
            WHERE parent_object_id = OBJECT_ID(N'dbo.MaterialImpulsoTienda')
              AND name = N'CK_MaterialImpulsoTienda_CuotaDiaria'
        )
        BEGIN
            ALTER TABLE dbo.MaterialImpulsoTienda WITH CHECK
                ADD CONSTRAINT CK_MaterialImpulsoTienda_CuotaDiaria
                CHECK (CuotaDiaria >= 0);
        END;
    END;

    -- El modelo permite varios materiales activos por tienda, pero no permite
    -- repetir el mismo nombre de material mientras ambos registros esten activos.
    IF EXISTS
    (
        SELECT 1
        FROM dbo.MaterialImpulsoTienda
        WHERE Activo = 1
        GROUP BY TiendaCadenaKey, NombreMaterial
        HAVING COUNT_BIG(*) > 1
    )
    BEGIN
        THROW 51003,
            N'Existen materiales activos duplicados para una tienda. Corrige los duplicados antes de crear el indice unico.',
            1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.MaterialImpulsoTienda')
          AND name = N'UX_MaterialImpulsoTienda_Tienda_Activo'
    )
    BEGIN
        DROP INDEX UX_MaterialImpulsoTienda_Tienda_Activo
            ON dbo.MaterialImpulsoTienda;
    END;

    IF NOT EXISTS
    (
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

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.MaterialImpulsoTienda')
          AND name = N'IX_MaterialImpulsoTienda_Tienda_Activo'
    )
    BEGIN
        CREATE INDEX IX_MaterialImpulsoTienda_Tienda_Activo
            ON dbo.MaterialImpulsoTienda (TiendaCadenaKey, Activo)
            INCLUDE (NombreMaterial, Descripcion, CuotaDiaria, FechaCreacion);
    END;

    ---------------------------------------------------------------------------
    -- 3. Evidencias fotograficas y acumulado por material
    ---------------------------------------------------------------------------
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
                REFERENCES dbo.MaterialImpulsoTienda (MaterialImpulsoTiendaId),
            CONSTRAINT CK_FotoMaterialImpulso_TamanoBytes
                CHECK (TamanoBytes > 0)
        );
    END;
    ELSE
    BEGIN
        IF COL_LENGTH(N'dbo.FotoMaterialImpulso', N'FotoMaterialImpulsoId') IS NULL
           OR COL_LENGTH(N'dbo.FotoMaterialImpulso', N'MaterialImpulsoTiendaId') IS NULL
           OR COL_LENGTH(N'dbo.FotoMaterialImpulso', N'TiendaCadenaKey') IS NULL
           OR COL_LENGTH(N'dbo.FotoMaterialImpulso', N'NombreArchivo') IS NULL
           OR COL_LENGTH(N'dbo.FotoMaterialImpulso', N'TipoContenido') IS NULL
           OR COL_LENGTH(N'dbo.FotoMaterialImpulso', N'TamanoBytes') IS NULL
           OR COL_LENGTH(N'dbo.FotoMaterialImpulso', N'Contenido') IS NULL
           OR COL_LENGTH(N'dbo.FotoMaterialImpulso', N'FechaCaptura') IS NULL
        BEGIN
            THROW 51004,
                N'La tabla dbo.FotoMaterialImpulso existe, pero su estructura no corresponde a la esperada por la API.',
                1;
        END;

        IF EXISTS
        (
            SELECT 1
            FROM dbo.FotoMaterialImpulso AS foto
            LEFT JOIN dbo.MaterialImpulsoTienda AS material
                ON material.MaterialImpulsoTiendaId = foto.MaterialImpulsoTiendaId
            WHERE material.MaterialImpulsoTiendaId IS NULL
        )
        BEGIN
            THROW 51005,
                N'Existen fotografias sin un material asociado. Corrige esos registros antes de crear la relacion.',
                1;
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.foreign_key_columns AS fkc
            WHERE fkc.parent_object_id = OBJECT_ID(N'dbo.FotoMaterialImpulso')
              AND fkc.parent_column_id = COLUMNPROPERTY(
                    OBJECT_ID(N'dbo.FotoMaterialImpulso'),
                    N'MaterialImpulsoTiendaId',
                    'ColumnId')
              AND fkc.referenced_object_id = OBJECT_ID(N'dbo.MaterialImpulsoTienda')
        )
        BEGIN
            ALTER TABLE dbo.FotoMaterialImpulso WITH CHECK
                ADD CONSTRAINT FK_FotoMaterialImpulso_MaterialImpulsoTienda
                FOREIGN KEY (MaterialImpulsoTiendaId)
                REFERENCES dbo.MaterialImpulsoTienda (MaterialImpulsoTiendaId);
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.check_constraints
            WHERE parent_object_id = OBJECT_ID(N'dbo.FotoMaterialImpulso')
              AND name = N'CK_FotoMaterialImpulso_TamanoBytes'
        )
        BEGIN
            ALTER TABLE dbo.FotoMaterialImpulso WITH CHECK
                ADD CONSTRAINT CK_FotoMaterialImpulso_TamanoBytes
                CHECK (TamanoBytes > 0);
        END;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.FotoMaterialImpulso')
          AND name = N'IX_FotoMaterialImpulso_Material_Fecha'
    )
    BEGIN
        CREATE INDEX IX_FotoMaterialImpulso_Material_Fecha
            ON dbo.FotoMaterialImpulso
                (MaterialImpulsoTiendaId, FechaCaptura DESC);
    END;

    COMMIT TRANSACTION;

    SELECT
        N'Instalacion completada correctamente.' AS Resultado,
        (SELECT COUNT_BIG(*) FROM dbo.MaterialImpulsoTienda) AS MaterialesRegistrados,
        (SELECT COUNT_BIG(*) FROM dbo.FotoMaterialImpulso) AS EvidenciasRegistradas;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
