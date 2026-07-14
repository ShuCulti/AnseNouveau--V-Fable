using AnseNouveau.Domain.Models;

namespace AnseNouveau.Business.Interfaces
{
    public interface IShopService
    {
        List<Shop> GetAll();
        Shop? GetById(int id);
        int Create(Shop shop);
        bool Update(Shop shop);
    }
}
