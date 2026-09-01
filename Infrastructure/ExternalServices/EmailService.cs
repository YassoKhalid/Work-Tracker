using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SessionTrackerApi.Infrastructure.ExternalServices;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public EmailService(IConfiguration config)
    {
        _config = config;
        _httpClient = new HttpClient();
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var apiKey = _config["EmailSettings:ResendApiKey"] ?? "";
        
        // Note: For unverified domains, Resend requires sending FROM onboarding@resend.dev
        // and it will only successfully deliver TO the email address you registered Resend with.
        var requestBody = new
        {
            from = "onboarding@resend.dev",
            to = new[] { toEmail },
            subject = subject,
            html = body
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        
        var response = await _httpClient.PostAsync("https://api.resend.com/emails", content);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to send email via Resend API: {response.StatusCode} - {error}");
        }
    }
}