using AnseNouveau.Domain.Models;

namespace AnseNouveau.Business.Interfaces
{
    public interface IAppUserService
    {
        List<AppUser> GetByShop(int shopId);
        AppUser? GetById(int id);
        int Create(AppUser appUser, string pin);
        bool Update(AppUser appUser, string? newPin);
    }
}
