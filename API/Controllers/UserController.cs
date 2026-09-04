using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SessionTrackerApi.Application.Features.Sessions.Commands;
using SessionTrackerApi.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SessionTrackerApi.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    public UserController(IMediator mediator, IAppDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue("UserId") ?? "0");

    [HttpGet("hourly-rate")]
    public async Task<IActionResult> GetDefaultHourlyRate()
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());
        return Ok(new { hourlyRate = user?.DefaultHourlyRate ?? 140 });
    }

    [HttpPut("hourly-rate")]
    public async Task<IActionResult> SetDefaultHourlyRate([FromBody] SetRateRequest request)
    {
        await _mediator.Send(new SetDefaultHourlyRateCommand(GetUserId(), request.HourlyRate));
        return NoContent();
    }
}

public record SetRateRequest(decimal HourlyRate);