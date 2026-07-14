namespace AnseNouveau.API.Dtos
{
    public class ShopWriteDto
    {
        public string Name { get; set; } = string.Empty;
        public string? ReceiptHeader { get; set; }
        public string? ReceiptFooter { get; set; }
        public string BaseCurrency { get; set; } = string.Empty;
        public decimal TaxRatePercent { get; set; }
    }
}
