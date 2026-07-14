using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.Reports;

namespace AnseNouveau.Domain.RepositoryInterfaces
{
    // Sales and SaleLines are immutable: insert only, no update or delete ever.
    public interface ISaleRepository
    {
        // Inserts the sale, its lines, the stock movements and the stock decrement in one transaction.
        int Create(Sale sale, IEnumerable<StockMovement> movements);
        Sale? GetById(int id);
        List<Sale> GetByDateRange(int shopId, DateTime fromUtc, DateTime toUtc);
        bool HasRefund(int saleId);
        ZReport GetZReport(int shopId, DateTime fromUtc, DateTime toUtc);
        List<ProfitReportRow> GetProfitReport(int shopId, DateTime fromUtc, DateTime toUtc);
        decimal GetCashTotal(int shopId, DateTime fromUtc, DateTime toUtc);
    }
}
