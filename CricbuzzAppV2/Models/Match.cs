using System.ComponentModel.DataAnnotations;

namespace CricbuzzAppV2.Models
{
    public class Match
    {
        public int MatchId { get; set; }

        [Required]
        public int TeamAId { get; set; }

        [Required]
        public int TeamBId { get; set; }

        public int? WinnerTeamId { get; set; }

        [Required]
        public string MatchType { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public string Venue { get; set; } = string.Empty;

        // Navigation properties (nullable so model binding doesn’t break)
        public Team? TeamA { get; set; }
        public Team? TeamB { get; set; }
        public Team? WinnerTeam { get; set; }

        public ICollection<Scorecard>? Scorecards { get; set; }

        public string DisplayName => $"{TeamA?.TeamName} vs {TeamB?.TeamName}";
//       Include MatchType in dropdowns
        public string DisplayNameWithType => $"{TeamA?.TeamName} vs {TeamB?.TeamName} ( {MatchType}  | {Date:dd MMM yyyy}  | {Venue} )";
    }
}
