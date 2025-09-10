using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.Models.Auth.Users;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using LawPlatform.Utilities.Enums;

namespace LawPrlatform.DataAccess.EntitiesConfigurations
{
    public class LawyerConfiguration : IEntityTypeConfiguration<Lawyer>
    {
        public void Configure(EntityTypeBuilder<Lawyer> builder)
        {
            builder.HasKey(l => l.Id);

            builder.HasOne(l => l.User)
                   .WithOne()
                   .HasForeignKey<Lawyer>(l => l.Id)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(l => l.Bio)
                   .HasMaxLength(1000);

            builder.Property(l => l.Qualifications)
                   .HasMaxLength(500);

            builder.Property(l => l.BankAccountNumber)
                   .HasMaxLength(50);

            builder.Property(l => l.BankName)
                   .HasMaxLength(100);

            builder.Property(l => l.Country)
                   .HasMaxLength(100);

            builder.Property(l => l.Status)
                   .HasDefaultValue(ApprovalStatus.Pending);

            builder.Property(l => l.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.ToTable("Lawyers");
        }
    }
}
