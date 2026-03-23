using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using socmed_backend.DTOs;
using socmed_backend.Services;

namespace socmed_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RepliesController : ControllerBase
{
    private readonly IReplyService _replyService;

    public RepliesController(IReplyService replyService)
    {
        _replyService = replyService;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPut("{replyId}")]
    public async Task<IActionResult> UpdateReply(string replyId, [FromBody] UpdateReplyDto dto)
    {
        var reply = await _replyService.UpdateReplyAsync(replyId, CurrentUserId, dto);
        if (reply == null) return BadRequest(new { message = "Could not update reply. It might not exist or you don't have permission." });
        return Ok(reply);
    }

    [HttpDelete("{replyId}")]
    public async Task<IActionResult> DeleteReply(string replyId)
    {
        var success = await _replyService.DeleteReplyAsync(replyId, CurrentUserId);
        if (!success) return BadRequest(new { message = "Could not delete reply. It might not exist or you don't have permission." });
        return NoContent();
    }

    [HttpPost("{replyId}/like")]
    public async Task<IActionResult> ToggleLike(string replyId)
    {
        await _replyService.ToggleLikeAsync(replyId, CurrentUserId);
        return Ok();
    }
}
