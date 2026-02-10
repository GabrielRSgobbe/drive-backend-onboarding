using Drive.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Drive.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _cfg;

    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration cfg)
    {
        _userManager = userManager;
        _cfg = cfg;
    }

    public record RegisterRequest(string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record AuthResponse(string AccessToken);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        var user = new ApplicationUser { UserName = req.Email, Email = req.Email};
        var result = await _userManager.CreateAsync(user, req.Password);

        if(!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));


        return Ok();
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await _userManager.FindByEmailAsync(req.Email);
        if(user is null) return Unauthorized();

        var ok = await _userManager.CheckPasswordAsync(user, req.Password);
        if(!ok) return Unauthorized();

        var token = GenerateJwt(user);
        return Ok(new AuthResponse(token));
    }

    private string GenerateJwt(ApplicationUser user)
    {
        var key = _cfg["Jwt:Key"]!;
        var issuer = _cfg["Jwt:Issuer"]!;
        var audience = _cfg ["Jwt:Audience"]!;
        var expMin = int.Parse(_cfg["Jwt:ExpiresMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new Claim (ClaimTypes.NameIdentifier, user.Id),
            new Claim (ClaimTypes.Name, user.Email ?? user.UserName ?? "")
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expMin),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    
        
    
    }
}