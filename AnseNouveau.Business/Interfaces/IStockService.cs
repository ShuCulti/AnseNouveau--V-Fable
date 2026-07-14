using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.Reports;

namespace AnseNouveau.Business.Interfaces
{
    public interface IStockService
    {
        void RecordDelivery(int productId, decimal qty, int userId, string? reason);
        void RecordAdjustment(int productId, decimal qtyDelta, string reason, int userId);
        List<StockMovement> GetMovements(int productId, DateTime fromDate, DateTime toDate);
        List<StockReportRow> GetStockReport(int shopId, DateTime fromDate, DateTime toDate);
        List<StockAtDateRow> GetStockAtDate(int shopId, DateTime date);
        List<ProfitReportRow> GetProfitReport(int shopId, DateTime fromDate, DateTime toDate);
    }
}
