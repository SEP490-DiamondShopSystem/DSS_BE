using DiamondShop.Domain.Models.Orders.ValueObjects;
using DiamondShop.Domain.Models.Transactions;
using DiamondShop.Domain.Models.Transactions.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Databases.Configurations.TransactionConfig
{
    internal class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transaction");
            builder.Property(o => o.Id)
                .HasConversion(
                    o => o.Value,
                    dbValue => TransactionId.Parse(dbValue));
            builder.Property(o => o.PayMethodId)
            .HasConversion(
                Id => Id.Value,
                dbValue => PaymentMethodId.Parse(dbValue));
            builder.Property(o => o.RefundedTransacId)
            .HasConversion(
                Id => Id.Value,
                dbValue => TransactionId.Parse(dbValue));
            builder.Property(o => o.OrderId)
               .HasConversion(
                   o => o.Value,
                   dbValue => OrderId.Parse(dbValue));
            builder.Property(o => o.TransactionType).HasConversion<string>();
            builder.HasOne(o => o.PayMethod).WithMany().HasForeignKey(o => o.PayMethodId).IsRequired(false);
            builder.HasOne(o => o.RefundedTransaction).WithOne().HasForeignKey<Transaction>(o => o.RefundedTransacId).IsRequired(false);
            builder.HasKey(o => o.Id);
        }
    }
}
