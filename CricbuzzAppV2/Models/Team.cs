using System.ComponentModel.DataAnnotations;

namespace CricbuzzAppV2.Models
{
    public class Team
    {
        public int TeamId { get; set; }

        [Required(ErrorMessage = "Team name is required")]
        [Display(Name = "Team Name")]
        public string TeamName { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Coach name is required")]
        [Display(Name = "Coach")]
        public string Coach { get; set; }

        // Final saved image path / URL
        public string? ImageUrl { get; set; }

        public ICollection<Player> Players { get; set; } = new List<Player>();
    }
}
