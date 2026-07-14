namespace AnseNouveau.Domain.Models
{
    public class Shop
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ReceiptHeader { get; set; }
        public string? ReceiptFooter { get; set; }
        public string BaseCurrency { get; set; } = string.Empty;
        public decimal TaxRatePercent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
