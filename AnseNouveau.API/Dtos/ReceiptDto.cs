namespace AnseNouveau.API.Dtos
{
    // Everything a receipt needs; shaped so an ESC/POS printer step can consume it later.
    public class ReceiptDto
    {
        public int SaleId { get; set; }
        public DateTime SaleTimeUtc { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string? ReceiptHeader { get; set; }
        public string? ReceiptFooter { get; set; }
        public List<SaleLineDto> Lines { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public string BaseCurrency { get; set; } = string.Empty;
        public string TenderCurrency { get; set; } = string.Empty;
        public decimal TenderRate { get; set; }
        public decimal TotalInTenderCurrency { get; set; }
        public decimal? AmountTendered { get; set; }
        public decimal? ChangeGiven { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
