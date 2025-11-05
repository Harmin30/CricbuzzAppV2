using System.ComponentModel.DataAnnotations;

namespace CricbuzzAppV2.Models
{
    public enum InningsStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2
    }

    public class MatchInnings
    {
        public int MatchInningsId { get; set; }

        // 🔗 Match relation
        [Required]
        public int MatchId { get; set; }
        public Match? Match { get; set; }

        // 🏏 Teams
        [Required]
        public int BattingTeamId { get; set; }
        public Team? BattingTeam { get; set; }

        [Required]
        public int BowlingTeamId { get; set; }
        public Team? BowlingTeam { get; set; }

        // 🔢 Innings info

        public int InningsNumber { get; set; } // 1,2,3,4 (Test support)

        // ✅ ADD THESE
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }

        // 📊 Team totals
        public int TotalRuns { get; set; } = 0;
        public int WicketsLost { get; set; } = 0;

        public double OversBowled { get; set; } = 0;

        // ➕ Extras
        public int Extras { get; set; } = 0;

        public InningsStatus Status { get; set; } = InningsStatus.NotStarted;

        // 🔹 Navigation
        public ICollection<BattingInningsScorecard> BattingScorecards { get; set; }
    = new List<BattingInningsScorecard>();

        public ICollection<BowlingScorecard>? BowlingScorecards { get; set; }
    }
}
