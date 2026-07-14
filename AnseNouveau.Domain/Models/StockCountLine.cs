namespace AnseNouveau.Domain.Models
{
    public class StockCountLine
    {
        public int Id { get; set; }
        public int StockCountId { get; set; }
        public int ProductId { get; set; }
        public decimal ExpectedQty { get; set; }
        public decimal CountedQty { get; set; }
    }
}
