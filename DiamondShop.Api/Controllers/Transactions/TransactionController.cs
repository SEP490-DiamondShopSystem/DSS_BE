using DiamondShop.Application.Dtos.Responses.Transactions;
using DiamondShop.Application.Usecases.Transactions.Commands.AddManualPayments;
using DiamondShop.Application.Usecases.Transactions.Commands.AddManualRefunds;
using DiamondShop.Application.Usecases.Transactions.Commands.CreatePaymentLink;
using DiamondShop.Application.Usecases.Transactions.Queries.GetAll;
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

        public TransactionController(IMapper mapper, ISender sender)
        {
            _mapper = mapper;
            _sender = sender;
        }

        [HttpPost]
        public async Task<ActionResult> CreatePaymentLink(CreatePaymentLinkCommand createPaymentLink, CancellationToken cancellationToken = default)
        {
            var result = await _sender.Send(createPaymentLink);
            if (result.IsSuccess)
                return Ok(result.Value);
            return MatchError(result.Errors, ModelState);
        }
        [HttpPost("Manual")]
        public async Task<ActionResult> CreateManualPayment(AddTransactionManuallyCommand addTransactionManuallyCommand)
        {
            var result = await _sender.Send(addTransactionManuallyCommand);
            if (result.IsSuccess)
                return Ok(result.Value);
            return MatchError(result.Errors, ModelState);
        }
        [HttpPost("Refund/Manual")]
        public async Task<ActionResult> CreateManualRefund(AddManualRefundCommand addManualRefundCommand)
        {
            var result = await _sender.Send(addManualRefundCommand);
            if (result.IsSuccess)
                return Ok(result.Value);
            return MatchError(result.Errors, ModelState);
        }
        [HttpGet("All")]
        public async Task<ActionResult> GetAllTransaction(GetAllTransactionQuery getAll, CancellationToken cancellationToken = default)
        {
            var result = await _sender.Send(getAll, cancellationToken);
            var mappedResult = _mapper.Map<List<TransactionDto>>(result);
            return Ok(mappedResult);
        }
    }
}
