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

        // Null means this result predates revision assessment.
        public bool? RevisionNeeded { get; set; }

        public string? SuggestedRevision { get; set; }

        public string? RevisionReason { get; set; }

        [MaxLength(16)]
        public string? RevisionDecision { get; set; }

        public DateTime? RevisionReviewedAtUtc { get; set; }

        [MaxLength(200)]
        public string? ModelName { get; set; }

        public DateTime ClassifiedAtUtc { get; set; }
            = DateTime.UtcNow;

        public Requirement? Requirement { get; set; }
    }
}
