using MediatR;
using SessionTrackerApi.Application.Interfaces;

namespace SessionTrackerApi.Application.Features.Sessions.Commands;

public record UpdateSessionCommand(int Id, string Status, string CancelReason, decimal HourlyRate, double DurationInHours) : IRequest<bool>;

public class UpdateSessionCommandHandler : IRequestHandler<UpdateSessionCommand, bool>
{
    private readonly IAppDbContext _context;

    public UpdateSessionCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(UpdateSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions.FindAsync(new object[] { request.Id }, cancellationToken);
        if (session == null) return false;

        session.Status = request.Status;
        session.CancelReason = request.CancelReason;
        session.HourlyRate = request.HourlyRate;
        session.DurationInHours = request.DurationInHours;
        
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}