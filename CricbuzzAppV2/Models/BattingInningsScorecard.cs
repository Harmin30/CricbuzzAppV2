using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CricbuzzAppV2.Models
{
    public class BattingInningsScorecard
    {
        [Key] // ✅ EXPLICIT PRIMARY KEY
        public int BattingScorecardId { get; set; }

        [Required]
        public int MatchInningsId { get; set; }

        [ValidateNever]
        public MatchInnings MatchInnings { get; set; } = null!;

        [Required(ErrorMessage = "Please select a batsman")]
        public int PlayerId { get; set; }

        [ValidateNever]
        public Player Player { get; set; } = null!;

        [Required(ErrorMessage = "Runs are required")]
        [Range(0, 500)]
        public int Runs { get; set; }

        [Required(ErrorMessage = "Balls faced are required")]
        [Range(0, 500)]
        public int BallsFaced { get; set; }

        [Range(0, 200)]
        public int Fours { get; set; }

        [Range(0, 200)]
        public int Sixes { get; set; }

        [Required]
        [Range(1, 11)]
        public int BattingPosition { get; set; }

        public string? HowOut { get; set; }
    }
}
