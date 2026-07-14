namespace AnseNouveau.Domain.Models
{
    public class SaleLine
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int? SellUnitId { get; set; }
        public string NameSnapshot { get; set; } = string.Empty;
        public decimal PriceSnapshot { get; set; }
        public decimal Qty { get; set; }
        public decimal LineTotal { get; set; }
    }
}
