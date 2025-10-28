using System;
using System.ComponentModel.DataAnnotations;

namespace CricbuzzAppV2.Models
{
    public class Audit
    {
        public int AuditId { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty;  // Created, Updated, Deleted

    [Required]
        public string EntityName { get; set; } = string.Empty;  // Player, Team, Match, etc.

        public string? EntityId { get; set; }  // ID of the affected record

        [Required]
        public string UserName { get; set; } = string.Empty;  // User who performed the action

  public string? Details { get; set; }  // Additional details about the change

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}