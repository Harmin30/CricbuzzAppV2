using System.Numerics;

namespace CricbuzzAppV2.Models
{
    public class Team
    {
        public int TeamId { get; set; } // Primary Key
        public string TeamName { get; set; } // Team Name
        public string Country { get; set; } // Country of the Team
        public string Coach { get; set; } // Coach Name
        public string? ImageUrl { get; set; } // ✅ Team Logo URL

        // Navigation property to players
        public ICollection<Player> Players { get; set; } = new List<Player>();
    }
}
