namespace AnseNouveau.API.Dtos
{
    public class StockAtDateRowDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Qty { get; set; }
    }
}
