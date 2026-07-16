-- Product import generated from fridge photos (Jul 2026).
-- Everything is a placeholder except the name: Price 0.00, CostPrice 0, StockQty 0,
-- Barcode NULL. Fill in real prices via the back office (Price Grid), assign barcodes
-- by opening Edit and scanning into the barcode field, and enter stock as
-- Adjustment movements when you count it.
-- Lines marked -- CHECK are products whose label was hard to read; rename or delete.
-- Run ONCE against AnseNouveau_Dev (running twice duplicates everything):
--   sqlcmd -S localhost,1433 -U sa -P <password> -i ProductImport_Fridges.sql

USE AnseNouveau_Dev;
GO

-- The beer/wine fridges get their own department for the Z-report.
IF NOT EXISTS (SELECT 1 FROM Departments WHERE ShopId = 1 AND Name = 'Alcohol')
    INSERT INTO Departments (ShopId, Name, SortOrder) VALUES (1, 'Alcohol', 4);
GO

DECLARE @Drinks int = (SELECT MIN(Id) FROM Departments WHERE ShopId = 1 AND Name = 'Drinks');
DECLARE @Snacks int = (SELECT MIN(Id) FROM Departments WHERE ShopId = 1 AND Name = 'Snacks');
DECLARE @Alcohol int = (SELECT MIN(Id) FROM Departments WHERE ShopId = 1 AND Name = 'Alcohol');

-- ---------------------------------------------------------------------------
-- Fridge 1: mixed sodas / milks / juices / waters
-- ---------------------------------------------------------------------------
INSERT INTO Products (ShopId, DepartmentId, Name, CostPrice) VALUES
(1, @Drinks, 'Schweppes Lemon can', 0),
(1, @Drinks, 'Schweppes Indian Tonic can', 0),
(1, @Drinks, 'Orangina can', 0),
(1, @Drinks, 'AK-100 Vanilla drink can', 0),
(1, @Drinks, 'Ginger Beer bottle', 0),                       -- CHECK brand
(1, @Drinks, 'Perrier bottle', 0),
(1, @Drinks, 'Malta bottle', 0),                             -- CHECK brand (extracto de malta)
(1, @Drinks, 'Obsesso Iced Coffee Latte', 0),
(1, @Drinks, 'Milo drink box', 0),
(1, @Drinks, 'Nesquik milk box', 0),
(1, @Drinks, 'Supligen Peanut can', 0),
(1, @Drinks, 'Supligen Vanilla can', 0),
(1, @Drinks, 'V8 Original can', 0),
(1, @Drinks, 'Lucozade Energy bottle', 0),
(1, @Drinks, 'Cranberry juice bottle', 0),                   -- CHECK brand
(1, @Drinks, 'Mixed fruit juice bottle', 0),                 -- CHECK brand (Fruc?)
(1, @Drinks, 'Pear nectar bottle', 0),                       -- CHECK white creamy bottle
(1, @Drinks, 'Volvic Touch of Strawberry', 0),
(1, @Drinks, 'Blue Waters bottled water', 0),
(1, @Drinks, 'Cristaline water bottle', 0),
(1, @Drinks, 'Heavenly water 1.35L', 0);

-- ---------------------------------------------------------------------------
-- Fridge 2: sodas / iced tea / energy + chocolate shelf
-- ---------------------------------------------------------------------------
INSERT INTO Products (ShopId, DepartmentId, Name, CostPrice) VALUES
(1, @Drinks, 'Canada Dry Ginger Ale can', 0),
(1, @Drinks, 'Red Bull 250ml can', 0),
(1, @Drinks, 'Big Burst Orange drink', 0),
(1, @Drinks, 'Rauch EisTee Lemon', 0),
(1, @Drinks, 'Rauch EisTee Raspberry', 0),                   -- CHECK flavour (pink bottle)
(1, @Drinks, 'Rauch EisTee Peach', 0),                       -- CHECK flavour
(1, @Drinks, 'Fanta Orange can', 0),
(1, @Drinks, 'Sprite Lemon can', 0),
(1, @Drinks, 'OKF Grapefruit sparkling can', 0),             -- CHECK
(1, @Drinks, 'A&W Root Beer can', 0),
(1, @Drinks, 'Coco Rico can', 0),
(1, @Drinks, 'Jumex Guava Nectar can', 0),
(1, @Drinks, 'Jamaica drink can', 0),                        -- CHECK (agua de jamaica?)
(1, @Drinks, 'Ting Grapefruit bottle', 0),
(1, @Drinks, 'Carib Shandy Lime bottle', 0),
(1, @Snacks, 'Snickers bar', 0),
(1, @Snacks, 'Bounty bar', 0),
(1, @Snacks, 'Twix bar', 0),
(1, @Snacks, 'Mars bar', 0);

