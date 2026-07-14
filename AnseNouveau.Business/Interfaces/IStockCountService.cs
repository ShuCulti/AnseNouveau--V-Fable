using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.Reports;

namespace AnseNouveau.Business.Interfaces
{
    public interface IStockCountService
    {
        int Open(int shopId, int userId, string? notes);
        StockCount? GetById(int id);
        StockCount? GetOpen(int shopId);
        int SubmitLine(int stockCountId, int productId, decimal countedQty);
        bool Close(int stockCountId, int userId);
        List<StockCountDiffRow> GetDifferenceReport(int stockCountId);
    }
}
