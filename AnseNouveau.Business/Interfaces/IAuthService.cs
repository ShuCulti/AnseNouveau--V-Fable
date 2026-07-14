using AnseNouveau.Domain.Models;

namespace AnseNouveau.Business.Interfaces
{
    public interface IAuthService
    {
        AppUser? Login(int shopId, int? userId, string? displayName, string pin);
        List<AppUser> GetActiveUsers(int shopId);
    }
}
