﻿using DiamondShop.Application.Dtos.Requests.Carts;
using DiamondShop.Application.Dtos.Responses.Promotions;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Application.Services.Interfaces.Deliveries;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Common.Carts;
using DiamondShop.Domain.Models.AccountAggregate;
using DiamondShop.Domain.Models.AccountAggregate.Entities;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Models.Promotions;
using DiamondShop.Domain.Models.Promotions.ValueObjects;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Repositories.PromotionsRepo;
using DiamondShop.Domain.Services.Implementations;
using DiamondShop.Domain.Services.interfaces;
using FluentResults;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.Promotions.Queries.GetApplicablePromotions
{
    public record GetApplicablePromotionForCartQuery(CartRequestDto CartRequestDto): IRequest<Result<ApplicablePromotionDto>>;
    internal class GetApplicablePromotionForCartQueryHandler : IRequestHandler<GetApplicablePromotionForCartQuery, Result<ApplicablePromotionDto>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IDiscountRepository _discountRepository;
        private readonly IPromotionServices _promotionServices;
        private readonly ICartModelService _cartModelService;
        private readonly IDeliveryFeeServices _deliveryFeeServices;
        private readonly IMapper _mapper;
        private readonly IAccountRepository _accountRepository;
        private readonly IOptionsMonitor<ApplicationSettingGlobal> _optionsMonitor;
        private readonly ILocationService _locationService;

        public GetApplicablePromotionForCartQueryHandler(IPromotionRepository promotionRepository, IDiscountRepository discountRepository, IPromotionServices promotionServices, ICartModelService cartModelService, IDeliveryFeeServices deliveryFeeServices, IMapper mapper, IAccountRepository accountRepository, IOptionsMonitor<ApplicationSettingGlobal> optionsMonitor, ILocationService locationService)
        {
            _promotionRepository = promotionRepository;
            _discountRepository = discountRepository;
            _promotionServices = promotionServices;
            _cartModelService = cartModelService;
            _deliveryFeeServices = deliveryFeeServices;
            _mapper = mapper;
            _accountRepository = accountRepository;
            _optionsMonitor = optionsMonitor;
            _locationService = locationService;
        }

        public async Task<Result<ApplicablePromotionDto>> Handle(GetApplicablePromotionForCartQuery request, CancellationToken cancellationToken)
        {
            var promotionRule = _optionsMonitor.CurrentValue.PromotionRule;
            var getAllActivePromo = await _promotionRepository.GetActivePromotion();
            var emptyPromo = new List<Promotion>();
            Account? userAccount = null;
            if (request.CartRequestDto.AccountId != null)
                userAccount = await _accountRepository.GetById(AccountId.Parse(request.CartRequestDto.AccountId));
            // init response
            ApplicablePromotionDto response = new();
            getAllActivePromo.ForEach(x => response.Promotions.Add(new PromoResponse(0,x.Id.Value,_mapper.Map<PromotionDto>(x), false)));
            
            var cartItem = _mapper.Map<List<CartItem>>(request.CartRequestDto.Items);
            PromotionId promotionId = null;
            ShippingPrice getShippingPrice = new();
            var getDiscounts = await _discountRepository.GetActiveDiscount();
            List<CartProduct> getProducts = _cartModelService.GetCartProduct(cartItem).Result;//PrepareCartProduct(cartItem).Result;
            if (request.CartRequestDto.UserAddress != null)
            {
                getShippingPrice = _deliveryFeeServices.GetShippingPrice(request.CartRequestDto.UserAddress).Result;
            }
            if(request.CartRequestDto.IsAtShopOrder)
            {
                var shopLocation = _locationService.GetShopLocation();
                var createShopAddress = Address.Create(0, shopLocation.Province, shopLocation.District, shopLocation.Ward, shopLocation.Road, AccountId.Parse("0"), AddressId.Parse("0"));
                getShippingPrice = ShippingPrice.CreateDeliveryAtShop(createShopAddress);
            }    
            // this step is to get the promotion 
            Result<CartModel> result = await _cartModelService.ExecuteNormalOrder(getProducts, getDiscounts, emptyPromo, getShippingPrice, userAccount, _optionsMonitor.CurrentValue.CartModelRules);
            if (result.IsFailed)
                return response;
            CartModel cartModel = result.Value;
            //this step is to apply promotion from the clone of result.value cartmodel
            // a concurrent bag is needed
            var successfulPromotions = new ConcurrentBag<PromoResponse>();
            Parallel.ForEach(getAllActivePromo, (promo) => 
            {
                var clonedCart = CartModel.CloneCart(cartModel);
                if (clonedCart == null)
                    return;
                var result = PromotionService.IsCartMeetPromotionRequirent(clonedCart, promo, promotionRule);
                if(result.IsSuccess)
                {
                    successfulPromotions.Add(new PromoResponse(clonedCart.OrderPrices.OrderPriceExcludeShipAndWarranty,promo.Id.Value, _mapper.Map<PromotionDto>(promo), true));
                }
            });
            foreach (var item in response.Promotions)
            {
                var founded = successfulPromotions.FirstOrDefault(x => x.PromoId == item.PromoId);
                if (founded != null)
                {
                    item.IsApplicable = true;
                    item.AmountSaved = founded.AmountSaved;
                }
            }
            response.Promotions.OrderByDescending(x => x.AmountSaved).ToList();
            return response;
        }
    }
}
