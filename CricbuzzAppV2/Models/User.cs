namespace CricbuzzAppV2.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }

        // Store the salt separately
        public string PasswordSalt { get; set; }
        // Add role
        public string Role { get; set; } = "Viewer"; // default role
    }
}
