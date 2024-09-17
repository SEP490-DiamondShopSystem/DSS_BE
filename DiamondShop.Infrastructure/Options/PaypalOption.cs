using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Options
{
    public class PaypalOption
    {
        public static string Section = "PaypalDevelopment";
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Url { get; set; }
    }
}
