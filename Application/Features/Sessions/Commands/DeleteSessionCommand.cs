using MediatR;
using SessionTrackerApi.Application.Interfaces;

namespace SessionTrackerApi.Application.Features.Sessions.Commands;

public record DeleteSessionCommand(int Id) : IRequest<bool>;

public class DeleteSessionCommandHandler : IRequestHandler<DeleteSessionCommand, bool>
{
    private readonly IAppDbContext _context;

    public DeleteSessionCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(DeleteSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions.FindAsync(new object[] { request.Id }, cancellationToken);
        if (session == null) return false;

        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}