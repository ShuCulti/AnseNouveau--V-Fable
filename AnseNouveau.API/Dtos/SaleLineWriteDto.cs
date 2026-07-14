namespace AnseNouveau.API.Dtos
{
    public class SaleLineWriteDto
    {
        public int? SellUnitId { get; set; }
        // Ad-hoc unknown items: no SellUnitId, name and price typed at the register.
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public decimal Qty { get; set; }
    }
}
