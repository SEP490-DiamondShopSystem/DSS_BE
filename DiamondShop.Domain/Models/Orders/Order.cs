using DiamondShop.Domain.BusinessRules;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Models.AccountAggregate;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Models.CustomizeRequests.ValueObjects;
using DiamondShop.Domain.Models.Notifications.ValueObjects;
using DiamondShop.Domain.Models.Orders.Entities;
using DiamondShop.Domain.Models.Orders.Enum;
using DiamondShop.Domain.Models.Orders.ValueObjects;
using DiamondShop.Domain.Models.Promotions;
using DiamondShop.Domain.Models.Promotions.ValueObjects;
using DiamondShop.Domain.Models.Transactions;
using DiamondShop.Domain.Models.Transactions.Entities;
using DiamondShop.Domain.Models.Transactions.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text; 
using System.Threading.Tasks; 

namespace DiamondShop.Domain.Models.Orders
{
    public class Order : Entity<OrderId>, IAggregateRoot
    {
        public AccountId AccountId { get; set; }
        public Account? Account { get; set; }
        public CustomizeRequestId? CustomizeRequestId { get; set; }
        public PromotionId? PromotionId { get; set; }
        public Promotion? Promotion { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now.ToUniversalTime();
        public DateTime ExpectedDate { get; set; } = DateTime.Now.AddDays(OrderRules.ExpectedDeliveryDate).ToUniversalTime();
        public DateTime? ShippedDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? CancelledReason { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentType PaymentType { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalRefund { get; set; } = 0;
        public decimal TotalFine { get; set; } = 0;
        public string ShippingAddress { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public List<OrderLog> Logs { get; set; } = new();
        public List<Transaction> Transactions { get; set; } = new();

        public OrderId? ParentOrderId { get; set; } // for replacement order
        public DeliveryPackageId? DeliveryPackageId { get; set; }

        public string? Note { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public DateTime? ShipFailedDate{ get; set; }
        public int ShipFailedCount { get; set; } = 0;
        [NotMapped]
        public bool IsCustomOrder { get => CustomizeRequestId != null; }
        public void AddTransaction(Transaction transactionTypePay) 
        {
            if(transactionTypePay.TransactionType != Models.Transactions.Enum.TransactionType.Pay)
                throw new InvalidOperationException("Transaction type must be Pay");
            Transactions.Add(transactionTypePay);
        }
        public void AddRefund(Transaction transactionTypeRefund)
        {
            if (transactionTypeRefund.TransactionType != Models.Transactions.Enum.TransactionType.Refund)
                throw new InvalidOperationException("Transaction type must be Refund");
            TotalRefund += transactionTypeRefund.TotalAmount;
            TotalFine += transactionTypeRefund.FineAmount;
            Transactions.Add(transactionTypeRefund);
        }

        public Order() { }
        public static Order Create(AccountId accountId, PaymentType paymentType, 
            decimal totalPrice, decimal shippingFee, string shippingAddress,
            PromotionId promotionId = null, OrderId givenId = null)
        {
            return new Order()
            {
                Id = givenId is null ? OrderId.Create() : givenId,
                AccountId = accountId,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                PromotionId = promotionId,
                PaymentType = paymentType,
                TotalPrice = totalPrice,
                ShippingFee = shippingFee,
                ShippingAddress = shippingAddress
            };
        }
    }
}
