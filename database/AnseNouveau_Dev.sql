-- AnseNouveau POS database: schema + dev seed.
-- Run against Azure SQL Edge / SQL Server (docker container sql-server-mac, localhost:1433):
--   sqlcmd -S localhost,1433 -U sa -P <password> -i AnseNouveau_Dev.sql

IF DB_ID('AnseNouveau_Dev') IS NULL
    CREATE DATABASE AnseNouveau_Dev;
GO

USE AnseNouveau_Dev;
GO

-- ---------------------------------------------------------------------------
-- Schema (create order: Shops -> AppUsers/Departments/ExchangeRates -> Products
-- -> SellUnits -> PriceHistory -> Sales -> SaleLines -> StockCounts
-- -> StockCountLines -> StockMovements -> CashCounts)
-- ---------------------------------------------------------------------------

CREATE TABLE Shops (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    ReceiptHeader NVARCHAR(200) NULL,
    ReceiptFooter NVARCHAR(200) NULL,
    BaseCurrency CHAR(3) NOT NULL,
    TaxRatePercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE AppUsers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ShopId INT NOT NULL REFERENCES Shops(Id),
    DisplayName NVARCHAR(50) NOT NULL,
    PinHash NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Cashier', 'Admin')),
    IsActive BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE Departments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ShopId INT NOT NULL REFERENCES Shops(Id),
    Name NVARCHAR(50) NOT NULL,
    SortOrder INT NOT NULL DEFAULT 0
);
GO

-- Manually maintained rates: 1 unit of CurrencyCode = RateToBase in the shop's base currency.
CREATE TABLE ExchangeRates (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ShopId INT NOT NULL REFERENCES Shops(Id),
    CurrencyCode CHAR(3) NOT NULL,
    RateToBase DECIMAL(18,6) NOT NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_ExchangeRates_ShopId_CurrencyCode UNIQUE (ShopId, CurrencyCode)
);
GO

-- StockQty is a cached SUM of the StockMovements ledger; nothing writes it directly.
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ShopId INT NOT NULL REFERENCES Shops(Id),
    DepartmentId INT NULL REFERENCES Departments(Id),
    Barcode NVARCHAR(20) NULL,
    Name NVARCHAR(100) NOT NULL,
    StockQty DECIMAL(10,2) NOT NULL DEFAULT 0,
    CostPrice DECIMAL(10,2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE UNIQUE INDEX UX_Products_ShopId_Barcode
    ON Products(ShopId, Barcode)
    WHERE Barcode IS NOT NULL;
GO

-- A product has no sell price of its own; it is sold as units with independent prices.
CREATE TABLE SellUnits (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL REFERENCES Products(Id),
    Label NVARCHAR(50) NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    UnitsPerSale DECIMAL(10,2) NOT NULL,
    IsCold BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    SortOrder INT NOT NULL DEFAULT 0
);
GO

-- Written automatically on every sell-unit price change.
CREATE TABLE PriceHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SellUnitId INT NOT NULL REFERENCES SellUnits(Id),
    OldPrice DECIMAL(10,2) NOT NULL,
    NewPrice DECIMAL(10,2) NOT NULL,
    ChangedByUserId INT NOT NULL REFERENCES AppUsers(Id),
    ChangedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- IMMUTABLE: insert only, never updated or deleted. Corrections are Refund sales.
CREATE TABLE Sales (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ShopId INT NOT NULL REFERENCES Shops(Id),
    UserId INT NOT NULL REFERENCES AppUsers(Id),
    SaleTimeUtc DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL CHECK (Status IN ('Completed', 'Refund')),
    RefundOfSaleId INT NULL REFERENCES Sales(Id),
    TotalAmount DECIMAL(10,2) NOT NULL,
    PaymentMethod NVARCHAR(10) NOT NULL CHECK (PaymentMethod IN ('Cash', 'Card', 'Other')),
    TenderCurrency CHAR(3) NOT NULL,
    TenderRate DECIMAL(18,6) NOT NULL,
    AmountTendered DECIMAL(10,2) NULL,
    ChangeGiven DECIMAL(10,2) NULL
);
GO

-- Immutable; snapshots keep receipts stable when prices change later.
CREATE TABLE SaleLines (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SaleId INT NOT NULL REFERENCES Sales(Id),
    SellUnitId INT NULL REFERENCES SellUnits(Id),
    NameSnapshot NVARCHAR(120) NOT NULL,
    PriceSnapshot DECIMAL(10,2) NOT NULL,
    Qty DECIMAL(10,2) NOT NULL,
    LineTotal DECIMAL(10,2) NOT NULL
);
GO

CREATE TABLE StockCounts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ShopId INT NOT NULL REFERENCES Shops(Id),
    UserId INT NOT NULL REFERENCES AppUsers(Id),
    StartedAt DATETIME2 NOT NULL,
    ClosedAt DATETIME2 NULL,
    Notes NVARCHAR(200) NULL
);
GO

