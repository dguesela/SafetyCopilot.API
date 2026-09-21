namespace SafetyCopilot.API.Services.Interfaces;

public interface IPdfTextService
{
    Task<string> ExtractTextAsync(
        byte[] pdfContent,
        CancellationToken cancellationToken = default);
}