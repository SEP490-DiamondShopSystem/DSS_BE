using DiamondShop.Domain.Models.StaffAggregate.ValueObjects;
using DiamondShop.Domain.Roles;
using DiamondShop.Infrastructure.Identity.Models;
using DiamondShop.Infrastructure.Securities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Databases.Configurations
{
    internal class IdentityConfiguration
    {
        internal static void ApplyIdentityConfiguration(ModelBuilder builder)
        {
            builder.Entity<CustomIdentityUser>().ToTable("User");
            builder.Entity<CustomIdentityUserRole>().ToTable("UserRoles");
            builder.Entity<CustomIdentityRole>().ToTable("Role");
            builder.Entity<CustomIdentityUserRoleClaim>().ToTable("RoleClaims");
            builder.Entity<CustomIdentityUserClaims>().ToTable("UserClaims");
            builder.Entity<CustomIdentityUserLogins>().ToTable("UserLogins");
            builder.Entity<CustomIdentityUserToken>().ToTable("UserToken");
            builder.Entity<CustomIdentityUser>(b =>
            {
                // Each User can have many UserClaims
                b.HasMany(e => e.UserClaims)
                    .WithOne()
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                // Each User can have many UserLogins
                b.HasMany(e => e.UserLogins)
                    .WithOne()
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                // Each User can have many UserTokens
                b.HasMany(e => e.UserTokens)
                    .WithOne()
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();
                // Each User can have many role
                b.HasMany(e => e.UserRoles)
                .WithMany(r => r.Users)
                .UsingEntity<CustomIdentityUserRole>();
            });
            builder.Entity<CustomIdentityRole>(b =>
            {
                b.HasMany(e => e.RoleClaims)
                    .WithOne(e => e.Role)
                    .HasForeignKey(rc => rc.RoleId)
                    .IsRequired();
                //b.HasData(SYSTEM_ROLE);
            });
        }
        
    }
    
}
