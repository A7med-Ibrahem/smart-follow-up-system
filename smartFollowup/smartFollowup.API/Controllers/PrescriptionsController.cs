using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFollowUp.API.DTOs;
using SmartFollowUp.API.Services;
using System.Security.Claims;

namespace SmartFollowUp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrescriptionsController : ControllerBase
    {
        private readonly PrescriptionService _prescriptionService;
        private readonly AuditService _auditService;

        public PrescriptionsController(PrescriptionService prescriptionService, AuditService auditService)
        {
            _prescriptionService = prescriptionService;
            _auditService = auditService;
        }

        // POST api/prescriptions
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CreatePrescription(CreatePrescriptionRequestDto request)
        {
            var doctorId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _prescriptionService.CreatePrescriptionAsync(request, doctorId);

            if (result == null)
                return BadRequest(new { message = "Case not found or not assigned to you" });

            await _auditService.LogAsync(
                action: "CREATE",
                entityName: "Prescription",
                entityId: result.Id.ToString(),
                newValues: $"CaseId: {request.CaseId}, Medications: {request.Medications.Count}",
                userId: doctorId,
                userName: User.FindFirst(ClaimTypes.Name)?.Value ?? "Doctor",
                userRole: "Doctor",
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(result);
        }

        // GET api/prescriptions/case/{caseId}
        [HttpGet("case/{caseId}")]
        [Authorize(Roles = "Doctor,Patient")]
        public async Task<IActionResult> GetCasePrescriptions(long caseId)
        {
            var result = await _prescriptionService.GetCasePrescriptionsAsync(caseId);
            return Ok(result);
        }

        // POST api/prescriptions/medications/{medicationId}/confirm
        [HttpPost("medications/{medicationId}/confirm")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> ConfirmMedicationTaken(long medicationId)
        {
            var patientId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var success = await _prescriptionService.ConfirmMedicationTakenAsync(medicationId, patientId);
            if (!success)
                return NotFound(new { message = "Medication not found" });

            return Ok(new { message = "Medication confirmed as taken" });
        }

        // PUT api/prescriptions/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> UpdatePrescription(long id, CreatePrescriptionRequestDto request)
        {
            var doctorId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _prescriptionService.UpdatePrescriptionAsync(id, request, doctorId);

            if (result == null)
                return NotFound(new { message = "Prescription not found" });

            await _auditService.LogAsync(
                action: "UPDATE",
                entityName: "Prescription",
                entityId: id.ToString(),
                newValues: $"Updated medications: {request.Medications.Count}",
                userId: doctorId,
                userName: User.FindFirst(ClaimTypes.Name)?.Value ?? "Doctor",
                userRole: "Doctor",
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(result);
        }
    }
}