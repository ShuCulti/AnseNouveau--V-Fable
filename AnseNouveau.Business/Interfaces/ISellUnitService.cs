using AnseNouveau.Domain.Models;

namespace AnseNouveau.Business.Interfaces
{
    public interface ISellUnitService
    {
        SellUnit? GetById(int id);
        List<SellUnit> GetByProduct(int productId);
        int Create(SellUnit sellUnit);
        bool Update(SellUnit sellUnit);
    }
}
