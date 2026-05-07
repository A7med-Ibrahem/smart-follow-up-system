using Microsoft.EntityFrameworkCore;
using SmartFollowUp.API.Data;
using SmartFollowUp.API.DTOs;
using SmartFollowUp.API.Models;

namespace SmartFollowUp.API.Services
{
    public class PrescriptionService
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;

        public PrescriptionService(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // Create Prescription
        public async Task<PrescriptionResponseDto?> CreatePrescriptionAsync(CreatePrescriptionRequestDto request, long doctorId)
        {
            var existingCase = await _context.Cases
                .FirstOrDefaultAsync(c => c.Id == request.CaseId && c.DoctorId == doctorId);

            if (existingCase == null) return null;

            var prescription = new Prescription
            {
                CaseId = request.CaseId,
                DoctorId = doctorId,
                Instructions = request.Instructions,
                CreatedAt = DateTime.UtcNow
            };

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            foreach (var med in request.Medications)
            {
                _context.PrescriptionMedications.Add(new PrescriptionMedication
                {
                    PrescriptionId = prescription.Id,
                    MedicationName = med.MedicationName,
                    Dosage = med.Dosage,
                    Frequency = med.Frequency,
                    DurationDays = med.DurationDays
                });
            }

            await _context.SaveChangesAsync();
            return await GetPrescriptionByIdAsync(prescription.Id);
        }

        // Get Prescriptions for Case
        public async Task<List<PrescriptionResponseDto>> GetCasePrescriptionsAsync(long caseId)
        {
            return await _context.Prescriptions
                .Where(p => p.CaseId == caseId)
                .Include(p => p.Medications)
                .Select(p => new PrescriptionResponseDto
                {
                    Id = p.Id,
                    CaseId = p.CaseId,
                    Instructions = p.Instructions,
                    CreatedAt = p.CreatedAt,
                    Medications = p.Medications.Select(m => new MedicationResponseDto
                    {
                        Id = m.Id,
                        MedicationName = m.MedicationName,
                        Dosage = m.Dosage,
                        Frequency = m.Frequency,
                        DurationDays = m.DurationDays
                    }).ToList()
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // Get Prescription by ID
        public async Task<PrescriptionResponseDto?> GetPrescriptionByIdAsync(long prescriptionId)
        {
            var p = await _context.Prescriptions
                .Include(p => p.Medications)
                .FirstOrDefaultAsync(p => p.Id == prescriptionId);

            if (p == null) return null;

            return new PrescriptionResponseDto
            {
                Id = p.Id,
                CaseId = p.CaseId,
                Instructions = p.Instructions,
                CreatedAt = p.CreatedAt,
                Medications = p.Medications.Select(m => new MedicationResponseDto
                {
                    Id = m.Id,
                    MedicationName = m.MedicationName,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    DurationDays = m.DurationDays
                }).ToList()
            };
        }

        // Update Prescription
        public async Task<PrescriptionResponseDto?> UpdatePrescriptionAsync(long prescriptionId, CreatePrescriptionRequestDto request, long doctorId)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Medications)
                .Include(p => p.Case)
                .FirstOrDefaultAsync(p => p.Id == prescriptionId && p.DoctorId == doctorId);

            if (prescription == null) return null;

            prescription.Instructions = request.Instructions;
            prescription.UpdatedAt = DateTime.UtcNow;

            foreach (var med in prescription.Medications)
            {
                var adherences = _context.MedicationAdherences
                    .Where(a => a.MedicationId == med.Id);
                _context.MedicationAdherences.RemoveRange(adherences);
            }
            _context.PrescriptionMedications.RemoveRange(prescription.Medications);

            foreach (var med in request.Medications)
            {
                _context.PrescriptionMedications.Add(new PrescriptionMedication
                {
                    PrescriptionId = prescription.Id,
                    MedicationName = med.MedicationName,
                    Dosage = med.Dosage,
                    Frequency = med.Frequency,
                    DurationDays = med.DurationDays
                });
            }

            await _context.SaveChangesAsync();

            // بعت Notification للمريض
            await _notificationService.SendNotificationAsync(
                prescription.Case.PatientId,
                "prescription",
                "Prescription Updated 💊",
                "Your doctor has updated your prescription. Please check your medications."
            );

            return await GetPrescriptionByIdAsync(prescriptionId);
        }

        // Confirm Medication Taken
        public async Task<bool> ConfirmMedicationTakenAsync(long medicationId, long patientId)
        {
            var medication = await _context.PrescriptionMedications
                .Include(m => m.Prescription)
                    .ThenInclude(p => p.Case)
                .FirstOrDefaultAsync(m => m.Id == medicationId && m.Prescription.Case.PatientId == patientId);

            if (medication == null) return false;

            var adherence = new MedicationAdherence
            {
                MedicationId = medicationId,
                PatientId = patientId,
                ScheduledAt = DateTime.UtcNow,
                ConfirmedAt = DateTime.UtcNow,
                Status = "taken"
            };

            _context.MedicationAdherences.Add(adherence);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}