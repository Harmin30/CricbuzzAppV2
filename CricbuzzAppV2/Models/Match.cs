using System.ComponentModel.DataAnnotations;

namespace CricbuzzAppV2.Models
{
    public enum MatchStatus
    {
        Upcoming = 0,
        Live = 1,
        Completed = 2,
        Abandoned = 3
    }

    public class Match
    {
        public int MatchId { get; set; }

        [Required(ErrorMessage = "Team A is required")]
        public int TeamAId { get; set; }

        [Required(ErrorMessage = "Team B is required")]
        public int TeamBId { get; set; }

        public int? WinnerTeamId { get; set; }

        [Required(ErrorMessage = "Match type is required")]
        [StringLength(20)]
        public string MatchType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Match date is required")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Venue is required")]
        [StringLength(150)]
        public string Venue { get; set; } = string.Empty;

        // 🔹 Navigation properties (unchanged)
        public Team? TeamA { get; set; }
        public Team? TeamB { get; set; }
        public Team? WinnerTeam { get; set; }

        // 🔥 NEW FIELDS (FOR MARKET-READY SCORECARDS)

        /// <summary>
        /// Overs per innings.
        /// NULL = unlimited overs (Test matches)
        /// </summary>
        [Range(1, 200, ErrorMessage = "Overs must be greater than 0")]
        public int? OversLimit { get; set; }

        /// <summary>
        /// 1 = Limited overs (T20/ODI/Custom)
        /// 2 = Test matches
        /// </summary>
        [Range(1, 4)]
        public int MaxInningsPerTeam { get; set; } = 1;

        public MatchStatus Status { get; set; } = MatchStatus.Upcoming;

        public string? ResultDescription { get; set; }


        // Optional but useful later (toss logic)
        public int? TossWinnerTeamId { get; set; }
        public bool? ElectedToBat { get; set; }

        // 🔹 Existing relationship (will be replaced later)
        public ICollection<Scorecard>? Scorecards { get; set; }

        // 🔹 Existing computed properties (UNCHANGED)
        public string DisplayName =>
            $"{TeamA?.TeamName} vs {TeamB?.TeamName}";

        public string DisplayNameWithType =>
            $"{TeamA?.TeamName} vs {TeamB?.TeamName} ({MatchType} | {Date:dd MMM yyyy} | {Venue})";

        public ICollection<MatchInnings> MatchInnings { get; set; }
    = new List<MatchInnings>();

    }

}
