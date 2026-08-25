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
            var now = DateTime.Now;

            if (now.Hour == 0 && now.Minute == 0)
            {
                await SendNightlyDigestAsync();
                await Task.Delay(TimeSpan.FromSeconds(61), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task SendNightlyDigestAsync()
    {
        try
        {
            using var scope  = _serviceProvider.CreateScope();
            var context      = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
            var config       = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var yesterday = DateTime.Now.AddDays(-1).Date;
            var users = await context.Users.ToListAsync();

            foreach (var user in users)
            {
                if (string.IsNullOrEmpty(user.Email)) continue;

                var sessions = await context.Sessions
                    .Where(s => s.UserId == user.Id && s.StartTime.Date == yesterday)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();

                if (!sessions.Any()) continue;

                var subject = $"📋 Session Digest — {yesterday:MMMM dd, yyyy}";
                var body = SessionDigestEmailBuilder.Build(yesterday, sessions);

                await emailService.SendEmailAsync(user.Email, subject, body);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionReminderWorker] Failed to send nightly digest: {ex.Message}");
        }
    }
}