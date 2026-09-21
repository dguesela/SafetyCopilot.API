using System.ComponentModel.DataAnnotations;

namespace SafetyCopilot.API.Models
{
    
    public class AiClassification
    {
        public Guid Id { get; set; }
            = Guid.NewGuid();

        public Guid RequirementId { get; set; }

        public bool IsHazard { get; set; }

        public double? Confidence { get; set; }

        public string? Explanation { get; set; }

        [MaxLength(200)]
        public string? ModelName { get; set; }

        public DateTime ClassifiedAtUtc { get; set; }
            = DateTime.UtcNow;

        public Requirement? Requirement { get; set; }
    }
}
