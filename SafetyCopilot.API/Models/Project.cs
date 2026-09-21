using System.ComponentModel.DataAnnotations;

namespace SafetyCopilot.API.Models
{
    public class Project
    {
        public Guid Id { get; set; }
            = Guid.NewGuid();

        public Guid UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
            = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;

        public DateTime? UpdatedAtUtc { get; set; }

        public User? User { get; set; }

        public ICollection<RequirementDocument>
            RequirementDocuments
        { get; set; }
            = new List<RequirementDocument>();
    }
}
