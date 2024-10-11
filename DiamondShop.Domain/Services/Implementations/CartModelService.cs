using DiamondShop.Domain.Common.Carts;
using DiamondShop.Domain.Models.AccountAggregate.Entities;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Models.JewelryModels;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Repositories.JewelryModelRepo;
using DiamondShop.Domain.Repositories.JewelryRepo;
using DiamondShop.Domain.Repositories.PromotionsRepo;
using DiamondShop.Domain.Services.interfaces;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Services.Implementations
{
    public class CartModelService : ICartModelService
    {
        private readonly IDiamondServices _diamondServices;
        private readonly IPromotionServices _promotionServices;
        private readonly IDiscountService _discountService;
        private readonly IDiamondRepository _diamondRepository;
        private readonly IJewelryRepository _jewelryRepository;
        private readonly IJewelryModelRepository _jewelryModelRepository;
        private readonly IPromotionRepository _promotionRepository;
        private readonly IDiscountRepository _discountRepository;

        public CartModelService(IDiamondServices diamondServices, IPromotionServices promotionServices, IDiscountService discountService, IDiamondRepository diamondRepository, IJewelryRepository jewelryRepository, IJewelryModelRepository jewelryModelRepository, IPromotionRepository promotionRepository, IDiscountRepository discountRepository)
        {
            _diamondServices = diamondServices;
            _promotionServices = promotionServices;
            _discountService = discountService;
            _diamondRepository = diamondRepository;
            _jewelryRepository = jewelryRepository;
            _jewelryModelRepository = jewelryModelRepository;
            _promotionRepository = promotionRepository;
            _discountRepository = discountRepository;
        }

        public void AssignProductAndItemCounter(CartModel cartModel)
        {
            var products = cartModel.Products;
            cartModel.OrderCounter.TotalItem = products.Count;
            foreach (var product in products)
            {
                if (IsProduct(product))
                    cartModel.OrderCounter.TotalProduct += 1;
                else
                    cartModel.OrderCounter.TotalItem += 1;
            }
        }


        public async Task<Result<CartModel>> Execute(List<CartProduct> products, IDiscountRepository _discountRepository, IPromotionRepository _promotionRepository, IDiamondPriceRepository _diamondPriceRepository, ISizeMetalRepository _sizeMetalRepository, IMetalRepository _metalRepository)
        {
            ArgumentNullException.ThrowIfNull(products);
            var cartModel = new CartModel { Products = products };
            if (cartModel.Products.Count == 0)
                return Result.Ok(cartModel);
            AssignProductAndItemCounter(cartModel);
            cartModel.Products.ForEach(async prd => await AssignDefaultPriceToProduct(prd, _diamondPriceRepository, _sizeMetalRepository, _metalRepository));
            InitOrderPrice(cartModel);
            ValidateCartItems(cartModel).Wait();
            SetCartModelValidation(cartModel);
            var getDiscount = await _discountRepository.GetActiveDiscount();
            var getPromotion = await _promotionRepository.GetActivePromotion();
            foreach (var discount in getDiscount ) 
            {
                var result = _discountService.ApplyDiscountOnCartModel(cartModel,discount);
                if(result.IsSuccess) 
                {
                    //do nothing
                }
            }
            foreach(var promotion in getPromotion)
            {
                var result = _promotionServices.ApplyPromotionOnCartModel(cartModel, promotion);
                if (result.IsSuccess)
                {
                    break;// only one promotion is applied at a time
                }
            }
            return Result.Ok(cartModel);
        }

        public void InitOrderPrice(CartModel cartModel)
        {
            cartModel.OrderPrices.DefaultPrice = cartModel.Products.Sum(product => product.ReviewPrice.DefaultPrice);
        }

        public bool IsProduct(CartProduct item)
        {
            if (item.Jewelry != null && item.Diamond != null)// when diamond is child of jewelry then it has 2 field
                return false;
            if (item.Jewelry != null || item.JewelryModel != null)
                return true;
            return true;// if diamond only have its own id, then it is a main product
        }

        public void SetCartModelValidation(CartModel cartModel)
        {
            for (int i = 0; i < cartModel.Products.Count; i++)
            {
                var prod = cartModel.Products[i];
                if (prod.IsValid is false)
                {
                    if (IsProduct(prod))
                    {
                        cartModel.OrderCounter.TotalInvalidProduct += 1; // a product is also an item
                        cartModel.OrderCounter.TotalInvalidItem += 1;
                    }
                    else
                    {
                        cartModel.OrderCounter.TotalInvalidItem += 1;
                    }
                    cartModel.OrderValidation.InvalidItemIndex.Append(i);
                }
            }
        }
        public async Task<CartProduct?> FromCartItem(CartItem cartItem , IJewelryRepository _jewelryRepository, IJewelryModelRepository _jewelryModelRepository, IDiamondRepository _diamondRepository)
        {
            if (cartItem.JewelryId != null && cartItem.DiamondId != null && cartItem.JewelryModelId != null)
                throw new Exception("some how this cartItem have all 3 id, diamond, jewelry and model id, from cartItem id: " + cartItem.Id.Value);
            CartProduct cartProduct = new();
            
            if (cartItem.JewelryId is not null)
            {
                var jewelry = await _jewelryRepository.GetById(cartItem.JewelryId);
                cartProduct.Jewelry = jewelry;
            }
            if (cartItem.JewelryModelId is not null)
            {
                var jewelryModel = await _jewelryModelRepository.GetByIdMinimal(cartItem.JewelryModelId);
                cartProduct.JewelryModel = jewelryModel ;
            }
            if (cartItem.DiamondId is not null)
            {
                var diamond = await _diamondRepository.GetById(cartItem.DiamondId);
                cartProduct.Diamond = diamond ;
            }
            return cartProduct;
        }

        public async Task AssignDefaultPriceToProduct(CartProduct cartProduct, IDiamondPriceRepository _diamondPriceRepository, ISizeMetalRepository _sizeMetalRepository, IMetalRepository _metalRepository)
        {
            if (cartProduct.Jewelry != null && cartProduct.Diamond != null && cartProduct.JewelryModel != null)
                throw new Exception("some how this product have all 3 id, diamond, jewelry and model , from product: " + cartProduct.CartProductId);
            var reviewPrice = new CheckoutPrice();
            if (cartProduct.Jewelry is not null)
            {
                //TODO: assign default price to jewelry
            }
            if (cartProduct.JewelryModel is not null)
            {
                //TODO: assign default price to jewelry model
            }
            if (cartProduct.Diamond is not null)
            {
                var prices =  _diamondPriceRepository.GetPriceByShapes(cartProduct.Diamond.DiamondShape).Result;
                var diamondPrice = _diamondServices.GetDiamondPrice(cartProduct.Diamond, prices).Result;
                if (diamondPrice == null)
                    reviewPrice.DefaultPrice = 0;
                else
                    reviewPrice.DefaultPrice = diamondPrice.Price;

            }
            cartProduct.ReviewPrice = reviewPrice;
        }

        public Task ValidateCartItems(CartModel cartModel)
        {
            var products = cartModel.Products;
            foreach (var product in products)
            {
                if (product.Jewelry != null && product.Diamond != null && product.JewelryModel != null)
                    throw new Exception("some how this product have all 3 id, diamond, jewelry and model , from product: " + product.CartProductId);
                var result = CheckIsSoldOrActive(product);
                if (result.IsFailed)
                {
                    product.IsAvailable = false;
                }
            }
            int[] unavailableItemIndex = products.Where(p => p.IsAvailable is false).Select(p => products.IndexOf(p)).ToArray();
            cartModel.OrderValidation.UnavailableItemIndex = unavailableItemIndex;
            return Task.CompletedTask;
        }
        private Result CheckIsSoldOrActive(CartProduct product) 
        {
            if (product.Jewelry is not null)
                return product.Jewelry.IsSold ? Result.Fail("already sold") : Result.Ok();
            if (product.Diamond is not null)
            {
                if (product.Diamond.IsActive is false)
                    return Result.Fail("not active");
                return product.Diamond.IsSold ? Result.Fail("already sold") : Result.Ok();
            }
            if (product.JewelryModel is not null) { }
            return Result.Fail("unknonw product type");
                //return product.JewelryModel.
        }
    }
    
}
