using AnseNouveau.Business.Interfaces;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;

namespace AnseNouveau.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAppUserRepository _appUserRepository;

        public AuthService(IAppUserRepository appUserRepository)
        {
            _appUserRepository = appUserRepository;
        }

        public AppUser? Login(int shopId, int? userId, string? displayName, string pin)
        {
            AppUser? user = null;
            if (userId.HasValue)
            {
                user = _appUserRepository.GetById(userId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(displayName))
            {
                user = _appUserRepository.GetByShop(shopId)
                    .FirstOrDefault(u => u.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
            }

            if (user == null || user.ShopId != shopId || !user.IsActive)
            {
                return null;
            }
            return BCrypt.Net.BCrypt.Verify(pin, user.PinHash) ? user : null;
        }

        public List<AppUser> GetActiveUsers(int shopId)
        {
            return _appUserRepository.GetByShop(shopId).Where(u => u.IsActive).ToList();
        }
    }
}
