using MediatR;
using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Application.Interfaces;

namespace SessionTrackerApi.Application.Features.Sessions.Queries;

public record GetSessionsSummaryQuery(
    int UserId,
    string? Search,
    string? Status,
    DateTime? From,
    DateTime? To
) : IRequest<SessionSummary>;  

public record SessionSummary(
    int TotalSessions,
    double TotalHours,
    double CompletedHours,
    decimal CompletedEarnings,
    decimal PaidEarnings
);

public class GetSessionsSummaryQueryHandler : IRequestHandler<GetSessionsSummaryQuery, SessionSummary>
{
    private readonly IAppDbContext _context;
    public GetSessionsSummaryQueryHandler(IAppDbContext context) => _context = context;

    public async Task<SessionSummary> Handle(GetSessionsSummaryQuery request, CancellationToken cancellationToken)
    {
        // Apply the same filters as GetSessionsQuery
        var query = _context.Sessions.Where(s => s.UserId == request.UserId);

        if (!string.IsNullOrEmpty(request.Search))
            query = query.Where(s => s.Title != null && s.Title.ToLower().Contains(request.Search.ToLower()));

        if (!string.IsNullOrEmpty(request.Status) && request.Status != "All")
            query = query.Where(s => s.Status == request.Status);

        if (request.From.HasValue)
            query = query.Where(s => s.StartTime.Date >= request.From.Value.Date);

        if (request.To.HasValue)
            query = query.Where(s => s.StartTime.Date <= request.To.Value.Date);

        // Pull only the 3 fields needed for aggregation — no heavy payload
        var sessions = await query.Select(s => new {
            s.Status,
            s.DurationInHours,
            s.HourlyRate
        }).ToListAsync(cancellationToken);

        return new SessionSummary(
            TotalSessions:     sessions.Count,
            TotalHours:        sessions.Sum(s => s.DurationInHours),
            CompletedHours:    sessions.Where(s => s.Status == "Completed").Sum(s => s.DurationInHours),
            CompletedEarnings: sessions.Where(s => s.Status == "Completed").Sum(s => (decimal)s.DurationInHours * s.HourlyRate),
            PaidEarnings:      sessions.Where(s => s.Status == "Paid").Sum(s => (decimal)s.DurationInHours * s.HourlyRate)
        );
    }
}