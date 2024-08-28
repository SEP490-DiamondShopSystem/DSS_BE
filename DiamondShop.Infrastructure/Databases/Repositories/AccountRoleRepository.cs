using DiamondShop.Domain.Models.CustomerAggregate;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Databases.Repositories
{
    internal class AccountRoleRepository : BaseRepository<AccountRole>, IAccountRoleRepository
    {
        public AccountRoleRepository(DiamondShopDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<DiamondShopCustomerRole>> GetCustomerRoles()
        {
            var accountRole =  _set.Where(r => r.RoleType == Domain.Models.AccountRoleAggregate.AccountRoleType.Customer).AsNoTracking().ToList();
            return accountRole.Select(r => new DiamondShopCustomerRole(r)).ToList();
        }

        public async Task<List<DiamondShopStoreRoles>> GetStaffRoles()
        {
            var accountRole = _set.Where(r => r.RoleType == Domain.Models.AccountRoleAggregate.AccountRoleType.Staff).AsNoTracking().ToList();
            return accountRole.Select(r => new DiamondShopStoreRoles(r)).ToList();
        }

    }
}
