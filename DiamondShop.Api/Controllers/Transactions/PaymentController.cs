using DiamondShop.Application.Dtos.Responses.Transactions;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Application.Usecases.Transactions.Commands.AddManualPayments;
using DiamondShop.Application.Usecases.Transactions.Commands.AddManualRefunds;
using DiamondShop.Application.Usecases.Transactions.Commands.CreatePaymentLink;
using DiamondShop.Application.Usecases.Transactions.Queries.GetAll;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.XlsIO.Parser.Biff_Records.PivotTable;
using System.Text;

namespace DiamondShop.Api.Controllers.Transactions
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PaymentController : ApiControllerBase
    {
        private readonly ISender _sender;
        private readonly IMapper _mapper;

        public PaymentController(ISender sender, IMapper mapper)
        {
            _sender = sender;
            _mapper = mapper;
        }

       
    }
}
