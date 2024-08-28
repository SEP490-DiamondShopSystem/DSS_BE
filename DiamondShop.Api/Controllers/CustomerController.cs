using DiamondShop.Application.Usecases.Customers.Queries.GetCustomerDetail;
using DiamondShop.Application.Usecases.Customers.Queries.GetCustomerPage;
using DiamondShop.Domain.Models.CustomerAggregate.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DiamondShop.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ApiControllerBase
    {
        private readonly ISender _sender;

        public CustomerController(ISender sender)
        {
            _sender = sender;
        }
        [HttpGet]
        public async Task<ActionResult> GetPaging([FromQuery]GetCustomerPageQuery getCustomerPageQuery)
        {
            return Ok((await _sender.Send(getCustomerPageQuery)).Value);
        }
        [HttpGet("{customerId}")]
        public async Task<ActionResult> GetPaging([FromRoute] string customerId)
        {
            var id = CustomerId.Parse(customerId);
            return Ok((await _sender.Send(new GetCustomerDetailQuery(id))).Value);
        }

    }
}
