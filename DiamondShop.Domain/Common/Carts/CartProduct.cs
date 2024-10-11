using DiamondShop.Domain.Models.Diamonds;
using DiamondShop.Domain.Models.Jewelries;
using DiamondShop.Domain.Models.JewelryModels;
using DiamondShop.Domain.Models.Promotions.Entities;
using DiamondShop.Domain.Models.Promotions.ValueObjects;

namespace DiamondShop.Domain.Common.Carts
{
    public class CartProduct
    {
        public string CartProductId { get; set; } = DateTime.UtcNow.Ticks.ToString();
        public Jewelry? Jewelry { get; set; }
        public Diamond? Diamond { get; set; }
        public JewelryModel? JewelryModel { get; set; }
        public CheckoutPrice ReviewPrice { get; set; } = new();
        public string? EngravedText { get; set; }
        public string? EngravedFont { get; set; }
        //public decimal? PurchasedPrice { get; set; }
        public bool IsValid { get => (Jewelry != null || Diamond != null || JewelryModel != null); }
        public bool IsAvailable { get; set; } = true; // always true unless when check the product is sold or inactive
        /////////////////////////////////
        /////////////////////////////////
        public DiscountId? DiscountId { get; set; }
        public int? DiscountPercent { get; set; }
        public bool IsHavingDiscount { get => DiscountId is not null; }
        
        public PromotionId? PromotionId { get; set; }
        public bool IsHavingPromotion { get => PromotionId is not null; }
        public PromoReqId? RequirementQualifedId{ get; set; }
        public bool IsReqirement { get => RequirementQualifedId is not null; }
        public GiftId? GiftAssignedId { get; set; }
        public bool IsGift { get => GiftAssignedId is not null; }
    }

}
