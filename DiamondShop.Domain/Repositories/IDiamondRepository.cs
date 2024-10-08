﻿using DiamondShop.Domain.Models.Diamonds;
using DiamondShop.Domain.Models.Diamonds.ValueObjects;
using DiamondShop.Domain.Models.DiamondShapes;
using DiamondShop.Domain.Models.Promotions;
using DiamondShop.Domain.Models.Promotions.Entities;
using DiamondShop.Domain.Models.Jewelries.ValueObjects;

namespace DiamondShop.Domain.Repositories
{
    public interface IDiamondRepository : IBaseRepository<Diamond>
    {
        Task<(Diamond diamond,List<Discount> discounts, List<Promotion> promotion)> GetByIdIncludeDiscountAndPromotion(DiamondId id, CancellationToken cancellationToken = default);
        public void UpdateRange(List<Diamond> diamonds);
    }
}
