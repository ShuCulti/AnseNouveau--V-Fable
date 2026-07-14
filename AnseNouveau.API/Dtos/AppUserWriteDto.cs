namespace AnseNouveau.API.Dtos
{
    public class AppUserWriteDto
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        // Required on create; on update null means keep the current PIN.
        public string? Pin { get; set; }
    }
}
