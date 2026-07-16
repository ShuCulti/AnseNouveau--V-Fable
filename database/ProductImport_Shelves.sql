-- Product import generated from shelf photos (Jul 2026): personal care,
-- household and automotive. Same rules as ProductImport_Fridges.sql:
-- placeholders everywhere (Price 0.00, CostPrice 0, StockQty 0, Barcode NULL),
-- one 'Each' sell unit per product. Lines marked -- CHECK were hard to read.
-- Run ONCE against AnseNouveau_Dev:
--   sqlcmd -S localhost,1433 -U sa -P <password> -i ProductImport_Shelves.sql

USE AnseNouveau_Dev;
GO

IF NOT EXISTS (SELECT 1 FROM Departments WHERE ShopId = 1 AND Name = 'Personal Care')
    INSERT INTO Departments (ShopId, Name, SortOrder) VALUES (1, 'Personal Care', 5);
IF NOT EXISTS (SELECT 1 FROM Departments WHERE ShopId = 1 AND Name = 'Automotive')
    INSERT INTO Departments (ShopId, Name, SortOrder) VALUES (1, 'Automotive', 6);
GO

DECLARE @PersonalCare int = (SELECT MIN(Id) FROM Departments WHERE ShopId = 1 AND Name = 'Personal Care');
DECLARE @Household int = (SELECT MIN(Id) FROM Departments WHERE ShopId = 1 AND Name = 'Household');
DECLARE @Automotive int = (SELECT MIN(Id) FROM Departments WHERE ShopId = 1 AND Name = 'Automotive');

-- ---------------------------------------------------------------------------
-- Hair care / relaxers / colour
-- ---------------------------------------------------------------------------
INSERT INTO Products (ShopId, DepartmentId, Name, CostPrice) VALUES
(1, @PersonalCare, 'ORS Olive Oil relaxer kit', 0),
(1, @PersonalCare, 'Creme of Nature relaxer kit', 0),
(1, @PersonalCare, 'Soft & Beautiful relaxer kit', 0),
(1, @PersonalCare, 'T-Tree relaxer kit', 0),                    -- CHECK brand
(1, @PersonalCare, 'Revlon Colorsilk hair colour', 0),
(1, @PersonalCare, 'Wave kit', 0),                              -- CHECK (Wave Nouveau?)
(1, @PersonalCare, 'ORS Olive Oil sheen spray', 0),
(1, @PersonalCare, 'Luster''s Pink oil moisturizer', 0),        -- CHECK
(1, @PersonalCare, 'Hair mousse', 0),                           -- CHECK brand
(1, @PersonalCare, 'Blue Magic hair grease', 0),
(1, @PersonalCare, 'Softee hair grease', 0),                    -- CHECK brand (green jar)
(1, @PersonalCare, 'Magic shaving powder', 0),                  -- CHECK
(1, @PersonalCare, 'Clairol Pure White developer 30', 0);

-- ---------------------------------------------------------------------------
-- Deodorants / body sprays
-- ---------------------------------------------------------------------------
INSERT INTO Products (ShopId, DepartmentId, Name, CostPrice) VALUES
(1, @PersonalCare, 'Body spray can', 0),                        -- CHECK: split per brand later
(1, @PersonalCare, 'BOD Man body spray', 0),
(1, @PersonalCare, 'Axe body spray', 0),
(1, @PersonalCare, 'Dove deodorant spray', 0),
(1, @PersonalCare, 'Fa deodorant spray', 0),
(1, @PersonalCare, 'Nivea roll-on deodorant', 0),
(1, @PersonalCare, 'Dove roll-on deodorant', 0),
(1, @PersonalCare, 'Deodorant stick', 0);                       -- CHECK: split per brand later

-- ---------------------------------------------------------------------------
-- Wash / soap / oral care
-- ---------------------------------------------------------------------------
INSERT INTO Products (ShopId, DepartmentId, Name, CostPrice) VALUES
(1, @PersonalCare, 'Dove body wash', 0),
(1, @PersonalCare, 'Dove Men+Care body wash', 0),
(1, @PersonalCare, 'St. Ives body wash', 0),
(1, @PersonalCare, 'Irish Spring body wash', 0),
(1, @PersonalCare, 'Irish Spring bar soap', 0),
(1, @PersonalCare, 'Clere body lotion', 0),
(1, @PersonalCare, 'Jergens Ultra Healing lotion', 0),
(1, @PersonalCare, 'Jergens Nourishing Honey lotion', 0),
(1, @PersonalCare, 'Vaseline Intensive Care Cocoa Radiant', 0),
(1, @PersonalCare, 'Vaseline Intensive Care Soothing Hydration', 0),
(1, @PersonalCare, 'Vaseline Intensive Care Essential Healing', 0),
(1, @PersonalCare, 'Vaseline Intensive Care Aloe Soothe', 0),   -- CHECK variant
(1, @PersonalCare, 'Queen Helene Cocoa Butter lotion', 0),      -- CHECK
(1, @PersonalCare, 'Vaseline petroleum jelly jar', 0),
(1, @PersonalCare, 'VO5 shampoo', 0),
(1, @PersonalCare, 'Dove shampoo', 0),
(1, @PersonalCare, 'TRESemme shampoo', 0),
(1, @PersonalCare, 'Herbal Essences shampoo', 0),               -- CHECK
(1, @PersonalCare, 'Head & Shoulders shampoo', 0),
(1, @PersonalCare, 'Axe body wash', 0),
(1, @PersonalCare, 'Suave shampoo', 0),                         -- CHECK (gold bottle)
(1, @PersonalCare, 'Nair body cream', 0),
(1, @PersonalCare, 'Listerine mouthwash', 0),
(1, @PersonalCare, 'Colgate toothpaste', 0),
(1, @PersonalCare, 'Toothpaste (other brand)', 0);              -- CHECK: split per brand later

