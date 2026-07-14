using AnseNouveau.Business.Inputs;
using AnseNouveau.Business.Interfaces;
using AnseNouveau.Domain.Exceptions;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.Reports;
using AnseNouveau.Domain.RepositoryInterfaces;

namespace AnseNouveau.Business.Services
{
    public class SaleService : ISaleService
    {
        private static readonly string[] ValidPaymentMethods = { "Cash", "Card", "Other" };

        private readonly ISaleRepository _saleRepository;
        private readonly ISellUnitRepository _sellUnitRepository;
        private readonly IProductRepository _productRepository;
        private readonly IShopRepository _shopRepository;
        private readonly IExchangeRateRepository _exchangeRateRepository;

        public SaleService(
            ISaleRepository saleRepository,
            ISellUnitRepository sellUnitRepository,
            IProductRepository productRepository,
            IShopRepository shopRepository,
            IExchangeRateRepository exchangeRateRepository)
        {
            _saleRepository = saleRepository;
            _sellUnitRepository = sellUnitRepository;
            _productRepository = productRepository;
            _shopRepository = shopRepository;
            _exchangeRateRepository = exchangeRateRepository;
        }

        public Sale CompleteSale(int shopId, int userId, List<SaleLineInput> lines, string paymentMethod,
            string tenderCurrency, decimal? amountTendered)
        {
            if (lines == null || lines.Count == 0)
            {
                throw new ValidationException("A sale needs at least one line.");
            }
            if (!ValidPaymentMethods.Contains(paymentMethod))
            {
                throw new ValidationException("Payment method must be Cash, Card or Other.");
            }
            Shop shop = _shopRepository.GetById(shopId)
                ?? throw new ValidationException("Shop not found.");

            decimal tenderRate = ResolveTenderRate(shop, tenderCurrency);

            List<SaleLine> saleLines = new();
            List<StockMovement> movements = new();
            DateTime now = DateTime.UtcNow;
            foreach (SaleLineInput input in lines)
            {
                if (input.Qty <= 0)
                {
                    throw new ValidationException("Line quantity must be greater than zero.");
                }
                if (input.SellUnitId.HasValue)
                {
                    SellUnit sellUnit = _sellUnitRepository.GetById(input.SellUnitId.Value)
                        ?? throw new ValidationException($"Sell unit {input.SellUnitId} not found.");
                    if (!sellUnit.IsActive)
                    {
                        throw new ValidationException($"Sell unit {input.SellUnitId} is not active.");
                    }
                    Product product = _productRepository.GetById(sellUnit.ProductId)
                        ?? throw new ValidationException($"Product for sell unit {input.SellUnitId} not found.");
                    if (product.ShopId != shopId)
                    {
                        throw new ValidationException($"Sell unit {input.SellUnitId} belongs to another shop.");
                    }
                    saleLines.Add(new SaleLine
                    {
                        SellUnitId = sellUnit.Id,
                        NameSnapshot = $"{product.Name} ({sellUnit.Label})",
                        PriceSnapshot = sellUnit.Price,
                        Qty = input.Qty,
                        LineTotal = Round2(sellUnit.Price * input.Qty)
                    });
                    movements.Add(new StockMovement
                    {
                        ProductId = product.Id,
                        QtyDelta = -(input.Qty * sellUnit.UnitsPerSale),
                        MovementType = "Sale",
                        UserId = userId,
                        MovedAt = now
                    });
                }
                else
                {
                    // Ad-hoc unknown item: name + price typed at the register, no stock movement.
                    if (string.IsNullOrWhiteSpace(input.Name) || !input.Price.HasValue)
                    {
                        throw new ValidationException("Ad-hoc lines need a name and a price.");
                    }
                    if (input.Price.Value < 0)
                    {
                        throw new ValidationException("Line price cannot be negative.");
                    }
                    saleLines.Add(new SaleLine
                    {
                        SellUnitId = null,
                        NameSnapshot = input.Name,
                        PriceSnapshot = input.Price.Value,
                        Qty = input.Qty,
                        LineTotal = Round2(input.Price.Value * input.Qty)
                    });
                }
            }

            decimal totalAmount = saleLines.Sum(l => l.LineTotal);
            decimal totalInTender = Round2(totalAmount / tenderRate);

            decimal? changeGiven = null;
            if (paymentMethod == "Cash")
            {
                if (!amountTendered.HasValue)
                {
                    throw new ValidationException("Cash sales need the amount tendered.");
                }
                if (amountTendered.Value < totalInTender)
                {
                    throw new ValidationException("Amount tendered is less than the total.");
                }
                changeGiven = Round2(amountTendered.Value - totalInTender);
            }
            else
            {
                amountTendered = null;
            }

            Sale sale = new()
            {
                ShopId = shopId,
                UserId = userId,
                SaleTimeUtc = now,
                Status = "Completed",
                TotalAmount = totalAmount,
                PaymentMethod = paymentMethod,
                TenderCurrency = tenderCurrency,
                TenderRate = tenderRate,
                AmountTendered = amountTendered,
                ChangeGiven = changeGiven,
                Lines = saleLines
            };
            _saleRepository.Create(sale, movements);
            return sale;
        }

