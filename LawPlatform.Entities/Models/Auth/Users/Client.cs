using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LawPlatform.Entities.Models.Auth.Identity;
using Microsoft.AspNetCore.Http;


namespace LawPlatform.Entities.Models.Auth.Users
{
    public class Client
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [ForeignKey(nameof(Id))]
        public User User { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ProfileImage ProfileImage { get; set; }
        public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();


    }
}
