using AnseNouveau.Domain.Models;

namespace AnseNouveau.Domain.RepositoryInterfaces
{
    public interface IExchangeRateRepository
    {
        List<ExchangeRate> GetByShop(int shopId);
        ExchangeRate? GetByCurrency(int shopId, string currencyCode);
        int Create(ExchangeRate exchangeRate);
        bool Update(ExchangeRate exchangeRate);
    }
}