CREATE TABLE StockCountLines (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StockCountId INT NOT NULL REFERENCES StockCounts(Id),
    ProductId INT NOT NULL REFERENCES Products(Id),
    ExpectedQty DECIMAL(10,2) NOT NULL,
    CountedQty DECIMAL(10,2) NOT NULL,
    CONSTRAINT UQ_StockCountLines_StockCountId_ProductId UNIQUE (StockCountId, ProductId)
);
GO

-- Append-only ledger: the ONLY way stock changes.
CREATE TABLE StockMovements (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL REFERENCES Products(Id),
    QtyDelta DECIMAL(10,2) NOT NULL,
    MovementType NVARCHAR(20) NOT NULL CHECK (MovementType IN ('Sale', 'Delivery', 'Adjustment', 'Refund', 'CountFix')),
    SaleId INT NULL REFERENCES Sales(Id),
    StockCountId INT NULL REFERENCES StockCounts(Id),
    Reason NVARCHAR(200) NULL,
    UserId INT NOT NULL REFERENCES AppUsers(Id),
    MovedAt DATETIME2 NOT NULL
);
GO

CREATE INDEX IX_StockMovements_ProductId_MovedAt ON StockMovements(ProductId, MovedAt);
GO

CREATE TABLE CashCounts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ShopId INT NOT NULL REFERENCES Shops(Id),
    UserId INT NOT NULL REFERENCES AppUsers(Id),
    BusinessDate DATE NOT NULL,
    FloatAmount DECIMAL(10,2) NOT NULL,
    ExpectedCash DECIMAL(10,2) NOT NULL,
    CountedCash DECIMAL(10,2) NOT NULL,
    Difference AS (CountedCash - ExpectedCash) PERSISTED,
    ClosedAt DATETIME2 NOT NULL,
    Notes NVARCHAR(200) NULL,
    CONSTRAINT UQ_CashCounts_ShopId_BusinessDate UNIQUE (ShopId, BusinessDate)
);
GO

-- ---------------------------------------------------------------------------
-- Seed data
-- ---------------------------------------------------------------------------

INSERT INTO Shops (Name, ReceiptHeader, ReceiptFooter, BaseCurrency, TaxRatePercent)
VALUES ('Anse Nouveau Mini-Market', 'Anse Nouveau Mini-Market - Sint Maarten', 'Thank you! See you soon.', 'EUR', 0.00);
GO

-- PINs: Admin = 1234, Cashier = 5678 (BCrypt).
INSERT INTO AppUsers (ShopId, DisplayName, PinHash, Role, IsActive) VALUES
(1, 'Admin', '$2a$11$rbMigEltYQRIvAAKWiGtA.ZgLNmN.V7v.DVXKiJ1srCfzbGZ5Bqoq', 'Admin', 1),
(1, 'Cashier', '$2a$11$kaR21fePtlWTNGzX1PH4iOwR.fGWSBAidV.KMvmI/cZydf8Z0603K', 'Cashier', 1);
GO

INSERT INTO Departments (ShopId, Name, SortOrder) VALUES
(1, 'Drinks', 1),
(1, 'Snacks', 2),
(1, 'Household', 3);
GO

-- EUR base; USD and ANG circulate on the island.
INSERT INTO ExchangeRates (ShopId, CurrencyCode, RateToBase) VALUES
(1, 'USD', 0.920000),
(1, 'ANG', 0.514000);
GO

