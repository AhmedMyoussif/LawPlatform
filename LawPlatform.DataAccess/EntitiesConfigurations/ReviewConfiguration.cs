using LawPlatform.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPlatform.DataAccess.EntitiesConfigurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Comment)
               .HasMaxLength(1000);

        builder.Property(r => r.Rating)
                .IsRequired();

        builder.Property(r => r.IsDeleted)
               .HasDefaultValue(false);

        builder.Property(r => r.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()")
               .ValueGeneratedOnAdd();

        builder.Property(r => r.UpdatedAt)
                .HasDefaultValue(null)
                .ValueGeneratedOnUpdate();

        builder.Property(r => r.DeletedAt)
                .HasDefaultValue(null);

        // Relation with Lawyer (User)
        builder.HasOne(r => r.Lawyer)
               .WithMany()
               .HasForeignKey(r => r.LawyerId)
               .OnDelete(DeleteBehavior.Restrict);

        // Relation with Client (User)
        builder.HasOne(r => r.Client)
               .WithMany()
               .HasForeignKey(r => r.ClientId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
