using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DiamondShop.Api.Controllers.Transactions
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ApiControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ISender _sender;

    }
}
