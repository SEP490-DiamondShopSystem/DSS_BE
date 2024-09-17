using DiamondShop.Domain.Common.ValueObjects;
using FluentResults;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Services.Currencies
{

    public class CurrencyExchangeService
    {
        private static string APP_ID = "f429ba0fbb014bc4beccefec8c46ac98";
        public async Task<Result<Money>> ConvertCurrency(Money source)
        {

            var options = new RestClientOptions($"https://openexchangerates.org/api/convert/null/Required/Required?app_id={APP_ID}&prettyprint=false");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("Accept", "application/json");
            var response = await client.GetAsync(request);

            Console.WriteLine("{0}", response.Content);
            return Result.Ok(Money.CreateVnd(22.2m));
        }
    }
}
