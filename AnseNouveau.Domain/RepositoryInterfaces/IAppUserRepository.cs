using AnseNouveau.Domain.Models;

namespace AnseNouveau.Domain.RepositoryInterfaces
{
    public interface IAppUserRepository
    {
        List<AppUser> GetByShop(int shopId);
        AppUser? GetById(int id);
        int Create(AppUser appUser);
        bool Update(AppUser appUser);
    }
}
