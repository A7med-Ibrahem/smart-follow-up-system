using Microsoft.EntityFrameworkCore;
using SmartFollowUp.API.Data;
using SmartFollowUp.API.DTOs;

namespace SmartFollowUp.API.Services
{
    public class PatientService
    {
        private readonly AppDbContext _context;

        public PatientService(AppDbContext context)
        {
            _context = context;
        }

        // Get Patient Profile
        public async Task<PatientProfileResponseDto?> GetPatientProfileAsync(long patientId)
        {
            var user = await _context.Users
                .Include(u => u.PatientProfile)
                .FirstOrDefaultAsync(u => u.Id == patientId && u.Role == "patient");

            if (user == null) return null;

            var latestCase = await _context.Cases
                .Where(c => c.PatientId == patientId)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            return new PatientProfileResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Age = user.PatientProfile?.Age,
                Gender = user.PatientProfile?.Gender,
                ChronicDiseases = user.PatientProfile?.ChronicDiseases,
                Allergies = user.PatientProfile?.Allergies,
                CurrentMedications = user.PatientProfile?.CurrentMedications,
                RiskLevel = latestCase?.CurrentRiskLevel ?? "stable"
            };
        }

        // Update Patient Profile
        public async Task<PatientProfileResponseDto?> UpdatePatientProfileAsync(long patientId, UpdatePatientProfileRequestDto request)
        {
            var user = await _context.Users
                .Include(u => u.PatientProfile)
                .FirstOrDefaultAsync(u => u.Id == patientId && u.Role == "patient");

            if (user == null) return null;

            // Update Phone
            if (request.Phone != null)
                user.Phone = request.Phone;

            // Update Profile
            if (user.PatientProfile != null)
            {
                if (request.Allergies != null)
                    user.PatientProfile.Allergies = request.Allergies;

                if (request.CurrentMedications != null)
                    user.PatientProfile.CurrentMedications = request.CurrentMedications;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetPatientProfileAsync(patientId);
        }
    }
}