using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using SessionTrackerApi.Application.Interfaces;
using SessionTrackerApi.Domain.Entities;

namespace SessionTrackerApi.Infrastructure.ExternalServices;

public class GoogleCalendarService : IGoogleCalendarService
{
    public async Task<List<Session>> FetchNewSessionsAsync(DateTime fromDate)
    {
        UserCredential credential;

        var credentialsEnv = Environment.GetEnvironmentVariable("GOOGLE_CREDENTIALS_JSON");
        var tokenEnv       = Environment.GetEnvironmentVariable("GOOGLE_TOKEN_JSON");

        if (!string.IsNullOrEmpty(credentialsEnv) && !string.IsNullOrEmpty(tokenEnv))
        {
            // Production (Railway): build credential directly from env vars — no browser/file needed
            using var credStream  = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(credentialsEnv));
            var secrets           = GoogleClientSecrets.FromStream(credStream).Secrets;
            var tokenResponse     = Google.Apis.Json.NewtonsoftJsonSerializer.Instance
                                        .Deserialize<TokenResponse>(tokenEnv);

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = secrets,
                Scopes = new[] { CalendarService.Scope.CalendarReadonly }
            });

            credential = new UserCredential(flow, "user", tokenResponse);
        }
        else
        {
            // Local development: use credentials.json + file-based token cache
            using var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { CalendarService.Scope.CalendarReadonly },
                "user", CancellationToken.None, new FileDataStore("token.json", true));
        }

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