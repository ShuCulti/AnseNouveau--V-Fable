namespace AnseNouveau.API.Dtos
{
    public class CashCountWriteDto
    {
        public DateTime BusinessDate { get; set; }
        public decimal FloatAmount { get; set; }
        public decimal CountedCash { get; set; }
        public string? Notes { get; set; }
    }
}
