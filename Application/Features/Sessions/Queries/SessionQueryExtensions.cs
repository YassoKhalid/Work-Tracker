using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Domain.Entities;

namespace SessionTrackerApi.Application.Features.Sessions.Queries;

public static class SessionQueryExtensions
{
    public static IQueryable<Session> ApplyFilters(
        this IQueryable<Session> query, 
        int userId, 
        string? search, 
        string? status, 
        DateTime? from, 
        DateTime? to)
    {
        query = query.Where(s => s.UserId == userId);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(s => s.Title != null && EF.Functions.ILike(s.Title, $"%{search}%"));

        if (!string.IsNullOrEmpty(status) && status != "All")
            query = query.Where(s => s.Status == status);  

        if (from.HasValue)
            query = query.Where(s => s.StartTime.Date >= from.Value.Date);

        if (to.HasValue)
            query = query.Where(s => s.StartTime.Date <= to.Value.Date);  

        return query;
    }
}