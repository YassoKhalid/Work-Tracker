using MediatR;
using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Application.Interfaces;
using SessionTrackerApi.Domain.Entities;

namespace SessionTrackerApi.Application.Features.Sessions.Queries;

public record GetSessionsQuery(int UserId, string? Search, 
string? Status, DateTime? From, DateTime? To, int Page = 1, int PageSize = 50) : IRequest<PageResult<Session>>;

public record PageResult<T>(List<T> Items, int Page, int PageSize, int TotalCount);



public class GetSessionsQueryHandler : IRequestHandler<GetSessionsQuery, PageResult<Session>>
{
    private readonly IAppDbContext _context;
    
    public GetSessionsQueryHandler(IAppDbContext context) => _context = context;

    public async Task<PageResult<Session>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Sessions.ApplyFilters(
            request.UserId, request.Search, request.Status, request.From, request.To);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
        .OrderBy(s => s.StartTime)
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToListAsync(cancellationToken);

        return new PageResult<Session>(items, request.Page, request.PageSize, totalCount);

    }
}