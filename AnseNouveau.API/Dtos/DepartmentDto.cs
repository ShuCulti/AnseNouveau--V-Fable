namespace AnseNouveau.API.Dtos
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
