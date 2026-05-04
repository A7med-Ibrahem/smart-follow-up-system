namespace SmartFollowUp.API.DTOs
{
    public class UpdatePatientProfileRequestDto
    {
        public string? Phone { get; set; }
        public string? Allergies { get; set; }
        public string? CurrentMedications { get; set; }
    }

    public class PatientProfileResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? ChronicDiseases { get; set; }
        public string? Allergies { get; set; }
        public string? CurrentMedications { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
    }
}