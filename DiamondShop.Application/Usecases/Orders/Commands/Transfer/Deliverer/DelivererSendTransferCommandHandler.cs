﻿using DiamondShop.Application.Commons.Models;
using DiamondShop.Application.Dtos.Requests.Orders;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Application.Services.Interfaces.Transfers;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Models.Orders.Enum;
using DiamondShop.Domain.Models.Orders.ErrorMessages;
using DiamondShop.Domain.Models.Orders.ValueObjects;
using DiamondShop.Domain.Models.Transactions;
using DiamondShop.Domain.Models.Transactions.Entities;
using DiamondShop.Domain.Models.Transactions.Enum;
using DiamondShop.Domain.Models.Transactions.ErrorMessages;
using DiamondShop.Domain.Repositories.OrderRepo;
using DiamondShop.Domain.Repositories.TransactionRepo;
using DiamondShop.Domain.Services.interfaces;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace DiamondShop.Application.Usecases.Orders.Commands.Transfer.Deliverer
{
    public record DelivererSendTransferCommand(string AccountId, TransferVerifyRequestDto TransferSubmitRequestDto) : IRequest<Result<Order>>;
    internal class DelivererSendTransferCommandHandler : IRequestHandler<DelivererSendTransferCommand, Result<Order>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderLogRepository _orderLogRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderTransactionService _orderTransactionService;
        private readonly IOrderService _orderService;
        private readonly ITransferFileService _transferFileService;

        public DelivererSendTransferCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, IOrderService orderService, IOrderLogRepository orderLogRepository, IOrderTransactionService orderTransactionService, ITransactionRepository transactionRepository, ITransferFileService transferFileService)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _orderService = orderService;
            _orderLogRepository = orderLogRepository;
            _orderTransactionService = orderTransactionService;
            _transactionRepository = transactionRepository;
            _transferFileService = transferFileService;
        }

        public async Task<Result<Order>> Handle(DelivererSendTransferCommand request, CancellationToken token)
        {
            request.Deconstruct(out string accountId, out TransferVerifyRequestDto transferSubmitRequestDto);
            transferSubmitRequestDto.Deconstruct(out string orderId, out IFormFile evidence);
            await _unitOfWork.BeginTransactionAsync(token);
            var order = await _orderRepository.GetById(OrderId.Parse(orderId));
            if (order == null)
                return Result.Fail(OrderErrors.OrderNotFoundError);
            else if (order.DelivererId != AccountId.Parse(accountId))
                return Result.Fail(OrderErrors.OnlyDelivererAllowedError);
            else if (order.Status != OrderStatus.Delivering && order.PaymentType != PaymentType.COD && order.PaymentMethodId != PaymentMethod.BANK_TRANSFER.Id)
                return Result.Fail(OrderErrors.UnTransferableError);
            var transactions = await _transactionRepository.GetByOrderId(order.Id, token);
            //expected only exists a deposit transaction
            if(transactions.Count != 1)
                return Result.Fail(TransactionErrors.TransactionExistError);
            //remaing amount
            var payAmount = order.TotalPrice - order.DepositFee;
            var manualPayment = Transaction.CreateManualPayment(order.Id, "ACB", "NGUYENVANA", $"Chuyển tiền còn lại từ tài khoản {order.Account?.FullName.FirstName} {order.Account?.FullName.LastName} cho đơn hàng {order.OrderCode}", payAmount, TransactionType.Pay);
            await _transactionRepository.Create(manualPayment, token);
            await _unitOfWork.SaveChangesAsync(token);
            //add evidence to blob
            var uploadResult = await _transferFileService.UploadTransferImage(manualPayment, new FileData(evidence.FileName, null, evidence.ContentType, evidence.OpenReadStream()));
            if (uploadResult.IsFailed)
                return Result.Fail(uploadResult.Errors);
            manualPayment.Evidence = Media.Create("evidence", uploadResult.Value, evidence.ContentType);
            await _transactionRepository.Update(manualPayment, token);
            await _unitOfWork.SaveChangesAsync(token);
            await _unitOfWork.CommitAsync(token);
            return order;
        }
    }
}
