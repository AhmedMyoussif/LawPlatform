using System.Security.Claims;
using LawPlatform.API.Hubs;
using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.DataAccess.Services.Notification;
using LawPlatform.Entities.DTO.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly LawPlatformContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NotificationsController(INotificationService notificationService,
                                   IHubContext<NotificationHub> hubContext,
                                   LawPlatformContext context,
                                   IHttpContextAccessor httpContextAccessor)
    {
        _notificationService = notificationService;
        _hubContext = hubContext;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] NotificationDto dto)
    {
        await _notificationService.NotifyUserAsync(dto.UserId, dto.Title, dto.Message);

        await _hubContext.Clients.User(dto.UserId).SendAsync("ReceiveNotification", dto.Title, dto.Message);

        return Ok(new { Message = "Notification sent." });
    }

    [HttpGet("my-notifications")]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return Ok(notifications);
    }

    private string GetUserId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
