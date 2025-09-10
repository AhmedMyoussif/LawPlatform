using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LawPlatform.Entities.Models.Auth.Identity;


namespace LawPlatform.Entities.Models.Auth.Users
{
    public class Client
    {
        [Key]
        public string Id { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}
