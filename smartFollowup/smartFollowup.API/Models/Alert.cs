namespace SmartFollowUp.API.Models
{
    public class Alert
    {
        public long Id { get; set; }
        public long CaseId { get; set; }
        public long ReportId { get; set; }
        public string AlertType { get; set; } = string.Empty; // critical, escalation, warning
        public string Priority { get; set; } = "low"; // low, medium, high
        public string Status { get; set; } = "open"; // open, handled
        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
        public DateTime? HandledAt { get; set; }

        // Navigation Properties
        public Case Case { get; set; } = null!;
        public DailyReport DailyReport { get; set; } = null!;
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}