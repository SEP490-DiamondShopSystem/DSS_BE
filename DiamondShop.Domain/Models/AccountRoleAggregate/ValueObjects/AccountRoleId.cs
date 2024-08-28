using DiamondShop.Domain.Models.CustomerAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Models.AccountRoleAggregate.ValueObjects
{
    public record AccountRoleId
    {
        public string Value  { get; private set; }
        public static AccountRoleId Parse(string id)
        {
            return new AccountRoleId() { Value = id } ;
        }
        public static AccountRoleId Create(int id)
        {
            return new AccountRoleId() 
            {
                Value = id.ToString(),
            };
        }
    }
}
