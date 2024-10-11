using DiamondShop.Domain.Models.Orders;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Services.interfaces
{
    public interface IOrderTransactionService
    {
        Result<(decimal allowAmount, decimal remainingAmount)> GetTransactionValueForOrder(Order order, decimal wantedAmount);
        decimal GetDepositValueForOrder(Order order);
        decimal GetFullPaymentValueForOrder(Order order);
        decimal GetRemaingValueForOrder(Order order);
        decimal GetCODValueForOrder(Order order);
    }
}
