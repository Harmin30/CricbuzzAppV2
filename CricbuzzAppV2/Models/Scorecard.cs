using System.ComponentModel.DataAnnotations;

namespace CricbuzzAppV2.Models
{
    public class Scorecard
    {
        public int ScorecardId { get; set; }

        [Required]
        public int MatchId { get; set; }
        public Match? Match { get; set; } 

        [Required]
        public int PlayerId { get; set; }
        public Player? Player { get; set; }

        // Batting stats (optional)
        [Range(0, int.MaxValue, ErrorMessage = "Runs cannot be negative")]
        public int? RunsScored { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Balls cannot be negative")]
        public int? BallsFaced { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Fours cannot be negative")]
        public int? Fours { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Sixes cannot be negative")]
        public int? Sixes { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Ones cannot be negative")]
        public int? Ones { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Twos cannot be negative")]
        public int? Twos { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Threes cannot be negative")]
        public int? Threes { get; set; }

        public string? HowOut { get; set; }

        // Bowling stats (optional)
        [Range(0, double.MaxValue, ErrorMessage = "Overs cannot be negative")]
        public double? OversBowled { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Runs conceded cannot be negative")]
        public int? RunsConceded { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Wickets cannot be negative")]
        public int? WicketsTaken { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Maidens cannot be negative")]
        public int? Maidens { get; set; }
    }
}
