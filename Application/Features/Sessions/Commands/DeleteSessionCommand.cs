using MediatR;
using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Application.Interfaces;

namespace SessionTrackerApi.Application.Features.Sessions.Commands;

public record DeleteSessionCommand(int Id, int UserId) : IRequest<bool>;

public class DeleteSessionCommandHandler : IRequestHandler<DeleteSessionCommand, bool>
{
    private readonly IAppDbContext _context;

    public DeleteSessionCommandHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(DeleteSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == request.UserId, cancellationToken);
        if (session == null) return false;

        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}