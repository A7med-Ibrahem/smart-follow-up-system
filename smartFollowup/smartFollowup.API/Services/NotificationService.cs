using Microsoft.EntityFrameworkCore;
using SmartFollowUp.API.Data;
using SmartFollowUp.API.DTOs;
using SmartFollowUp.API.Models;

namespace SmartFollowUp.API.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        // Get Notifications for User
        public async Task<List<NotificationResponseDto>> GetUserNotificationsAsync(long userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .Select(n => new NotificationResponseDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Title = n.Title,
                    Body = n.Body,
                    IsRead = n.IsRead,
                    SentAt = n.SentAt
                })
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();
        }

        // Mark as Read
        public async Task<bool> MarkAsReadAsync(long notificationId, long userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return true;
        }

        // Mark All as Read
        public async Task MarkAllAsReadAsync(long userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
                n.IsRead = true;

            await _context.SaveChangesAsync();
        }

        // Send Notification
        public async Task SendNotificationAsync(long userId, string type, string title, string body, long? alertId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                AlertId = alertId,
                Type = type,
                Title = title,
                Body = body,
                IsRead = false,
                SentAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        // Get Unread Count
        public async Task<int> GetUnreadCountAsync(long userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }
    }
}