using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Services.Currencies.Models
{
    public class OpenExchangeResponses
    {
        public string Disclaimer { get; set; }
        public string License { get; set; }
        public OpenExchangeConvertRequest Request { get; set; }
        public OpenExchangeMeta Meta { get; set; }
        public decimal Response { get; set; }
    }

    public class OpenExchangeMeta
    {
        public long Timestamp { get; set; }
        public decimal Rate { get; set; }
    }
    public class OpenExchangeConvertRequest
    {
        public string Query { get; set; }
        public decimal Amount { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }
}
