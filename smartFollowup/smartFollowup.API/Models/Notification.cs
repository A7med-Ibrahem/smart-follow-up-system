namespace SmartFollowUp.API.Models
{
    public class Notification
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long? AlertId { get; set; }
        public string Type { get; set; } = string.Empty; // reminder, alert, prescription, instruction
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User User { get; set; } = null!;
        public Alert? Alert { get; set; }
    }
}