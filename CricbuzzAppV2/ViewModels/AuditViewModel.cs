namespace CricbuzzAppV2.ViewModels
{
    public class AuditViewModel
    {
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = "";
        public string Action { get; set; } = "";

        // One-line summary (collapsed view)
        public string Summary { get; set; } = "";

        // Structured grouped details (expanded view)
        public Dictionary<string, List<string>> Groups { get; set; }
            = new();
    }
}
