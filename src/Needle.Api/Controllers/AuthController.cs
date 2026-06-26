using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Needle.Api.Contracts.Auth;

namespace Needle.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _configuration = configuration;
    }

    /// <summary>
    /// Issues a development JWT for local testing.
    /// </summary>
    [HttpPost("dev-token")]
    [ProducesResponseType(typeof(CreateDevTokenResponse), StatusCodes.Status200OK)]
    public IActionResult CreateDevToken(CreateDevTokenRequest request)
    {
        if (request.UserId == Guid.Empty)
        {
            return BadRequest(new { message = "UserId cannot be empty." });
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return BadRequest(new { message = "DisplayName cannot be empty." });
        }

        var issuer = _configuration["Jwt:Issuer"]
                     ?? throw new InvalidOperationException("JWT issuer was not configured.");

        var audience = _configuration["Jwt:Audience"]
                       ?? throw new InvalidOperationException("JWT audience was not configured.");

        var signingKey = _configuration["Jwt:SigningKey"]
                         ?? throw new InvalidOperationException("JWT signing key was not configured.");

        var expiresInMinutes = _configuration.GetValue<int>("Jwt:ExpiresInMinutes");

        if (expiresInMinutes <= 0)
        {
            throw new InvalidOperationException("JWT expiration must be greater than zero.");
        }

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.UserId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, request.UserId.ToString()),
            new Claim(ClaimTypes.Name, request.DisplayName.Trim())
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(signingKey));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return Ok(new CreateDevTokenResponse(
            accessToken,
            expiresAt));
    }
}