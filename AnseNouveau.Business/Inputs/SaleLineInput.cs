namespace AnseNouveau.Business.Inputs
{
    // One cart line as entered at the POS: either a sell unit reference or an ad-hoc name + price.
    public class SaleLineInput
    {
        public int? SellUnitId { get; set; }
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public decimal Qty { get; set; }
    }
}
