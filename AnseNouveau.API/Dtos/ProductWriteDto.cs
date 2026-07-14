namespace AnseNouveau.API.Dtos
{
    public class ProductWriteDto
    {
        public int? DepartmentId { get; set; }
        public string? Barcode { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal CostPrice { get; set; }
        public bool IsActive { get; set; }
        // Create only: opening stock enters as an Adjustment movement.
        public decimal OpeningQty { get; set; }
        // Create only: lets the POS create an unknown product with one sell unit in a single call.
        public SellUnitWriteDto? InitialSellUnit { get; set; }
    }
}
