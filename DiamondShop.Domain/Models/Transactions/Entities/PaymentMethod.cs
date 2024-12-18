﻿using DiamondShop.Domain.Common;
using DiamondShop.Domain.Models.Transactions.ValueObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Models.Transactions.Entities
{
    public class PaymentMethod : Entity<PaymentMethodId>
    {
        public static PaymentMethod BANK_TRANSFER = PaymentMethod.Create("BANK_TRANSFER", PaymentMethodId.Parse("1"));
        public static PaymentMethod ZALOPAY = PaymentMethod.Create("ZALOPAY", PaymentMethodId.Parse("2"),50_000_000m);
        public static PaymentMethod CASH = PaymentMethod.Create("CASH", PaymentMethodId.Parse("3"));
        //TODO : khả năng add thêm nếu zalopay hạn mức thấp quá làm ko được
        public static PaymentMethod VNPAY = PaymentMethod.Create("VNPAY", PaymentMethodId.Parse("4"));
        public string MethodName { get; set; }
        public string? MethodThumbnailPath { get; set; }
        public decimal? MaxSupportedPrice { get; set; }
        public bool Status { get; set; } = true;
        public static PaymentMethod Create(string methodName, string givenId = null)
        {
            return new PaymentMethod
            {
                MethodName = methodName,
                Status = true,
                Id = givenId == null ? PaymentMethodId.Create() : PaymentMethodId.Parse(givenId),
            };
        }
        internal static PaymentMethod Create(string methodName, PaymentMethodId id, decimal? maxAmount = null)
        {
            return new PaymentMethod
            {
                MethodName = methodName,
                Status = true,
                Id = id,
                MaxSupportedPrice = maxAmount
            };
        }
        public void ChangeStatus(bool status) => Status = status;
        public void ChangeMaxSupportedPrice(decimal? newPrice) => MaxSupportedPrice = newPrice;
        public PaymentMethod() { }
    }
    public static class PaymentMethodHelper
    {

        public static List<PaymentMethod> AllMethods = new List<PaymentMethod> { PaymentMethod.BANK_TRANSFER, PaymentMethod.ZALOPAY };
        public static string GetMethodName(PaymentMethod method)
        {
            ArgumentNullException.ThrowIfNull(method);
            if (method.Id == PaymentMethod.BANK_TRANSFER.Id)
                return "Chuyển khoản qua cho shop qua ngân hàng";
            if (method.Id == PaymentMethod.ZALOPAY.Id)
                return "Thanh toán qua ZaloPay";
            if(method.Id == PaymentMethod.CASH.Id)
                return "Thanh toán bằng tiền mặt";
            if(method.Id == PaymentMethod.VNPAY.Id)
                return "Thanh toán qua VNPAY";
            return "Không xác định";
        }
    }
}
