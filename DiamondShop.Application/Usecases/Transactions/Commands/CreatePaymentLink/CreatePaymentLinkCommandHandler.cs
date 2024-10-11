using DiamondShop.Application.Services.Data;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Application.Services.Models;
using DiamondShop.Commons;
using DiamondShop.Domain.Models.AccountAggregate;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Models.Orders.Enum;
using DiamondShop.Domain.Models.Orders.ValueObjects;
using DiamondShop.Domain.Models.Transactions.Entities;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Repositories.OrderRepo;
using DiamondShop.Domain.Repositories.TransactionRepo;
using DiamondShop.Domain.Services.interfaces;
using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.Transactions.Commands.CreatePaymentLink
{
    public record CreatePaymentLinkCommand(string userId, string orderId, string paymentMethodName, PaymentType PaymentType) : IRequest<Result<PaymentLinkResponse>>;
    internal class CreatePaymentLinkCommandHandler : IRequestHandler<CreatePaymentLinkCommand, Result<PaymentLinkResponse>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderTransactionService _orderTransactionService;
        private readonly IPaymentService _paymentService;

        public CreatePaymentLinkCommandHandler(ITransactionRepository transactionRepository, IPaymentMethodRepository paymentMethodRepository, IAccountRepository accountRepository, IOrderRepository orderRepository, IOrderTransactionService orderTransactionService, IPaymentService paymentService)
        {
            _transactionRepository = transactionRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _accountRepository = accountRepository;
            _orderRepository = orderRepository;
            _orderTransactionService = orderTransactionService;
            _paymentService = paymentService;
        }

        public async Task<Result<PaymentLinkResponse>> Handle(CreatePaymentLinkCommand request, CancellationToken cancellationToken)
        {
            var accId = AccountId.Parse(request.userId);
            var orderId = OrderId.Parse(request.orderId);
            var tryGetAcc = await _accountRepository.GetById(accId);
            if(tryGetAcc == null)
                return Result.Fail(new NotFoundError("Account not found"));
            var tryGetOrder = await _orderRepository.GetById(orderId);
            if(tryGetOrder == null)
                return Result.Fail(new NotFoundError("Order not found"));
            var getPaymentMethod = await _paymentMethodRepository.GetById(request.paymentMethodName);
            if(getPaymentMethod == null)
                return Result.Fail(new NotFoundError("Payment method not found"));
            if(tryGetOrder.CustomizeRequestId == null)
            {
                return await CreateForNormalOrder(tryGetAcc, tryGetOrder, request.PaymentType);   
            }
            else
            {
                return await CreateForCustomOrder(tryGetAcc, tryGetOrder, request.PaymentType);
            }
        }
        private async Task<Result<PaymentLinkResponse>> CreateForNormalOrder(Account account, Order order, PaymentType paymentType)
        {
            decimal amountToCreate = 0;
            switch (paymentType)
            {
                case PaymentType.Payall:
                    amountToCreate = _orderTransactionService.GetFullPaymentValueForOrder(order);
                    break;
                case PaymentType.COD:
                    amountToCreate = _orderTransactionService.GetCODValueForOrder(order);
                    break;
                default:
                    throw new Exception("unidentified payment Type");
            }
            if (amountToCreate == 0)
                Result.Fail(new ConflictError("error why order value is 0 "));
            PaymentLinkRequest request = new PaymentLinkRequest { Amount = amountToCreate, Account = account, Order = order, Email = account.Email, };
            Result<PaymentLinkResponse> paymentResponse =  await _paymentService.CreatePaymentLink(request);
            return paymentResponse;
        }
        private async Task<Result<PaymentLinkResponse>> CreateForCustomOrder(Account account, Order order, PaymentType paymentType)
        {
            decimal amountToCreate = 0;
            switch (paymentType)
            {
                case PaymentType.Payall:
                    amountToCreate = _orderTransactionService.GetFullPaymentValueForOrder(order);
                    break;
                case PaymentType.COD:
                    amountToCreate = _orderTransactionService.GetDepositValueForOrder(order);
                    break;
                default:
                    throw new Exception("unidentified payment Type");
            }
            if (amountToCreate == 0)
                Result.Fail(new ConflictError("error why order value is 0 "));
            PaymentLinkRequest request = new PaymentLinkRequest { Amount = amountToCreate, Account = account, Order = order, Email = account.Email, };
            Result<PaymentLinkResponse> paymentResponse = await _paymentService.CreatePaymentLink(request);
            return paymentResponse;
        }
    }
}
