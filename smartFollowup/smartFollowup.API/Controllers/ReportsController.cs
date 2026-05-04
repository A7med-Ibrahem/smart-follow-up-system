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
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportsController(ReportService reportService)
        {
            _reportService = reportService;
        }

        // POST api/reports
        [HttpPost]
        [Authorize(Roles = "patient")]
        public async Task<IActionResult> CreateReport(CreateReportRequestDto request)
        {
            var patientId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _reportService.CreateReportAsync(request, patientId);
            if (result == null)
                return BadRequest(new { message = "Case not found or not assigned to you" });

            return Ok(result);
        }

        // GET api/reports/case/{caseId}
        [HttpGet("case/{caseId}")]
        [Authorize(Roles = "doctor")]
        public async Task<IActionResult> GetCaseReports(long caseId)
        {
            var result = await _reportService.GetCaseReportsAsync(caseId);
            return Ok(result);
        }
    }
}