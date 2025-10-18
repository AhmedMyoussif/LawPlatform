using LawPlatform.Entities.Models.Auth.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.Entities.Models;

public class Review
{
    public Guid Id { get; set; }
    public string? Comment { get; set; } = null!;
    public double Rating { get; set; }
    public string LawyerId { get; set; }
    public string ClientId { get; set; }
    public bool? IsDeleted { get; set; }

    public User Lawyer { get; set; } = null!;
    public User Client { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt{ get; set; }
    public DateTime? DeletedAt {get; set; }
}
