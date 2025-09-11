using System;

namespace CricbuzzAppV2.Models
{
    public class PlayerStats
    {
        public int PlayerStatsId { get; set; }
        public int PlayerId { get; set; }
        public Player? Player { get; set; }

        public enum CricketFormat { Test, ODI, T20 }
        public CricketFormat Format { get; set; }

        // Raw performance data
        public int MatchesPlayed { get; set; }
        public int Innings { get; set; }
        public int Runs { get; set; }
        public int BallsFaced { get; set; } // for Strike Rate
        public int HighestScore { get; set; }
        public int Hundreds { get; set; }
        public int Fifties { get; set; }

        public int BallsBowled { get; set; }
        public int RunsConceded { get; set; } // for Economy
        public int Wickets { get; set; }
        public string BestBowling { get; set; }

        // Calculated properties
        public double BattingAverage => Innings > 0 ? Math.Round((double)Runs / Innings, 2) : 0;
        public double StrikeRate => BallsFaced > 0 ? Math.Round((double)Runs / BallsFaced * 100, 2) : 0;
        public double BowlingAverage => Wickets > 0 ? Math.Round((double)RunsConceded / Wickets, 2) : 0;
        public double Economy => BallsBowled > 0 ? Math.Round((double)RunsConceded / (BallsBowled / 6.0), 2) : 0;
    }
}
