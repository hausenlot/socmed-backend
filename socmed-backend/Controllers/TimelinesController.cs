using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using socmed_backend.DTOs;
using socmed_backend.Services;

namespace socmed_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimelinesController : ControllerBase
{
    private readonly ITimelineService _timelineService;

    public TimelinesController(ITimelineService timelineService)
    {
        _timelineService = timelineService;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private string? CurrentUsername => User.FindFirstValue(ClaimTypes.Name);

    [HttpGet("home")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<RantResponseDto>>> GetHomeTimeline(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var rants = await _timelineService.GetHomeTimelineAsync(CurrentUserId!, page, pageSize);
        return Ok(rants);
    }

    [HttpGet("user/{username}")]
    public async Task<ActionResult<IEnumerable<RantResponseDto>>> GetUserTimeline(
        string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var rants = await _timelineService.GetUserTimelineAsync(username, page, pageSize, CurrentUserId);
        return Ok(rants);
    }

    [HttpGet("mentions")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<RantResponseDto>>> GetMentionsTimeline(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var rants = await _timelineService.GetMentionsTimelineAsync(CurrentUsername!, page, pageSize, CurrentUserId);
        return Ok(rants);
    }

    [HttpGet("bookmarks")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<RantResponseDto>>> GetBookmarksTimeline(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var rants = await _timelineService.GetBookmarksTimelineAsync(CurrentUserId!, page, pageSize);
        return Ok(rants);
    }
}
