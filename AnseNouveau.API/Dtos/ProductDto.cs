namespace AnseNouveau.API.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public int? DepartmentId { get; set; }
        public string? Barcode { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal StockQty { get; set; }
        public decimal CostPrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<SellUnitDto> SellUnits { get; set; } = new();
    }
}
