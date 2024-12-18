﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiamondShop.Domain.Models.Promotions;
using DiamondShop.Domain.Models.Promotions.ValueObjects;

namespace DiamondShop.Infrastructure.Databases.Configurations.PromoConfig
{
    internal class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
    {
        public void Configure(EntityTypeBuilder<Promotion> builder)
        {
            builder.ToTable("Promotion");
            builder.Property(o => o.Id)
                .HasConversion(
                    o => o.Value,
                    dbValue => PromotionId.Parse(dbValue));
            builder.HasMany(o => o.Gifts).WithOne(p => p.Promotion).HasForeignKey(p => p.PromotionId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            builder.Property(o => o.RedemptionMode).HasConversion<string>();
            builder.OwnsOne(o => o.Thumbnail, childNavigation =>
            {
                childNavigation.ToJson();
            });
            builder.HasKey(o => o.Id);
            builder.HasQueryFilter(x => x.Status != Domain.Models.Promotions.Enum.Status.Soft_deleted);
        }
    }
}
