using System.Text.Json;

namespace CricbuzzAppV2.Helpers
{
    public static class AuditFormatter
    {
        // Fields that should NEVER be shown to admins
        private static readonly HashSet<string> IgnoreFields = new()
        {
            "MatchId", "TeamAId", "TeamBId",
            "TeamA", "TeamB", "WinnerTeam",
            "Scorecards", "DisplayName",
            "DisplayNameWithType",
            "TossWinnerTeamId",
            "ElectedToBat"
        };

        public static (string Summary, Dictionary<string, List<string>> Groups)
            Format(string action, string entity, string? details, Dictionary<string, string> teams)
        {
            var groups = new Dictionary<string, List<string>>();
            var highlights = new List<string>();

            if (string.IsNullOrWhiteSpace(details) || !details.Trim().StartsWith("{"))
                return ($"{entity} {action.ToLower()}", groups);

            using var doc = JsonDocument.Parse(details);

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                var field = prop.Name;
                if (IgnoreFields.Contains(field))
                    continue;

                var group = GetGroup(entity, field);
                if (!groups.ContainsKey(group))
                    groups[group] = new List<string>();

                if (prop.Value.ValueKind == JsonValueKind.Object &&
                    prop.Value.TryGetProperty("NewValue", out var newValElem))
                {
                    var newVal = Normalize(field, newValElem.ToString(), teams);
                    groups[group].Add(BuildSentence(entity, field, newVal));
                    AddHighlight(field, newVal, highlights);
                }
                else
                {
                    var newVal = Normalize(field, prop.Value.ToString(), teams);
                    groups[group].Add($"{Pretty(field)}: {newVal}");
                    AddHighlight(field, newVal, highlights);
                }
            }

            string summary = BuildSummary(entity, action, highlights);
            return (summary, groups);
        }

        // ---------------- HELPERS ----------------

        private static string BuildSummary(string entity, string action, List<string> highlights)
        {
            if (highlights.Count == 0)
                return $"{entity} {action.ToLower()}";

            return $"{entity} {action.ToLower()} • " + string.Join(" • ", highlights.Take(2));
        }

        private static void AddHighlight(string field, string value, List<string> list)
        {
            if (field == "WinnerTeamId")
                list.Add($"Winner: {value}");
            if (field == "Status" && value == "Completed")
                list.Add("Result declared");
        }

        private static string GetGroup(string entity, string field)
        {
            if (entity == "Match")
            {
                if (field is "WinnerTeamId" or "ResultDescription" or "Status")
                    return "🏆 Result";

                if (field is "Date" or "Venue" or "MatchType")
                    return "📅 Match Info";

                if (field is "MaxInningsPerTeam" or "OversLimit")
                    return "⚙️ Configuration";
            }

            return "Other";
        }

        private static string Normalize(string field, string value, Dictionary<string, string> teams)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "not set";

            if (DateTime.TryParse(value, out var dt))
                return dt.ToString("dd MMM yyyy, hh:mm tt");

            if (field == "Status")
                return value switch
                {
                    "0" => "Scheduled",
                    "1" => "Live",
                    "2" => "Completed",
                    _ => value
                };

            if (field == "WinnerTeamId" && teams.TryGetValue(value, out var team))
                return team;

            return value;
        }

        private static string Pretty(string field)
        {
            return field switch
            {
                "WinnerTeamId" => "Winner",
                "ResultDescription" => "Result",
                "MaxInningsPerTeam" => "Max innings per team",
                _ => field
            };
        }

        private static string BuildSentence(string entity, string field, string newVal)
        {
            if (field == "WinnerTeamId")
                return $"Winner: {newVal}";
            if (field == "ResultDescription")
                return $"Result: {newVal}";
            if (field == "Status")
                return $"Status: {newVal}";

            return $"{Pretty(field)}: {newVal}";
        }
    }
}
