using Microsoft.EntityFrameworkCore;
using SmartFollowUp.API.Data;
using SmartFollowUp.API.DTOs;
using SmartFollowUp.API.Models;

namespace SmartFollowUp.API.Services
{
    public class ReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        // Submit Daily Report
        public async Task<ReportResponseDto?> CreateReportAsync(CreateReportRequestDto request, long patientId)
        {
            // تأكد إن الـ Case موجودة وتابعة للمريض
            var existingCase = await _context.Cases
                .FirstOrDefaultAsync(c => c.Id == request.CaseId && c.PatientId == patientId);

            if (existingCase == null) return null;

            // عمل التقرير
            var report = new DailyReport
            {
                CaseId = request.CaseId,
                PatientId = patientId,
                Temperature = request.Temperature,
                PainLevel = request.PainLevel,
                Swelling = request.Swelling,
                Bleeding = request.Bleeding,
                Notes = request.Notes,
                SubmittedAt = DateTime.UtcNow
            };

            _context.DailyReports.Add(report);
            await _context.SaveChangesAsync();

            // AI Analysis
            var riskLevel = CalculateRiskLevel(request);
            var riskScore = CalculateRiskScore(request);

            var analysis = new AiAnalysis
            {
                ReportId = report.Id,
                RiskScore = riskScore,
                RiskLevel = riskLevel,
                AnalysisDetails = $"{{\"painLevel\":{request.PainLevel},\"temperature\":{request.Temperature},\"swelling\":{request.Swelling},\"bleeding\":{request.Bleeding}}}",
                AnalyzedAt = DateTime.UtcNow
            };

            _context.AiAnalyses.Add(analysis);

            // تحديث الـ Risk Level في الـ Case
            existingCase.CurrentRiskLevel = riskLevel;

            // لو Critical — عمل Alert
            if (riskLevel == "critical")
            {
                var alert = new Alert
                {
                    CaseId = request.CaseId,
                    ReportId = report.Id,
                    AlertType = "critical",
                    Priority = "high",
                    Status = "open",
                    TriggeredAt = DateTime.UtcNow
                };
                _context.Alerts.Add(alert);
            }

            await _context.SaveChangesAsync();

            return new ReportResponseDto
            {
                Id = report.Id,
                CaseId = report.CaseId,
                Temperature = report.Temperature ?? 0,
                PainLevel = report.PainLevel ?? 0,
                Swelling = report.Swelling,
                Bleeding = report.Bleeding,
                Notes = report.Notes,
                SubmittedAt = report.SubmittedAt,
                RiskLevel = riskLevel,
                RiskScore = riskScore
            };
        }

        // Get Reports for Case with Pagination
        public async Task<PaginatedResponseDto<ReportResponseDto>> GetCaseReportsAsync(long caseId, PaginationRequestDto pagination)
        {
            var query = _context.DailyReports
                .Where(r => r.CaseId == caseId)
                .Include(r => r.AiAnalysis);

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(r => r.SubmittedAt)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(r => new ReportResponseDto
                {
                    Id = r.Id,
                    CaseId = r.CaseId,
                    Temperature = r.Temperature ?? 0,
                    PainLevel = r.PainLevel ?? 0,
                    Swelling = r.Swelling,
                    Bleeding = r.Bleeding,
                    Notes = r.Notes,
                    SubmittedAt = r.SubmittedAt,
                    RiskLevel = r.AiAnalysis != null ? r.AiAnalysis.RiskLevel : "stable",
                    RiskScore = r.AiAnalysis != null ? r.AiAnalysis.RiskScore ?? 0 : 0
                })
                .ToListAsync();

            return new PaginatedResponseDto<ReportResponseDto>
            {
                Data = data,
                CurrentPage = pagination.Page,
                PageSize = pagination.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize),
                HasNextPage = pagination.Page * pagination.PageSize < totalCount,
                HasPreviousPage = pagination.Page > 1
            };
        }

        // AI Risk Calculation
        private string CalculateRiskLevel(CreateReportRequestDto request)
        {
            var score = CalculateRiskScore(request);
            if (score >= 70) return "critical";
            if (score >= 40) return "moderate";
            return "stable";
        }

        private decimal CalculateRiskScore(CreateReportRequestDto request)
        {
            decimal score = 0;

            // Temperature
            if (request.Temperature >= 39) score += 30;
            else if (request.Temperature >= 38) score += 15;

            // Pain Level
            if (request.PainLevel >= 8) score += 30;
            else if (request.PainLevel >= 5) score += 15;

            // Swelling
            if (request.Swelling) score += 20;

            // Bleeding
            if (request.Bleeding) score += 20;

            return Math.Min(score, 100);
        }
    }
}