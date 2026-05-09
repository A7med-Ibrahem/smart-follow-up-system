using Microsoft.EntityFrameworkCore;
using SmartFollowUp.API.Data;
using SmartFollowUp.API.Enums;

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

        public async Task CheckAndEscalateAsync()
        {
            var criticalAlerts = await _context.Alerts
                .Include(a => a.Case)
                    .ThenInclude(c => c.Patient)
                .Include(a => a.Case)
                    .ThenInclude(c => c.Doctor)
                .Where(a => a.AlertType == AlertType.Critical &&
                            a.Status == AlertStatus.Open &&
                            a.TriggeredAt < DateTime.UtcNow.AddHours(-1))
                .ToListAsync();

            foreach (var alert in criticalAlerts)
            {
                alert.Priority = AlertPriority.High;

                await _notificationService.SendNotificationAsync(
                    alert.Case.DoctorId,
                    NotificationType.Alert.ToString(),
                    "🚨 Emergency Escalation",
                    $"Patient {alert.Case.Patient.Name} has a critical condition that requires immediate attention!",
                    alert.Id
                );
            }

            await _context.SaveChangesAsync();
        }
    }
}