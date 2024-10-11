using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.BusinessRules
{
    public static class MoneyVndRoundUpRules
    {
        //globally use this function 
        public static decimal RoundAmountFromDecimal(decimal amount)
        {
            return Math.Round(Decimal.Divide(amount, 1000), 1) * 1000;
        }
        
    }
}
