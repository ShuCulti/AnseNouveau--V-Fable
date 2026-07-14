namespace AnseNouveau.Domain.Reports
{
    public class ZReport
    {
        public DateTime BusinessDate { get; set; }
        public decimal SalesTotal { get; set; }
        public decimal RefundTotal { get; set; }
        public decimal NetTotal { get; set; }
        public decimal CashTotal { get; set; }
        public decimal CardTotal { get; set; }
        public decimal OtherTotal { get; set; }
        public int SaleCount { get; set; }
        public List<DepartmentSalesRow> Departments { get; set; } = new();
    }
}
