using DiamondShop.Domain.BusinessRules;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Models.Orders.Enum;
using DiamondShop.Domain.Models.Transactions;
using DiamondShop.Domain.Repositories.TransactionRepo;
using DiamondShop.Domain.Services.interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Services.Implementations
{
    public class OrderTransactionService : IOrderTransactionService
    {
        private readonly ILogger<OrderTransactionService> _logger;
        private readonly ITransactionRepository _transactionRepository;

        public OrderTransactionService(ILogger<OrderTransactionService> logger, ITransactionRepository transactionRepository)
        {
            _logger = logger;
            _transactionRepository = transactionRepository;
        }

        public decimal GetCODValueForOrder(Order order)
        {
            if (order.PaymentType != Models.Orders.Enum.PaymentType.COD)
            {
                throw new Exception("this is not of type COD ");
            }
            var codPercent = OrderPaymentRules.CODPercent;
            var neededToPayAmountRaw = order.TotalPrice * (Decimal.Divide(codPercent, 100));
            var roundedValue = MoneyVndRoundUpRules.RoundAmountFromDecimal(neededToPayAmountRaw); //Math.Round(Decimal.Divide(neededToPayAmountRaw, 1000), 1) * 1000;//the function is tested in linq pad
            return roundedValue;
        }

        public decimal GetCorrectAmountFromOrder(Order order)
        {
            decimal? getCorrectAmount = null;
            Func<Order, decimal> getRemainingForCOD = (Order inputOrder) =>
            {
                if (inputOrder.PaymentStatus == PaymentStatus.Deposited && inputOrder.PaymentType == PaymentType.COD)
                    return GetRemaingValueForOrder(inputOrder);
                else if (inputOrder.PaymentStatus == PaymentStatus.Pending && inputOrder.PaymentType == PaymentType.COD)
                {
                    if (inputOrder.IsCustomOrder)
                        return GetDepositValueForOrder(inputOrder);
                    return GetCODValueForOrder(inputOrder);
                }
                else
                    throw new Exception("the order is not deposited or is not COD  to get the remaining");
            };
            if (order.IsCustomOrder)
            {
                if (order.PaymentType == PaymentType.Payall)
                    getCorrectAmount = GetFullPaymentValueForOrder(order);

                else if (order.PaymentType == PaymentType.COD)
                    getCorrectAmount = getRemainingForCOD(order);

                else
                    throw new Exception("unspecified payment type");
            }
            else
            {
                if (order.PaymentType == PaymentType.Payall)
                    getCorrectAmount = GetFullPaymentValueForOrder(order);

                else if (order.PaymentType == PaymentType.COD)
                    getCorrectAmount = getRemainingForCOD(order);
                else
                    throw new Exception("unspecified payment type");
            }
            if (getCorrectAmount is null)
                throw new Exception("the correct amount is null");
            return getCorrectAmount.Value;
        }

        public decimal GetDepositValueForOrder(Order order)
        {
            if (order.PaymentType != Models.Orders.Enum.PaymentType.COD)
            {
                throw new Exception("this is not of type COD ");
            }
            var depositPercent = OrderPaymentRules.DepositPercent;
            var neededToPayAmountRaw = order.TotalPrice * (Decimal.Divide(depositPercent, 100));
            var roundedValue = MoneyVndRoundUpRules.RoundAmountFromDecimal(neededToPayAmountRaw); //Math.Round(Decimal.Divide(neededToPayAmountRaw, 1000), 1) * 1000;//the function is tested in linq pad
            return roundedValue;
        }

        public decimal GetFullPaymentValueForOrder(Order order)
        {
            return order.TotalPrice;
        }

        public async Task<Transaction> GetRefundAmountFromOrder(Order order, decimal fineAmount, string description)
        {
            if (order.PaymentStatus == Models.Orders.Enum.PaymentStatus.Refunded)
            {
                throw new Exception("this order is already refunded");
            }
            var sumTransactions = order.Transactions.Where(t => t.TransactionType == Models.Transactions.Enum.TransactionType.Pay).Sum(x => x.TotalAmount);
            var refundTrans = order.Transactions.Where(t => t.TransactionType == Models.Transactions.Enum.TransactionType.Refund || t.TransactionType == Models.Transactions.Enum.TransactionType.Partial_Refund).Sum(x => x.TotalAmount);
            var leftAmount = sumTransactions - refundTrans;
            if (leftAmount <= 0)
                throw new Exception("the order is already refunded");
            if (leftAmount - fineAmount <= 0)
                throw new Exception("the fine amount is more or the same as the left amount to refund");
            return Transaction.CreateManualRefund(order.Id, description, leftAmount - fineAmount, fineAmount);
        }

        public decimal GetRemaingValueForOrder(Order order)
        {
            var transactions = order.Transactions;
            var paidAmount = transactions.Sum(x => x.TransactionAmount);
            return order.TotalPrice - paidAmount;
        }

        public Result<(decimal allowAmount, decimal remainingAmount)> GetTransactionValueForOrder(Order order, decimal wantedAmount)
        {
            var orderTotal = order.TotalPrice;
            var remaining = orderTotal - wantedAmount;
            if (wantedAmount > TransactionRules.MaximumPerTransaction)
                return Result.Fail($"the wanted to pay amount is: {wantedAmount} , which is not in our rules is maximum = {TransactionRules.MaximumPerTransaction}");
            if (remaining < TransactionRules.MinimumPerTransaction)
                return Result.Fail($"the remaining money is: {remaining} , which is not in our rules is minimum = {TransactionRules.MinimumPerTransaction}");
            return Result.Ok((wantedAmount, remaining));
        }
    }
}
