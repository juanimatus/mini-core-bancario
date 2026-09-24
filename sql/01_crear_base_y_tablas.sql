/* Crea la base MiniCoreBancario, sus tablas y datos de ejemplo. Es idempotente: se puede correr varias veces. */

IF DB_ID(N'MiniCoreBancario') IS NULL
    CREATE DATABASE MiniCoreBancario;
GO

USE MiniCoreBancario;
GO

IF OBJECT_ID(N'dbo.Cuentas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cuentas (
        Id        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Cuentas PRIMARY KEY,
        Titular   NVARCHAR(100)     NOT NULL,
        -- DECIMAL y no FLOAT: el dinero no admite errores de redondeo.
        Saldo     DECIMAL(18,2)     NOT NULL
                  CONSTRAINT DF_Cuentas_Saldo DEFAULT 0
                  CONSTRAINT CK_Cuentas_Saldo_NoNegativo CHECK (Saldo >= 0),
        FechaAlta DATETIME2(0)      NOT NULL CONSTRAINT DF_Cuentas_FechaAlta DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID(N'dbo.Movimientos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Movimientos (
        Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Movimientos PRIMARY KEY,
        CuentaId    INT           NOT NULL CONSTRAINT FK_Movimientos_Cuentas REFERENCES dbo.Cuentas (Id),
        Tipo        VARCHAR(12)   NOT NULL
                    CONSTRAINT CK_Movimientos_Tipo CHECK (Tipo IN ('DEPOSITO', 'RETIRO', 'TRANSFER_IN', 'TRANSFER_OUT')),
        Monto       DECIMAL(18,2) NOT NULL CONSTRAINT CK_Movimientos_Monto CHECK (Monto > 0),
        Fecha       DATETIME2(0)  NOT NULL CONSTRAINT DF_Movimientos_Fecha DEFAULT SYSUTCDATETIME(),
        Descripcion NVARCHAR(200) NULL
    );
END
GO

-- Indice para la consulta tipica: movimientos de una cuenta ordenados por fecha.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Movimientos_CuentaId_Fecha')
    CREATE INDEX IX_Movimientos_CuentaId_Fecha ON dbo.Movimientos (CuentaId, Fecha);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Cuentas)
BEGIN
    INSERT INTO dbo.Cuentas (Titular, Saldo) VALUES
        (N'Ana Perez',    15000.50),
        (N'Bruno Gomez',   2300.00),
        (N'Carla Lopez',      0.00);
END
GO
