using AnseNouveau.Domain.Models;

namespace AnseNouveau.Domain.RepositoryInterfaces
{
    public interface ISellUnitRepository
    {
        SellUnit? GetById(int id);
        List<SellUnit> GetByProduct(int productId);
        int Create(SellUnit sellUnit);
        // Update never touches Price; price changes go through UpdatePrice so PriceHistory is always written.
        bool Update(SellUnit sellUnit);
        bool UpdatePrice(int sellUnitId, decimal newPrice, int changedByUserId);
    }
}
