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
    //public class DiamondShopStoreRoles : AccountRole
    //{
    //    public const string StaffId = "11";
    //    public const string ManagerId = "22";
    //    public const string AdminId = "33";


    //    public static DiamondShopStoreRoles Staff = new DiamondShopStoreRoles(new AccountRole(AccountRoleId.Create(StaffId),AccountRoleType.Staff,"staff","staff"));
    //    public static DiamondShopStoreRoles Manager = new DiamondShopStoreRoles(new AccountRole(AccountRoleId.Create(ManagerId), AccountRoleType.Staff, "manager", "manager"));
    //    public static DiamondShopStoreRoles Admin = new DiamondShopStoreRoles(new AccountRole(AccountRoleId.Create(AdminId), AccountRoleType.Staff, "admin", "admin"));

    //    public List<Staff> Staffs { get; private set; } = new();

    //    public DiamondShopStoreRoles(AccountRole value) : base(value.Id,value.RoleType,value.RoleName,value.RoleDescription)
    //    {
    //    }
    //    private DiamondShopStoreRoles() : base(null, AccountRoleType.Staff, null, null) { }


    //}
    //public class DiamondShopCustomerRole : AccountRole
    //{
    //    public const string CustomerId = "1";
    //    public const string CustomerBronzeId= "2";
    //    public const string CustomerSilverId = "3";
    //    public const string CustomerGoldId = "4";

    //    public static DiamondShopCustomerRole Customer = new  DiamondShopCustomerRole(new AccountRole(AccountRoleId.Create(CustomerId), AccountRoleType.Customer, "customer", "customer"));
    //    public static DiamondShopCustomerRole CustomerBronze = new DiamondShopCustomerRole(new AccountRole(AccountRoleId.Create(CustomerBronzeId), AccountRoleType.Customer, "customer_bronze", "customer_bronze"));
    //    public static DiamondShopCustomerRole CustomerSilver = new DiamondShopCustomerRole(new AccountRole(AccountRoleId.Create(CustomerSilverId), AccountRoleType.Customer, "customer_silver", "customer_silver"));
    //    public static DiamondShopCustomerRole CustomerGold = new DiamondShopCustomerRole(new AccountRole(AccountRoleId.Create(CustomerGoldId), AccountRoleType.Customer, "customer_gold", "customer_gold"));

    //    public List<Customer> Customers { get; private set; } = new();

    //    public DiamondShopCustomerRole(AccountRole value) : base(value.Id, value.RoleType, value.RoleName, value.RoleDescription)
    //    {

    //    }
    //    // this constructor is for ef mapping purpose, have no use
    //    private DiamondShopCustomerRole() : base(null,AccountRoleType.Customer,null,null){ }
    //}
    public class Nothing { }

}
