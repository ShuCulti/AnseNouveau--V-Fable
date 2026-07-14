namespace AnseNouveau.Domain.Models
{
    public class ExchangeRate
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal RateToBase { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
