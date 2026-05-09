using Microsoft.EntityFrameworkCore;
using SmartFollowUp.API.Data;
using SmartFollowUp.API.DTOs;
using SmartFollowUp.API.Enums;
using SmartFollowUp.API.Models;

namespace SmartFollowUp.API.Services
{
    public class AdminService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public AdminService(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Get All Doctor Requests
        public async Task<List<DoctorRequestResponseDto>> GetDoctorRequestsAsync(string? status = null)
        {
            var query = _context.DoctorRequests.AsQueryable();

            if (status != null && Enum.TryParse<DoctorRequestStatus>(status, true, out var statusEnum))
                query = query.Where(r => r.Status == statusEnum);

            return await query
                .Select(r => new DoctorRequestResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Email = r.Email,
                    Specialty = r.Specialty,
                    LicenseNumber = r.LicenseNumber,
                    Status = r.Status.ToString(),
                    RejectionReason = r.RejectionReason,
                    CreatedAt = r.CreatedAt
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        // Approve Doctor Request
        public async Task<bool> ApproveDoctorRequestAsync(long requestId, long adminId)
        {
            var request = await _context.DoctorRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Status == DoctorRequestStatus.Pending);

            if (request == null) return false;

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Smart@123"),
                Role = UserRole.Doctor,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var doctorProfile = new DoctorProfile
            {
                UserId = user.Id,
                Specialty = request.Specialty,
                LicenseNumber = request.LicenseNumber
            };

            _context.DoctorProfiles.Add(doctorProfile);

            request.Status = DoctorRequestStatus.Approved;
            request.ReviewedBy = adminId;

            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                request.Email,
                request.Name,
                "Smart Follow Up — Account Approved ✅",
                $@"
                <h2>Congratulations, {request.Name}!</h2>
                <p>Your doctor account has been approved.</p>
                <p><strong>Email:</strong> {request.Email}</p>
                <p><strong>Temporary Password:</strong> Smart@123</p>
                <p>Please login and change your password.</p>
                <br>
                <p>Smart Follow Up Team</p>
                "
            );

            return true;
        }

        // Reject Doctor Request
        public async Task<bool> RejectDoctorRequestAsync(long requestId, long adminId, string reason)
        {
            var request = await _context.DoctorRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Status == DoctorRequestStatus.Pending);

            if (request == null) return false;

            request.Status = DoctorRequestStatus.Rejected;
            request.RejectionReason = reason;
            request.ReviewedBy = adminId;

            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                request.Email,
                request.Name,
                "Smart Follow Up — Application Update",
                $@"
                <h2>Dear {request.Name},</h2>
                <p>We regret to inform you that your registration request has been rejected.</p>
                <p><strong>Reason:</strong> {reason}</p>
                <p>If you have any questions, please contact support.</p>
                <br>
                <p>Smart Follow Up Team</p>
                "
            );

            return true;
        }

        // Toggle Doctor Active Status
        public async Task<bool> ToggleDoctorStatusAsync(long doctorId)
        {
            var doctor = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == doctorId && u.Role == UserRole.Doctor);

            if (doctor == null) return false;

            doctor.IsActive = !doctor.IsActive;
            await _context.SaveChangesAsync();

            return true;
        }

        // Get Analytics
        public async Task<AnalyticsResponseDto> GetAnalyticsAsync()
        {
            return new AnalyticsResponseDto
            {
                TotalDoctors = await _context.Users.CountAsync(u => u.Role == UserRole.Doctor),
                TotalPatients = await _context.Users.CountAsync(u => u.Role == UserRole.Patient),
                TotalCases = await _context.Cases.CountAsync(),
                ActiveCases = await _context.Cases.CountAsync(c => c.Status == CaseStatus.Active),
                TotalReports = await _context.DailyReports.CountAsync(),
                CriticalAlerts = await _context.Alerts.CountAsync(a => a.AlertType == AlertType.Critical && a.Status == AlertStatus.Open),
                PendingDoctorRequests = await _context.DoctorRequests.CountAsync(r => r.Status == DoctorRequestStatus.Pending)
            };
        }
    }
}