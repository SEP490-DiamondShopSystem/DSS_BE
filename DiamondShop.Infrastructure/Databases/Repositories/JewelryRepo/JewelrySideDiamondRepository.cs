﻿using DiamondShop.Domain.Models.Jewelries.Entities;
using DiamondShop.Domain.Repositories.JewelryRepo;

namespace DiamondShop.Infrastructure.Databases.Repositories.JewelryRepo
{
    internal class JewelrySideDiamondRepository : BaseRepository<JewelrySideDiamond>, IJewelrySideDiamondRepository
    {
        public JewelrySideDiamondRepository(DiamondShopDbContext dbContext) : base(dbContext)
        {
        }

        public async Task CreateRange(List<JewelrySideDiamond> jewelrySideDiamonds)
        {
            await _set.AddRangeAsync(jewelrySideDiamonds);
        }
    }
}
