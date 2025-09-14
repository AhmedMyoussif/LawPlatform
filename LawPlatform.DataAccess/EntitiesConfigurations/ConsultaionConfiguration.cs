using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawPlatform.DataAccess.EntitiesConfigurations
{
    public class ConsultationConfiguration : IEntityTypeConfiguration<Consultation>
    {
        public void Configure(EntityTypeBuilder<Consultation> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(c => c.Description)
                   .HasMaxLength(2000);

            builder.Property(c => c.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(c => c.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // Relation with Client
            builder.HasOne(c => c.Client)
                   .WithMany(u => u.Consultations)
                   .HasForeignKey(c => c.ClientId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relation with Offers
            builder.HasMany(c => c.Proposals)
                   .WithOne(o => o.Consultation)
                   .HasForeignKey(o => o.ConsultationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
