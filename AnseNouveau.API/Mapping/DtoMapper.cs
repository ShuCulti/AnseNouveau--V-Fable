using AnseNouveau.API.Dtos;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.Reports;

namespace AnseNouveau.API.Mapping
{
    // Manual mapping helpers shared by the controllers.
    public static class DtoMapper
    {
        public static ShopDto ToDto(Shop shop)
        {
            return new ShopDto
            {
                Id = shop.Id,
                Name = shop.Name,
                ReceiptHeader = shop.ReceiptHeader,
                ReceiptFooter = shop.ReceiptFooter,
                BaseCurrency = shop.BaseCurrency,
                TaxRatePercent = shop.TaxRatePercent,
                CreatedAt = shop.CreatedAt
            };
        }

        public static AppUserDto ToDto(AppUser user)
        {
            return new AppUserDto
            {
                Id = user.Id,
                ShopId = user.ShopId,
                DisplayName = user.DisplayName,
                Role = user.Role,
                IsActive = user.IsActive
            };
        }

        public static DepartmentDto ToDto(Department department)
        {
            return new DepartmentDto
            {
                Id = department.Id,
                ShopId = department.ShopId,
                Name = department.Name,
                SortOrder = department.SortOrder
            };
        }

        public static ExchangeRateDto ToDto(ExchangeRate rate)
        {
            return new ExchangeRateDto
            {
                Id = rate.Id,
                ShopId = rate.ShopId,
                CurrencyCode = rate.CurrencyCode,
                RateToBase = rate.RateToBase,
                UpdatedAt = rate.UpdatedAt
            };
        }

        public static SellUnitDto ToDto(SellUnit sellUnit)
        {
            return new SellUnitDto
            {
                Id = sellUnit.Id,
                ProductId = sellUnit.ProductId,
                Label = sellUnit.Label,
                Price = sellUnit.Price,
                UnitsPerSale = sellUnit.UnitsPerSale,
                IsCold = sellUnit.IsCold,
                IsActive = sellUnit.IsActive,
                SortOrder = sellUnit.SortOrder
            };
        }

