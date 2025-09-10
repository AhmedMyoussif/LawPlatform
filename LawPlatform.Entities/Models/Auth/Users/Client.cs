using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LawPlatform.Entities.Models.Auth.Identity;


namespace LawPlatform.Entities.Models.Auth.Users
{
    public class Client
    {
        public string Id { get; set; }
        public User User { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
    }
}
