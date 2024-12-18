﻿using DiamondShop.Domain.BusinessRules;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Common.Enums;
using DiamondShop.Domain.Common.Products;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.AccountAggregate;
using DiamondShop.Domain.Models.Diamonds;
using DiamondShop.Domain.Models.Jewelries.Entities;
using DiamondShop.Domain.Models.Jewelries.ErrorMessages;
using DiamondShop.Domain.Models.Jewelries.ValueObjects;
using DiamondShop.Domain.Models.JewelryModels;
using DiamondShop.Domain.Models.JewelryModels.Entities;
using DiamondShop.Domain.Models.JewelryModels.ValueObjects;
using DiamondShop.Domain.Models.Promotions;
using DiamondShop.Domain.Models.Promotions.Entities;
using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiamondShop.Domain.Models.Jewelries
{
    public class Jewelry : Entity<JewelryId>, IAggregateRoot
    {
        public static ProductStatus[] UnLockableState = new ProductStatus[] { ProductStatus.Sold, ProductStatus.Inactive, ProductStatus.PreOrder };
        public JewelryModelId ModelId { get; set; }
        public JewelryModel Model { get; set; }
        public SizeId SizeId { get; set; }
        public Size Size { get; set; }
        public MetalId MetalId { get; set; }
        public Metal Metal { get; set; }
        public float Weight { get; set; }
        public string SerialCode { get; set; }
        [NotMapped] // this is not sold price, this is just price from diamond and jewelry, no discount or promotion is applied
        public decimal TotalPrice
        {
            get
            {
                var inalPrice = 0m;
                if (ND_Price is not null)
                    inalPrice += ND_Price.Value;
                if (D_Price is not null)
                    inalPrice += D_Price.Value;
                if (SD_Price is not null)
                    inalPrice += SD_Price.Value;
                return inalPrice;
            }
        }
        //[NotMapped]
        //public decimal DiscountPrice { get; set; }
        [NotMapped]
        public decimal DiscountReducedAmount { get; set; } = 0;
        [NotMapped]
        public Discount? Discount { get; set; }
        [NotMapped]
        public decimal PromotionReducedAmount { get; set; } = 0;

        [NotMapped]
        public decimal SalePrice { get => TotalPrice - DiscountReducedAmount; }

        [NotMapped]
        public bool IsAllSideDiamondPriceKnown { get; set; }

        [NotMapped]
        public bool IsAllDiamondPriceKnown
        {
            get
            {
                if (Diamonds.Count == 0)
                    return true;
                if (!(Diamonds.Any(d => d.IsPriceKnown) == false))
                    return true;
                D_Price = 0;
                return false;
            }
        }
        [NotMapped]
        public bool IsMetalPriceKnown { get => ND_Price > 0; }
        [NotMapped]
        public bool IsJewelryPriceKnown
        {
            get
            {
                return IsAllDiamondPriceKnown && IsMetalPriceKnown && IsAllSideDiamondPriceKnown;
            }
        }
        [NotMapped]
        public string? Title { get => GetTitle(); }
        public List<Diamond> Diamonds { get; set; } = new();
        public JewelrySideDiamond? SideDiamond { get; set; }
        public JewelryReview? Review { get; set; }
        public Media? Thumbnail { get; set; }

        public decimal? ND_Price { get; set; }
        public decimal? SD_Price { get; set; }
        public decimal? D_Price { get; set; }
        public decimal? SoldPrice { get; set; }
        public string? EngravedText { get; set; }
        public string? EngravedFont { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Active;
        public ProductLock? ProductLock { get; set; }
        //[Timestamp]
        //public byte[] Version { get; set; }
        private Jewelry() { }
        public static Jewelry Create(
           JewelryModelId modelId, SizeId sizeId, MetalId metalId,
           float weight, string serialCode, ProductStatus status, JewelryId givenId = null)
        {
            return new Jewelry()
            {
                Id = givenId is null ? JewelryId.Create() : givenId,
                ModelId = modelId,
                SizeId = sizeId,
                MetalId = metalId,
                Weight = weight,
                SerialCode = serialCode,
                Status = status,
            };
        }
        public static Jewelry Create(
            JewelryModelId modelId, SizeId sizeId, MetalId metalId,
            float weight, string serialCode, ProductStatus status, JewelrySideDiamond? sideDiamond, JewelryId givenId = null)
        {
            return new Jewelry()
            {
                Id = givenId is null ? JewelryId.Create() : givenId,
                ModelId = modelId,
                SizeId = sizeId,
                MetalId = metalId,
                Weight = weight,
                SerialCode = serialCode,
                SideDiamond = sideDiamond,
                Status = status,
            };
        }
        public void SetSoldUnavailable(decimal soldPrice, string? engravedText, string? engravedFont)
        {
            //ND_Price = noDiamondPrice;
            //D_Price = soldPrice - noDiamondPrice;
            SoldPrice = soldPrice;
            EngravedText = engravedText;
            EngravedFont = engravedFont;
            Diamonds.ForEach(p =>
            {
                if (p.Status == ProductStatus.PreOrder)
                {
                    p.SetSoldPreOrder(p.TruePrice, p.TruePrice);
                    return;
                }
                if (p.TruePrice != null)
                    p.SetSold(p.TruePrice, p.TruePrice);
            });
        }
        public void SetSold()
        {
            if (Status == ProductStatus.PreOrder)
                Status = ProductStatus.Sold;
        }
        public void SetSold(decimal soldPrice, string? engravedText, string? engravedFont)
        {

            Status = ProductStatus.Sold;
            //khúc này không cần, trong cartModel đã tính rồi, đã set giá cho ND , D  và SD
            //Giá thằng đó là giá default, là giá gốc, còn sold price là giá bán
            // lý do ko tính mấy thằng kia theo giá bán do giá gốc nó mới chuẩn
            // Giá gốc = ND + D + SD rồi mới bắt đầu tính discount, promo, nên để nguyên v, lúc
            // lấy sản phẩm ra xem thì cũng xem tổng giá với giá ban đầu
            //ND_Price = noDiamondPrice;
            //D_Price = soldPrice - noDiamondPrice;
            SoldPrice = soldPrice;
            EngravedText = engravedText;
            EngravedFont = engravedFont;
            Diamonds.ForEach(p =>
            {
                if (p.TruePrice != null)
                    p.SetSold(p.TruePrice, p.TruePrice);
            });
        }

        public void SetSell()
        {
            Status = ProductStatus.Active;
            ND_Price = null;
            D_Price = null;
            SD_Price = null;
            SoldPrice = null;
            EngravedFont = null;
            EngravedText = null;
            foreach (var diamond in Diamonds)
            {
                diamond.SetForJewelry(this);
            }
        }
        public void SetInactive()
        {
            if (Status == ProductStatus.LockForUser)
                RemoveLock();
            Status = ProductStatus.Inactive;
            ND_Price = null;
            D_Price = null;
            SD_Price = null;
            SoldPrice = null;
            EngravedFont = null;
            EngravedText = null;
        }
        public void SetLockForUser(Account? userAccount, int lockHour, bool isUnlock, JewelryRules rules)
        {
            if (isUnlock)
            {
                RemoveLock();
                return;
            }
            if (Status == ProductStatus.Sold)
                throw new Exception("Can not lock a sold product");
            if (lockHour > rules.MaxLockHours || lockHour < 1)
                throw new Exception("thoi gian lock san pham toi da la  " + rules.MaxLockHours + " va toi thieu la 1");
            Status = ProductStatus.LockForUser;
            if (userAccount is not null)
                ProductLock = ProductLock.CreateLockForUser(userAccount.Id, TimeSpan.FromHours(lockHour));
            else
                ProductLock = ProductLock.CreateLock(TimeSpan.FromHours(lockHour));

        }
        public void RemoveLock()
        {
            if (ProductLock is null)
                throw new Exception("This product is not locked");
            Status = ProductStatus.Active;
            ProductLock = null;
            foreach (var diamond in Diamonds)
            {
                diamond.RemoveLock();
            }
        }
        public void SetTotalDiamondPrice(decimal totalAllDiamondPrice)
        {
            D_Price = totalAllDiamondPrice;
        }
        public void AssignJewelryDiscount(Discount discount)
        {
            var reducedAmountRaw = TotalPrice * ((decimal)discount.DiscountPercent / (decimal)100);
            var correctReducedAmount = MoneyVndRoundUpRules.RoundAmountFromDecimal(reducedAmountRaw);
            DiscountReducedAmount = correctReducedAmount;
            Discount = discount;
        }
        public void AssignJewelryPromotion(Promotion promotion, decimal reducedAmount)
        {
            PromotionReducedAmount = reducedAmount;
        }
        public Result CanBeLock()
        {
            if (UnLockableState.Contains(this.Status))
                return Result.Fail(JewelryErrors.InCorrectState("không thể khóa sản phẩm này"));
            return Result.Ok();
        }
        public string GetTitle()
        {
            string title = "trang sức mã #" + SerialCode;
            if (Model != null)
            {
                title = $"{Model.Name} ";
                if(Metal != null)
                    title += $"in {Metal.Name} ";
                
                if (SideDiamond != null)
                    title += $"({SideDiamond.Carat} Tw)";
            }
            return title;
        }
    }
}
