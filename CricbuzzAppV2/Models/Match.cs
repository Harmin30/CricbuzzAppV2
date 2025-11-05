using System.ComponentModel.DataAnnotations;

namespace CricbuzzAppV2.Models
{
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

        public Team? TeamA { get; set; }
        public Team? TeamB { get; set; }
        public Team? WinnerTeam { get; set; }

        public ICollection<Scorecard>? Scorecards { get; set; }

        public string DisplayName =>
            $"{TeamA?.TeamName} vs {TeamB?.TeamName}";

        public string DisplayNameWithType =>
            $"{TeamA?.TeamName} vs {TeamB?.TeamName} ({MatchType} | {Date:dd MMM yyyy} | {Venue})";
    }
}
