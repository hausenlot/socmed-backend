using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using socmed_backend.DTOs;
using socmed_backend.Services;

namespace socmed_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IFollowService _followService;

    public UsersController(IUserService userService, IFollowService followService)
    {
        _userService = userService;
        _followService = followService;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet("{username}")]
    public async Task<ActionResult<UserProfileDto>> GetProfile(string username)
    {
        var profile = await _userService.GetUserByUsernameAsync(username, CurrentUserId);
        if (profile == null) return NotFound(new { message = "User not found." });
        return Ok(profile);
    }

    [HttpGet("{username}/rants")]
    public async Task<ActionResult<IEnumerable<RantResponseDto>>> GetUserRants(
        string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (pageSize > 50) pageSize = 50;
        if (page < 1) page = 1;
        var rants = await _userService.GetUserRantsAsync(username, page, pageSize, CurrentUserId);
        return Ok(rants);
    }

    [HttpGet("search")]
    public async Task<ActionResult> SearchUsers([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest(new { message = "Search query is required." });
        var users = await _userService.SearchUsersAsync(q);
        return Ok(users);
    }

    [HttpGet("suggested")]
    public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetSuggestedUsers([FromQuery] int count = 5)
    {
        var users = await _userService.GetSuggestedUsersAsync(CurrentUserId, count);
        return Ok(users);
    }

    [HttpGet("{username}/replies")]
    public async Task<ActionResult> GetUserReplies(
        string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (pageSize > 50) pageSize = 50;
        if (page < 1) page = 1;
        var replies = await _userService.GetUserRepliesAsync(username, page, pageSize);
        return Ok(replies);
    }

    [HttpGet("{username}/likes")]
    public async Task<ActionResult<IEnumerable<RantResponseDto>>> GetUserLikes(
        string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (pageSize > 50) pageSize = 50;
        if (page < 1) page = 1;
        var rants = await _userService.GetUserLikedRantsAsync(username, page, pageSize, CurrentUserId);
        return Ok(rants);
    }

    // --- Profile management (auth required) ---

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var success = await _userService.UpdateProfileAsync(CurrentUserId!, dto);
        if (!success) return NotFound(new { message = "User not found." });
        return Ok(new { message = "Profile updated." });
    }

    [HttpPost("profile/image")]
    [Authorize]
    public async Task<IActionResult> UploadProfileImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { message = "Only JPEG, PNG, GIF and WebP images are allowed." });

        if (file.Length > 104857600) // 100 MB limit
            return BadRequest(new { message = "File size must not exceed 100 MB." });

        var success = await _userService.UpdateProfileImageAsync(CurrentUserId!, file);
        if (!success) return NotFound(new { message = "User not found or upload failed." });

        // Return updated profile so frontend can refresh the image URL
        var updatedProfile = await _userService.GetUserByUsernameAsync(
            User.FindFirstValue(ClaimTypes.Name)!, CurrentUserId);

        return Ok(new { message = "Profile image uploaded.", profileImageUrl = updatedProfile?.ProfileImageUrl });
    }

    [HttpPost("profile/banner")]
    [Authorize]
    public async Task<IActionResult> UploadBannerImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { message = "Only JPEG, PNG, GIF and WebP images are allowed." });

        if (file.Length > 104857600) // 100 MB limit for banners
            return BadRequest(new { message = "File size must not exceed 100 MB." });

        var success = await _userService.UpdateBannerImageAsync(CurrentUserId!, file);
        if (!success) return NotFound(new { message = "User not found or upload failed." });

        // Return updated profile so frontend can refresh the image URL
        var updatedProfile = await _userService.GetUserByUsernameAsync(
            User.FindFirstValue(ClaimTypes.Name)!, CurrentUserId);

        return Ok(new { message = "Banner image uploaded.", bannerImageUrl = updatedProfile?.BannerImageUrl });
    }

    [HttpDelete("profile/image")]
    [Authorize]
    public async Task<IActionResult> DeleteProfileImage()
    {
        var success = await _userService.RemoveProfileImageAsync(CurrentUserId!);
        if (!success) return NotFound(new { message = "User not found." });
        return Ok(new { message = "Profile image removed." });
    }

    [HttpDelete("profile/banner")]
    [Authorize]
    public async Task<IActionResult> DeleteBannerImage()
    {
        var success = await _userService.RemoveBannerImageAsync(CurrentUserId!);
        if (!success) return NotFound(new { message = "User not found." });
        return Ok(new { message = "Banner image removed." });
    }

    // --- Follows ---

    [HttpPost("{username}/follow")]
    [Authorize]
    public async Task<IActionResult> ToggleFollow(string username)
    {
        var success = await _followService.ToggleFollowAsync(CurrentUserId!, username);
        if (!success) return BadRequest(new { message = "Could not follow user. Either they don't exist or you're trying to follow yourself." });
        return Ok(new { message = "Follow status toggled." });
    }

    [HttpGet("{username}/followers")]
    public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetFollowers(
        string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var followers = await _followService.GetFollowersAsync(username, page, pageSize, CurrentUserId);
        return Ok(followers);
    }

    [HttpGet("{username}/following")]
    public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetFollowing(
        string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var following = await _followService.GetFollowingAsync(username, page, pageSize, CurrentUserId);
        return Ok(following);
    }
}
