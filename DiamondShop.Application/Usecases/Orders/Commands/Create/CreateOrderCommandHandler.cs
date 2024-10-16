﻿using DiamondShop.Application.Dtos.Requests.Orders;
using DiamondShop.Application.Services.Data;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Application.Services.Models;
using DiamondShop.Application.Usecases.Orders.Commands.Validate;
using DiamondShop.Domain.Common.Carts;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Models.Diamonds;
using DiamondShop.Domain.Models.Diamonds.ValueObjects;
using DiamondShop.Domain.Models.Jewelries;
using DiamondShop.Domain.Models.Jewelries.ValueObjects;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Models.Orders.Entities;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Repositories.JewelryModelRepo;
using DiamondShop.Domain.Repositories.JewelryRepo;
using DiamondShop.Domain.Repositories.OrderRepo;
using DiamondShop.Domain.Repositories.TransactionRepo;
using DiamondShop.Domain.Services.interfaces;
using FluentResults;
using MediatR;

namespace DiamondShop.Application.Usecases.Orders.Commands.Create
{
    public record CreateOrderCommand(OrderRequestDto OrderRequestDto, List<OrderItemRequestDto> OrderItemRequestDtos, string Phone, string Address) : IRequest<Result<PaymentLinkResponse>>;
    internal class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<PaymentLinkResponse>>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IDiamondRepository _diamondRepository;
        private readonly IJewelryRepository _jewelryRepository;
        private readonly IMainDiamondRepository _mainDiamondRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMainDiamondService _mainDiamondService;
        private readonly IPaymentService _paymentService;
        private readonly ICartModelService _cartModelService;
        private readonly ISender _sender;

        public CreateOrderCommandHandler(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, IAccountRepository accountRepository, IUnitOfWork unitOfWork, IPaymentService paymentService, IJewelryRepository jewelryRepository, IMainDiamondRepository mainDiamondRepository, ISender sender, IDiamondRepository diamondRepository, IMainDiamondService mainDiamondService, IPaymentMethodRepository paymentMethodRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            _jewelryRepository = jewelryRepository;
            _mainDiamondRepository = mainDiamondRepository;
            _sender = sender;
            _diamondRepository = diamondRepository;
            _mainDiamondService = mainDiamondService;
            _paymentMethodRepository = paymentMethodRepository;
        }

        public async Task<Result<PaymentLinkResponse>> Handle(CreateOrderCommand request, CancellationToken token)
        {
            await _unitOfWork.BeginTransactionAsync(token);
            request.Deconstruct(out OrderRequestDto orderReq, out List<OrderItemRequestDto> orderItemReqs, out string phone, out string address);
            var account = await _accountRepository.GetById(AccountId.Parse(orderReq.accountId));
            //Validate account status


            List<CartProduct> products = new();
            List<Jewelry> jewelries = new();
            List<Diamond> diamonds = new();
            foreach (var item in orderItemReqs)
            {

                CartProduct cartProduct = new();
                if (item.jewelryId != null)
                {
                    cartProduct.Jewelry = await _jewelryRepository.GetById(JewelryId.Parse(item.jewelryId));
                    cartProduct.EngravedFont = item.engravedFont;
                    cartProduct.EngravedText = item.engravedText;
                    jewelries.Add(cartProduct.Jewelry);
                }
                if (item.diamondId != null)
                {
                    cartProduct.Diamond = await _diamondRepository.GetById(DiamondId.Parse(item.diamondId));
                    cartProduct.Diamond.JewelryId = cartProduct.Jewelry?.Id;
                    diamonds.Add(cartProduct.Diamond);
                }
                products.Add(cartProduct);
            }

            //Validate matching diamond
            List<IError> matchingErrors = new();
            foreach (var jewelry in jewelries)
            {
                var attachedDiamond = diamonds.Where(p => p.JewelryId == jewelry.Id).ToList();
                var result = await _mainDiamondService.CheckMatchingDiamond(jewelry.ModelId, attachedDiamond, _mainDiamondRepository);
                if (result.IsFailed) matchingErrors.AddRange(result.Errors);
            }
            if (matchingErrors.Count > 0)
                return Result.Fail(matchingErrors);

            //Validate CartModel
            var cartModelResult = await _sender.Send(new ValidateOrderCommand(products));
            if (cartModelResult.IsFailed)
                return Result.Fail(cartModelResult.Errors);

            jewelries.ForEach(p => p.SetSold());
            _jewelryRepository.UpdateRange(jewelries);

            diamonds.ForEach(p => p.SetSold());
            _diamondRepository.UpdateRange(diamonds);

            var cartModel = cartModelResult.Value;
            var orderPromo = cartModel.Promotion.Promotion;

            var order = Order.Create(AccountId.Parse(orderReq.accountId), orderReq.paymentType, cartModel.OrderPrices.FinalPrice, cartModel.ShippingPrice.FinalPrice, address, orderPromo?.Id);
            await _orderRepository.Create(order, token);
            await _unitOfWork.SaveChangesAsync(token);

            List<OrderItem> orderItems = cartModel.Products.Select(p =>
            {
                string giftedId = p.Diamond?.Id?.Value ?? p.Jewelry?.Id?.Value;
                var gift = giftedId is null ? null : orderPromo?.Gifts.FirstOrDefault(k => k.ItemId == giftedId);
                return OrderItem.Create(order.Id, p.Jewelry?.Id, p.Diamond?.Id,
                p.EngravedText, p.EngravedFont, p.ReviewPrice.FinalPrice,
                p.DiscountId, p.DiscountPercent,
                gift?.UnitType, gift?.UnitValue);
            }).ToList();
            await _orderItemRepository.CreateRange(orderItems);
            await _unitOfWork.SaveChangesAsync(token);

            await _unitOfWork.CommitAsync(token);

            //Create Paymentlink if not transfer
            if (orderReq.isTransfer) return new PaymentLinkResponse() { };
            string title = "";
            string callbackURL = "";
            string returnURL = "";
            string description = "";
            PaymentLinkRequest paymentLinkRequest = new PaymentLinkRequest()
            {
                Account = account,
                Order = order,
                Email = account.Email,
                Phone = phone,
                Address = address,
                Title = title,
                Description = description,
                Amount = order.TotalPrice,
            };
            return await _paymentService.CreatePaymentLink(paymentLinkRequest, token);
        }

    }
}