using MediatR;
using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Application.Interfaces;

namespace SessionTrackerApi.Application.Features.Sessions.Commands;

public record PatchSessionCommand(
    int Id,
    int UserId,
    string? Status,
    string? CancelReason,
    decimal? HourlyRate,
    double? DurationInHours,
    string? Notes,
    string? PaidNote
) : IRequest<bool>;

public class PatchSessionCommandHandler : IRequestHandler<PatchSessionCommand, bool>
{
    private readonly IAppDbContext _context;
    public PatchSessionCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(PatchSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == request.UserId, cancellationToken);

        if (session == null) return false;

        if (request.Status          != null) session.Status          = request.Status;
        if (request.CancelReason    != null) session.CancelReason    = request.CancelReason;
        if (request.HourlyRate      != null) session.HourlyRate      = request.HourlyRate.Value;
        if (request.DurationInHours != null) session.DurationInHours = request.DurationInHours.Value;
        if (request.Notes           != null) session.Notes           = request.Notes;
        if (request.PaidNote        != null) session.PaidNote        = request.PaidNote;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}