using MediatR;
using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Application.Interfaces;

namespace SessionTrackerApi.Application.Features.Sessions.Commands;

public record SyncCalendarCommand(int UserId) : IRequest<int>;

public class SyncCalendarCommandHandler : IRequestHandler<SyncCalendarCommand, int>
{
    private readonly IAppDbContext _context;
    private readonly IGoogleCalendarService _calendarService;

    public SyncCalendarCommandHandler(IAppDbContext context, IGoogleCalendarService calendarService)
    {
        _context = context;
        _calendarService = calendarService;
    }

    public async Task<int> Handle(SyncCalendarCommand request, CancellationToken cancellationToken)
    {
        var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var fetchedSessions = await _calendarService.FetchNewSessionsAsync(startOfMonth, request.UserId);

        int processedCount = 0;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        var defaultRate = user?.DefaultHourlyRate ?? 140;

        // ── Upsert: add new sessions, update changed ones ──
        foreach (var fetchedSession in fetchedSessions)
        {
            var existingSession = await _context.Sessions
                .FirstOrDefaultAsync(s => s.GoogleEventId == fetchedSession.GoogleEventId && s.UserId == request.UserId, cancellationToken);

            if (existingSession == null)
            {
                fetchedSession.HourlyRate = defaultRate;
                fetchedSession.UserId = request.UserId;
                _context.Sessions.Add(fetchedSession);
                processedCount++;
            }
            else
            {
                existingSession.DurationInHours = fetchedSession.DurationInHours;
                existingSession.StartTime = fetchedSession.StartTime;
                existingSession.Title = fetchedSession.Title;
                processedCount++;
            }
        }

        // ── Delete sessions that were removed from Google Calendar ──
        // Only check sessions within the sync window (this month onwards)
        var fetchedEventIds = fetchedSessions
            .Select(s => s.GoogleEventId)
            .Where(id => id != null)
            .ToHashSet();

        var orphaned = await _context.Sessions
            .Where(s => s.UserId == request.UserId
                     && s.GoogleEventId != null
                     && s.StartTime >= startOfMonth
                     && !fetchedEventIds.Contains(s.GoogleEventId))
            .ToListAsync(cancellationToken);

        if (orphaned.Count > 0)
        {
            _context.Sessions.RemoveRange(orphaned);
            processedCount += orphaned.Count;
        }

        if (processedCount > 0)
            await _context.SaveChangesAsync(cancellationToken);

        return processedCount;
    }
}