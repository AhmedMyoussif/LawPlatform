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
    public class CustomerConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasOne(c => c.User)
                   .WithOne()
                   .HasForeignKey<Client>(c => c.Id)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("Customers");
        }
    }
}
