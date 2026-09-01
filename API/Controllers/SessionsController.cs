using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SessionTrackerApi.Application.Features.Sessions.Commands;
using SessionTrackerApi.Application.Features.Sessions.Queries;

namespace SessionTrackerApi.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class SessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SessionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue("UserId") ?? "0");

    [HttpPost("sync")]
    public async Task<IActionResult> SyncCalendar()
    {
        try
        {
            var addedCount = await _mediator.Send(new SyncCalendarCommand(GetUserId()));
            return Ok(new { message = $"Sync completed successfully. Added {addedCount} new sessions." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message, stack = ex.StackTrace });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetSessions()
    {
        var sessions = await _mediator.Send(new GetSessionsQuery(GetUserId()));
        return Ok(sessions);
    }

   [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateSessionRequest request)
    {
        var result = await _mediator.Send(new UpdateSessionCommand(id, request.Status, request.CancelReason, request.HourlyRate, request.DurationInHours, request.Notes, GetUserId()));
        
        if (!result) return NotFound(new { message = "Session not found" });
        return Ok(new { message = "Session updated successfully!" });
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSession(int id)
    {
        var result = await _mediator.Send(new DeleteSessionCommand(id, GetUserId()));
        
        if (!result) return NotFound(new { message = "Session not found" });
        return Ok(new { message = "Session deleted successfully!" });
    }

    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail(
        [FromServices] SessionTrackerApi.Infrastructure.ExternalServices.EmailService emailService)
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            await emailService.SendEmailAsync(
                userEmail!,
                "📋 Session Tracker — Email Test",
                "<h1 style='color:#6366f1;font-family:sans-serif'>It works! ✅</h1><p style='font-family:sans-serif'>Your email configuration is working correctly. You will receive daily digests at midnight.</p>");
            return Ok(new { message = $"Test email sent to {userEmail}!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
        }
    }
}

public record UpdateSessionRequest(string Status, string CancelReason, decimal HourlyRate, double DurationInHours, string? Notes);