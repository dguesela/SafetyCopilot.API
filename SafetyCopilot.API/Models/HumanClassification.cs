namespace SafetyCopilot.API.Models
{
    
    public class HumanClassification
    {
        public Guid Id { get; set; }
            = Guid.NewGuid();

        public Guid RequirementId { get; set; }

        public Guid UserId { get; set; }

        public bool IsHazard { get; set; }

        public DateTime ClassifiedAtUtc { get; set; }
            = DateTime.UtcNow;

        public Requirement? Requirement { get; set; }

        public User? User { get; set; }
    }
}
