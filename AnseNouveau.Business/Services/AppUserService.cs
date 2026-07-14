using AnseNouveau.Business.Interfaces;
using AnseNouveau.Domain.Exceptions;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.RepositoryInterfaces;

namespace AnseNouveau.Business.Services
{
    public class AppUserService : IAppUserService
    {
        private readonly IAppUserRepository _appUserRepository;

        public AppUserService(IAppUserRepository appUserRepository)
        {
            _appUserRepository = appUserRepository;
        }

        public List<AppUser> GetByShop(int shopId)
        {
            return _appUserRepository.GetByShop(shopId);
        }

        public AppUser? GetById(int id)
        {
            return _appUserRepository.GetById(id);
        }

        public int Create(AppUser appUser, string pin)
        {
            ValidatePin(pin);
            appUser.PinHash = BCrypt.Net.BCrypt.HashPassword(pin);
            return _appUserRepository.Create(appUser);
        }

        public bool Update(AppUser appUser, string? newPin)
        {
            AppUser? existing = _appUserRepository.GetById(appUser.Id);
            if (existing == null)
            {
                return false;
            }
            if (newPin != null)
            {
                ValidatePin(newPin);
                appUser.PinHash = BCrypt.Net.BCrypt.HashPassword(newPin);
            }
            else
            {
                appUser.PinHash = existing.PinHash;
            }
            return _appUserRepository.Update(appUser);
        }

        private static void ValidatePin(string pin)
        {
            if (pin.Length != 4 || !pin.All(char.IsDigit))
            {
                throw new ValidationException("PIN must be exactly 4 digits.");
            }
        }
    }
}
