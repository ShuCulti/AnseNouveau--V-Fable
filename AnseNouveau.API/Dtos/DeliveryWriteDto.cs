namespace AnseNouveau.API.Dtos
{
    public class DeliveryWriteDto
    {
        public int ProductId { get; set; }
        public decimal Qty { get; set; }
        public string? Reason { get; set; }
    }
}
