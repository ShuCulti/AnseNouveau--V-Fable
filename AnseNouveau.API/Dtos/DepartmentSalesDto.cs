namespace AnseNouveau.API.Dtos
{
    public class DepartmentSalesDto
    {
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}
