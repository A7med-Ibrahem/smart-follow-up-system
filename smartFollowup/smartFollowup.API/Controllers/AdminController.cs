using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFollowUp.API.DTOs;
using SmartFollowUp.API.Services;
using System.Security.Claims;

namespace SmartFollowUp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;
        private readonly AuditService _auditService;

        public AdminController(AdminService adminService, AuditService auditService)
        {
            _adminService = adminService;
            _auditService = auditService;
        }

        // GET api/admin/doctor-requests
        [HttpGet("doctor-requests")]
        public async Task<IActionResult> GetDoctorRequests([FromQuery] string? status = null)
        {
            var result = await _adminService.GetDoctorRequestsAsync(status);
            return Ok(result);
        }

        // PUT api/admin/doctor-requests/{id}/approve
        [HttpPut("doctor-requests/{id}/approve")]
        public async Task<IActionResult> ApproveDoctorRequest(long id)
        {
            var adminId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var success = await _adminService.ApproveDoctorRequestAsync(id, adminId);
            if (!success)
                return NotFound(new { message = "Request not found or already processed" });

            await _auditService.LogAsync(
                action: "APPROVE",
                entityName: "DoctorRequest",
                entityId: id.ToString(),
                userId: adminId,
                userName: "Admin",
                userRole: "Admin",
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new { message = "Doctor approved successfully" });
        }

        // PUT api/admin/doctor-requests/{id}/reject
        [HttpPut("doctor-requests/{id}/reject")]
        public async Task<IActionResult> RejectDoctorRequest(long id, RejectDoctorRequestDto request)
        {
            var adminId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var success = await _adminService.RejectDoctorRequestAsync(id, adminId, request.RejectionReason);
            if (!success)
                return NotFound(new { message = "Request not found or already processed" });

            await _auditService.LogAsync(
                action: "REJECT",
                entityName: "DoctorRequest",
                entityId: id.ToString(),
                newValues: $"Reason: {request.RejectionReason}",
                userId: adminId,
                userName: "Admin",
                userRole: "Admin",
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new { message = "Doctor request rejected" });
        }

        // PUT api/admin/doctors/{id}/toggle-status
        [HttpPut("doctors/{id}/toggle-status")]
        public async Task<IActionResult> ToggleDoctorStatus(long id)
        {
            var success = await _adminService.ToggleDoctorStatusAsync(id);
            if (!success)
                return NotFound(new { message = "Doctor not found" });

            return Ok(new { message = "Doctor status updated" });
        }

        // GET api/admin/analytics
        [HttpGet("Analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            var result = await _adminService.GetAnalyticsAsync();
            return Ok(result);
        }
    }
}