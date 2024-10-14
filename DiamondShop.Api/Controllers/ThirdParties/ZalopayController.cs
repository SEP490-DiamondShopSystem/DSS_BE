using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Infrastructure.Options;
using DiamondShop.Infrastructure.Services.Payments.Zalopays;
using DiamondShop.Infrastructure.Services.Payments.Zalopays.Models;
using DiamondShop.Infrastructure.Services.Payments.Zalopays.Models.Responses;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace DiamondShop.Api.Controllers.ThirdParties
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZalopayController : ApiControllerBase
    {
        private readonly IOptions<UrlOptions> _urlOptions;
        private readonly ZalopayClient _zalopayClient;
        private readonly IPaymentService _paymentService;

        public ZalopayController(IOptions<UrlOptions> urlOptions, ZalopayClient zalopayClient, IPaymentService paymentService)
        {
            _urlOptions = urlOptions;
            _zalopayClient = zalopayClient;
            _paymentService = paymentService;
        }

        [HttpPost]
        [Produces<ZalopayCreateOrderResponse>]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> CreateOrder([FromBody]ZalopayCreateOrderBody body )
        {
            var myUrl = _urlOptions.Value.HttpsUrl;
            var result = await _zalopayClient.CreateOrder(body,"http://localhost:7160/swagger/index.html", $"{myUrl}/api/Zalopay/Callback");
            return Ok(result);
        }
        [HttpGet("Banks")]
        [Produces(typeof(Dictionary<string, List<ZalopayBankResponse>>))]
        public async Task<ActionResult> GetBanks()
        {
            return Ok(await ZalopayClient.GetBanks());
        }
        [HttpGet("Transaction/{app_transaction_id}")]
        [Produces<ZalopayTransactionResponse>]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> GetTransactionDetail(string app_transaction_id )
        {
            var result = await _zalopayClient.GetTransactionDetail(app_transaction_id);
            return Ok(result);
        }
        [HttpGet("Transaction/refund/{merchant_refund_id}/{time_stamp}")]
        [Produces<ZalopayRefundResponse>]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> GetRefundTransactionDetail(string merchant_refund_id, string time_stamp)
        {
            var result = await _zalopayClient.GetRefundTransactionDetail(merchant_refund_id, time_stamp);
            return Ok(result);
        }
        [HttpPost("Transaction/refund")]
        [Produces<ZalopayRefundResponse>]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> RefundTransaction([FromForm] string zalo_trans_id, [FromForm] long amount, [FromForm] long refund_ree = 0)
        {
            var result = await _zalopayClient.RefundTransaction(zalo_trans_id,amount,refund_ree );
            return Ok(result);
        }
        [HttpPost("Callback")]
        public async Task<ActionResult> Callback()
        {
            var result = await _paymentService.Callback();
            return Ok(result);  
        }
        [HttpGet("Return")]
        public async Task<ActionResult> Return()
        {
            //this will redirect client to the frontend, but not important for now, so just return ok
            return Ok();
        }
    }
}
