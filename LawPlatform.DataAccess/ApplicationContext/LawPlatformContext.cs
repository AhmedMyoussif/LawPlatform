using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.Models;
using LawPlatform.Entities.Models.Auth.Identity;
using LawPlatform.Entities.Models.Auth.Users;
using LawPlatform.Entities.Models.Auth.UserTokens;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LawPlatform.DataAccess.ApplicationContext
{
    public class LawPlatformContext : IdentityDbContext<User, Role, string>, IDataProtectionKeyContext

    {
        public LawPlatformContext(DbContextOptions<LawPlatformContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LawPlatformContext).Assembly);
        }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<Lawyer> Lawyers { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Consultation> consultations { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<ConsultationCategory> ConsultationCategories { get; set; }
        public DbSet<ConsultationFile> ConsultationFiles { get; set; }

    }
}
