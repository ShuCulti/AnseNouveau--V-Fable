using AnseNouveau.Business.Interfaces;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;

namespace AnseNouveau.Business.Services
{
    public class ShopService : IShopService
    {
        private readonly IShopRepository _shopRepository;

        public ShopService(IShopRepository shopRepository)
        {
            _shopRepository = shopRepository;
        }

        public List<Shop> GetAll()
        {
            return _shopRepository.GetAll();
        }

        public Shop? GetById(int id)
        {
            return _shopRepository.GetById(id);
        }

        public int Create(Shop shop)
        {
            return _shopRepository.Create(shop);
        }

        public bool Update(Shop shop)
        {
            return _shopRepository.Update(shop);
        }
    }
}
