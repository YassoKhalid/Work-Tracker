using MediatR;
using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Application.Interfaces;

namespace SessionTrackerApi.Application.Features.Sessions.Commands;

public record SetDefaultHourlyRateCommand(int UserId, decimal Rate) : IRequest;

public class SetDefaultHourlyRateCommandHandler : IRequestHandler<SetDefaultHourlyRateCommand>
{
    private readonly IAppDbContext _context;

    public SetDefaultHourlyRateCommandHandler(IAppDbContext context) => _context = context;

    public async Task Handle(SetDefaultHourlyRateCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) return;

        user.DefaultHourlyRate = request.Rate;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
