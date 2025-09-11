using System;

namespace CricbuzzAppV2.Models
{
    public class PlayerStats
    {
        public int PlayerStatsId { get; set; }

        // Foreign key to Player
        public int PlayerId { get; set; }
        public Player? Player { get; set; }

        // Cricket format
        public enum CricketFormat { Test, ODI, T20 }
        public CricketFormat Format { get; set; }

        // Batting stats (optional)
        public int? MatchesPlayed { get; set; }
        public int? Innings { get; set; }
        public int? Runs { get; set; }
        public int? BallsFaced { get; set; }
        public int? HighestScore { get; set; }
        public int? Hundreds { get; set; }
        public int? Fifties { get; set; }
        public int? DoubleHundreds { get; set; }

        // Bowling stats (optional)
        public int? BallsBowled { get; set; }
        public int? RunsConceded { get; set; }
        public int? Wickets { get; set; }
        public string? BestBowling { get; set; }

        // Calculated properties
        public double BattingAverage => (Innings.HasValue && Innings.Value > 0 && Runs.HasValue)
                                        ? Math.Round((double)Runs.Value / Innings.Value, 2)
                                        : 0;

        public double StrikeRate => (BallsFaced.HasValue && BallsFaced.Value > 0 && Runs.HasValue)
                                   ? Math.Round((double)Runs.Value / BallsFaced.Value * 100, 2)
                                   : 0;

        public double BowlingAverage => (Wickets.HasValue && Wickets.Value > 0 && RunsConceded.HasValue)
                                       ? Math.Round((double)RunsConceded.Value / Wickets.Value, 2)
                                       : 0;

        public double Economy => (BallsBowled.HasValue && BallsBowled.Value > 0 && RunsConceded.HasValue)
                                 ? Math.Round((double)RunsConceded.Value / (BallsBowled.Value / 6.0), 2)
                                 : 0;
    }
}
