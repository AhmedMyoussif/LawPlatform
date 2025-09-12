using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPlatform.Entities.Models.Auth.Users;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LawPrlatform.DataAccess.EntitiesConfigurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasOne(c => c.User)
                   .WithOne()
                   .HasForeignKey<Client>(c => c.Id)
                   .OnDelete(DeleteBehavior.Cascade);
            
            builder.Property(b => b.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(b => b.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(b => b.Address)
                .IsRequired();

            builder.Property(b => b.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(b => b.UpdatedAt)
                .IsRequired(false);

            builder.ToTable("Clients");
        }
    }
}
