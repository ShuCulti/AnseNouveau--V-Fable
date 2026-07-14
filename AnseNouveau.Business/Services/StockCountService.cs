using AnseNouveau.Business.Interfaces;
using AnseNouveau.Domain.Exceptions;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.Reports;
using AnseNouveau.Domain.RepositoryInterfaces;

namespace AnseNouveau.Business.Services
{
    public class StockCountService : IStockCountService
    {
        private readonly IStockCountRepository _stockCountRepository;
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly IProductRepository _productRepository;

        public StockCountService(
            IStockCountRepository stockCountRepository,
            IStockMovementRepository stockMovementRepository,
            IProductRepository productRepository)
        {
            _stockCountRepository = stockCountRepository;
            _stockMovementRepository = stockMovementRepository;
            _productRepository = productRepository;
        }

        public int Open(int shopId, int userId, string? notes)
        {
            if (_stockCountRepository.GetOpenByShop(shopId) != null)
            {
                throw new ValidationException("A stock count is already open for this shop.");
            }
            return _stockCountRepository.Create(new StockCount
            {
                ShopId = shopId,
                UserId = userId,
                StartedAt = DateTime.UtcNow,
                Notes = notes
            });
        }

        public StockCount? GetById(int id)
        {
            return _stockCountRepository.GetById(id);
        }

        public StockCount? GetOpen(int shopId)
        {
            return _stockCountRepository.GetOpenByShop(shopId);
        }

        // ExpectedQty is frozen at scan time; rescans only update CountedQty.
        public int SubmitLine(int stockCountId, int productId, decimal countedQty)
        {
            StockCount stockCount = _stockCountRepository.GetById(stockCountId)
                ?? throw new ValidationException("Stock count not found.");
            if (stockCount.ClosedAt.HasValue)
            {
                throw new ValidationException("This stock count is already closed.");
            }
            if (countedQty < 0)
            {
                throw new ValidationException("Counted quantity cannot be negative.");
            }
            Product product = _productRepository.GetById(productId)
                ?? throw new ValidationException($"Product {productId} not found.");
            return _stockCountRepository.UpsertLine(new StockCountLine
            {
                StockCountId = stockCountId,
                ProductId = productId,
                ExpectedQty = product.StockQty,
                CountedQty = countedQty
            });
        }

        // Closing writes one CountFix movement per differing line, then stamps ClosedAt.
        public bool Close(int stockCountId, int userId)
        {
            StockCount? stockCount = _stockCountRepository.GetById(stockCountId);
            if (stockCount == null || stockCount.ClosedAt.HasValue)
            {
                return false;
            }
            DateTime now = DateTime.UtcNow;
            List<StockMovement> movements = _stockCountRepository.GetLines(stockCountId)
                .Where(l => l.CountedQty != l.ExpectedQty)
                .Select(l => new StockMovement
                {
                    ProductId = l.ProductId,
                    QtyDelta = l.CountedQty - l.ExpectedQty,
                    MovementType = "CountFix",
                    StockCountId = stockCountId,
                    Reason = "stock count",
                    UserId = userId,
                    MovedAt = now
                })
                .ToList();
            if (movements.Count > 0)
            {
                _stockMovementRepository.AddRange(movements);
            }
            return _stockCountRepository.Close(stockCountId);
        }

        public List<StockCountDiffRow> GetDifferenceReport(int stockCountId)
        {
            return _stockCountRepository.GetDifferenceReport(stockCountId);
        }
    }
}
