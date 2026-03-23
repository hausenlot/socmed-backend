using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using socmed_backend.Data;
using socmed_backend.DTOs;
using socmed_backend.Models;

namespace socmed_backend.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IMultimediaService _multimediaService;

    public AuthService(AppDbContext context, IConfiguration configuration, IMultimediaService multimediaService)
    {
        _context = context;
        _configuration = configuration;
        _multimediaService = multimediaService;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        // Check if username is already taken (case-insensitive)
        var existingUser = await _context.Users
            .AnyAsync(u => u.Username.ToLower() == dto.Username.ToLower());

        if (existingUser) return null;

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = dto.Username.ToLower(),
            DisplayName = dto.DisplayName ?? dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            DisplayName = user.DisplayName,
            ProfileImageUrl = user.ProfileMediaId != null ? _multimediaService.GetPublicUrl(user.ProfileMediaId) : null
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == dto.Username.ToLower());

        // User not found or is a seeded test user with no password
        if (user == null || user.PasswordHash == null) return null;

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return null;

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            DisplayName = user.DisplayName,
            ProfileImageUrl = user.ProfileMediaId != null ? _multimediaService.GetPublicUrl(user.ProfileMediaId) : null
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
