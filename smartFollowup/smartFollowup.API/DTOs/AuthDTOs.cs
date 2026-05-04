namespace SmartFollowUp.API.DTOs
{
    // Request — Login
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // Request — Register Doctor
    public class RegisterDoctorRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string Hospital { get; set; } = string.Empty;
    }

    // Request — Register Patient
    public class RegisterPatientRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? ChronicDiseases { get; set; }
        public string? Allergies { get; set; }
        public string? CurrentMedications { get; set; }
    }

    // Response — Login & Register
    public class AuthResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
    // Request — Doctor Registration Request
    public class DoctorRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Specialty { get; set; }
        public string? LicenseNumber { get; set; }
    }

    public class ForgotPasswordRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}