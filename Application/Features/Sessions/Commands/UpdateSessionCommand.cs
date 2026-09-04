using MediatR;
using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Application.Interfaces;

namespace SessionTrackerApi.Application.Features.Sessions.Commands;

public record UpdateSessionCommand(int Id, string Status, string CancelReason, decimal HourlyRate, double DurationInHours, string? Notes, string? PaidNote, int UserId) : IRequest<bool>;

public class UpdateSessionCommandHandler : IRequestHandler<UpdateSessionCommand, bool>
{
    private readonly IAppDbContext _context;

    public UpdateSessionCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(UpdateSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == request.UserId, cancellationToken);
        if (session == null) return false;

        session.Status = request.Status;
        session.CancelReason = request.CancelReason;
        session.HourlyRate = request.HourlyRate;
        session.DurationInHours = request.DurationInHours;
        session.Notes = request.Notes;
        session.PaidNote = request.PaidNote;
        
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}