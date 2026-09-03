using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SessionTrackerApi.Application.Interfaces;
using SessionTrackerApi.Infrastructure.ExternalServices;

namespace SessionTrackerApi.Application.BackgroundServices;

public class SessionReminderWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public SessionReminderWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;

            // Fire daily at 20:00 UTC = 11:00 PM Egypt time (EEST, UTC+3 in summer)
            if (now.Hour == 20 && now.Minute == 0)
            {
                await SendDailyReminderAsync();
                await Task.Delay(TimeSpan.FromSeconds(61), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task SendDailyReminderAsync()
    {
        try
        {
            using var scope  = _serviceProvider.CreateScope();
            var context      = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            var users = await context.Users.ToListAsync();

            foreach (var user in users)
            {
                if (string.IsNullOrEmpty(user.Email)) continue;

                // Get today's sessions for this user
                var today = DateTime.Now.Date;
                var todaySessions = await context.Sessions
                    .Where(s => s.UserId == user.Id && s.StartTime.Date == today)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();

                // Get today's pending sessions specifically
                var pendingSessions = todaySessions
                    .Where(s => s.Status == "Pending")
                    .ToList();

                // Only send if there are sessions today
                if (!todaySessions.Any()) continue;

                var subject = pendingSessions.Any()
                    ? $"⏰ {pendingSessions.Count} unsigned session(s) today — please review"
                    : $"✅ Session Digest — {today:MMMM dd, yyyy} (all signed)";

                var body = SessionDigestEmailBuilder.BuildReminder(today, todaySessions, pendingSessions);

                await emailService.SendEmailAsync(user.Email, subject, body);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionReminderWorker] Failed to send daily reminder: {ex.Message}");
        }
    }
}