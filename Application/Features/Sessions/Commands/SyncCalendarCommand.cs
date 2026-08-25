using MediatR;
using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Application.Interfaces;

namespace SessionTrackerApi.Application.Features.Sessions.Commands;

public record SyncCalendarCommand : IRequest<int>;

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
        var fetchedSessions = await _calendarService.FetchNewSessionsAsync(startOfMonth);
        
        int processedCount = 0;

        foreach (var fetchedSession in fetchedSessions)
        {
            var existingSession = await _context.Sessions
                .FirstOrDefaultAsync(s => s.GoogleEventId == fetchedSession.GoogleEventId, cancellationToken);
            
            if (existingSession == null)
            {
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

        if (processedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return processedCount;
    }
}