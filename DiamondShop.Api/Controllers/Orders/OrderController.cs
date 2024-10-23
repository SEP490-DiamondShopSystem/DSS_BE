using DiamondShop.Api.Controllers.Orders.Cancel;
using DiamondShop.Application.Dtos.Requests.Orders;
using DiamondShop.Application.Dtos.Responses.Orders;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Application.Usecases.Orders.Commands.Accept;
using DiamondShop.Application.Usecases.Orders.Commands.AddToDelivery;
using DiamondShop.Application.Usecases.Orders.Commands.Complete;
using DiamondShop.Application.Usecases.Orders.Commands.Create;
using DiamondShop.Application.Usecases.Orders.Commands.Preparing;
using DiamondShop.Application.Usecases.Orders.Queries.GetAll;
using DiamondShop.Application.Usecases.Orders.Queries.GetPaymentLink;
using DiamondShop.Application.Usecases.Orders.Queries.GetUserOrderDetail;
using DiamondShop.Application.Usecases.Orders.Queries.GetUserOrders;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Models.Orders.ValueObjects;
using DiamondShop.Domain.Models.RoleAggregate;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiamondShop.Api.Controllers.Orders
{
    [Route("api/Order")]
    [ApiController]
    [Authorize]
    public class OrderController : ApiControllerBase
    {
        private readonly ISender _sender;
        private readonly IMapper _mapper;

        public OrderController(ISender sender, IMapper mapper)
        {
            _sender = sender;
            _mapper = mapper;
        }
        [HttpGet("All")]
        [Produces<List<OrderDto>>]
        [Authorize(Roles = AccountRole.ManagerId)]
        [Authorize(Roles = AccountRole.StaffId)]
        public async Task<ActionResult> GetAllOrder()
        {
            var result = await _sender.Send(new GetAllOrderQuery());
            var mappedResult = _mapper.Map<List<OrderDto>>(result);
            return Ok(mappedResult);
        }

        [HttpGet("User/All")]
        [Produces<List<OrderDto>>]
        public async Task<ActionResult> GetUserOrder()
        {
            var userId = User.FindFirst(IJwtTokenProvider.USER_ID_CLAIM_NAME);
            if (userId != null)
            {
                var result = await _sender.Send(new GetUserOrdersQuery(userId.Value));
                var mappedResult = _mapper.Map<List<OrderDto>>(result);
                return Ok(mappedResult);
            }
            else
                return Unauthorized();
        }
        [HttpGet("User/{orderId}")]
        [Authorize(Roles = AccountRole.CustomerId)]
        public async Task<ActionResult> GetUserOrderDetail([FromRoute] string orderId)
        {
            var userId = User.FindFirst(IJwtTokenProvider.USER_ID_CLAIM_NAME);
            if (userId != null)
            {
                var result = await _sender.Send(new GetOrderDetailQuery(orderId, userId.Value, false));
                var mappedResult = _mapper.Map<OrderDto>(result.Value);
                return Ok(mappedResult);
            }
            else
                return Unauthorized();
        }
        [HttpGet("{orderId}")]
        [Authorize(Roles = AccountRole.StaffId)]
        public async Task<ActionResult> GetOrderDetail([FromRoute] string orderId)
        {
            var userId = User.FindFirst(IJwtTokenProvider.USER_ID_CLAIM_NAME);
            if (userId != null)
            {
                var result = await _sender.Send(new GetOrderDetailQuery(orderId, userId.Value, true));
                var mappedResult = _mapper.Map<OrderDto>(result.Value);
                return Ok(mappedResult);
            }
            else
                return Unauthorized();
        }

        [HttpPost("Checkout")]
        [Authorize(Roles = AccountRole.CustomerId)]
        public async Task<ActionResult> Checkout([FromBody] CheckoutRequestDto checkoutRequestDto)
        {
            var userId = User.FindFirst(IJwtTokenProvider.USER_ID_CLAIM_NAME);
            if (userId != null)
            {
                var result = await _sender.Send(new CreateOrderCommand(userId.Value, checkoutRequestDto.BillingDetail, checkoutRequestDto.CreateOrderInfo));
                if (result.IsSuccess)
                {
                    return Ok(result.Value);
                }
                return MatchError(result.Errors, ModelState);
            }
            else
                return Unauthorized();
        }

        [HttpPut("Cancel")]
        [Authorize(Roles = AccountRole.CustomerId)]
        public async Task<ActionResult> CancelOrder([FromRoute] string orderId)
        {
            var userId = User.FindFirst(IJwtTokenProvider.USER_ID_CLAIM_NAME);
            if (userId != null)
            {
                var result = await _sender.Send(new CancelOrderCommand(orderId, userId.Value, false));
                if (result.IsSuccess)
                {
                    var mappedResult = _mapper.Map<OrderDto>(result.Value);
                    return Ok(mappedResult);
                }
                else
                    return MatchError(result.Errors, ModelState);
            }
            else
                return Unauthorized();
        }

        [HttpPut("Reject")]
        [Authorize(Roles = AccountRole.StaffId)]
        public async Task<ActionResult> RejectOrder([FromQuery] string orderId)
        {
            var userId = User.FindFirst(IJwtTokenProvider.USER_ID_CLAIM_NAME);
            if (userId != null)
            {
                var result = await _sender.Send(new CancelOrderCommand(orderId, userId.Value, true));
                if (result.IsSuccess)
                {
                    var mappedResult = _mapper.Map<OrderDto>(result.Value);
                    return Ok(mappedResult);
                }
                else
                    return MatchError(result.Errors, ModelState);
            }
            else
                return Unauthorized();
        }

        [HttpPut("Accept")]
        [Authorize(Roles = AccountRole.StaffId)]
        public async Task<ActionResult> AcceptOrder([FromQuery] AcceptOrderCommand acceptOrderCommand)
        {
            var result = await _sender.Send(acceptOrderCommand);
            if (result.IsSuccess)
            {
                var mappedResult = _mapper.Map<OrderDto>(result.Value);
                return Ok(mappedResult);
            }
            else
                return MatchError(result.Errors, ModelState);
        }

        [HttpPut("Preparing")]
        [Authorize(Roles = AccountRole.StaffId)]
        public async Task<ActionResult> PreparingOrder([FromQuery] PreparingOrderCommand preparingOrderCommand)
        {
            var result = await _sender.Send(preparingOrderCommand);
            if (result.IsSuccess)
            {
                var mappedResult = _mapper.Map<OrderDto>(result.Value);
                return Ok(mappedResult);
            }
            else
                return MatchError(result.Errors, ModelState);
        }

        [HttpPut("AddToDelivery")]
        [Authorize(Roles = AccountRole.ManagerId)]
        public async Task<ActionResult> DeliveringOrder([FromQuery] AddOrderToDeliveryCommand addOrderToDeliveryCommand)
        {
            var result = await _sender.Send(addOrderToDeliveryCommand);
            if (result.IsSuccess)
            {
                return Ok("Order added to delivery!");
            }
            else
                return MatchError(result.Errors, ModelState);
        }
        [HttpPut("Complete")]
        [Authorize(Roles = AccountRole.StaffId)]
        public async Task<ActionResult> CompleteOrder([FromQuery] string orderId)
        {
            var userId = User.FindFirst(IJwtTokenProvider.USER_ID_CLAIM_NAME);
            if (userId != null)
            {
                var result = await _sender.Send(new CompleteOrderCommand(orderId, userId.Value));
                if (result.IsSuccess)
                {
                    var mappedResult = _mapper.Map<OrderDto>(result.Value);
                    return Ok(mappedResult);
                }
                else
                    return MatchError(result.Errors, ModelState);
            }
            else
                return Unauthorized();
        }
        [HttpGet("PaymentLink/{orderId}")]
        public async Task<ActionResult> GetPaymentLink([FromRoute] GetOrderPaymentLink getOrderPaymentLink)
        {
            var result = await _sender.Send(getOrderPaymentLink);
            if(result.IsFailed)
                return MatchError(result.Errors, ModelState);
            return Ok(result.Value);
        }
    }
}
