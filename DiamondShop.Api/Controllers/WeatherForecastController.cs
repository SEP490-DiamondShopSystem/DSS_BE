using DiamondShop.Api.Controllers;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Infrastructure.Options;
using DiamondShop.Infrastructure.Services;
using DiamondShop.Infrastructure.Services.Payments.Paypals;
using DiamondShop.Infrastructure.Services.Payments.Paypals.Models;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DiamondShopSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ApiControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBlobFileServices _blobFileServices;
        private readonly IOptions<PaypalOption> _paypal;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IDateTimeProvider dateTimeProvider, IBlobFileServices blobFileServices, IOptions<PaypalOption> paypal)
        {
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _blobFileServices = blobFileServices;
            _paypal = paypal;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
        [HttpGet("GetTimeZone")]
        public ActionResult GetTimeZone()
        {
            return Ok(TimeZoneInfo.Local);
        }
        [HttpPost("upload-file")]
        public async Task<ActionResult> Upload(IFormFile files)
        {
            using Stream openstream = files.OpenReadStream();
            var result = await _blobFileServices.UploadFileAsync("fakefileupload/fakefile", openstream, files.ContentType);
            return Ok();
        }
        [HttpDelete("delete-file")]
        public async Task<ActionResult> download()
        {
            var result = await _blobFileServices.DeleteFileAsync("fakefileupload/fakefile");
            return Ok();
        }
        [HttpDelete("test-excel")]
        public async Task<ActionResult> excel()
        {
            //var test = new ExcelManagerService();
            //test.Testing();
            return Ok();
        }
        [Route("/callback-ngrok")]
        [HttpGet]
        public async Task<ActionResult> CallbackNgrokTest()
        {
            Console.WriteLine("Hello world, calling from vnpay");
            return Ok();
        }
        [Route("/exception")]
        [HttpGet]
        public async Task<ActionResult> exception()
        {
            throw new Exception();
        }
        [Route("/badrequest")]
        [HttpGet]
        public async Task<ActionResult> basdreqiest()
        {
            return MatchError(new List<IError>(), ModelState); 
        }
        [Route("/paypalresponse")]
        [HttpGet]
        public async Task<ActionResult> paypal()
        {
            var paypalClient = new PaypalClient(_paypal);
            return Ok( await paypalClient.GetAccessToken());
        }
    }
}
