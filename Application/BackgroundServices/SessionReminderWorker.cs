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

            // Fire daily at 19:00 UTC = 10:00 PM Egypt time (EEST, UTC+3 in summer)
            if (now.Hour == 19 && now.Minute == 0)
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

                // Get ALL pending sessions for this user
                var pendingSessions = await context.Sessions
                    .Where(s => s.UserId == user.Id && s.Status == "Pending")
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();

                // Also get yesterday's sessions for the digest
                var yesterday = DateTime.Now.AddDays(-1).Date;
                var yesterdaySessions = await context.Sessions
                    .Where(s => s.UserId == user.Id && s.StartTime.Date == yesterday)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();

                // Always send if there are pending sessions, or if there were sessions yesterday
                if (!pendingSessions.Any() && !yesterdaySessions.Any()) continue;

                var subject = pendingSessions.Any()
                    ? $"⏰ Reminder: You have {pendingSessions.Count} unsigned session(s)"
                    : $"📋 Session Digest — {yesterday:MMMM dd, yyyy}";

                var body = SessionDigestEmailBuilder.BuildReminder(yesterday, yesterdaySessions, pendingSessions);

                await emailService.SendEmailAsync(user.Email, subject, body);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionReminderWorker] Failed to send daily reminder: {ex.Message}");
        }
    }
}