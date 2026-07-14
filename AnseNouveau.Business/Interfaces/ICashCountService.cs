using AnseNouveau.Domain.Models;

namespace AnseNouveau.Business.Interfaces
{
    public interface ICashCountService
    {
        decimal GetExpectedCash(int shopId, DateTime businessDate, decimal floatAmount);
        CashCount SaveCount(int shopId, int userId, DateTime businessDate, decimal floatAmount,
            decimal countedCash, string? notes);
        CashCount? GetByBusinessDate(int shopId, DateTime businessDate);
    }
}
