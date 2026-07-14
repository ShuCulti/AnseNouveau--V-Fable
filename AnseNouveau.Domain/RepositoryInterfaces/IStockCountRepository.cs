using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.Reports;

namespace AnseNouveau.Domain.RepositoryInterfaces
{
    public interface IStockCountRepository
    {
        int Create(StockCount stockCount);
        StockCount? GetById(int id);
        StockCount? GetOpenByShop(int shopId);
        bool Close(int id);
        int UpsertLine(StockCountLine line);
        List<StockCountLine> GetLines(int stockCountId);
        List<StockCountDiffRow> GetDifferenceReport(int stockCountId);
    }
}
