using AnseNouveau.Business.Interfaces;
using AnseNouveau.Domain.Exceptions;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;

namespace AnseNouveau.Business.Services
{
    public class PriceService : IPriceService
    {
        private readonly ISellUnitRepository _sellUnitRepository;
        private readonly IPriceHistoryRepository _priceHistoryRepository;

        public PriceService(ISellUnitRepository sellUnitRepository, IPriceHistoryRepository priceHistoryRepository)
        {
            _sellUnitRepository = sellUnitRepository;
            _priceHistoryRepository = priceHistoryRepository;
        }

        // The repository writes the PriceHistory row in the same transaction as the price update.
        public bool ChangePrice(int sellUnitId, decimal newPrice, int changedByUserId)
        {
            if (newPrice < 0)
            {
                throw new ValidationException("Price cannot be negative.");
            }
            return _sellUnitRepository.UpdatePrice(sellUnitId, newPrice, changedByUserId);
        }

        public List<PriceHistory> GetHistory(int sellUnitId)
        {
            return _priceHistoryRepository.GetBySellUnit(sellUnitId);
        }
    }
}
