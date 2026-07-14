namespace AnseNouveau.API.Dtos
{
    public class PriceHistoryDto
    {
        public int Id { get; set; }
        public int SellUnitId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public int ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
