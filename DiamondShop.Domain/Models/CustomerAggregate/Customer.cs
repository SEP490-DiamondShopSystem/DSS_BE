using BeatvisionRemake.Domain.Common;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.CustomerAggregate.ValueObjects;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Models.CustomerAggregate
{
    public class Customer : Entity<CustomerId>, IAggregateRoot , IAccountBase
    {
        private Customer()
        {

        }
        private Customer(CustomerId customerId ,FullName fullName,string email) : base(customerId)
        {
            FullName = fullName;
            Email = email;
        }
        public string IdentityId { get; private set; }
        public List<AccountRole> Roles { get; private set; } = new();
        public FullName FullName { get; private set; }
        public string Email { get; private set; }
        public static Customer Create( FullName fullName, string email)
        {
            var user = new Customer(CustomerId.Create(), fullName,email);
            return user;
        }
        public void SetIdentity(string identityID)
        {
            IdentityId = identityID; 
        }
        public void AddRole(AccountRole role) 
        {
            ArgumentNullException.ThrowIfNull(role);
            if (role.RoleType != AccountRoleAggregate.AccountRoleType.Customer)
                throw new ArgumentException("invalid role");
            Roles.Add(role);
        }
        public void RemoveRole(AccountRole role)
        {
            ArgumentNullException.ThrowIfNull(role);
            if (role.RoleType != AccountRoleAggregate.AccountRoleType.Customer)
                throw new ArgumentException("invalid role");
            Roles.Remove(role);
        }
    }
}
