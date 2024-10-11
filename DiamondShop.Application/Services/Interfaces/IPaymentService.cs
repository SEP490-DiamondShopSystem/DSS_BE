using DiamondShop.Application.Services.Models;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Models.Transactions;
using DiamondShop.Domain.Models.Transactions.ValueObjects;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DiamondShop.Application.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Result<PaymentLinkResponse>> CreatePaymentLink(PaymentLinkRequest paymentLinkRequest, CancellationToken cancellationToken = default);
        Task<object> Callback();// the object is dependent on the provider, so return object, be anonymouse as possible
        Task<Result<PaymentRefundDetail>> Refund(Order order, Transaction forTransaction, decimal fineAmount,string description = null);
        Task<PaymentDetail> GetTransactionDetail(Transaction payTrasactionType);
        Task<PaymentRefundDetail> GetRefundDetail(Transaction refundTransactionType);
    }
}
