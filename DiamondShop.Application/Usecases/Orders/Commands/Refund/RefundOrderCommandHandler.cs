﻿using DiamondShop.Application.Commons.Models;
using DiamondShop.Application.Dtos.Requests.Orders;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Application.Services.Interfaces.Transfers;
using DiamondShop.Domain.BusinessRules;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Models.Orders.Entities;
using DiamondShop.Domain.Models.Orders.Enum;
using DiamondShop.Domain.Models.Orders.ErrorMessages;
using DiamondShop.Domain.Models.Orders.ValueObjects;
using DiamondShop.Domain.Models.Transactions;
using DiamondShop.Domain.Models.Transactions.Enum;
using DiamondShop.Domain.Models.Transactions.ErrorMessages;
using DiamondShop.Domain.Repositories.OrderRepo;
using DiamondShop.Domain.Repositories.TransactionRepo;
using DiamondShop.Domain.Services.interfaces;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.Orders.Commands.Refund
{
    public record RefundOrderCommand(string AccountId, RefundConfirmRequestDto RefundConfirmRequestDto) : IRequest<Result<Order>>;
    internal class RefundOrderCommandHandler : IRequestHandler<RefundOrderCommand, Result<Order>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderTransactionService _orderTransactionService;
        private readonly IOrderLogRepository _orderLogRepository;
        private readonly IOrderService _orderService;
        private readonly ITransferFileService _transferFileService;
        private readonly IOptionsMonitor<ApplicationSettingGlobal> _optionsMonitor;

        public RefundOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, IOrderTransactionService orderTransactionService, IOrderLogRepository orderLogRepository, IOrderService orderService, ITransactionRepository transactionRepository, ITransferFileService transferFileService, IOptionsMonitor<ApplicationSettingGlobal> optionsMonitor)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _orderTransactionService = orderTransactionService;
            _orderLogRepository = orderLogRepository;
            _orderService = orderService;
            _transactionRepository = transactionRepository;
            _transferFileService = transferFileService;
            _optionsMonitor = optionsMonitor;
        }

        public async Task<Result<Order>> Handle(RefundOrderCommand request, CancellationToken token)
        {
            var shopRule = _optionsMonitor.CurrentValue.ShopBankAccountRules;
            request.Deconstruct(out string accountId, out RefundConfirmRequestDto refundConfirmRequestDto);
            refundConfirmRequestDto.Deconstruct(out string orderId, out decimal amount, out string transactionCode, out IFormFile evidence);
            await _unitOfWork.BeginTransactionAsync(token);
            var order = await _orderRepository.GetById(OrderId.Parse(orderId));
            if (order == null)
                return Result.Fail(OrderErrors.OrderNotFoundError);
            if (order.PaymentStatus != PaymentStatus.Refunding)
                return Result.Fail(OrderErrors.RefundedError);
            var transactions = await _transactionRepository.GetByOrderId(order.Id, token);
            if (transactions.Any(p => p.Status == TransactionStatus.Verifying))
                return Result.Fail(OrderErrors.Transfer.ExistVerifyingTransferError);
            if (transactions.Count > 0)
            {
                //var getStateChangingLogs = await _orderLogRepository.GetStateChangingLog(order);
                //var currentStatus = order.Status;
                //var orderCancelOrRejectLogs = getStateChangingLogs.Where(l => l.Status == currentStatus).FirstOrDefault();
                //var indexOfLog = getStateChangingLogs.IndexOf(orderCancelOrRejectLogs);
                //OrderLog previousLog = getStateChangingLogs[indexOfLog - 1];
                //var previousStatus = previousLog.Status;

                var refundAmount = transactions.Where(x => x.TransactionType == TransactionType.Pay && x.Status == TransactionStatus.Valid)
                    .Sum(p => p.TransactionAmount);
                if (order.Status == OrderStatus.Cancelled && order.PaymentType == PaymentType.Payall)
                    refundAmount = MoneyVndRoundUpRules.RoundAmountFromDecimal(refundAmount * (1m - 0.01m * OrderPaymentRules.Default.PayAllFine));
                if (order.Status == OrderStatus.Cancelled && order.PaymentType == PaymentType.COD)
                {
                    var fine = order.DepositFee;
                    refundAmount = MoneyVndRoundUpRules.RoundAmountFromDecimal(fine);
                }
                if (refundAmount != amount)
                    return Result.Fail(TransactionErrors.TransactionNotValid);
                var refundPayment = Transaction.CreateManualRefund(order.Id, shopRule.BankName, shopRule.AccountName, AccountId.Parse(accountId), transactionCode, $"Hoàn tiền đến khách hàng {order.Account?.FullName.FirstName} {order.Account?.FullName.LastName} cho đơn hàng ${order.OrderCode}", refundAmount);
                await _transactionRepository.Create(refundPayment);
                await _unitOfWork.SaveChangesAsync(token);
                var uploadResult = await _transferFileService.UploadTransferImage(refundPayment, new FileData(evidence.FileName, null, evidence.ContentType, evidence.OpenReadStream()));
                if (uploadResult.IsFailed)
                    return Result.Fail(uploadResult.Errors);
                refundPayment.Evidence = Media.Create("evidence", uploadResult.Value, evidence.ContentType);
                order.PaymentStatus = PaymentStatus.Refunded;
            }
            await _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync(token);
            await _unitOfWork.CommitAsync(token);
            return order;
        }
    }
}
