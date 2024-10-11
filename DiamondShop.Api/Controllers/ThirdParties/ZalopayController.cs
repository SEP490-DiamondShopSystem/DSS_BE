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

        public ZalopayController(IOptions<UrlOptions> urlOptions, ZalopayClient zalopayClient)
        {
            _urlOptions = urlOptions;
            _zalopayClient = zalopayClient;
        }

        [HttpPost]
        [Produces<ZalopayCreateOrderResponse>]
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
        public async Task<ActionResult> GetTransactionDetail(string app_transaction_id )
        {
            var result = await _zalopayClient.GetTransactionDetail(app_transaction_id);
            return Ok(result);
        }
        [HttpGet("Transaction/refund/{merchant_refund_id}/{time_stamp}")]
        [Produces<ZalopayRefundResponse>]
        public async Task<ActionResult> GetRefundTransactionDetail(string merchant_refund_id, string time_stamp)
        {
            var result = await _zalopayClient.GetRefundTransactionDetail(merchant_refund_id, time_stamp);
            return Ok(result);
        }
        [HttpPost("Transaction/refund")]
        [Produces<ZalopayRefundResponse>]
        public async Task<ActionResult> RefundTransaction([FromForm] string zalo_trans_id, [FromForm] long amount, [FromForm] long refund_ree = 0)
        {
            var result = await _zalopayClient.RefundTransaction(zalo_trans_id,amount,refund_ree );
            return Ok(result);
        }
        [HttpPost("Callback")]
        public async Task<ActionResult> Callback()
        {
            var result = await _zalopayClient.Callback();
            return Ok(result);  
        }
        [HttpGet("Return")]
        public async Task<ActionResult> Return()
        {
            return Ok();
        }
        [HttpPost("TestCallback")]
        public async Task TestCallback()
        {
            var result = new Dictionary<string, object>();
            result["return_code"] = 1;
            result["return_message"] = "success";
            var jsonResult = JsonConvert.SerializeObject(result);
            HttpContext.Response.ContentType = "application/json";
            await HttpContext.Response.WriteAsync(jsonResult);
            //return Ok();
        }

    }
}
