namespace AnseNouveau.API.Dtos
{
    public class StockCountDiffDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ExpectedQty { get; set; }
        public decimal CountedQty { get; set; }
        public decimal Difference { get; set; }
    }
}
