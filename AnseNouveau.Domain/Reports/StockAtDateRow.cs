namespace AnseNouveau.Domain.Reports
{
    public class StockAtDateRow
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Qty { get; set; }
    }
}
