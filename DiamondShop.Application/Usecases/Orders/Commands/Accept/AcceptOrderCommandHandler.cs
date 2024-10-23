using DiamondShop.Application.Dtos.Requests.Orders;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Models.Orders.Enum;
using DiamondShop.Domain.Models.Orders.ValueObjects;
using DiamondShop.Domain.Models.Transactions;
using DiamondShop.Domain.Models.Transactions.Enum;
using DiamondShop.Domain.Repositories.OrderRepo;
using DiamondShop.Domain.Repositories.TransactionRepo;
using FluentResults;
using MediatR;

namespace DiamondShop.Application.Usecases.Orders.Commands.Accept
{
    public record AcceptOrderCommand(string orderId) : IRequest<Result<Order>>;
    internal class AcceptOrderCommandHandler : IRequestHandler<AcceptOrderCommand, Result<Order>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        public AcceptOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, ITransactionRepository transactionRepository, IOrderItemRepository orderItemRepository)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _transactionRepository = transactionRepository;
            _orderItemRepository = orderItemRepository;
        }

        public async Task<Result<Order>> Handle(AcceptOrderCommand request, CancellationToken token)
        {
            request.Deconstruct(out string orderId);
            await _unitOfWork.BeginTransactionAsync(token);
            var orderQuery = _orderRepository.GetQuery();
            orderQuery = _orderRepository.QueryInclude(orderQuery, p => p.Account);
            var order = orderQuery.FirstOrDefault(p => p.Id == OrderId.Parse(orderId));
            if (order == null)
                return Result.Fail("No order found!");
            else if (order.Status != OrderStatus.Pending)
                return Result.Fail($"Order can only be accepted when it's {OrderStatus.Pending.ToString().ToLower()}!");
            
            //TODO: Add shipping price & fix transactioncode
            Transaction trans = Transaction.CreateManualPayment(order.Id, $"Transfer from {order.Account.FullName} for order #{order.Id}", order.TotalPrice, TransactionType.Pay);
            trans.AppTransactionCode = "";
            trans.PaygateTransactionCode = "";
            await _transactionRepository.Create(trans);
            await _unitOfWork.SaveChangesAsync(token);


            order.Status = OrderStatus.Processing;
            order.PaymentStatus = order.PaymentType == PaymentType.Payall ? PaymentStatus.PaidAll : PaymentStatus.Deposited;
            await _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync(token);

            var orderItemQuery = _orderItemRepository.GetQuery();
            orderItemQuery = _orderItemRepository.QueryFilter(orderItemQuery, p => p.OrderId == order.Id);
            var orderItems = orderItemQuery.ToList();
            orderItems.ForEach(p => p.Status = OrderItemStatus.Preparing);
            _orderItemRepository.UpdateRange(orderItems);

            await _unitOfWork.SaveChangesAsync(token);
            await _unitOfWork.CommitAsync(token);
            //TODO: Add notification
            return Result.Ok(order);
        }
    }
}
