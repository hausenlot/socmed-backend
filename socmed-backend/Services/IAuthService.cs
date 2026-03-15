using socmed_backend.DTOs;

namespace socmed_backend.Services;

public interface IAuthService
{
    /// <summary>Creates a new user account. Returns null if username is taken.</summary>
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);

    /// <summary>Validates credentials and returns a JWT. Returns null if invalid.</summary>
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
}
