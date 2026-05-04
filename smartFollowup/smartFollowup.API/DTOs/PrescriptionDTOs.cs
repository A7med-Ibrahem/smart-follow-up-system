namespace SmartFollowUp.API.DTOs
{
    public class CreatePrescriptionRequestDto
    {
        public long CaseId { get; set; }
        public string? Instructions { get; set; }
        public List<CreateMedicationDto> Medications { get; set; } = new List<CreateMedicationDto>();
    }

    public class CreateMedicationDto
    {
        public string MedicationName { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public int? DurationDays { get; set; }
    }

    public class PrescriptionResponseDto
    {
        public long Id { get; set; }
        public long CaseId { get; set; }
        public string? Instructions { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MedicationResponseDto> Medications { get; set; } = new List<MedicationResponseDto>();
    }

    public class MedicationResponseDto
    {
        public long Id { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public int? DurationDays { get; set; }
    }
}