-- ---------------------------------------------------------------------------
-- Baby / health / feminine / misc personal
-- ---------------------------------------------------------------------------
INSERT INTO Products (ShopId, DepartmentId, Name, CostPrice) VALUES
(1, @PersonalCare, 'Johnson''s baby lotion', 0),
(1, @PersonalCare, 'Johnson''s baby oil', 0),
(1, @PersonalCare, 'Johnson''s baby powder', 0),
(1, @PersonalCare, 'Baby Magic baby wash', 0),                  -- CHECK
(1, @PersonalCare, 'Ammens medicated powder', 0),
(1, @PersonalCare, 'Summer''s Eve feminine wash', 0),
(1, @PersonalCare, 'White Rain cotton balls', 0),
(1, @PersonalCare, 'Sun care lotion', 0),                       -- CHECK: split per brand later
(1, @PersonalCare, 'Tanning oil', 0),                           -- CHECK
(1, @PersonalCare, 'Alcolado Glacial', 0),                      -- CHECK (green mentholated splash)
(1, @PersonalCare, 'Limacol lotion', 0),                        -- CHECK
(1, @PersonalCare, 'Hand sanitizer', 0),
(1, @Household, 'Dettol antiseptic liquid', 0),
(1, @Household, 'Dettol spray', 0),                             -- CHECK
(1, @Household, 'Loyal dishwashing liquid', 0),
(1, @Household, 'Money-print tumbler/wrap', 0),                 -- CHECK: the dollar-bill cylinders
(1, @Household, 'Candles (white)', 0),                          -- CHECK
(1, @Household, 'D-Fense insect spray', 0);                     -- CHECK

-- ---------------------------------------------------------------------------
-- Automotive shelf
-- ---------------------------------------------------------------------------
INSERT INTO Products (ShopId, DepartmentId, Name, CostPrice) VALUES
(1, @Automotive, 'Armor All Protectant', 0),
(1, @Automotive, 'Armor All Wet Tire Shine', 0),
(1, @Automotive, 'Armor All Car Wash', 0),
(1, @Automotive, 'Tuff Stuff foam cleaner', 0),
(1, @Automotive, 'Brake parts cleaner', 0),
(1, @Automotive, 'Carb cleaner', 0),
(1, @Automotive, 'Harris spray paint', 0),                      -- one entry; split per colour if needed
(1, @Automotive, 'Engine enamel spray', 0),                     -- CHECK
(1, @Automotive, 'Shell Helix motor oil 1L', 0),
(1, @Automotive, 'Castrol GTX 20W-50 motor oil', 0),
(1, @Automotive, 'Castrol motor oil (green bottle)', 0),        -- CHECK grade
(1, @Automotive, 'XCEL Type A transmission fluid', 0),
(1, @Automotive, 'XCEL Turbo motor oil', 0),
(1, @Automotive, 'XCEL Premium GL-1 gear oil', 0),
(1, @Automotive, 'Windshield washer fluid', 0),
(1, @Automotive, 'Power steering fluid', 0),
(1, @Automotive, 'Red-Cool coolant', 0),
(1, @Automotive, 'Radiator additive', 0),                       -- CHECK brand (Magnative?)
(1, @Automotive, 'Little Trees air freshener', 0),
(1, @Automotive, 'Car wash mitt', 0),
(1, @Household, 'Dog leash', 0),                                -- CHECK: hanging items
(1, @Household, 'Dog chain collar', 0),
(1, @Household, 'Fly swatter', 0);                              -- CHECK

-- ---------------------------------------------------------------------------
-- One 'Each' sell unit per new product (only products with no units yet).
-- ---------------------------------------------------------------------------
INSERT INTO SellUnits (ProductId, Label, Price, UnitsPerSale, IsCold, IsActive, SortOrder)
SELECT p.Id, 'Each', 0.00, 1, 0, 1, 1
FROM Products p
WHERE p.ShopId = 1
  AND NOT EXISTS (SELECT 1 FROM SellUnits su WHERE su.ProductId = p.Id);
GO
