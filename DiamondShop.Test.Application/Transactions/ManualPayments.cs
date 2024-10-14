using Castle.Core.Logging;
using DiamondShop.Application.Services.Data;
using DiamondShop.Application.Usecases.JewelryModelCategories.Queries.GetAll;
using DiamondShop.Application.Usecases.Transactions.Commands.AddManualPayments;
using DiamondShop.Application.Usecases.Transactions.Queries.GetAll;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Models.Diamonds.ValueObjects;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Models.Orders.Entities;
using DiamondShop.Domain.Models.Orders.Enum;
using DiamondShop.Domain.Models.Orders.ValueObjects;
using DiamondShop.Domain.Repositories.OrderRepo;
using DiamondShop.Domain.Repositories.TransactionRepo;
using DiamondShop.Domain.Services.Implementations;
using DiamondShop.Domain.Services.interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Test.Application.Transactions
{
    //[CollectionDefinition("Order")]
    public class InitContext //: ICollect
    {
        public Order ManualNormalOrder { get; set; }
        public IServiceCollection ServiceCollections{ get; set; }
        public Mock<IMediator> Mediator { get; set; }
        public Mock<IHttpContextAccessor> HttpContextMock { get; set; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; set; }
        public InitContext() 
        {
            HttpContextMock = new Mock<IHttpContextAccessor>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            SetupServices();
            SetupMediator();
            var manualNormalOrder = new Order()
            {
                AccountId = AccountId.Parse("whatever"),
                CreatedDate = DateTime.UtcNow,
                Id = OrderId.Create(),
            };
            manualNormalOrder.Items.AddRange(_diamondItems);
            manualNormalOrder.Items.ForEach(i => i.OrderId = manualNormalOrder.Id);
            manualNormalOrder.TotalPrice = manualNormalOrder.Items.Sum(i => i.PurchasedPrice);
            ManualNormalOrder = manualNormalOrder;
            
        }
        private void SetupServices()
        {
            var services = new ServiceCollection();
            var serviceCollection = services
                .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(GetAllTransactionQuery).Assembly));
            ServiceCollections = serviceCollection;
        }
        private void SetupMediator()
        {
            Mediator = new Mock<IMediator>();
        }
        private static List<OrderItem> _diamondItems = new List<OrderItem>()
        {
            new OrderItem()
            {
                Id = OrderItemId.Create(),
                DiamondId = DiamondId.Create(),
                PurchasedPrice = 20_000_000,
                Status = OrderItemStatus.Preparing,
            },
            new OrderItem()
            {
                Id = OrderItemId.Create(),
                DiamondId = DiamondId.Create(),
                PurchasedPrice = 30_000_000,
                Status = OrderItemStatus.Preparing,
            }
        };
    }
    //[Collection("Order")]
    public class ManualPayments : IClassFixture<InitContext>
    {
        public InitContext Context;

        public ManualPayments(InitContext initContext)
        {
            Context= initContext;
        }
        [Fact]
        public async void Should_Create_PaymentLink_For_Normal_Order()
        {
            // Arrange
            //var _sender = Context.ServiceCollections.BuildServiceProvider().GetRequiredService<ISender>();

            var transactionRepository = new Mock<ITransactionRepository>().Object;
            var orderRepository = new Mock<IOrderRepository>();
            orderRepository.Setup(r => r.GetById(It.IsAny<object>())).Returns(  Task.FromResult(Context.ManualNormalOrder));
            var paymentMethodRepository = new Mock<IPaymentMethodRepository>().Object;
            IOrderTransactionService orderPaymentService = new OrderTransactionService( Mock.Of<ILogger<OrderTransactionService>>(), transactionRepository);

            var handler = new AddTransactionManuallyCommandHandler(Context.HttpContextMock.Object,transactionRepository,orderPaymentService,orderRepository.Object,paymentMethodRepository,Context.UnitOfWorkMock.Object);
            Context.Mediator.Setup(m => m.Send(It.IsAny<AddTransactionManuallyCommand>(),CancellationToken.None))
                .Returns( async (AddTransactionManuallyCommand command, CancellationToken cancellationToken) => 
                {
                    return await handler.Handle(command,CancellationToken.None);
                });
            var command = new AddTransactionManuallyCommand(orderId: Context.ManualNormalOrder.Id.Value, PaymentType: PaymentType.Payall, description: "Test");
            // Act

            var result = await Context.Mediator.Object.Send(command);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value.TotalAmount == Context.ManualNormalOrder.Items.Sum(i => i.PurchasedPrice));
            Assert.True(result.Value.IsManual = true);


        }
    }
}