INSERT INTO Products (ShopId, DepartmentId, Barcode, Name, CostPrice) VALUES
(1, 1, '7461001000011', 'Presidente Beer 250ml', 0.60),   -- 1
(1, 1, '5449000000996', 'Coca-Cola 355ml',      0.45),   -- 2
(1, 1, '7461001000028', 'Water 500ml',          0.15),   -- 3
(1, 1, '7461001000035', 'Orange Juice 1L',      0.90),   -- 4
(1, 2, '7461002000010', 'Plantain Chips',       0.50),   -- 5
(1, 2, '7461002000027', 'Salted Peanuts 100g',  0.40),   -- 6
(1, 2, '7461002000034', 'Coconut Cookies',      0.80),   -- 7
(1, 3, '7461003000019', 'Rice 1kg',             1.10),   -- 8
(1, 3, '7461003000026', 'Dish Soap 500ml',      0.95),   -- 9
(1, 1, NULL,            'Ice Bag 2kg',          0.50),   -- 10 (no barcode -> POS quick button)
(1, 2, NULL,            'Bread Loaf',           0.70);   -- 11 (no barcode -> POS quick button)
GO

-- Cold vs warm share the same stock pool; prices are independent, never calculated.
INSERT INTO SellUnits (ProductId, Label, Price, UnitsPerSale, IsCold, IsActive, SortOrder) VALUES
(1,  'Cold single', 1.50, 1,  1, 1, 1),
(1,  'Warm single', 1.20, 1,  0, 1, 2),
(1,  'Case (24)',   20.00, 24, 0, 1, 3),
(2,  'Cold can',    1.25, 1,  1, 1, 1),
(2,  'Warm can',    1.00, 1,  0, 1, 2),
(2,  'Six-pack',    5.00, 6,  0, 1, 3),
(2,  'Case (24)',   18.00, 24, 0, 1, 4),
(3,  'Cold bottle', 1.00, 1,  1, 1, 1),
(3,  'Warm bottle', 0.75, 1,  0, 1, 2),
(3,  'Pack (12)',   7.00, 12, 0, 1, 3),
(4,  'Cold carton', 2.50, 1,  1, 1, 1),
(4,  'Warm carton', 2.25, 1,  0, 1, 2),
(5,  'Bag',         1.25, 1,  0, 1, 1),
(6,  'Pack',        1.00, 1,  0, 1, 1),
(7,  'Pack',        2.00, 1,  0, 1, 1),
(8,  'Bag',         2.50, 1,  0, 1, 1),
(9,  'Bottle',      2.75, 1,  0, 1, 1),
(10, 'Bag',         2.00, 1,  1, 1, 1),
(11, 'Loaf',        1.80, 1,  0, 1, 1);
GO

-- Opening stock enters the ledger as Adjustment movements (hard rule: never a direct StockQty write).
INSERT INTO StockMovements (ProductId, QtyDelta, MovementType, Reason, UserId, MovedAt) VALUES
(1,  96,  'Adjustment', 'opening stock', 1, SYSUTCDATETIME()),
(2,  120, 'Adjustment', 'opening stock', 1, SYSUTCDATETIME()),
(3,  60,  'Adjustment', 'opening stock', 1, SYSUTCDATETIME()),
(4,  24,  'Adjustment', 'opening stock', 1, SYSUTCDATETIME()),
(5,  40,  'Adjustment', 'opening stock', 1, SYSUTCDATETIME()),
(6,  30,  'Adjustment', 'opening stock', 1, SYSUTCDATETIME()),
(7,  25,  'Adjustment', 'opening stock', 1, SYSUTCDATETIME()),
(8,  50,  'Adjustment', 'opening stock', 1, SYSUTCDATETIME()),
(9,  20,  'Adjustment', 'opening stock', 1, SYSUTCDATETIME()),
(10, 15,  'Adjustment', 'opening stock', 1, SYSUTCDATETIME()),
(11, 12,  'Adjustment', 'opening stock', 1, SYSUTCDATETIME());
GO

-- Refresh the cached quantity from the ledger.
UPDATE p
SET StockQty = ISNULL(m.Total, 0)
FROM Products p
LEFT JOIN (
    SELECT ProductId, SUM(QtyDelta) AS Total
    FROM StockMovements
    GROUP BY ProductId
) m ON m.ProductId = p.Id;
GO
