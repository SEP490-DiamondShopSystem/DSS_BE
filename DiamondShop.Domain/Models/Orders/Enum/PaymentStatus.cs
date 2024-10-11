using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Models.Orders.Enum
{
    public enum  PaymentStatus
    {
        PaidAll = 1, Deposited = 2, Refunding = 3, Refunded = 4
    }
}
