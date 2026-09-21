namespace SafetyCopilot.API.DTOs.Auth
{
    public class RegisterResponse
    {
        public Guid UserId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public string UserName { get; internal set; }
    }
}
