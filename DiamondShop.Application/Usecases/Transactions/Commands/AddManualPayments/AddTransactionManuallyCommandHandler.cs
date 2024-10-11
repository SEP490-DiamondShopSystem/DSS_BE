using Azure.Core;
using DiamondShop.Application.Services.Data;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Application.Services.Models;
using DiamondShop.Commons;
using DiamondShop.Domain.Models.AccountAggregate;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Models.Orders.Enum;
using DiamondShop.Domain.Models.Orders.ValueObjects;
using DiamondShop.Domain.Models.Transactions;
using DiamondShop.Domain.Models.Transactions.Enum;
using DiamondShop.Domain.Repositories.OrderRepo;
using DiamondShop.Domain.Repositories.TransactionRepo;
using DiamondShop.Domain.Services.interfaces;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.Transactions.Commands.AddManualPayments
{
    public record AddTransactionManuallyCommand(string description, PaymentType PaymentType, string orderId) : IRequest<Result<Transaction>>;
    internal class AddTransactionManuallyCommandHandler : IRequestHandler<AddTransactionManuallyCommand, Result<Transaction>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IOrderTransactionService _orderTransactionService;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddTransactionManuallyCommandHandler(IHttpContextAccessor httpContextAccessor, ITransactionRepository transactionRepository, IOrderTransactionService orderTransactionService, IOrderRepository orderRepository, IPaymentMethodRepository paymentMethodRepository, IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _transactionRepository = transactionRepository;
            _orderTransactionService = orderTransactionService;
            _orderRepository = orderRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Transaction>> Handle(AddTransactionManuallyCommand request, CancellationToken cancellationToken)
        {
            var orderId = OrderId.Parse(request.orderId);
            var tryGetOrder = await _orderRepository.GetById(orderId);
            if (tryGetOrder is null)
                return Result.Fail(new NotFoundError("Order not found"));
            Transaction newTrans;
            if (tryGetOrder.CustomizeRequestId == null)// normal order 
            {
                newTrans = CreateForNormalOrder(request, tryGetOrder, request.PaymentType);
            }
            else
            {
                newTrans = CreateForCustomOrder(request, tryGetOrder, request.PaymentType);
            }
            if (newTrans == null)
                return Result.Fail(new ConflictError("Failed to create transaction"));
            await _transactionRepository.Create(newTrans);
            await _unitOfWork.SaveChangesAsync();
            return Result.Ok(newTrans);
        }
        private Transaction CreateForNormalOrder(AddTransactionManuallyCommand request, Order order, PaymentType paymentType)
        {
            decimal amountToCreate = 0;
            switch (paymentType)
            {
                case PaymentType.Payall:
                    amountToCreate = _orderTransactionService.GetFullPaymentValueForOrder(order);
                    return Transaction.CreateManualPayment(order.Id, request.description, amountToCreate, TransactionType.Pay);
                case PaymentType.COD:
                    amountToCreate = _orderTransactionService.GetCODValueForOrder(order);
                    return Transaction.CreateManualPayment(order.Id, request.description, amountToCreate, TransactionType.Pay);
                default:
                    throw new Exception("unidentified payment Type");
            }
        }
        private Transaction CreateForCustomOrder(AddTransactionManuallyCommand request, Order order, PaymentType paymentType)
        {
            decimal amountToCreate = 0;
            switch (paymentType)
            {
                case PaymentType.Payall:
                    amountToCreate = _orderTransactionService.GetFullPaymentValueForOrder(order);
                    return Transaction.CreateManualPayment(order.Id, request.description, amountToCreate, TransactionType.Pay);
                case PaymentType.COD:
                    amountToCreate = _orderTransactionService.GetDepositValueForOrder(order);
                    return Transaction.CreateManualPayment(order.Id, request.description, amountToCreate, TransactionType.Pay);
                default:
                    throw new Exception("unidentified payment Type");
            }
        }
    }
}
