﻿using DiamondShop.Domain.Common;
using DiamondShop.Domain.Models.DiamondPrices;
using DiamondShop.Domain.Models.Diamonds.Enums;
using DiamondShop.Domain.Models.DiamondShapes;
using DiamondShop.Domain.Models.DiamondShapes.ValueObjects;
using DiamondShop.Domain.Models.JewelryModels.Enum;
using DiamondShop.Domain.Models.JewelryModels.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Models.JewelryModels.Entities
{
    public class SideDiamondOpt : Entity<SideDiamondOptId>
    {
        public JewelryModelId ModelId { get; set; }
        public JewelryModel Model { get; set; }
        public DiamondShapeId ShapeId { get; set; }
        public DiamondShape Shape { get; set; }
        public Color ColorMin { get; set; }
        public Color ColorMax { get; set; }
        public Clarity ClarityMin { get; set; }
        public Clarity ClarityMax { get; set; }
        public SettingType SettingType { get; set; }
        //Total CaratWeight
        public float CaratWeight { get; set; }
        public int Quantity { get; set; }
        public bool IsLabGrown { get; set; }
        [NotMapped]
        public bool IsFancyShape { get => DiamondShape.IsFancyShape(ShapeId); }
        [NotMapped]
        public List<DiamondPrice> DiamondPrice { get; set; } = new();
        [NotMapped]
        public int TotalPriceMatched { get => DiamondPrice.Count; }
        //[NotMapped]
        //public DiamondPrice DiamondPriceFound { get; set; }
        [NotMapped]
        public decimal AveragePricePerCarat { get; set; } = 0;
        [NotMapped]
        public bool IsPriceKnown { get => TotalPrice > 0; }
        [NotMapped]
        public decimal TotalPrice { get => AveragePricePerCarat * (decimal)AverageCarat * Quantity; }
        [NotMapped]
        public float AverageCarat { get => (float)CaratWeight / (float)Quantity; }
        public SideDiamondOpt() { }
        public static SideDiamondOpt Create(JewelryModelId modelId, DiamondShapeId shapeId, Color colorMin, Color colorMax, Clarity clarityMin, Clarity clarityMax, SettingType settingType, float caratWeight, int quantity, bool isLabDiamond, SideDiamondOptId givenId = null)
        {
            return new SideDiamondOpt()
            {
                Id = givenId is null ? SideDiamondOptId.Create() : givenId,
                ModelId = modelId,
                ShapeId = shapeId,
                ColorMin = colorMin,
                ColorMax = colorMax,
                ClarityMin = clarityMin,
                IsLabGrown = isLabDiamond,
                ClarityMax = clarityMax,
                SettingType = settingType,
                CaratWeight = caratWeight,
                Quantity = quantity,
            };
        }
    }
}
