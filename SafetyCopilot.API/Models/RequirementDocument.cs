using System.ComponentModel.DataAnnotations;

namespace SafetyCopilot.API.Models
{
    public class RequirementDocument
    {
        public Guid Id { get; set; }
            = Guid.NewGuid();

        public Guid ProjectId { get; set; }

        [Required]
        [MaxLength(500)]
        public string FileName { get; set; }
            = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; }
            = "application/pdf";

        public long FileSizeBytes { get; set; }

        [Required]
        public byte[] PdfContent { get; set; }
            = Array.Empty<byte>();

        public string? ExtractedText { get; set; }

        public DateTime UploadedAtUtc { get; set; }
            = DateTime.UtcNow;

        public DateTime? ProcessedAtUtc { get; set; }

        public Project? Project { get; set; }

        public ICollection<Requirement>
            Requirements
        { get; set; }
            = new List<Requirement>();
    }
}
