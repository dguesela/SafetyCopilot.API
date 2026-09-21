using System.ComponentModel.DataAnnotations;

namespace SafetyCopilot.API.Models
{
   
    public class Requirement
    {
        public Guid Id { get; set; }
            = Guid.NewGuid();

        public Guid DocumentId { get; set; }

        [MaxLength(100)]
        public string? RequirementNumber { get; set; }

        [Required]
        public string RequirementText { get; set; }
            = string.Empty;

        public int SequenceNumber { get; set; }

        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;

        public RequirementDocument? Document { get; set; }

        public HumanClassification?
            HumanClassification
        { get; set; }

        public AiClassification?
            AiClassification
        { get; set; }
    }
}
