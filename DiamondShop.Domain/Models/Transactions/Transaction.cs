﻿using DiamondShop.Domain.Common;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Models.Orders.ValueObjects;
using DiamondShop.Domain.Models.Transactions.Entities;
using DiamondShop.Domain.Models.Transactions.Enum;
using DiamondShop.Domain.Models.Transactions.ValueObjects;

namespace DiamondShop.Domain.Models.Transactions
{
    public class Transaction : Entity<TransactionId>, IAggregateRoot
    {
        public PaymentMethodId PayMethodId { get; set; }
        public PaymentMethod PayMethod { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Description { get; set; }
        public DateTime? PayDate { get; set; }
        public string AppTransactionCode { get; set; }
        public string PaygateTransactionCode { get; set; }
        public string TimeStampe { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal FineAmount { get; set; }

        public TransactionId? RefundedTransacId { get; set; }
        public Transaction? RefundedTransaction { get; set; }
        public OrderId? OrderId { get; set; }
        public static Transaction CreatePayment(PaymentMethodId paymentMethodId,OrderId orderId,string description,string appTransactionCode, string paygateTransactionCode , string timeStamp, decimal amount, DateTime paygatePaydate )
        {
            var id = TransactionId.Create();
            return new Transaction
            {
                Id = id,
                PayMethodId = paymentMethodId,
                TransactionType = TransactionType.Pay,
                Description = description,
                PayDate = paygatePaydate,
                PaygateTransactionCode = paygateTransactionCode,
                AppTransactionCode = appTransactionCode,
                TimeStampe = timeStamp,
                TransactionAmount = amount,
                OrderId = orderId,
                FineAmount = 0,
            };
        }
        public static Transaction CreateRefund(PaymentMethodId paymentMethodId, OrderId orderId, TransactionId forTransactionId, string description,string appTransactionCode, string paygateTransactionCode, string timeStamp, decimal refundAmound, decimal fineAmount = 0)
        {
            var id = TransactionId.Create();
            return new Transaction
            {
                Id = id,
                OrderId = orderId,
                PayMethodId = paymentMethodId,
                TransactionType = (fineAmount == 0) ? TransactionType.Refund : TransactionType.Partial_Refund,
                Description = description,
                PayDate = DateTime.UtcNow,
                PaygateTransactionCode = paygateTransactionCode,
                AppTransactionCode = appTransactionCode,
                TimeStampe = timeStamp,
                TransactionAmount = refundAmound,
                FineAmount = fineAmount,
                RefundedTransacId = forTransactionId,
            };
        }
        private Transaction() { }
    }
}
