namespace SafetyCopilot.API.Services.Interfaces
{

    public interface IRequirementParserService
    {
        IReadOnlyList<ParsedRequirement>
            ParseRequirements(string text);
    }

    public record ParsedRequirement(
        string? RequirementNumber,
        string RequirementText,
        int SequenceNumber
    );
}