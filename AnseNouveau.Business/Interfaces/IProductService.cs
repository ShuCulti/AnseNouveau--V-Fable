using AnseNouveau.Domain.Models;

namespace AnseNouveau.Business.Interfaces
{
    public interface IProductService
    {
        List<Product> GetAll(int shopId);
        Product? GetById(int id);
        Product? GetByBarcode(int shopId, string barcode);
        List<Product> SearchByName(int shopId, string term);
        int Create(Product product, SellUnit? initialSellUnit, decimal openingQty, int userId);
        bool Update(Product product);
    }
}
