﻿using DiamondShop.Application.Dtos.Responses.Accounts;
using DiamondShop.Application.Dtos.Responses.CustomizeRequests;
using DiamondShop.Application.Dtos.Responses.Transactions;
using DiamondShop.Domain.Models.Orders.Enum;

namespace DiamondShop.Application.Dtos.Responses.Orders
{
    public class OrderDto
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public AccountDto? Account { get; set; }
        public string? DelivererId { get; set; }
        public AccountDto? Deliverer { get; set; }
        public string? CustomizeRequestId { get; set; }
        public CustomizeRequestDto? CustomizeRequest { get; set; }
        public string CreatedDate { get; set; }
        public string ExpectedDate { get; set; }
        public string? ShippedDate { get; set; }
        public string? CancelledDate { get; set; }
        public string? CancelledReason { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentType PaymentType { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? PaymentMethodId { get; set; }
        public PaymentMethodDto? PaymentMethod { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DepositFee { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal OrderAmountSaved { get; set; } 
        public decimal UserRankAmountSaved { get; set; } 
        public decimal TotalRefund { get; set; } 
        public decimal TotalFine { get; set; }
        public string ShippingAddress { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public List<OrderLogDto> Logs { get; set; }
        public List<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
        public string OrderCode { get; set; }
        public string? ParentOrderId { get; set; } // for replacement order
        public string? DeliveryPackageId { get; set; }
        public string? ExpiredDate { get; set; }
        public string? ShipFailedDate { get; set; }
        public string? PromotionCode { get; set; }
        public string? CompleteDate { get; set; }
        public string? FinishPreparedDate { get; set; }
        public bool IsCollectAtShop { get; set; }
        public bool? HasDelivererReturned { get; set; }
        public int ShipFailedCount { get; set; }
    }
}
