using DiamondShop.Domain.BusinessRules;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Models.CustomizeRequests.ValueObjects;
using DiamondShop.Domain.Models.DiamondShapes;
using DiamondShop.Domain.Models.DiamondShapes.ValueObjects;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Models.Orders.Enum;
using DiamondShop.Domain.Models.Transactions;
using DiamondShop.Domain.Repositories.TransactionRepo;
using DiamondShop.Domain.Services.Implementations;
using DiamondShop.Domain.Services.interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Org.BouncyCastle.Crypto.Prng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Test.Domain
{
    public class OrderTransactionServiceTest
    {
        public List<DiamondShape> _diamondShapes;
        public IOrderTransactionService  OrderTransactionService{ get; set; }
        public OrderTransactionServiceTest()
        {
            _diamondShapes = new List<DiamondShape> {
                DiamondShape.Create("Round",DiamondShapeId.Parse(1.ToString())),
                DiamondShape.Create("Princess",DiamondShapeId.Parse(2.ToString())),
                DiamondShape.Create("Cushion",DiamondShapeId.Parse(3.ToString())),
                DiamondShape.Create("Emerald",DiamondShapeId.Parse(4.ToString())),
                DiamondShape.Create("Oval",DiamondShapeId.Parse(5.ToString())),
                DiamondShape.Create("Radiant",DiamondShapeId.Parse(6.ToString())),
                DiamondShape.Create("Asscher",DiamondShapeId.Parse(7.ToString())),
                DiamondShape.Create("Marquise",DiamondShapeId.Parse(8.ToString())),
                DiamondShape.Create("Heart",DiamondShapeId.Parse(9.ToString())),
                DiamondShape.Create("Pear",DiamondShapeId.Parse(10.ToString()))
            };
            var loggerMock = Mock.Of<ILogger<OrderTransactionService>>();
            var trasactionRepoMock = Mock.Of<ITransactionRepository>();
            OrderTransactionService = new OrderTransactionService(loggerMock,trasactionRepoMock);
        }
        [Fact]
        public void NormalOrder_Payall_Pending_Should_return_full()
        {
            // Arrange
            var expected = 30_000_000m;
            var order = Order.Create(AccountId.Create(), PaymentType.Payall,30_000_000m, 300_000m,"adsf");
            // Act
            var amount = OrderTransactionService.GetCorrectAmountFromOrder(order);
            // Assert
            Assert.Equal(amount, expected);
        }

        [Fact]
        public void NormalOrder_COD_Pending_Should_return_partly()
        {
            // Arrange
            var amount = 30_000_000m;
            var expected = amount * ( decimal.Divide(OrderPaymentRules.CODPercent,100));
            var order = Order.Create(AccountId.Create(), PaymentType.COD, amount, 300_000m, "adsf");
            // Act
            var result = OrderTransactionService.GetCorrectAmountFromOrder(order);
            // Assert
            Assert.Equal(expected, result);
        }
        [Fact]
        public void NormalOrder_COD_Deposited_Should_return_partly()
        {
            // Arrange
            var amount = 30_000_000m;
            var transactionAmount = amount * (decimal.Divide(OrderPaymentRules.CODPercent, 100));
            var expected = amount - transactionAmount;
            //var expected = amount * (decimal.Divide(OrderPaymentRules.CODPercent, 100));
            var order = Order.Create(AccountId.Create(), PaymentType.COD, amount, 300_000m, "adsf");
            var transaction = Transaction.CreateManualPayment(order.Id,"asdf", transactionAmount,DiamondShop.Domain.Models.Transactions.Enum.TransactionType.Pay);
            order.Transactions.Add(transaction);
            order.PaymentStatus = PaymentStatus.Deposited;
            // Act
            var result = OrderTransactionService.GetCorrectAmountFromOrder(order);
            // Assert
            Assert.Equal(expected, result);
        }
         [Fact]
        public void CustomOrder_Payall_Pending_Should_return_full()
        {
            // Arrange
            var expected = 30_000_000m;
            var order = Order.Create(AccountId.Create(), PaymentType.Payall,30_000_000m, 300_000m,"adsf");
            order.CustomizeRequestId = CustomizeRequestId.Create();
            // Act
            var amount = OrderTransactionService.GetCorrectAmountFromOrder(order);
            // Assert
            Assert.Equal(amount, expected);
        }

        [Fact]
        public void CustomOrder_COD_Pending_Should_return_partly()
        {
            // Arrange
            var amount = 30_000_000m;
            var expected = amount * ( decimal.Divide(OrderPaymentRules.CODPercentCustom,100));
            var order = Order.Create(AccountId.Create(), PaymentType.COD, amount, 300_000m, "adsf");
            order.CustomizeRequestId = CustomizeRequestId.Create();
            // Act
            var result = OrderTransactionService.GetCorrectAmountFromOrder(order);
            // Assert
            Assert.Equal(expected, result);
        }
        [Fact]
        public void CustomOrder_COD_Deposited_Should_return_partly()
        {
            // Arrange
            var amount = 30_000_000m;
            var transactionAmount = amount * (decimal.Divide(OrderPaymentRules.CODPercentCustom, 100));
            var expected = amount - transactionAmount;
            //var expected = amount * (decimal.Divide(OrderPaymentRules.CODPercent, 100));
            var order = Order.Create(AccountId.Create(), PaymentType.COD, amount, 300_000m, "adsf");
            order.CustomizeRequestId = CustomizeRequestId.Create();
            var transaction = Transaction.CreateManualPayment(order.Id,"asdf", transactionAmount,DiamondShop.Domain.Models.Transactions.Enum.TransactionType.Pay);
            order.Transactions.Add(transaction);
            order.PaymentStatus = PaymentStatus.Deposited;
            // Act
            var result = OrderTransactionService.GetCorrectAmountFromOrder(order);
            // Assert
            Assert.Equal(expected, result);
        }
        [Fact]
        public void NormalOrder_deposited_GetRefund_Should_return_transactionAmount() 
        {
            //arrange
            var amount = 30_000_000m;
            var transactionAmount = amount * (decimal.Divide(OrderPaymentRules.CODPercent, 100));
            var expected = amount - transactionAmount;
            var order = Order.Create(AccountId.Create(), PaymentType.COD, amount, 300_000m, "adsf");
            var transaction = Transaction.CreateManualPayment(order.Id, "asdf", transactionAmount, DiamondShop.Domain.Models.Transactions.Enum.TransactionType.Pay);
            order.Transactions.Add(transaction);
            order.PaymentStatus = PaymentStatus.Deposited;
            //act
            var refundAmount = OrderTransactionService.GetRefundAmountFromOrder(order, 0, "asdf").Result;
            //assert
            Assert.Equal(transactionAmount, refundAmount.TotalAmount);
        }
    }
}
