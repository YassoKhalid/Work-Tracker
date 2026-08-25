using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Application.Interfaces;
using SessionTrackerApi.Domain.Entities;

namespace SessionTrackerApi.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User>Users{set;get;}
    public DbSet<UserGoogleToken>UserGoogleTokens{set;get;}
    public DbSet<Session> Sessions { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}