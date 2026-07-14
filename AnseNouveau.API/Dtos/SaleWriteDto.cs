namespace AnseNouveau.API.Dtos
{
    public class SaleWriteDto
    {
        public List<SaleLineWriteDto> Lines { get; set; } = new();
        public string PaymentMethod { get; set; } = string.Empty;
        public string TenderCurrency { get; set; } = string.Empty;
        public decimal? AmountTendered { get; set; }
    }
}
