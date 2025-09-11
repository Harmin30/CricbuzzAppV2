using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CricbuzzAppV2.Models
{
    public class Player
    {
        public int PlayerId { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        public string? ImageUrl { get; set; } = string.Empty;

        [Required]
        public int TeamId { get; set; }

        [ValidateNever] // 🚀 Prevents MVC from requiring this during form validation
        public Team Team { get; set; } = null!;
    }
}
