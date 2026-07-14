using AnseNouveau.Domain.Models;

namespace AnseNouveau.Domain.RepositoryInterfaces
{
    public interface IPriceHistoryRepository
    {
        List<PriceHistory> GetBySellUnit(int sellUnitId);
    }
}
