namespace AnseNouveau.Domain.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string PinHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
