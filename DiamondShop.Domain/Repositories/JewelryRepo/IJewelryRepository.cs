﻿using DiamondShop.Domain.Models.AccountAggregate;
using DiamondShop.Domain.Models.Diamonds;
using DiamondShop.Domain.Models.Jewelries;
using DiamondShop.Domain.Models.Jewelries.Entities;
using DiamondShop.Domain.Models.JewelryModels.Entities;
using DiamondShop.Domain.Models.JewelryModels.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Repositories.JewelryRepo
{
    public interface IJewelryRepository : IBaseRepository<Jewelry>
    {
        public void UpdateRange(List<Jewelry> jewelries);
        public Task<List<Jewelry>> GetJewelry(JewelryModelId jewelryModelId);
        public Task<List<Jewelry>> GetJewelry(JewelryModelId jewelryModelId, MetalId metalId);
        public IQueryable<SizeId> GetSizesInStock(JewelryModelId modelId, MetalId metalId,
            SideDiamondOpt sideDiamondOpt);
        public IQueryable<SizeId> GetSizesInStock(JewelryModelId modelId, MetalId metalId);
        public Task<List<Jewelry>> GetLatestSameModel(JewelryModelId jewelryModelId, MetalId metalId, SizeId sizeId);
        public Task<bool> Existing(JewelryModelId modelId);
        public Task<bool> Existing(JewelryModelId modelId, MetalId metalId, SizeId sizeId);
        public Task<bool> Existing(JewelryModelId modelId, SideDiamondOpt sideDiamondOpt);
        public Task<bool> CheckDuplicatedSerial(string serialCode);
        public Task<bool> IsHavingDiamond(Jewelry jewelry, CancellationToken cancellationToken = default);
        Task<List<Jewelry>> GetLockJewelry(CancellationToken cancellationToken = default);
        Task<List<Jewelry>> GetLockJewelryForUser(Account userAccount,CancellationToken cancellationToken = default);

    }
}