        // A refund is a new immutable sale referencing the original, with positive stock movements.
        public Sale? RefundSale(int originalSaleId, int userId)
        {
            Sale? original = _saleRepository.GetById(originalSaleId);
            if (original == null)
            {
                return null;
            }
            if (original.Status != "Completed")
            {
                throw new ValidationException("Only completed sales can be refunded.");
            }
            if (_saleRepository.HasRefund(originalSaleId))
            {
                throw new ValidationException("This sale has already been refunded.");
            }

            DateTime now = DateTime.UtcNow;
            List<SaleLine> refundLines = new();
            List<StockMovement> movements = new();
            foreach (SaleLine line in original.Lines)
            {
                refundLines.Add(new SaleLine
                {
                    SellUnitId = line.SellUnitId,
                    NameSnapshot = line.NameSnapshot,
                    PriceSnapshot = line.PriceSnapshot,
                    Qty = -line.Qty,
                    LineTotal = -line.LineTotal
                });
                if (line.SellUnitId.HasValue)
                {
                    SellUnit? sellUnit = _sellUnitRepository.GetById(line.SellUnitId.Value);
                    if (sellUnit != null)
                    {
                        movements.Add(new StockMovement
                        {
                            ProductId = sellUnit.ProductId,
                            QtyDelta = line.Qty * sellUnit.UnitsPerSale,
                            MovementType = "Refund",
                            UserId = userId,
                            MovedAt = now
                        });
                    }
                }
            }

            Sale refund = new()
            {
                ShopId = original.ShopId,
                UserId = userId,
                SaleTimeUtc = now,
                Status = "Refund",
                RefundOfSaleId = original.Id,
                TotalAmount = -original.TotalAmount,
                PaymentMethod = original.PaymentMethod,
                TenderCurrency = original.TenderCurrency,
                TenderRate = original.TenderRate,
                Lines = refundLines
            };
            _saleRepository.Create(refund, movements);
            return refund;
        }

        public Sale? GetById(int id)
        {
            return _saleRepository.GetById(id);
        }

        public List<Sale> GetByDateRange(int shopId, DateTime fromDate, DateTime toDate)
        {
            (DateTime fromUtc, DateTime toUtc) = BusinessDay.ToUtcRange(fromDate, toDate);
            return _saleRepository.GetByDateRange(shopId, fromUtc, toUtc);
        }

        public ZReport GetZReport(int shopId, DateTime businessDate)
        {
            (DateTime fromUtc, DateTime toUtc) = BusinessDay.ToUtcRange(businessDate);
            ZReport report = _saleRepository.GetZReport(shopId, fromUtc, toUtc);
            report.BusinessDate = businessDate.Date;
            return report;
        }

        private decimal ResolveTenderRate(Shop shop, string tenderCurrency)
        {
            if (tenderCurrency == shop.BaseCurrency)
            {
                return 1m;
            }
            ExchangeRate? rate = _exchangeRateRepository.GetByCurrency(shop.Id, tenderCurrency);
            if (rate == null || rate.RateToBase <= 0)
            {
                throw new ValidationException($"No exchange rate configured for {tenderCurrency}.");
            }
            return rate.RateToBase;
        }

        private static decimal Round2(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