        public static ProductDto ToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                ShopId = product.ShopId,
                DepartmentId = product.DepartmentId,
                Barcode = product.Barcode,
                Name = product.Name,
                StockQty = product.StockQty,
                CostPrice = product.CostPrice,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                SellUnits = product.SellUnits.Select(ToDto).ToList()
            };
        }

        public static PriceHistoryDto ToDto(PriceHistory history)
        {
            return new PriceHistoryDto
            {
                Id = history.Id,
                SellUnitId = history.SellUnitId,
                OldPrice = history.OldPrice,
                NewPrice = history.NewPrice,
                ChangedByUserId = history.ChangedByUserId,
                ChangedAt = history.ChangedAt
            };
        }

        public static SaleLineDto ToDto(SaleLine line)
        {
            return new SaleLineDto
            {
                Id = line.Id,
                SaleId = line.SaleId,
                SellUnitId = line.SellUnitId,
                NameSnapshot = line.NameSnapshot,
                PriceSnapshot = line.PriceSnapshot,
                Qty = line.Qty,
                LineTotal = line.LineTotal
            };
        }

        public static SaleDto ToDto(Sale sale)
        {
            return new SaleDto
            {
                Id = sale.Id,
                ShopId = sale.ShopId,
                UserId = sale.UserId,
                SaleTimeUtc = sale.SaleTimeUtc,
                Status = sale.Status,
                RefundOfSaleId = sale.RefundOfSaleId,
                TotalAmount = sale.TotalAmount,
                PaymentMethod = sale.PaymentMethod,
                TenderCurrency = sale.TenderCurrency,
                TenderRate = sale.TenderRate,
                AmountTendered = sale.AmountTendered,
                ChangeGiven = sale.ChangeGiven,
                Lines = sale.Lines.Select(ToDto).ToList()
            };
        }

        public static ReceiptDto ToReceiptDto(Sale sale, Shop shop)
        {
            return new ReceiptDto
            {
                SaleId = sale.Id,
                SaleTimeUtc = sale.SaleTimeUtc,
                ShopName = shop.Name,
                ReceiptHeader = shop.ReceiptHeader,
                ReceiptFooter = shop.ReceiptFooter,
                Lines = sale.Lines.Select(ToDto).ToList(),
                TotalAmount = sale.TotalAmount,
                BaseCurrency = shop.BaseCurrency,
                TenderCurrency = sale.TenderCurrency,
                TenderRate = sale.TenderRate,
                TotalInTenderCurrency = Math.Round(sale.TotalAmount / sale.TenderRate, 2, MidpointRounding.AwayFromZero),
                AmountTendered = sale.AmountTendered,
                ChangeGiven = sale.ChangeGiven,
                PaymentMethod = sale.PaymentMethod
            };
        }

        public static StockMovementDto ToDto(StockMovement movement)
        {
            return new StockMovementDto
            {
                Id = movement.Id,
                ProductId = movement.ProductId,
                QtyDelta = movement.QtyDelta,
                MovementType = movement.MovementType,
                SaleId = movement.SaleId,
                StockCountId = movement.StockCountId,
                Reason = movement.Reason,
                UserId = movement.UserId,
                MovedAt = movement.MovedAt
            };
        }

        public static StockCountDto ToDto(StockCount stockCount)
        {
            return new StockCountDto
            {
                Id = stockCount.Id,
                ShopId = stockCount.ShopId,
                UserId = stockCount.UserId,
                StartedAt = stockCount.StartedAt,
                ClosedAt = stockCount.ClosedAt,
                Notes = stockCount.Notes
            };
        }

        public static StockCountDiffDto ToDto(StockCountDiffRow row)
        {
            return new StockCountDiffDto
            {
                ProductId = row.ProductId,
                ProductName = row.ProductName,
                ExpectedQty = row.ExpectedQty,
                CountedQty = row.CountedQty,
                Difference = row.Difference
            };
        }

        public static CashCountDto ToDto(CashCount cashCount)
        {
            return new CashCountDto
            {
                Id = cashCount.Id,
                ShopId = cashCount.ShopId,
                UserId = cashCount.UserId,
                BusinessDate = cashCount.BusinessDate,
                FloatAmount = cashCount.FloatAmount,
                ExpectedCash = cashCount.ExpectedCash,
                CountedCash = cashCount.CountedCash,
                Difference = cashCount.Difference,
                ClosedAt = cashCount.ClosedAt,
                Notes = cashCount.Notes
            };
        }

        public static ZReportDto ToDto(ZReport report)
        {
            return new ZReportDto
            {
                BusinessDate = report.BusinessDate,
                SalesTotal = report.SalesTotal,
                RefundTotal = report.RefundTotal,
                NetTotal = report.NetTotal,
                CashTotal = report.CashTotal,
                CardTotal = report.CardTotal,
                OtherTotal = report.OtherTotal,
                SaleCount = report.SaleCount,
                Departments = report.Departments.Select(d => new DepartmentSalesDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    Total = d.Total
                }).ToList()
            };
        }

        public static StockReportRowDto ToDto(StockReportRow row)
        {
            return new StockReportRowDto
            {
                ProductId = row.ProductId,
                ProductName = row.ProductName,
                OpeningQty = row.OpeningQty,
                SoldQty = row.SoldQty,
                DeliveredQty = row.DeliveredQty,
                AdjustedQty = row.AdjustedQty,
                ClosingQty = row.ClosingQty
            };
        }

        public static ProfitReportRowDto ToDto(ProfitReportRow row)
        {
            return new ProfitReportRowDto
            {
                ProductId = row.ProductId,
                ProductName = row.ProductName,
                UnitsSold = row.UnitsSold,
                Revenue = row.Revenue,
                Cost = row.Cost,
                Profit = row.Profit
            };
        }

        public static StockAtDateRowDto ToDto(StockAtDateRow row)
        {
            return new StockAtDateRowDto
            {
                ProductId = row.ProductId,
                ProductName = row.ProductName,
                Qty = row.Qty
            };
        }
    }
}
