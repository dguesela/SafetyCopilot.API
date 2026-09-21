namespace SafetyCopilot.API.Models
{
    public class AppUser
    {
        public Guid UserId { get; set; } = Guid.NewGuid();

        public string UserName { get; set; } = "";

        public string Email { get; set; } = "";

        public string PasswordHash { get; set; } = "";

        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = "";

        public string Role { get; set; } = "SafetyEngineer";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}
