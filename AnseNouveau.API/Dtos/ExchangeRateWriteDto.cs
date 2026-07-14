namespace AnseNouveau.API.Dtos
{
    public class ExchangeRateWriteDto
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal RateToBase { get; set; }
    }
}
