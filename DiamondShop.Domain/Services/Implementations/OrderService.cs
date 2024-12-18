﻿using DiamondShop.Domain.Common.Enums;
using DiamondShop.Domain.Models.AccountAggregate.ErrorMessages;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Models.Diamonds;
using DiamondShop.Domain.Models.Jewelries;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Models.Orders.Enum;
using DiamondShop.Domain.Models.Orders.ErrorMessages;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Models.Warranties.Enum;
using DiamondShop.Domain.Models.Warranties.ErrorMessages;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Repositories.JewelryRepo;
using DiamondShop.Domain.Repositories.OrderRepo;
using DiamondShop.Domain.Services.interfaces;
using FluentResults;

namespace DiamondShop.Domain.Services.Implementations
{
    public class OrderService : IOrderService
    {
        List<OrderStatus> cancellableState = new() {
            OrderStatus.Pending,
            OrderStatus.Processing,
            OrderStatus.Prepared,
            OrderStatus.Delivery_Failed,
        };
        List<OrderStatus> ongoingState = new() {
            OrderStatus.Processing,
            OrderStatus.Prepared,
            OrderStatus.Delivering,
        };
        List<OrderStatus> deliveringState = new() {
            OrderStatus.Delivery_Failed,
        };
        private readonly IAccountRepository _accountRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IJewelryRepository _jewelryRepository;
        private readonly IDiamondRepository _diamondRepository;

        public OrderService(IAccountRepository accountRepository, IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, IJewelryRepository jewelryRepository, IDiamondRepository diamondRepository)
        {
            _accountRepository = accountRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _jewelryRepository = jewelryRepository;
            _diamondRepository = diamondRepository;
        }

        public bool CheckForSameCity(List<Order> orders)
        {
            //TODO: MAKE IT
            return true;
        }

        public bool IsCancellable(OrderStatus status)
        {
            return cancellableState.Contains(status);
        }
        public bool IsProceedable(OrderStatus status)
        {
            return ongoingState.Contains(status);
        }
        public async Task<Result> CancelItems(Order order)
        {
            var orderItemQuery = _orderItemRepository.GetQuery();
            orderItemQuery = _orderItemRepository.QueryFilter(orderItemQuery, p => p.OrderId == order.Id);
            var items = orderItemQuery.ToList();
            List<IError> errors = new();
            List<Jewelry> jewelries = new List<Jewelry>();
            List<Diamond> diamonds = new List<Diamond>();
            foreach (var item in items)
            {
                // major error here
                //item.SetCancel();
                if (item.JewelryId != null)
                {
                    var jewelry = await _jewelryRepository.GetById(item.JewelryId);
                    if (jewelry.Status == ProductStatus.PreOrder)
                    {
                        await _jewelryRepository.Delete(jewelry);
                        if (jewelry.Diamonds != null)
                        {
                            foreach (var diamond in jewelry.Diamonds)
                            {
                                if (diamond.Status == ProductStatus.PreOrder)
                                {
                                    await _diamondRepository.Delete(diamond);
                                }
                                else
                                {
                                    diamond.SetSell();
                                    diamonds.Add(diamond);
                                    if(order.IsCustomOrder)
                                        await _diamondRepository.RemoveDiamondFromAllDiamondRequest(diamond);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (jewelry.Diamonds != null)
                        {
                            foreach (var diamond in jewelry.Diamonds)
                            {
                                diamond.SetForJewelry(jewelry);
                                diamonds.Add(diamond);
                                if (order.IsCustomOrder)
                                    await _diamondRepository.RemoveDiamondFromAllDiamondRequest(diamond);
                            }
                        }
                        jewelry.SetSell();
                        jewelries.Add(jewelry);
                    }
                }
                if (item.DiamondId != null)
                {
                    var diamond = await _diamondRepository.GetById(item.DiamondId);
                    if (diamond == null)
                        errors.Append(new Error($"Can't find diamond #{item.DiamondId}"));
                    else
                    {
                        diamond.SetSell();
                        diamonds.Add(diamond);
                    }
                }
                item.SetCancel();
            }
            if (errors.Count > 0)
                return Result.Fail(errors);
            _orderItemRepository.UpdateRange(items);
            _jewelryRepository.UpdateRange(jewelries);
            _diamondRepository.UpdateRange(diamonds);
            return Result.Ok();
        }

        public Result CheckWarranty(string? jewelryId, string? diamondId, WarrantyType warrantyType)
        {
            if (jewelryId != null && diamondId == null)
            {
                if (warrantyType != WarrantyType.Jewelry)
                {
                    return Result.Fail(WarrantyErrors.WrongJewelryError);
                }
            }
            else if (diamondId != null)
            {
                if (warrantyType != WarrantyType.Diamond)
                {
                    return Result.Fail(WarrantyErrors.WrongDiamondError);
                }
            }
            return Result.Ok();
        }
        public async Task<Result<Order>> AssignDeliverer(Order order, string delivererId)
        {
            var account = await _accountRepository.GetById(AccountId.Parse(delivererId));
            if (account == null)
                return Result.Fail(AccountErrors.AccountNotFoundError);
            if (account.Roles.Any(p => p.Id != AccountRole.Deliverer.Id))
                return Result.Fail("Tài khoản không phải người giao hàng");
            var orderQuery = _orderRepository.GetQuery();
            var conflictedOrderFlag = await _orderRepository.GetDelivererCurrentlyHandledOrder(account);
            if (conflictedOrderFlag != null)
                return Result.Fail(OrderErrors.DelivererIsUnavailableError);
            order.DelivererId = account.Id;
            order.HasDelivererReturned = false;
            return order;
        }

        public bool IsDeliverCancellable(OrderStatus status)
        {
            return deliveringState.Contains(status);
        }
    }
}
