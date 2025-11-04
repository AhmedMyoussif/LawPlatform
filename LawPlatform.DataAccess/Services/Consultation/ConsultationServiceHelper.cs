using LawPlatform.DataAccess.ApplicationContext;
using LawPlatform.Entities.DTO.Consultation;
using LawPlatform.Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

public static class ConsultationServiceHelper
{
    public static string? GetCurrentUserId(IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContextAccessor.HttpContext?.User.FindFirst("nameid")?.Value;
    }

    public static GetConsultationResponse ToConsultationResponse(Consultation c)
    {
        return new GetConsultationResponse
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            CreatedAt = c.CreatedAt,
            ClientId = c.ClientId,
            Budget = c.Budget,
            Duration = c.Duration,
            Status = c.Status,
            LawyerId = c.LawyerId,
            UrlFiles = c.Files?.Select(f => f.FilePath).ToList() ?? new(),
            Client = new ClientInfo
            {
                Id = c.Client.Id,
                FullName = c.Client.FirstName + " " + c.Client.LastName,
                ProfileImage = c.Client.ProfileImage?.ImageUrl
            }
        };
    }
    public static async Task<List<Consultation>> GetConsultationsAsync(
    LawPlatformContext context,
    Expression<Func<Consultation, bool>> predicate,
    bool includeFiles = false,
    int? take = null)
    {
        IQueryable<Consultation> query = context.consultations
            .Where(predicate)
            .Include(c => c.Client)
            .Include(c => c.Lawyer);

        if (includeFiles)
        {
            query = query.Include(c => c.Files);
        }

        query = query.OrderByDescending(c => c.CreatedAt);

        if (take.HasValue)
            query = query.Take(take.Value);

        return await query.ToListAsync();
    }


}
