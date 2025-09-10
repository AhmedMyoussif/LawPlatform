using System.ComponentModel.DataAnnotations;

using LawPlatform.Entities.Models.Auth.Identity;

namespace LawPlatform.Entities.Models.Auth.UserTokens
{
    public class UserRefreshToken
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string? Token { get; set; }
        public bool IsUsed { get; set; } //  true معناها إن الـ refresh token استخدم مرة، ومش المفروض يُستخدم تاني
        public DateTime ExpiryDateUtc { get; set; }
        public virtual User? User { get; set; }
    }
}
