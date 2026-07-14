using AnseNouveau.Business.Interfaces;
using AnseNouveau.Domain.Exceptions;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.Reports;
using AnseNouveau.Domain.RepositoryInterfaces;

namespace AnseNouveau.Business.Services
{
    public class StockService : IStockService
    {
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly ISaleRepository _saleRepository;
        private readonly IProductRepository _productRepository;

        public StockService(
            IStockMovementRepository stockMovementRepository,
            ISaleRepository saleRepository,
            IProductRepository productRepository)
        {
            _stockMovementRepository = stockMovementRepository;
            _saleRepository = saleRepository;
            _productRepository = productRepository;
        }

        public void RecordDelivery(int productId, decimal qty, int userId, string? reason)
        {
            if (qty <= 0)
            {
                throw new ValidationException("Delivery quantity must be greater than zero.");
            }
            EnsureProductExists(productId);
            _stockMovementRepository.AddRange(new[]
            {
                new StockMovement
                {
                    ProductId = productId,
                    QtyDelta = qty,
                    MovementType = "Delivery",
                    Reason = reason,
                    UserId = userId,
                    MovedAt = DateTime.UtcNow
                }
            });
        }

        public void RecordAdjustment(int productId, decimal qtyDelta, string reason, int userId)
        {
            if (qtyDelta == 0)
            {
                throw new ValidationException("Adjustment quantity cannot be zero.");
            }
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ValidationException("Adjustments need a reason.");
            }
            EnsureProductExists(productId);
            _stockMovementRepository.AddRange(new[]
            {
                new StockMovement
                {
                    ProductId = productId,
                    QtyDelta = qtyDelta,
                    MovementType = "Adjustment",
                    Reason = reason,
                    UserId = userId,
                    MovedAt = DateTime.UtcNow
                }
            });
        }

        public List<StockMovement> GetMovements(int productId, DateTime fromDate, DateTime toDate)
        {
            (DateTime fromUtc, DateTime toUtc) = BusinessDay.ToUtcRange(fromDate, toDate);
            return _stockMovementRepository.GetByProduct(productId, fromUtc, toUtc);
        }

        public List<StockReportRow> GetStockReport(int shopId, DateTime fromDate, DateTime toDate)
        {
            (DateTime fromUtc, DateTime toUtc) = BusinessDay.ToUtcRange(fromDate, toDate);
            return _stockMovementRepository.GetStockReport(shopId, fromUtc, toUtc);
        }

        // Stock as of the end of the given business day.
        public List<StockAtDateRow> GetStockAtDate(int shopId, DateTime date)
        {
            (_, DateTime toUtc) = BusinessDay.ToUtcRange(date);
            return _stockMovementRepository.GetStockAtDate(shopId, toUtc);
        }

        public List<ProfitReportRow> GetProfitReport(int shopId, DateTime fromDate, DateTime toDate)
        {
            (DateTime fromUtc, DateTime toUtc) = BusinessDay.ToUtcRange(fromDate, toDate);
            return _saleRepository.GetProfitReport(shopId, fromUtc, toUtc);
        }

        private void EnsureProductExists(int productId)
        {
            if (_productRepository.GetById(productId) == null)
            {
                throw new ValidationException($"Product {productId} not found.");
            }
        }
    }
}
