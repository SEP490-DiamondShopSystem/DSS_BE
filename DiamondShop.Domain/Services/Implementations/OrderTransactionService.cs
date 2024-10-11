using DiamondShop.Domain.BusinessRules;
using DiamondShop.Domain.Models.Orders;
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

        public decimal GetDepositValueForOrder(Order order)
        {
            if (order.PaymentType != Models.Orders.Enum.PaymentType.COD)
            {
                throw new Exception("this is not of type COD ");
            }
            var depositPercent = OrderPaymentRules.DepositPercent;
            var neededToPayAmountRaw = order.TotalPrice * (Decimal.Divide(depositPercent,100));
            var roundedValue = MoneyVndRoundUpRules.RoundAmountFromDecimal(neededToPayAmountRaw); //Math.Round(Decimal.Divide(neededToPayAmountRaw, 1000), 1) * 1000;//the function is tested in linq pad
            return roundedValue;
        }

        public decimal GetFullPaymentValueForOrder(Order order)
        {
            return order.TotalPrice;
        }

        public decimal GetRemaingValueForOrder(Order order)
        {
            var transactions = _transactionRepository.GetByOrderId(order.Id).Result;
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
            return Result.Ok(  (wantedAmount,remaining) );
        }
    }
}
