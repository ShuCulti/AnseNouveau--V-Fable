namespace AnseNouveau.API.Dtos
{
    public class CashCountDto
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public int UserId { get; set; }
        public DateTime BusinessDate { get; set; }
        public decimal FloatAmount { get; set; }
        public decimal ExpectedCash { get; set; }
        public decimal CountedCash { get; set; }
        public decimal Difference { get; set; }
        public DateTime ClosedAt { get; set; }
        public string? Notes { get; set; }
    }
}
