﻿using DiamondShop.Domain.Models.Diamonds.Enums;
using DiamondShop.Domain.Models.DiamondShapes.ValueObjects;
using DiamondShop.Domain.Models.DiamondShapes;
using DiamondShop.Domain.Models.Jewelries.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.DiamondPrices;
using DiamondShop.Domain.Models.Promotions.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using DiamondShop.Application.Dtos.Responses.Promotions;
using DiamondShop.Domain.Common.Enums;
using DiamondShop.Domain.Models.CustomizeRequests;
using DiamondShop.Application.Dtos.Responses.CustomizeRequests;
using DiamondShop.Application.Dtos.Responses.Jewelries;

namespace DiamondShop.Application.Dtos.Responses.Diamonds
{
    public class DiamondDto
    {
        public string Id { get; set; }
        public string? JewelryId { get; set; }
        public string DiamondShapeId { get; set; }
        public DiamondShapeDto DiamondShape { get; set; }
       // public DiamondWarrantyDto? Warranty { get; set; }
        /*public List<DiamondMedia> Medias { get; set;} = new();*/
        public Clarity Clarity { get; set; }
        public Color Color { get; set; }
        public Cut? Cut { get; set; }
        public decimal PriceOffset { get; set; }
        public float Carat { get; set; }
        public bool IsLabDiamond { get; set; }
        public float WidthLengthRatio { get; set; }
        public float Depth { get; set; }
        public float Table { get; set; }
        public Polish Polish { get; set; }
        public Symmetry Symmetry { get; set; }
        public Girdle Girdle { get; set; }
        public Culet Culet { get; set; }
        public Fluorescence Fluorescence { get; set; }
        public string Measurement { get; set; }
        public DiamondPriceDto? DiamondPrice { get; set; }
        public MediaDto? Thumbnail { get; set; }
        public List<MediaDto>? Gallery { get; set; }
        public ProductStatus Status { get; set; }
        public decimal? SoldPrice { get; set; }
        public decimal? DefaultPrice { get; set; }
        public DiscountDto? Discount { get; set; }
        public ProductLockDto? ProductLock { get; set; }
        public decimal TruePrice { get; set; }
        //public decimal? DiscountPrice { get; set; }
        public decimal DiscountReducedAmount { get; set; } 
        public decimal PromotionReducedAmount { get; set; } 
        public string? SerialCode { get; set; }
        public decimal? SalePrice { get; set; }
        public string Title { get; set; }
        public decimal CutOffsetFounded { get; set; }
        public DiamondRequestDto? DiamondRequest { get; set; }
        public JewelryDto? Jewelry { get; set; }
        public bool IsLockForJewelry { get => JewelryId != null; }
        public bool IsLockForCustomizeRequest { get => DiamondRequest != null; }
    }
}
