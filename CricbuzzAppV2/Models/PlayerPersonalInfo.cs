using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CricbuzzAppV2.Models
{
    public class PlayerPersonalInfo
    {
        [Key]
        public int PlayerPersonalInfoId { get; set; }

        // Foreign Key to Player
        [Required]
        public int PlayerId { get; set; }

        [ForeignKey("PlayerId")]
        public Player? Player { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [StringLength(100)]
        public string? BirthPlace { get; set; }

        [StringLength(50)]
        public string Height { get; set; }

        [StringLength(50)]
        public string BattingStyle { get; set; }

        [StringLength(50)]
        public string BowlingStyle { get; set; }

        [StringLength(50)]
        public string Role { get; set; } // Optional: Redundant, but useful
    }
}
