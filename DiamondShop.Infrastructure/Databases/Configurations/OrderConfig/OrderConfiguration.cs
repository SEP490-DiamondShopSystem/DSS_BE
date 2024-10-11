using DiamondShop.Domain.Models.Orders.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Models.Orders.ValueObjects;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Models.Transactions.ValueObjects;
using DiamondShop.Domain.Models.CustomizeRequests;
using DiamondShop.Domain.Models.CustomizeRequests.ValueObjects;

namespace DiamondShop.Infrastructure.Databases.Configurations.OrderConfig
{
    internal class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Order");
            builder.Property(o => o.Id)
                .HasConversion(
                    o => o.Value,
                    dbValue => OrderId.Parse(dbValue));
            builder.Property(o => o.AccountId).HasConversion(
                o => o.Value,
                dbValue => AccountId.Parse(dbValue));
            builder.Property(p => p.DeliveryPackageId)
            .HasConversion(
                Id => Id.Value,
                dbValue => DeliveryPackageId.Parse(dbValue));
            builder.Property(p => p.CustomizeRequestId)
            .HasConversion(
                Id => Id.Value,
                dbValue => CustomizeRequestId.Parse(dbValue));

            builder.HasOne(o => o.Account).WithMany().HasForeignKey(o => o.AccountId).IsRequired();

            builder.Property(o => o.Status).HasConversion<string>();
            builder.Property(o => o.PaymentType).HasConversion<string>();
            builder.Property(o => o.PaymentStatus).HasConversion<string>();

            builder.HasMany(o => o.Transactions).WithOne().HasForeignKey(o => o.OrderId).IsRequired(false);
            builder.HasMany(o => o.Items).WithOne().HasForeignKey(p => p.OrderId).IsRequired();
            builder.HasMany(o => o.Logs).WithOne().HasForeignKey(p => p.OrderId).IsRequired();
            
            builder.HasOne<Order>().WithOne().HasForeignKey<Order>(o => o.ParentOrderId).IsRequired(false);
            builder.HasOne<DeliveryPackage>().WithMany().HasForeignKey(o => o.DeliveryPackageId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne<CustomizeRequest>().WithOne().HasForeignKey<Order>(o => o.CustomizeRequestId).IsRequired(false);
            builder.HasKey(o => o.Id);
        }
    }
}
