namespace SafetyCopilot.API.Models
{
    public class ExperimentTask
    {
        public Guid Id { get; set; }
            = Guid.NewGuid();

        public Guid SessionId { get; set; }

        public ExperimentSession Session { get; set; }
            = null!;

        public string TaskCode { get; set; } = "";

        public string? Description { get; set; }

        public DateTime? StartedAtUtc { get; set; }

        public DateTime? CompletedAtUtc { get; set; }

        public bool Completed { get; set; }
    }
}
