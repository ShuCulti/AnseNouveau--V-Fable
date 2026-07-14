namespace AnseNouveau.API.Dtos
{
    public class AdjustmentWriteDto
    {
        public int ProductId { get; set; }
        public decimal QtyDelta { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
