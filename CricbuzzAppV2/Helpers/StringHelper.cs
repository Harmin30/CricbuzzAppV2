namespace CricbuzzAppV2.Helpers
{
    public static class StringHelper
    {
        public static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Trim + collapse multiple spaces into one
            return string.Join(" ",
                input.Trim()
                     .Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
