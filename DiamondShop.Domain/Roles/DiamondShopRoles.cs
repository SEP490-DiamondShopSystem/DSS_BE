using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.AccountRoleAggregate;
using DiamondShop.Domain.Models.AccountRoleAggregate.ValueObjects;
using DiamondShop.Domain.Models.CustomerAggregate;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Models.StaffAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Roles
{
    //public class DiamonShopRole
    //{
    //    public AccountRole Value { get; private set; }
    //    public DiamonShopRole(AccountRole value)
    //    {
    //        Value = value;
    //    }
    //    public static implicit operator AccountRole(DiamonShopRole diamondShopRole)
    //    {
    //        return diamondShopRole.Value;
    //    }

    //}
    public class DiamondShopStoreRoles : AccountRole
    {
        public static DiamondShopStoreRoles Staff = new DiamondShopStoreRoles(new AccountRole(AccountRoleId.Create(11),AccountRoleType.Staff,"staff","staff"));
        public static DiamondShopStoreRoles Manager = new DiamondShopStoreRoles(new AccountRole(AccountRoleId.Create(22), AccountRoleType.Staff, "manager", "manager"));
        public static DiamondShopStoreRoles Admin = new DiamondShopStoreRoles(new AccountRole(AccountRoleId.Create(33), AccountRoleType.Staff, "admin", "admin"));
        
        public List<Staff> Staffs { get; private set; } = new();
        
        public DiamondShopStoreRoles(AccountRole value) : base(value.Id,value.RoleType,value.RoleName,value.RoleDescription)
        {
        }
        private DiamondShopStoreRoles() : base(null, AccountRoleType.Staff, null, null) { }


    }
    public class DiamondShopCustomerRole : AccountRole
    {

        public static DiamondShopCustomerRole Customer = new  DiamondShopCustomerRole(new AccountRole(AccountRoleId.Create(1), AccountRoleType.Customer, "customer", "customer"));
        public static DiamondShopCustomerRole CustomerBronze = new DiamondShopCustomerRole(new AccountRole(AccountRoleId.Create(2), AccountRoleType.Customer, "customer_bronze", "customer_bronze"));
        public static DiamondShopCustomerRole CustomerSilver = new DiamondShopCustomerRole(new AccountRole(AccountRoleId.Create(3), AccountRoleType.Customer, "customer_silver", "customer_silver"));
        public static DiamondShopCustomerRole CustomerGold = new DiamondShopCustomerRole(new AccountRole(AccountRoleId.Create(4), AccountRoleType.Customer, "customer_gold", "customer_gold"));

        public List<Customer> Customers { get; private set; } = new();

        public DiamondShopCustomerRole(AccountRole value) : base(value.Id, value.RoleType, value.RoleName, value.RoleDescription)
        {

        }
        // this constructor is for ef mapping purpose, have no use
        private DiamondShopCustomerRole() : base(null,AccountRoleType.Customer,null,null){ }
    }

}
