namespace AnseNouveau.Domain.Models
{
    public class SellUnit
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Label { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal UnitsPerSale { get; set; }
        public bool IsCold { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }
}
