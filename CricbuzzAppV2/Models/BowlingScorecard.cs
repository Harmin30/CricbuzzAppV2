using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CricbuzzAppV2.Models
{
    public class BowlingScorecard
    {
        [Key]
        public int BowlingScorecardId { get; set; }

        [Required]
        public int MatchInningsId { get; set; }

        // 🚫 Do not validate navigation
        [ValidateNever]
        public MatchInnings MatchInnings { get; set; } = null!;

        [Required(ErrorMessage = "Please select a bowler")]
        public int PlayerId { get; set; }

        // 🚫 Do not validate navigation
        [ValidateNever]
        public Player Player { get; set; } = null!;

        [Required(ErrorMessage = "Overs are required")]
        [Range(0, 50, ErrorMessage = "Overs cannot be negative")]
        public double Overs { get; set; }

        [Range(0, 50, ErrorMessage = "Maidens cannot be negative")]
        public int Maidens { get; set; }

        [Range(0, 500, ErrorMessage = "Runs cannot be negative")]
        public int RunsConceded { get; set; }

        [Range(0, 10, ErrorMessage = "Wickets must be between 0 and 10")]
        public int Wickets { get; set; }
    }
}
