using Microsoft.EntityFrameworkCore;
using SmartFollowUp.API.Data;
using SmartFollowUp.API.DTOs;
using SmartFollowUp.API.Models;

namespace SmartFollowUp.API.Services
{
    public class CaseService
    {
        private readonly AppDbContext _context;

        private readonly EmailService _emailService;

        public CaseService(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Create Case
        public async Task<CaseResponseDto?> CreateCaseAsync(CreateCaseRequestDto request, long doctorId)
        {
            var patient = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.PatientEmail);

            if (patient == null)
            {
                patient = new User
                {
                    Name = request.PatientName,
                    Email = request.PatientEmail,
                    Phone = request.PatientPhone,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Smart@123"),
                    Role = "patient",
                    IsActive = true
                };
                _context.Users.Add(patient);
                await _context.SaveChangesAsync();
            }

            var newCase = new Case
            {
                DoctorId = doctorId,
                PatientId = patient.Id,
                OperationType = request.OperationType,
                OperationDate = request.OperationDate,
                InitialTreatment = request.InitialTreatment,
                Status = "active",
                CurrentRiskLevel = "stable"
            };

            _context.Cases.Add(newCase);
            await _context.SaveChangesAsync();

            await _emailService.SendPatientActivationEmailAsync(
            patient.Email,
            patient.Name,
            "Smart@123"
                       );

            return new CaseResponseDto
            {
                Id = newCase.Id,
                PatientName = patient.Name,
                PatientEmail = patient.Email,
                OperationType = newCase.OperationType ?? "",
                OperationDate = newCase.OperationDate,
                Status = newCase.Status,
                CurrentRiskLevel = newCase.CurrentRiskLevel,
                CreatedAt = newCase.CreatedAt
            };
        }

        // Get All Cases for Doctor
        public async Task<List<CaseResponseDto>> GetDoctorCasesAsync(long doctorId)
        {
            return await _context.Cases
                .Where(c => c.DoctorId == doctorId)
                .Include(c => c.Patient)
                .Select(c => new CaseResponseDto
                {
                    Id = c.Id,
                    PatientName = c.Patient.Name,
                    PatientEmail = c.Patient.Email,
                    OperationType = c.OperationType ?? "",
                    OperationDate = c.OperationDate,
                    Status = c.Status,
                    CurrentRiskLevel = c.CurrentRiskLevel,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        // Get Case by ID
        public async Task<CaseResponseDto?> GetCaseByIdAsync(long caseId, long doctorId)
        {
            var c = await _context.Cases
                .Include(c => c.Patient)
                .FirstOrDefaultAsync(c => c.Id == caseId && c.DoctorId == doctorId);

            if (c == null) return null;

            return new CaseResponseDto
            {
                Id = c.Id,
                PatientName = c.Patient.Name,
                PatientEmail = c.Patient.Email,
                OperationType = c.OperationType ?? "",
                OperationDate = c.OperationDate,
                Status = c.Status,
                CurrentRiskLevel = c.CurrentRiskLevel,
                CreatedAt = c.CreatedAt
            };
        }

        // Close Case
        public async Task<bool> CloseCaseAsync(long caseId, long doctorId)
        {
            var c = await _context.Cases
                .FirstOrDefaultAsync(c => c.Id == caseId && c.DoctorId == doctorId);

            if (c == null) return false;

            c.Status = "closed";
            c.ClosedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        // Search Patients
        public async Task<List<CaseResponseDto>> SearchPatientsAsync(string keyword, long doctorId)
        {
            return await _context.Cases
                .Where(c => c.DoctorId == doctorId &&
                    (c.Patient.Name.Contains(keyword) ||
                     c.Patient.Phone!.Contains(keyword)))
                .Include(c => c.Patient)
                .Select(c => new CaseResponseDto
                {
                    Id = c.Id,
                    PatientName = c.Patient.Name,
                    PatientEmail = c.Patient.Email,
                    OperationType = c.OperationType ?? "",
                    OperationDate = c.OperationDate,
                    Status = c.Status,
                    CurrentRiskLevel = c.CurrentRiskLevel,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }
    }
}