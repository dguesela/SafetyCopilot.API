/*
The ExperimentSession class represents a session of an experiment conducted by a user within a specific project. 
It contains properties to track the session's unique identifier, associated user and project, experimental condition, status, 
start and completion timestamps, as well as lists of tasks and measurements related to the session:

HumanAlone
AIAlone
HumanAIRecommendations
HumanAIExplanations
HumanAIUncertainty 
 
 */

using System.Diagnostics.Metrics;

namespace SafetyCopilot.API.Models
{
    public class ExperimentSession
    {
        public Guid Id { get; set; }
            = Guid.NewGuid();

        public Guid UserId { get; set; }

        public AppUser User { get; set; } = null!;

        public Guid ProjectId { get; set; }

        public Project Project { get; set; } = null!;

        public string Condition { get; set; } = "";

        public string Status { get; set; } = "Active";

        public DateTime StartedAtUtc { get; set; }
            = DateTime.UtcNow;

        public DateTime? CompletedAtUtc { get; set; }

        public List<ExperimentTask> Tasks { get; set; }
            = [];

        public List<Measurement> Measurements { get; set; }
            = [];
    }
}
