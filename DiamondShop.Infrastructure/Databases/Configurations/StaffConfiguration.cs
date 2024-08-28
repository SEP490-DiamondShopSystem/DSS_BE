using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.CustomerAggregate.ValueObjects;
using DiamondShop.Domain.Models.StaffAggregate;
using DiamondShop.Domain.Models.StaffAggregate.ValueObjects;
using DiamondShop.Infrastructure.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Databases.Configurations
{
    internal class StaffConfiguration : IEntityTypeConfiguration<Staff>
    {
        public void Configure(EntityTypeBuilder<Staff> builder)
        {
            builder.ToTable("Staff");
            builder.Property(o => o.Id)
               .HasConversion(
                   Id => Id.value,
                   dbValue => StaffId.Parse(dbValue));

            builder.OwnsOne(o => o.FullName,
               fullname =>
               {
                   fullname.Property(n => n.FirstName).HasColumnName("FirstName");  // Maps to FirstName column
                   fullname.Property(n => n.LastName).HasColumnName("LastName");
                   fullname.Ignore(n => n.Value);
               });
            builder.HasOne<CustomIdentityUser>()
                .WithOne()
                .HasForeignKey<Staff>(o => o.IdentityId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasMany(c => c.Roles)
                .WithMany(a => a.Staffs);

            builder.HasKey(o => o.Id);
            builder.HasIndex(o => o.Id);
            builder.HasIndex(o => o.IdentityId);
        }
    }
}
