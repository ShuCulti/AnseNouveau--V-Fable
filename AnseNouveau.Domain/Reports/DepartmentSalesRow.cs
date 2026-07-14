namespace AnseNouveau.Domain.Reports
{
    public class DepartmentSalesRow
    {
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}
