using AnseNouveau.Domain.Models;

namespace AnseNouveau.Business.Interfaces
{
    public interface IPriceService
    {
        bool ChangePrice(int sellUnitId, decimal newPrice, int changedByUserId);
        List<PriceHistory> GetHistory(int sellUnitId);
    }
}
