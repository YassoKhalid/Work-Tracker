using MediatR;
using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Application.Interfaces;
using SessionTrackerApi.Domain.Entities;

namespace SessionTrackerApi.Application.Features.Sessions.Queries;

public record GetSessionsQuery : IRequest<List<Session>>;

public class GetSessionsQueryHandler : IRequestHandler<GetSessionsQuery, List<Session>>
{
    private readonly IAppDbContext _context;
    
    public GetSessionsQueryHandler(IAppDbContext context) => _context = context;

    public async Task<List<Session>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Sessions.OrderBy(s => s.StartTime).ToListAsync(cancellationToken);
    }
}