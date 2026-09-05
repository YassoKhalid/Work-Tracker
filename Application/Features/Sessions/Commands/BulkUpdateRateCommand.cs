using MediatR;
using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Application.Interfaces;

namespace SessionTrackerApi.Application.Features.Sessions.Commands;
public record BulkUpdateRateCommand(List<int> SessionIds, decimal Rate, int UserId) : IRequest;

public class BulkUpdateRateCommandHandler : IRequestHandler<BulkUpdateRateCommand>
{
    private readonly IAppDbContext _context;
    public BulkUpdateRateCommandHandler(IAppDbContext context) => _context = context;

    public async Task Handle(BulkUpdateRateCommand request, CancellationToken ct)
    {
        var sessions = await _context.Sessions
            .Where(s => request.SessionIds.Contains(s.Id) && s.UserId == request.UserId)
            .ToListAsync(ct);

        foreach (var session in sessions)
            session.HourlyRate = request.Rate;

        await _context.SaveChangesAsync(ct);
    }
}
