/*
 This class represents a measurement taken during an experiment session. 
It includes properties for the measurement type, value, unit, and the time it was recorded. 
The MeasurementType property can take on various values such as:
 HazardIdentificationAccuracy
FalsePositiveRate
FalseNegativeRate
CompletionTime
CognitiveWorkload
Trust
OverReliance
UnderReliance
DecisionQuality
 */

namespace SafetyCopilot.API.Models
{
    public class Measurement
    {
        public Guid Id { get; set; }
            = Guid.NewGuid();

        public Guid SessionId { get; set; }

        public ExperimentSession Session { get; set; }
            = null!;

        public string MeasurementType { get; set; } = "";

        public decimal Value { get; set; }

        public string? Unit { get; set; }

        public DateTime RecordedAtUtc { get; set; }
            = DateTime.UtcNow;
    }
}
