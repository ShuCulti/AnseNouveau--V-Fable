namespace AnseNouveau.Domain.Models
{
    public class StockCount
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public int UserId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? Notes { get; set; }
    }
}
