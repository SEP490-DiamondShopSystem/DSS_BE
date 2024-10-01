﻿using DiamondShop.Domain.Models.JewelryModels.Entities;
using DiamondShop.Domain.Models.JewelryModels.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Databases.Configurations.JewelryModelConfig
{
    internal class SizeConfiguration : IEntityTypeConfiguration<Size>
    {
        protected static List<Size> SIZES = new List<Size>
        {
            Size.Create(3),
            Size.Create(4),
            Size.Create(5),
            Size.Create(6),
            Size.Create(7),
            Size.Create(8),
            Size.Create(9),
            Size.Create(10),
            Size.Create(11),
            Size.Create(12),
        };
        public void Configure(EntityTypeBuilder<Size> builder)
        {
            builder.ToTable("Size");
            builder.Property(o => o.Id)
                .HasConversion(
                    Id => Id.Value,
                    dbValue => SizeId.Parse(dbValue));
            builder.HasKey(p => p.Id);
            builder.HasIndex(p => p.Id);
            builder.HasData(SIZES);
        }
    }
}
