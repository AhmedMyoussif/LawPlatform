using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Entities.Models.Auth.Users;

namespace LawPlatform.Entities.Models
{
    public class ProfileImage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public string? ClientId { get; set; }
        [ForeignKey(nameof(ClientId))]
        public Client? Client { get; set; }

        public string? LawyerId { get; set; }
        [ForeignKey(nameof(LawyerId))]
        public Lawyer? Lawyer { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
