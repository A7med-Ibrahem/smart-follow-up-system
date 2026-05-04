using Microsoft.EntityFrameworkCore;
using SmartFollowUp.API.Data;
using SmartFollowUp.API.Models;

namespace SmartFollowUp.API.Services
{
    public class EscalationService
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;

        public EscalationService(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // Check and Escalate Emergency Cases
        public async Task CheckAndEscalateAsync()
        {
            // جيب كل الـ Critical Cases اللي الـ Alert بتاعها لسه Open
            var criticalAlerts = await _context.Alerts
                .Include(a => a.Case)
                    .ThenInclude(c => c.Patient)
                .Include(a => a.Case)
                    .ThenInclude(c => c.Doctor)
                .Where(a => a.AlertType == "critical" &&
                            a.Status == "open" &&
                            a.TriggeredAt < DateTime.UtcNow.AddHours(-1))
                .ToListAsync();

            foreach (var alert in criticalAlerts)
            {
                // رفع الـ Priority لـ high
                alert.Priority = "high";

                // بعت notification للدكتور تاني
                await _notificationService.SendNotificationAsync(
                    alert.Case.DoctorId,
                    "alert",
                    "🚨 Emergency Escalation",
                    $"Patient {alert.Case.Patient.Name} has a critical condition that requires immediate attention!",
                    alert.Id
                );
            }

            await _context.SaveChangesAsync();
        }
    }
}