-- ---------------------------------------------------------------------------
-- Fridge 3 (Vestfrost): wines / mixers / coconut water / juices
-- ---------------------------------------------------------------------------
INSERT INTO Products (ShopId, DepartmentId, Name, CostPrice) VALUES
(1, @Alcohol, 'Rude Boy Original', 0),
(1, @Alcohol, 'Rude Boy Mango', 0),
(1, @Alcohol, 'La Fuerza red wine', 0),
(1, @Alcohol, 'Stone''s Original Ginger Wine', 0),
(1, @Alcohol, 'Hard Wine Classic Red', 0),                   -- CHECK label
(1, @Alcohol, 'Brise de France Chardonnay 25cl', 0),
(1, @Alcohol, 'Brise de France Merlot 25cl', 0),
(1, @Alcohol, 'Brise de France Syrah 25cl', 0),
(1, @Alcohol, 'Sutter Home mini wine', 0),                   -- CHECK: split per varietal if needed
(1, @Alcohol, 'Sparkletini Raspberry', 0),
(1, @Alcohol, 'Rum Cola can', 0),                            -- CHECK brand
(1, @Drinks, 'Parrot Coconut Water can', 0),
(1, @Drinks, 'Goya Coconut Water', 0),
(1, @Drinks, 'Goya Organics Coconut Water', 0),
(1, @Drinks, 'Rica Pineapple Juice', 0),
(1, @Drinks, 'Tropicana Orange Juice small', 0),
(1, @Drinks, 'Tropicana Pure Premium Orange 1L', 0),
(1, @Drinks, 'Dimes Buzz Tangerine & Grapefruit', 0),
(1, @Drinks, 'Acqua Panna water bottle', 0);

-- ---------------------------------------------------------------------------
-- Fridge 4 (blue): AriZona / malts / Monster / sports drinks
-- ---------------------------------------------------------------------------
INSERT INTO Products (ShopId, DepartmentId, Name, CostPrice) VALUES
(1, @Drinks, 'AriZona Cherry Lime Rickey', 0),
(1, @Drinks, 'AriZona Fruit Punch', 0),
(1, @Drinks, 'AriZona Green Tea', 0),
(1, @Drinks, 'AriZona Mucho Mango', 0),
(1, @Drinks, 'PowerMalt Extra Energy', 0),
(1, @Drinks, 'Vita Malt Ginger', 0),
(1, @Drinks, 'Vita Malt Classic', 0),
(1, @Drinks, 'Monster Energy', 0),
(1, @Drinks, 'Monster Ultra', 0),
(1, @Drinks, 'Juice Monster Mango Loco', 0),
(1, @Drinks, 'Juice Monster Pipeline Punch', 0),
(1, @Drinks, 'Malta India can', 0),
(1, @Drinks, 'Vitamin Water', 0),
(1, @Drinks, 'Gatorade Lemon-Lime', 0),
(1, @Drinks, 'OKF Farmer''s Aloe Vera', 0),
(1, @Drinks, 'Sprite 500ml bottle', 0),
(1, @Drinks, 'Coca-Cola 500ml bottle', 0);

-- ---------------------------------------------------------------------------
-- Fridge 5: beer (Presidente 250ml already exists from the seed)
-- ---------------------------------------------------------------------------
INSERT INTO Products (ShopId, DepartmentId, Name, CostPrice) VALUES
(1, @Alcohol, 'Smirnoff Ice Original', 0),
(1, @Alcohol, 'Smirnoff Ice Raspberry', 0),
(1, @Alcohol, 'Heineken Original bottle', 0),
(1, @Alcohol, 'Heineken 0.0 bottle', 0),
(1, @Alcohol, 'Heineken can', 0),
(1, @Alcohol, 'Amstel Bright bottle', 0),                    -- CHECK
(1, @Alcohol, 'Desperados Original', 0),
(1, @Alcohol, 'Desperados Red', 0),
(1, @Alcohol, 'Desperados Tropical', 0),                     -- CHECK flavour
(1, @Alcohol, 'Red Stripe bottle', 0),
(1, @Alcohol, 'Prestige bottle', 0),
(1, @Alcohol, 'Carib Lager bottle', 0),
(1, @Alcohol, 'Coors Light bottle', 0),
(1, @Alcohol, 'Corona Extra bottle', 0),
(1, @Alcohol, 'Coronita bottle', 0),
(1, @Alcohol, 'Corona Cero 0.0', 0),
(1, @Alcohol, 'Old Milwaukee can', 0);                       -- CHECK

-- ---------------------------------------------------------------------------
-- One sell unit per new product so everything is ready for a price:
-- fridge items get 'Cold single' (cold), chocolate gets 'Each'.
-- Only touches products that have no units yet, so the seed data is untouched.
-- ---------------------------------------------------------------------------
INSERT INTO SellUnits (ProductId, Label, Price, UnitsPerSale, IsCold, IsActive, SortOrder)
SELECT p.Id,
       CASE WHEN p.DepartmentId = @Snacks THEN 'Each' ELSE 'Cold single' END,
       0.00,
       1,
       CASE WHEN p.DepartmentId = @Snacks THEN 0 ELSE 1 END,
       1,
       1
FROM Products p
WHERE p.ShopId = 1
  AND NOT EXISTS (SELECT 1 FROM SellUnits su WHERE su.ProductId = p.Id);
GO
