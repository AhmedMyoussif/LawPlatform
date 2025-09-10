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
    public class ConsultationFileConfiguration : IEntityTypeConfiguration<ConsultationFile>
    {
        public void Configure(EntityTypeBuilder<ConsultationFile> builder)
        {
            builder.HasKey(f => f.Id);

            builder.Property(f => f.FilePath)
                   .IsRequired();

            builder.Property(f => f.FileName)
                   .HasMaxLength(200)
                   .IsRequired();


            // Relation with Consultation
            builder.HasOne(f => f.Consultation)
                   .WithMany(c => c.Files)
                   .HasForeignKey(f => f.ConsultationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
