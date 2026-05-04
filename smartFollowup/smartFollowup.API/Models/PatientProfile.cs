namespace SmartFollowUp.API.Models
{
    public class PatientProfile
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; } // male, female
        public string? ChronicDiseases { get; set; }
        public string? Allergies { get; set; }
        public string? CurrentMedications { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;
    }
}