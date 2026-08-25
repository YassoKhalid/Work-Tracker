using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Domain.Entities;

namespace SessionTrackerApi.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; set; }
    DbSet<UserGoogleToken> UserGoogleTokens { get; set; }
    DbSet<Session> Sessions { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}