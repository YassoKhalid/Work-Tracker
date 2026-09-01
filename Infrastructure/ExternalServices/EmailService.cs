using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SessionTrackerApi.Infrastructure.ExternalServices;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpServer  = _config["EmailSettings:Server"]         ?? "smtp.gmail.com";
        var port        = 465; // Force port 465 because Railway blocks port 587 (SMTP STARTTLS)
        var senderEmail = _config["EmailSettings:SenderEmail"]    ?? "";
        var password    = _config["EmailSettings:SenderPassword"] ?? "";

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(senderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpServer, port, SecureSocketOptions.SslOnConnect);
        await client.AuthenticateAsync(senderEmail, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}