﻿using DiamondShop.Domain.Models.DeliveryFees.ValueObjects;
using DiamondShop.Domain.Models.DiamondPrices.ValueObjects;
using DiamondShop.Domain.Models.Diamonds.ValueObjects;
using DiamondShop.Domain.Models.DiamondShapes.ValueObjects;
using DiamondShop.Domain.Models.Jewelries.ValueObjects;
using DiamondShop.Domain.Models.JewelryModels.ValueObjects;
using DiamondShop.Domain.Models.Orders.ValueObjects;
using DiamondShop.Domain.Models.Promotions.ValueObjects;
using DiamondShop.Domain.Models.Transactions.ValueObjects;
using Mapster;

namespace DiamondShop.Application.Mappers
{
    public class GeneralTypeMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Dictionary<SideDiamondReqId, SideDiamondOptId>, Dictionary<string, string>>()
                .MapWith( src => src.ToDictionary(kvp => kvp.Key.Value.ToString(), kvp => kvp.Value.Value.ToString()));
            config.NewConfig<DiamondId, string>()
               .MapWith(src => src.Value);
            config.NewConfig<DiamondCriteriaId, string>()
               .MapWith(src => src.Value);
            config.NewConfig<DiamondShapeId, string>()
               .MapWith(src => src.Value);
           
            config.NewConfig<PromotionId, string>()
               .MapWith(src => src.Value);
            config.NewConfig<PromoReqId, string>()
               .MapWith(src => src.Value);
            config.NewConfig<GiftId, string>()
               .MapWith(src => src.Value);
            config.NewConfig<DiscountId, string>()
                   .MapWith(src => src.Value);

            config.NewConfig<JewelryId, string>()
               .MapWith(src => src.Value).Compile();
            config.NewConfig<JewelrySideDiamondId, string>()
               .MapWith(src => src.Value).Compile();

            config.NewConfig<JewelryModelId, string>()
               .MapWith(src => src.Value).Compile();
            config.NewConfig<JewelryModelCategoryId, string>()
                .MapWith(src => src.Value).Compile();
            config.NewConfig<MainDiamondReqId, string>()
                .MapWith(src => src.Value).Compile();
            config.NewConfig<SideDiamondReqId, string>()
                .MapWith(src => src.Value).Compile();
            config.NewConfig<SideDiamondOptId, string>()
                .MapWith(src => src.Value).Compile();

            config.NewConfig<SizeId, string>()
               .MapWith(src => src.Value).Compile();

            config.NewConfig<MetalId, string>()
               .MapWith(src => src.Value).Compile();

            config.NewConfig<OrderId, string>()
                .MapWith(src => src.Value).Compile();
            config.NewConfig<OrderItemId, string>()
                .MapWith(src => src.Value).Compile();
            config.NewConfig<OrderLogId, string>()
                .MapWith(src => src.Value).Compile();
            config.NewConfig<DeliveryPackageId, string>()
             .MapWith(src => src.Value).Compile();
            config.NewConfig<DeliveryFeeId, string>()
                .MapWith(src => src.Value).Compile();
            config.NewConfig<TransactionId, string>()
                .MapWith(src => src.Value).Compile();
            config.NewConfig<PaymentMethodId, string>()
                .MapWith(src => src.Value).Compile();

        }
    }
}
