using BeatvisionRemake.Domain.Common;
using DiamondShop.Domain.Models.AccountRoleAggregate;
using DiamondShop.Domain.Models.AccountRoleAggregate.ValueObjects;
using DiamondShop.Domain.Models.CustomerAggregate;
using DiamondShop.Domain.Models.StaffAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Models.RoleAggregate
{
    public class AccountRole : Entity<AccountRoleId>
    {
        public string RoleName { get; private set; }
        public AccountRoleType RoleType { get; private set; }
        public string RoleDescription { get; private set; }

        public AccountRole(AccountRoleId id,AccountRoleType type, string name, string description): base(id) 
        {
            RoleName = name;
            RoleType = type;
            RoleDescription = description;
        }
        private AccountRole()
        {

        }
    }
}
