using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
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

namespace DiamondShop.Application.Usecases.Orders.Commands.Complete
{
    public record CompleteOrderCommand(string orderId, string staffId) : IRequest<Result<Order>>;
    internal class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand, Result<Order>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IDeliveryPackageRepository _deliveryPackageRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CompleteOrderCommandHandler(IOrderRepository orderRepository, IDeliveryPackageRepository deliveryPackageRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _deliveryPackageRepository = deliveryPackageRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Order>> Handle(CompleteOrderCommand request, CancellationToken token)
        {
            request.Deconstruct(out string orderId, out string staffId);
            await _unitOfWork.BeginTransactionAsync(token);

            var orderQuery = _orderRepository.GetQuery();
            orderQuery = _orderRepository.QueryInclude(orderQuery, p => p.Items);
            orderQuery = _orderRepository.QueryFilter(orderQuery, p => p.Id == OrderId.Parse(orderId));
            var order = orderQuery.FirstOrDefault();
            if (order == null)
                return Result.Fail("Can't find the selected order");
            if (order.Status != OrderStatus.Delivering || order.DeliveryPackageId == null)
                return Result.Fail("This order hasn't been delivering therefore can't be completed");

            var deliveryQuery = _deliveryPackageRepository.GetQuery();
            deliveryQuery = _deliveryPackageRepository.QueryFilter(deliveryQuery, p => p.Id == order.DeliveryPackageId && p.Status == DeliveryPackageStatus.Delivering);
            var delivery = deliveryQuery.FirstOrDefault();
            if (delivery == null)
                return Result.Fail("Can't find the delivering package");
            if (delivery.DelivererId != AccountId.Parse(staffId))
                return Result.Fail("Only the deliverer has the permission to complete this order");

            order.Status = OrderStatus.Success;
            await _orderRepository.Update(order);

            await _unitOfWork.SaveChangesAsync(token);
            await _unitOfWork.CommitAsync(token);
            return Result.Ok(order);
        }
    }
}
