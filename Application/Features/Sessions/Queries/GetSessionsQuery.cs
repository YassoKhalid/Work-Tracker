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
        var query = _context.Sessions.Where(s => s.UserId == request.UserId);

        if (!string.IsNullOrEmpty(request.Search))
            query = query.Where(s => s.Title != null && s.Title.ToLower().Contains(request.Search.ToLower()));

        if (!string.IsNullOrEmpty(request.Status) && request.Status != "All")
            query = query.Where(s => s.Status == request.Status);  

        if (request.From.HasValue)
            query = query.Where(s => s.StartTime.Date >= request.From.Value.Date);

        if (request.To.HasValue)
            query = query.Where(s => s.StartTime.Date <= request.To.Value.Date);  

        return await query.OrderByDescending(s => s.StartTime).ToListAsync(cancellationToken);
    }
}