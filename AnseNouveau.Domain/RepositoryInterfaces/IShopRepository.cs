using AnseNouveau.Domain.Models;

namespace AnseNouveau.Domain.RepositoryInterfaces
{
    public interface IShopRepository
    {
        List<Shop> GetAll();
        Shop? GetById(int id);
        int Create(Shop shop);
        bool Update(Shop shop);
    }
}
