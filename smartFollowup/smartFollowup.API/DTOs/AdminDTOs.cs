namespace SmartFollowUp.API.DTOs
{
    public class DoctorRequestResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Specialty { get; set; }
        public string? LicenseNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RejectDoctorRequestDto
    {
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class AnalyticsResponseDto
    {
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalCases { get; set; }
        public int ActiveCases { get; set; }
        public int TotalReports { get; set; }
        public int CriticalAlerts { get; set; }
        public int PendingDoctorRequests { get; set; }
    }
}