using AnseNouveau.Domain.Models;

namespace AnseNouveau.Domain.RepositoryInterfaces
{
    public interface IProductRepository
    {
        List<Product> GetAll(int shopId);
        Product? GetById(int id);
        Product? GetByBarcode(int shopId, string barcode);
        List<Product> SearchByName(int shopId, string term);
        int Create(Product product);
        bool Update(Product product);
    }
}
