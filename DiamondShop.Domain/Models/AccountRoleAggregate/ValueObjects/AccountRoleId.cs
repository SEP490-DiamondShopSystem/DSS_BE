﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Models.AccountRoleAggregate.ValueObjects
{
    public record AccountRoleId(string Value)
    {
        //public string Value  { get; private set; }
        public static AccountRoleId Parse(string id)
        {
            return new AccountRoleId(id) { Value = id } ;
        }
        public static AccountRoleId Create(string id)
        {
            return new AccountRoleId(id);
            //{
            //    Value = id.ToString(),
            //};
        }
    }
}
