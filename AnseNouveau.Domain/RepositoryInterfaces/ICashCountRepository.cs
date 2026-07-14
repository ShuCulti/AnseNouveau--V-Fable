using AnseNouveau.Domain.Models;

namespace AnseNouveau.Domain.RepositoryInterfaces
{
    public interface ICashCountRepository
    {
        int Create(CashCount cashCount);
        CashCount? GetByBusinessDate(int shopId, DateTime businessDate);
    }
}
