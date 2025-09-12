﻿using LawPlatform.Entities.Models.Auth.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPlatform.Utilities.Enums;

namespace LawPlatform.DataAccess.EntitiesConfigurations
{
    public class LawyerConfiguration : IEntityTypeConfiguration<Lawyer>
    {
        public void Configure(EntityTypeBuilder<Lawyer> builder)
        {
            builder.HasKey(l => l.Id);

            builder.HasOne(l => l.User)
                   .WithOne()
                   .HasForeignKey<Lawyer>(l => l.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(l => l.FirstName)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(l => l.LastName)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(l => l.Bio)
                   .HasMaxLength(1000);

            builder.Property(l => l.Experiences)
                   .HasMaxLength(500);

            builder.Property(l => l.Qualifications)
                   .HasMaxLength(500);

            builder.Property(l => l.LicenseNumber)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(l => l.Specialization)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(l => l.Country)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(l => l.IBAN)
                   .HasMaxLength(34)
                   .IsRequired();

            builder.Property(l => l.BankAccountNumber)
                   .HasMaxLength(50);

            builder.Property(l => l.BankName)
                   .HasMaxLength(150)
                   .IsRequired();

            builder.Property(l => l.Status)
                   .HasDefaultValue(ApprovalStatus.Pending);

            builder.Property(l => l.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(l => l.UpdatedAt)
                   .IsRequired(false);

            builder.ToTable("Lawyers");
        }
    }
}
