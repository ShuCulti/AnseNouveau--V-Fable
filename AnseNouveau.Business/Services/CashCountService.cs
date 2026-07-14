using AnseNouveau.Business.Interfaces;
using AnseNouveau.Domain.Exceptions;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;

namespace AnseNouveau.Business.Services
{
    public class CashCountService : ICashCountService
    {
        private readonly ICashCountRepository _cashCountRepository;
        private readonly ISaleRepository _saleRepository;

        public CashCountService(ICashCountRepository cashCountRepository, ISaleRepository saleRepository)
        {
            _cashCountRepository = cashCountRepository;
            _saleRepository = saleRepository;
        }

        // Expected cash = float + cash sales - cash refunds for the business date (base currency).
        public decimal GetExpectedCash(int shopId, DateTime businessDate, decimal floatAmount)
        {
            (DateTime fromUtc, DateTime toUtc) = BusinessDay.ToUtcRange(businessDate);
            return floatAmount + _saleRepository.GetCashTotal(shopId, fromUtc, toUtc);
        }

        public CashCount SaveCount(int shopId, int userId, DateTime businessDate, decimal floatAmount,
            decimal countedCash, string? notes)
        {
            if (_cashCountRepository.GetByBusinessDate(shopId, businessDate) != null)
            {
                throw new ValidationException("A cash count already exists for this business date.");
            }
            decimal expectedCash = GetExpectedCash(shopId, businessDate, floatAmount);
            CashCount cashCount = new()
            {
                ShopId = shopId,
                UserId = userId,
                BusinessDate = businessDate.Date,
                FloatAmount = floatAmount,
                ExpectedCash = expectedCash,
                CountedCash = countedCash,
                Difference = countedCash - expectedCash,
                ClosedAt = DateTime.UtcNow,
                Notes = notes
            };
            cashCount.Id = _cashCountRepository.Create(cashCount);
            return cashCount;
        }

        public CashCount? GetByBusinessDate(int shopId, DateTime businessDate)
        {
            return _cashCountRepository.GetByBusinessDate(shopId, businessDate);
        }
    }
}
