using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LawPlatform.DataAccess.EntitiesConfigurations
{
    public class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
    {
        public void Configure(EntityTypeBuilder<Proposal> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Amount)
                   .IsRequired();

            builder.Property(o => o.Description)
                   .HasMaxLength(2000);

            builder.Property(o => o.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(o => o.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // Relation with Lawyer
            builder.HasOne(o => o.Lawyer)
                   .WithMany(l => l.Proposals)
                   .HasForeignKey(o => o.LawyerId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Enum as string
            builder.Property(o => o.Status)
                   .HasConversion<string>()
                   .HasDefaultValue(LawPlatform.Utilities.Enums.ProposalStatus.Pending);
        }
    }
}
