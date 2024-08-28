using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.CustomerAggregate;
using DiamondShop.Domain.Models.CustomerAggregate.ValueObjects;
using DiamondShop.Domain.Models.StaffAggregate;
using DiamondShop.Infrastructure.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DiamondShop.Infrastructure.Databases.Configurations
{
    internal class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        private const string IDENTITY_COLUMN_NAME = "IdentityId";
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customer");
            builder.Property(o => o.Id)
               .HasConversion(
                   Id => Id.Value,
                   dbValue => CustomerId.Parse(dbValue));
            builder.OwnsOne(o => o.FullName,
                fullname =>
                {
                    fullname.Property(n => n.FirstName).HasColumnName("FirstName");  // Maps to FirstName column
                    fullname.Property(n => n.LastName).HasColumnName("LastName");
                    fullname.Ignore(n => n.Value);
                });
            builder.HasOne<CustomIdentityUser>()
                .WithOne()
                .HasForeignKey<Customer>(c => c.IdentityId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.Roles)
                .WithMany(a => a.Customers);


            builder.HasKey(o => o.Id);
            builder.HasIndex(o => o.Id);
            builder.HasIndex(o => o.IdentityId);

        }
    }
}
