using System.ComponentModel.DataAnnotations;

namespace SafetyCopilot.API.Models
{
    public class User
    {
        public Guid Id { get; set; }
            = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string Email { get; set; }
            = string.Empty;

        [Required]
        [MaxLength(500)]
        public string PasswordHash { get; set; }
            = string.Empty;

        [MaxLength(200)]
        public string? DisplayName { get; set; }

        public string? OpenAiApiKeyEncrypted { get; set; }

        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;

        public DateTime? UpdatedAtUtc { get; set; }

        public ICollection<Project> Projects { get; set; }
            = new List<Project>();

        public ICollection<HumanClassification>
            HumanClassifications
        { get; set; }
            = new List<HumanClassification>();
    }
}
