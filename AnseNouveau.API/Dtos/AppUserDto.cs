namespace AnseNouveau.API.Dtos
{
    public class AppUserDto
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
