namespace SmartFollowUp.API.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // doctor, patient, admin
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = false;
        public string? ActivationToken { get; set; }
        public string? ResetToken { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public DoctorProfile? DoctorProfile { get; set; }
        public PatientProfile? PatientProfile { get; set; }
        public ICollection<Case> DoctorCases { get; set; } = new List<Case>();
        public ICollection<Case> PatientCases { get; set; } = new List<Case>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}