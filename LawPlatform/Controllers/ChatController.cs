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
        public async Task<IActionResult> GetConversation(Guid chatId)
        {
            var me = GetCurrentUserId();
            if (string.IsNullOrEmpty(me)) return Unauthorized();

            var msgs = await _chatService.GetConversationAsync(chatId);
            return Ok(msgs);
        }


        [HttpGet("get/cahtId")]
        public async Task<IActionResult> GetChat([FromQuery] string otherUserId, [FromQuery] Guid consultationId)
        {
            var me = GetCurrentUserId();
            if (string.IsNullOrEmpty(me)) return Unauthorized();

            var response = await _chatService.GetChatAsync(me, otherUserId, consultationId);

            if (!response.Succeeded)
                return NotFound(response);

            return Ok(response);
        }


        //[HttpPost("private/send")]
        //public async Task<IActionResult> SendPrivate([FromBody] SendMessageRequest model)
        //{
        //    var me = GetCurrentUserId();
        //    if (string.IsNullOrEmpty(me)) return Unauthorized();

        //    if (model == null || string.IsNullOrWhiteSpace(model.ReceiverId) || string.IsNullOrWhiteSpace(model.Content))
        //        return BadRequest("ReceiverId and Content are required.");

        //    var canChat = await _chatService.CanUsersChatAsync(me, model.ReceiverId);
        //    if (!canChat) return Forbid("You are not allowed to message this user.");

        //    var chatResponse = await _chatService.GetChatAsync(me, model.ReceiverId, model.ConsultationId);
        //    Guid chatId;

        //    if (!chatResponse.Succeeded || chatResponse.Data == null)
        //    {
        //        var newChat = new Chat
        //        {
        //            Id = Guid.NewGuid(),
        //            UserAId = me,
        //            UserBId = model.ReceiverId,
        //            ConsultationId = model.ConsultationId
        //        };
        //        await _chatService.CreateChatAsync(newChat);
        //        chatId = newChat.Id;
        //    }
        //    else
        //    {
        //        chatId = chatResponse.Data.ChatId;
        //    }

        //    var msg = new ChatMessage
        //    {
        //        SenderId = me,
        //        ReceiverId = model.ReceiverId,
        //        Content = model.Content,
        //        SentAt = DateTimeOffset.UtcNow,
        //        ConsultationId = model.ConsultationId,
        //        IsRead = false,
        //        ChatId = chatId
        //    };

        //    try
        //    {
        //        await _chatService.SaveMessageAsync(msg);

        //        await _hubContext.Clients.User(model.ReceiverId)
        //            .SendAsync("ReceivePrivateMessage", me, model.Content, msg.SentAt);

        //        await _hubContext.Clients.User(me)
        //            .SendAsync("MessageSent", model.ReceiverId, model.Content, msg.SentAt);

        //        return Ok(new { success = true, sentAt = msg.SentAt });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to save/send chat message from {Sender} to {Receiver}", me, model.ReceiverId);
        //        return StatusCode(500, "Failed to send message.");
        //    }
        //}

        [HttpPost("private/send")]
        public async Task<IActionResult> SendPrivate([FromBody] SendMessageRequest model)
        {
            var me = GetCurrentUserId();
            if (string.IsNullOrEmpty(me)) return Unauthorized();

            try
            {
                var msg = await _chatService.SendPrivateMessageAsync(me, model.ReceiverId, model.Content, model.ConsultationId);

                await _hubContext.Clients.User(model.ReceiverId)
                    .SendAsync("ReceivePrivateMessage", me, model.Content, msg.SentAt);

                await _hubContext.Clients.User(me)
                    .SendAsync("MessageSent", model.ReceiverId, model.Content, msg.SentAt);

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

        //[HttpGet("conversation")]
        //public async Task<IActionResult> GetConversation([FromQuery] string user1Id, [FromQuery] string user2Id, [FromQuery] Guid consultationId)
        //{
        //    var messages = await _chatService.GetConversationAsync(user1Id, user2Id, consultationId);
        //    return Ok(messages);
        //}


       

    }
}
