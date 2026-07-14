using AnseNouveau.Domain.Models;

namespace AnseNouveau.Business.Interfaces
{
    public interface IExchangeRateService
    {
        List<ExchangeRate> GetByShop(int shopId);
        int Create(ExchangeRate exchangeRate);
        bool Update(ExchangeRate exchangeRate);
    }
}
