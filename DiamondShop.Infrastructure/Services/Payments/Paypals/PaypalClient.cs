using DiamondShop.Infrastructure.Options;
using DiamondShop.Infrastructure.Services.Payments.Paypals.Models;
using FluentResults;
using Microsoft.Extensions.Options;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
namespace DiamondShop.Infrastructure.Services.Payments.Paypals
{
    public class PaypalClient
    {
        private readonly IOptions<PaypalOption> _options;

        public PaypalClient(IOptions<PaypalOption> options)
        {
            _options = options;
        }
        
        public async Task CreateOrder()
        {
            PaypalOption paypalOption = _options.Value;
            string accessToken = await GetAccessToken();
            string creatOrderUri = "/v2/checkout/orders";
            using(var httpClient = new HttpClient()) 
            {
                httpClient.BaseAddress = new Uri(creatOrderUri + creatOrderUri);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); ;
            }
        }
        public async Task CaptureOrder()
        {

        }

        public async Task<string> GetAccessToken()
        {
            PaypalOption paypalOpt = _options.Value;
            string oauthPath = "/v1/oauth2/token";
            //header value
            byte[] headerBytes = Encoding.UTF8.GetBytes($"{paypalOpt.ClientId}:{paypalOpt.ClientSecret}");
            string base64ClientIdClientSecret = Convert.ToBase64String(headerBytes);
            //body value
            var content = new StringContent("grant_type=client_credentials",null, "application/x-www-form-urlencoded");
            using (var client = new HttpClient())
            {
                //client.BaseAddress = new Uri(paypalOpt.Url + oauthPath);
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64ClientIdClientSecret);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
                var requestMessage = new HttpRequestMessage(HttpMethod.Post,paypalOpt.Url + oauthPath);
                requestMessage.Content = content;
                var httpResult = await client.SendAsync(requestMessage);
                if (httpResult.IsSuccessStatusCode is false)
                {
                    throw new Exception("why this is not success");
                }
                PaypalTokenResponse? paypalTokenResponse = await httpResult.Content.ReadFromJsonAsync<PaypalTokenResponse>();
                ArgumentNullException.ThrowIfNull(paypalTokenResponse);
                return paypalTokenResponse.AccessToken;
            }
        }
    }
}
