using DiamondShop.Domain.Models.AccountRoleAggregate;
using DiamondShop.Domain.Models.AccountRoleAggregate.ValueObjects;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Models.StaffAggregate.ValueObjects;
using DiamondShop.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Databases.Configurations
{
    internal class AccountRoleConfiguration : IEntityTypeConfiguration<AccountRole>
    {
        internal static List<AccountRole> customerRoles = new List<AccountRole>
            {
                AccountRole.Customer,
                AccountRole.CustomerGold,
                AccountRole.CustomerSilver,
                AccountRole.CustomerBronze,
            };
        internal static List<AccountRole> storeRoles = new List<AccountRole>
            {
                AccountRole.Staff,
                AccountRole.Manager,
                AccountRole.Admin,
            };
        internal static List<AccountRole> allRoles = new List<AccountRole>
            {
             AccountRole.Customer,
                AccountRole.CustomerGold,
                AccountRole.CustomerSilver,
                AccountRole.CustomerBronze,
                AccountRole.Staff,
                AccountRole.Manager,
                AccountRole.Admin,
            };
        public void Configure(EntityTypeBuilder<AccountRole> builder)
        {
            builder.Property(o => o.Id)
               .HasConversion(
                   Id => Id.Value,
                   dbValue => AccountRoleId.Parse(dbValue));
            builder.HasKey(a => a.Id);

            builder.ToTable("Account_Role");

            builder.Property(a => a.RoleType).IsRequired(true);
            builder.Property(a => a.RoleName).IsRequired(true).HasMaxLength(50);
            //builder.HasDiscriminator(a => a.RoleType)
            //    .HasValue<AccountRole>(AccountRoleType.None)
            //    .HasValue<DiamondShopCustomerRole>(AccountRoleType.Customer)
            //    .HasValue<DiamondShopStoreRoles>(AccountRoleType.Staff);
            builder.HasIndex(a => a.RoleName).IsUnique();
            builder.HasData(allRoles);

        }
    }
}
