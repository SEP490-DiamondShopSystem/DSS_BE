using DiamondShop.Application.Dtos.Requests.Orders;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Models.Diamonds;
using DiamondShop.Domain.Models.Jewelries;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Models.Orders.Enum;
using DiamondShop.Domain.Models.Orders.ValueObjects;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Repositories.JewelryRepo;
using DiamondShop.Domain.Repositories.OrderRepo;
using DiamondShop.Domain.Repositories.TransactionRepo;
using DiamondShop.Domain.Services.interfaces;
using FluentResults;
using MediatR;

namespace DiamondShop.Api.Controllers.Orders.Cancel
{
    public record CancelOrderCommand(string OrderId, string AccountId, bool ByShop) : IRequest<Result<Order>>;
    public record CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<Order>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IJewelryRepository _jewelryRepository;
        private readonly IDiamondRepository _diamondRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public CancelOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, IOrderService orderService, IJewelryRepository jewelryRepository, IDiamondRepository diamondRepository, ITransactionRepository transactionRepository)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _orderService = orderService;
            _jewelryRepository = jewelryRepository;
            _diamondRepository = diamondRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<Result<Order>> Handle(CancelOrderCommand request, CancellationToken token)
        {
            request.Deconstruct(out string orderId, out string accountId, out bool byShop);
            await _unitOfWork.BeginTransactionAsync();
            var orderQuery = _orderRepository.GetQuery();
            var order = orderQuery.FirstOrDefault(p => p.Id == OrderId.Parse(orderId));
            if (order == null)
                return Result.Fail("No order found!");
            else if (!_orderService.IsCancellable(order.Status))
                return Result.Fail("This order can't be cancelled anymore!");
            if (byShop)
            {
                order.Status = OrderStatus.Rejected;
            }
            else
            {
                if (order.AccountId != AccountId.Parse(accountId))
                    return Result.Fail("You're not allowed to cancel this order");
                order.Status = OrderStatus.Cancelled;
            }
            //TODO: Add Refund

            //Return to selling
            var orderItemQuery = _orderItemRepository.GetQuery();
            orderItemQuery = _orderItemRepository.QueryInclude(orderItemQuery, p => p.Jewelry);
            orderItemQuery = _orderItemRepository.QueryInclude(orderItemQuery, p => p.Diamond);
            orderItemQuery = _orderItemRepository.QueryFilter(orderItemQuery, p => p.OrderId == order.Id);
            var items = orderItemQuery.ToList();
            HashSet<Jewelry> jewelrySet = new();
            HashSet<Diamond> diamondSet = new();
            items.ForEach(p =>
            {
                if (p.Status != OrderItemStatus.Removed)
                {
                    p.Status = OrderItemStatus.Removed;
                    if (p.Jewelry != null) jewelrySet.Add(p.Jewelry);
                    if (p.Diamond != null) diamondSet.Add(p.Diamond);
                }
            });
            var jewelries = jewelrySet.ToList();
            jewelries.ForEach(p => p.SetSell());

            var diamonds = diamondSet.ToList();
            diamonds.ForEach(p => p.SetSell());

            _orderItemRepository.UpdateRange(items);
            _jewelryRepository.UpdateRange(jewelries);
            _diamondRepository.UpdateRange(diamonds);
            await _unitOfWork.SaveChangesAsync();



            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return Result.Ok(order);
        }
    }
}
