using DiamondShop.Application.Dtos.Requests.Orders;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Models.Orders.Enum;
using DiamondShop.Domain.Models.Orders.ValueObjects;
using DiamondShop.Domain.Repositories.OrderRepo;
using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.Orders.Commands.Preparing
{
    public record PreparingOrderCommand(string orderId) : IRequest<Result<Order>>;
    internal class PreparingOrderCommandHandler : IRequestHandler<PreparingOrderCommand, Result<Order>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PreparingOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, IOrderItemRepository orderItemRepository)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _orderItemRepository = orderItemRepository;
        }

        public async Task<Result<Order>> Handle(PreparingOrderCommand request, CancellationToken token)
        {
            request.Deconstruct(out string orderId);
            await _unitOfWork.BeginTransactionAsync(token);
            var orderQuery = _orderRepository.GetQuery();
            var order = orderQuery.FirstOrDefault(p => p.Id == OrderId.Parse(orderId));
            if (order == null)
                return Result.Fail("No order found!");
            else if (order.Status != OrderStatus.Processing)
                return Result.Fail($"Order can only be prepared when it's {OrderStatus.Processing.ToString().ToLower()}!");

            order.Status = OrderStatus.Prepared;
            await _orderRepository.Update(order);

            var orderItemQuery = _orderItemRepository.GetQuery();
            orderItemQuery = _orderItemRepository.QueryFilter(orderItemQuery, p => p.OrderId == order.Id);
            var orderItems = orderItemQuery.ToList();
            orderItems.ForEach(p => { if (p.Status == OrderItemStatus.Preparing) p.Status = OrderItemStatus.Done; });
            _orderItemRepository.UpdateRange(orderItems);

            await _unitOfWork.SaveChangesAsync(token);
            await _unitOfWork.CommitAsync(token);
            //TODO: Add notification
            return Result.Ok(order);
        }
    }
}
