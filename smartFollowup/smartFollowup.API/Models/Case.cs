namespace SmartFollowUp.API.Models
{
    public class Case
    {
        public long Id { get; set; }
        public long DoctorId { get; set; }
        public long PatientId { get; set; }
        public string? OperationType { get; set; }
        public DateTime? OperationDate { get; set; }
        public string? InitialTreatment { get; set; }
        public string Status { get; set; } = "active"; // active, closed, archived
        public string CurrentRiskLevel { get; set; } = "stable"; // stable, moderate, critical
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }

        // Navigation Properties
        public User Doctor { get; set; } = null!;
        public User Patient { get; set; } = null!;
        public ICollection<DailyReport> DailyReports { get; set; } = new List<DailyReport>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<DoctorNote> DoctorNotes { get; set; } = new List<DoctorNote>();
        public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
        public ICollection<WoundImage> WoundImages { get; set; } = new List<WoundImage>();
    }
}