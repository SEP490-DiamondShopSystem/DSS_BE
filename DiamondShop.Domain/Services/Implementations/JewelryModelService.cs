﻿using DiamondShop.Domain.Models.DiamondPrices.Entities;
using DiamondShop.Domain.Models.JewelryModels;
using DiamondShop.Domain.Models.JewelryModels.Entities;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Services.interfaces;

namespace DiamondShop.Domain.Services.Implementations
{
    public class JewelryModelService : IJewelryModelService
    {
        public void GetSizeMetalPrice(SizeMetal sizeMetal)
        {
            if (sizeMetal.Metal == null)
                throw new Exception("Please include metal to get price");
            sizeMetal.Price = sizeMetal.Metal.Price * (decimal)sizeMetal.Weight;
        }
        public void GetSizeMetalPrice(List<SizeMetal> sizeMetals)
        {
            foreach (var sizeMetal in sizeMetals)
            {
                if (sizeMetal.Metal == null)
                    throw new Exception("Please include metal to get price");
                sizeMetal.Price = sizeMetal.Metal.Price * (decimal)sizeMetal.Weight;
            }
        }
    }
}
