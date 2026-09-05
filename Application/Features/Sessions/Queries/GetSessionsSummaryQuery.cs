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
        var query = _context.Sessions.ApplyFilters(
            request.UserId, request.Search, request.Status, request.From, request.To);
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