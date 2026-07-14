using AnseNouveau.Business.Interfaces;
using AnseNouveau.Domain.Exceptions;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;

namespace AnseNouveau.Business.Services
{
    public class SellUnitService : ISellUnitService
    {
        private readonly ISellUnitRepository _sellUnitRepository;

        public SellUnitService(ISellUnitRepository sellUnitRepository)
        {
            _sellUnitRepository = sellUnitRepository;
        }

        public SellUnit? GetById(int id)
        {
            return _sellUnitRepository.GetById(id);
        }

        public List<SellUnit> GetByProduct(int productId)
        {
            return _sellUnitRepository.GetByProduct(productId);
        }

        public int Create(SellUnit sellUnit)
        {
            Validate(sellUnit);
            return _sellUnitRepository.Create(sellUnit);
        }

        public bool Update(SellUnit sellUnit)
        {
            Validate(sellUnit);
            return _sellUnitRepository.Update(sellUnit);
        }

        private static void Validate(SellUnit sellUnit)
        {
            if (string.IsNullOrWhiteSpace(sellUnit.Label))
            {
                throw new ValidationException("Sell unit label is required.");
            }
            if (sellUnit.Price < 0)
            {
                throw new ValidationException("Price cannot be negative.");
            }
            if (sellUnit.UnitsPerSale <= 0)
            {
                throw new ValidationException("Units per sale must be greater than zero.");
            }
        }
    }
}
