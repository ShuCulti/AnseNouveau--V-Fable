using AnseNouveau.Business.Interfaces;
using AnseNouveau.Domain.Exceptions;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;

namespace AnseNouveau.Business.Services
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly IExchangeRateRepository _exchangeRateRepository;

        public ExchangeRateService(IExchangeRateRepository exchangeRateRepository)
        {
            _exchangeRateRepository = exchangeRateRepository;
        }

        public List<ExchangeRate> GetByShop(int shopId)
        {
            return _exchangeRateRepository.GetByShop(shopId);
        }

        public int Create(ExchangeRate exchangeRate)
        {
            Validate(exchangeRate);
            return _exchangeRateRepository.Create(exchangeRate);
        }

        public bool Update(ExchangeRate exchangeRate)
        {
            Validate(exchangeRate);
            return _exchangeRateRepository.Update(exchangeRate);
        }

        private static void Validate(ExchangeRate exchangeRate)
        {
            if (exchangeRate.RateToBase <= 0)
            {
                throw new ValidationException("Exchange rate must be greater than zero.");
            }
        }
    }
}
