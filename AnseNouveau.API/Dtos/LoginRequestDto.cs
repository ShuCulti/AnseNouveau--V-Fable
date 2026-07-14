namespace AnseNouveau.API.Dtos
{
    public class LoginRequestDto
    {
        public int? UserId { get; set; }
        public string? DisplayName { get; set; }
        public string Pin { get; set; } = string.Empty;
    }
}
