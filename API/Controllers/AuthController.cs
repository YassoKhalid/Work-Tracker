using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SessionTrackerApi.Application.Interfaces;
using SessionTrackerApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SessionTrackerApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IAppDbContext _context;

    public AuthController(IConfiguration config, IAppDbContext context)
    {
        _config = config;
        _context = context;
    }

    private string GenerateJwtToken(string email, string name, int userId)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Name, name),
            new Claim("UserId", userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new Google.Apis.Auth.OAuth2.ClientSecrets
            {
                ClientId = _config["GoogleAuth:ClientId"],
                ClientSecret = _config["GoogleAuth:ClientSecret"]
            },
            Scopes = new[] { 
                "openid", 
                "profile", 
                "email", 
                "https://www.googleapis.com/auth/calendar.readonly" 
            }
        });

        var redirectUri = _config["GoogleAuth:RedirectUri"];
        var request = flow.CreateAuthorizationCodeRequest(redirectUri);
        var authorizationUrl = request.Build().ToString() + "&prompt=consent&access_type=offline";
        return Redirect(authorizationUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("No code provided by Google.");

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new Google.Apis.Auth.OAuth2.ClientSecrets
            {
                ClientId = _config["GoogleAuth:ClientId"],
                ClientSecret = _config["GoogleAuth:ClientSecret"]
            }
        });

        var redirectUri = _config["GoogleAuth:RedirectUri"];
        var tokenResponse = await flow.ExchangeCodeForTokenAsync(
            "user", 
            code, 
            redirectUri, 
            CancellationToken.None);

        var payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(tokenResponse.IdToken);
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
        if (user == null)
        {
            user = new User { Email = payload.Email, Name = payload.Name };
            _context.Users.Add(user);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        var userToken = await _context.UserGoogleTokens.FirstOrDefaultAsync(t => t.UserId == user.Id);
        if (userToken == null)
        {
            userToken = new UserGoogleToken { UserId = user.Id, CreatedAt = DateTime.UtcNow };
            _context.UserGoogleTokens.Add(userToken);
        }
        userToken.AccessToken = tokenResponse.AccessToken;
        userToken.RefreshToken = tokenResponse.RefreshToken ?? userToken.RefreshToken;
        userToken.AccessTokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3599);
        
        await _context.SaveChangesAsync(CancellationToken.None);

        var jwt = GenerateJwtToken(payload.Email, payload.Name, user.Id);
        
        // Redirect back to frontend with the token
        return Redirect($"/?token={jwt}");
    }
}