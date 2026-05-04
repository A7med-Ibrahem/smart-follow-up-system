namespace SmartFollowUp.API.Models
{
    public class DoctorRequest
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Specialty { get; set; }
        public string? LicenseNumber { get; set; }
        public string Status { get; set; } = "pending"; // pending, approved, rejected
        public string? RejectionReason { get; set; }
        public long? ReviewedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User? Reviewer { get; set; }
    }
}