using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using socmed_backend.DTOs;
using socmed_backend.Services;

namespace socmed_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RantsController : ControllerBase
{
    private readonly IRantService _rantService;
    private readonly IRantInteractionService _interactionService;
    private readonly IReplyService _replyService;

    public RantsController(IRantService rantService, IRantInteractionService interactionService, IReplyService replyService)
    {
        _rantService = rantService;
        _interactionService = interactionService;
        _replyService = replyService;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RantResponseDto>>> GetRants()
    {
        var rants = await _rantService.GetAllRantsAsync(CurrentUserId);
        return Ok(rants);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RantResponseDto>> GetRant(int id)
    {
        var rant = await _rantService.GetRantByIdAsync(id, CurrentUserId);
        if (rant == null) return NotFound();
        return Ok(rant);
    }

    [HttpPost]
    [Authorize]
    [DisableRequestSizeLimit]
    public async Task<ActionResult<RantResponseDto>> CreateRant([FromForm] CreateRantDto dto, [FromServices] IMultimediaService multimediaService)
    {
        string? mediaId = null;
        string? mediaType = null;
        if (dto.MediaFile != null)
        {
            using var stream = dto.MediaFile.OpenReadStream();
            var uploadResult = await multimediaService.UploadFileAsync(stream, dto.MediaFile.FileName, dto.MediaFile.ContentType);
            if (uploadResult != null)
            {
                mediaId = uploadResult.FileId;
                mediaType = uploadResult.MediaType;
            }
        }

        var rant = await _rantService.CreateRantAsync(dto.Content, CurrentUserId!, dto.QuoteRantId, mediaId, mediaType);
        return CreatedAtAction(nameof(GetRant), new { id = rant.Id }, rant);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateRant(int id, [FromBody] UpdateRantDto dto)
    {
        var success = await _rantService.UpdateRantAsync(id, dto.Content, CurrentUserId!);
        if (!success) return BadRequest(new { message = "Could not update rant. It may not exist or you don't have permission." });
        return NoContent();
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> PatchRant(int id, [FromBody] UpdateRantDto dto)
    {
        var success = await _rantService.UpdateRantAsync(id, dto.Content, CurrentUserId!);
        if (!success) return BadRequest(new { message = "Could not update rant. It may not exist or you don't have permission." });
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteRant(int id)
    {
        var success = await _rantService.SoftDeleteRantAsync(id, CurrentUserId!);
        if (!success) return BadRequest(new { message = "Could not delete rant. It may not exist or you don't have permission." });
        return NoContent();
    }

    [HttpGet("explore")]
    public async Task<ActionResult<IEnumerable<RantResponseDto>>> GetExplore()
    {
        var rants = await _rantService.GetExploreRantsAsync(CurrentUserId);
        return Ok(rants);
    }

    // --- Interactions ---

    [HttpPost("{id}/like")]
    [Authorize]
    public async Task<IActionResult> ToggleLike(int id)
    {
        var success = await _interactionService.ToggleLikeAsync(id, CurrentUserId!);
        if (!success) return NotFound(new { message = "Rant not found." });
        return Ok();
    }

    [HttpPost("{id}/rerant")]
    [Authorize]
    public async Task<IActionResult> ToggleReRant(int id)
    {
        var success = await _interactionService.ToggleReRantAsync(id, CurrentUserId!);
        if (!success) return NotFound(new { message = "Rant not found." });
        return Ok();
    }

    [HttpPost("{id}/bookmark")]
    [Authorize]
    public async Task<IActionResult> ToggleBookmark(int id)
    {
        var success = await _interactionService.ToggleBookmarkAsync(id, CurrentUserId!);
        if (!success) return NotFound(new { message = "Rant not found." });
        return Ok();
    }

    [HttpGet("{id}/likes")]
    public async Task<ActionResult<IEnumerable<string>>> GetLikes(int id)
    {
        var likers = await _interactionService.GetLikesAsync(id);
        return Ok(likers);
    }

    [HttpGet("{id}/rerants")]
    public async Task<ActionResult<IEnumerable<string>>> GetReRants(int id)
    {
        var reranters = await _interactionService.GetReRantsAsync(id);
        return Ok(reranters);
    }

    // --- Replies ---

    [HttpGet("{id}/replies")]
    public async Task<ActionResult> GetReplies(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var replies = await _replyService.GetRepliesAsync(id, CurrentUserId, page, pageSize);
        return Ok(replies);
    }

    [HttpPost("{id}/replies")]
    [Authorize]
    public async Task<ActionResult> CreateReply(int id, [FromBody] CreateReplyDto dto)
    {
        var reply = await _replyService.CreateReplyAsync(id, CurrentUserId!, dto);
        if (reply == null) return NotFound(new { message = "Rant not found." });
        return Ok(reply);
    }
}
