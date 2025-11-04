// LawPlatform.API/Controllers/ChatController.cs
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using LawPlatform.DataAccess.Services.Chat;
using LawPlatform.Entities.Models;
using LawPlatform.API.Hubs;
using LawPlatform.Entities.DTO.chat;

namespace LawPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _hubContext = hubContext;
            _logger = logger;
        }

        private string? GetCurrentUserId()
        {
            return User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? User?.FindFirst("nameid")?.Value;
        }

        [HttpGet("conversation/{chatId}")]
        [Authorize]
        public async Task<IActionResult> GetConversation(Guid chatId, int pageNumber = 1, int pageSize = 50)
        {
            var me = GetCurrentUserId();
            if (string.IsNullOrEmpty(me)) return Unauthorized();

            var msgs = await _chatService.GetConversationAsync(chatId, pageNumber, pageSize);
            return Ok(msgs);
        }


        //[HttpGet("chatId")]
        //public async Task<IActionResult> GetChat([FromQuery] string otherUserId, [FromQuery] Guid consultationId)
        //{
        //    var me = GetCurrentUserId();
        //    if (string.IsNullOrEmpty(me)) return Unauthorized();

        //    var response = await _chatService.GetChatAsync(me, otherUserId, consultationId);

        //    if (!response.Succeeded)
        //        return NotFound(response);

        //    return Ok(response);
        //}


        [HttpPost]
        public async Task<IActionResult> GetChatId([FromBody] SendMessageRequest model)
        {
            var me = GetCurrentUserId();
            if (string.IsNullOrEmpty(me)) return Unauthorized();

            try
            {
                var msg = await _chatService.GetChatId(me, model.ReceiverId, model.ConsultationId);
                return Ok(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message from {Sender} to {Receiver}", me, model.ReceiverId);
                return StatusCode(500, ex.Message);
            }
        }



        [HttpPost("private/mark-read/{otherUserId}")]
        public async Task<IActionResult> MarkAsRead(string otherUserId)
        {
            var me = GetCurrentUserId();
            if (string.IsNullOrEmpty(me)) return Unauthorized();

            try
            {
                await _chatService.MarkConversationAsReadAsync(me, otherUserId);

                await _hubContext.Clients.User(otherUserId).SendAsync("MessagesReadBy", me);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark conversation as read for {Reader} vs {Other}", me, otherUserId);
                return StatusCode(500, "Failed to mark as read.");
            }
        }

       

    }
}
