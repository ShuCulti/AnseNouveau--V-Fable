namespace AnseNouveau.API.Dtos
{
    public class SaleDto
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public int UserId { get; set; }
        public DateTime SaleTimeUtc { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? RefundOfSaleId { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string TenderCurrency { get; set; } = string.Empty;
        public decimal TenderRate { get; set; }
        public decimal? AmountTendered { get; set; }
        public decimal? ChangeGiven { get; set; }
        public List<SaleLineDto> Lines { get; set; } = new();
    }
}
