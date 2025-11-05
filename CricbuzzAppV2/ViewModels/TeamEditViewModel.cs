using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CricbuzzAppV2.ViewModels
{
    public class TeamEditViewModel
    {
        public int TeamId { get; set; }

        [Required]
        public string TeamName { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string Coach { get; set; }

        // Image options (same as Create)
        public IFormFile? ImageFile { get; set; }
        public string? ImageUrlInput { get; set; }

        // For showing current image in Edit
        public string? ExistingImageUrl { get; set; }
    }
}
