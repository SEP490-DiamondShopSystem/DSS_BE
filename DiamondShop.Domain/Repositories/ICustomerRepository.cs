﻿using DiamondShop.Domain.Common;
using DiamondShop.Domain.Models.CustomerAggregate;
using DiamondShop.Domain.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Repositories
{
    public interface ICustomerRepository : IBaseRepository<Customer>
    {
        Task AddRole(IUserIdentity identity, DiamondShopCustomerRole diamondShopCustomerRole, CancellationToken cancellationToken  = default);
        Task RemoveRole(IUserIdentity identity, DiamondShopCustomerRole diamondShopCustomerRole, CancellationToken cancellationToken = default);

    }
}
