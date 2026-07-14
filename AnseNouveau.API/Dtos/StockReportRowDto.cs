namespace AnseNouveau.API.Dtos
{
    public class StockReportRowDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal OpeningQty { get; set; }
        public decimal SoldQty { get; set; }
        public decimal DeliveredQty { get; set; }
        public decimal AdjustedQty { get; set; }
        public decimal ClosingQty { get; set; }
    }
}
