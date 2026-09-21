using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SafetyCopilot.API.DTOs
{

    public class UploadRequirementDocumentRequest
    {
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}