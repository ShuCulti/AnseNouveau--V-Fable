using AnseNouveau.Business.Inputs;
using AnseNouveau.Domain.Models;
using AnseNouveau.Domain.Reports;

namespace AnseNouveau.Business.Interfaces
{
    public interface ISaleService
    {
        Sale CompleteSale(int shopId, int userId, List<SaleLineInput> lines, string paymentMethod,
            string tenderCurrency, decimal? amountTendered);
        Sale? RefundSale(int originalSaleId, int userId);
        Sale? GetById(int id);
        List<Sale> GetByDateRange(int shopId, DateTime fromDate, DateTime toDate);
        ZReport GetZReport(int shopId, DateTime businessDate);
    }
}
