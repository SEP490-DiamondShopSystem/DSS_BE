﻿using DiamondShop.Commons;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Models.Diamonds.Enums;
using DiamondShop.Domain.Models.DiamondShapes.ValueObjects;
using DiamondShop.Domain.Models.JewelryModels;
using DiamondShop.Domain.Models.Promotions.Enum;
using DiamondShop.Domain.Models.Promotions.ValueObjects;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Models.Promotions.Entities
{
    public class Gift : Entity<GiftId>
    {
        public PromotionId? PromotionId { get; set; }
        public Promotion? Promotion { get; set; }
        public string Name { get; set; }
        public TargetType TargetType { get; set; }
        public string? ItemCode { get; set; }
        public UnitType UnitType { get; set; }
        public decimal UnitValue { get; set; }
        public decimal? MaxAmout { get; set; } = null;
        public int Amount { get; set; }
        //diamond config
        public List<DiamondShapeId>? DiamondGiftShapes { get; set; } = new();
        public DiamondOrigin? DiamondOrigin { get; set; }
        public float? CaratFrom { get; set; } 
        public float? CaratTo { get; set; }
        public Clarity? ClarityFrom { get; set; }
        public Clarity? ClarityTo { get; set; }
        public Cut? CutFrom { get; set; }
        public Cut? CutTo { get; set; }
        public Color? ColorFrom { get; set; }
        public Color? ColorTo { get; set; }
        [NotMapped]
        public JewelryModel? GiftedModel { get; set; }
        public static Gift CreateJewelry(string name,string itemCode,UnitType unitType,decimal unitValue,int amount)
        {
            return new Gift()
            {
                Id = GiftId.Create(),
                Name = name,
                TargetType = TargetType.Jewelry_Model,
                ItemCode = itemCode,
                UnitType = unitType,
                UnitValue = SetUnitValue(unitType,unitValue),
                Amount = amount
            };
        }
        public static Gift CreateDiamond(string name, string? itemId, UnitType unitType, decimal unitValue, int amount,
            List<DiamondShapeId> diamondGiftShapes,
            DiamondOrigin? diamondOrigin,
            float? caratFrom,
            float? caratTo,
            Clarity? clarityFrom,
            Clarity? clarityTo,
            Cut? cutFrom,
            Cut? cutTo,
            Color? colorFrom,
            Color? colorTo)
        {
            return new Gift()
            {
                Id = GiftId.Create(),
                Name = name,
                TargetType = TargetType.Diamond,
                ItemCode = itemId  ,
                UnitType = unitType,
                UnitValue = SetUnitValue(unitType, unitValue),
                Amount = amount,
                DiamondGiftShapes = diamondGiftShapes,
                DiamondOrigin = diamondOrigin,
                CaratFrom = caratFrom,
                CaratTo = caratTo,
                ClarityFrom = clarityFrom,
                ClarityTo = clarityTo,
                CutFrom = cutFrom,
                CutTo = cutTo,
                ColorFrom = colorFrom,
                ColorTo = colorTo
            };
        }
        public static Gift CreateOrder(string name, UnitType unitType, decimal unitValue)
        {
            return new Gift()
            {
                Id = GiftId.Create(),
                Name = name,
                TargetType = TargetType.Order,
                ItemCode = null,
                UnitType = unitType,
                UnitValue = SetUnitValue(unitType, unitValue),
                Amount = 1,
            };
        }
        public void SetPromotion(Promotion promotion)
        {
            PromotionId = promotion.Id;
        }
        private static decimal SetUnitValue(UnitType unitType, decimal unitValue)
        {
            decimal trueUnitValue = unitType switch
            {
                UnitType.Percent => Math.Clamp(unitValue, 0, 100),
                UnitType.Fix_Price => Math.Clamp(unitValue, 0, 100000000),// max sale 100 tr
                //UnitType.Free_Gift => unitValue,
                _ => throw new Exception("un detected unit type for this one"),
            };
            return trueUnitValue;
        }
        public void SetMaxAmount(decimal maxAmount)
        {
            if (UnitType == UnitType.Fix_Price)
                throw new Exception("không thể set giá max cho lại gift là tiền, phần trăm mới được max price");
            if (maxAmount < 1000)
                throw new Exception("giá trị max phải lớn hơn 1000");
            MaxAmout = maxAmount;

        }
        public Gift() { }
    }
}
