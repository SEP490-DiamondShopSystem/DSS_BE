﻿using DiamondShop.Domain.Common;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.JewelryModels.ValueObjects;

namespace DiamondShop.Domain.Models.JewelryModels.Entities
{
    public class Metal : Entity<MetalId>
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public Media? Thumbnail { get; set; }
        public Metal() { }
        public static Metal Create(string name, decimal price, MetalId? givenId = null) => new Metal() { Id = givenId is null ? MetalId.Create() : givenId, Name = name, Price = price };
        public void Update(decimal price) => Price = price;
    }
}
