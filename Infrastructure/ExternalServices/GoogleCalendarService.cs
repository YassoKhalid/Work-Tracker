using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.EntityFrameworkCore;
using SessionTrackerApi.Application.Interfaces;
using SessionTrackerApi.Domain.Entities;

namespace SessionTrackerApi.Infrastructure.ExternalServices;

public class GoogleCalendarService : IGoogleCalendarService
{
    private readonly IAppDbContext _context;
    private readonly IConfiguration _config;

    public GoogleCalendarService(IAppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<List<Session>> FetchNewSessionsAsync(DateTime fromDate, int userId)
    {
        var token = await _context.UserGoogleTokens.FirstOrDefaultAsync(t => t.UserId == userId);
        if (token == null || string.IsNullOrEmpty(token.AccessToken))
            return new List<Session>();

        var tokenResponse = new TokenResponse
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken
        };

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new Google.Apis.Auth.OAuth2.ClientSecrets
            {
                ClientId = _config["GoogleAuth:ClientId"],
                ClientSecret = _config["GoogleAuth:ClientSecret"]
            },
            Scopes = new[] { CalendarService.Scope.CalendarReadonly }
        });

        var credential = new UserCredential(flow, userId.ToString(), tokenResponse);

        var service = new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "SessionTracker",
        });

        var request = service.Events.List("primary");
        request.TimeMinDateTimeOffset = fromDate;
        request.ShowDeleted  = false;
        request.SingleEvents = true;
        request.OrderBy      = EventsResource.ListRequest.OrderByEnum.StartTime;

        var events   = await request.ExecuteAsync();
        var sessions = new List<Session>();

        if (events.Items != null)
        {
            foreach (var item in events.Items)
            {
                double duration  = 1.0;
                DateTime startTime = DateTime.Now;

                if (item.Start.DateTimeDateTimeOffset.HasValue && item.End.DateTimeDateTimeOffset.HasValue)
                {
                    var exactStart = item.Start.DateTimeDateTimeOffset.Value;
                    var exactEnd   = item.End.DateTimeDateTimeOffset.Value;
                    duration  = (exactEnd - exactStart).TotalHours;
                    startTime = exactStart.DateTime;
                }
                else if (!string.IsNullOrEmpty(item.Start.Date) && !string.IsNullOrEmpty(item.End.Date))
                {
                    DateTime.TryParse(item.Start.Date, out startTime);
                    DateTime.TryParse(item.End.Date, out var endDate);
                    duration = (endDate - startTime).TotalHours;
                }

                if (duration <= 0) duration = 1.0;

                sessions.Add(new Session
                {
                    GoogleEventId   = item.Id,
                    Title           = item.Summary ?? "No Title",
                    StartTime       = startTime,
                    DurationInHours = Math.Round(duration, 2)
                });
            }
        }

        return sessions;
    }
}