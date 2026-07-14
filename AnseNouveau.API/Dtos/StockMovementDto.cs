namespace AnseNouveau.API.Dtos
{
    public class StockMovementDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public decimal QtyDelta { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public int? SaleId { get; set; }
        public int? StockCountId { get; set; }
        public string? Reason { get; set; }
        public int UserId { get; set; }
        public DateTime MovedAt { get; set; }
    }
}
