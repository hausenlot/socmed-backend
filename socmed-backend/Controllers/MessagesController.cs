using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using socmed_backend.DTOs;
using socmed_backend.Services;

namespace socmed_backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    /// <summary>Get all conversations for the current logged-in user.</summary>
    [HttpGet("conversations")]
    public async Task<ActionResult<IEnumerable<ConversationDto>>> GetConversations()
    {
        var conversations = await _messageService.GetConversationsAsync(CurrentUserId);
        return Ok(conversations);
    }

    /// <summary>Start or retrieve a 1-to-1 conversation by recipient username.</summary>
    [HttpPost("conversations/1to1")]
    public async Task<ActionResult<ConversationDto>> GetOrCreate1To1([FromBody] Start1To1ChatRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RecipientUsername))
            return BadRequest(new { message = "Recipient username is required." });

        try
        {
            var conversation = await _messageService.GetOrCreate1To1ConversationAsync(CurrentUserId, dto.RecipientUsername);
            return Ok(conversation);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get message history for a specific conversation (paginated).</summary>
    [HttpGet("conversations/{conversationId}/history")]
    public async Task<ActionResult<IEnumerable<MessageResponseDto>>> GetChatHistory(
        string conversationId, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var history = await _messageService.GetChatHistoryAsync(CurrentUserId, conversationId, page, pageSize);
            return Ok(history);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>Send a message inside an existing conversation.</summary>
    [HttpPost("conversations/{conversationId}")]
    public async Task<ActionResult<MessageResponseDto>> SendMessage(string conversationId, [FromBody] SendMessageRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest(new { message = "Content cannot be empty." });

        try
        {
            var message = await _messageService.SendMessageAsync(CurrentUserId, conversationId, dto.Content);
            if (message == null)
                return NotFound(new { message = "Conversation not found or access denied." });

            return Ok(message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>Mark a conversation's messages as read by the current user.</summary>
    [HttpPut("conversations/{conversationId}/read")]
    public async Task<IActionResult> MarkAsRead(string conversationId)
    {
        var success = await _messageService.MarkAsReadAsync(CurrentUserId, conversationId);
        if (!success)
            return NotFound(new { message = "Conversation participant entry not found." });

        return Ok(new { message = "Conversation marked as read." });
    }
}
