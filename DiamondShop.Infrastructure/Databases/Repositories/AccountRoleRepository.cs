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

        public async Task<List<AccountRole>> GetRoles()
        {
            return await _dbContext.AccountRoles.ToListAsync();
        }

    }
}
