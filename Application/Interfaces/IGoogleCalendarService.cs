using SessionTrackerApi.Domain.Entities;

namespace SessionTrackerApi.Application.Interfaces;

public interface IGoogleCalendarService
{
    Task<List<Session>> FetchNewSessionsAsync(DateTime fromDate);
}