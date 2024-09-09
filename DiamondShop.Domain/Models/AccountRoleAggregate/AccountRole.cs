using BeatvisionRemake.Domain.Common;
using DiamondShop.Domain.Models.AccountRoleAggregate;
using DiamondShop.Domain.Models.AccountRoleAggregate.ValueObjects;
using DiamondShop.Domain.Models.CustomerAggregate;
using DiamondShop.Domain.Models.StaffAggregate;
using DiamondShop.Domain.Roles;
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
        public List<Staff> Staffs { get; private set; } = new();
        public List<Customer> Customers { get; private set; } = new();

        public AccountRole(AccountRoleId id,AccountRoleType type, string name, string description): base(id) 
        {
            RoleName = name;
            RoleType = type;
            RoleDescription = description;
        }
        private AccountRole()
        {

        }

        public const string StaffId = "11";
        public const string ManagerId = "22";
        public const string AdminId = "33";
        public static AccountRole Staff = new AccountRole(AccountRoleId.Create(StaffId), AccountRoleType.Staff, "staff", "staff");
        public static AccountRole Manager = new AccountRole(AccountRoleId.Create(ManagerId), AccountRoleType.Staff, "manager", "manager");
        public static AccountRole Admin = new AccountRole(AccountRoleId.Create(AdminId), AccountRoleType.Staff, "admin", "admin");
        public const string CustomerId = "1";
        public const string CustomerBronzeId = "2";
        public const string CustomerSilverId = "3";
        public const string CustomerGoldId = "4";
        public static AccountRole Customer = new AccountRole(AccountRoleId.Create(CustomerId), AccountRoleType.Customer, "customer", "customer");
        public static AccountRole CustomerBronze = new AccountRole(AccountRoleId.Create(CustomerBronzeId), AccountRoleType.Customer, "customer_bronze", "customer_bronze");
        public static AccountRole CustomerSilver = new AccountRole(AccountRoleId.Create(CustomerSilverId), AccountRoleType.Customer, "customer_silver", "customer_silver");
        public static AccountRole CustomerGold = new AccountRole(AccountRoleId.Create(CustomerGoldId), AccountRoleType.Customer, "customer_gold", "customer_gold");
    }
}
