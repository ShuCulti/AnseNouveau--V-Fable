using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.Reports;

namespace AnseNouveau.Domain.RepositoryInterfaces
{
    // StockMovements is an append-only ledger: insert only, no update or delete ever.
    public interface IStockMovementRepository
    {
        // Inserts the movements and applies the deltas to Products.StockQty in one transaction.
        void AddRange(IEnumerable<StockMovement> movements);
        List<StockMovement> GetByProduct(int productId, DateTime fromUtc, DateTime toUtc);
        List<StockReportRow> GetStockReport(int shopId, DateTime fromUtc, DateTime toUtc);
        List<StockAtDateRow> GetStockAtDate(int shopId, DateTime atUtc);
    }
}
