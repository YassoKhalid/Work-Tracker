using MediatR;
using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Application.Interfaces;
using SessionTrackerApi.Domain.Entities;

namespace SessionTrackerApi.Application.Features.Sessions.Queries;

public record GetSessionsQuery(int UserId, string? Search, 
string? Status, DateTime? From, DateTime? To) : IRequest<List<Session>>;
public class GetSessionsQueryHandler : IRequestHandler<GetSessionsQuery, List<Session>>
{
    private readonly IAppDbContext _context;
    
    public GetSessionsQueryHandler(IAppDbContext context) => _context = context;

    public async Task<List<Session>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Sessions.ApplyFilters(
            request.UserId, request.Search, request.Status, request.From, request.To);

        return await query
            .OrderByDescending(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }
}