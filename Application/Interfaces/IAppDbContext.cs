using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Domain.Entities;

namespace SessionTrackerApi.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Session> Sessions { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}