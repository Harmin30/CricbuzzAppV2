using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CricbuzzAppV2.ViewModels
{
    public class PlayerEditViewModel
    {
        public int PlayerId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        public int TeamId { get; set; }

        // Image options
        public IFormFile? ImageFile { get; set; }
        public string? ImageUrlInput { get; set; }

        // For preview
        public string? ExistingImageUrl { get; set; }
    }
}
